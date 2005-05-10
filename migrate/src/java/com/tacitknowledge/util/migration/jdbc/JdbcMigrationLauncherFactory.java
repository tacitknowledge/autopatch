/* Copyright 2005 Tacit Knowledge LLC
 *
 * Licensed under the Tacit Knowledge Open License, Version 1.0 (the "License");
 * you may not use this file except in compliance with the License. You may
 * obtain a copy of the License at http://www.tacitknowledge.com/licenses-1.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.tacitknowledge.util.migration.jdbc;

import java.io.IOException;
import java.io.InputStream;
import java.util.Properties;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.jdbc.util.NonPooledDataSource;

/**
 * Creates and configures a new <code>JdbcMigrationContext</code> based on the values
 * in the <em>migration.properties</em> file for the given system.  This is a convenience
 * class for systems that need to initialize the autopatch framework but do not to or can not
 * configure the framework themselves.
 * <p>
 * This factory expects a file named <code>migration.properties</code> to be in the root of the
 * class path.  This file must contain these properties (where <i>systemName</i> is the name of
 * the system being patched):
 * <table>
 * <tr><th>Key</th><th>description</th></tr>
 * <tr><td><i>systemName</i>.patch.path</td><td></td></tr>
 * <tr><td><i>systemName</i>.jdbc.database.type</td>
 *     <td>The database type; also accepts <i>systemName</i>.jdbc.dialect</td></tr>
 * <tr><td><i>systemName</i>.jdbc.driver</td><td>The JDBC driver to use</td></tr>
 * <tr><td><i>systemName</i>.jdbc.url</td><td>The JDBC URL to the database</td></tr>
 * <tr><td><i>systemName</i>.jdbc.username</td><td>The database user name</td></tr>
 * <tr><td><i>systemName</i>.jdbc.password</td><td>The database password</td></tr>
 * </table>
 *
 * @author Scott Askew (scott@tacitknowledge.com)
 */
public class JdbcMigrationLauncherFactory
{
    /**
     * Creates and configures a new <code>JdbcMigrationContext</code> based on the
     * values in the <em>migration.properties</em> file for the given system.
     *
     * @param  systemName the system to patch
     * @return a fully configured <code>JdbcMigrationContext</code>.
     * @throws MigrationException if an unexpected error occurs
     */
    public JdbcMigrationLauncher createMigrationLauncher(String systemName)
        throws MigrationException
    {
        JdbcMigrationLauncher launcher = new JdbcMigrationLauncher();
        configureFromMigrationProperties(launcher, systemName);
        return launcher;
    }

    /**
     * Loads the configuration from the migration config properties file.
     *
     * @param  launcher the launcher to configure
     * @param systemName the name of the system
     * @throws MigrationException if an unexpected error occurs
     */
    private void configureFromMigrationProperties(JdbcMigrationLauncher launcher, String systemName)
        throws MigrationException
    {
        DataSourceMigrationContext context = new DataSourceMigrationContext();
        context.setSystemName(systemName);
        ClassLoader cl = Thread.currentThread().getContextClassLoader();
        InputStream is = cl.getResourceAsStream(MigrationContext.MIGRATION_CONFIG_FILE);
        if (is != null)
        {
            try
            {
                Properties props = new Properties();
                props.load(is);

                launcher.setPatchPath(getRequiredParam(props, systemName + ".patch.path"));

                // Set up the JDBC migration context; accepts on of two property names
                String databaseType = getRequiredParam(props,
                    systemName + ".jdbc.database.type", systemName + ".jdbc.dialect");
                context.setDatabaseType(new DatabaseType(databaseType));

                // Set up the data source
                NonPooledDataSource dataSource = new NonPooledDataSource();
                dataSource.setDriverClass(getRequiredParam(props, systemName + ".jdbc.driver"));
                dataSource.setDatabaseUrl(getRequiredParam(props, systemName + ".jdbc.url"));
                dataSource.setUsername(getRequiredParam(props, systemName + ".jdbc.username"));
                dataSource.setPassword(getRequiredParam(props, systemName + ".jdbc.password"));
                context.setDataSource(dataSource);

                // done reading in config, set launcher's context
                launcher.setJdbcMigrationContext(context);
            }
            catch (IOException e)
            {
                throw new MigrationException("Error reading in migration properties file", e);
            }
            finally
            {
                try
                {
                    is.close();
                }
                catch (IOException ioe)
                {
                    throw new MigrationException("Error closing migration properties file", ioe);
                }
            }
        }
        else
        {
            throw new MigrationException("Unable to find migration properties file '"
                    + MigrationContext.MIGRATION_CONFIG_FILE + "'");
        }
    }

    /**
     * Returns the value of the specified configuration parameter.
     *
     * @param  props the properties file containing the values
     * @param  param the parameter to return
     * @return the value of the specified configuration parameter
     * @throws IllegalArgumentException if the parameter does not exist
     */
    public static String getRequiredParam(Properties props, String param)
        throws IllegalArgumentException
    {
        String value = props.getProperty(param);
        if (value == null)
        {
            throw new IllegalArgumentException("'" + param + "' is a required "
                + "initialization parameter.  Aborting.");
        }
        return value;
    }

    /**
     * Returns the value of the specified configuration parameter.
     *
     * @param  props the properties file containing the values
     * @param  param the parameter to return
     * @param  alternate the alternate parameter to return
     * @return the value of the specified configuration parameter
     */
    public static String getRequiredParam(Properties props, String param, String alternate)
    {
        try
        {
            return getRequiredParam(props, param);
        }
        catch (IllegalArgumentException e1)
        {
            try
            {
                return getRequiredParam(props, alternate);
            }
            catch (IllegalArgumentException e2)
            {
                throw new IllegalArgumentException("Either '" + param + "' or '" + alternate
                    + "' must be specified as an initialization parameter.  Aborting.");
            }
        }
    }
}


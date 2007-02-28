/* Copyright 2007 Tacit Knowledge LLC
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

import javax.naming.Context;
import javax.naming.InitialContext;
import javax.naming.NamingException;
import javax.servlet.ServletContextEvent;
import javax.sql.DataSource;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.jdbc.util.ConfigurationUtil;
import com.tacitknowledge.util.migration.jdbc.util.NonPooledDataSource;

/**
 * Creates and configures new <code>JdbcMigrationContext</code> objects based on the values
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
 * <p>
 * Optional properties include:
 * <table>
 * <tr><td><i>systemName</i>.postpatch.path</td><td></td></tr>
 * <tr><td><i>systemName</i>.readonly</td><td>boolean true to skip patch application</td></tr>
 * <tr><td><i>systemName</i>.jdbc.systems</td><td>Set of names for multiple JDBC connections that
 *                                                    should all have patches applied. Names will be
 *                                                    looked up in the same properties file as
 *                                                    <i>systemName.jdbcname</i>.database.type, where
 *                                                    all of the jdbc entries above should be present</td></tr>
 * </table>
 *
 * @author Scott Askew (scott@tacitknowledge.com)
 */
public class JdbcMigrationLauncherFactory
{
    /** Class logger */
    private static Log log = LogFactory.getLog(JdbcMigrationLauncherFactory.class);
    
    /**
     * Creates and configures a new <code>JdbcMigrationLauncher</code> based on the
     * values in the <em>migration.properties</em> file for the given system.
     *
     * @param  systemName the system to patch
     * @return a fully configured <code>JdbcMigrationLauncher</code>.
     * @throws MigrationException if an unexpected error occurs
     */
    public JdbcMigrationLauncher createMigrationLauncher(String systemName)
        throws MigrationException
    {
        log.info("Creating JdbcMigrationLauncher for system " + systemName);
        JdbcMigrationLauncher launcher = getJdbcMigrationLauncher();
        configureFromMigrationProperties(launcher, systemName);
        return launcher;
    }
    
    /**
     * Creates and configures a new <code>JdbcMigrationLauncher</code> based on the
     * values in the servlet context and JNDI for a web-application.
     *
     * @param  sce the name of the context event to use in getting properties
     * @return a fully configured <code>JdbcMigrationLauncher</code>.
     * @throws MigrationException if an unexpected error occurs
     */
    public JdbcMigrationLauncher createMigrationLauncher(ServletContextEvent sce)
        throws MigrationException
    {
        JdbcMigrationLauncher launcher = getJdbcMigrationLauncher();
        configureFromServletContext(launcher, sce);
        return launcher;
    }
    
    /**
     * Used to configure the migration launcher with properties from a servlet 
     * context.  You do not need migration.properties to use this method.
     * 
     * @param launcher the launcher to configure
     * @param sce the event to get the context and associated parameters from
     * @throws MigrationException if a problem with the look up in JNDI occurs
     */
    private void configureFromServletContext(JdbcMigrationLauncher launcher, 
            ServletContextEvent sce) throws MigrationException
    {
        DataSourceMigrationContext context = getDataSourceMigrationContext();
        String systemName = ConfigurationUtil.getRequiredParam("migration.systemname", sce, this);
        context.setSystemName(systemName);
        
        String readOnly = sce.getServletContext().getInitParameter("migration.readonly");
        launcher.setReadOnly(false);
        if ("true".equals(readOnly)) {
            launcher.setReadOnly(true);
        }
        
        String patchPath = ConfigurationUtil.getRequiredParam("migration.patchpath", sce, this);
        launcher.setPatchPath(patchPath);
        
        String postPatchPath = sce.getServletContext().getInitParameter("migration.postpatchpath");
        launcher.setPostPatchPath(postPatchPath);
        
        
        // FIXME test that we have actually gotten the database names for both cases
        String databases = sce.getServletContext().getInitParameter("migration.jdbc.systems");
        String[] databaseNames;
        if ((databases == null) || "".equals(databases))
        {
            databaseNames = new String[1];
            databaseNames[0] = "";
        }
        else
        {
            databaseNames = databases.split(",");
        }
        
        for (int i = 0; i < databaseNames.length; i++)
        {
            String databaseName = databaseNames[i];
            if (databaseName != "")
            {
                databaseName = databaseName + ".";
            }
            String databaseType = ConfigurationUtil.getRequiredParam("migration." + databaseName + "databasetype", sce, this);
            context.setDatabaseType(new DatabaseType(databaseType));
        
            String dataSource = ConfigurationUtil.getRequiredParam("migration." + databaseName + "datasource", sce, this);
            try
            {
                Context ctx = new InitialContext();
                if (ctx == null) 
                {
                    throw new IllegalArgumentException("A jndi context must be "
                                                       + "present to use this configuration.");
                }
                DataSource ds = (DataSource) ctx.lookup("java:comp/env/" + dataSource);
                context.setDataSource(ds);
                launcher.addContext(context);
            } 
            catch (NamingException e)
            {
                throw new MigrationException("Problem with JNDI look up of " + dataSource, e);
            }
        }
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
        ClassLoader cl = Thread.currentThread().getContextClassLoader();
        InputStream is = cl.getResourceAsStream(MigrationContext.MIGRATION_CONFIG_FILE);
        if (is != null)
        {
            try
            {
                Properties props = new Properties();
                props.load(is);

                configureFromMigrationProperties(launcher, systemName, props);
            }
            catch (IOException e)
            {
                throw new MigrationException("Error reading in autopatch properties file", e);
            }
            finally
            {
                try
                {
                    is.close();
                }
                catch (IOException ioe)
                {
                    throw new MigrationException("Error closing autopatch properties file", ioe);
                }
            }
        }
        else
        {
            throw new MigrationException("Unable to find autopatch properties file '"
                    + MigrationContext.MIGRATION_CONFIG_FILE + "'");
        }
    }

    /**
     * Configure the launcher from the provided properties, system name
     * 
     * @param launcher The launcher to configure
     * @param systemName The name of the system we're configuring
     * @param props The Properties object with our configuration information
     * @throws IllegalArgumentException if a required parameter is missing
     */
    private void configureFromMigrationProperties(JdbcMigrationLauncher launcher, String systemName, 
                                                  Properties props) 
        throws IllegalArgumentException
    {
        launcher.setPatchPath(ConfigurationUtil.getRequiredParam(props, systemName + ".patch.path"));
        launcher.setPostPatchPath(props.getProperty(systemName + ".postpatch.path"));
        launcher.setReadOnly(false);
        if ("true".equals(props.getProperty(systemName + ".readonly"))) {
            launcher.setReadOnly(true);
        }

        // TODO refactor the database name extraction from this and the servlet example
        String databases = props.getProperty(systemName + ".jdbc.systems");
        String[] databaseNames;
        if ((databases == null) || "".equals(databases))
        {
            databaseNames = new String[1];
            databaseNames[0] = "jdbc";
        }
        else
        {
            databaseNames = databases.split(",");
        }
        
        for (int i = 0; i < databaseNames.length; i++)
        {
            String databaseName = databaseNames[i];
            if (databaseName != "")
            {
                databaseName = "." + databaseName;
            }
            
            // Set up the data source
            NonPooledDataSource dataSource = new NonPooledDataSource();
            dataSource.setDriverClass(ConfigurationUtil.getRequiredParam(props, systemName + databaseName + ".driver"));
            dataSource.setDatabaseUrl(ConfigurationUtil.getRequiredParam(props, systemName + databaseName + ".url"));
            dataSource.setUsername(ConfigurationUtil.getRequiredParam(props, systemName + databaseName + ".username"));
            dataSource.setPassword(ConfigurationUtil.getRequiredParam(props, systemName + databaseName + ".password"));
        
            // Set up the JDBC migration context; accepts one of two property names
            DataSourceMigrationContext context = getDataSourceMigrationContext();
            String databaseType = ConfigurationUtil.getRequiredParam(props,
                                                                     systemName + databaseName + ".database.type", 
                                                                     systemName + databaseName + ".dialect");
            log.debug("setting type to " + databaseType);
            context.setDatabaseType(new DatabaseType(databaseType));
            
            // Finish setting up the context
            context.setSystemName(systemName);
            
            context.setDataSource(dataSource);

            // done reading in config, set launcher's context
            launcher.addContext(context);
        }
    }
    
    /**
     * Get a DataSourceMigrationContext
     * 
     * @return DataSourceMigrationContext for use with the launcher
     */
    public DataSourceMigrationContext getDataSourceMigrationContext()
    {
        return new DataSourceMigrationContext();
    }
    
    /**
     * Get a JdbcMigrationLauncher
     * 
     * @return JdbcMigrationLauncher
     */
    public JdbcMigrationLauncher getJdbcMigrationLauncher()
    {
        return new JdbcMigrationLauncher();
    }
}


/* Copyright 2006 Tacit Knowledge LLC
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
import java.util.Iterator;
import java.util.Properties;

import javax.naming.Context;
import javax.naming.InitialContext;
import javax.naming.NamingException;
import javax.servlet.ServletContext;
import javax.servlet.ServletContextEvent;
import javax.sql.DataSource;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

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
 * <tr><td><i>systemName</i>.postpatch.path</td><td></td></tr>
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
        String systemName = getRequiredParam("migration.systemname", sce);
        context.setSystemName(systemName);
        
        String databaseType = getRequiredParam("migration.databasetype", sce);
        context.setDatabaseType(new DatabaseType(databaseType));
        
        String patchPath = getRequiredParam("migration.patchpath", sce);
        launcher.setPatchPath(patchPath);
        
        String postPatchPath = sce.getServletContext().getInitParameter("migration.postpatchpath");
        launcher.setPostPatchPath(postPatchPath);
        
        String dataSource = getRequiredParam("migration.datasource", sce);
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
            launcher.setContext(context);
        } 
        catch (NamingException e)
        {
            throw new MigrationException("Problem with JNDI look up of " + dataSource, e);
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
     * Configure the launcher from the provided properties, system name
     * 
     * @param launcher The launcher to configure
     * @param systemName The name of the system we're configuring
     * @param props The Properties object with our configuration information
     * @throws IllegalArgumentException if a required parameter is missing
     * @throws MigrationException if there is problem setting the context into the launcher
     */
    private void configureFromMigrationProperties(JdbcMigrationLauncher launcher, String systemName, 
                                                  Properties props) 
        throws IllegalArgumentException, MigrationException
    {
        launcher.setPatchPath(getRequiredParam(props, systemName + ".patch.path"));
        launcher.setPostPatchPath(props.getProperty(systemName + ".postpatch.path"));

        // Set up the data source
        NonPooledDataSource dataSource = new NonPooledDataSource();
        dataSource.setDriverClass(getRequiredParam(props, systemName + ".jdbc.driver"));
        dataSource.setDatabaseUrl(getRequiredParam(props, systemName + ".jdbc.url"));
        dataSource.setUsername(getRequiredParam(props, systemName + ".jdbc.username"));
        dataSource.setPassword(getRequiredParam(props, systemName + ".jdbc.password"));
        
        // Set up the JDBC migration context; accepts one of two property names
        DataSourceMigrationContext context = getDataSourceMigrationContext();
        String databaseType = getRequiredParam(props,
            systemName + ".jdbc.database.type", systemName + ".jdbc.dialect");
        context.setDatabaseType(new DatabaseType(databaseType));

        // Finish setting up the context
        context.setSystemName(systemName);
        
        context.setDataSource(dataSource);

        // done reading in config, set launcher's context
        launcher.setContext(context);
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
            System.err.println("Parameter named: " + param + " was not found.");
            System.err.println("-----Parameters found-----");
            Iterator propNameIterator = props.keySet().iterator();
            while (propNameIterator.hasNext())
            {
                String name = (String) propNameIterator.next();
                String val = props.getProperty(name);
                System.err.println(name + " = " + val);
            }
            System.err.println("--------------------------");
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
    
    /**
     * Returns the value of the specified servlet context initialization parameter.
     * 
     * @param  param the parameter to return
     * @param  sce the <code>ServletContextEvent</code> being handled
     * @return the value of the specified servlet context initialization parameter
     * @throws IllegalArgumentException if the parameter does not exist
     */
    private String getRequiredParam(String param, ServletContextEvent sce)
        throws IllegalArgumentException
    {
        ServletContext context = sce.getServletContext();
        String value = context.getInitParameter(param);
        if (value == null)
        {
            throw new IllegalArgumentException("'" + param + "' is a required "
                + "servlet context initialization parameter for the \""
                + getClass().getName() + "\" class.  Aborting.");
        }
        return value;
    }
}


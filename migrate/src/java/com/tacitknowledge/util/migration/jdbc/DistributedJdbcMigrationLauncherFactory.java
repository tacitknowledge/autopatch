/* 
 * Copyright 2006 Tacit Knowledge LLC
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
import java.util.HashMap;
import java.util.Properties;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.DistributedMigrationProcess;
import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.jdbc.util.NonPooledDataSource;

/**
 * Creates and configures a new <code>DistributedJdbcMigrationContext</code> based on the values
 * in the <em>migration.properties</em> file for the given system.  This is a convenience
 * class for systems that need to initialize the autopatch framework but do not to or can not
 * configure the framework themselves.
 * <p>
 * This factory expects a file named <code>migration.properties</code> to be in the 
 * root of the  class path.  This file must contain these properties (where <i>systemName</i> 
 * is the name of the system being patched):
 * <table>
 * <tr><th>Key</th><th>description</th></tr>
 * <tr><td><i>systemName</i>.context</td><td>The context to use for orchestration</td></tr>
 * <tr><td><i>systemName</i>.controlled.systems</td><td>comma-delimited systems to manage</td></tr>
 * </table>
 * <p>
 * For each system in the controlled systems list, the properties file should contain
 * information as directed in the documenation for JdbcMigrationLauncher.
 * 
 * @see com.tacitknowledge.util.migration.jdbc.JdbcMigrationLauncher
 * @author Mike Hardy (mike@tacitknowledge.com)
 */
public class DistributedJdbcMigrationLauncherFactory extends JdbcMigrationLauncherFactory
{
    /** Class logger */
    private static Log log = LogFactory.getLog(DistributedJdbcMigrationLauncherFactory.class);

    /**
     * Creates and configures a new <code>JdbcMigrationLauncher</code> based on the
     * values in the <em>migration.properties</em> file for the given system.
     *
     * @param  systemName the system to patch
     * @return a fully configured <code>DistributedJdbcMigrationLauncher</code>.
     * @throws MigrationException if an unexpected error occurs
     */
    public JdbcMigrationLauncher createMigrationLauncher(String systemName)
        throws MigrationException
    {
        log.info("Creating DistributedJdbcMigrationLauncher for system " + systemName);
        DistributedJdbcMigrationLauncher launcher = getDistributedJdbcMigrationLauncher();
        configureFromMigrationProperties(launcher, systemName);
        return launcher;
    }
    
    /**
     * Get a new DistributedJdbcMigrationLauncher
     * 
     * @return DistributedJdbcMigrationLauncher
     */
    public DistributedJdbcMigrationLauncher getDistributedJdbcMigrationLauncher()
    {
        return new DistributedJdbcMigrationLauncher();
    }
    
    /**
     * Loads the configuration from the migration config properties file.
     *
     * @param  launcher the launcher to configure
     * @param systemName the name of the system
     * @throws MigrationException if an unexpected error occurs
     */
    private void configureFromMigrationProperties(DistributedJdbcMigrationLauncher launcher, String systemName)
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
    private void configureFromMigrationProperties(DistributedJdbcMigrationLauncher launcher, 
                                                  String systemName, Properties props) 
        throws IllegalArgumentException, MigrationException
    {
        // Get the name of the context to use for our patch information
        String patchStoreContextName = getRequiredParam(props, systemName + ".context");
        
        // Set up the data source
        NonPooledDataSource dataSource = new NonPooledDataSource();
        dataSource.setDriverClass(getRequiredParam(props, patchStoreContextName + ".jdbc.driver"));
        dataSource.setDatabaseUrl(getRequiredParam(props, patchStoreContextName + ".jdbc.url"));
        dataSource.setUsername(getRequiredParam(props, patchStoreContextName + ".jdbc.username"));
        dataSource.setPassword(getRequiredParam(props, patchStoreContextName + ".jdbc.password"));
        
        // Set up the JDBC migration context; accepts one of two property names
        DataSourceMigrationContext context = getDataSourceMigrationContext();
        String databaseType = getRequiredParam(props,
            patchStoreContextName + ".jdbc.database.type", patchStoreContextName + ".jdbc.dialect");
        context.setDatabaseType(new DatabaseType(databaseType));

        // Finish setting up the context
        context.setSystemName(systemName);
        context.setDataSource(dataSource);

        // done reading in config, set launcher's context
        launcher.setJdbcMigrationContext(context);
        
        // Get our controlled systems, and instantiate their launchers
        HashMap controlledSystems = new HashMap();
        String[] controlledSystemNames = 
            getRequiredParam(props, systemName + ".controlled.systems").split(",");
        for (int i = 0; i < controlledSystemNames.length; i++)
        {
            log.info("Creating controlled migration launcher for system " + controlledSystemNames[i]);
            JdbcMigrationLauncherFactory factory = JdbcMigrationLauncherFactoryLoader.createFactory();
            JdbcMigrationLauncher subLauncher = factory.createMigrationLauncher(controlledSystemNames[i]);
            controlledSystems.put(controlledSystemNames[i], subLauncher);
            
            // Make sure the controlled migration process gets migration events
            launcher.getMigrationProcess().addListener(subLauncher);
        }
        
        // communicate our new-found controlled systems to the migration process
        ((DistributedMigrationProcess)launcher.getMigrationProcess()).setControlledSystems(controlledSystems);
    }
}

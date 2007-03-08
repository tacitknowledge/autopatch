/* 
 * Copyright 2007 Tacit Knowledge LLC
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

import javax.servlet.ServletContextEvent;
import javax.servlet.ServletContextListener;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.discovery.ClassDiscoveryUtil;
import com.tacitknowledge.util.discovery.WebAppResourceListSource;
import com.tacitknowledge.util.migration.MigrationException;

/**
 * Used to configure the migration engine using JNDI and properties 
 * set in the servlet context for a web application.  This newer class 
 * removes the need to use a migration.properties file.  Instead, set the 
 * following properties (context-param) in the web.xml file:
 * 
 * <ul>
 * <li>migration.systemname - name of the system to update
 * <li>migration.databasetype - ex: mysql
 * <li>migration.patchpath - colon separated path to look for files
 * <li>migration.datasource - ex: jdbc/clickstream
 * </ul>
 * All properties listed above are required.  
 * 
 * @author Chris A. (chris@tacitknowledge.com)
 * @version $Id$
 */
public class WebAppJNDIMigrationLauncher implements ServletContextListener
{
    /**
     * Keeps track of the first run of the class within this web app deployment.
     * This should always be true, but you can never be too careful.
     */
    private static boolean firstRun = true;
    
    /**
     * Class logger
     */
    private static Log log = LogFactory.getLog(WebAppJNDIMigrationLauncher.class);
    
    /** {@inheritDoc} */
    public void contextInitialized(ServletContextEvent sce)
    {
        try
        {

            // WEB-INF/classes and WEB-INF/lib are not in the system classpath (as defined by
            // System.getProperty("java.class.path")); add it to the search path here
            if (firstRun)
            {
                ClassDiscoveryUtil.addResourceListSource(
                    new WebAppResourceListSource(sce.getServletContext().getRealPath("/WEB-INF")));
            }
            firstRun = false;
            
            // The MigrationLauncher is responsible for handling the interaction
            // between the PatchTable and the underlying MigrationTasks; as each
            // task is executed, the patch level is incremented, etc.
            try
            {
                JdbcMigrationLauncherFactory launcherFactory = 
                    JdbcMigrationLauncherFactoryLoader.createFactory();
                JdbcMigrationLauncher launcher
                    = launcherFactory.createMigrationLauncher(sce);
                launcher.doMigrations();
            }
            catch (MigrationException e)
            {
                // Runtime exceptions coming from a ServletContextListener prevent the
                // application from being deployed.  In this case, the intention is
                // for migration-enabled applications to fail-fast if there are any
                // errors during migration.
                throw new RuntimeException("Migration exception caught during migration", e);
            }
        }
        catch (RuntimeException e)
        {
            // Catch all exceptions for the sole reason of logging in
            // as many places as possible - debugging migration
            // problems requires detection first, and that means
            // getting the word of failures out.
            log.error(e);
            System.out.println(e.getMessage());
            e.printStackTrace(System.out);
            System.err.println(e.getMessage());
            e.printStackTrace(System.err);
            
            throw e;
        }
    }

    /** {@inheritDoc} */
    public void contextDestroyed(ServletContextEvent sce)
    {
        log.debug("context is being destroyed " + sce);
    }
}

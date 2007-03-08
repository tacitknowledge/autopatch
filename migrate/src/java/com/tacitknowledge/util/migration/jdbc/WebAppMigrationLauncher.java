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

import javax.servlet.ServletContext;
import javax.servlet.ServletContextEvent;
import javax.servlet.ServletContextListener;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.discovery.ClassDiscoveryUtil;
import com.tacitknowledge.util.discovery.WebAppResourceListSource;
import com.tacitknowledge.util.migration.MigrationException;

/**
 * Launches the migration process upon application context creation.  This class
 * is intentionally fail-fast, meaning that it throws a RuntimeException if any
 * problems arise during migration and will prevent the web application from
 * being fully deployed.
 * <p>
 * This class expects the following servlet context init parameters:
 * <ul>
 *    <li>migration.systemname - the name of the logical system being migrated</li>
 * </ul>
 * <p>
 * Below is an example of how this class can be configured in web.xml:
 * <pre>
 *   ...
 *   &lt;context-param&gt;
 *       &lt;param-name&gt;migration.systemname&lt;/param-name&gt;
 *       &lt;param-value&gt;milestone&lt;/param-value&gt;
 *   &lt;/context-param&gt;
 *   ...
 *   &lt;!-- immediately after the filter configs... --&gt;
 *   ...
 *   &lt;listener&gt;
 *       &lt;listener-class&gt;
 *           com.tacitknowledge.util.migration.patchtable.WebAppMigrationLauncher
 *       &lt;/listener-class&gt;
 *   &lt;/listener&gt;
 *   ...
 * </pre> 
 * 
 * @author  Scott Askew (scott@tacitknowledge.com)
 * @version $Id$
 * @see     com.tacitknowledge.util.migration.MigrationProcess
 */
public class WebAppMigrationLauncher implements ServletContextListener
{
    /**
     * Keeps track of the first run of the class within this web app deployment.
     * This should always be true, but you can never be too careful.
     */
    private static boolean firstRun = true;
    
    /**
     * Class logger
     */
    private static Log log = LogFactory.getLog(WebAppMigrationLauncher.class);
    
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
            
            String systemName = getRequiredParam("migration.systemname", sce);
            
            // The MigrationLauncher is responsible for handling the interaction
            // between the PatchTable and the underlying MigrationTasks; as each
            // task is executed, the patch level is incremented, etc.
            try
            {
                JdbcMigrationLauncherFactory launcherFactory = 
                    JdbcMigrationLauncherFactoryLoader.createFactory(); 
                JdbcMigrationLauncher launcher
                    = launcherFactory.createMigrationLauncher(systemName);
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

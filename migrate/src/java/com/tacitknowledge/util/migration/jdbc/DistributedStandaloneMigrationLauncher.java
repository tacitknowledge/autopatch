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

import java.util.Properties;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;


/**
 * Launches the migration process as a standalone application.  
 * <p>
 * This class expects the following Java environment parameters:
 * <ul>
 *    <li>migration.systemname - the name of the logical system being migrated</li>
 * </ul>
 * <p>
 * Below is an example of how this class can be configured in build.xml:
 * <pre>
 *   ...
 *  &lt;target name="patch.database" description="Runs the migration system"&gt;
 *   &lt;java 
 *       fork="true"
 *       classpathref="patch.classpath" 
 *       failonerror="true" 
 *       classname="com.tacitknowledge.util.migration.jdbc.DistributedStandaloneMigrationLauncher"&gt;
 *     &lt;sysproperty key="migration.systemname" value="${application.name}"/&gt;
 *   &lt;/java&gt;
 * &lt;/target&gt;
 *   ...
 * </pre> 
 * 
 * @author  Mike Hardy (mike@tacitknowledge.com)
 * @see     com.tacitknowledge.util.migration.DistributedMigrationProcess
 */
public class DistributedStandaloneMigrationLauncher
{
    /**
     * Class logger
     */
    private static Log log = LogFactory.getLog(DistributedStandaloneMigrationLauncher.class);
    
    /**
     * Private constructor - this object shouldn't be instantiated
     */
    private DistributedStandaloneMigrationLauncher()
    { 
        // does nothing
    }
    
    /**
     * Run the migrations for the given system name
     *
     * @param arguments the command line arguments, if any (none are used)
     * @exception Exception if anything goes wrong
     */
    public static void main(String[] arguments) throws Exception
    {
        String systemName = getRequiredParam("migration.systemname", 
                System.getProperties(), arguments);
        
        // The MigrationLauncher is responsible for handling the interaction
        // between the PatchTable and the underlying MigrationTasks; as each
        // task is executed, the patch level is incremented, etc.
        try
        {
            DistributedJdbcMigrationLauncherFactory factory = 
                new DistributedJdbcMigrationLauncherFactory();
            JdbcMigrationLauncher launcher = factory.createMigrationLauncher(systemName);
            launcher.doMigrations();
        }
        catch (Exception e)
        {
            log.error(e);
            throw e;
        }
    }

    /**
     * Returns the value of the specified servlet context initialization parameter.
     * 
     * @param  param the parameter to return
     * @param  properties the <code>Properties</code> for the Java system
     * @param  arguments optionally takes the arguments passed into the main to 
     * use as the migration system name
     * @return the value of the specified system initialization parameter
     * @throws IllegalArgumentException if the parameter does not exist
     */
    private static String getRequiredParam(String param, Properties properties, 
            String[] arguments) throws IllegalArgumentException
    {
        String value = properties.getProperty(param);
        if (value == null)
        {
            if ((arguments != null) && (arguments.length > 0))
            {
                value = arguments[0].trim();
            }
            else
            {
                throw new IllegalArgumentException("'" + param + "' is a required "
                        + "initialization parameter.  Aborting.");
            }
        }
        return value;
    }
}

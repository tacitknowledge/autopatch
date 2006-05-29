/* Copyright 2004 Tacit Knowledge LLC
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

package com.tacitknowledge.util.migration;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.jdbc.JdbcMigrationLauncher;

/**
 * Discovers and executes a sequence of system patches from multiple controlled
 * systems, each of which has its own MigrationProcess.
 * 
 * @author  Scott Askew (scott@tacitknowledge.com)
 * @see MigrationProcess   
 */
public class DistributedMigrationProcess extends MigrationProcess
{
    /** Class logger */
    private static Log log = LogFactory.getLog(DistributedMigrationProcess.class);
    
    /** The JdbcMigrationLaunchers we are controlling */
    private HashMap controlledSystems = new HashMap();
    
    /**
     * Creates a new <code>Migration</code> instance.
     */
    public DistributedMigrationProcess()
    {
        super();
    }
    
    /**
     * Applies necessary patches to the system.
     * 
     * @param  currentLevel the current system patch level
     * @param  context information and resources that are available to the migration tasks
     * @throws MigrationException if a migration fails
     * @return the number of <code>MigrationTask</code>s that have executed
     */
    public int doMigrations(int currentLevel, MigrationContext context)
        throws MigrationException
    {
        log.trace("Starting doMigrations");
        List migrations = getMigrationTasks();
        validateTasks(migrations);
        Collections.sort(migrations);
        int taskCount = 0;
        
        for (Iterator i = migrations.iterator(); i.hasNext();)
        {
            MigrationTask task = (MigrationTask) i.next();
            if (task.getLevel().intValue() > currentLevel)
            {
                applyPatch(context, task);
                taskCount++;
            }
        }
        
        if (taskCount > 0)
        {
            log.info("Migration complete (" + taskCount + " tasks executed)");
        }
        else
        {
            log.info("System up-to-date.  No migration tasks have been run.");
        }
        
        return taskCount;
    }

    /**
     * Returns a list of all migration tasks, regardless of patch level.
     * 
     * @return a list of all migration tasks
     * @throws MigrationException if one or more migration tasks could not be
     *         created
     */
    public List getMigrationTasks() throws MigrationException
    {
        List tasks = new ArrayList();
        
        for (Iterator controlledSystemIter = getControlledSystems().keySet().iterator();
            controlledSystemIter.hasNext();)
        {
            String controlledSystemName = (String)controlledSystemIter.next();
            JdbcMigrationLauncher launcher = 
                (JdbcMigrationLauncher)getControlledSystems().get(controlledSystemName);
            List subTasks = launcher.getMigrationProcess().getMigrationTasks();
            log.info("Found " + subTasks.size() + " for system " + controlledSystemName);
            if (log.isDebugEnabled())
            {
                for (Iterator subTaskIter = subTasks.iterator(); subTaskIter.hasNext();)
                {
                    log.debug("\tFound subtask " + ((MigrationTask)subTaskIter.next()).getName());
                }
            }
            tasks.addAll(subTasks);
        }
        
        // Its difficult to tell what's going on when you don't see any patches.
        // This will help people realize they don't have patches, and perhaps
        // help them discover why.
        if (tasks.size() == 0)
        {
            log.info("No patches were discovered in your classpath. "
                     + "Run with DEBUG logging enabled for patch search details.");
        }
        
        return tasks;
    }

    /**
     * Get the list of systems we are controlling
     * 
     * @return HashMap of JdbcMigrationLauncher objects keyed by String system names
     */
    public HashMap getControlledSystems()
    {
        return controlledSystems;
    }

    /**
     * Set the list of systems to control
     * 
     * @param controlledSystems HashMap of JdbcMigrationLauncher objects keyed by String system names
     */
    public void setControlledSystems(HashMap controlledSystems)
    {
        this.controlledSystems = controlledSystems;
    }
}

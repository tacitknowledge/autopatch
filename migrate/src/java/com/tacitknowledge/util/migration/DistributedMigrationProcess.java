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

package com.tacitknowledge.util.migration;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.List;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.jdbc.JdbcMigrationContext;
import com.tacitknowledge.util.migration.jdbc.JdbcMigrationLauncher;

/**
 * Discovers and executes a sequence of system patches from multiple controlled
 * systems, each of which has its own MigrationProcess.
 * 
 * @author Mike Hardy (mike@tacitknowledge.com)
 * @see com.tacitknowledge.util.migration.MigrationProcess   
 */
public class DistributedMigrationProcess extends MigrationProcess
{
    /** Class logger */
    private static Log log = LogFactory.getLog(DistributedMigrationProcess.class);
    
    /** The JdbcMigrationLaunchers we are controlling, keyed by system name */
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
        log.debug("Starting doMigrations");
        
        // Get all the migrations, with their launchers, then get the list of just the migrations
        LinkedHashMap migrationsWithLaunchers = getMigrationTasksWithLaunchers();
        List migrations = new ArrayList();
        migrations.addAll(migrationsWithLaunchers.keySet());
        
        // make sure the migrations are okay, then sort them
        validateTasks(migrations);
        Collections.sort(migrations);
        
        // Roll through once, just printing out what we'll do
        int taskCount = 0;
        for (Iterator i = migrations.iterator(); i.hasNext();)
        {
            MigrationTask task = (MigrationTask) i.next();
            if (task.getLevel().intValue() > currentLevel)
            {
                log.info("Will execute patch task '" + getTaskLabel(task) + "'");
                if (log.isDebugEnabled())
                {
                    JdbcMigrationLauncher launcher = 
                        (JdbcMigrationLauncher) migrationsWithLaunchers.get(task);
                    // Get all the contexts the task will execute in
                    for (Iterator j = launcher.getContexts().keySet().iterator(); j.hasNext();)
                    {
                        MigrationContext launcherContext = (MigrationContext) j.next();
                        log.debug("Task will execute in context '" + launcherContext + "'");
                    }
                }
                taskCount++;
            }
        }
        if (taskCount > 0)
        {
            log.info("A total of " + taskCount + " patch tasks will execute.");
        }
        else
        {
            log.info("System up-to-date.  No patch tasks will execute.");
        }
        
        // See if we should execute
        if (isReadOnly())
        {
            if (taskCount > 0)
            {
                throw new MigrationException("Unapplied patches exist, but read-only flag is set");
            }
            
            log.info("In read-only mode - skipping patch application");
            return 0;
        }
        
        // Roll through each migration, applying it if necessary
        taskCount = 0;
        for (Iterator i = migrations.iterator(); i.hasNext();)
        {
            MigrationTask task = (MigrationTask) i.next();
            if (task.getLevel().intValue() > currentLevel)
            {
                // Execute the task in the context it was loaded from
                JdbcMigrationLauncher launcher = 
                    (JdbcMigrationLauncher) migrationsWithLaunchers.get(task);
                // Get all the contexts the task will execute in
                for (Iterator j = launcher.getContexts().keySet().iterator(); j.hasNext();)
                {
                    MigrationContext launcherContext = (MigrationContext) j.next();
                    applyPatch(launcherContext, task, true);
                }
                taskCount++;
            }
        }
        
        if (taskCount > 0)
        {
            log.info("Patching complete (" + taskCount + " patch tasks executed)");
        }
        else
        {
            log.info("System up-to-date.  No patch tasks have been run.");
        }
        
        return taskCount;
    }

    /**
     * Returns a LinkedHashMap of task/launcher pairings, regardless of patch level.
     * 
     * @return LinkedHashMap containing MigrationTask / JdbcMigrationLauncher pairings
     * @throws MigrationException if one or more migration tasks could not be
     *         created
     */
    public LinkedHashMap getMigrationTasksWithLaunchers() throws MigrationException
    {
        LinkedHashMap tasks = new LinkedHashMap();
        
        // Roll through all our controlled system names
        for (Iterator controlledSystemIter = getControlledSystems().keySet().iterator();
            controlledSystemIter.hasNext();)
        {
            // Get the sub launcher that runs patches for the current name
            String controlledSystemName = (String) controlledSystemIter.next();
            JdbcMigrationLauncher subLauncher = 
                (JdbcMigrationLauncher) getControlledSystems().get(controlledSystemName);
            
            // Get all the tasks for that sub launcher
            List subTasks = subLauncher.getMigrationProcess().getMigrationTasks();
            log.info("Found " + subTasks.size() + " for system " + controlledSystemName);
            for (Iterator subTaskIter = subTasks.iterator(); subTaskIter.hasNext();)
            {
                MigrationTask task = (MigrationTask) subTaskIter.next();
                if (log.isDebugEnabled())
                {
                    Iterator launchers = subLauncher.getContexts().keySet().iterator();
                    String systemName = ((JdbcMigrationContext) launchers.next()).getSystemName();
                    log.debug("\tMigration+Launcher binder found subtask " 
                              + task.getName() + " for launcher context " 
                              + systemName);
                }
                
                // store the task, related to its launcher
                tasks.put(task, subLauncher);
            }
        }
        
        return tasks;
    }

    /**
     * Returns a List of MigrationTasks, regardless of patch level.
     * 
     * @return List containing MigrationTask objects
     * @throws MigrationException if one or more migration tasks could not be
     *         created
     */
    public List getMigrationTasks() throws MigrationException
    {
        List tasks = new ArrayList();
        
        for (Iterator controlledSystemIter = getControlledSystems().keySet().iterator();
            controlledSystemIter.hasNext();)
        {
            String controlledSystemName = (String) controlledSystemIter.next();
            JdbcMigrationLauncher launcher = 
                (JdbcMigrationLauncher) getControlledSystems().get(controlledSystemName);
            List subTasks = launcher.getMigrationProcess().getMigrationTasks();
            log.info("Found " + subTasks.size() + " for system " + controlledSystemName);
            if (log.isDebugEnabled())
            {
                for (Iterator subTaskIter = subTasks.iterator(); subTaskIter.hasNext();)
                {
                    log.debug("\tFound subtask " + ((MigrationTask) subTaskIter.next()).getName());
                }
            }
            tasks.addAll(subTasks);
        }
        
        // Its difficult to tell what's going on when you don't see any patches.
        // This will help people realize they don't have patches, and perhaps
        // help them discover why.
        if (tasks.size() == 0)
        {
            log.info("No patch tasks were discovered in your classpath. "
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
     * @param controlledSystems HashMap of system name / JdbcMigrationLauncher pairs
     */
    public void setControlledSystems(HashMap controlledSystems)
    {
        this.controlledSystems = controlledSystems;
    }
}

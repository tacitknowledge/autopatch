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
import java.util.Map;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

/**
 * Discovers and executes a sequence of system patches.  Patches take the form
 * of <code>MigrationTask</code> instances, each of which performs an atomic
 * migration or patch transaction.  <code>MigrationTask</code>s are executed
 * sequentially based on the result of <code>MigrationTask.getOrder</code>.  No
 * two tasks can return the same result for <code>getOrder</code>, and this
 * class will throw a <code>MigrationException</code> should such a situation
 * occur.
 * <p>
 * One useful pre-defined <code>MigrationTask</code> is
 * <code>SqlScriptMigrationTask</code>, which wraps a .SQL file and executes
 * all statements inside it.  Any file in the migration task search path that
 * matches the pattern "^patch(\d+)(_.+)?\.sql" will be wrapped with the 
 * <code>SqlScriptMigrationTask</code>.  The execution order for these tasks
 * is defined by the number immediately following the "patch" part of the SQL
 * script file name.
 * <p>
 * Example:
 * <pre>
 *    // Find the patches
 *    migrationRunner.addResourcePackage("com.example.myapp.migration");
 *    migrationRunner.addResourceDirectory("db/sql");
 * 
 *    try
 *    {
 *        <i>... figure out the current patch level...</i>
 *        migrationRunner.doMigration(currentLevel, context);
 *        <i>... update patch level</i>
 *        <i>... commit MigrationContext ...</i>
 *    }
 *    catch (MigrationException e)
 *    {
 *        <i>... rollback MigrationContext ...</i>
 *    }
 * </pre>
 * 
 * @author  Scott Askew (scott@tacitknowledge.com)
 */
public class MigrationProcess
{
    /** Class logger */
    private static Log log = LogFactory.getLog(MigrationProcess.class);
    
    /**
     * The list of package names containing the <code>MigrationTask</code>s
     * and SQL scripts to execute. 
     */
    private List resourcePackages = new ArrayList();
    
    /**
     * Migration task providers
     */
    private List migrationTaskSources = new ArrayList();
    
    /**
     * Used to broadcast migration task notifications
     */
    private MigrationBroadcaster broadcaster = new MigrationBroadcaster();
    
    /**
     * Creates a new <code>Migration</code> instance.
     */
    public MigrationProcess()
    {
        addMigrationTaskSource(new ClassMigrationTaskSource());
    }
    
    /**
     * Adds the given package to the migration task search path.
     * 
     * @param packageName the name of the package to add to the search path  
     */
    public void addResourcePackage(String packageName)
    {
        resourcePackages.add(packageName);
    }

    /**
     * Adds the given classpath-relative directory to the migration task
     * search path.
     * 
     * @param dir the name of the directory to add to the search path  
     */
    public void addResourceDirectory(String dir)
    {
        // Make the path package-name-like so that ClassLoader.getResourceAsStream
        // will work correctly
        String packageName = dir.replace('/', '.').replace('\\', '.');
        addResourcePackage(packageName);
    }
    
    /**
     * Adds a <code>MigrationTaskSource</code> to the list of sources that
     * provide this instance with <code>MigrationTask</code>s.
     * 
     * @param source the <code>MigrationTaskSource</code> to add; may not
     *        be <code>null</code>
     */
    public void addMigrationTaskSource(MigrationTaskSource source)
    {
        if (source == null)
        {
            throw new IllegalArgumentException("source cannot be null.");
        }
        migrationTaskSources.add(source);
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
     * Apply a single patch
     * 
     * @param context the context the patch will need during application
     * @param task the application task to carry out
     * @throws MigrationException if the patch application fails
     */
    public void applyPatch(MigrationContext context, MigrationTask task) throws MigrationException
    {
        String label = getTaskLabel(task);
        broadcaster.notifyListeners(task, context, MigrationBroadcaster.TASK_START);
        log.debug("broadcaster has " + broadcaster.getListeners().size() + " listeners");
        log.info("Running migration task \"" + label + "\"...");
        try
        {
            long startTime = System.currentTimeMillis();
            task.migrate(context);
            long duration = System.currentTimeMillis() - startTime;
            log.info("Finished migration task \"" + label + "\" (" + duration + " millis.)");
            broadcaster.notifyListeners(task, context, MigrationBroadcaster.TASK_SUCCESS);
            context.commit();
        }
        catch (MigrationException e)
        {
            broadcaster.notifyListeners(task, context, e, MigrationBroadcaster.TASK_FAILED);
            try
            {
                context.rollback();
                log.info("Migration failed; rollback successful");
            }
            catch (MigrationException me)
            {
                log.info("Migration failed; COULD NOT ROLL BACK TRANSACTION", me);
            }
            throw e;
        }
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
        for (Iterator i = resourcePackages.iterator(); i.hasNext();)
        {
            String packageName = (String) i.next();
            log.debug("Searching for patches in package " + packageName);
            
            for (Iterator j = migrationTaskSources.iterator(); j.hasNext();)
            {
                MigrationTaskSource source = (MigrationTaskSource) j.next();
                List sourceTasks = source.getMigrationTasks(packageName);
                if (log.isDebugEnabled())
                {
                    if (sourceTasks.size() > 0)
                    {
                        log.debug("Source [" + source + "] found " 
                                  + sourceTasks.size() + " patches: "
                                  + sourceTasks);
                    }
                    else
                    {
                        log.debug("Source [" + source + "] returned 0 patches.");
                    }
                }
                tasks.addAll(sourceTasks);
            }
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
     * Returns the number to use when creating the next patch.
     * 
     * @return the number to use when creating the next patch
     * @throws MigrationException if the existing tasks are invalid 
     */
    public int getNextPatchLevel() throws MigrationException
    {
        List tasks = getMigrationTasks();

        if (tasks.size() == 0)
        {
            return 1;
        }

        Collections.sort(tasks);
        validateTasks(tasks);
        MigrationTask lastTask = (MigrationTask) tasks.get(tasks.size() - 1);
        
        return lastTask.getLevel().intValue() + 1;
    }

    /**
     * Registers the given <code>MigrationListener</code> as being interested
     * in migration task events.
     * 
     * @param listener the listener to add; may not be <code>null</code>
     */
    public void addListener(MigrationListener listener)
    {
        broadcaster.addListener(listener);
    }

    /**
     * Removes the given <code>MigrationListener</code> from the list of listeners
     * associated with this <code>Migration</code> instance.
     * 
     * @param  listener the listener to add; may not be <code>null</code>
     * @return <code>true</code> if the listener was located and removed,
     *         otherwise <code>false</code>.
     */
    public boolean removeListener(MigrationListener listener)
    {
        return broadcaster.removeListener(listener);
    }
    
    /**
     * Get all of the MigrationListeners
     * 
     * @return List of MigrationListeners
     */
    public List getListeners()
    {
        return broadcaster.getListeners();
    }

    /**
     * Returns a user-friendly label for the specified task.
     * 
     * @param  task the task to create a label for
     * @return a user-friendly label for the specified task
     */
    private String getTaskLabel(MigrationTask task)
    {
        return task.getName() + " [" + task.getClass().getName() + "]";
    }

    /**
     * Ensures that no two <code>MigrationTasks</code> have the same ordering.
     * 
     * @param  migrations the list of defined migration tasks
     * @throws MigrationException if the migration tasks are not correctly defined
     */
    public void validateTasks(List migrations) throws MigrationException
    {
        Map usedOrderNumbers = new HashMap();
        for (Iterator i = migrations.iterator(); i.hasNext();)
        {
            MigrationTask task = (MigrationTask) i.next();
            
            Integer level = task.getLevel();
            if (level == null)
            {
                throw new MigrationException("Migration task '" + getTaskLabel(task)
                    + "' does not have a patch level defined.");
            }
            
            if (usedOrderNumbers.containsKey(level))
            {
                MigrationTask otherTask = (MigrationTask) usedOrderNumbers.get(level);
                throw new MigrationException("Migration task " + getTaskLabel(task)
                    + " has a conflicting patch level with " + getTaskLabel(otherTask)
                    + "; both are configured for patch level " + level);
            }
            
            usedOrderNumbers.put(level, task);
        }
    }
}

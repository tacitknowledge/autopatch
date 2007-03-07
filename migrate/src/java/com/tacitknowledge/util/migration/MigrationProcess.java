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
     * and SQL scripts to execute as patches
     */
    private List patchResourcePackages = new ArrayList();
    
    /**
     * The list of package names containing <code>MigrationTask</code>s
     * and SQL scripts to execute after patch execution
     */
    private List postPatchResourcePackages = new ArrayList();
    
    /**
     * Migration task providers
     */
    private List migrationTaskSources = new ArrayList();
    
    /**
     * Used to broadcast migration task notifications
     */
    private MigrationBroadcaster broadcaster = new MigrationBroadcaster();
    
    /**
     * Whether we actually want to apply patches, or just look
     */
    private boolean readOnly = false;
    
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
    public void addPatchResourcePackage(String packageName)
    {
        patchResourcePackages.add(packageName);
    }

    /**
     * Adds the given classpath-relative directory to the migration task
     * search path.
     * 
     * @param dir the name of the directory to add to the post-patch search path  
     */
    public void addPatchResourceDirectory(String dir)
    {
        // Make the path package-name-like so that ClassLoader.getResourceAsStream
        // will work correctly
        String packageName = dir.replace('/', '.').replace('\\', '.');
        addPatchResourcePackage(packageName);
    }
    
    /**
     * Adds the given package to the post-patch migration task search path.
     * 
     * @param packageName the name of the package to add to the search path  
     */
    public void addPostPatchResourcePackage(String packageName)
    {
        postPatchResourcePackages.add(packageName);
    }

    /**
     * Adds the given classpath-relative directory to the post-patch migration task
     * search path.
     * 
     * @param dir the name of the directory to add to the post-patch search path  
     */
    public void addPostPatchResourceDirectory(String dir)
    {
        // Make the path package-name-like so that ClassLoader.getResourceAsStream
        // will work correctly
        String packageName = dir.replace('/', '.').replace('\\', '.');
        addPostPatchResourcePackage(packageName);
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
        // FIXME test null source protection
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
        
        // Roll through once, just printing out what we'll do
        for (Iterator i = migrations.iterator(); i.hasNext();)
        {
            MigrationTask task = (MigrationTask) i.next();
            if (task.getLevel().intValue() > currentLevel)
            {
                log.info("Will execute patch task '" + 
                         task.getName() + " [" + task.getClass().getName() + "]" + 
                         "'");
                log.debug("Task will execute in context '" + context + "'");
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
        // FIXME test read-only mode throwing an exception or letting it go
        if (isReadOnly())
        {
            if (taskCount > 0)
            {
                throw new MigrationException("Unapplied patches exist, but read-only flag is set");
            }
            
            log.info("In read-only mode - skipping patch application");
            return 0;
        }
        
        // Now apply them
        taskCount = 0;
        for (Iterator i = migrations.iterator(); i.hasNext();)
        {
            MigrationTask task = (MigrationTask) i.next();
            if (task.getLevel().intValue() > currentLevel)
            {
                applyPatch(context, task, true);
                taskCount++;
            }
        }
        
        if (taskCount > 0)
        {
            log.info("Patching complete (" + taskCount + " patch tasks executed)");
        }
        else
        {
            log.info("System up-to-date.  No patch tasks executed.");
        }
        
        return taskCount;
    }
    
    /**
     * Run post-migration tasks
     * 
     * @return the number of <code>MigrationTask</code>s that executed
     * @exception MigrationException if a post-patch task fails
     */
    public int doPostPatchMigrations(MigrationContext context) throws MigrationException
    {
        log.info("Running post-patch tasks...");
        List postMigrationTasks = getPostPatchMigrationTasks();
        validateTasks(postMigrationTasks);
        Collections.sort(postMigrationTasks);
        
        if (postMigrationTasks.size() == 0)
        {
            log.info("No post-patch tasks found.");
            return 0;
        }
        
        // Roll through once, just printing out what we'll do
        int taskCount = 0;
        for (Iterator i = postMigrationTasks.iterator(); i.hasNext(); taskCount++)
        {
            MigrationTask task = (MigrationTask) i.next();
            log.info("Will execute post-patch task '" + 
                     task.getName() + " [" + task.getClass().getName() + "]" + 
                         "'");
        }
        log.info("A total of " + taskCount + " post-patch tasks will execute.");
        
        // See if we should execute
        // FIXME test read-only mode with no patches skipping post-patch tasks
        if (isReadOnly())
        {
            log.info("In read-only mode - skipping post-patch task execution");
            return 0;
        }
        
        // Now execute them
        taskCount = 0;
        for (Iterator i = postMigrationTasks.iterator(); i.hasNext(); taskCount++)
        {
            MigrationTask task = (MigrationTask) i.next();
            applyPatch(context, task, false);
        }
        log.info("Post-patch tasks complete (" + taskCount + " tasks executed)");
        
        return taskCount;
    }

    /**
     * Apply a single patch
     * 
     * @param context the context the patch will need during application
     * @param task the application task to carry out
     * @param broadcast whether to broadcast to listeners that the patch applied
     * @throws MigrationException if the patch application fails
     */
    public void applyPatch(MigrationContext context, MigrationTask task, boolean broadcast) 
        throws MigrationException
    {
        String label = getTaskLabel(task);
        if (broadcast)
        {
            broadcaster.notifyListeners(task, context, MigrationBroadcaster.TASK_START);
            log.debug("broadcaster has " + broadcaster.getListeners().size() + " listeners");
        }
        log.info("Executing patch task \"" + label + "\"...");
        
        try
        {
            long startTime = System.currentTimeMillis();
            task.migrate(context);
            long duration = System.currentTimeMillis() - startTime;
            log.info("Finished patch task \"" + label + "\" (" + duration + " millis.)");
            if (broadcast)
            {
                broadcaster.notifyListeners(task, context, MigrationBroadcaster.TASK_SUCCESS);
            }
            context.commit();
        }
        catch (MigrationException e)
        {
            if (broadcast)
            {
                broadcaster.notifyListeners(task, context, e, MigrationBroadcaster.TASK_FAILED);
            }
            try
            {
                context.rollback();
                log.info("Patch task failed; rollback successful");
            }
            catch (MigrationException me)
            {
                log.info("Patch task failed; COULD NOT ROLL BACK TRANSACTION", me);
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
        return getTasksFromPackages(patchResourcePackages);
    }

    /**
     * Returns a list of all post-patch migration tasks
     * 
     * @return a list of all post-patch migration tasks
     * @throws MigrationException if one or more post-patch migration tasks could not be created
     */
    public List getPostPatchMigrationTasks() throws MigrationException
    {
        return getTasksFromPackages(postPatchResourcePackages);
    }

    /**
     * Instantiate all the MigrationTask objects in the given resource packages
     * 
     * @param resourcePackages a List of Strings specifying package names to look for tasks in
     * @return List of MigrationTask objects instantiated from the given packages
     * @throws MigrationException if one or more post-patch migration tasks could not be created
     */
    private List getTasksFromPackages(List resourcePackages) throws MigrationException
    {
        List tasks = new ArrayList();
        for (Iterator i = resourcePackages.iterator(); i.hasNext();)
        {
            String packageName = (String) i.next();
            log.debug("Searching for patch tasks in package " + packageName);
            
            for (Iterator j = migrationTaskSources.iterator(); j.hasNext();)
            {
                MigrationTaskSource source = (MigrationTaskSource) j.next();
                List sourceTasks = source.getMigrationTasks(packageName);
                if (sourceTasks.size() > 0)
                {
                    log.debug("Source [" + source + "] found " 
                              + sourceTasks.size() + " patch tasks: "
                              + sourceTasks);
                }
                else
                {
                    log.debug("Source [" + source + "] returned 0 patch tasks.");
                }
                
                tasks.addAll(sourceTasks);
            }
        }
        
        // Its difficult to tell what's going on when you don't see any patches.
        // This will help people realize they don't have patches, and perhaps
        // help them discover why.
        if (tasks.size() == 0)
        {
            log.info("No patch tasks were discovered in your classpath. "
                     + "Run with DEBUG logging enabled for patch task search details.");
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
                throw new MigrationException("Patch task '" + getTaskLabel(task)
                    + "' does not have a patch level defined.");
            }
            
            if (usedOrderNumbers.containsKey(level))
            {
                MigrationTask otherTask = (MigrationTask) usedOrderNumbers.get(level);
                throw new MigrationException("Patch task " + getTaskLabel(task)
                    + " has a conflicting patch level with " + getTaskLabel(otherTask)
                    + "; both are configured for patch level " + level);
            }
            
            usedOrderNumbers.put(level, task);
        }
    }

    /**
     * See if we are actually applying patches, or if it is just readonly
     * 
     * @return boolean true if we will skip application
     */
    public boolean isReadOnly()
    {
        return readOnly;
    }

    /**
     * Set whether or not to actually apply patches
     * 
     * @param readOnly boolean true if we should skip application
     */
    public void setReadOnly(boolean readOnly)
    {
        this.readOnly = readOnly;
    }
}

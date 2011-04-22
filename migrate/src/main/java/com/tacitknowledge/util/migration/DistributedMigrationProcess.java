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
import java.util.Map.Entry;

import org.apache.commons.collections.CollectionUtils;
import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.jdbc.JdbcMigrationContext;
import com.tacitknowledge.util.migration.jdbc.JdbcMigrationLauncher;

/**
 * Discovers and executes a sequence of system patches from multiple controlled
 * systems, each of which has its own MigrationProcess.
 * 
 * @author Mike Hardy (mike@tacitknowledge.com)
 * @author Artie Pesh-Imam (apeshimam@tacitknowledge.com)
 * @see com.tacitknowledge.util.migration.MigrationProcess
 */
public class DistributedMigrationProcess extends MigrationProcess
{
    /** Class logger */
    private static Log log = LogFactory.getLog(DistributedMigrationProcess.class);

    /** The JdbcMigrationLaunchers we are controlling, keyed by system name */
    private HashMap controlledSystems = new HashMap();

    /**
     * If true, any nodes of the controlled systems that are not at the system's
     * current patch level are patched to bring them in sync with the other
     * nodes. This is not enabled by default because in a distributed system, if
     * there are cross schema dependencies, then the patching of the node may
     * fail since it is being patched 'out of order'.
     * 
     * For example:
     * 
     * system 1 has one node system 2 has one node
     * 
     * patch1 applies to system1 and creates a table patch2 applies to system2
     * and creates a table that references the table in system1 patch3 applies
     * to system2, dropping the reference to the table in system1 patch4 applies
     * to system1, dropping the table.
     * 
     * Later, to add capacity, system2 has a node added. When the second node is
     * forcibly 'synced' patches 2 and 3 are applied to it. The patching fails
     * when patch2 is applied because the table no longer exists in system1.
     * 
     * Therefore, forcing a sync is usually safe for systems that don't contain
     * external references, but should not be used for interdependent systems.
     * 
     * Instead, it would be better to import the schema from a node already at
     * the current patch level using database tools, then the new node can
     * participate in the regular patching process.
     * 
     */
    private boolean forceSync = false;

    /**
     * Creates a new <code>Migration</code> instance.
     */
    public DistributedMigrationProcess()
    {
        super();
    }

    /**
     * Execute a dry run of the rollback process and return a count of the
     * number of tasks which will rollback.
     * 
     * @param rollbacks a <code>List</code> of RollbackableMigrationTasks
     * @param migrationsWithLaunchers a <code>LinkedHashMap</code> of task to launcher
     * @return count of the number of rollbacks
     */
    protected final int rollbackDryRun(final List rollbacks, final LinkedHashMap rollbacksWithLaunchers)
    {
        // take the list of rollbacks
        // iterate through the rollbacks
        // log the context in which each rollback will execute

        int taskCount = 0;

        if (isPatchSetRollbackable(rollbacks))
        {
            for (Iterator i = rollbacks.iterator(); i.hasNext();)
            {
                RollbackableMigrationTask task = (RollbackableMigrationTask) i.next();
                log.info("Will execute rollback for task '" + task.getName() + "'");
                taskCount++;
                JdbcMigrationLauncher launcher = (JdbcMigrationLauncher) rollbacksWithLaunchers
                        .get(task);
                for (Iterator contextIterator = launcher.getContexts().keySet().iterator(); contextIterator
                        .hasNext();)
                {

                    MigrationContext context = (MigrationContext) contextIterator.next();
                    log.debug("Task will execute in context '" + context + "'");

                }
            }
        }

        return taskCount;
    }

    /**
     * Execute a dry run of the patch process and return a count of the number
     * of patches we would have executed.
     * 
     * @param currentPatchLevel The current patch level of the database
     * @param migrationsWithLaunchers a map of migration task to launcher
     * @return count of the number of patches
     */
    protected final int patchDryRun(final int currentPatchLevel, final LinkedHashMap migrationsWithLaunchers)
    {
        int taskCount = 0;

        for (Iterator i = migrationsWithLaunchers.entrySet().iterator(); i.hasNext();)
        {
            Entry entry = (Entry) i.next();
            MigrationTask task = (MigrationTask) entry.getKey();
            JdbcMigrationLauncher launcher = (JdbcMigrationLauncher) entry.getValue();

            if (task.getLevel().intValue() > currentPatchLevel)
            {
                log.info("Will execute patch task '" + getTaskLabel(task) + "'");
                if (log.isDebugEnabled())
                {
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

        return taskCount;
    }

    /**
     * Applies the necessary rollbacks to the system.
     * 
     * @param currentLevel the current system patch level
     * @param rollbackLevel the level that the system should rollback to
     * @param context information and resources that are available to the migration tasks
     * @throws MigrationException if a rollback fails
     * @return the number of <code>RollbackableMigrationTasks</code> which have been rolled back
     * @Override
     */
    public final int doRollbacks(final int currentLevel, final int rollbackLevel, final MigrationContext context,
            boolean forceRollback) throws MigrationException
    {
        log.debug("Starting doRollbacks");
        int taskCount = 0;

        // get all of the rollbacks, with launchers, then get the list of just
        // rollbacks
        LinkedHashMap rollbacksWithLaunchers = getMigrationTasksWithLaunchers();
        List rollbacks = new ArrayList();
        rollbacks.addAll(rollbacksWithLaunchers.keySet());

        // filter tasks which are not required to run because they are at a
        // level below the rollback level
        PatchRollbackPredicate rollbackPredicate = new PatchRollbackPredicate(currentLevel,
                rollbackLevel);
        CollectionUtils.filter(rollbacks, rollbackPredicate);

        // make sure that the rollbacks are ok, sort and then reverse them
        validateTasks(rollbacks);
        Collections.sort(rollbacks);
        Collections.reverse(rollbacks);

        validateControlledSystems(currentLevel);
        taskCount = rollbackDryRun(rollbacks, rollbacksWithLaunchers);

        if (taskCount > 0)
        {
            log.info("A total of " + taskCount + " rollback patch tasks will execute.");
        }
        else
        {
            log.info("System up-to-date.  No patch tasks will rollback.");
        }

        taskCount = 0;
        if (isPatchSetRollbackable(rollbacks) || forceRollback)
        {
            if (isReadOnly())
            {
                throw new MigrationException("Unapplied rollbacks exist, but read-only flag is set");
            }

            for (Iterator rollbackIterator = rollbacks.iterator(); rollbackIterator.hasNext();)
            {
                RollbackableMigrationTask task = (RollbackableMigrationTask) rollbackIterator
                        .next();
                // Execute the task in the context it was loaded from
                JdbcMigrationLauncher launcher = (JdbcMigrationLauncher) rollbacksWithLaunchers
                        .get(task);

                // iterate through all the contexts
                for (Iterator j = launcher.getContexts().keySet().iterator(); j.hasNext();)
                {
                    MigrationContext launcherContext = (MigrationContext) j.next();
                    applyRollback(launcherContext, task, true);
                }
                taskCount++;
            }

        }
        else
        {
            log
                    .info("Could not complete rollback because one or more of the tasks is not rollbackable.");
        }

        if (taskCount > 0)
        {
            log.info("Rollback complete (" + taskCount + " patch tasks rolledback)");
        }
        else
        {
            log.info("The system could not rollback the tasks");
        }
        return taskCount;
    }

    /**
     * Applies necessary patches to the system.
     * 
     * @param currentLevel the current system patch level
     * @param context information and resources that are available to the migration tasks
     * @throws MigrationException if a migration fails
     * @return the number of <code>MigrationTask</code>s that have executed
     * @Override
     */
    public final int doMigrations(final int currentLevel, final MigrationContext context) throws MigrationException
    {
        log.debug("Starting doMigrations");

        // Get all the migrations, with their launchers, then get the list of
        // just the migrations
        LinkedHashMap migrationsWithLaunchers = getMigrationTasksWithLaunchers();
        List migrations = new ArrayList();
        migrations.addAll(migrationsWithLaunchers.keySet());

        // make sure the migrations are okay, then sort them
        validateTasks(migrations);
        Collections.sort(migrations);

        validateControlledSystems(currentLevel);

        // determine how many tasks we're going to execute
        int taskCount = patchDryRun(currentLevel, migrationsWithLaunchers);

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
            if ((task.getLevel().intValue() > currentLevel) && !forceSync)
            {
                // Execute the task in the context it was loaded from
                JdbcMigrationLauncher launcher = (JdbcMigrationLauncher) migrationsWithLaunchers
                        .get(task);
                // Get all the contexts the task will execute in
                for (Iterator j = launcher.getContexts().keySet().iterator(); j.hasNext();)
                {
                    MigrationContext launcherContext = (MigrationContext) j.next();
                    applyPatch(launcherContext, task, true);
                }
                taskCount++;
            }
            else if (forceSync)// if a sync is forced, need to check all
            // the contexts to identify the ones out of
            // sync
            {
                boolean patchesApplied = false;
                ArrayList outOfSyncContexts = new ArrayList();

                // first need to iterate over all the contexts and determined
                // which one's are out of sync.
                // can't sync yet because if there are multiple contexts that
                // are out of sync, after the
                // first one is synced, the remaining one's have their patch
                // level updated via the
                // MigrationListener.migrationSuccessful event.
                JdbcMigrationLauncher launcher = (JdbcMigrationLauncher) migrationsWithLaunchers
                        .get(task);
                for (Iterator j = launcher.getContexts().keySet().iterator(); j.hasNext();)
                {
                    MigrationContext launcherContext = (MigrationContext) j.next();
                    PatchInfoStore patchInfoStore = (PatchInfoStore) launcher.getContexts().get(
                            launcherContext);

                    if (task.getLevel().intValue() > patchInfoStore.getPatchLevel())
                    {
                        outOfSyncContexts.add(launcherContext);
                    }
                }

                // next patch the contexts that have been determined to be out
                // of sync
                for (Iterator iter = outOfSyncContexts.iterator(); iter.hasNext();)
                {
                    MigrationContext launcherContext = (MigrationContext) iter.next();
                    applyPatch(launcherContext, task, true);
                    patchesApplied = true;
                }

                if (patchesApplied)
                {
                    taskCount++;
                }
            } // else if forceSync
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
     * Validates that the controlled systems are all at the current patch level.
     * 
     * @throws MigrationException if all the controlled systems are not at the current patch level.
     */
    protected final void validateControlledSystems(final int currentLevel) throws MigrationException
    {
        for (Iterator it = getControlledSystems().keySet().iterator(); it.hasNext();)
        {
            String systemName = (String) it.next();
            JdbcMigrationLauncher launcher = (JdbcMigrationLauncher) getControlledSystems().get(
                    systemName);
            for (Iterator contextIt = launcher.getContexts().keySet().iterator(); contextIt
                    .hasNext();)
            {
                JdbcMigrationContext ctx = (JdbcMigrationContext) contextIt.next();
                PatchInfoStore patchInfoStore = (PatchInfoStore) launcher.getContexts().get(ctx);
                int patchLevel = patchInfoStore.getPatchLevel();
                if (patchLevel != currentLevel)
                {
                    String message = "Database " + ctx.getDatabaseName()
                            + " is out of sync with system: " + systemName + ".  "
                            + ctx.getDatabaseName() + " is at patch level "
                            + Integer.toString(patchLevel) + " and the System is at patch level "
                            + Integer.toString(currentLevel) + ".";
                    if (getForceSync())
                    {
                        log.info(message + "  Continuing since 'forcesync' was specified.");
                    }
                    else
                    {
                        throw new MigrationException(message);
                    }
                }
            }
        }
    }

    /**
     * Returns a LinkedHashMap of task/launcher pairings, regardless of patch
     * level.
     * 
     * @return LinkedHashMap containing MigrationTask / JdbcMigrationLauncher
     *         pairings
     * @throws MigrationException if one or more migration tasks could not be created
     */
    public final LinkedHashMap getMigrationTasksWithLaunchers() throws MigrationException
    {
        LinkedHashMap tasks = new LinkedHashMap();

        // Roll through all our controlled system names
        for (Iterator controlledSystemIter = getControlledSystems().keySet().iterator(); controlledSystemIter
                .hasNext();)
        {
            // Get the sub launcher that runs patches for the current name
            String controlledSystemName = (String) controlledSystemIter.next();
            JdbcMigrationLauncher subLauncher = (JdbcMigrationLauncher) getControlledSystems().get(
                    controlledSystemName);

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
                    log.debug("\tMigration+Launcher binder found subtask " + task.getName()
                            + " for launcher context " + systemName);
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
     * @throws MigrationException if one or more migration tasks could not be created
     */
    public final List getMigrationTasks() throws MigrationException
    {
        List tasks = new ArrayList();

        for (Iterator controlledSystemIter = getControlledSystems().keySet().iterator(); controlledSystemIter
                .hasNext();)
        {
            String controlledSystemName = (String) controlledSystemIter.next();
            JdbcMigrationLauncher launcher = (JdbcMigrationLauncher) getControlledSystems().get(
                    controlledSystemName);
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
     * @return HashMap of JdbcMigrationLauncher objects keyed by String system
     *         names
     */
    public final HashMap getControlledSystems()
    {
        return controlledSystems;
    }

    /**
     * Set the list of systems to control
     * 
     * @param controlledSystems HashMap of system name / JdbcMigrationLauncher pairs
     */
    public final void setControlledSystems(final HashMap controlledSystems)
    {
        this.controlledSystems = controlledSystems;
    }

    public final boolean getForceSync()
    {
        return forceSync;
    }

    public final void setForceSync(final boolean forceSync)
    {
        this.forceSync = forceSync;
    }
}

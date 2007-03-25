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
#region Imports
using System;
using System.Collections.Generic;
using log4net;
using com.tacitknowledge.util.migration.ado;
#endregion

namespace com.tacitknowledge.util.migration
{
	/// <summary>
    /// Discovers and executes a sequence of system patches from multiple controlled systems,
    /// each of which has its own <code>MigrationProcess</code>.
	/// </summary>
	/// <author>Mike Hardy (mike@tacitknowledge.com)</author>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public class DistributedMigrationProcess : MigrationProcess
    {
        #region Member variables
        private static readonly ILog log = LogManager.GetLogger(typeof(DistributedMigrationProcess));
        
        /// <summary>
        /// <code>AdoMigrationLauncher</code>s we are controlling (keyed by system name)
        /// </summary>
        private IDictionary<String, AdoMigrationLauncher> controlledSystems =
            new Dictionary<String, AdoMigrationLauncher>();
        #endregion

        #region Public properties
        /// <summary>
        /// A dictionary of <code>IMigrationTask</code>/<code>AdoMigrationLauncher</code> pairings,
        /// regardless of patch level.
		/// </summary>
        /// <exception cref="MigrationException">
        /// if one or more migration tasks could not be created
        /// </exception>
	    public IDictionary<IMigrationTask, AdoMigrationLauncher> MigrationTasksWithLaunchers
		{
			get
			{
                IDictionary<IMigrationTask, AdoMigrationLauncher> tasks =
                    new Dictionary<IMigrationTask, AdoMigrationLauncher>();
				
				// Roll through all our controlled system names
                foreach (String controlledSystemName in ControlledSystems.Keys)
                {
                    //Get the sublauncher that runs patches for the current name
                    AdoMigrationLauncher subLauncher = ControlledSystems[controlledSystemName];
					
					// Get all the tasks for that sub launcher
					IList<IMigrationTask> subTasks = subLauncher.MigrationProcess.MigrationTasks;
					log.Info("Found " + subTasks.Count + " for system " + controlledSystemName);
					
                    foreach (IMigrationTask task in subTasks)
					{
						if (log.IsDebugEnabled)
						{
                            IEnumerator<IAdoMigrationContext> contexts =
                                ((IEnumerable<IAdoMigrationContext>)subLauncher.Contexts.Keys).GetEnumerator();

                            if (contexts.MoveNext())
                            {
                                String systemName = contexts.Current.SystemName;

                                log.Debug("\tMigration+Launcher binder found subtask " + task.Name
                                    + " for launcher context " + systemName);
                            }
						}
						
						// Store the task, related to its launcher
						tasks.Add(task, subLauncher);
					}
				}
				
				return tasks;
			}
		}

		/// <summary>
        /// A list of <code>IMigrationTask</code>s, regardless of patch level.
		/// </summary>
        /// <exception cref="MigrationException">
        /// if one or more migration tasks could not be created
        /// </exception>
        public override IList<IMigrationTask> MigrationTasks
		{
			get
			{
                IList<IMigrationTask> tasks = new List<IMigrationTask>();

                foreach (String controlledSystemName in ControlledSystems.Keys)
                {
                    AdoMigrationLauncher launcher = ControlledSystems[controlledSystemName];
                    IList<IMigrationTask> subTasks = launcher.MigrationProcess.MigrationTasks;

                    log.Info("Found " + subTasks.Count + " for system " + controlledSystemName);
					
                    foreach (IMigrationTask task in subTasks)
					{
                        if (log.IsDebugEnabled)
                        {
                            log.Debug("\tFound subtask " + task.Name);
                        }
                        
                        tasks.Add(task);
					}
				}
				
				// Its difficult to tell what's going on when you don't see any patches.
				// This will help people realize they don't have patches, and perhaps
				// help them discover why.
				if (tasks.Count == 0)
				{
                    log.Info("No migration tasks were discovered in your classpath. "
                        + "Run with DEBUG logging enabled for migration task search details.");
                }
				
				return tasks;
			}
		}

        /// <summary>
        /// <code>AdoMigrationLauncher</code>s we are controlling (keyed by system name).
        /// </summary>
        public IDictionary<String, AdoMigrationLauncher> ControlledSystems
		{
			get { return controlledSystems; }
			set { this.controlledSystems = value; }
		}
        #endregion

        #region Public methods
        /// <seealso cref="MigrationProcess.DoMigrations(int, IMigrationContext)"/>
		public override int DoMigrations(int currentLevel, IMigrationContext context)
		{
			log.Debug("Starting DoMigrations");
			
			// Get all migrations, with their launchers, then get the list of just the migrations
			IDictionary<IMigrationTask, AdoMigrationLauncher> migrationsWithLaunchers = MigrationTasksWithLaunchers;
            List<IMigrationTask> migrations = new List<IMigrationTask>();

            foreach (IMigrationTask mt in migrationsWithLaunchers.Keys)
            {
                migrations.Add(mt);
            }

            // make sure the migrations are okay, then sort them
            ValidateTasks(migrations);
            migrations.Sort();

            int taskCount = 0;
            // Roll through once, just printing out what we'll do
            foreach (IMigrationTask task in migrations)
            {
                if (task.Level.Value > currentLevel)
                {
                    log.Info("Will execute patch task '" + GetTaskLabel(task) + "'");

                    if (log.IsDebugEnabled)
                    {
                        AdoMigrationLauncher launcher = migrationsWithLaunchers[task];

                        // Get all the contexts the task will execute in
                        foreach (IMigrationContext launcherContext in launcher.Contexts.Keys)
                        {
                            log.Debug("Task will execute in context '" + launcherContext + "'");
                        }
                    }

                    taskCount++;
                }
            }

            if (taskCount > 0)
            {
                log.Info("A total of " + taskCount + " patch tasks will execute.");
            }
            else
            {
                log.Info("System up-to-date. No patch tasks will execute.");
            }

            // See if we should execute
            if (ReadOnly)
            {
                if (taskCount > 0)
                {
                    throw new MigrationException("Unapplied patches exist, but read-only flag is set");
                }

                log.Info("In read-only mode - skipping patch application");
                return 0;
            }

			// Roll through each migration, applying it if necessary
			taskCount = 0;
			foreach (IMigrationTask task in migrations)
			{
				if (task.Level.Value > currentLevel)
				{
					// Execute the task in the context it was loaded from
                    AdoMigrationLauncher launcher = migrationsWithLaunchers[task];

                    foreach (IMigrationContext launcherContext in launcher.Contexts.Keys)
                    {
                        ApplyPatch(launcherContext, task, true);
                    }
					
                    taskCount++;
				}
			}
			
			if (taskCount > 0)
			{
				log.Info("Migration complete (" + taskCount + " tasks executed)");
                
			}
			else
			{
				log.Info("System up-to-date. No migration tasks have been run.");
			}
			
			return taskCount;
		}
        #endregion
    }
}

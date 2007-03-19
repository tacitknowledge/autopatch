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
#region Imports
using System;
using System.Collections.Generic;
using log4net;
using log4net.Config;
using com.tacitknowledge.util.migration.ado;
using AutopatchNET.src.com.tacitknowledge.util.migration.ADO;
using AutopatchNET.src.com.tacitknowledge.util.migration.ADO.util;

#endregion
namespace com.tacitknowledge.util.migration
{
	
	/// <summary> Discovers and executes a sequence of system patches from multiple controlled
	/// systems, each of which has its own MigrationProcess.
	/// 
	/// </summary>
	/// <author>  Mike Hardy (mike@tacitknowledge.com)
	/// </author>
	/// <seealso cref="com.tacitknowledge.util.migration.MigrationProcess">
	/// </seealso>
	public class DistributedMigrationProcess:MigrationProcess
    {

        #region Members
        /// <summary>Class logger </summary>
       
        private static ILog log;

        /// <summary>The ADOMigrationLaunchers we are controlling, keyed by system name </summary>

        private AutopatchNET.src.com.tacitknowledge.util.migration.ADO.util.ControlledSystemsList controlledSystems = new AutopatchNET.src.com.tacitknowledge.util.migration.ADO.util.ControlledSystemsList();


        #endregion

        #region Methods
        /// <summary>
        /// Returns a dictionary of task/launcher pairings, regardless of patch level.
		/// </summary>
		/// <returns>
        /// Dictionary containing <code>IMigrationTask</code>s / <code>ADOMigrationLauncher</code> pairings
		/// </returns>
        /// <exception cref="MigrationException">
        /// if one or more migration tasks could not be created
        /// </exception>
	    public IDictionary<IMigrationTask, ADOMigrationLauncher> MigrationTasksWithLaunchers
		{
			get
			{
                IDictionary<IMigrationTask, ADOMigrationLauncher> tasks = new Dictionary<IMigrationTask, ADOMigrationLauncher>();
				
				// Roll through all our controlled system names
                foreach (ControlledSystem cs in ControlledSystems)
                {
                    //Get the sublauncher that runs patches for the current name
                    ADOMigrationLauncher subLauncher = cs.AdoMigrationLauncher;
					
					// Get all the tasks for that sub launcher
					IList<IMigrationTask> subTasks = subLauncher.MigrationProcess.MigrationTasks;
					log.Info("Found " + subTasks.Count + " for system " + cs.ControlledSystemName);
					
                    foreach (IMigrationTask task in subTasks)
					{
						if (log.IsDebugEnabled)
						{
							log.Debug("\tMigration+Launcher binder found subtask " + task.Name + " for launcher context " + subLauncher.Context.getSystemName());
						}
						
						// store the task, related to its launcher
						tasks.Add(task, subLauncher);
					}
				}
				
				return tasks;
			}
			
		}
		/// <summary> Returns a List of MigrationTasks, regardless of patch level.
		/// 
		/// </summary>
		/// <returns> List containing IMigrationTask objects
		/// </returns>
		/// <throws>  MigrationException if one or more migration tasks could not be </throws>
        public override IList<IMigrationTask> MigrationTasks
		{
			get
			{
                IList<IMigrationTask> tasks = new List<IMigrationTask>();
				
				
				//for (System.Collections.IEnumerator controlledSystemIter = new SupportClass.HashSetSupport(ControlledSystems.Keys).GetEnumerator(); controlledSystemIter.MoveNext(); )
				
                foreach (ControlledSystem cs in controlledSystems)
                {

                    ADOMigrationLauncher launcher = cs.AdoMigrationLauncher;
					IList<IMigrationTask> subTasks = launcher.MigrationProcess.MigrationTasks;
					log.Info("Found " + subTasks.Count + " for system " + cs.ControlledSystemName);
					if (log.IsDebugEnabled)
					{
                        foreach (IMigrationTask task in subTasks)
						{
							//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratornext'"
							log.Debug("\tFound subtask " + task.Name);
                            tasks.Add(task);
						}
					}
					
				}
				
				// Its difficult to tell what's going on when you don't see any patches.
				// This will help people realize they don't have patches, and perhaps
				// help them discover why.
				if (tasks.Count == 0)
				{
					log.Info("No patches were discovered in your classpath. " + "Run with DEBUG logging enabled for patch search details.");
				}
				
				return tasks;
			}
			
		}
		/// <summary> Get the list of systems we are controlling
		/// 
		/// </summary>
		/// <returns> Hashlist of ADOMigrationLauncher objects keyed by String system names
		/// </returns>
		
		/// <summary> 
        /// Set the list of systems to control
		/// 
		/// </summary>
        /// <returns>A list of systems to control and the ADOMigrationlauncher for controlling them</returns>
		/// <param name="controlledSystems">HashList of ADOMigrationLauncher objects keyed by String system names
		/// </param>
        public AutopatchNET.src.com.tacitknowledge.util.migration.ADO.util.ControlledSystemsList ControlledSystems
		{
			get
			{
				return controlledSystems;
			}
			
			set
			{
				this.controlledSystems = value;
			}
			
		}
		
		
		/// <summary> Creates a new <code>Migration</code> instance.</summary>
		public DistributedMigrationProcess():base()
		{
		}
		
		/// <summary> Applies necessary patches to the system.
		/// 
		/// </summary>
		/// <param name="currentLevel">the current system patch level
		/// </param>
		/// <param name="context">information and resources that are available to the migration tasks
		/// </param>
		/// <throws>  MigrationException if a migration fails </throws>
		/// <returns> the number of <code>IMigrationTask</code>s that have executed
		/// </returns>
		public override int DoMigrations(int currentLevel, IMigrationContext context)
		{
			log.Debug("Starting DoMigrations");
			
			// Get all the migrations, with their launchers, then get the list of just the migrations
			IDictionary<IMigrationTask, ADOMigrationLauncher> migrationsWithLaunchers = MigrationTasksWithLaunchers;
            List<IMigrationTask> migrations = new List<IMigrationTask>();
            foreach (IMigrationTask mt in migrationsWithLaunchers.Keys)
            {
                migrations.Add(mt);

                // make sure the migrations are okay, then sort them
               
            }
            ValidateTasks(migrations);
            migrations.Sort();
			
			// Roll through each migration, applying it if necessary
			int taskCount = 0;
			//we can use foreach here as we have an ArrayList of MigrationTasks or migrations
			foreach (IMigrationTask mts in migrations)
			{
				
				if (mts.Level > currentLevel)
				{
					// Execute the task in the context it was loaded from
                    ADOMigrationLauncher launcher = (ADOMigrationLauncher)migrationsWithLaunchers[mts];
					ApplyPatch(launcher.Context, mts, true);
					taskCount++;
				}
			}
			
			if (taskCount > 0)
			{
				log.Info("Migration complete (" + taskCount + " tasks executed)");
                
			}
			else
			{
				log.Info("System up-to-date.  No migration tasks have been run.");
			}
			
			return taskCount;
		}
		static DistributedMigrationProcess()
		{
			log = LogManager.GetLogger(typeof(DistributedMigrationProcess));
		}
    }
        #endregion
}
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
using System.Collections;
using log4net;
using log4net.Config;
using com.tacitknowledge.util.migration.ado;
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
      
        private System.Collections.Hashtable controlledSystems = new System.Collections.Hashtable();

        #endregion

        #region Methods
        /// <summary> Returns a LinkedHashMap of task/launcher pairings, regardless of patch level.
		/// 
		/// </summary>
		/// <returns> LinkedHashMap containing MigrationTask / ADOMigrationLauncher pairings
		/// </returns>
		/// <throws>  MigrationException if one or more migration tasks could not be </throws>
		/// <summary>         created
		/// </summary>
	    public HashTable MigrationTasksWithLaunchers
		{
			get
			{
				Hashtable tasks = new Hashtable();
				
				// Roll through all our controlled system names
				
				for (System.Collections.IEnumerator controlledSystemIter = new SupportClass.HashSetSupport(ControlledSystems.Keys).GetEnumerator(); controlledSystemIter.MoveNext(); )
				{
					// Get the sub launcher that runs patches for the current name
					System.String controlledSystemName = (System.String) controlledSystemIter.Current;
					//UPGRADE_TODO: Method 'java.util.HashMap.get' was converted to 'System.Collections.Hashtable.Item' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilHashMapget_javalangObject'"
					ADOMigrationLauncher subLauncher = (ADOMigrationLauncher) ControlledSystems[controlledSystemName];
					
					// Get all the tasks for that sub launcher
					System.Collections.IList subTasks = subLauncher.MigrationProcess.MigrationTasks;
					log.info("Found " + subTasks.Count + " for system " + controlledSystemName);
					//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratorhasNext'"
					for (System.Collections.IEnumerator subTaskIter = subTasks.GetEnumerator(); subTaskIter.MoveNext(); )
					{
						//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratornext'"
						MigrationTask task = (MigrationTask) subTaskIter.Current;
						if (log.isDebugEnabled())
						{
							log.debug("\tMigration+Launcher binder found subtask " + task.getName() + " for launcher context " + subLauncher.Context.getSystemName());
						}
						
						// store the task, related to its launcher
						tasks.put(task, subLauncher);
					}
				}
				
				return tasks;
			}
			
		}
		/// <summary> Returns a List of MigrationTasks, regardless of patch level.
		/// 
		/// </summary>
		/// <returns> List containing MigrationTask objects
		/// </returns>
		/// <throws>  MigrationException if one or more migration tasks could not be </throws>
		/// <summary>         created
		/// </summary>
		override public System.Collections.IList MigrationTasks
		{
			get
			{
				System.Collections.IList tasks = new System.Collections.ArrayList();
				
				//UPGRADE_TODO: Method 'java.util.HashMap.keySet' was converted to 'SupportClass.HashSetSupport' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilHashMapkeySet'"
				//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratorhasNext'"
				for (System.Collections.IEnumerator controlledSystemIter = new SupportClass.HashSetSupport(ControlledSystems.Keys).GetEnumerator(); controlledSystemIter.MoveNext(); )
				{
					//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratornext'"
					System.String controlledSystemName = (System.String) controlledSystemIter.Current;
					//UPGRADE_TODO: Method 'java.util.HashMap.get' was converted to 'System.Collections.Hashtable.Item' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilHashMapget_javalangObject'"
					ADOMigrationLauncher launcher = (ADOMigrationLauncher) ControlledSystems[controlledSystemName];
					System.Collections.IList subTasks = launcher.MigrationProcess.MigrationTasks;
					log.info("Found " + subTasks.Count + " for system " + controlledSystemName);
					if (log.isDebugEnabled())
					{
						//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratorhasNext'"
						for (System.Collections.IEnumerator subTaskIter = subTasks.GetEnumerator(); subTaskIter.MoveNext(); )
						{
							//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratornext'"
							log.debug("\tFound subtask " + ((MigrationTask) subTaskIter.Current).getName());
						}
					}
					SupportClass.ICollectionSupport.AddAll(tasks, subTasks);
				}
				
				// Its difficult to tell what's going on when you don't see any patches.
				// This will help people realize they don't have patches, and perhaps
				// help them discover why.
				if (tasks.Count == 0)
				{
					log.info("No patches were discovered in your classpath. " + "Run with DEBUG logging enabled for patch search details.");
				}
				
				return tasks;
			}
			
		}
		/// <summary> Get the list of systems we are controlling
		/// 
		/// </summary>
		/// <returns> HashMap of ADOMigrationLauncher objects keyed by String system names
		/// </returns>
		//UPGRADE_TODO: Class 'java.util.HashMap' was converted to 'System.Collections.Hashtable' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilHashMap'"
		/// <summary> Set the list of systems to control
		/// 
		/// </summary>
		/// <param name="controlledSystems">HashMap of ADOMigrationLauncher objects keyed by String system names
		/// </param>
		public System.Collections.Hashtable ControlledSystems
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
		/// <returns> the number of <code>MigrationTask</code>s that have executed
		/// </returns>
		public override int doMigrations(int currentLevel, MigrationContext context)
		{
			log.debug("Starting doMigrations");
			
			// Get all the migrations, with their launchers, then get the list of just the migrations
			LinkedHashMap migrationsWithLaunchers = MigrationTasksWithLaunchers;
			System.Collections.IList migrations = new System.Collections.ArrayList();
			migrations.addAll(migrationsWithLaunchers.keySet());
			
			// make sure the migrations are okay, then sort them
			validateTasks(migrations);
			SupportClass.CollectionsSupport.Sort(migrations, null);
			
			// Roll through each migration, applying it if necessary
			int taskCount = 0;
			//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratorhasNext'"
			for (System.Collections.IEnumerator i = migrations.GetEnumerator(); i.MoveNext(); )
			{
				//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratornext'"
				MigrationTask task = (MigrationTask) i.Current;
				if (task.getLevel() > currentLevel)
				{
					// Execute the task in the context it was loaded from
					ADOMigrationLauncher launcher = (ADOMigrationLauncher) migrationsWithLaunchers.get_Renamed(task);
					applyPatch(launcher.Context, task, true);
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
		static DistributedMigrationProcess()
		{
			log = LogManager.GetLogger(typeof(DistributedMigrationProcess));
		}
    }
        #endregion
}
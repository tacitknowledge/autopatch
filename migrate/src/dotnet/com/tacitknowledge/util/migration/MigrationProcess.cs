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
using log4net;
using log4net.Config;
#endregion
namespace com.tacitknowledge.util.migration
{
	
	/// <summary> Discovers and executes a sequence of system patches.  Patches take the form
	/// of <code>MigrationTask</code> instances, each of which performs an atomic
	/// migration or patch transaction.  <code>MigrationTask</code>s are executed
	/// sequentially based on the result of <code>MigrationTask.getOrder</code>.  No
	/// two tasks can return the same result for <code>getOrder</code>, and this
	/// class will throw a <code>MigrationException</code> should such a situation
	/// occur.
	/// <p>
	/// One useful pre-defined <code>MigrationTask</code> is
	/// <code>SqlScriptMigrationTask</code>, which wraps a .SQL file and executes
	/// all statements inside it.  Any file in the migration task search path that
	/// matches the pattern "^patch(\d+)(_.+)?\.sql" will be wrapped with the 
	/// <code>SqlScriptMigrationTask</code>.  The execution order for these tasks
	/// is defined by the number immediately following the "patch" part of the SQL
	/// script file name.
	/// <p>
	/// Example:
	/// <pre>
	/// // Find the patches
	/// migrationRunner.addResourcePackage("com.example.myapp.migration");
	/// migrationRunner.addResourceDirectory("db/sql");
	/// 
	/// try
	/// {
	/// <i>... figure out the current patch level...</i>
	/// migrationRunner.doMigration(currentLevel, context);
	/// <i>... update patch level</i>
	/// <i>... commit MigrationContext ...</i>
	/// }
	/// catch (MigrationException e)
	/// {
	/// <i>... rollback MigrationContext ...</i>
	/// }
	/// </pre>
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	public class MigrationProcess
	{
		/// <summary> Returns a list of all migration tasks, regardless of patch level.
		/// 
		/// </summary>
		/// <returns> a list of all migration tasks
		/// </returns>
		/// <throws>  MigrationException if one or more migration tasks could not be </throws>
		/// <summary>         created
		/// </summary>
		virtual public System.Collections.IList MigrationTasks
		{
			get
			{
				return getTasksFromPackages(patchResourcePackages);
			}
			
		}
		/// <summary> Returns a list of all post-patch migration tasks
		/// 
		/// </summary>
		/// <returns> a list of all post-patch migration tasks
		/// </returns>
		/// <throws>  MigrationException if one or more post-patch migration tasks could not be created </throws>
		virtual public System.Collections.IList PostPatchMigrationTasks
		{
			get
			{
				return getTasksFromPackages(postPatchResourcePackages);
			}
			
		}
		/// <summary> Returns the number to use when creating the next patch.
		/// 
		/// </summary>
		/// <returns> the number to use when creating the next patch
		/// </returns>
		/// <throws>  MigrationException if the existing tasks are invalid  </throws>
		virtual public int NextPatchLevel
		{
			get
			{
				System.Collections.IList tasks = MigrationTasks;
				
				if (tasks.Count == 0)
				{
					return 1;
				}
				
				SupportClass.CollectionsSupport.Sort(tasks, null);
				validateTasks(tasks);
				MigrationTask lastTask = (MigrationTask) tasks[tasks.Count - 1];
				
				return lastTask.getLevel() + 1;
			}
			
		}
		/// <summary> Get all of the MigrationListeners
		/// 
		/// </summary>
		/// <returns> List of MigrationListeners
		/// </returns>
		virtual public System.Collections.IList Listeners
		{
			get
			{
				return broadcaster.Listeners;
			}
			
		}
		/// <summary>Class logger </summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.MigrationProcess'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
		
		/// <summary> The list of package names containing the <code>MigrationTask</code>s
		/// and SQL scripts to execute as patches
		/// </summary>
		private System.Collections.IList patchResourcePackages = new System.Collections.ArrayList();
		
		/// <summary> The list of package names containing <code>MigrationTask</code>s
		/// and SQL scripts to execute after patch execution
		/// </summary>
		private System.Collections.IList postPatchResourcePackages = new System.Collections.ArrayList();
		
		/// <summary> Migration task providers</summary>
		private System.Collections.IList migrationTaskSources = new System.Collections.ArrayList();
		
		/// <summary> Used to broadcast migration task notifications</summary>
		private MigrationBroadcaster broadcaster = new MigrationBroadcaster();
		
		/// <summary> Creates a new <code>Migration</code> instance.</summary>
		public MigrationProcess()
		{
			addMigrationTaskSource(new ClassMigrationTaskSource());
		}
		
		/// <summary> Adds the given package to the migration task search path.
		/// 
		/// </summary>
		/// <param name="packageName">the name of the package to add to the search path  
		/// </param>
		public virtual void  addPatchResourcePackage(System.String packageName)
		{
			patchResourcePackages.Add(packageName);
		}
		
		/// <summary> Adds the given classpath-relative directory to the migration task
		/// search path.
		/// 
		/// </summary>
		/// <param name="dir">the name of the directory to add to the post-patch search path  
		/// </param>
		public virtual void  addPatchResourceDirectory(System.String dir)
		{
			// Make the path package-name-like so that ClassLoader.getResourceAsStream
			// will work correctly
			System.String packageName = dir.Replace('/', '.').Replace('\\', '.');
			addPatchResourcePackage(packageName);
		}
		
		/// <summary> Adds the given package to the post-patch migration task search path.
		/// 
		/// </summary>
		/// <param name="packageName">the name of the package to add to the search path  
		/// </param>
		public virtual void  addPostPatchResourcePackage(System.String packageName)
		{
			postPatchResourcePackages.Add(packageName);
		}
		
		/// <summary> Adds the given classpath-relative directory to the post-patch migration task
		/// search path.
		/// 
		/// </summary>
		/// <param name="dir">the name of the directory to add to the post-patch search path  
		/// </param>
		public virtual void  addPostPatchResourceDirectory(System.String dir)
		{
			// Make the path package-name-like so that ClassLoader.getResourceAsStream
			// will work correctly
			System.String packageName = dir.Replace('/', '.').Replace('\\', '.');
			addPostPatchResourcePackage(packageName);
		}
		
		/// <summary> Adds a <code>MigrationTaskSource</code> to the list of sources that
		/// provide this instance with <code>MigrationTask</code>s.
		/// 
		/// </summary>
		/// <param name="source">the <code>MigrationTaskSource</code> to add; may not
		/// be <code>null</code>
		/// </param>
		public virtual void  addMigrationTaskSource(MigrationTaskSource source)
		{
			if (source == null)
			{
				throw new System.ArgumentException("source cannot be null.");
			}
			migrationTaskSources.Add(source);
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
		public virtual int doMigrations(int currentLevel, MigrationContext context)
		{
			log.trace("Starting doMigrations");
			System.Collections.IList migrations = MigrationTasks;
			validateTasks(migrations);
			SupportClass.CollectionsSupport.Sort(migrations, null);
			int taskCount = 0;
			
			//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratorhasNext'"
			for (System.Collections.IEnumerator i = migrations.GetEnumerator(); i.MoveNext(); )
			{
				//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratornext'"
				MigrationTask task = (MigrationTask) i.Current;
				if (task.getLevel() > currentLevel)
				{
					applyPatch(context, task, true);
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
		
		/// <summary> Run post-migration tasks
		/// 
		/// </summary>
		/// <returns> the number of <code>MigrationTask</code>s that executed
		/// </returns>
		/// <exception cref="MigrationException">if a post-patch task fails
		/// </exception>
		public virtual int doPostPatchMigrations(MigrationContext context)
		{
			log.info("Running post-patch tasks...");
			System.Collections.IList postMigrationTasks = PostPatchMigrationTasks;
			validateTasks(postMigrationTasks);
			SupportClass.CollectionsSupport.Sort(postMigrationTasks, null);
			int taskCount = 0;
			//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratorhasNext'"
			for (System.Collections.IEnumerator i = postMigrationTasks.GetEnumerator(); i.MoveNext(); taskCount++)
			{
				//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratornext'"
				MigrationTask task = (MigrationTask) i.Current;
				applyPatch(context, task, false);
			}
			
			if (taskCount > 0)
			{
				log.info("Post-patch tasks complete (" + taskCount + " tasks executed)");
			}
			else
			{
				log.info("No post-patch tasks have been run.");
			}
			
			return taskCount;
		}
		
		/// <summary> Apply a single patch
		/// 
		/// </summary>
		/// <param name="context">the context the patch will need during application
		/// </param>
		/// <param name="task">the application task to carry out
		/// </param>
		/// <param name="broadcast">whether to broadcast to listeners that the patch applied
		/// </param>
		/// <throws>  MigrationException if the patch application fails </throws>
		public virtual void  applyPatch(MigrationContext context, MigrationTask task, bool broadcast)
		{
			System.String label = getTaskLabel(task);
			if (broadcast)
			{
				broadcaster.notifyListeners(task, context, MigrationBroadcaster.TASK_START);
				log.debug("broadcaster has " + broadcaster.Listeners.Count + " listeners");
			}
			log.info("Running migration task \"" + label + "\"...");
			try
			{
				long startTime = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
				task.migrate(context);
				long duration = (System.DateTime.Now.Ticks - 621355968000000000) / 10000 - startTime;
				log.info("Finished migration task \"" + label + "\" (" + duration + " millis.)");
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
					log.info("Migration failed; rollback successful");
				}
				catch (MigrationException me)
				{
					log.info("Migration failed; COULD NOT ROLL BACK TRANSACTION", me);
				}
				throw e;
			}
		}
		
		/// <summary> Instantiate all the MigrationTask objects in the given resource packages
		/// 
		/// </summary>
		/// <param name="resourcePackages">a List of Strings specifying package names to look for tasks in
		/// </param>
		/// <returns> List of MigrationTask objects instantiated from the given packages
		/// </returns>
		/// <throws>  MigrationException if one or more post-patch migration tasks could not be created </throws>
		private System.Collections.IList getTasksFromPackages(System.Collections.IList resourcePackages)
		{
			System.Collections.IList tasks = new System.Collections.ArrayList();
			//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratorhasNext'"
			for (System.Collections.IEnumerator i = resourcePackages.GetEnumerator(); i.MoveNext(); )
			{
				//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratornext'"
				System.String packageName = (System.String) i.Current;
				log.debug("Searching for migration tasks in package " + packageName);
				
				//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratorhasNext'"
				for (System.Collections.IEnumerator j = migrationTaskSources.GetEnumerator(); j.MoveNext(); )
				{
					//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratornext'"
					MigrationTaskSource source = (MigrationTaskSource) j.Current;
					System.Collections.IList sourceTasks = source.getMigrationTasks(packageName);
					if (log.isDebugEnabled())
					{
						if (sourceTasks.Count > 0)
						{
							//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
							log.debug("Source [" + source + "] found " + sourceTasks.Count + " migration tasks: " + SupportClass.CollectionToString(sourceTasks));
						}
						else
						{
							//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
							log.debug("Source [" + source + "] returned 0 migration tasks.");
						}
					}
					SupportClass.ICollectionSupport.AddAll(tasks, sourceTasks);
				}
			}
			
			// Its difficult to tell what's going on when you don't see any patches.
			// This will help people realize they don't have patches, and perhaps
			// help them discover why.
			if (tasks.Count == 0)
			{
				log.info("No migration tasks were discovered in your classpath. " + "Run with DEBUG logging enabled for migration task search details.");
			}
			
			return tasks;
		}
		
		/// <summary> Registers the given <code>MigrationListener</code> as being interested
		/// in migration task events.
		/// 
		/// </summary>
		/// <param name="listener">the listener to add; may not be <code>null</code>
		/// </param>
		public virtual void  addListener(MigrationListener listener)
		{
			broadcaster.addListener(listener);
		}
		
		/// <summary> Removes the given <code>MigrationListener</code> from the list of listeners
		/// associated with this <code>Migration</code> instance.
		/// 
		/// </summary>
		/// <param name="listener">the listener to add; may not be <code>null</code>
		/// </param>
		/// <returns> <code>true</code> if the listener was located and removed,
		/// otherwise <code>false</code>.
		/// </returns>
		public virtual bool removeListener(MigrationListener listener)
		{
			return broadcaster.removeListener(listener);
		}
		
		/// <summary> Returns a user-friendly label for the specified task.
		/// 
		/// </summary>
		/// <param name="task">the task to create a label for
		/// </param>
		/// <returns> a user-friendly label for the specified task
		/// </returns>
		private System.String getTaskLabel(MigrationTask task)
		{
			return task.getName() + " [" + task.GetType().FullName + "]";
		}
		
		/// <summary> Ensures that no two <code>MigrationTasks</code> have the same ordering.
		/// 
		/// </summary>
		/// <param name="migrations">the list of defined migration tasks
		/// </param>
		/// <throws>  MigrationException if the migration tasks are not correctly defined </throws>
		public virtual void  validateTasks(System.Collections.IList migrations)
		{
			//UPGRADE_TODO: Class 'java.util.HashMap' was converted to 'System.Collections.Hashtable' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilHashMap'"
			System.Collections.IDictionary usedOrderNumbers = new System.Collections.Hashtable();
			//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratorhasNext'"
			for (System.Collections.IEnumerator i = migrations.GetEnumerator(); i.MoveNext(); )
			{
				//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratornext'"
				MigrationTask task = (MigrationTask) i.Current;
				
				System.Int32 level = task.getLevel();
				//UPGRADE_TODO: The 'System.Int32' structure does not have an equivalent to NULL. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1291'"
				if (level == null)
				{
					throw new MigrationException("Migration task '" + getTaskLabel(task) + "' does not have a patch level defined.");
				}
				
				if (usedOrderNumbers.Contains(level))
				{
					MigrationTask otherTask = (MigrationTask) usedOrderNumbers[level];
					throw new MigrationException("Migration task " + getTaskLabel(task) + " has a conflicting patch level with " + getTaskLabel(otherTask) + "; both are configured for patch level " + level);
				}
				
				usedOrderNumbers[level] = task;
			}
		}
		static MigrationProcess()
		{
			log = LogManager.GetLogger(typeof(MigrationProcess));
		}
	}
}
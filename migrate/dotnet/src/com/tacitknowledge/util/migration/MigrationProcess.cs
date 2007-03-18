/*
 * Copyright 2006 Tacit Knowledge LLC
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
#endregion

namespace com.tacitknowledge.util.migration
{
	/// <summary>
    /// <para>
    /// Discovers and executes a sequence of system patches. Patches take the form
	/// of <code>IMigrationTask</code> instances, each of which performs an atomic
	/// migration or patch transaction. <code>IMigrationTask</code>s are executed
	/// sequentially based on the result of <code>IMigrationTask.Level</code>. No
	/// two tasks can return the same result for <code>Level</code>, and this
	/// class will throw a <code>MigrationException</code> should such a situation
	/// occur.
    /// </para>
	/// <para>
	/// One useful pre-defined <code>IMigrationTask</code> is
	/// <code>SqlScriptMigrationTask</code>, which wraps a .SQL file and executes
	/// all statements inside it. Any file in the migration task search path that
	/// matches the pattern "^patch(\d+)(_.+)?\.sql" will be wrapped with the 
	/// <code>SqlScriptMigrationTask</code>. The execution order for these tasks
	/// is defined by the number immediately following the "patch" part of the SQL
	/// script file name.
    /// </para>
	/// <para>
	/// Example:
	/// <code>
    /// MigrationProcess process = new MigrationProcess();
	/// // Find the patches
    /// process.AddPatchResourceAssembly("D:/Some/Project/Folder/TestLibrary.DLL");
    /// process.AddPatchResourceDirectory("db/sql");
	/// 
	/// try
	/// {
	/// <i>... figure out the current patch level...</i>
    /// process.DoMigrations(currentLevel, context);
	/// <i>... update patch level</i>
	/// <i>... commit IMigrationContext ...</i>
	/// }
	/// catch (MigrationException e)
	/// {
	/// <i>... rollback IMigrationContext ...</i>
	/// }
    /// </code>
	/// </para>
	/// </summary>
	/// <author>Scott Askew (scott@tacitknowledge.com)</author>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public class MigrationProcess
    {
        #region Member variables
        private static ILog log;

        /// <summary>
        /// The list of assembly names containing <code>IMigrationTask</code>s
        /// and SQL scripts to execute as patches.
        /// </summary>
        private IList<String> patchResourceAssemblies = new List<String>();

        /// <summary>
        /// The list of assembly names containing <code>IMigrationTask</code>s
        /// and SQL scripts to execute after patch execution.
        /// </summary>
        private IList<String> postPatchResourceAssemblies = new List<String>();

        /// <summary>
        /// The list of <code>IMigrationTaskSource</code>s.
        /// </summary>
        private IList<IMigrationTaskSource> migrationTaskSources = new List<IMigrationTaskSource>();
        #endregion

        #region Public delegates
        /// <summary>
        /// Defines the delegate for migration status events.
        /// </summary>
        /// <param name="task">the task being run for migration</param>
        /// <param name="context">the migration context</param>
        /// <param name="e">the exception if error status; <code>null</code> otherwise</param>
        public delegate void MigrationStatusEventHandler(IMigrationTask task, IMigrationContext context, MigrationException e);
        #endregion

        #region Public events
        /// <summary>
        /// Notifies the listeners that the given task is about to start execution.
        /// </summary>
        public event MigrationStatusEventHandler MigrationStarted;

        /// <summary>
        /// Notifies the listener that the given task has completed execution.
        /// </summary>
        public event MigrationStatusEventHandler MigrationSuccessful;

        /// <summary>
        /// Notifies the listener that the given task has failed execution.
        /// </summary>
        public event MigrationStatusEventHandler MigrationFailed;
        #endregion

        #region Costructors
        /// <summary>
        /// Static constructor.
        /// </summary>
        static MigrationProcess()
        {
            log = LogManager.GetLogger(typeof(MigrationProcess));
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MigrationProcess()
        {
            AddMigrationTaskSource(new ClassMigrationTaskSource());
        }
        #endregion
        
        #region Public properties
        /// <summary>
        /// Returns a list of all migration tasks, regardless of patch level.
		/// </summary>
        /// <exception cref="MigrationException">if one or more migration tasks could not be created</exception>
        public virtual IList<IMigrationTask> MigrationTasks
		{
			get
			{
				return GetTasksFromAssemblies(patchResourceAssemblies);
			}
		}

		/// <summary>
        /// Returns a list of all post-patch migration tasks.
		/// </summary>
        /// <exception cref="MigrationException">if one or more post-patch migration tasks could not be created</exception>
        public virtual IList<IMigrationTask> PostPatchMigrationTasks
		{
			get
			{
				return GetTasksFromAssemblies(postPatchResourceAssemblies);
			}
		}

		/// <summary>
        /// Returns the number to use when creating the next patch.
		/// </summary>
        /// <exception cref="MigrationException">if the existing tasks are invalid</exception>
        public virtual int NextPatchLevel
		{
			get
			{
                List<IMigrationTask> tasks = (List<IMigrationTask>) MigrationTasks;
				
				if (tasks.Count == 0)
				{
					return 1;
				}
				
                // Sort tasks.
                // Validate them.
                // Get the last task number.
                tasks.Sort();
				ValidateTasks(tasks);
				IMigrationTask lastTask = tasks[tasks.Count - 1];
				
				return lastTask.Level.Value + 1;
			}
		}
        #endregion
        
        #region Public methods
        /// <summary>
        /// Adds the given assembly to the migration task search path.
		/// </summary>
		/// <param name="assemblyName">
        /// the name of the assembly to add to the search path  
		/// </param>
		public virtual void AddPatchResourceAssembly(String assemblyName)
		{
			patchResourceAssemblies.Add(assemblyName);
		}
		
		/// <summary>
        /// Adds the given classpath-relative directory to the migration task search path.
		/// </summary>
		/// <param name="dir">
        /// the name of the directory to add to the post-patch search path  
		/// </param>
		public virtual void AddPatchResourceDirectory(String dir)
		{
            // TODO Clarify what to do with .SQL patch resources
			// Make the path package-name-like so that ClassLoader.getResourceAsStream
			// will work correctly
            String packageName = dir.Replace('/', '.').Replace('\\', '.');
			AddPatchResourceAssembly(packageName);
		}
		
		/// <summary>
        /// Adds the given assembly to the post-patch migration task search path.
		/// </summary>
		/// <param name="assemblyName">
        /// the name of the assembly to add to the search path  
		/// </param>
        public virtual void AddPostPatchResourceAssembly(String assemblyName)
		{
            postPatchResourceAssemblies.Add(assemblyName);
		}
		
		/// <summary>
        /// Adds the given classpath-relative directory to the post-patch migration task
		/// search path.
		/// </summary>
		/// <param name="dir">
        /// the name of the directory to add to the post-patch search path  
		/// </param>
		public virtual void AddPostPatchResourceDirectory(String dir)
		{
            // TODO Clarify what to do with .SQL patch resources
			// Make the path package-name-like so that ClassLoader.getResourceAsStream
			// will work correctly
			String packageName = dir.Replace('/', '.').Replace('\\', '.');
			AddPostPatchResourceAssembly(packageName);
		}
		
		/// <summary>
        /// Adds a <code>IMigrationTaskSource</code> to the list of sources that
		/// provide this instance with <code>IMigrationTask</code>s.
		/// </summary>
		/// <param name="source">
        /// the <code>IMigrationTaskSource</code> to add; may not be <code>null</code>
		/// </param>
		public virtual void AddMigrationTaskSource(IMigrationTaskSource source)
		{
			if (source == null)
			{
				throw new ArgumentException("source cannot be null.");
			}

            migrationTaskSources.Add(source);
		}
		
		/// <summary>
        /// Applies necessary patches to the system.
		/// </summary>
		/// <param name="currentLevel">
        /// the current system patch level
		/// </param>
		/// <param name="context">
        /// information and resources that are available to the migration tasks
		/// </param>
		/// <returns>the number of <code>IMigrationTask</code>s that have executed</returns>
        /// <exception cref="MigrationException">if a migration fails</exception>
        public virtual int DoMigrations(int currentLevel, IMigrationContext context)
		{
			log.Info("Starting DoMigrations");
            //Console.WriteLine("Starting DoMigrations");

            List<IMigrationTask> migrations = (List<IMigrationTask>) MigrationTasks;
            
            migrations.Sort();
			ValidateTasks(migrations);
			
            int taskCount = 0;

            // Loop through migration tasks and see if we should apply them
            foreach (IMigrationTask task in migrations)
            {
                if (task.Level.Value > currentLevel)
                {
                    ApplyPatch(context, task, true);
                    taskCount++;
                }
            }
            
			if (taskCount > 0)
			{
				log.Info("Migration complete (" + taskCount + " tasks executed)");
                //Console.WriteLine("Migration complete (" + taskCount + " tasks executed)");
			}
			else
			{
				log.Info("System up-to-date. No migration tasks have been run.");
                //Console.WriteLine("System up-to-date. No migration tasks have been run.");
			}
			
			return taskCount;
		}
		
		/// <summary>
        /// Run post-migration tasks.
		/// </summary>
        /// <returns>
        /// the number of <code>IMigrationTask</code>s that executed
		/// </returns>
        /// <exception cref="MigrationException">if a post-patch task fails</exception>
        public virtual int DoPostPatchMigrations(IMigrationContext context)
		{
			log.Info("Running post-patch tasks...");
            //Console.WriteLine("Running post-patch tasks...");
            List<IMigrationTask> postMigrationTasks = (List<IMigrationTask>) PostPatchMigrationTasks;

            postMigrationTasks.Sort();
			ValidateTasks(postMigrationTasks);

            if (postMigrationTasks.Count == 0)
            {
                // No post migration tasks to perform
                return 0;
            }

			int taskCount = 0;

            foreach (IMigrationTask task in postMigrationTasks)
			{
				ApplyPatch(context, task, false);
                taskCount++;
			}
			
			if (taskCount > 0)
			{
				log.Info("Post-patch tasks complete (" + taskCount + " tasks executed)");
                //Console.WriteLine("Post-patch tasks complete (" + taskCount + " tasks executed)");
			}
			else
			{
				log.Info("No post-patch tasks have been run.");
                //Console.WriteLine("No post-patch tasks have been run.");
			}
			
			return taskCount;
		}
		
		/// <summary>
        /// Apply a single patch.
		/// </summary>
		/// <param name="context">
        /// the context the patch will need during application
		/// </param>
		/// <param name="task">
        /// the application task to carry out
		/// </param>
		/// <param name="broadcast">
        /// whether to broadcast to listeners that the patch applied
		/// </param>
        /// <exception cref="MigrationException">if the patch application fails </exception>
		public virtual void ApplyPatch(IMigrationContext context, IMigrationTask task, bool broadcast)
		{
			String label = GetTaskLabel(task);
			
            if (broadcast)
			{
                if (MigrationStarted != null)
                {
                    MigrationStarted(task, context, null);
                    //broadcaster.notifyListeners(task, context, MigrationBroadcaster.TASK_START);
                    //log.Debug("broadcaster has " + broadcaster.Listeners.Count + " listeners");
                }
			}
			
            log.Info("Running migration task \"" + label + "\"...");
            //Console.WriteLine("Running migration task \"" + label + "\"...");
			
            try
			{
				long startTime = DateTime.Now.Ticks;
                task.Migrate(context);
                long duration = (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond;
                log.Info("Finished migration task \"" + label + "\" (" + duration + " millis.)");
                //Console.WriteLine("Finished migration task \"" + label + "\" (" + duration + " millis.)");
				
                if (broadcast)
				{
                    if (MigrationSuccessful != null)
                    {
                        MigrationSuccessful(task, context, null);
                        //broadcaster.notifyListeners(task, context, MigrationBroadcaster.TASK_SUCCESS);
                    }
				}
				
                context.Commit();
			}
			catch (MigrationException e)
			{
				if (broadcast)
				{
                    if (MigrationFailed != null)
                    {
                        MigrationFailed(task, context, e);
                        //broadcaster.notifyListeners(task, context, e, MigrationBroadcaster.TASK_FAILED);
                    }
				}
				
                try
				{
					context.Rollback();
					log.Info("Migration failed; rollback successful");
                    //Console.WriteLine("Migration failed; rollback successful");
				}
				catch (MigrationException me)
				{
					log.Info("Migration failed; COULD NOT ROLL BACK TRANSACTION", me);
                    //Console.WriteLine("Migration failed; COULD NOT ROLL BACK TRANSACTION");
				}
				
                throw e;
			}
		}
		
		/// <summary>
        /// Ensures that no two <code>IMigrationTask</code>s have the same ordering.
		/// </summary>
		/// <param name="migrations">the list of defined migration tasks</param>
        /// <exception cref="MigrationException">
        /// if the migration tasks are not correctly defined
        /// </exception>
		public virtual void ValidateTasks(IList<IMigrationTask> migrations)
		{
			// We need to get tasks in the right order here
            IDictionary<int, IMigrationTask> useOrderedNumbers = new Dictionary<int, IMigrationTask>();
            
            foreach (IMigrationTask task in migrations)
            {
				int? level = task.Level;
				
				if (!level.HasValue)
				{
					throw new MigrationException("Migration task '" + GetTaskLabel(task) + "' does not have a patch level defined.");
				}

                if (useOrderedNumbers.ContainsKey(level.Value))
				{
					IMigrationTask otherTask = (IMigrationTask) useOrderedNumbers[level.Value];
					throw new MigrationException("Migration task " + GetTaskLabel(task) + " has a conflicting patch level with " + GetTaskLabel(otherTask) + "; both are configured for patch level " + level.Value);
				}
                
                useOrderedNumbers[level.Value] = task;
			}
		}
        #endregion

        #region Private members
        /// <summary>
        /// Returns a user-friendly label for the specified task.
        /// </summary>
        /// <param name="task">the task to create a label for</param>
        /// <returns>a user-friendly label for the specified task</returns>
        private String GetTaskLabel(IMigrationTask task)
        {
            return task.Name + " [" + task.GetType().FullName + "]";
        }

        /// <summary>
        /// Instantiate all the <code>IMigrationTask</code> objects in the given resource assemblies.
        /// </summary>
        /// <param name="resourceAssemblies">
        /// a list of strings specifying assembly names to look for tasks in
        /// </param>
        /// <returns>
        /// a list of <code>IMigrationTask</code> objects instantiated from the given assemblies
        /// </returns>
        /// <exception cref="MigrationException">
        /// if one or more tasks could not be created
        /// </exception>
        private IList<IMigrationTask> GetTasksFromAssemblies(IList<String> resourceAssemblies)
        {
            IList<IMigrationTask> tasks = new List<IMigrationTask>();

            foreach (String assemblyPath in resourceAssemblies)
            {
                log.Debug("Searching for migration tasks in assembly " + assemblyPath);
                //Console.WriteLine("Searching for migration tasks in assembly " + assemblyPath);

                foreach (IMigrationTaskSource source in migrationTaskSources)
                {
                    IList<IMigrationTask> sourceTasks = source.GetMigrationTasks(assemblyPath);

                    if (sourceTasks.Count > 0)
                    {
                        // We have tasks to add so we'll add them here
                        log.Debug("Source [" + source + "] found " + sourceTasks.Count + " migration tasks: " + sourceTasks.ToString());
                        //Console.WriteLine("Source [" + source + "] found " + sourceTasks.Count + " migration tasks: " + sourceTasks.ToString());
                        foreach (IMigrationTask task in sourceTasks)
                        {
                            // Add the source task
                            tasks.Add(task);
                        }
                    }
                    else
                    {
                        log.Debug("Source [" + source + "] returned 0 migration tasks.");
                        //Console.WriteLine("Source [" + source + "] returned 0 migration tasks.");
                    }
                }
            }

            // Its difficult to tell what's going on when you don't see any patches.
            // This will help people realize they don't have patches, and perhaps
            // help them discover why.
            if (tasks.Count == 0)
            {
                log.Info("No migration tasks were discovered in your classpath. "
                    + "Run with DEBUG logging enabled for migration task search details.");
                //Console.WriteLine("No migration tasks were discovered in your classpath. Run with DEBUG logging enabled for migration task search details.");
            }

            return tasks;
        }
        #endregion
    }
}

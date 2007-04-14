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
using System.Data.Common;
using System.IO;
using System.Threading;
using log4net;
using com.tacitknowledge.util.migration;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
	/// <summary>
    /// Core starting point for a database migration run. This class obtains a connection
	/// to the database, checks its patch level, delegates the actual execution of the migration
	/// tasks to a <code>MigrationProcess</code> instance, and then commits and cleans everything
	/// up at the end.
	/// </summary>
	/// <author>Scott Askew (scott@tacitknowledge.com)</author>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public class AdoMigrationLauncher
	{
        #region Member variables
        /// <summary>
        /// The token separator char for separating entries in paths.
        /// </summary>
        private static readonly char[] PATH_TOKEN_DELIMITER = new char[] { ';' };
        private static readonly ILog log = LogManager.GetLogger(typeof(AdoMigrationLauncher));
		
        /// <summary>
        /// The <code>MigrationProcess</code> responsible for applying the patches.
        /// </summary>
		private MigrationProcess migrationProcess = null;
		
        /// <summary>
        /// The amount of time, in milliseconds, between attempts to obtain a lock on the patches
        /// table. Defaults to 15 seconds.
        /// </summary>
		private long lockPollMillis = 15000;
		
        /// <summary>
        /// The path containing directories and packages to search through to locate patches.
        /// </summary>
		private String patchPath = null;
		
        /// <summary>
        /// The path containing directories and packages to search through to locate post-patch tasks.
        /// </summary>
		private String postPatchPath = null;
		
		/// <summary>
        /// A dictionary of <code>IAdoMigrationContext</code>/<code>IPatchInfoStore</code> pairings
        /// used for the migrations.
		/// </summary>
		private IDictionary<IAdoMigrationContext, IPatchInfoStore> contexts =
            new Dictionary<IAdoMigrationContext, IPatchInfoStore>();
		
        /// <summary>
        /// Indicates whether we actually want to apply patches, or just look.
        /// </summary>
		private bool readOnly = false;
        #endregion

        #region Private delegates
        /// <summary>
        /// The delegate to invoke when adding a patch directory.
        /// </summary>
        /// <param name="directoryPath">The directory path to process</param>
        private delegate void AddDirectoryDelegate(String directoryPath);
        /// <summary>
        /// The delegate to invoke when adding a patch assembly.
        /// </summary>
        /// <param name="assemblyPath">The assembly path to process</param>
        private delegate void AddAssemblyDelegate(String assemblyPath);
        #endregion
        
        #region Public properties
        /// <summary>
        /// The <code>MigrationProcess</code> we'll use to migrate things.
		/// </summary>
		public virtual MigrationProcess NewMigrationProcess
		{
			get { return new MigrationProcess(); }
		}

        /// <summary>
        /// The path containing directories (separated by ";") to search through to locate patches.
        /// </summary>
		public virtual String PatchPath
		{
			get { return patchPath; }

			set
			{
				this.patchPath = value;
                ProcessPath(value, migrationProcess.AddPatchResourceAssembly, migrationProcess.AddPatchResourceDirectory);
			}
		}

        /// <summary>
        /// The path containing directories (separated by ";") to search through to locate post-patch
        /// tasks.
        /// </summary>
		public virtual String PostPatchPath
		{
			get { return postPatchPath; }
			
			set
			{
				this.postPatchPath = value;
                ProcessPath(value, migrationProcess.AddPostPatchResourceAssembly, migrationProcess.AddPostPatchResourceDirectory);
			}
		}

		/// <summary>
        /// The next patch level, for use when creating a new patch.
		/// </summary>
		public virtual int NextPatchLevel
		{
			get { return migrationProcess.NextPatchLevel; }
		}

		/// <summary>
        /// A dictionary of <code>IAdoMigrationContext</code>/<code>IPatchInfoStore</code> pairings
        /// used for the migrations.
		/// </summary>
		public virtual IDictionary<IAdoMigrationContext, IPatchInfoStore> Contexts
		{
			get { return contexts; }
		}
		
        /// <summary>
        /// The amount of time, in milliseconds, between attempts to obtain a lock on the patches
        /// table. Defaults to 15 seconds.
        /// </summary>
		public virtual long LockPollMillis
		{
			get { return lockPollMillis; }
			set { this.lockPollMillis = value; }
		}

        /// <summary>
        /// The <code>MigrationProcess</code> responsible for applying the patches.
        /// </summary>
		public virtual MigrationProcess MigrationProcess
		{
			get { return migrationProcess; }
			set { this.migrationProcess = value; }
		}
		
        /// <summary>
        /// Indicates whether we actually want to apply patches, or just look.
        /// </summary>
		public virtual bool ReadOnly
		{
			get { return readOnly; }
			set { this.readOnly = value; }
		}
        #endregion
		
        #region Constructors
		/// <summary>
        /// Create a new <code>MigrationProcess</code> and add a <code>SqlScriptMigrationTaskSource</code>.
        /// </summary>
		public AdoMigrationLauncher()
		{
			MigrationProcess = NewMigrationProcess;
			
   			// Make sure this class is notified when a patch is applied so that
			// the patch level can be updated (see #MigrationSuccessful).
            migrationProcess.MigrationStarted += new MigrationProcess.MigrationStatusEventHandler(MigrationStarted);
            migrationProcess.MigrationSuccessful += new MigrationProcess.MigrationStatusEventHandler(MigrationSuccessful);
            migrationProcess.MigrationFailed += new MigrationProcess.MigrationStatusEventHandler(MigrationFailed);
			
			MigrationProcess.AddMigrationTaskSource(new SqlScriptMigrationTaskSource());
		}
		
		/// <summary>
        /// Create a new <code>AdoMigrationLauncher</code>.
		/// </summary>
		/// <param name="context">
        /// the <code>IAdoMigrationContext</code> to use
		/// </param>
		public AdoMigrationLauncher(IAdoMigrationContext context)
            : this()
		{
			AddContext(context);
		}
        #endregion
		
        #region Public methods
		/// <summary>
        /// Starts the application migration process.
		/// </summary>
		/// <returns>
        /// the number of patches applied
		/// </returns>
		/// <exception cref="MigrationException">
        /// if an unrecoverable error occurs during
        /// </exception>
		public virtual int DoMigrations()
		{
			if (contexts.Count == 0)
			{
				throw new MigrationException("You must configure a migration context");
			}
			
			try
			{
                int migrationCount = 0;

                foreach (IAdoMigrationContext context in contexts.Keys)
                {
                    migrationCount = DoMigrations(context);
                    log.Info("Executed " + migrationCount + " patches for context " + context);
                }
				
                return migrationCount;
			}
			catch (DbException e)
			{
				throw new MigrationException("DbException during migration", e);
			}
		}
		
        /// <seealso cref="MigrationProcess.MigrationStatusEventHandler(IMigrationTask, IMigrationContext, MigrationException)"/>
        public void MigrationStarted(IMigrationTask task, IMigrationContext ctx, MigrationException e)
		{
			log.Debug("Started task " + task.Name + " for context " + ctx);
		}

        /// <seealso cref="MigrationProcess.MigrationStatusEventHandler(IMigrationTask, IMigrationContext, MigrationException)"/>
        public void MigrationSuccessful(IMigrationTask task, IMigrationContext ctx, MigrationException e)
		{
			log.Debug("Task " + task.Name + " was successful for context " + ctx + " in launcher " + this);
			int patchLevel = task.Level.Value;
			
			// update all of our controlled patch tables
            foreach (IPatchInfoStore patchInfoStore in contexts.Values)
			{
                patchInfoStore.UpdatePatchLevel(patchLevel);
			}
		}

        /// <seealso cref="MigrationProcess.MigrationStatusEventHandler(IMigrationTask, IMigrationContext, MigrationException)"/>
        public void MigrationFailed(IMigrationTask task, IMigrationContext ctx, MigrationException e)
		{
			log.Debug("Task " + task.Name + " failed for context " + ctx, e);
		}

		/// <summary>
        /// Get the patch level from the database.
		/// </summary>
		/// <param name="ctx">
        /// the migration context to get the patch level for
		/// </param>
		/// <returns>
        /// the current database patch level
		/// </returns>
		/// <exception cref="MigrationException">
        /// if there is a database connection error, or the patch level can't be determined
		/// </exception>
		public virtual int GetDatabasePatchLevel(IAdoMigrationContext ctx)
		{
            return contexts[ctx].PatchLevel;
		}
		
		/// <summary>
        /// Adds an <code>IAdoMigrationContext</code> used for the migrations.
		/// </summary>
		/// <param name="context">
        /// the <code>IAdoMigrationContext</code> used for the migrations
		/// </param>
		public virtual void AddContext(IAdoMigrationContext context)
		{
			IPatchInfoStore patchTable = new PatchTable(context);
			log.Debug("Adding context " + context + " with patch table " + patchTable + " in launcher " + this);
			contexts.Add(context, patchTable);
		}
        #endregion

        #region Protected methods
		/// <summary>
        /// Performs the application migration process in one go.
		/// </summary>
		/// <param name="context">
        /// the context to run the patches in
		/// </param>
		/// <returns>
        /// the number of patches applied
		/// </returns>
		/// <exception cref="DbException">
        /// if an unrecoverable database error occurs while working with the patches table
		/// </exception>
		/// <exception cref="MigrationException">
        /// if an unrecoverable error occurs during the migration
		/// </exception>
		protected virtual int DoMigrations(IAdoMigrationContext context)
		{
			IPatchInfoStore patchTable = CreatePatchStore(context);
			
			LockPatchStore(context);
			
			// Now apply the patches
			int executedPatchCount = 0;
			try
			{
				int patchLevel = patchTable.PatchLevel;
				
    			executedPatchCount = migrationProcess.DoMigrations(patchLevel, context);
			}
			catch (MigrationException me)
			{
				// If there was any kind of error, we don't want to eat it, but we do
				// want to unlock the patch store. So do that, then re-throw.
				patchTable.UnlockPatchStore();
				throw me;
			}
			
			// Do any post-patch tasks
			try
			{
				migrationProcess.DoPostPatchMigrations(context);
				return executedPatchCount;
			}
			finally
			{
				try
				{
					patchTable.UnlockPatchStore();
				}
				catch (MigrationException e)
				{
					log.Error("Error unlocking patch table: ", e);
				}
			}
		}

        /// <summary>
        /// Create a patch table object for use in migrations.
		/// </summary>
		/// <param name="context">
        /// the context to create the store in
		/// </param>
		/// <returns>
        /// a <code>PatchInfoStore</code> object for use in accessing patch state information
		/// </returns>
		/// <exception cref="MigrationException">
        /// if unable to create the store
        /// </exception>
		protected virtual IPatchInfoStore CreatePatchStore(IAdoMigrationContext context)
		{
			//IPatchInfoStore piStore = new PatchTable(context);
			
			// Make sure the table is created before claiming it exists by returning
            IPatchInfoStore piStore = contexts[context];
			int patchLevel = piStore.PatchLevel;
			
			return piStore;
		}
        #endregion
		
        #region Private methods
        /// <summary>
        /// The path to examine which will populate patch directories and assemblies. This method will
        /// also process all subdirectories of the supplied path.
        /// </summary>
        /// <param name="path">the path to examine</param>
        /// <param name="addAssemblyDelegate">the delegate to invoke for addition of patch directory</param>
        /// <param name="addDirectoryDelegate">the delegate to invoke for addiotion of patch assembly</param>
        private void ProcessPath(String path, AddAssemblyDelegate addAssemblyDelegate, AddDirectoryDelegate addDirectoryDelegate)
        {
            String[] entries = path.Split(PATH_TOKEN_DELIMITER, StringSplitOptions.RemoveEmptyEntries);
            foreach (String entry in entries)
            {
                if (Directory.Exists(entry))
                {
                    // Add the directory itself (since its children might be SQL patches)
                    addDirectoryDelegate(entry);

                    // Find and add all .NET code assemblies residing in DLL files
                    String[] allFiles = Directory.GetFiles(entry, "*.dll", SearchOption.AllDirectories);
                    foreach (String fileName in allFiles)
                    {
                        addAssemblyDelegate(fileName);
                        log.Debug("Adding .NET code assembly with path: " + fileName);
                    }

                }
                else
                {
                    log.Debug("The supplied path '" + entry + "' does not point to a directory. Skipping");
                }
            }

        }

        /// <summary>
        /// Lock the patch store. This is done safely, such that we safely handle the case where
		/// other migration launchers are patching at the same time.
		/// </summary>
		/// <param name="context">
        /// the context to lock the store in
		/// </param>
		/// <exception cref="MigrationException">
        /// if the reading or setting lock state fails
        /// </exception>
		private void LockPatchStore(IAdoMigrationContext context)
		{
			// Patch locks ensure that only one system sharing a patch store will patch
			// it at the same time.
			bool lockObtained = false;

			while (!lockObtained)
			{
				WaitForFreeLock(context);
				
                IPatchInfoStore piStore = contexts[context];
                int patchLevel = piStore.PatchLevel;

				try
				{
					piStore.LockPatchStore();
					lockObtained = true;
				}
				catch (Exception e)
				{
					// this happens when someone woke up at the same time,
					// raced us to the lock and won. We re-sleep and try again.
				}
			}
		}
		
		/// <summary>
        /// Pauses until the patch lock become available.
		/// </summary>
		/// <param name="context">
        /// the context related to the store
		/// </param>
		/// <exception cref="MigrationException">
        /// if an unrecoverable error occurs
        /// </exception>
		private void WaitForFreeLock(IAdoMigrationContext context)
		{
			IPatchInfoStore piStore = contexts[context];

			while (piStore.PatchStoreLocked)
			{
				log.Info("Waiting for migration lock for system \"" + context.SystemName + "\"");

				try
				{
					Thread.Sleep(new TimeSpan(TimeSpan.TicksPerMillisecond * LockPollMillis));
				}
				catch (ThreadInterruptedException e)
				{
					log.Error("Received ThreadInterruptedException while waiting for patch lock", e);
				}
			}
        }
        #endregion
    }
}

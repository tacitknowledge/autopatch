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
using System.Data.Common;
using log4net;
using log4net.Config;
using com.tacitknowledge.util.migration;

#endregion
namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Core starting point for a database migration run.  This class obtains a connection
	/// to the database, checks its patch level, delegates the actual execution of the migration
	/// tasks to a <code>MigrationProcess</code> instance, and then commits and cleans everything
	/// up at the end.
	/// <p>
	/// This class is <b>NOT</b> threadsafe.
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	public class ADOMigrationLauncher : MigrationListener
    {

        #region Members
        /// <summary> Class logger</summary>
        //UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.ADOMigrationLauncher'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
        private static ILog log;

        /// <summary>
        /// The name of the system we're updating (Multi-node)
        /// </summary>
        private String systemName = null;

        /// <summary> The patch level store in use</summary>
        private IPatchInfoStore patchTable = null;

        /// <summary> The <code>MigrationProcess</code> responsible for applying the patches</summary>
        private MigrationProcess migrationProcess = null;

        /// <summary> The amount time, in milliseconds, between attempts to obtain a lock on the
        /// patches table.  Defaults to 15 seconds.
        /// </summary>
        private long lockPollMillis = 15000;

        /// <summary> The path containing directories and packages to search through to locate patches.</summary>
        private System.String patchPath = null;

        /// <summary> The path containing directories and packages to search through to locate post-patch tasks.</summary>
        private System.String postPatchPath = null;

        /// <summary> The <code>IMigrationContext</code> to use for all migrations.</summary>
        private ADOMigrationContext context = null;
		
        #endregion 

        # region Methods

        /// <summary>
        /// The name of the system we're updating
        /// </summary>
        public String SystemName
        {
            get { return systemName; }
            set { systemName = value; }
        }

        /// <summary> Get the MigrationProcess we'll use to Migrate things
		/// 
		/// </summary>
		/// <returns> MigrationProcess for migration control
		/// </returns>
		virtual public MigrationProcess NewMigrationProcess
		{
			get
			{
				return new MigrationProcess();
			}
			
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <summary> Returns the colon-separated path of packages and directories within the
		/// class path that are sources of patches.
		/// 
		/// </summary>
		/// <returns> a colon-separated path of packages and directories within the
		/// class path that are sources of patches
		/// </returns>
		/// <summary> Sets the colon-separated path of packages and directories within the
		/// class path that are sources of patches.
		/// 
		/// </summary>
		/// <param name="searchPath">a colon-separated path of packages and directories within the
		/// class path that are sources of patches
		/// </param>
		virtual public System.String PatchPath
		{
			get
			{
				return patchPath;
			}
			
			set
			{
				this.patchPath = value;
				SupportClass.Tokenizer st = new SupportClass.Tokenizer(value, ":");
				while (st.HasMoreTokens())
				{
					System.String path = st.NextToken();
					if (path.IndexOf('/') > - 1)
					{
						migrationProcess.addPatchResourceDirectory(path);
					}
					else
					{
						migrationProcess.addPatchResourcePackage(path);
					}
				}
			}
			
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <summary> Returns the colon-separated path of packages and directories within the
		/// class path that are sources of post-patch tasks
		/// 
		/// </summary>
		/// <returns> a colon-separated path of packages and directories within the
		/// class path that are sources of post-patch tasks
		/// </returns>
		/// <summary> Sets the colon-separated path of packages and directories within the
		/// class path that are sources of post-patch tasks
		/// 
		/// </summary>
		/// <param name="searchPath">a colon-separated path of packages and directories within the
		/// class path that are sources of post-patch tasks
		/// </param>
		virtual public System.String PostPatchPath
		{
			get
			{
				return postPatchPath;
			}
			
			set
			{
				this.postPatchPath = value;
				if (value == null)
				{
					return ;
				}
				SupportClass.Tokenizer st = new SupportClass.Tokenizer(value, ":");
				while (st.HasMoreTokens())
				{
					System.String path = st.NextToken();
					if (path.IndexOf('/') > - 1)
					{
						migrationProcess.addPostPatchResourceDirectory(path);
					}
					else
					{
						migrationProcess.addPostPatchResourcePackage(path);
					}
				}
			}
			
		}
		/// <summary> Get the patch level from the database
		/// 
		/// </summary>
		/// <returns> int representing the current database patch level
		/// </returns>
		/// <exception cref="MigrationException">if there is a database connection error,
		/// or the patch level can't be determined
		/// </exception>
		virtual public int DatabasePatchLevel
		{
			get
			{
				return patchTable.PatchLevel;
			}
			
		}
		/// <summary> Get the next patch level, for use when creating a new patch
		/// 
		/// </summary>
		/// <returns> int representing the first unused patch number
		/// </returns>
		/// <exception cref="MigrationException">if the next patch level can't be determined
		/// </exception>
		virtual public int NextPatchLevel
		{
			get
			{
				return migrationProcess.NextPatchLevel;
			}
			
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <summary> Returns the <code>ADOMigrationContext</code> used for the migrations.
		/// 
		/// </summary>
		/// <returns> the <code>ADOMigrationContext</code> used for the migrations
		/// </returns>
		/// <summary> Sets the <code>ADOMigrationContext</code> used for the migrations.
		/// 
		/// </summary>
		/// <param name="ADOMigrationContext">the <code>ADOMigrationContext</code> used for the migrations
		/// </param>
		/// <throws>  MigrationException if a database connection cannot be obtained </throws>
		virtual public ADOMigrationContext Context
		{
			get
			{
				return context;
			}
			
			set
			{
				this.context = value;
				try
				{
					patchTable = new PatchTable(context, context.Connection);
				}
				catch (System.Data.OleDb.OleDbException e)
				{
					throw new MigrationException("Could not obtain ADO Connection", e);
				}
			}
			
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <summary> Get how long to wait for the patch store lock
		/// 
		/// </summary>
		/// <returns> the wait time for the patch store, in milliseconds
		/// </returns>
		/// <summary> Set how long to wait for the patch store lock
		/// 
		/// </summary>
		/// <param name="lockPollMillis">the wait time for the patch store, in milliseconds
		/// </param>
		virtual public long LockPollMillis
		{
			get
			{
				return lockPollMillis;
			}
			
			set
			{
				this.lockPollMillis = value;
			}
			
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <summary> Get the migration process to use for migrations
		/// 
		/// </summary>
		/// <returns> MigrationProcess to use for migrations
		/// </returns>
		/// <summary> Set the migration process to use for migrations
		/// 
		/// </summary>
		/// <param name="migrationProcess">the MigrationProcess to use for migrations
		/// </param>
		virtual public MigrationProcess MigrationProcess
		{
			get
			{
				return migrationProcess;
			}
			
			set
			{
				this.migrationProcess = value;
			}
			
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <summary> Get the patch table we use to store migration information
		/// 
		/// </summary>
		/// <returns> PatchStore with information about our migration state
		/// </returns>
		/// <summary> Set the patch table where we should put information about the migrationo
		/// 
		/// </summary>
		/// <param name="patchTable">where we should put information about the migration
		/// </param>
		virtual public IPatchInfoStore PatchTable
		{
			get
			{
				return patchTable;
			}
			
			set
			{
				this.patchTable = value;
			}
			
		}
		
		/// <summary> Create a new MigrationProcess and add a SqlScriptMigrationTaskSource</summary>
		public ADOMigrationLauncher()
		{
			MigrationProcess = NewMigrationProcess;
			
			// Make sure this class is notified when a patch is applied so that
			// the patch level can be updated (see #migrationSuccessful).
			migrationProcess.addListener(this);
			
			MigrationProcess.addMigrationTaskSource(new SqlScriptMigrationTaskSource());
		}
		
		/// <summary> Create a new <code>MigrationLancher</code>.
		/// 
		/// </summary>
		/// <param name="context">the <code>ADOMigrationContext</code> to use.
		/// </param>
		/// <throws>  MigrationException if an unexpected error occurs </throws>
		public ADOMigrationLauncher(ADOMigrationContext context):this()
		{
			Context = context;
		}
		
		/// <summary> Starts the application migration process.
		/// 
		/// </summary>
		/// <returns> the number of patches applied
		/// </returns>
		/// <throws>  MigrationException if an unrecoverable error occurs during </throws>
		/// <summary>         the migration
		/// </summary>
		public virtual int doMigrations()
		{
			if (context == null)
			{
				throw new MigrationException("You must configure a migration context");
			}
			
			//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
			System.Data.OleDb.OleDbConnection conn = null;
			try
			{
				conn = context.Connection;
				return doMigrations(conn);
			}
			catch (System.Data.OleDb.OleDbException e)
			{
				throw new MigrationException("SqlException during migration", e);
			}
			finally
			{
				SqlUtil.close(conn, null, null);
			}
		}
		
		/// <seealso cref="MigrationListener.migrationStarted(IMigrationTask, IMigrationContext)">
		/// </seealso>
		public virtual void  migrationStarted(IMigrationTask task, IMigrationContext ctx)
		{
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
			log.Debug("Started task " + task.Name + " for context " + ctx);
		}
		
		/// <seealso cref="MigrationListener.migrationSuccessful(IMigrationTask, IMigrationContext)">
		/// </seealso>
		public virtual void  migrationSuccessful(IMigrationTask task, IMigrationContext ctx)
		{
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
			log.Debug("Task " + task.Name + " was successful for context " + ctx);
			int patchLevel = task.Level;
			patchTable.UpdatePatchLevel(patchLevel);
		}
		
		/// <seealso cref="MigrationListener.migrationFailed(IMigrationTask, IMigrationContext, MigrationException)">
		/// </seealso>
		public virtual void  migrationFailed(IMigrationTask task, IMigrationContext ctx, MigrationException e)
		{
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
			log.Debug("Task " + task.Name + " failed for context " + ctx, e);
		}
		
		/// <summary> Performs the application migration process in one go
		/// 
		/// </summary>
		/// <param name="conn">the connection to use
		/// </param>
		/// <returns> the number of patches applied
		/// </returns>
		/// <throws>  SQLException if an unrecoverable database error occurs while </throws>
		/// <summary>         working with the patches table.
		/// </summary>
		/// <throws>  MigrationException if an unrecoverable error occurs during </throws>
		/// <summary>         the migration
		/// </summary>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
		protected internal virtual int doMigrations(DbConnection conn)
		{
			patchTable = createPatchStore(conn);
			
			// Save auto-commit state
			bool b = true;
			try
			{
				lockPatchStore();
				
				// make sure we can at least attempt to roll back patches
				// DDL usually can't rollback - we'd need compensating transactions
				b = SupportClass.TransactionManager.manager.GetAutoCommit(conn);
				SupportClass.TransactionManager.manager.SetAutoCommit(conn, false);
				
				// Now apply the patches
				int executedPatchCount = 0;
				try
				{
					int patchLevel = patchTable.PatchLevel;
					executedPatchCount = migrationProcess.doMigrations(patchLevel, context);
				}
				catch (MigrationException me)
				{
					// If there was any kind of error, we don't want to eat it, but we do
					// want to unlock the patch store. So do that, then re-throw.
					patchTable.UnlockPatchStore();
					throw me;
				}
				finally
				{
					try
					{
						SupportClass.TransactionManager.manager.Commit(conn);
					}
					catch (System.Data.OleDb.OleDbException e)
					{
						log.Error("Error unlocking patch table: ", e);
					}
				}
				
				// Do any post-patch tasks
				try
				{
					migrationProcess.doPostPatchMigrations(context);
					return executedPatchCount;
				}
				finally
				{
					try
					{
						patchTable.UnlockPatchStore();
						SupportClass.TransactionManager.manager.Commit(conn);
					}
					catch (System.Data.OleDb.OleDbException e)
					{
						log.Error("Error unlocking patch table: ", e);
					}
				}
			}
			finally
			{
				// restore auto-commit setting
				SupportClass.TransactionManager.manager.SetAutoCommit(conn, b);
			}
		}
		
		/// <summary> Lock the patch store. This is done safely, such that we safely handle the case where 
		/// other migration launchers are patching at the same time.
		/// 
		/// </summary>
		/// <throws>  MigrationException if the reading or setting lock state fails </throws>
		private void  lockPatchStore()
		{
			// Patch locks ensure that only one system sharing a patch store will patch
			// it at the same time.
			bool lockObtained = false;
			while (!lockObtained)
			{
				waitForFreeLock();
				
				try
				{
					patchTable.LockPatchStore();
					lockObtained = true;
				}
				catch (System.SystemException ise)
				{
					// this happens when someone woke up at the same time,
					// raced us to the lock and won. We re-sleep and try again.
				}
			}
		}
		
		/// <summary> create a patch table object for use in migrations
		/// 
		/// </summary>
		/// <param name="conn">the database connection to use for table access
		/// </param>
		/// <returns> PatchTable object for use in accessing patch state information
		/// </returns>
		/// <throws>  MigrationException if unable to create the store </throws>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
		protected internal virtual IPatchInfoStore createPatchStore(System.Data.OleDb.OleDbConnection conn)
		{
			IPatchInfoStore piStore = new PatchTable(context, conn);
			
			// Make sure the table is created before claiming it exists by returning
			int generatedAux = patchTable.PatchLevel;
			try
			{
				if (!SupportClass.TransactionManager.manager.GetAutoCommit(conn))
				{
					SupportClass.TransactionManager.manager.Commit(conn);
				}
			}
			catch (System.Data.OleDb.OleDbException sqle)
			{
				throw new MigrationException("Unable to commit connection after creating patch store");
			}
			
			return piStore;
		}
		
		/// <summary> Pauses until the patch lock become available.
		/// 
		/// </summary>
		/// <throws>  MigrationException if an unrecoverable error occurs </throws>
		private void  waitForFreeLock()
		{
			while (patchTable.PatchStoreLocked)
			{
				log.Info("Waiting for migration lock for system \"" + context.getSystemName() + "\"");
				try
				{
					//UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
					System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64) 10000 * LockPollMillis));
				}
				catch (System.Threading.ThreadInterruptedException e)
				{
					log.Error("Received InterruptedException while waiting for patch lock", e);
				}
			}
		}
		static ADOMigrationLauncher()
		{
			log = LogManager.GetLogger(typeof(ADOMigrationLauncher));
        }
        #endregion
    }
}
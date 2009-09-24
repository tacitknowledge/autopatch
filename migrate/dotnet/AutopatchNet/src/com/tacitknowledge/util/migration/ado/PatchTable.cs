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
using log4net;
using log4net.Config;
using com.tacitknowledge.util.migration;
using System.Data;
using System.Data.Common;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
	
	
	/// <summary> Manages interactions with the "patches" table.  The patches table stores
	/// the current patch level for a given system, as well as a system-scoped lock
	/// use to avoid concurrent patches to the system.  A system is defined as an
	/// exclusive target of a patch.
	/// <p>
	/// This class is responsible for:
	/// <ul>
	/// <li>Validating the existence of the patches table and creating it if it
	/// doesn't exist</li>
	/// <li>Determining if a patch is currently running on a given system</li>
	/// <li>Obtaining and releasing patch locks for a given system</li>
	/// <li>Obtaining and incrementing the patch level for a given system</li>
	/// </ul>
	/// <p>
	/// <strong>TRANSACTIONS:</strong> Transactions should be committed by the calling
	/// class as needed.  This class does not explictly commit or rollback transactions.
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	public class PatchTable : IPatchInfoStore
	{
		/// <summary> {@inheritDoc}</summary>
        virtual public int PatchLevel
        {
            get
            {
                CreatePatchStoreIfNeeded();

                DbCommand stmt = null;
                DbDataReader rs = null;
                try
                {
                    stmt = context.Database.GetSqlStringCommand(getSql("readLevel"));
                    stmt.Connection = context.Connection;
                    stmt.Transaction = context.Transaction;
                    context.Database.AddInParameter(stmt, "@SystemName", DbType.AnsiString, context.SystemName);

                    rs = stmt.ExecuteReader();
                    if (rs.Read())
                    {
                        return rs.GetInt32(1 - 1);
                    }
                    else
                    {
                        // We don't yet have a patch record for this system; create one
                        createSystemPatchRecord();
                        return 0;
                    }
                }
                catch (DbException e)
                {
                    throw new MigrationException("Unable to get patch level", e);
                }
                finally
                {
                    if (rs != null)
                    {
                        rs.Close();
                        rs.Dispose();
                    }
                    if (stmt != null)
                    {
                        stmt.Dispose();
                    }
                }
            }
        }

        /// <summary> {@inheritDoc}</summary>
		virtual public bool PatchStoreLocked
		{
			get
			{
				CreatePatchStoreIfNeeded();

                DbCommand stmt = null;
                DbDataReader rs = null;
                try
				{
                    stmt = context.Database.GetSqlStringCommand(getSql("readLock"));
                    stmt.Connection = context.Connection;
                    stmt.Transaction = context.Transaction;
                    context.Database.AddInParameter(stmt, "@SystemName", DbType.AnsiString, context.SystemName);
					rs = stmt.ExecuteReader();
					
					if (rs.Read())
					{
						return ("T".Equals(System.Convert.ToString(rs[1 - 1])));
					}
					else
					{
						return false;
					}
				}
				catch (DbException e)
				{
					throw new MigrationException("Unable to determine if table is locked", e);
				}
				finally
				{
                    if (rs != null)
                    {
                        rs.Close();
                        rs.Dispose();
                    }
                    if (stmt != null)
                    {
                        stmt.Dispose();
                    }
                }
			}
		}
        
		/// <summary> Class logger</summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.PatchTable'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
		
		/// <summary> The migration configuration </summary>
		private IAdoMigrationContext context = null;
		
		/// <summary> The database connection </summary>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
        //private System.Data.Common.DbConnection conn = null;
		
		/// <summary> Keeps track of table validation (see #createPatchesTableIfNeeded)</summary>
		private bool tableExistenceValidated = false;
		
		/// <summary> Create a new <code>PatchTable</code>.
		/// 
		/// </summary>
		/// <param name="migrationContext">the migration configuration and connection source
		/// </param>
		/// <param name="connection">the database connection to use; this will NOT be closed
		/// </param>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
        public PatchTable(IAdoMigrationContext migrationContext)
		{
			this.context = migrationContext;
			
			if (context.DatabaseType == null)
			{
				throw new System.ArgumentException("The ADO.NET database type is required");
			}
		}
		
		/// <summary> {@inheritDoc}</summary>
		public virtual void  CreatePatchStoreIfNeeded()
		{
			if (tableExistenceValidated)
			{
				return ;
			}

            DbCommand stmt = null;
            DbDataReader rs = null;
            try
			{
				// TODO: Find a better, cross-platform way to determine if a table exists.
				//       Simply expecting a SQLException is kind of a hack
                stmt = context.Database.GetSqlStringCommand(getSql("readLevel"));
                stmt.Connection = context.Connection;
                stmt.Transaction = context.Transaction;
                context.Database.AddInParameter(stmt, "@SystemName", DbType.AnsiString, context.SystemName);
				rs = stmt.ExecuteReader();
				log.Debug("'patches' table already exists.");
				tableExistenceValidated = true;
			}
			catch (DbException)
			{
				log.Info("'patches' table must not exist; creating....");
				try
				{
					if (log.IsDebugEnabled)
					{
                        log.Debug("Creating patches table with SQL '" + getSql("createPatches") + "'");
					}
                    context.Database.ExecuteNonQuery(CommandType.Text, getSql("createPatches"));
				}
				catch (DbException sqle)
				{
					throw new MigrationException("Unable to create patch table", sqle);
				}
				tableExistenceValidated = true;
				log.Info("Created 'patches' table.");
			}
			finally
			{
                if (rs != null)
                {
                    rs.Close();
                    rs.Dispose();
                }
                if (stmt != null)
                {
                    stmt.Dispose();
                }
			}
		}
		
		/// <summary> {@inheritDoc}</summary>
        public virtual void UpdatePatchLevel(int level)
        {
            // Make sure a patch record already exists for this system
            int generatedAux = PatchLevel;

            DbCommand stmt = null;
            try
            {
                stmt = context.Database.GetSqlStringCommand(getSql("updateLevel"));
                stmt.Connection = context.Connection;
                stmt.Transaction = context.Transaction;
                context.Database.AddInParameter(stmt, "@PatchLevel", DbType.Int32, level);
                context.Database.AddInParameter(stmt, "@SystemName", DbType.AnsiString, context.SystemName);
                stmt.ExecuteNonQuery();
            }
            catch (DbException e)
            {
                throw new MigrationException("Unable to update patch level", e);
            }
            finally
            {
                if (stmt != null)
                {
                    stmt.Dispose();
                }
            }
        }
		
		/// <summary> {@inheritDoc}</summary>
		public virtual void  LockPatchStore()
		{
			if (PatchStoreLocked)
			{
				throw new System.SystemException("Patch table is already locked!");
			}
			updatePatchLock(true);
		}
		
		/// <summary> {@inheritDoc}</summary>
		public virtual void  UnlockPatchStore()
		{
			updatePatchLock(false);
		}
		
		/// <summary> Returns the SQL to execute for the database type associated with this patch table.
		/// 
		/// </summary>
		/// <param name="key">the key within <code><i>database</i>.properties</code> whose
		/// SQL should be returned
		/// </param>
		/// <returns> the SQL to execute for the database type associated with this patch table
		/// </returns>
		protected internal virtual System.String getSql(System.String key)
		{
			return context.DatabaseType.getProperty(key);
		}
		
		/// <summary> Creates an initial record in the patches table for this system. 
		/// 
		/// </summary>
		/// <throws>  SQLException if an unrecoverable database error occurs </throws>
		private void  createSystemPatchRecord()
		{
			System.String systemName = context.SystemName;
			DbCommand stmt = null;
			try
			{
                stmt = context.Database.GetSqlStringCommand(getSql("createLevel"));
                context.Database.AddInParameter(stmt, "@SystemName", DbType.AnsiString, systemName);
                context.Database.ExecuteNonQuery(stmt);
				log.Info("Created patch record for " + systemName);
			}
			catch (DbException e)
			{
				log.Error("Error creating patch record for system '" + systemName + "'", e);
				throw e;
			}
			finally
			{
                if (stmt != null)
                {
                    stmt.Dispose();
                }
            }
		}
		
		/// <summary> Obtains or releases a lock for this system in the patches table. 
		/// 
		/// </summary>
		/// <param name="lock"><code>true</code> if a lock is to be obtained, <code>false</code>
		/// if it is to be removed 
		/// </param>
		/// <throws>  MigrationException if an unrecoverable database error occurs </throws>
		private void  updatePatchLock(bool lock_Renamed)
		{
            System.String sqlkey = (lock_Renamed) ? "obtainLock" : "releaseLock";
			DbCommand stmt = null;
			
			try
			{
                stmt = context.Database.GetSqlStringCommand(getSql(sqlkey));
                context.Database.AddInParameter(stmt, "@SystemName", DbType.AnsiString, context.SystemName);

                if (log.IsDebugEnabled)
				{
					log.Debug("Updating patch table lock: " + getSql(sqlkey));
				}
                context.Database.ExecuteNonQuery(stmt);
			}
			catch (DbException e)
			{
				throw new MigrationException("Unable to update patch lock to " + lock_Renamed, e);
			}
			finally
			{
                if (stmt != null)
                {
                    stmt.Dispose();
                }
            }
		}
		static PatchTable()
		{
			log = LogManager.GetLogger(typeof(PatchTable));
		}
	}
}
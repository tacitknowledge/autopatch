/* Copyright 2005 Tacit Knowledge LLC
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
				
				System.Data.OleDb.OleDbCommand stmt = null;
				//UPGRADE_TODO: Interface 'java.sql.ResultSet' was converted to 'System.Data.OleDb.OleDbDataReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javasqlResultSet'"
				System.Data.OleDb.OleDbDataReader rs = null;
				try
				{
					stmt = SupportClass.TransactionManager.manager.PrepareStatement(conn, getSql("level.read"));
					SupportClass.TransactionManager.manager.SetValue(stmt, 1, context.getSystemName());
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
				catch (System.Data.OleDb.OleDbException e)
				{
					throw new MigrationException("Unable to get patch level", e);
				}
				finally
				{
					SqlUtil.close(null, stmt, rs);
				}
			}
			
		}
		/// <summary> {@inheritDoc}</summary>
		virtual public bool PatchStoreLocked
		{
			get
			{
				CreatePatchStoreIfNeeded();
				
				System.Data.OleDb.OleDbCommand stmt = null;
				//UPGRADE_TODO: Interface 'java.sql.ResultSet' was converted to 'System.Data.OleDb.OleDbDataReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javasqlResultSet'"
				System.Data.OleDb.OleDbDataReader rs = null;
				try
				{
					stmt = SupportClass.TransactionManager.manager.PrepareStatement(conn, getSql("lock.read"));
					SupportClass.TransactionManager.manager.SetValue(stmt, 1, context.getSystemName());
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
				catch (System.Data.OleDb.OleDbException e)
				{
					throw new MigrationException("Unable to determine if table is locked", e);
				}
				finally
				{
					SqlUtil.close(null, stmt, rs);
				}
			}
			
		}
		/// <summary> Class logger</summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.PatchTable'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
		
		/// <summary> The migration configuration </summary>
		private ADOMigrationContext context = null;
		
		/// <summary> The database connection </summary>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
		private System.Data.OleDb.OleDbConnection conn = null;
		
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
		public PatchTable(ADOMigrationContext migrationContext, System.Data.OleDb.OleDbConnection connection)
		{
			this.context = migrationContext;
			this.conn = connection;
			
			if (context.getDatabaseType() == null)
			{
				throw new System.ArgumentException("The ADO database type is required");
			}
		}
		
		/// <summary> {@inheritDoc}</summary>
		public virtual void  CreatePatchStoreIfNeeded()
		{
			if (tableExistenceValidated)
			{
				return ;
			}
			
			System.Data.OleDb.OleDbCommand stmt = null;
			//UPGRADE_TODO: Interface 'java.sql.ResultSet' was converted to 'System.Data.OleDb.OleDbDataReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javasqlResultSet'"
			System.Data.OleDb.OleDbDataReader rs = null;
			try
			{
				// TODO: Find a better, cross-platform way to determine if a table exists.
				//       Simply expecting a SQLException is kind of a hack
				stmt = SupportClass.TransactionManager.manager.PrepareStatement(conn, getSql("level.read"));
				SupportClass.TransactionManager.manager.SetValue(stmt, 1, context.getSystemName());
				rs = stmt.ExecuteReader();
				log.Debug("'patches' table already exists.");
				tableExistenceValidated = true;
			}
			catch (System.Data.OleDb.OleDbException e)
			{
				SqlUtil.close(null, stmt, rs);
				log.Info("'patches' table must not exist; creating....");
				try
				{
					stmt = SupportClass.TransactionManager.manager.PrepareStatement(conn, getSql("patches.create"));
					if (log.IsDebugEnabled())
					{
						log.Debug("Creating patches table with SQL '" + getSql("patches.create") + "'");
					}
					stmt.ExecuteNonQuery();
				}
				catch (System.Data.OleDb.OleDbException sqle)
				{
					throw new MigrationException("Unable to create patch table", sqle);
				}
				tableExistenceValidated = true;
				log.Info("Created 'patches' table.");
			}
			finally
			{
				SqlUtil.close(null, stmt, rs);
			}
		}
		
		/// <summary> {@inheritDoc}</summary>
		public virtual void  UpdatePatchLevel(int level)
		{
			// Make sure a patch record already exists for this system
			int generatedAux = PatchLevel;
			
			System.Data.OleDb.OleDbCommand stmt = null;
			try
			{
				stmt = SupportClass.TransactionManager.manager.PrepareStatement(conn, getSql("level.update"));
				SupportClass.TransactionManager.manager.SetValue(stmt, 1, level);
				SupportClass.TransactionManager.manager.SetValue(stmt, 2, context.getSystemName());
				stmt.ExecuteNonQuery();
			}
			catch (System.Data.OleDb.OleDbException e)
			{
				throw new MigrationException("Unable to update patch level", e);
			}
			finally
			{
				SqlUtil.close(null, stmt, null);
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
			return context.getDatabaseType().getProperty(key);
		}
		
		/// <summary> Creates an initial record in the patches table for this system. 
		/// 
		/// </summary>
		/// <throws>  SQLException if an unrecoverable database error occurs </throws>
		private void  createSystemPatchRecord()
		{
			System.String systemName = context.getSystemName();
			System.Data.OleDb.OleDbCommand stmt = null;
			try
			{
				stmt = SupportClass.TransactionManager.manager.PrepareStatement(conn, getSql("level.create"));
				SupportClass.TransactionManager.manager.SetValue(stmt, 1, systemName);
				stmt.ExecuteNonQuery();
				log.Info("Created patch record for " + systemName);
			}
			catch (System.Data.OleDb.OleDbException e)
			{
				log.Error("Error creating patch record for system '" + systemName + "'", e);
				throw e;
			}
			finally
			{
				SqlUtil.close(null, stmt, null);
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
			System.String sqlkey = (lock_Renamed)?"lock.obtain":"lock.release";
			System.Data.OleDb.OleDbCommand stmt = null;
			
			try
			{
				stmt = SupportClass.TransactionManager.manager.PrepareStatement(conn, getSql(sqlkey));
				if (log.IsDebugEnabled())
				{
					log.Debug("Updating patch table lock: " + getSql(sqlkey));
				}
				SupportClass.TransactionManager.manager.SetValue(stmt, 1, context.getSystemName());
				stmt.ExecuteNonQuery();
			}
			catch (System.Data.OleDb.OleDbException e)
			{
				throw new MigrationException("Unable to update patch lock to " + lock_Renamed, e);
			}
			finally
			{
				SqlUtil.close(null, stmt, null);
			}
		}
		static PatchTable()
		{
			log = LogManager.GetLogger(typeof(PatchTable));
		}
	}
}
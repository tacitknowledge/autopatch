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
using AutopatchNET.src.com.tacitknowledge.util.migration;
using MigrationException = com.tacitknowledge.util.migration.MigrationException;
#endregion
namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Provides ADO resources to migration tasks.
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	public class DataSourceMigrationContext : ADOMigrationContext
    {

        #region Members
        /// <summary> The database connection to use</summary>
        //UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
        private System.Data.Common.DbConnection connection = null;

        /// <summary> The DataSource to use</summary>
        //UPGRADE_ISSUE: Interface 'javax.sql.DataSource' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxsqlDataSource'"
        private MigrationDataSource dataSource = null;

        /// <summary> The name of the system being patched</summary>
        private String systemName = null;

        /// <summary> The name of the system being patched</summary>
        private DatabaseType databaseType = null;

        #endregion

        #region Methods
        /// <summary> Returns the database connection to use
		/// 
		/// </summary>
		/// <returns> the database connection to use
		/// </returns>
		/// <throws>  SQLException if an unexpected error occurs </throws>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
		public System.Data.Common.DbConnection Connection
		{
			get
			{
				if ((connection == null) || (connection.State == System.Data.ConnectionState.Closed))
				{
					
					/*
                     * Obtain a connection object for talking to the Data store
                     */
 
				}
				return connection;
			}
			
		}
		
		
		/// <seealso cref="ADOMigrationContext.commit()">
		/// </seealso>
		public virtual void  commit()
		{
			try
			{
				SupportClass.TransactionManager.manager.Commit(Connection);
			}
			catch (System.Data.OleDb.OleDbException e)
			{
				throw new MigrationException("Error committing SQL transaction", e);
			}
		}
		
		/// <seealso cref="ADOMigrationContext.rollback()">
		/// </seealso>
		public virtual void  rollback()
		{
			try
			{
				SupportClass.TransactionManager.manager.RollBack(Connection);
			}
			catch (System.Data.OleDb.OleDbException e)
			{
				throw new MigrationException("Could not rollback SQL transaction", e);
			}
		}
		
		/// <summary> Returns the type of database being patched.
		/// 
		/// </summary>
		/// <returns> the type of database being patched
		/// </returns>
		public virtual DatabaseType getDatabaseType()
		{
			return databaseType;
		}
		
		/// <summary> Returns the type of database being patched.
		/// 
		/// </summary>
		/// <param name="type">the type of database being patched
		/// </param>
		public virtual void  setDatabaseType(DatabaseType type)
		{
			this.databaseType = type;
		}
		
		/// <returns> Returns the systemName.
		/// </returns>
		public virtual System.String getSystemName()
		{
			return systemName;
		}
		
		/// <summary> Sets the system name.
		/// 
		/// </summary>
		/// <param name="name">the name of the system to patch
		/// </param>
		public virtual void  setSystemName(System.String name)
		{
			if (name == null)
			{
				throw new System.ArgumentException("systemName cannot be null");
			}
			if (name.Length > com.tacitknowledge.util.migration.ado.ADOMigrationContext_Fields.MAX_SYSTEMNAME_LENGTH)
			{
				throw new System.ArgumentException("systemName cannot be longer than " + com.tacitknowledge.util.migration.ado.ADOMigrationContext_Fields.MAX_SYSTEMNAME_LENGTH + " characters");
			}
			this.systemName = name;
        }
        #endregion
    }
}
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
using System.Data;
using System.Data.Common;
using com.tacitknowledge.util.migration;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
	/// <summary>
    /// Provides ADO.NET resources to migration tasks.
	/// </summary>
    /// <author>Scott Askew (scott@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public class DataSourceMigrationContext : IAdoMigrationContext
    {
        #region Member variables
        private static readonly int MAX_SYSTEMNAME_LENGTH = 30;

        /// <summary>
        /// Will provide the <code>DbConnection</code> required to talk to the data store.
        /// </summary>
        private MigrationDataSource dataSource = null;

        /// <summary>
        /// The database connection to use.
        /// </summary>
        private DbConnection connection = null;

        /// <summary>
        /// The name of the system to patch.
        /// </summary>
        private String systemName = null;

        /// <summary>
        /// The database type.
        /// </summary>
        private DatabaseType databaseType = null;
        #endregion

        #region Public properties
        /// <seealso cref="IAdoMigrationContext.Connection"/>
		public DbConnection Connection
		{
			get
			{
				if ((connection == null) || (connection.State == ConnectionState.Closed))
				{
					// Obtain a connection object for talking to the Data store
                    dataSource = new MigrationDataSource();
                    connection = dataSource.Connection;

                    return connection;
				}

				return connection;
			}
		}

        /// <seealso cref="IAdoMigrationContext.DatabaseType"/>
        public virtual DatabaseType DatabaseType
        {
            get { return databaseType;}
            set { databaseType = value; }
        }

        /// <seealso cref="IAdoMigrationContext.SystemName"/>
        public virtual String SystemName
        {
            get
            {
                return systemName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("systemName cannot be null");
                }

                if (value.Length > MAX_SYSTEMNAME_LENGTH)
                {
                    throw new ArgumentException("systemName cannot be longer than " + MAX_SYSTEMNAME_LENGTH + " characters");
                }

                systemName = value;
            }
        }
        #endregion

        #region Public methods
        /// <seealso cref="IMigrationContext.Commit()"/>
		public virtual void Commit()
		{
			try
			{
                // TODO SupportClass.TransactionManager.manager.Commit(Connection);
			}
			catch (DbException e)
			{
				throw new MigrationException("Error committing SQL transaction", e);
			}
		}

        /// <seealso cref="IMigrationContext.Rollback()"/>
		public virtual void Rollback()
		{
			try
			{
                // TODO SupportClass.TransactionManager.manager.RollBack(Connection);
			}
			catch (DbException e)
			{
				throw new MigrationException("Could not rollback SQL transaction", e);
			}
		}
        #endregion
    }
}

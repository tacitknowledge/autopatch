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
using Microsoft.Practices.EnterpriseLibrary.Data;
using com.tacitknowledge.util.migration;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
	/// <summary>
    /// <para>
    /// Provides ADO.NET resources to migration tasks. The interaction with the data source happens
    /// through <code>Database</code> object which has associated <code>DbConnection</code>
    /// and <code>DbTransaction</code> objects. Both of these objects are created/opened pon first use.
    /// The connection must be closed either explicitly or through the <code>Dispose()</code> method.
    /// </para>
    /// <para>
    /// Because of the reason that ADO.NET transactions are not reusable we need to manage open connections
    /// and their transactions inside this class. An object of this class always has an open connection with
    /// an active transaction available (or they will be created on demand on first access). When current
    /// transaction either commits or rolls back, the open connection initiates another transactions to
    /// keep servicing requests. The connection automatically closes and rolls back any pending transaction
    /// when the class' <code>Dispose()</code> method is called.
    /// <remarks>
    /// It is extremely important to call this class' <code>Dispose()</code> method when you are done
    /// working with an instance of it, or close the resources explicitly by calling
    /// <code>CloseConnection()</code>.
    /// </remarks>
    /// Because of this non-reusability nature of transaction objects the developer is advised to never
    /// the reference to one in his code, but rather access the property directly when there's a need to
    /// get the currently active transaction object.
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// using (DataSourceMigrationContext context = new DataSourceMigrationContext())
    /// {
    ///     Database db = context.Database;
    /// 
    ///     try
    ///     {
    ///         using (DbCommand dbCmd = db.GetSqlStringCommand("UPDATE table1 SET field1 = 'qwerty'"))
    ///         {
    ///             db.ExecuteNonQuery(dbCmd, context.Transaction);
    ///         }
    ///         context.Commit();
    ///     }
    ///     catch (Exception)
    ///     {
    ///         context.Rollback();
    ///     }
    /// }
    /// </code>
    /// </para>
	/// </summary>
    /// <author>Scott Askew (scott@tacitknowledge.com)</author>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public class DataSourceMigrationContext : IAdoMigrationContext, IDisposable
    {
        #region Member variables
        private static readonly int MAX_SYSTEMNAME_LENGTH = 30;
        // TODO Change the code to use non-default databases (for example coming from configuration manager)
        private readonly Database database = DatabaseFactory.CreateDatabase();
        private DbConnection connection = null;
        private DbTransaction transaction = null;
        private String systemName = null;
        private DatabaseType databaseType = null;
        #endregion

        #region Public properties
        /// <seealso cref="IAdoMigrationContext.Database"/>
        public virtual Database Database
        {
            get { return database; }
        }

        /// <seealso cref="IAdoMigrationContext.Connection"/>
        public virtual DbConnection Connection
		{
			get
			{
				if (connection == null || connection.State == ConnectionState.Closed)
				{
                    // Get the connection
                    connection = database.CreateConnection();
                    // Open it
                    connection.Open();
                    // Create the accompanying transaction
                    transaction = connection.BeginTransaction();
				}

				return connection;
			}
		}

        /// <seealso cref="IAdoMigrationContext.Transaction"/>
        public virtual DbTransaction Transaction
        {
            get
            {
                // Make sure that the connection is instantiated and opened so that there will
                // be an accompying transaction to return
                DbConnection conn = Connection;

                return transaction;
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
        /// <summary>
        /// Additionally to base functionality of committing a pending transaction immediately opens
        /// a new one for use.
        /// </summary>
        /// <seealso cref="IAdoMigrationContext.Commit()"/>
		public virtual void Commit()
		{
			try
			{
                // Commit the current transaction
                Transaction.Commit();
                // Immediately open a new one to use
                transaction = connection.BeginTransaction();
			}
			catch (DbException e)
			{
				throw new MigrationException("Error committing SQL transaction", e);
			}
		}

        /// <summary>
        /// Additionally to base functionality of rolling back a pending transaction immediately opens
        /// a new one for use.
        /// </summary>
        /// <seealso cref="IAdoMigrationContext.Rollback()"/>
		public virtual void Rollback()
		{
			try
			{
                // Rollback the transaction
                Transaction.Rollback();
                // Immediately open a new one to use
                transaction = connection.BeginTransaction();
			}
			catch (DbException e)
			{
				throw new MigrationException("Could not rollback SQL transaction", e);
			}
		}

        /// <seealso cref="IAdoMigrationContext.Rollback()"/>
        public virtual void CloseConnection(bool commitTransaction)
        {
            if (transaction != null)
            {
                try
                {
                    if (commitTransaction)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception)
                {}
                finally
                {
                    transaction.Dispose();
                    transaction = null;
                }
            }

            if (connection != null)
            {
                try
                {
                    if (connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                    }
                }
                catch (Exception)
                {}
                finally
                {
                    connection.Dispose();
                    connection = null;
                }
            }
        }

        /// <seealso cref="Object.ToString()"/>
        public override String ToString()
        {
            return "DataSourceMigrationContext[" + DatabaseType + "/" + SystemName + "/"
                + Database + "]";
        }
        #endregion

        #region IDisposable members
        /// <seealso cref="IDisposable.Dispose()"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// <para>
        /// The bulk of the clean-up code is implemented in this function.
        /// </para>
        /// <para>
        /// This function releases any resources held by the class instance. It specifically closes the open
        /// <code>DbConnection</code> and rolls back any pending <code>DbTransaction</code>s. So make sure
        /// you explicitly commit any transactions in calling code otherwise the changes will not be persisted.
        /// </para>
        /// </summary>
        /// <param name="disposing">
        /// <code>true</code> if the method is being called in the disposer; <code>false</code>
        /// if the method is being called in the finalizer
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed resources
                CloseConnection(false);
            }
            // Free unmanaged resources
        }
        #endregion
    }
}

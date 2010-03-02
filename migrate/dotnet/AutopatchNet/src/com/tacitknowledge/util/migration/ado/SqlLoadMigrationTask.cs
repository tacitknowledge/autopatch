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
using log4net.Config;using MigrationContext = com.tacitknowledge.util.migration.IMigrationContext;
using MigrationException = com.tacitknowledge.util.migration.MigrationException;
using MigrationTaskSupport = com.tacitknowledge.util.migration.MigrationTaskSupport;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Base class used for creating bulk data loading <code>IMigrationTask</code>s.
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	/// <version>  $Id$
	/// </version>
	public abstract class SqlLoadMigrationTask:MigrationTaskSupport
	{
		/// <summary> Returns an input stream representing the data to load.
		/// 
		/// </summary>
		/// <returns> an input stream representing the data to load
		/// </returns>
		protected internal abstract System.IO.Stream ResourceAsStream{get;}
		/// <summary> Returns the <code>PreparedStatement</code> SQL used for inserting rows
		/// into the table.
		/// 
		/// </summary>
		/// <returns> the <code>PreparedStatement</code> SQL used for inserting rows
		/// into the table
		/// </returns>
		protected internal abstract System.String StatmentSql{get;}
		/// <summary> Class logger</summary>
		private static ILog log;
		
		/// <summary> Creates a new <code>SqlScriptMigrationTask</code>.</summary>
		public SqlLoadMigrationTask()
		{
			// Nothing to do
		}
		
		/// <seealso cref="MigrationTaskSupport.Migrate(IMigrationContext)">
		/// </seealso>
		public override void Migrate(IMigrationContext ctx)
		{
			DataSourceMigrationContext context = (DataSourceMigrationContext) ctx;
			
			try
			{
                System.Data.Common.DbCommand stmt = context.Database.GetSqlStringCommand(StatmentSql);
                stmt.Connection = context.Connection;
                stmt.Transaction = context.Transaction;
				System.Collections.IList rows = getData(ResourceAsStream);
				int rowCount = rows.Count;
				for (int i = 0; i < rowCount; i++)
				{
					System.String data = (System.String) rows[i];
					bool loadRowFlag = insert(data, stmt);
					if (loadRowFlag)
					{
                        stmt.ExecuteNonQuery();
					}
				}
			}
			catch (System.Exception e)
			{
				System.String message = Name + ": Error running SQL \"" + StatmentSql + "\"";
				log.Error(message, e);
                throw new MigrationException(message, e);
			}
		}
		
		/// <summary> Inserts the given row of data into the database using the given
		/// prepared statement.  Subclasses should parse the row and call the
		/// appropriate set methods on the prepared statement.
		/// 
		/// </summary>
		/// <param name="data">the current row of data to load
		/// </param>
		/// <param name="stmt">the statement used for inserting data into the DB
		/// 
		/// </param>
		/// <returns> false if you do not want this row loaded, true otherwise
		/// </returns>
		/// <throws>  Exception if an unexpected error occurs </throws>
		protected internal abstract bool insert(System.String data, System.Data.Common.DbCommand stmt);
		
		/// <seealso cref="java.lang.Object.toString()">
		/// </seealso>
		public override System.String ToString()
		{
			return Name;
		}
		
		/// <summary> Returns the data to load as a list of rows.
		/// 
		/// </summary>
        /// <param name="inputStream">the input stream containing the data to load
		/// </param>
		/// <returns> the data to load as a list of rows
		/// </returns>
		/// <throws>  IOException if the input stream could not be read </throws>
		protected internal virtual System.Collections.IList getData(System.IO.Stream inputStream)
		{
			System.Collections.IList data = new System.Collections.ArrayList();
			System.IO.StreamReader reader = new System.IO.StreamReader(inputStream);
			System.String line = null;
			while ((line = reader.ReadLine()) != null)
			{
				data.Add(line);
			}
			return data;
		}
		static SqlLoadMigrationTask()
		{
			log = LogManager.GetLogger(typeof(SqlLoadMigrationTask));
		}
	}
}
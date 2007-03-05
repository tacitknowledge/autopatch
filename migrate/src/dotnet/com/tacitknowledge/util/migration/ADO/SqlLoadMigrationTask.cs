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
using log4net.Config;using MigrationContext = com.tacitknowledge.util.migration.MigrationContext;
using MigrationException = com.tacitknowledge.util.migration.MigrationException;
using MigrationTaskSupport = com.tacitknowledge.util.migration.MigrationTaskSupport;
#endregion
namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Base class used for creating bulk data loading <code>MigrationTask</code>s.
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
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.SqlLoadMigrationTask'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
		
		/// <summary> Creates a new <code>SqlScriptMigrationTask</code>.</summary>
		public SqlLoadMigrationTask()
		{
			// Nothing to do
		}
		
		/// <seealso cref="MigrationTaskSupport.migrate(MigrationContext)">
		/// </seealso>
		public override void  migrate(MigrationContext ctx)
		{
			DataSourceMigrationContext context = (DataSourceMigrationContext) ctx;
			
			try
			{
				//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
				System.Data.OleDb.OleDbConnection conn = context.Connection;
				System.Data.OleDb.OleDbCommand stmt = SupportClass.TransactionManager.manager.PrepareStatement(conn, StatmentSql);
				System.Collections.IList rows = getData(ResourceAsStream);
				int rowCount = rows.Count;
				for (int i = 0; i < rowCount; i++)
				{
					System.String data = (System.String) rows[i];
					bool loadRowFlag = insert(data, stmt);
					if (loadRowFlag)
					{
						//UPGRADE_ISSUE: Method 'java.sql.PreparedStatement.addBatch' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javasqlPreparedStatementaddBatch'"
						stmt.addBatch();
						if (i % 50 == 0)
						{
							//UPGRADE_TODO: Method 'java.sql.Statement.executeBatch' was converted to 'SupportClass.BatchManager.manager.ExecuteUpdate' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javasqlStatementexecuteBatch'"
							SupportClass.BatchManager.manager.ExecuteUpdate(stmt);
						}
					}
				}
				//UPGRADE_TODO: Method 'java.sql.Statement.executeBatch' was converted to 'SupportClass.BatchManager.manager.ExecuteUpdate' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javasqlStatementexecuteBatch'"
				SupportClass.BatchManager.manager.ExecuteUpdate(stmt);
			}
			catch (System.Exception e)
			{
				System.String message = getName() + ": Error running SQL \"" + StatmentSql + "\"";
				log.error(message, e);
				if (e is System.Data.OleDb.OleDbException)
				{
					//UPGRADE_ISSUE: Method 'java.sql.SQLException.getNextException' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javasqlSQLExceptiongetNextException'"
					if (((System.Data.OleDb.OleDbException) e).getNextException() != null)
					{
						//UPGRADE_ISSUE: Method 'java.sql.SQLException.getNextException' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javasqlSQLExceptiongetNextException'"
						log.error("Chained SQL Exception", ((System.Data.OleDb.OleDbException) e).getNextException());
					}
				}
				
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
		protected internal abstract bool insert(System.String data, System.Data.OleDb.OleDbCommand stmt);
		
		/// <seealso cref="java.lang.Object.toString()">
		/// </seealso>
		public override System.String ToString()
		{
			return getName();
		}
		
		/// <summary> Returns the data to load as a list of rows.
		/// 
		/// </summary>
		/// <param name="is">the input stream containing the data to load
		/// </param>
		/// <returns> the data to load as a list of rows
		/// </returns>
		/// <throws>  IOException if the input stream could not be read </throws>
		protected internal virtual System.Collections.IList getData(System.IO.Stream is_Renamed)
		{
			System.Collections.IList data = new System.Collections.ArrayList();
			//UPGRADE_TODO: The differences in the expected value  of parameters for constructor 'java.io.BufferedReader.BufferedReader'  may cause compilation errors.  "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1092'"
			//UPGRADE_WARNING: At least one expression was used more than once in the target code. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1181'"
			System.IO.StreamReader reader = new System.IO.StreamReader(new System.IO.StreamReader(is_Renamed, System.Text.Encoding.Default).BaseStream, new System.IO.StreamReader(is_Renamed, System.Text.Encoding.Default).CurrentEncoding);
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
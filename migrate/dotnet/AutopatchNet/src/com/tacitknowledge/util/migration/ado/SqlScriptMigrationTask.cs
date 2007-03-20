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
#endregion

namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Adaptss a SQL or DDL database patch for use with the AutoPatch framework.  
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	public class SqlScriptMigrationTask:MigrationTaskSupport
	{
		/// <summary> Class logger</summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.SqlScriptMigrationTask'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
		
		/// <summary> The SQL to execute</summary>
		private System.String sql = null;
		
		/// <summary> Creates a new <code>SqlScriptMigrationTask</code>.</summary>
		public SqlScriptMigrationTask()
		{
			// do nothing
		}
		
		/// <summary> Creates a new <code>SqlScriptMigrationTask</code>
		/// 
		/// </summary>
		/// <param name="name">the name of the SQL script to execute; this is just an
		/// identifier and does not have to correspond to a file name 
		/// </param>
		/// <param name="level">the patch level of the migration task
		/// </param>
		/// <param name="sql">the SQL to execute
		/// </param>
		public SqlScriptMigrationTask(System.String name, int level, System.String sql)
		{
			Name = name;
			Level = level;
			this.sql = sql;
		}
		
		/// <summary> Creates a new <code>SqlScriptMigrationTask</code> containing the SQL
		/// contained in the given <code>InputStream</code>.
		/// 
		/// </summary>
		/// <param name="name">the name of the SQL script to execute; this is just an
		/// identifier and does not have to correspond to a file name 
		/// </param>
		/// <param name="level">the patch level of the migration task
		/// </param>
		/// <param name="is">the source of the SQL to execute
		/// </param>
		/// <throws>  IOException if there was problem reading the input stream </throws>
		public SqlScriptMigrationTask(System.String name, int level, System.IO.Stream is_Renamed)
		{
			Name = name;
			Level = level;
			System.Text.StringBuilder sqlBuffer = new System.Text.StringBuilder();
			//UPGRADE_TODO: The differences in the expected value  of parameters for constructor 'java.io.BufferedReader.BufferedReader'  may cause compilation errors.  "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1092'"
			//UPGRADE_WARNING: At least one expression was used more than once in the target code. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1181'"
			System.IO.StreamReader buf = new System.IO.StreamReader(new System.IO.StreamReader(is_Renamed, System.Text.Encoding.Default).BaseStream, new System.IO.StreamReader(is_Renamed, System.Text.Encoding.Default).CurrentEncoding);
			System.String line = buf.ReadLine();
			while (line != null)
			{
				sqlBuffer.Append(line).Append("\n");
				line = buf.ReadLine();
			}
			sql = sqlBuffer.ToString();
		}
		
		/// <seealso cref="MigrationTaskSupport.Migrate(IMigrationContext)">
		/// </seealso>
		public override void  Migrate(IMigrationContext ctx)
		{
			ADOMigrationContext context = (ADOMigrationContext) ctx;
			
			//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
			System.Data.Common.DbConnection conn = null;
            System.Data.Common.DbCommand stmt = null;
			System.String sqlStatement = "";
			try
			{
				conn = context.Connection;
				System.Collections.IList sqlStatements = getSqlStatements(context);
				//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratorhasNext'"
				for (System.Collections.IEnumerator i = sqlStatements.GetEnumerator(); i.MoveNext(); )
				{
					//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratornext'"
					sqlStatement = ((System.String) i.Current);
					if (log.IsDebugEnabled)
					{
						log.Debug(Name + ": Attempting to execute: " + sqlStatement);
					}
					//UPGRADE_TODO: Method 'java.sql.Connection.createStatement' was converted to 'SupportClass.TransactionManager.manager.CreateStatement' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javasqlConnectioncreateStatement'"
                    stmt = null;// SupportClass.TransactionManager.manager.CreateStatement(conn);
					//UPGRADE_TODO: Method 'java.sql.Statement.execute' was converted to 'System.Data.OleDb.OleDbCommand.ExecuteNonQuery' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javasqlStatementexecute_javalangString'"
					System.Data.Common.DbCommand temp_OleDbCommand;
					temp_OleDbCommand = stmt;
					temp_OleDbCommand.CommandText = sqlStatement;
					temp_OleDbCommand.ExecuteNonQuery();
					//SqlUtil.close(null, stmt, null);
					stmt = null;
				}
			}
			catch (System.Exception e)
			{
				System.String message = Name + ": Error running SQL \"" + sqlStatement + "\"";
				log.Error(message, e);
				
				if (e is System.Data.OleDb.OleDbException)
				{
					//UPGRADE_ISSUE: Method 'java.sql.SQLException.getNextException' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javasqlSQLExceptiongetNextException'"
                    //if (((System.Data.OleDb.OleDbException) e).getNextException() != null)
                    //{
                    //    //UPGRADE_ISSUE: Method 'java.sql.SQLException.getNextException' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javasqlSQLExceptiongetNextException'"
                    //    log.Error("Chained SQL Exception", ((System.Data.OleDb.OleDbException) e).getNextException());
                    //}
				}
				
				throw new MigrationException(message, e);
			}
			finally
			{
				//SqlUtil.close(null, stmt, null);
			}
		}
		
		/// <summary> Parses the SQL/DDL to execute and returns a list of individual statements.  For database
		/// types that support mulitple statements in a single <code>Statement.execute</code> call,
		/// this method will return a one-element <code>List</code> containing the entire SQL
		/// file.
		/// 
		/// </summary>
		/// <param name="context">the IMigrationContext, to figure out db type and if it 
		/// can handle multiple statements at once
		/// </param>
		/// <returns> a list of SQL and DDL statements to execute
		/// </returns>
		public virtual System.Collections.IList getSqlStatements(ADOMigrationContext context)
		{
			System.Collections.IList statements = new System.Collections.ArrayList();
			if (context.getDatabaseType().MultipleStatementsSupported)
			{
				statements.Add(sql);
				return statements;
			}
			
			System.Text.StringBuilder currentStatement = new System.Text.StringBuilder();
			bool inQuotedString = false;
			bool inComment = false;
			char[] sqlChars = sql.ToCharArray();
			for (int i = 0; i < sqlChars.Length; i++)
			{
				if (sqlChars[i] == '\n')
				{
					inComment = false;
				}
				
				if (!inComment)
				{
					switch (sqlChars[i])
					{
						
						case '-': 
						case '/': 
							if (!inQuotedString && i + 1 < sqlChars.Length && sqlChars[i + 1] == sqlChars[i])
							{
								inComment = true;
							}
							else
							{
								currentStatement.Append(sqlChars[i]);
							}
							break;
						
						case '\'': 
							inQuotedString = !inQuotedString;
							currentStatement.Append(sqlChars[i]);
							break;
						
						case ';': 
							if (!inQuotedString)
							{
								// If we're in a stored procedure, just keep rolling
								if (context.getDatabaseType().getDatabaseType().Equals("oracle") && (currentStatement.ToString().Trim().ToLower().StartsWith("begin") || currentStatement.ToString().Trim().ToLower().StartsWith("create or replace method") || currentStatement.ToString().Trim().ToLower().StartsWith("create or replace function") || currentStatement.ToString().Trim().ToLower().StartsWith("create or replace procedure") || currentStatement.ToString().ToString().ToLower().StartsWith("create or replace package")))
								{
									currentStatement.Append(sqlChars[i]);
								}
								else
								{
									statements.Add(currentStatement.ToString().Trim());
									currentStatement = new System.Text.StringBuilder();
								}
							}
							else
							{
								currentStatement.Append(sqlChars[i]);
							}
							break;
						
						default: 
							currentStatement.Append(sqlChars[i]);
							break;
						
					}
				}
			}
			if (currentStatement.ToString().Trim().Length > 0)
			{
				statements.Add(currentStatement.ToString().Trim());
			}
			
			return statements;
		}
		
		/// <seealso cref="java.lang.Object.toString()">
		/// </seealso>
		public override System.String ToString()
		{
			return Name;
		}

        static SqlScriptMigrationTask()
		{
			log = LogManager.GetLogger(typeof(SqlScriptMigrationTask));
		}
	}
}
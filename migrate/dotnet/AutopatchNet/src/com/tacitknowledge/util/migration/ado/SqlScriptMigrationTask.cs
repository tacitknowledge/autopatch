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
using System.Data.Common;
using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net;
using com.tacitknowledge.util.migration;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
	/// <summary>
    /// Adapts a SQL (DML or DDL) database patch for use with the AutoPatch framework.
	/// </summary>
	/// <author>Scott Askew (scott@tacitknowledge.com)</author>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public class SqlScriptMigrationTask : MigrationTaskSupport
	{
        #region Member variables
        private static readonly ILog log = LogManager.GetLogger(typeof(SqlScriptMigrationTask));

        /// <summary>
        /// The SQL to execute
        /// </summary>
		private String sql = null;
        #endregion

        #region Constructors
		/// <summary>
        /// Creates a new <code>SqlScriptMigrationTask</code>.
		/// </summary>
		/// <param name="name">
        /// the name of the SQL script to execute; this is just an
		/// identifier and does not have to correspond to a file name 
		/// </param>
		/// <param name="level">
        /// the patch level of the migration task
		/// </param>
		/// <param name="sql">
        /// the SQL to execute
		/// </param>
		public SqlScriptMigrationTask(String name, int level, String sql)
		{
			Name = name;
			Level = level;
			this.sql = sql;
		}
		
		/// <summary>
        /// Creates a new <code>SqlScriptMigrationTask</code> containing the SQL
        /// contained in the given <code>StreamReader</code>.
		/// </summary>
		/// <param name="name">
        /// the name of the SQL script to execute; this is just an
		/// identifier and does not have to correspond to a file name 
		/// </param>
		/// <param name="level">
        /// the patch level of the migration task
		/// </param>
		/// <param name="sr">
        /// the source of the SQL to execute
		/// </param>
        /// <exception cref="IOException">if there was an IO error</exception>
        /// <exception cref="OutOfMemoryException">
        /// if there is insufficient memory to allocate a buffer for the returned string
        /// </exception>
		public SqlScriptMigrationTask(String name, int level, StreamReader sr)
		{
            Name = name;
            Level = level;

            String line = null;
            StringBuilder sb = new StringBuilder();

            while ((line = sr.ReadLine()) != null)
            {
                sb.Append(line).Append(Environment.NewLine);
            }

            sql = sb.ToString();
        }
        #endregion

        #region Public methods
        /// <seealso cref="MigrationTaskSupport.Migrate(IMigrationContext)" />
		public override void Migrate(IMigrationContext ctx)
		{
            IAdoMigrationContext context = (IAdoMigrationContext)ctx;
            String sqlStatement = null;

            try
            {
                IList<String> sqlStatements = GetSqlStatements(context);
                DbConnection dbConn = context.Connection;
                DbCommand dbCmd = null;

                // Cleaning the slate before we execute the patch.
                // This was inspired by a Sybase ASE server that did not allow
                // ALTER TABLE statements in multi-statement transactions. Instead of putting
                // an if (sybase) conditional, we decided to clean the slate for everyone.
                context.Commit();

                foreach (String tempSqlStatement in sqlStatements)
                {
                    sqlStatement = tempSqlStatement;

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(Name + ": Attempting to execute: " + sqlStatement);
                    }

                    try
                    {
                        dbCmd = dbConn.CreateCommand();
                        dbCmd.CommandText = sqlStatement;
                        dbCmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        if (dbCmd != null)
                        {
                            dbCmd.Dispose();
                        }

                        dbCmd = null;
                    }
                }

                context.Commit();
            }
            catch (Exception e)
            {
                String message = Name + ": Error running SQL \"" + sqlStatement + "\"";
                log.Error(message, e);

                if (e is DbException && e.InnerException != null)
                {
                    log.Error("Chained SQL Exception", e.InnerException);
                }

                throw new MigrationException(message, e);
            }
		}
		
		/// <summary>
        /// Parses the SQL (DML/DDL) to execute and returns a list of individual statements. For database
		/// types that support multiple statements in a single <code>Statement.execute</code> call,
		/// this method will return a one-element <code>List</code> containing the entire SQL
		/// file.
		/// </summary>
		/// <param name="context">
        /// the <code>IMigrationContext</code>, to figure out db type and if it can handle multiple
        /// statements at once
		/// </param>
		/// <returns>
        /// a list of SQL (DML/DDL) statements to execute
		/// </returns>
        public virtual IList<String> GetSqlStatements(IAdoMigrationContext context)
		{
			IList<String> statements = new List<String>();

			if (context.DatabaseType.MultipleStatementsSupported)
			{
				statements.Add(sql);
				return statements;
			}
			
			StringBuilder currentStatement = new StringBuilder();
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
								if (context.DatabaseType.getDatabaseType().Equals("oracle")
                                    && (currentStatement.ToString().Trim().ToLower().StartsWith("begin")
                                        || currentStatement.ToString().Trim().ToLower().StartsWith("create or replace method")
                                        || currentStatement.ToString().Trim().ToLower().StartsWith("create or replace function")
                                        || currentStatement.ToString().Trim().ToLower().StartsWith("create or replace procedure")
                                        || currentStatement.ToString().Trim().ToLower().StartsWith("create or replace package")))
								{
									currentStatement.Append(sqlChars[i]);
								}
								else
								{
									statements.Add(currentStatement.ToString().Trim());
									currentStatement = new StringBuilder();
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
		
		/// <seealso cref="Object.ToString()" />
		public override String ToString()
		{
			return Name;
		}
        #endregion
	}
}

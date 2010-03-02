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
using SqlLoadMigrationTask = com.tacitknowledge.util.migration.ado.SqlLoadMigrationTask;
#endregion

namespace com.tacitknowledge.util.migration.ado.loader
{
	
	/// <summary> Loads files assumed to be in a delimited format and representing a 
	/// table in the database.  The file name should begin with the prefix:  
	/// "&lt;tablename&gt;_db".  The file's first row should represent the name of 
	/// each column in the table that the underlying data elements (rows 2 
	/// through n) will be mapped to.  
	/// 
	/// </summary>
	/// <author>  Chris A. (chris@tacitknowledge.com)
	/// </author>
	/// <version>  $Id$
	/// </version>
	public abstract class DelimitedFileLoader:SqlLoadMigrationTask
    {
        private const string PARAMETER_NAME_PREFIX = "@P";
        #region Member Variable
        /// <summary> The path separator</summary>
        public static readonly System.String PATH_SEPARATOR = System.IO.Path.DirectorySeparatorChar.ToString();

        /// <summary> Class logger</summary>
        private static ILog log;

        /// <summary> Private variable that indicates if the header has been parsed or not. </summary>
        private bool parsedHeader = false;
        #endregion

        #region Methods
        /// <summary> Gets the expected file delimiter.  A pipe-delimited 
		/// reader should return "|", for example.
		/// 
		/// </summary>
		/// <returns> the delimiter string used to separate columns
		/// </returns>
		public abstract System.Char Delimiter{get;}

		/// <summary> Returns the table name from the full path name 
		/// by parsing it out of a file in the format
		/// name_db&lt;period&gt;(some extension)
		/// 
		/// </summary>
		/// <returns> the name of the table to add data to
		/// </returns>
		virtual protected internal System.String TableFromName
		{
			get
			{
				System.String name = Name;
				int startTable = name.LastIndexOf(PATH_SEPARATOR);
				int endTable = name.IndexOf("_db", startTable);
				return name.Substring((startTable + 1), (endTable) - ((startTable + 1)));
			}
			
		}
		/// <summary> Parses the table name from the file name, and the column names 
		/// from the header (first row) of the delimited file.  Creates an 
		/// insert query to insert a single row. 
		/// 
		/// </summary>
		/// <returns> query in the format: INSERT INTO tablename (colname, colname)...
		/// </returns>
		override protected internal System.String StatmentSql
		{
			get
			{
				try
				{
					System.String columnHeader = getHeader(ResourceAsStream);
					System.Char delimiter = Delimiter;
                    string[] columnNames = columnHeader.Split(delimiter);

                    System.Text.StringBuilder query = new System.Text.StringBuilder("INSERT INTO ");
                    query.Append(TableFromName);
					query.Append(" (");
					System.Collections.IEnumerator it = columnNames.GetEnumerator();
					bool firstTime = true;
					while (it.MoveNext())
					{
						if (!firstTime)
						{
							query.Append(", ");
						}
						else
						{
							firstTime = false;
						}
						query.Append((string) it.Current);
					}
					query.Append(") VALUES (");
					for (int i = 1; i <= columnNames.Length; i++)
					{
						if (i > 1)
                        {
                            query.Append(", ");
                        }
                        query.Append(PARAMETER_NAME_PREFIX + i.ToString());
                    }
                    query.Append(")");
					return query.ToString();
				}
				catch (System.ArgumentException e)
				{
					log.Error("No header was found for file: " + Name, e);
				}
				return null;
			}
        }

        /// <summary> Gets an input stream by first checking the current classloader, 
		/// then trying to use the system classloader, and finally, trying 
		/// to access the file on the file system.  If the file is not found, 
		/// an <code>IllegalArgumentException</code> will be thrown.
		/// 
		/// </summary>
		/// <returns> the file as an input stream
		/// </returns>
		override protected internal System.IO.Stream ResourceAsStream
		{
			get
			{
				FileLoadingUtility utility = new FileLoadingUtility(Name);
				return utility.ResourceAsStream;
			}
			
		}
		
		
		/// <summary> Should return the name of the file to load.  This will 
		/// support either an absolute name, or the name of a file 
		/// in the classpath of the application in which this is executed.
		/// 
		/// </summary>
		/// <returns> the absolute path name of the table to load
		/// </returns>
        public abstract override String Name
        {
            get;
        }
		
		/// <summary> Parses a line of data, and sets the prepared statement with the 
		/// values.  If a token contains "&lt;null&gt;" then a null value is passed 
		/// in. 
		/// 
		/// </summary>
		/// <param name="data">the tokenized string that is mapped to a row
		/// </param>
		/// <param name="stmt">the statement to populate with data to be inserted
		/// </param>
		/// <returns> false if the header is returned, true otherwise
		/// </returns>
		/// <throws>  SQLException if an error occurs while inserting data into the database </throws>
		override protected internal bool insert(System.String data, System.Data.Common.DbCommand stmt)
		{
			if (!parsedHeader)
			{
				parsedHeader = true;
				log.Info("Header returned: " + data);
				return false;
			}

            int counter = 1;
			log.Info("Row being parsed: " + data);
            stmt.Parameters.Clear();
            foreach (string colVal in data.Split(Delimiter))
            {
                System.Data.Common.DbParameter parameter = stmt.CreateParameter();
                parameter.ParameterName = PARAMETER_NAME_PREFIX + counter.ToString();

                if (colVal.ToLower().Equals("<null>"))
                {
                    parameter.Value = DBNull.Value;
                }
                else
                {
                    parameter.Value = colVal;
                }

                stmt.Parameters.Add(parameter);
                counter++;
            }
            return true;
		}
		
		/// <summary> Returns the header (first line) of the file.
		/// 
		/// </summary>
        /// <param name="inputStream">the input stream containing the data to load
		/// </param>
		/// <returns> the first row
		/// </returns>
		/// <throws>  IOException if the input stream could not be read </throws>
		protected internal virtual System.String getHeader(System.IO.Stream inputStream)
		{
            System.IO.StreamReader reader = new System.IO.StreamReader(inputStream);
			return reader.ReadLine();
		}
		static DelimitedFileLoader()
		{
			
            log = LogManager.GetLogger(typeof(DelimitedFileLoader));
        }
        #endregion
    }
}
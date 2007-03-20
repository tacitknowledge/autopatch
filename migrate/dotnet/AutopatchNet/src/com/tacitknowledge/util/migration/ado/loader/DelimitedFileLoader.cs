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
	/// "&lt;tablename&gt;_tb".  The file's first row should represent the name of 
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
        #region Member Variable
        /// <summary> The path separator</summary>
        //UPGRADE_NOTE: Final was removed from the declaration of 'PATH_SEPARATOR '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        public static readonly System.String PATH_SEPARATOR = System.IO.Path.DirectorySeparatorChar.ToString();

        /// <summary> Class logger</summary>
        //UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.loader.DelimitedFileLoader'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
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
		public abstract System.String Delimiter{get;}
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
				//UPGRADE_WARNING: Method 'java.lang.String.indexOf' was converted to 'System.String.IndexOf' which may throw an exception. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1101'"
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
					System.String delimiter = Delimiter;
                    //SupportClass.Tokenizer st = new SupportClass.Tokenizer(columnHeader, delimiter);
					System.Collections.ArrayList columnNames = new System.Collections.ArrayList();
                    //while (st.HasMoreTokens())
                    //{
                    //    columnNames.Add((st.NextToken().Trim()));
                    //}
					System.Text.StringBuilder query = new System.Text.StringBuilder("INSERT INTO ");
					query.Append(TableFromName);
					query.Append(" (");
					System.Collections.IEnumerator it = columnNames.GetEnumerator();
					bool firstTime = true;
					//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratorhasNext'"
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
						//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratornext'"
						query.Append((System.String) it.Current);
					}
					query.Append(") VALUES (");
					for (int i = 0; i < columnNames.Count; i++)
					{
						if (i > 0)
						{
							query.Append(", ");
						}
						query.Append("?");
					}
					query.Append(")");
					return query.ToString();
				}
				catch (System.IO.IOException e)
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
		protected internal bool insert(System.String data, System.Data.OleDb.OleDbCommand stmt)
		{
			if (!parsedHeader)
			{
				parsedHeader = true;
				log.Info("Header returned: " + data);
				return false;
			}
            //SupportClass.Tokenizer st = new SupportClass.Tokenizer(data, Delimiter);
			//int counter = 1;
			log.Info("Row being parsed: " + data);
            //while (st.HasMoreTokens())
            //{
            //    System.String colVal = st.NextToken();
            //    if (colVal.ToUpper().Equals("<null>".ToUpper()))
            //    {
            //        SupportClass.TransactionManager.manager.SetValue(stmt, counter, null);
            //    }
            //    else
            //    {
            //        SupportClass.TransactionManager.manager.SetValue(stmt, counter, colVal);
            //    }
            //    counter++;
            //}
			return true;
		}
		
		/// <summary> Returns the header (first line) of the file.
		/// 
		/// </summary>
		/// <param name="is">the input stream containing the data to load
		/// </param>
		/// <returns> the first row
		/// </returns>
		/// <throws>  IOException if the input stream could not be read </throws>
		protected internal virtual System.String getHeader(System.IO.Stream is_Renamed)
		{
			//UPGRADE_TODO: The differences in the expected value  of parameters for constructor 'java.io.BufferedReader.BufferedReader'  may cause compilation errors.  "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1092'"
			//UPGRADE_WARNING: At least one expression was used more than once in the target code. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1181'"
			System.IO.StreamReader reader = new System.IO.StreamReader(new System.IO.StreamReader(is_Renamed, System.Text.Encoding.Default).BaseStream, new System.IO.StreamReader(is_Renamed, System.Text.Encoding.Default).CurrentEncoding);
			return reader.ReadLine();
		}
		static DelimitedFileLoader()
		{
			
            log = LogManager.GetLogger(typeof(DelimitedFileLoader));
        }
        #endregion
    }
}
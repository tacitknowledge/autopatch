/* Copyright 2004 Tacit Knowledge LLC
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
#endregion

namespace com.tacitknowledge.util.migration.ado.util
{
	
	/// <summary> Utility class for dealing with ADO.
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	/// <version>  $Id$
	/// </version>
	public sealed class SqlUtil
    {
        #region Members
        /// <summary> Class logger</summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.util.SqlUtil'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
        #endregion

        #region Methods
        /// <summary> Hidden constructor for utility class</summary>
		private SqlUtil()
		{
			// Hidden
		}
		
		/// <summary> Ensures the given connection, statement, and result are properly closed.
		/// 
		/// </summary>
		/// <param name="conn">the connection to close; may be <code>null</code>
		/// </param>
		/// <param name="stmt">the statement to close; may be <code>null</code>
		/// </param>
		/// <param name="rs">the result set to close; may be <code>null</code>
		/// </param>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
		//UPGRADE_TODO: Interface 'java.sql.ResultSet' was converted to 'System.Data.OleDb.OleDbDataReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javasqlResultSet'"
		/*
        public static void  close(System.Data.Common.DbConnection conn, System.Data.OleDb.OleDbCommand stmt, System.Data.OleDb.OleDbDataReader rs)
		{
			if (rs != null)
			{
				try
				{
					rs.Close();
				}
				catch (System.Data.OleDb.OleDbException e)
				{
					log.Error("Error closing ResultSet", e);
                    
				}
			}
			
			if (stmt != null)
			{
				try
				{
					//UPGRADE_ISSUE: Method 'java.sql.Statement.close' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javasqlStatementclose'"
					//stmt.close();
				}
				catch (System.Data.OleDb.OleDbException e)
				{
					log.Error("Error closing Statment", e);
				}
			}
			
			if (conn != null)
			{
				try
				{
					SupportClass.TransactionManager.manager.Close(conn);
				}
				catch (System.Data.OleDb.OleDbException e)
				{
					log.Error("Error closing Connection", e);
				}
			}
		}
		*/

        /*
		/// <summary> Returns a connection from the <code>DataSource</code> located in JNDI
		/// under the specified name. 
		/// 
		/// </summary>
		/// <param name="dsn">the name of the DataSource in JDNI
		/// </param>
		/// <returns> a connection from the <code>DataSource</code>
		/// </returns>
		/// <throws>  NamingException if the datasource could not be found in JNDI </throws>
		/// <throws>  SQLException if a connnection could not be made to the database </throws>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
		public static System.Data.OleDb.OleDbConnection getConnection(System.String dsn)
		{
			//UPGRADE_TODO: Constructor 'javax.naming.InitialContext.InitialContext' was converted to 'System.DirectoryServices.DirectoryEntry' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaxnamingInitialContextInitialContext'"
			//UPGRADE_TODO: Adjust remoting context initialization manually. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1258'"
			System.DirectoryServices.DirectoryEntry context = new System.DirectoryServices.DirectoryEntry();
			//UPGRADE_ISSUE: Interface 'javax.sql.DataSource' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxsqlDataSource'"
			//UPGRADE_TODO: Method 'javax.naming.InitialContext.lookup' was converted to 'System.Activator.GetObject' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaxnamingInitialContextlookup_javalangString'"
			DataSource ds = (DataSource) Activator.GetObject(typeof(System.MarshalByRefObject), SupportClass.ParseURILookup(dsn));
			System.Data.OleDb.OleDbConnection temp_Connection;
			//UPGRADE_TODO: Change connection string to .NET format. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1063'"
			//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
			temp_Connection = new System.Data.OleDb.OleDbConnection();
			temp_Connection.Open();
			return temp_Connection;
		}
		
		/// <summary> Established and returns a connection based on the specified parameters. 
		/// 
		/// </summary>
		/// <param name="driver">the ADO driver to use
		/// </param>
		/// <param name="url">the database URL
		/// </param>
		/// <param name="user">the username
		/// </param>
		/// <param name="pass">the password
		/// </param>
		/// <returns> a ADO connection
		/// </returns>
		/// <throws>  ClassNotFoundException if the driver could not be loaded </throws>
		/// <throws>  SQLException if a connnection could not be made to the database </throws>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
		public static System.Data.OleDb.OleDbConnection getConnection(System.String driver, System.String url, System.String user, System.String pass)
		{
			//UPGRADE_TODO: The differences in the format  of parameters for method 'java.lang.Class.forName'  may cause compilation errors.  "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1092'"
			System.Type.GetType(driver);
			System.Data.OleDb.OleDbConnection temp_Connection;
			//UPGRADE_TODO: Change connection string to .NET format. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1063'"
			temp_Connection = new System.Data.OleDb.OleDbConnection(url + ";   User ID=" + user + ";   PWD=" + pass);
			temp_Connection.Open();
			return temp_Connection;
		}
         * */
		static SqlUtil()
		{
            log = LogManager.GetLogger(typeof(SqlUtil));
        }
        #endregion
    }
}
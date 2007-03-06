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
using log4net.Config;
using System.Data;
using System.Data.OleDb;
#endregion

namespace com.tacitknowledge.util.migration.ado.util
{
	
	/// <summary> A partial <code>DataSource</code> implementation that simply wraps a single,
	/// already opened <code>Connection</code>.
	/// <p>
	/// Only the two <code>getConnection</code> methods are supported.
	/// 
	/// </summary>
	/// <author>  Scott Askew (scott@tacitknowledge.com)
	/// </author>
	//UPGRADE_ISSUE: Interface 'javax.sql.DataSource' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxsqlDataSource'"
	public class ConnectionWrapper
    {
        #region Member Variables
        /// <summary> The message used in <code>UnsupportedOperationException</code>s.</summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'UNSUPPORTED_OPERATION_EXCEPTION_MSG '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		//UPGRADE_NOTE: The initialization of  'UNSUPPORTED_OPERATION_EXCEPTION_MSG' was moved to static method 'com.tacitknowledge.util.migration.ado.util.ConnectionWrapper'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		public static readonly System.String UNSUPPORTED_OPERATION_EXCEPTION_MSG;
		
		/// <summary> The underlying connection</summary>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
		private System.Data.OleDb.OleDbConnection connection = null;

        ///<summary>Class Logger</summary>
        private static ILog log;
        #endregion

        #region Methods
        /// <summary> Creates a new <code>ConnectionWrapper</code>. 
		/// 
		/// </summary>
		/// <param name="connection">the connection to use for this data source
		/// </param>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
		public ConnectionWrapper(System.Data.OleDb.OleDbConnection connection)
		{
			this.connection = connection;
		}
		
		/// <seealso cref="javax.sql.DataSource.getConnection()">
		/// </seealso>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
		public System.Data.OleDb.OleDbConnection getConnection()
		{
			return connection;
		}
		
		/// <seealso cref="javax.sql.DataSource.getConnection(java.lang.String, java.lang.String)">
		/// </seealso>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
		public  System.Data.OleDb.OleDbConnection getConnection(System.String user, System.String pass)
		{
			return connection;
		}
		
		/// <seealso cref="javax.sql.DataSource.getLogWriter()">
		/// </seealso>
		public System.IO.StreamWriter getLogWriter()
		{
			throw new System.NotSupportedException(UNSUPPORTED_OPERATION_EXCEPTION_MSG);
		}
		
		/// <seealso cref="javax.sql.DataSource.setLogWriter(java.io.PrintWriter)">
		/// </seealso>
		public void  setLogWriter(System.IO.StreamWriter arg0)
		{
			throw new System.NotSupportedException(UNSUPPORTED_OPERATION_EXCEPTION_MSG);
		}
		
		/// <seealso cref="javax.sql.DataSource.getLoginTimeout()">
		/// </seealso>
		public int getLoginTimeout()
		{
			throw new System.NotSupportedException(UNSUPPORTED_OPERATION_EXCEPTION_MSG);
		}
		
		/// <seealso cref="javax.sql.DataSource.setLoginTimeout(int)">
		/// </seealso>
		public void  setLoginTimeout(int arg0)
		{
			throw new System.NotSupportedException(UNSUPPORTED_OPERATION_EXCEPTION_MSG);
		}
		static ConnectionWrapper()
		{
			UNSUPPORTED_OPERATION_EXCEPTION_MSG = typeof(ConnectionWrapper) + " is not a fully functioning DataSource and only" + " supports the getConnection methods.";
            log = LogManager.GetLogger(typeof(ConnectionWrapper));       
        }



        #endregion
    }
}
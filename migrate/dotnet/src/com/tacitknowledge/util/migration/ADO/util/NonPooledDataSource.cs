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

using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

using log4net;
using log4net.Config;
#endregion

namespace com.tacitknowledge.util.migration.ado.util
{


    /// <summary> A partial <code>DataSource</code> implementation that can be used in environments
	/// where the containing application (usually an applications server) does not
	/// provide a pooled DataSource.  This can be used to run migrations from standalone
	/// applications.
	/// <p>
	/// Only the two <code>getConnection</code> methods are supported.
	/// 
	/// </summary>
	/// <author>  Scott Askew (scott@tacitknowledge.com)
	/// </author>
	//UPGRADE_ISSUE: Interface 'javax.sql.DataSource' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxsqlDataSource'"
	public class NonPooledDataSource
    {

        #region Member Variables
        /// <summary> Class logger</summary>
        private static ILog log;

        /// <summary> The message used in <code>UnsupportedOperationException</code>s.</summary>
        public static readonly System.String UNSUPPORTED_OPERATION_EXCEPTION_MSG;

        /// <summary> The name of the database sqlclient type </summary>
        private System.String sqlClientType = null;

        /// <summary> The ADO URL</summary>
        private System.String server = null;

        /// <summary> The user to login as</summary>
        private System.String username = null;

        /// <summary> The database password</summary>
        private System.String password = null;

        /// <summary>The database name</summary>
        private String name = null;


        #endregion


        #region Methods
        /// <summary>
        /// Sets and returns the name of the database
        /// </summary>
        /// <param name="name">The name of the database</param>
        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <returns> Returns the server where the database is hosted.
		/// </returns>
		/// <param name="server">The server of the database.
		/// </param>
		public System.String Server
		{
			get
			{
				return server;
			}
			
			set
			{
				this.server = value;
			}
			
		}
     

		/// <returns> Returns the SqlClient.
		/// </returns>
		/// <param name="SqlClient">The SqlClient to set.
		/// </param>
		public System.String SqlClientType
		{
			get
			{
				return sqlClientType;
			}
			
			set
			{
				this.sqlClientType = value;
			}
			
		}
		/// <returns> Returns the password.
		/// </returns>
		/// <param name="password">The password to set.
		/// </param>
		public System.String Password
		{
			get
			{
				return password;
			}
			
			set
			{
				this.password = value;
			}
			
		}
		/// <returns> Returns the username.
		/// </returns>
		/// <param name="username">The username to set.
		/// </param>
		virtual public System.String Username
		{
			get
			{
				return username;
			}
			
			set
			{
				this.username = value;
			}
			
		}

       

		/// <summary> Creates a new <code>BasicDataSource</code>.
		/// 
		/// </summary>
		public NonPooledDataSource()
		{
			// Default constructor
		}
		
		/// <seealso cref="System.Data.Common.DbConnection">
		/// </seealso>
		public System.Data.Common.DbConnection getConnection()
		{
			return getConnection(Username, Password);
		}
		
		/// <seealso cref="System.Data.Common.DbConnection">
		/// </seealso>
		public System.Data.Common.DbConnection getConnection(System.String user, System.String pass)
		{
			try
			{
				return SqlUtil.getConnection(DriverClass, DatabaseUrl, Username, Password);
			}
			//UPGRADE_NOTE: Exception 'java.lang.ClassNotFoundException' was converted to 'System.Exception' which has different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1100'"
			catch (System.Exception e)
			{
				//UPGRADE_ISSUE: Constructor 'java.sql.SQLException.SQLException' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javasqlSQLExceptionSQLException_javalangString'"
				throw new SQLException("Could not locate ADO driver " + driverClass);
			}
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
		static NonPooledDataSource()
		{
			UNSUPPORTED_OPERATION_EXCEPTION_MSG = typeof(NonPooledDataSource) + " is not a fully functioning DataSource and only" + " supports the getConnection methods.";
            log = LogManager.GetLogger(typeof(NonPooledDataSource));
        }
        #endregion
    }
}
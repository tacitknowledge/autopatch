using System;
using System.Data.Common;
using System.Data.OracleClient;
using System.Data.SqlClient;

using System.Collections.Generic;
using System.Text;

using log4net;
using log4net.Config;

using AutopatchNET.dotnet.com.tacitknowledge.util.migration.ADO.conf;

namespace AutopatchNET.src.com.tacitknowledge.util.migration.ADO.data
{
    /// <summary>
    /// The DBConnectionFactory provides the appropriate connection object(OracleConnection, SQLConnection etc.) based on the
    /// database type we're configured to use. 
    /// 
    /// </summary>
    class DBConnectionFactory
    {
        #region Members
        /// <summary>
        /// Log object
        /// </summary>
        private static ILog log;

        private const String ORACLE_DB = "oracle";

        private const String SQL_DB = "sqlserver";

        // private const String MYSQL_DB = "mysql";
        #endregion

        #region Methods
        /// <summary>
        /// Determines the type of database we're configured to use and returns the appropriate connection
        /// object
        /// </summary>
        /// <returns></returns>
      

        public DbConnection getConnection()
        {
            /*
             * Determine the type of database we're configured to use
             */
            log.Debug("Getting configuration information from ConfigurationManager");
            ConfigurationManager configMgr = new ConfigurationManager();
            DBConfiguration dbConfig = configMgr.getDBConfiguration();
            
            /*
             * Determine the type of DB
             */
            if (dbConfig.DatabaseType == ORACLE_DB)
            {
                return getOracleConnection(dbConfig);
            }
            
                return getSQLConnection(dbConfig);
            
           
            //TODO: throw exception if configured for a different database type


        }

        /// <summary>
        /// Returns a SQLConnection for Microsoft SQL Server
        /// </summary>
        /// <param name="dbConfig"></param>
        /// <returns></returns>
        private System.Data.SqlClient.SqlConnection getSQLConnection(DBConfiguration dbConfig)
        {
            return new SqlConnection();
        }

        /// <summary>
        /// Returns an OracleConnection object
        /// </summary>
        /// <param name="dbConfig"></param>
        /// <returns></returns>
        private OracleConnection getOracleConnection(DBConfiguration dbConfig)
        {
            return new OracleConnection();
        }

        

        static DBConnectionFactory()
		{
            log = LogManager.GetLogger(typeof(DBConnectionFactory));
		}
    }
        #endregion
}

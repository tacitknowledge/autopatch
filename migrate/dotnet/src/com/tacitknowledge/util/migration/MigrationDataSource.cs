using System;
using System.Data.Common;

using log4net;
using log4net.Config;

using AutopatchNET.src.com.tacitknowledge.util.migration.ADO.data;

namespace AutopatchNET.src.com.tacitknowledge.util.migration
{
    /// <summary>
    /// Contains the connection object necessary to reach the data store
    /// </summary>
    class MigrationDataSource
    {

        #region Members
        /// <summary>
        /// Log object
        /// </summary>
        private static ILog log;
        #endregion  

        #region Methods
        /// <summary>
        /// Returns the connection object for the data store
        /// </summary>
        /// <returns></returns>
        public DbConnection getConnection()
        {
            
            log.Debug("Getting Connection from DBConnectionFactory");
            DBConnectionFactory dbConnFactory = new DBConnectionFactory();

            return dbConnFactory.getConnection();

        }

        static MigrationDataSource()
        {
            log = LogManager.GetLogger(typeof(MigrationDataSource));
        }
        #endregion
    }
}

/*
 * Copyright 2006 Tacit Knowledge LLC
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
using log4net;
using AutopatchNET.src.com.tacitknowledge.util.migration.ADO.data;
#endregion

namespace AutopatchNET.src.com.tacitknowledge.util.migration
{
    /// <summary>
    /// Contains the connection object necessary to reach the data store
    /// </summary>
    /// <author>Ian Mortimer (imorti@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public class MigrationDataSource
    {
        #region Member variables
        private static ILog log;
        #endregion

        #region Costructors
        /// <summary>
        /// Static constructor.
        /// </summary>
        static MigrationDataSource()
        {
            log = LogManager.GetLogger(typeof(MigrationDataSource));
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns the connection object for the data store.
        /// </summary>
        /// <returns>the connection object for the data store</returns>
        public DbConnection getConnection()
        {
            log.Debug("Getting Connection from DBConnectionFactory");
            DBConnectionFactory dbConnFactory = new DBConnectionFactory();

            return dbConnFactory.getConnection();
        }
        #endregion
    }
}

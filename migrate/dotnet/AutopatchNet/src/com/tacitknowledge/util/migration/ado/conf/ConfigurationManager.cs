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
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Configuration;
using log4net;
using log4net.Config;
#endregion

/*
 * The purpose of this class is to load up configuration information from a resource file and make 
 * configuration information available to classes that require it. 
 * Information such as the database type, location, name, IP, database schema, username and password will be managed
 * by the ConfigurationManager.
 */

namespace com.tacitknowledge.util.migration.ado.conf
{
    class ConfigurationManager
    {

        #region Members
        /*
         * Logger
         */ 
        private static ILog log;

        /*
         * Database configuration for checking PatchTable
         */ 
        private DBConfiguration dbConfig;

        /*
         * MigrationConfiguration
         */
        private MigrationConfiguration migrationConfig;

        private ConfigurationFactory configFactory;

        #endregion

        #region Methods
        /*
         * Public constructor. Will load up the information from the resource
         * file if it hasn't been loaded yet. If it has been loaded, it will 
         */
        public ConfigurationManager()
        {
             configFactory = new ConfigurationFactory();
        }
        /*
         * This method returns the MigrationConfiguration object
         */ 
        public MigrationConfiguration getMigrationConfiguration()
        {
                try
                {
                    
                    migrationConfig = configFactory.getMigrationConfiguration();
                }
                catch (Exception e)
                {
                    log.Debug("Error getting MigrationConfiguration object from ConfigurationFactory: " + e.ToString());
                }
                return migrationConfig;

        }

        /// <summary>
        /// This method obtains the DBConfiguration object throught he ConfigurationFactory
        /// </summary>
        public DBConfiguration getDBConfiguration()
        {
            try
            {
                dbConfig = configFactory.getDBConfiguration();
            }catch(Exception e){
            
                log.Debug("Error getting DBConfiguration object from ConfigurationFactory: " + e.ToString());

            }
            return dbConfig;

        }


        static ConfigurationManager()
        {
            //Get Logger
            log = LogManager.GetLogger(typeof(ConfigurationManager));
    
        }

        #endregion





    }
}

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
using System.Configuration;
using log4net;
using log4net.Config;
#endregion

/*
 * <summary>This class is responsible for retrieving the configuration information in the application config and loading into
 * objects. It then returns these objects to the ConfigurationManager to be accessed by the rest of the application.</summary>
 * 
 * <author>imorti@tacitknowledge.com</author>
 * 
 */ 
namespace com.tacitknowledge.util.migration.ado.conf
{

    class ConfigurationFactory
    {
        #region Members
        /*
         * Log
         */ 
        private static ILog log;
        /*
         * MigrationConfiguration object
         */
        private MigrationConfiguration migrationConfig = null;

        public MigrationConfiguration MigrationConfiguration
        {
            get {
                if (migrationConfig != null)
                {
                    return migrationConfig;

                }
                else
                {
                    getMigrationConfiguration();
                    return migrationConfig;
                }
                
                 }
            set { migrationConfig = value; }
        }

        /*
         * DBConfiguration oejct
         */
        private DBConfiguration dbConfig = null;

        public DBConfiguration DbConfiguration
        {
            get {

                if (dbConfig != null)
                {
                    return dbConfig;
                   
                }
                else
                {
                    getDBConfiguration();
                    return dbConfig;
                }

                
                }
            set { dbConfig = value; }
        }


        #endregion

        #region Methods





        internal DBConfiguration getDBConfiguration()
        {
            if (dbConfig == null)
            {

                try
                {

                    // Retrieve DB configuration settings from app.config file
                    dbConfig = System.Configuration.ConfigurationManager.GetSection("MigrationDatabaseSettings") as DBConfiguration;

                }
                catch (Exception e)
                {

                    log.Fatal("Error retrieving MigrationDatabaseSettings: " + e.StackTrace);
                    throw new ApplicationException("ConfigurationFactory::getDBConfiguration - error retrieving DB Configuration settings:" + e.StackTrace);
                }

            }
            return dbConfig;
            
        }


        internal MigrationConfiguration getMigrationConfiguration()
        {
            if (migrationConfig == null)
            {
                try
                {
                    migrationConfig = System.Configuration.ConfigurationManager.GetSection("MigrationSettings") as MigrationConfiguration;
                }
                catch (Exception e)
                {

                    log.Debug("Error retrieving Migration Settings: " + e.StackTrace);
                    throw new ApplicationException("ConfigurationFactory::getMigrationConfiguration - error retrieving Migration settings:" + e.StackTrace);
                }
            }
            return migrationConfig;
        }

        static ConfigurationFactory()
        {
            //Get Logger
            log = LogManager.GetLogger(typeof(ConfigurationFactory));
            
        }

        #endregion


    } 




}

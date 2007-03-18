using System;
using System.Configuration;

using log4net;
using log4net.Config;

/*
 * <summary>This class is responsible for retrieving the configuration information in the application config and loading into
 * objects. It then returns these objects to the ConfigurationManager to be accessed by the rest of the application.</summary>
 * 
 * <author>imorti@tacitknowledge.com</author>
 * 
 */ 
namespace AutopatchNET.dotnet.com.tacitknowledge.util.migration.ADO.conf
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
            if (dbConfig != null)
            {

                try
                {

                    // Retrieve DB configuration settings from app.config file
                    dbConfig = null;// ConfigurationManager.GetSection("MigrationDatabaseSettings") as DBConfiguration;

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
            if (migrationConfig != null)
            {
                try
                {
                    migrationConfig = null; // ConfigurationManager.GetSection("MigrationSettings") as MigrationConfiguration;
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

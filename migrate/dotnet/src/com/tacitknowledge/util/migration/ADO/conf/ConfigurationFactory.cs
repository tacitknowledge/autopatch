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

        #endregion

        #region Methods

        public DBConfiguration getDBConfiguration()
        {
            try
            {
                // Retrieve DB configuration settings from app.config file
                DBConfiguration configData = ConfigurationManager.GetSection("MigrationDatabaseSettings") as DBConfiguration;
            }
            catch (Exception e)
            {

                log.Fatal("Error retrieving MigrationDatabaseSettings: " + e.ToString);

            }
            return configData;
            }

        public MigrationConfiguration getMigrationConfiguration()
        {

            try
            {
                MigrationConfiguration configData = ConfigurationManager.GetSection("MigrationSettings") as MigrationConfiguration;
            }
            catch(Exception e) {

                log.Debug("Error retrieving MigrationSettings: " + e.ToString);
                
            }

            return configData;

         }


        static ConfigurationFactory()
        {
            //Get Logger
            log = LogManager.GetLogger(typeof(ConfigurationFactory));
        }

        #endregion


    } 




}

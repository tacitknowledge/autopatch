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
using log4net;
using log4net.Config;
using com.tacitknowledge.util.migration.ado.conf;
#endregion

namespace com.tacitknowledge.util.migration
{
    /// <summary>
    /// The purpose of this class is to be called from the Global.asax file and initiate Migration tasks.
    /// </summary>
    /// <version>$Id$</version>
    class AutoPatchEventListener
    {
        #region Members

        /*
         * Keep track of first run. Should be moot at this point as this is called from Global.asax file at appplication
         * start up, however for debugging purposes this could be useful. 
         */
        //private static bool firstRun = true;

        /*
         * Log4Net logger
         */ 
        private static ILog log;

        private ConfigurationManager configMgr;

        private MigrationConfiguration migrationConfig;

        #endregion

        #region Methods
      
        /*
         * This method initializes all AutoPatch processing at start up time for the application.
         *After it retrieves the configuration, it loads the class the application is configured to use for Migration.
         */
        public void initialize()
        {

            AutoPatchLauncher launcher;

            /*
             * Load up configuration through configuration manager
             */
            getConfiguration();


            /*
             * Load the class we're configured to use for managing migrations
             */
            launcher = null;// System.Activator.CreateInstance(AutopatchNET, String(migrationConfig.Launcher));

            launcher.initialize();



        }
        /*
         *  This method will handle any post processing necessary. 
         */
        public void destroy()
        {
        }


        /* <summary>
       * This method retrieves configuration information from the app.config file 
       *</summary>
       */
        private void getConfiguration()
        {
            configMgr = new ConfigurationManager();
            migrationConfig = configMgr.getMigrationConfiguration();

        }

        static AutoPatchEventListener()
        {
            log = LogManager.GetLogger(typeof(AutoPatchEventListener));
        }

        #endregion
    }
}

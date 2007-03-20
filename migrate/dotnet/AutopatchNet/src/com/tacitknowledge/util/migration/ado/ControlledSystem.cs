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
using log4net;
using log4net.Config;
using com.tacitknowledge.util.migration.ado;
#endregion

namespace com.tacitknowledge.util.migration.ado
{   
    /// <summary>
    /// An entity object representing a controlled system and the MigrationLauncher used to control said system
    /// </summary>
    public class ControlledSystem
    {


        #region Members
        /// <summary>
        /// Name of the system we're controlling
        /// </summary>
        private System.String controlledSystemName;

        /// <summary>
        /// The AdoMigrationLauncher object for the system we're controlling
        /// </summary>
        private AdoMigrationLauncher adoMigrationLauncher;
        #endregion

        #region Methods
        /// <summary>
        /// Returns/sets the name of the system we're controlling
        /// </summary>
        public System.String ControlledSystemName
        {
            get { return controlledSystemName; }
            set { controlledSystemName = value; }
        }
       
        /// <summary>
        /// Returns/sets the AdoMigrationLauncher for this ControlledSystem
        /// </summary>
        public AdoMigrationLauncher AdoMigrationLauncher
        {
            get { return adoMigrationLauncher; }
            set { adoMigrationLauncher = value; }
        }

        #endregion



    }
}

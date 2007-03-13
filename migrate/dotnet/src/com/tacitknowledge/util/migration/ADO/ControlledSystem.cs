using System;
using System.Collections.Generic;
using System.Text;

using log4net;
using log4net.Config;

using com.tacitknowledge.util.migration.ado;

namespace AutopatchNET.src.com.tacitknowledge.util.migration.ADO
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
        /// The ADOMigrationLauncher object for the system we're controlling
        /// </summary>
        private ADOMigrationLauncher adoMigrationLauncher;
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
        /// Returns/sets the ADOMigrationLauncher for this ControlledSystem
        /// </summary>
        public ADOMigrationLauncher AdoMigrationLauncher
        {
            get { return adoMigrationLauncher; }
            set { adoMigrationLauncher = value; }
        }

        #endregion



    }
}

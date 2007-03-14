using System;

using System.Configuration;

namespace AutopatchNET.dotnet.com.tacitknowledge.util.migration.ADO.conf
{
    /*
     * This class contains configuration information for the AutoPatchEventListener.
     * By obtaining this class from the configuration manager, the AutoPatchEventListener
     * can determine which launcher to use at runtime.
     */
    class MigrationConfiguration : ConfigurationSection
    {


     

        #region Methods
        /*
         * The Launcher we should be using initially
         * 
         */
        [ConfigurationProperty("launcher")]
        public String Launcher
        {
            get { return (string)this["launcher"]; }
            set { this["launcher"] = value; }
        }
        [ConfigurationProperty("systemname")]
        public String SystemName
        {
            get { return (string) this["systemname"];}
            set { this["systemname"] = value;}
        }
        [ConfigurationProperty("patchpath")]
        public String PatchPath
        {
            get { return (string)this["patchpath"]; }
            set { this["patchpath"] = value; }
        }
        [ConfigurationProperty("postpatchpath")]
        public String PostPatchPath
        {
            get { return (string)this["postpatchpath"]; }
            set { this["postpatchpath"] = value; }
        }

        #endregion

    }
}

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
#endregion

namespace com.tacitknowledge.util.migration.ado.conf
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

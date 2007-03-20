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
    /* <summary>
     * Contains information to connect to DB to check Patch levels.
     * <author>imortimer@tacitknowledge.com</author>
     * </summary>
     */
    class DBConfiguration : ConfigurationSection
    {
        #region Members


       

        #endregion


        #region Methods
        /*
         * Returns the provider name to use for connection
         * 
         */
        [ConfigurationProperty("type")]
        public String DatabaseType
        {
            get { return (string)this["type"]; }
            set { this["type"] = value; }
        }


        /*
         * Returns the server name.
         * 
         */
        [ConfigurationProperty("server")]
        public String Server
        {
            get { return (string)this["server"]; }
            set { this["server"] = value; }
        }

       
        /*
         * Returns the username
         * 
         */
        [ConfigurationProperty("username")]
        public String Username
        {
            get { return (string)this["username"]; }
            set { this["username"] = value; }
        }

        /*
         * Returns the password
         * 
         */
        [ConfigurationProperty("password")]
        public String Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }


        #endregion

    }
}

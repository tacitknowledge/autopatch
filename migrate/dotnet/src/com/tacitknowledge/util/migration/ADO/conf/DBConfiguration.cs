using System;
using System.Configuration;

namespace AutopatchNET.dotnet.com.tacitknowledge.util.migration.ADO.conf
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

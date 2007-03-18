/* Copyright 2006 Tacit Knowledge LLC
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


using AutopatchNET.dotnet.com.tacitknowledge.util.migration.ADO.conf;

using com.tacitknowledge.util.migration;

#endregion
namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Creates and configures a new <code>ADOMigrationContext</code> based on the values
	/// in the <em>migration.properties</em> file for the given system.  This is a convenience
	/// class for systems that need to initialize the autopatch framework but do not to or can not
	/// configure the framework themselves.
	/// </summary>
	/// <author>  Scott Askew (scott@tacitknowledge.com)
	/// </author>
	public class ADOMigrationLauncherFactory
    {

        #region Members
        /// <summary>Class logger </summary>
        //UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.ADOMigrationLauncherFactory'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
        private static ILog log;
        #endregion

        #region Methods
        /// <summary> Get a DataSourceMigrationContext
		/// 
		/// </summary>
		/// <returns> DataSourceMigrationContext for use with the launcher
		/// </returns>
		public DataSourceMigrationContext DataSourceMigrationContext
		{
			get
			{
				return new DataSourceMigrationContext();
			}
			
		}
		/// <summary> Get a ADOMigrationLauncher
		/// 
		/// </summary>
		/// <returns> ADOMigrationLauncher
		/// </returns>
		 public ADOMigrationLauncher ADOMigrationLauncher
		{
			get
			{
				return new ADOMigrationLauncher();
			}
			
		}
		
		/// <summary> Creates and configures a new <code>ADOMigrationLauncher</code> based on the
		/// values in the <em>MigrationConfiguration object</em> file for the given system.
		/// 
		/// </summary>
		/// <param name="systemName">the system to patch
		/// </param>
		/// <returns> a fully configured <code>ADOMigrationLauncher</code>.
		/// </returns>
		/// <throws>  MigrationException if an unexpected error occurs </throws>
		public ADOMigrationLauncher createMigrationLauncher(System.String systemName)
		{
			log.Info("Creating ADOMigrationLauncher for system " + systemName);
			ADOMigrationLauncher launcher = ADOMigrationLauncher;
			configureFromMigrationProperties(launcher, systemName);
			return launcher;
		}
		
		
		
		/// <summary> Loads the configuration from the migration config properties file.
		/// 
		/// </summary>
		/// <param name="launcher">the launcher to configure
		/// </param>
		/// <param name="systemName">the name of the system
		/// </param>
		/// <throws>  MigrationException if an unexpected error occurs </throws>
		private void  configureFromMigrationProperties(ADOMigrationLauncher launcher, System.String systemName)
		{
			//TODO: alter this to use MigrationConfigurationManager
		}
		
		/// <summary> Configure the launcher from the provided properties, system name
		/// 
		/// </summary>
		/// <param name="launcher">The launcher to configure
		/// </param>
		/// <param name="systemName">The name of the system we're configuring
		/// </param>
		/// <param name="props">The Properties object with our configuration information
		/// </param>
		/// <throws>  IllegalArgumentException if a required parameter is missing </throws>
		/// <throws>  MigrationException if there is problem setting the context into the launcher </throws>
		
		private void  configureFromMigrationProperties(ADOMigrationLauncher launcher, System.String systemName, System.Collections.Specialized.NameValueCollection props)
		{
            //TODO: change to use MigrationConfigurationManager
            launcher.PatchPath = null;//getRequiredParam(props, systemName + ".patch.path");
			launcher.PostPatchPath = props.Get(systemName + ".postpatch.path");
			
			// Set up the data source
			/*NonPooledDataSource dataSource = new NonPooledDataSource();
			dataSource.DriverClass = getRequiredParam(props, systemName + ".ado.driver");
			dataSource.DatabaseUrl = getRequiredParam(props, systemName + ".ado.url");
			dataSource.Username = getRequiredParam(props, systemName + ".ado.username");
			dataSource.Password = getRequiredParam(props, systemName + ".ado.password");*/
			
			// Set up the ADO migration context; accepts one of two property names
			DataSourceMigrationContext context = DataSourceMigrationContext;
            //TODO: change to use MigrationConfigurationManager
            System.String databaseType = null;// getRequiredParam(props, systemName + ".ado.database.type", systemName + ".ado.dialect");
			context.setDatabaseType(new DatabaseType(databaseType));
			
			// Finish setting up the context
			context.setSystemName(systemName);
			
			//context.DataSource = dataSource;
			
			// done reading in config, set launcher's context
			launcher.Context = context;
		}
		
		/// <summary> Returns the value of the specified configuration parameter.
		/// 
		/// </summary>
		/// <param name="props">the properties file containing the values
		/// </param>
		/// <param name="param">the parameter to return
		/// </param>
		/// <returns> the value of the specified configuration parameter
		/// </returns>
		/// <throws>  IllegalArgumentException if the parameter does not exist </throws>
		//UPGRADE_ISSUE: Class hierarchy differences between 'java.util.Properties' and 'System.Collections.Specialized.NameValueCollection' may cause compilation errors. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1186'"
		//TODO: Alter parameter retrieval to use MigrationConfigurationManager
       /* public static System.String getRequiredParam(System.Collections.Specialized.NameValueCollection props, System.String param)
		{
			System.String value_Renamed = props.Get(param);
			if (value_Renamed == null)
			{
				System.Console.Error.WriteLine("Parameter named: " + param + " was not found.");
				System.Console.Error.WriteLine("-----Parameters found-----");
				System.Collections.IEnumerator propNameIterator = new SupportClass.HashSetSupport(props).GetEnumerator();
				//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratorhasNext'"
				while (propNameIterator.MoveNext())
				{
					//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratornext'"
					System.String name = (System.String) propNameIterator.Current;
					System.String val = props.Get(name);
					System.Console.Error.WriteLine(name + " = " + val);
				}
				System.Console.Error.WriteLine("--------------------------");
				throw new System.ArgumentException("'" + param + "' is a required " + "initialization parameter.  Aborting.");
			}
			return value_Renamed;
		}*/
		
		/// <summary> Returns the value of the specified configuration parameter.
		/// 
		/// </summary>
		/// <param name="props">the properties file containing the values
		/// </param>
		/// <param name="param">the parameter to return
		/// </param>
		/// <param name="alternate">the alternate parameter to return
		/// </param>
		/// <returns> the value of the specified configuration parameter
		/// </returns>
		//TODO: Alter class to use MigrationConfigurationManager
		/*public static System.String getRequiredParam(System.Collections.Specialized.NameValueCollection props, System.String param, System.String alternate)
		{
			try
			{
				return getRequiredParam(props, param);
			}
			catch (System.ArgumentException e1)
            {
                log.Debug("ArgumentException in getRequiredParam: " + e1.StackTrace);
				try
				{
					return getRequiredParam(props, alternate);
				}
				catch (System.ArgumentException e2)
				{
					throw new System.ArgumentException("Either '" + param + "' or '" + alternate + "' must be specified as an initialization parameter.  Aborting.");
				}
			}
		}
		*/
		/// <summary> Returns the value of the specified configuration parameter.
		/// 
		/// </summary>
		/// <param name="param">the parameter to return
		/// </param>
		/// <returns> the value of the specified servlet context initialization parameter
		/// </returns>
		/// <throws>  IllegalArgumentException if the parameter does not exist </throws>
        //TODO: use ConfigurationManager to retrieve a required parameter
		/*private System.String getRequiredParam(System.String param)
		{
			/*
             * 
             */ 
			/*
			if (value_Renamed == null)
			{
				throw new System.ArgumentException("'" + param + "' is a required " + "servlet context initialization parameter for the \"" + GetType().FullName + "\" class.  Aborting.");
			}
			return value_Renamed;
		}*/
		static ADOMigrationLauncherFactory()
		{
			log = LogManager.GetLogger(typeof(ADOMigrationLauncherFactory));
		}
    }
        #endregion
}
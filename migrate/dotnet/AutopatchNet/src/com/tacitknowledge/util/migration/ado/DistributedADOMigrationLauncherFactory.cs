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
using com.tacitknowledge.util.migration;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Creates and configures a new <code>DistributedADOMigrationContext</code> based on the values
	/// in the <em>migration.properties</em> file for the given system.  This is a convenience
	/// class for systems that need to initialize the autopatch framework but do not to or can not
	/// configure the framework themselves.
	/// <p>
	/// This factory expects a file named <code>migration.properties</code> to be in the 
	/// root of the  class path.  This file must contain these properties (where <i>systemName</i> 
	/// is the name of the system being patched):
	/// <table>
	/// <tr><th>Key</th><th>description</th></tr>
	/// <tr><td><i>systemName</i>.context</td><td>The context to use for orchestration</td></tr>
	/// <tr><td><i>systemName</i>.controlled.systems</td><td>comma-delimited systems to manage</td></tr>
	/// </table>
	/// <p>
	/// For each system in the controlled systems list, the properties file should contain
	/// information as directed in the documenation for ADOMigrationLauncher.
	/// 
	/// </summary>
	/// <seealso cref="ADOMigrationLauncher">
	/// </seealso>
	/// <author>  Mike Hardy (mike@tacitknowledge.com)
	/// </author>
	public class DistributedADOMigrationLauncherFactory:ADOMigrationLauncherFactory
	{
		/// <summary> Get a new DistributedADOMigrationLauncher
		/// 
		/// </summary>
		/// <returns> DistributedADOMigrationLauncher
		/// </returns>
		virtual public DistributedADOMigrationLauncher DistributedADOMigrationLauncher
		{
			get
			{
				return new DistributedADOMigrationLauncher();
			}
			
		}
		/// <summary>Class logger </summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.DistributedADOMigrationLauncherFactory'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
		
		/// <summary> Creates and configures a new <code>ADOMigrationLauncher</code> based on the
		/// values in the <em>migration.properties</em> file for the given system.
		/// 
		/// </summary>
		/// <param name="systemName">the system to patch
		/// </param>
		/// <returns> a fully configured <code>DistributedADOMigrationLauncher</code>.
		/// </returns>
		/// <throws>  MigrationException if an unexpected error occurs </throws>
		public new ADOMigrationLauncher createMigrationLauncher(System.String systemName)
		{
			log.Info("Creating DistributedADOMigrationLauncher for system " + systemName);
			DistributedADOMigrationLauncher launcher = DistributedADOMigrationLauncher;
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
		private void  configureFromMigrationProperties(DistributedADOMigrationLauncher launcher, System.String systemName)
		{
			//UPGRADE_ISSUE: Class 'java.lang.ClassLoader' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javalangClassLoader'"
			//UPGRADE_ISSUE: Method 'java.lang.Thread.getContextClassLoader' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javalangThreadgetContextClassLoader'"
			//ClassLoader cl = SupportClass.ThreadClass.Current().getContextClassLoader();
			//UPGRADE_ISSUE: Method 'java.lang.ClassLoader.getResourceAsStream' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javalangClassLoader'"
            System.IO.Stream is_Renamed = null;// cl.getResourceAsStream(com.tacitknowledge.util.migration.MigrationContext_Fields.MIGRATION_CONFIG_FILE);
			if (is_Renamed != null)
			{
				try
				{
					//UPGRADE_ISSUE: Class hierarchy differences between 'java.util.Properties' and 'System.Collections.Specialized.NameValueCollection' may cause compilation errors. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1186'"
					//UPGRADE_TODO: Format of property file may need to be changed. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1089'"
					System.Collections.Specialized.NameValueCollection props = new System.Collections.Specialized.NameValueCollection();
					//UPGRADE_TODO: Method 'java.util.Properties.load' was converted to 'System.Collections.Specialized.NameValueCollection' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilPropertiesload_javaioInputStream'"
					props = new System.Collections.Specialized.NameValueCollection(System.Configuration.ConfigurationSettings.AppSettings);
					
					configureFromMigrationProperties(launcher, systemName, props);
				}
				catch (System.IO.IOException e)
				{
					throw new MigrationException("Error reading in migration properties file", e);
				}
				finally
				{
					try
					{
						is_Renamed.Close();
					}
					catch (System.IO.IOException ioe)
					{
						throw new MigrationException("Error closing migration properties file", ioe);
					}
				}
			}
			else
			{
				throw new MigrationException("Unable to find migration properties file '"/* TODO + com.tacitknowledge.util.migration.MigrationContext_Fields.MIGRATION_CONFIG_FILE*/ + "'");
			}
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
		//UPGRADE_ISSUE: Class hierarchy differences between 'java.util.Properties' and 'System.Collections.Specialized.NameValueCollection' may cause compilation errors. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1186'"
		private void  configureFromMigrationProperties(DistributedADOMigrationLauncher launcher, System.String systemName, System.Collections.Specialized.NameValueCollection props)
		{
			// Get the name of the context to use for our patch information
            System.String patchStoreContextName = null;//getRequiredParam(props, systemName + ".context");
			
			// Set up the data source
			/* TODO
			dataSource.DriverClass = getRequiredParam(props, patchStoreContextName + ".ado.driver");
			dataSource.DatabaseUrl = getRequiredParam(props, patchStoreContextName + ".ado.url");
			dataSource.Username = getRequiredParam(props, patchStoreContextName + ".ado.username");
			dataSource.Password = getRequiredParam(props, patchStoreContextName + ".ado.password");
			*/
			// Get any post-patch task paths
			launcher.PostPatchPath = props.Get(patchStoreContextName + ".postpatch.path");
			
			// Set up the ADO migration context; accepts one of two property names
			DataSourceMigrationContext context = DataSourceMigrationContext;
            System.String databaseType = null;// getRequiredParam(props, patchStoreContextName + ".ado.database.type", patchStoreContextName + ".ado.dialect");
			context.setDatabaseType(new DatabaseType(databaseType));
			
			// Finish setting up the context
			context.setSystemName(systemName);
			//context.DataSource = dataSource;
			
			// done reading in config, set launcher's context
			launcher.Context = context;
			
			// Get our controlled systems, and instantiate their launchers
			//UPGRADE_TODO: Class 'java.util.HashMap' was converted to 'System.Collections.Hashtable' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilHashMap'"
			System.Collections.Hashtable controlledSystems = new System.Collections.Hashtable();
            System.String[] controlledSystemNames = null;// getRequiredParam(props, systemName + ".controlled.systems").split(",");
			for (int i = 0; i < controlledSystemNames.Length; i++)
			{
				log.Info("Creating controlled migration launcher for system " + controlledSystemNames[i]);
				ADOMigrationLauncherFactory factory = ADOMigrationLauncherFactoryLoader.createFactory();
				ADOMigrationLauncher subLauncher = factory.createMigrationLauncher(controlledSystemNames[i]);
				controlledSystems[controlledSystemNames[i]] = subLauncher;
				
				// Make sure the controlled migration process gets migration events
				//launcher.MigrationProcess.addListener(subLauncher);
                launcher.MigrationProcess.MigrationStarted += new MigrationProcess.MigrationStatusEventHandler(subLauncher.MigrationStarted);
                launcher.MigrationProcess.MigrationSuccessful += new MigrationProcess.MigrationStatusEventHandler(subLauncher.MigrationSuccessful);
                launcher.MigrationProcess.MigrationFailed += new MigrationProcess.MigrationStatusEventHandler(subLauncher.MigrationFailed);
            }
			
			// communicate our new-found controlled systems to the migration process
            ((DistributedMigrationProcess)launcher.MigrationProcess).ControlledSystems = null;// controlledSystems;
		}
		static DistributedADOMigrationLauncherFactory()
		{
			log = LogManager.GetLogger(typeof(DistributedADOMigrationLauncherFactory));
		}
	}
}
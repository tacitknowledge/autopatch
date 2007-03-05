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
using log4net.Config;using MigrationContext = com.tacitknowledge.util.migration.MigrationContext;
using MigrationException = com.tacitknowledge.util.migration.MigrationException;
using NonPooledDataSource = com.tacitknowledge.util.migration.ado.util.NonPooledDataSource;
#endregion
namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Creates and configures a new <code>ADOMigrationContext</code> based on the values
	/// in the <em>migration.properties</em> file for the given system.  This is a convenience
	/// class for systems that need to initialize the autopatch framework but do not to or can not
	/// configure the framework themselves.
	/// <p>
	/// This factory expects a file named <code>migration.properties</code> to be in the root of the
	/// class path.  This file must contain these properties (where <i>systemName</i> is the name of
	/// the system being patched):
	/// <table>
	/// <tr><th>Key</th><th>description</th></tr>
	/// <tr><td><i>systemName</i>.patch.path</td><td></td></tr>
	/// <tr><td><i>systemName</i>.postpatch.path</td><td></td></tr>
	/// <tr><td><i>systemName</i>.ado.database.type</td>
	/// <td>The database type; also accepts <i>systemName</i>.ado.dialect</td></tr>
	/// <tr><td><i>systemName</i>.ado.driver</td><td>The ADO driver to use</td></tr>
	/// <tr><td><i>systemName</i>.ado.url</td><td>The ADO URL to the database</td></tr>
	/// <tr><td><i>systemName</i>.ado.username</td><td>The database user name</td></tr>
	/// <tr><td><i>systemName</i>.ado.password</td><td>The database password</td></tr>
	/// </table>
	/// 
	/// </summary>
	/// <author>  Scott Askew (scott@tacitknowledge.com)
	/// </author>
	public class ADOMigrationLauncherFactory
	{
		/// <summary> Get a DataSourceMigrationContext
		/// 
		/// </summary>
		/// <returns> DataSourceMigrationContext for use with the launcher
		/// </returns>
		virtual public DataSourceMigrationContext DataSourceMigrationContext
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
		virtual public ADOMigrationLauncher ADOMigrationLauncher
		{
			get
			{
				return new ADOMigrationLauncher();
			}
			
		}
		/// <summary>Class logger </summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.ADOMigrationLauncherFactory'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
		
		/// <summary> Creates and configures a new <code>ADOMigrationLauncher</code> based on the
		/// values in the <em>migration.properties</em> file for the given system.
		/// 
		/// </summary>
		/// <param name="systemName">the system to patch
		/// </param>
		/// <returns> a fully configured <code>ADOMigrationLauncher</code>.
		/// </returns>
		/// <throws>  MigrationException if an unexpected error occurs </throws>
		public virtual ADOMigrationLauncher createMigrationLauncher(System.String systemName)
		{
			log.info("Creating ADOMigrationLauncher for system " + systemName);
			ADOMigrationLauncher launcher = ADOMigrationLauncher;
			configureFromMigrationProperties(launcher, systemName);
			return launcher;
		}
		
		/// <summary> Creates and configures a new <code>ADOMigrationLauncher</code> based on the
		/// values in the servlet context and JNDI for a web-application.
		/// 
		/// </summary>
		/// <param name="sce">the name of the context event to use in getting properties
		/// </param>
		/// <returns> a fully configured <code>ADOMigrationLauncher</code>.
		/// </returns>
		/// <throws>  MigrationException if an unexpected error occurs </throws>
		//UPGRADE_ISSUE: Class 'javax.servlet.ServletContextEvent' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxservletServletContextEvent'"
		public virtual ADOMigrationLauncher createMigrationLauncher(ServletContextEvent sce)
		{
			ADOMigrationLauncher launcher = ADOMigrationLauncher;
			configureFromServletContext(launcher, sce);
			return launcher;
		}
		
		/// <summary> Used to configure the migration launcher with properties from a servlet 
		/// context.  You do not need migration.properties to use this method.
		/// 
		/// </summary>
		/// <param name="launcher">the launcher to configure
		/// </param>
		/// <param name="sce">the event to get the context and associated parameters from
		/// </param>
		/// <throws>  MigrationException if a problem with the look up in JNDI occurs </throws>
		//UPGRADE_ISSUE: Class 'javax.servlet.ServletContextEvent' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxservletServletContextEvent'"
		private void  configureFromServletContext(ADOMigrationLauncher launcher, ServletContextEvent sce)
		{
			DataSourceMigrationContext context = DataSourceMigrationContext;
			System.String systemName = getRequiredParam("migration.systemname", sce);
			context.setSystemName(systemName);
			
			System.String databaseType = getRequiredParam("migration.databasetype", sce);
			context.setDatabaseType(new DatabaseType(databaseType));
			
			System.String patchPath = getRequiredParam("migration.patchpath", sce);
			launcher.PatchPath = patchPath;
			
			//UPGRADE_ISSUE: Method 'javax.servlet.ServletContext.getInitParameter' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxservletServletContextgetInitParameter_javalangString'"
			//UPGRADE_ISSUE: Method 'javax.servlet.ServletContextEvent.getServletContext' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxservletServletContextEvent'"
			System.String postPatchPath = sce.getServletContext().getInitParameter("migration.postpatchpath");
			launcher.PostPatchPath = postPatchPath;
			
			System.String dataSource = getRequiredParam("migration.datasource", sce);
			try
			{
				//UPGRADE_TODO: Constructor 'javax.naming.InitialContext.InitialContext' was converted to 'System.DirectoryServices.DirectoryEntry' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaxnamingInitialContextInitialContext'"
				//UPGRADE_TODO: Adjust remoting context initialization manually. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1258'"
				System.DirectoryServices.DirectoryEntry ctx = new System.DirectoryServices.DirectoryEntry();
				if (ctx == null)
				{
					throw new System.ArgumentException("A jndi context must be " + "present to use this configuration.");
				}
				//UPGRADE_ISSUE: Interface 'javax.sql.DataSource' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxsqlDataSource'"
				//UPGRADE_TODO: Method 'javax.naming.Context.lookup' was converted to 'System.Activator.GetObject' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaxnamingContextlookup_javalangString'"
				DataSource ds = (DataSource) Activator.GetObject(typeof(System.MarshalByRefObject), SupportClass.ParseURILookup("java:comp/env/" + dataSource));
				context.DataSource = ds;
				launcher.Context = context;
			}
			//UPGRADE_NOTE: Exception 'javax.naming.NamingException' was converted to 'System.Exception' which has different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1100'"
			catch (System.Exception e)
			{
				throw new MigrationException("Problem with JNDI look up of " + dataSource, e);
			}
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
			//UPGRADE_ISSUE: Class 'java.lang.ClassLoader' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javalangClassLoader'"
			//UPGRADE_ISSUE: Method 'java.lang.Thread.getContextClassLoader' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javalangThreadgetContextClassLoader'"
			ClassLoader cl = SupportClass.ThreadClass.Current().getContextClassLoader();
			//UPGRADE_ISSUE: Method 'java.lang.ClassLoader.getResourceAsStream' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javalangClassLoader'"
			System.IO.Stream is_Renamed = cl.getResourceAsStream(com.tacitknowledge.util.migration.MigrationContext_Fields.MIGRATION_CONFIG_FILE);
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
				throw new MigrationException("Unable to find migration properties file '" + com.tacitknowledge.util.migration.MigrationContext_Fields.MIGRATION_CONFIG_FILE + "'");
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
		private void  configureFromMigrationProperties(ADOMigrationLauncher launcher, System.String systemName, System.Collections.Specialized.NameValueCollection props)
		{
			launcher.PatchPath = getRequiredParam(props, systemName + ".patch.path");
			launcher.PostPatchPath = props.Get(systemName + ".postpatch.path");
			
			// Set up the data source
			NonPooledDataSource dataSource = new NonPooledDataSource();
			dataSource.DriverClass = getRequiredParam(props, systemName + ".ado.driver");
			dataSource.DatabaseUrl = getRequiredParam(props, systemName + ".ado.url");
			dataSource.Username = getRequiredParam(props, systemName + ".ado.username");
			dataSource.Password = getRequiredParam(props, systemName + ".ado.password");
			
			// Set up the ADO migration context; accepts one of two property names
			DataSourceMigrationContext context = DataSourceMigrationContext;
			System.String databaseType = getRequiredParam(props, systemName + ".ado.database.type", systemName + ".ado.dialect");
			context.setDatabaseType(new DatabaseType(databaseType));
			
			// Finish setting up the context
			context.setSystemName(systemName);
			
			context.DataSource = dataSource;
			
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
		public static System.String getRequiredParam(System.Collections.Specialized.NameValueCollection props, System.String param)
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
		}
		
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
		//UPGRADE_ISSUE: Class hierarchy differences between 'java.util.Properties' and 'System.Collections.Specialized.NameValueCollection' may cause compilation errors. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1186'"
		public static System.String getRequiredParam(System.Collections.Specialized.NameValueCollection props, System.String param, System.String alternate)
		{
			try
			{
				return getRequiredParam(props, param);
			}
			catch (System.ArgumentException e1)
			{
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
		
		/// <summary> Returns the value of the specified servlet context initialization parameter.
		/// 
		/// </summary>
		/// <param name="param">the parameter to return
		/// </param>
		/// <param name="sce">the <code>ServletContextEvent</code> being handled
		/// </param>
		/// <returns> the value of the specified servlet context initialization parameter
		/// </returns>
		/// <throws>  IllegalArgumentException if the parameter does not exist </throws>
		//UPGRADE_ISSUE: Class 'javax.servlet.ServletContextEvent' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxservletServletContextEvent'"
		private System.String getRequiredParam(System.String param, ServletContextEvent sce)
		{
			//UPGRADE_ISSUE: Method 'javax.servlet.ServletContextEvent.getServletContext' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxservletServletContextEvent'"
			System.Web.HttpApplicationState context = sce.getServletContext();
			//UPGRADE_ISSUE: Method 'javax.servlet.ServletContext.getInitParameter' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxservletServletContextgetInitParameter_javalangString'"
			System.String value_Renamed = context.getInitParameter(param);
			if (value_Renamed == null)
			{
				throw new System.ArgumentException("'" + param + "' is a required " + "servlet context initialization parameter for the \"" + GetType().FullName + "\" class.  Aborting.");
			}
			return value_Renamed;
		}
		static ADOMigrationLauncherFactory()
		{
			log = LogManager.GetLogger(typeof(ADOMigrationLauncherFactory));
		}
	}
}
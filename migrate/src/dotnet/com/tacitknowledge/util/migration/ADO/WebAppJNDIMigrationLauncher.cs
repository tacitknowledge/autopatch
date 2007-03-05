/* Copyright 2005 Tacit Knowledge LLC
*
* Licensed under the Tacit Knowledge Open License, Version 1.0 (the "License");
* you may not use this file except in compliance with the License. You may
* obtain a copy of the License at http://www.tacitknowledge.com/licenses-1.0.
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.*/
#region Imports
using System;
using log4net;
using log4net.Config;
using ClassDiscoveryUtil = com.tacitknowledge.util.discovery.ClassDiscoveryUtil;
using WebAppResourceListSource = com.tacitknowledge.util.discovery.WebAppResourceListSource;
using MigrationException = com.tacitknowledge.util.migration.MigrationException;
#endregion
namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Used to configure the migration engine using JNDI and properties 
	/// set in the servlet context for a web application.  This newer class 
	/// removes the need to use a migration.properties file.  Instead, set the 
	/// following properties (context-param) in the web.xml file:
	/// 
	/// <ul>
	/// <li>migration.systemname - name of the system to update
	/// <li>migration.databasetype - ex: mysql
	/// <li>migration.patchpath - colon separated path to look for files
	/// <li>migration.datasource - ex: ado/clickstream
	/// </ul>
	/// All properties listed above are required.  
	/// 
	/// </summary>
	/// <author>  Chris A. (chris@tacitknowledge.com)
	/// </author>
	/// <version>  $Id$
	/// </version>
	//UPGRADE_ISSUE: Interface 'javax.servlet.ServletContextListener' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxservletServletContextListener'"
	public class WebAppJNDIMigrationLauncher : ServletContextListener
	{
		/// <summary> Keeps track of the first run of the class within this web app deployment.
		/// This should always be true, but you can never be too careful.
		/// </summary>
		private static bool firstRun = true;
		
		/// <summary> Class logger</summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.WebAppJNDIMigrationLauncher'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
		
		/// <seealso cref="ServletContextListener.contextInitialized(ServletContextEvent)">
		/// </seealso>
		//UPGRADE_ISSUE: Class 'javax.servlet.ServletContextEvent' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxservletServletContextEvent'"
		public virtual void  contextInitialized(ServletContextEvent sce)
		{
			try
			{
				
				// WEB-INF/classes and WEB-INF/lib are not in the system classpath (as defined by
				// System.getProperty("java.class.path")); add it to the search path here
				if (firstRun)
				{
					ClassDiscoveryUtil.addResourceListSource(new WebAppResourceListSource(System.Web.HttpContext.Current.Server.MapPath("//WEB-INF")));
				}
				firstRun = false;
				
				// The MigrationLauncher is responsible for handling the interaction
				// between the PatchTable and the underlying MigrationTasks; as each
				// task is executed, the patch level is incremented, etc.
				try
				{
					ADOMigrationLauncherFactory launcherFactory = ADOMigrationLauncherFactoryLoader.createFactory();
					ADOMigrationLauncher launcher = launcherFactory.createMigrationLauncher(sce);
					launcher.doMigrations();
				}
				catch (MigrationException e)
				{
					// Runtime exceptions coming from a ServletContextListener prevent the
					// application from being deployed.  In this case, the intention is
					// for migration-enabled applications to fail-fast if there are any
					// errors during migration.
					throw new RuntimeException("Migration exception caught during migration", e);
				}
			}
			catch (System.SystemException e)
			{
				// Catch all exceptions for the sole reason of logging in
				// as many places as possible - debugging migration
				// problems requires detection first, and that means
				// getting the word of failures out.
				log.error(e);
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				System.Console.Out.WriteLine(e.Message);
				SupportClass.WriteStackTrace(e, System.Console.Out);
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				System.Console.Error.WriteLine(e.Message);
				SupportClass.WriteStackTrace(e, System.Console.Error);
				
				throw e;
			}
		}
		
		/// <seealso cref="ServletContextListener.contextDestroyed(ServletContextEvent)">
		/// </seealso>
		//UPGRADE_ISSUE: Class 'javax.servlet.ServletContextEvent' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxservletServletContextEvent'"
		public virtual void  contextDestroyed(ServletContextEvent sce)
		{
			log.debug("context is being destroyed " + sce);
		}
		static WebAppJNDIMigrationLauncher()
		{
			log = LogManager.GetLogger(typeof(WebAppJNDIMigrationLauncher));
		}
	}
}
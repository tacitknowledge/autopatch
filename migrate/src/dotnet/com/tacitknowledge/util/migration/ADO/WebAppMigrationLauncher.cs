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
* limitations under the License.
*/
#region Imports
using System;
using log4net;
using log4net.Config;using ClassDiscoveryUtil = com.tacitknowledge.util.discovery.ClassDiscoveryUtil;
using WebAppResourceListSource = com.tacitknowledge.util.discovery.WebAppResourceListSource;
using MigrationException = com.tacitknowledge.util.migration.MigrationException;
#endregion
namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Launches the migration process upon application context creation.  This class
	/// is intentionally fail-fast, meaning that it throws a RuntimeException if any
	/// problems arise during migration and will prevent the web application from
	/// being fully deployed.
	/// <p>
	/// This class expects the following servlet context init parameters:
	/// <ul>
	/// <li>migration.systemname - the name of the logical system being migrated</li>
	/// </ul>
	/// <p>
	/// Below is an example of how this class can be configured in web.xml:
	/// <pre>
	/// ...
	/// &lt;context-param&gt;
	/// &lt;param-name&gt;migration.systemname&lt;/param-name&gt;
	/// &lt;param-value&gt;milestone&lt;/param-value&gt;
	/// &lt;/context-param&gt;
	/// ...
	/// &lt;!-- immediately after the filter configs... --&gt;
	/// ...
	/// &lt;listener&gt;
	/// &lt;listener-class&gt;
	/// com.tacitknowledge.util.migration.patchtable.WebAppMigrationLauncher
	/// &lt;/listener-class&gt;
	/// &lt;/listener&gt;
	/// ...
	/// </pre> 
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	/// <version>  $Id$
	/// </version>
	/// <seealso cref="com.tacitknowledge.util.migration.MigrationProcess">
	/// </seealso>
	//UPGRADE_ISSUE: Interface 'javax.servlet.ServletContextListener' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javaxservletServletContextListener'"
	public class WebAppMigrationLauncher : ServletContextListener
	{
		/// <summary> Keeps track of the first run of the class within this web app deployment.
		/// This should always be true, but you can never be too careful.
		/// </summary>
		private static bool firstRun = true;
		
		/// <summary> Class logger</summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.WebAppMigrationLauncher'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
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
				
				System.String systemName = getRequiredParam("migration.systemname", sce);
				
				// The MigrationLauncher is responsible for handling the interaction
				// between the PatchTable and the underlying MigrationTasks; as each
				// task is executed, the patch level is incremented, etc.
				try
				{
					ADOMigrationLauncherFactory launcherFactory = ADOMigrationLauncherFactoryLoader.createFactory();
					ADOMigrationLauncher launcher = launcherFactory.createMigrationLauncher(systemName);
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
		static WebAppMigrationLauncher()
		{
			log = LogManager.GetLogger(typeof(WebAppMigrationLauncher));
		}
	}
}
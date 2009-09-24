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
using com.tacitknowledge.util.migration.ado.conf;
using MigrationException = com.tacitknowledge.util.migration.MigrationException;
using com.tacitknowledge.util.migration;
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

    public class WebAppMigrationLauncher : AutoPatchLauncher 
	{
		/// <summary> Keeps track of the first run of the class within this web app deployment.
		/// This should always be true, but you can never be too careful.
		/// </summary>
		//private static bool firstRun = true;
		
		/// <summary>
		/// Log object
		/// </summary>
		private static ILog log;
		
		/// <summary>
		/// 
		/// </summary>
		public override void  initialize()
		{
			try
			{
				//TODO: must create way to get System name from configuration. 
                MigrationConfigurationManager configMgr = new MigrationConfigurationManager();
                MigrationConfiguration migrationConfig = configMgr.getMigrationConfiguration();
				
				// The MigrationLauncher is responsible for handling the interaction
				// between the PatchTable and the underlying MigrationTasks; as each
				// task is executed, the patch level is incremented, etc.
				try
				{
					AdoMigrationLauncherFactory launcherFactory = AdoMigrationLauncherFactoryLoader.createFactory();
					AdoMigrationLauncher launcher = launcherFactory.createMigrationLauncher(migrationConfig.SystemName);
					launcher.DoMigrations();
                    //firstRun = false;
				}
				catch (MigrationException e)
				{
					// Runtime exceptions coming from a ServletContextListener prevent the
					// application from being deployed.  In this case, the intention is
					// for migration-enabled applications to fail-fast if there are any
					// errors during migration.
					throw new ApplicationException("Migration exception caught during migration", e);
				}
			}
			catch (System.SystemException e)
			{
				// Catch all exceptions for the sole reason of logging in
				// as many places as possible - debugging migration
				// problems requires detection first, and that means
				// getting the word of failures out.
				log.Error(e.StackTrace);
				
				System.Console.Out.WriteLine(e.Message);
				
				
				System.Console.Error.WriteLine(e.Message);
				
				
				throw e;
			}
		}

        public override void dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }		
		
		static WebAppMigrationLauncher()
		{
			log = LogManager.GetLogger(typeof(WebAppMigrationLauncher));
		}
	}
}
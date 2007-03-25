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
#endregion

namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Launches the migration process as a standalone application.  
	/// <p>
	/// This class expects the following Java environment parameters:
	/// <ul>
	/// <li>migration.systemname - the name of the logical system being migrated</li>
	/// </ul>
	/// <p>
	/// Alternatively, you can pass the migration system name on the command line as the 
	/// first argument.
	/// <p>
	/// Below is an example of how this class can be configured in an Ant build.xml file:
	/// <pre>
	/// ...
	/// &lt;target name="patch.information" description="Prints out information about patch levels"&gt;
	/// &lt;java 
	/// fork="true"
	/// classpathref="patch.classpath" 
	/// failonerror="true" 
	/// classname="com.tacitknowledge.util.migration.ado.DistributedMigrationInformation"&gt;
	/// &lt;sysproperty key="migration.systemname" value="${application.name}"/&gt;
	/// &lt;/java&gt;
	/// &lt;/target&gt;
	/// ...
	/// </pre> 
	/// 
	/// </summary>
	/// <author>   Mike Hardy (mike@tacitknowledge.com)
	/// </author>
	/// <seealso cref="com.tacitknowledge.util.migration.DistributedMigrationProcess">
	/// </seealso>
	/// <seealso cref="com.tacitknowledge.util.migration.ado.DistributedAdoMigrationLauncherFactory">
	/// </seealso>
	public class DistributedMigrationInformation
	{
		/// <summary>Class logger </summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.DistributedMigrationInformation'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
		
		/// <summary> Private constructor - this object shouldn't be instantiated</summary>
		private DistributedMigrationInformation()
		{
			// does nothing
		}
		
		/// <summary> Get the migration level information for the given system name
		/// 
		/// </summary>
		/// <param name="arguments">the command line arguments, if any (none are used)
		/// </param>
		/// <exception cref="Exception">if anything goes wrong
		/// </exception>
		[STAThread]
		public static void  Main(System.String[] arguments)
		{
			DistributedMigrationInformation info = new DistributedMigrationInformation();
			//UPGRADE_ISSUE: Method 'java.lang.System.getProperty' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javalangSystem'"
            System.String migrationName = null;// System_Renamed.getProperty("systemname");
			if (migrationName == null)
			{
				if ((arguments != null) && (arguments.Length > 0))
				{
					migrationName = arguments[0].Trim();
				}
				else
				{
					throw new System.ArgumentException("The migration.systemname " + "system property is required");
				}
			}
			info.getMigrationInformation(migrationName);
		}
		
		/// <summary> Get the migration level information for the given system name
		/// 
		/// </summary>
		/// <param name="systemName">the name of the system
		/// </param>
		/// <throws>  Exception if anything goes wrong </throws>
		public virtual int  getMigrationInformation(System.String systemName)
		{
			// The MigrationLauncher is responsible for handling the interaction
			// between the PatchTable and the underlying MigrationTasks; as each
			// task is executed, the patch level is incremented, etc.
			try
			{
				DistributedAdoMigrationLauncherFactory launcherFactory = new DistributedAdoMigrationLauncherFactory();
				DistributedAdoMigrationLauncher launcher = (DistributedAdoMigrationLauncher) launcherFactory.createMigrationLauncher(systemName);

                foreach (IAdoMigrationContext context in launcher.Contexts.Keys)
                {
                    int currentLevel = launcher.GetDatabasePatchLevel(context);
                    int nextPatchLevel = launcher.NextPatchLevel;
                    
                    log.Info("Current Database patch level is        : " + currentLevel);
                    
                    int unappliedPatches = nextPatchLevel - launcher.GetDatabasePatchLevel(context) - 1;
                    
                    log.Info("Current number of unapplied patches is : " + unappliedPatches);
                    log.Info("The next patch to author should be     : " + nextPatchLevel);
                    
                    // We are only interested in info about the first one
                    return (nextPatchLevel - 1);
                }

                return 0;
			}
			catch (System.Exception e)
			{
				log.Error(e);
				throw e;
			}
		}
		static DistributedMigrationInformation()
		{
			log = LogManager.GetLogger(typeof(DistributedMigrationInformation));
		}
	}
}
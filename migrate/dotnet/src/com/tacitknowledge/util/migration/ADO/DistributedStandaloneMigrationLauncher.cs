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
	/// Below is an example of how this class can be configured in build.xml:
	/// <pre>
	/// ...
	/// &lt;target name="patch.database" description="Runs the migration system"&gt;
	/// &lt;java 
	/// fork="true"
	/// classpathref="patch.classpath" 
	/// failonerror="true" 
	/// classname="com.tacitknowledge.util.migration.ado.DistributedStandaloneMigrationLauncher"&gt;
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
	public class DistributedStandaloneMigrationLauncher
	{
		/// <summary> Class logger</summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.DistributedStandaloneMigrationLauncher'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
		
		/// <summary> Private constructor - this object shouldn't be instantiated</summary>
		private DistributedStandaloneMigrationLauncher()
		{
			// does nothing
		}
		
		/// <summary> Run the migrations for the given system name
		/// 
		/// </summary>
		/// <param name="arguments">the command line arguments, if any (none are used)
		/// </param>
		/// <exception cref="Exception">if anything goes wrong
		/// </exception>
		[STAThread]
		public static void  Main(System.String[] arguments)
		{
			//UPGRADE_TODO: Method 'java.lang.System.getProperties' was converted to 'SupportClass.GetProperties' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangSystemgetProperties'"
            System.String systemName = null;//getRequiredParam("migration.systemname", SupportClass.GetProperties(), arguments);
			
			// The MigrationLauncher is responsible for handling the interaction
			// between the PatchTable and the underlying MigrationTasks; as each
			// task is executed, the patch level is incremented, etc.
			try
			{
				DistributedADOMigrationLauncherFactory factory = new DistributedADOMigrationLauncherFactory();
				ADOMigrationLauncher launcher = factory.createMigrationLauncher(systemName);
				launcher.doMigrations();
			}
			catch (System.Exception e)
			{
				log.Error(e);
				throw e;
			}
		}
		
		/// <summary> Returns the value of the specified servlet context initialization parameter.
		/// 
		/// </summary>
		/// <param name="param">the parameter to return
		/// </param>
		/// <param name="properties">the <code>Properties</code> for the Java system
		/// </param>
		/// <param name="arguments">optionally takes the arguments passed into the main to 
		/// use as the migration system name
		/// </param>
		/// <returns> the value of the specified system initialization parameter
		/// </returns>
		/// <throws>  IllegalArgumentException if the parameter does not exist </throws>
		//UPGRADE_ISSUE: Class hierarchy differences between 'java.util.Properties' and 'System.Collections.Specialized.NameValueCollection' may cause compilation errors. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1186'"
		private static System.String getRequiredParam(System.String param, System.Collections.Specialized.NameValueCollection properties, System.String[] arguments)
		{
			System.String value_Renamed = properties.Get(param);
			if (value_Renamed == null)
			{
				if ((arguments != null) && (arguments.Length > 0))
				{
					value_Renamed = arguments[0].Trim();
				}
				else
				{
					throw new System.ArgumentException("'" + param + "' is a required " + "initialization parameter.  Aborting.");
				}
			}
			return value_Renamed;
		}
		static DistributedStandaloneMigrationLauncher()
		{
			log = LogManager.GetLogger(typeof(DistributedStandaloneMigrationLauncher));
		}
	}
}
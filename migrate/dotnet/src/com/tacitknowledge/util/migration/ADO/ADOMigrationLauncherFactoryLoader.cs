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
	
	/// <summary> Load a MigrationLauncherFactory. This will default to loading the
	/// ADOMigrationLauncherFactory, but will examine the system properties
	/// for a property called "migration.factory" and load that one if specified
	/// 
	/// </summary>
	/// <author>  Jacques Morel
	/// </author>
	public class ADOMigrationLauncherFactoryLoader
	{
		/// <summary>Class logger </summary>
		private static ILog log;
		
		/// <summary> Create the ADOMigrationLauncherFactory
		/// 
		/// </summary>
		/// <returns> ADOMigrationLauncherFactory (or subclass)
		/// </returns>
		public static ADOMigrationLauncherFactory createFactory()
		{
			
			System.String factoryName = SupportClass.GetProperties().Get("migration.factory");
			log.Debug("Creating ADOMigrationLauncher using " + factoryName);
			if (factoryName == null)
			{
				factoryName = typeof(ADOMigrationLauncherFactory).FullName;
			}
			
			// Load the factory
			System.Type factoryClass = null;
			try
			{
				//UPGRADE_TODO: The differences in the format  of parameters for method 'java.lang.Class.forName'  may cause compilation errors.  "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1092'"
				factoryClass = System.Type.GetType(factoryName);
			}
			//UPGRADE_NOTE: Exception 'java.lang.ClassNotFoundException' was converted to 'System.Exception' which has different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1100'"
			catch (System.Exception e)
			{
				throw new System.ArgumentException("Migration factory class '" + factoryName + "' not found.  Aborting.");
			}
			try
			{
				return (ADOMigrationLauncherFactory) System.Activator.CreateInstance(factoryClass);
			}
			catch (System.Exception e)
			{
				throw new ApplicationException("Problem while instantiating factory class '" + factoryName + "'.  Aborting.", e);
			}
		}
		static ADOMigrationLauncherFactoryLoader()
		{
			log = LogManager.GetLogger(typeof(ADOMigrationLauncherFactoryLoader));
		}
	}
}
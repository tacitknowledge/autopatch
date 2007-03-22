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
	
	/// <summary> Support using AutoPatch via injection I think. Jacques Morel contributed this, 
	/// and I'm not sure what it does actually.
	/// 
	/// </summary>
	/// <author>  Jacques Morel
	/// </author>
	public class AutoPatchSupport
	{
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <summary> Get the current patch level
		/// 
		/// </summary>
		/// <returns> int with the patch level in the patch database
		/// </returns>
		/// <throws>  SQLException if there is a problem creating the patch table </throws>
		/// <throws>  MigrationException if there is a problem getting the patch level </throws>
		/// <summary> Set the patch level to the specified level
		/// 
		/// </summary>
		/// <param name="patchLevel">the level to set the patch table to
		/// </param>
		/// <throws>  SQLException if there is a problem creating the patch table </throws>
		/// <throws>  MigrationException if the store can't be locked </throws>
		virtual public int PatchLevel
		{
			get
			{
				PatchTable patchTable = makePatchTable();
				return patchTable.PatchLevel;
			}
			
			set
			{
				PatchTable patchTable = makePatchTable();
				patchTable.LockPatchStore();
				patchTable.UpdatePatchLevel(value);
				log.Info("Set the patch level to " + value);
				patchTable.UnlockPatchStore();
			}
			
		}
		/// <summary> Get the highest patch level of all the configured patches
		/// 
		/// </summary>
		/// <returns> int with the highest patch level
		/// </returns>
		/// <throws>  MigrationException if there is problem getting the patch level </throws>
		virtual public int HighestPatchLevel
		{
			get
			{
				return launcher.NextPatchLevel - 1;
			}
			
		}
		/// <summary>Class logger </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'log '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.AutoPatchSupport'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static readonly ILog log;
		
		/// <summary>The launcher we'll use </summary>
		private AdoMigrationLauncher launcher;
		
		/// <summary> Create a new support component for the given system name. 
		/// This will use create a factory for you.
		/// 
		/// </summary>
		/// <param name="systemName">the name of the system to be patched
		/// </param>
		/// <throws>  MigrationException if there is a problem </throws>
		/// <seealso cref="AdoMigrationLauncherFactoryLoader">
		/// </seealso>
		public AutoPatchSupport(System.String systemName):this(AdoMigrationLauncherFactoryLoader.createFactory(), systemName)
		{
		}
		
		/// <summary> Create support component with the given factory and system name
		/// 
		/// </summary>
		/// <param name="launcherFactory">the factory to use for migrations
		/// </param>
		/// <param name="systemName">the system to patch
		/// </param>
		/// <throws>  MigrationException if there is any problem </throws>
		public AutoPatchSupport(AdoMigrationLauncherFactory launcherFactory, System.String systemName):this(launcherFactory.createMigrationLauncher(systemName))
		{
		}
		
		/// <summary> Create a support component with the given configured launcher
		/// 
		/// </summary>
		/// <param name="launcher">the launcher to use for the migrations
		/// </param>
		public AutoPatchSupport(AdoMigrationLauncher launcher)
		{
			this.launcher = launcher;
		}
		
		/// <summary> Create the patch table (if necessary)
		/// 
		/// </summary>
		/// <returns> PatchTable object for the configured migration launcher
		/// </returns>
		/// <throws>  SQLException if there is a problem </throws>
		public virtual PatchTable makePatchTable()
		{
			IAdoMigrationContext ADOMigrationContext = launcher.Context;
			return new PatchTable(ADOMigrationContext, ADOMigrationContext.Connection);
		}
		static AutoPatchSupport()
		{
			log = LogManager.GetLogger(typeof(AutoPatchSupport));
		}
	}
}
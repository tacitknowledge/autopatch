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
using System.Collections.Generic;
using log4net;
using log4net.Config;
using DistributedMigrationProcess = com.tacitknowledge.util.migration.DistributedMigrationProcess;
using MigrationException = com.tacitknowledge.util.migration.MigrationException;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Creates an DistributedAutoPatch environment using a configuration supplied by dependency
	/// injection. Exports a hook that can be called to execute AutoPatch after configuration.
	/// 
	/// </summary>
	/// <author>  Mike Hardy (mike@tacitknowledge.com)
	/// </author>
	public class DistributedAutoPatchService:DistributedAdoMigrationLauncherFactory
	{
		/// <summary> Configure and return a DistributedAdoMigrationLauncher to use for patching
		/// 
		/// </summary>
		/// <returns> DistributedAdoMigrationLauncher configured from injected properties
		/// </returns>
		/// <exception cref="MigrationException">if there is a problem setting the database context
		/// </exception>
		virtual public DistributedAdoMigrationLauncher Launcher
		{
			get
			{
				DistributedAdoMigrationLauncher launcher = DistributedADOMigrationLauncher;
				launcher.AddContext(Context);
				
				// Grab the controlled systems and subjugate them
				//UPGRADE_TODO: Class 'java.util.HashMap' was converted to 'System.Collections.Hashtable' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilHashMap'"
				System.Collections.Hashtable controlledLaunchers = new System.Collections.Hashtable();
				for (int i = 0; i < controlledSystems.Length; i++)
				{
					AutoPatchService controlledSystem = controlledSystems[i];
					AdoMigrationLauncher subLauncher = controlledSystem.Launcher;
					
                    IEnumerator<IAdoMigrationContext> contexts =
                        ((IEnumerable<IAdoMigrationContext>)subLauncher.Contexts.Keys).GetEnumerator();

                    if (contexts.MoveNext())
                    {
                        controlledLaunchers.Add(contexts.Current.SystemName, subLauncher);
                    }
					
					// Make sure the controlled migration process gets migration events
					//launcher.MigrationProcess.addListener(subLauncher);
                    launcher.MigrationProcess.MigrationStarted += new MigrationProcess.MigrationStatusEventHandler(subLauncher.MigrationStarted);
                    launcher.MigrationProcess.MigrationSuccessful += new MigrationProcess.MigrationStatusEventHandler(subLauncher.MigrationSuccessful);
                    launcher.MigrationProcess.MigrationFailed += new MigrationProcess.MigrationStatusEventHandler(subLauncher.MigrationFailed);
				}

                ((DistributedMigrationProcess)launcher.MigrationProcess).ControlledSystems = null;// controlledLaunchers;
				launcher.PostPatchPath = PostPatchPath;
				
				return launcher;
			}
			
		}
		/// <summary> Configure and return a DataSourceMigrationContext from this object's
		/// injected properties
		/// 
		/// </summary>
		/// <returns> DataSourceMigrationContext configured from injected properties
		/// </returns>
		private DataSourceMigrationContext Context
		{
			get
			{
				DataSourceMigrationContext context = DataSourceMigrationContext;
				context.SystemName = SystemName;
				context.DatabaseType = new DatabaseType(DatabaseType);
				
				return context;
			}
			
		}
		
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <returns> Returns the systemName.
		/// </returns>
		/// <param name="systemName">The systemName to set.
		/// </param>
		virtual public System.String SystemName
		{
			get
			{
				return systemName;
			}
			
			set
			{
				this.systemName = value;
			}
			
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <returns> Returns the databaseType.
		/// </returns>
		/// <param name="dialect">The databaseType to set.
		/// </param>
		virtual public System.String DatabaseType
		{
			get
			{
				return databaseType;
			}
			
			set
			{
				this.databaseType = value;
			}
			
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <returns> the controlled AutoPatchService objects
		/// </returns>
		/// <summary> Takes an Array of AutoPatchService objects to control when patching </summary>
		/// <param name="controlledSystems">the AutoPatchService objects to control
		/// </param>
		virtual public AutoPatchService[] ControlledSystems
		{
			get
			{
				return controlledSystems;
			}
			
			set
			{
				this.controlledSystems = value;
			}
			
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <returns> Returns the postPatchPath.
		/// </returns>
		/// <param name="postPatchPath">The postPatchPath to set.
		/// </param>
		virtual public System.String PostPatchPath
		{
			get
			{
				return postPatchPath;
			}
			
			set
			{
				this.postPatchPath = value;
			}
			
		}
		/// <summary>Class logger </summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.DistributedAutoPatchService'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
		
		/// <summary>The name of the schema to patch </summary>
		private System.String systemName = null;
				
		/// <summary>The type of database </summary>
		private System.String databaseType = null;
		
		/// <summary>The AutoPatchServices this object should control </summary>
		private AutoPatchService[] controlledSystems = null;
		
		/// <summary>The patch to the post-patch tasks </summary>
		private System.String postPatchPath = null;
		
		/// <summary> Patches all of the databases in your distributed system, if necessary.
		/// 
		/// </summary>
		/// <throws>  MigrationException if an unexpected error occurs </throws>
		public virtual void  patch()
		{
			DistributedAdoMigrationLauncher launcher = Launcher;
			
			try
			{
				log.Info("Applying patches....");
				int patchesApplied = launcher.DoMigrations();
				log.Info("Applied " + patchesApplied + " " + (patchesApplied == 1?"patch":"patches") + ".");
			}
			catch (MigrationException e)
			{
				throw new MigrationException("Error applying patches", e);
			}
		}
		static DistributedAutoPatchService()
		{
			log = LogManager.GetLogger(typeof(AutoPatchService));
		}
	}
}
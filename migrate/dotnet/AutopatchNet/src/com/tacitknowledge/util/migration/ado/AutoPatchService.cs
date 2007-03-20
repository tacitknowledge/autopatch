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
using MigrationException = com.tacitknowledge.util.migration.MigrationException;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Creates an AutoPatch environment using a configuration supplied by dependency
	/// injection. Exports a hook that can be called to execute AutoPatch after configuration
	/// 
	/// </summary>
	/// <author>  Scott Askew (scott@tacitknowledge.com)
	/// </author>
	public class AutoPatchService:ADOMigrationLauncherFactory
    {

        #region Member Variables
        /// <summary>Class logger </summary>
        private static ILog log;

        /// <summary>The name of the schema to patch </summary>
        private System.String systemName = null;

        /// <summary>The type of database </summary>
        private System.String databaseType = null;

        /// <summary>The path to the SQL patches </summary>
        private System.String patchPath = null;

        /// <summary>The patch to the post-patch tasks </summary>
        private System.String postPatchPath = null;
        #endregion

        /// <summary> Configure and return a ADOMigrationLauncher to use for patching
		/// 
		/// </summary>
		/// <returns> ADOMigrationLauncher configured from injected properties
		/// </returns>
		/// <exception cref="MigrationException">if there is a problem setting the context
		/// </exception>
		virtual public ADOMigrationLauncher Launcher
		{
			get
			{
				ADOMigrationLauncher launcher = ADOMigrationLauncher;
				launcher.Context = Context;
				launcher.PatchPath = PatchPath;
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
		 protected internal DataSourceMigrationContext Context
		{
			get
			{
				DataSourceMigrationContext context = DataSourceMigrationContext;
				context.setSystemName(SystemName);
				context.setDatabaseType(new DatabaseType(DatabaseType));
				
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
		/// <returns> Returns the patchPath.
		/// </returns>
		/// <param name="patchPath">The patchPath to set.
		/// </param>
		virtual public System.String PatchPath
		{
			get
			{
				return patchPath;
			}
			
			set
			{
				this.patchPath = value;
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
		
		/// <summary> Patches the database, if necessary.
		/// 
		/// </summary>
		/// <throws>  MigrationException if an unexpected error occurs </throws>
		public virtual void  patch()
		{
			ADOMigrationLauncher launcher = Launcher;
			
			try
			{
				log.Info("Applying patches....");
				int patchesApplied = launcher.doMigrations();
				log.Info("Applied " + patchesApplied + " " + (patchesApplied == 1?"patch":"patches") + ".");
			}
			catch (MigrationException e)
			{
				throw new MigrationException("Error applying patches", e);
			}
		}
		static AutoPatchService()
		{
			log = LogManager.GetLogger(typeof(AutoPatchService));
		}
	}
}
/* 
* Copyright 2006 Tacit Knowledge LLC
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
using com.tacitknowledge.util.migration;


#endregion
namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Core starting point for a distributed database migration run.  This class obtains a connection
	/// to the orchestration database, checks its patch level, delegates the actual execution of the migration
	/// tasks to a <code>MigrationProcess</code> instance, and then commits and cleans everything
	/// up at the end.
	/// <p>
	/// This class is <b>NOT</b> threadsafe.
	/// 
	/// </summary>
	/// <author>  Mike Hardy (mike@tacitknowledge.com)
	/// </author>
	public class DistributedADOMigrationLauncher:ADOMigrationLauncher
	{
		/// <summary> Override the sub-class so we get a DistributedMigrationProcess instead of the
		/// normal one
		/// 
		/// </summary>
		/// <returns> DistributedMigrationProcess
		/// </returns>
		override public MigrationProcess NewMigrationProcess
		{
			get
			{
				return new DistributedMigrationProcess();
			}
			
		}
		/// <summary> Create a new MigrationProcess and add a SqlScriptMigrationTaskSource</summary>
		public DistributedADOMigrationLauncher():base()
		{
		}
		
		/// <summary> Create a new <code>MigrationLancher</code>.
		/// 
		/// </summary>
		/// <param name="context">the <code>ADOMigrationContext</code> to use.
		/// </param>
		/// <throws>  MigrationException if an unexpected error occurs </throws>
		public DistributedADOMigrationLauncher(ADOMigrationContext context):base(context)
		{
		}
		
		/// <summary> Starts the application migration process across all configured contexts
		/// 
		/// </summary>
		/// <returns> the number of patches applied
		/// </returns>
		/// <throws>  MigrationException if an unrecoverable error occurs during </throws>
		/// <summary>         the migration
		/// </summary>
		public override int doMigrations()
		{
			if (Context == null)
			{
				throw new MigrationException("You must configure a migration context");
			}
			
			//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
			System.Data.OleDb.OleDbConnection conn = null;
			try
			{
				conn = Context.Connection;
				return base.doMigrations(conn);
			}
			catch (System.Data.OleDb.OleDbException e)
			{
				throw new MigrationException("SqlException during migration", e);
			}
			finally
			{
				SqlUtil.close(conn, null, null);
			}
		}
	}
}
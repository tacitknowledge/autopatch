/* Copyright 2004 Tacit Knowledge LLC
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
#endregion
namespace com.tacitknowledge.util.migration
{
	
	
	/// <summary> Provides system resources to migration tasks. 
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	/// <version>  $Id$
	/// </version>
	public struct MigrationContext_Fields{
		/// <summary> The name of the migration configuration file</summary>
		public readonly static System.String MIGRATION_CONFIG_FILE = "migration.properties";
	}
	public interface MigrationContext
	{
		//UPGRADE_NOTE: Members of interface 'MigrationContext' were extracted into structure 'MigrationContext_Fields'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1045'"
		
		/// <summary> Commits the current migration transaction.
		/// 
		/// </summary>
		/// <throws>  MigrationException if there was an unrecoverable error committing </throws>
		/// <summary>         the transaction
		/// </summary>
		void  commit();
		
		/// <summary> Rolls back the current migration transaction. 
		/// 
		/// </summary>
		/// <throws>  MigrationException if there was an unrecoverable error committing </throws>
		/// <summary>         the transaction
		/// </summary>
		void  rollback();
	}
}
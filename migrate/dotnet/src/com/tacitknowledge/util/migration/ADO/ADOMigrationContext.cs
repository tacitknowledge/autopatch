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
using MigrationContext = com.tacitknowledge.util.migration.MigrationContext;
using MigrationException = com.tacitknowledge.util.migration.MigrationException;
#endregion
namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Contains the configuration and resources for a database patch run.  
	/// 
	/// </summary>
	/// <author>  Scott Askew (scott@tacitknowledge.com)
	/// </author>
	public struct ADOMigrationContext_Fields{
		/// <summary> Max length for the systemName columne</summary>
		public readonly static int MAX_SYSTEMNAME_LENGTH = 30;
	}
	public interface ADOMigrationContext:MigrationContext
	{
		//UPGRADE_NOTE: Members of interface 'ADOMigrationContext' were extracted into structure 'ADOMigrationContext_Fields'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1045'"
		/// <summary> Returns the database connection to use
		/// 
		/// </summary>
		/// <returns> the database connection to use
		/// </returns>
		/// <throws>  SQLException if an unexpected error occurs </throws>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
		System.Data.OleDb.OleDbConnection Connection
		{
			get;
			
		}
		
		/// <seealso cref="MigrationContext.commit()">
		/// </seealso>
		public void  commit();
		
		/// <seealso cref="MigrationContext.rollback()">
		/// </seealso>
		public void  rollback();
		
		/// <returns> the name of the system to patch
		/// </returns>
		System.String getSystemName();
		
		/// <returns> Returns the database type.
		/// </returns>
		DatabaseType getDatabaseType();
	}
}
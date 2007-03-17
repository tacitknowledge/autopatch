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

#endregion
namespace com.tacitknowledge.util.migration.ado
{
	
	
	public interface ADOMigrationContext:IMigrationContext
    {

        
        #region Methods
           
		/// <summary> Returns the database connection to use
		/// 
		/// </summary>
		/// <returns> the database connection to use
		/// </returns>
		/// <throws>  SQLException if an unexpected error occurs </throws>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
		System.Data.Common.DbConnection Connection
		{
			get;
			
		}
		
		
		
		/// <returns> the name of the system to patch
		/// </returns>
		String getSystemName();
		
		/// <returns> Returns the database type.
		/// </returns>
		DatabaseType getDatabaseType();

        
	}
#endregion
}
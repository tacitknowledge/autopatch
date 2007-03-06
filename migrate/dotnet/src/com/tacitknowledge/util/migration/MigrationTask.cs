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
#endregion
namespace com.tacitknowledge.util.migration
{
	
	/// <summary> A single, idempotent migration task.
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	public interface MigrationTask:System.IComparable
	{
		/// <summary> Performs a migration
		/// 
		/// </summary>
		/// <param name="context">the <code>MigrationContext</code> for this run
		/// </param>
		/// <throws>  MigrationException if an unexpected error occurred </throws>
		void  migrate(MigrationContext context);
		
		/// <summary> Returns the name of this migration task. 
		/// 
		/// </summary>
		/// <returns> the name of this migration task
		/// </returns>
		System.String getName();
		
		/// <summary> Returns the relative order in which this migration should occur.
		/// 
		/// </summary>
		/// <returns> the relative order in which this migration should occur; may never
		/// return <code>null</code>
		/// </returns>
		System.Int32 getLevel();
	}
}
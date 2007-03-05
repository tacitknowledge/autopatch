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
	
	/// <summary> A source of <code>MigrationTask</code>s.
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	/// <version>  $Id$
	/// </version>
	public interface MigrationTaskSource
	{
		/// <summary> Returns a list of <code>MigrationTasks</code> that are in the given
		/// package.
		/// 
		/// </summary>
		/// <param name="packageName">to package to search for migration tasks
		/// </param>
		/// <returns> a list of migration tasks; if not tasks were found, then an empty
		/// list must be returned.
		/// </returns>
		/// <throws>  MigrationException if an unrecoverable error occurs </throws>
		System.Collections.IList getMigrationTasks(System.String packageName);
	}
}
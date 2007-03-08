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
	
	/// <summary> Receives notifications regarding migration task migrations.
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	public interface MigrationListener
    {

        #region Methods
        /// <summary> Notifies the listener that the given task is about to start execution.
		/// 
		/// </summary>
		/// <param name="task">the recently finished task
		/// </param>
		/// <param name="context">the migration context
		/// </param>
		/// <throws>  MigrationException if an unrecoverable error occurs </throws>
		void  migrationStarted(MigrationTask task, MigrationContext context);
		
		/// <summary> Notifies the listener that the given task has completed execution. 
		/// 
		/// </summary>
		/// <param name="task">the recently finished task
		/// </param>
		/// <param name="context">the migration context
		/// </param>
		/// <throws>  MigrationException if an unrecoverable error occurs </throws>
		void  migrationSuccessful(MigrationTask task, MigrationContext context);
		
		/// <summary> Notifies the listener that the given task has completed execution. 
		/// 
		/// </summary>
		/// <param name="task">the recently finished task
		/// </param>
		/// <param name="context">the migration context
		/// </param>
		/// <param name="e">the <code>MigrationException</code> thrown by the task
		/// </param>
		/// <throws>  MigrationException if an unrecoverable error occurs </throws>
		void  migrationFailed(MigrationTask task, MigrationContext context, MigrationException e);
    }
        #endregion

}
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
	
	/// <summary> Interface for the persistence of information related to the patch level
	/// of the system, as well as whether patches are currently being applied
	/// 
	/// </summary>
	/// <author>  Mike Hardy (mike@tacitknowledge.com)
	/// </author>
	public interface PatchInfoStore
	{
		/// <summary> Returns the current patch level of the system
		/// 
		/// </summary>
		/// <returns> the current patch level of the system
		/// </returns>
		/// <exception cref="MigrationException">if it is not possible to get the patch level
		/// </exception>
		int PatchLevel
		{
			get;
			
		}
		/// <summary> Determines if the patch store is already locked
		/// 
		/// </summary>
		/// <returns> <code>true</code> if the patch store is already locked
		/// </returns>
		/// <exception cref="MigrationException">if checking for the lock fails
		/// </exception>
		bool PatchStoreLocked
		{
			get;
			
		}
		/// <summary> Creates the patch storage area if it has not been done before
		/// 
		/// </summary>
		/// <exception cref="MigrationException">if creation is unsuccessful
		/// </exception>
		void  createPatchStoreIfNeeded();
		
		/// <summary> Updates the system patch level to the specified value
		/// 
		/// </summary>
		/// <param name="level">the new system patch level
		/// </param>
		/// <exception cref="MigrationException">if updating the patch level failed
		/// </exception>
		void  updatePatchLevel(int level);
		
		/// <summary> Places a lock for this system on the patch store
		/// 
		/// </summary>
		/// <exception cref="MigrationException">if locking the store fails
		/// </exception>
		/// <exception cref="IllegalStateException">if a lock already exists
		/// </exception>
		void  lockPatchStore();
		
		/// <summary> Removes any locks for this system on the patch store
		/// 
		/// </summary>
		/// <exception cref="MigrationException">if unlocking the store fails 
		/// </exception>
		void  unlockPatchStore();
	}
}
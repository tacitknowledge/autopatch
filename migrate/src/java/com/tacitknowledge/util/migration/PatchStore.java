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
package com.tacitknowledge.util.migration;

/**
 * Interface for the persistence of information related to the patch level
 * of the system, as well as whether patches are currently being applied
 * 
 * @author Mike Hardy (mike@tacitknowledge.com)
 */
public interface PatchStore
{
    /**
     * Creates the patch storage area if it has not been done before
     * 
     * @exception MigrationException if creation is unsuccessful
     */
    public void createPatchStoreIfNeeded() throws MigrationException;
    
    /**
     * Returns the current patch level of the system
     *
     * @return the current patch level of the system
     * @exception MigrationException if it is not possible to get the patch level
     */
    public int getPatchLevel() throws MigrationException;
    
    /**
     * Updates the system patch level to the specified value
     * 
     * @param level the new system patch level
     * @exception MigrationException if updating the patch level failed
     */
    public void updatePatchLevel(int level) throws MigrationException;
    
    /**
     * Determines if the patch store is already locked
     * 
     * @return <code>true</code> if the patch store is already locked
     * @exception MigrationException if checking for the lock fails
     */
    public boolean isPatchStoreLocked() throws MigrationException;
    
    /**
     * Places a lock for this system on the patch store
     * 
     * @exception MigrationException if locking the store fails
     * @exception IllegalStateException if a lock already exists
     */
    public void lockPatchStore() throws MigrationException;
    
    /**
     * Removes any locks for this system on the patch store
     * 
     * @exception MigrationException if unlocking the store fails 
     */
    public void unlockPatchStore() throws MigrationException;
    
}

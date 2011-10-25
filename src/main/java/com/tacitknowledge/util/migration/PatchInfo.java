/* Copyright 2011 Tacit Knowledge
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
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
 * of the system, as well as whether patches were applied
 *
 * @author Ulises Pulido (ulises@tacitknowledge.com)
 */

public interface PatchInfo {

    /**
     * Returns the current patch level of the system
     *
     * @return the current patch level of the system
     * @throws MigrationException if it is not possible to get the patch level
     */
    public int getPatchLevel() throws MigrationException;

    /**
     * Determines if a given patch has been applied in the system
     *
     * @throws MigrationException if unlocking the store fails
     */
    public boolean isPatchApplied(int patchLevel) throws MigrationException;

}

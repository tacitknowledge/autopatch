/* Copyright 2004 Tacit Knowledge
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


import org.apache.commons.collections.CollectionUtils;

import java.util.Collections;
import java.util.List;
import java.util.Set;

public class MissingPatchMigrationRunnerStrategy implements MigrationRunnerStrategy{

    public boolean shouldMigrationRun(int migrationLevel, PatchInfoStore patchInfoStore) throws MigrationException {

        if (patchInfoStore == null) {
            throw new IllegalArgumentException("Patch Info Store should not be null");
        }

        return !patchInfoStore.isPatchApplied(migrationLevel);
    }

    public boolean isSynchronized(PatchInfoStore currentPatchInfoStore, PatchInfoStore patchInfoStore) throws MigrationException {

        if( currentPatchInfoStore == null || patchInfoStore == null ){
            throw new IllegalArgumentException("currentPatchInfoStore and patchInfoStore should not be null");
        }
        Set<Integer> currentPatchInfoStorePatchesApplied = currentPatchInfoStore.getPatchesApplied();
        Set<Integer> patchInfoStorePatchesApplied = patchInfoStore.getPatchesApplied();

        return currentPatchInfoStorePatchesApplied.equals(patchInfoStorePatchesApplied);
    }

    public void getRollbackCandidates(List migrationTasksForRollback, int[] rollbackLevels, PatchInfoStore currentPatchInfoStore) throws MigrationException {
        //TODO Adjust accordingly to this strategy (currently it is the same as in OrderedMigrationRunnerStrategy)
        int rollbackLevel = rollbackLevels[0];
        int currentPatchLevel = currentPatchInfoStore.getPatchLevel();

        if (currentPatchLevel < rollbackLevel)
        {
            throw new IllegalArgumentException(
                    "The rollback patch level cannot be greater than the current patch level");
        }

        PatchRollbackPredicate rollbackPredicate = new PatchRollbackPredicate(currentPatchLevel,
                rollbackLevel);
        CollectionUtils.filter(migrationTasksForRollback, rollbackPredicate);
        Collections.sort(migrationTasksForRollback);
         // need to reverse the list do we apply the rollbacks in descending
        // order
        Collections.reverse(migrationTasksForRollback);

    }
}

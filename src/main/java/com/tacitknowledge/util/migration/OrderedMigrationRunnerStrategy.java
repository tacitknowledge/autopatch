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

/**
 *
 * Helps to get information about how the migrations should run based in an ordered stategy, i.e.
 * If the level of the <code>MigrationTask</code> is greater than the current level and
 * the <code>MigrationTask</code>has not yet been applied. Then the information of that migration
 * is considered missing.
 *
 * @author Oscar Gonzalez (oscar@tacitknowledge.com)
 */
public class OrderedMigrationRunnerStrategy implements MigrationRunnerStrategy
{
    public boolean shouldMigrationRun(int migrationLevel, PatchInfoStore patchInfoStore) throws MigrationException {
        return shouldMigrationRun(migrationLevel, patchInfoStore.getPatchLevel());
    }

    public boolean shouldMigrationRun(int migrationLevel, int currentLevel) {
        return migrationLevel > currentLevel;
    }
}

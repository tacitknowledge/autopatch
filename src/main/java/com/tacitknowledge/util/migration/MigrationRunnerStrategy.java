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
 * Dictates the methods that different algorithms should run when we
 * try to apply patches or get information about the patches that need to be run.
 *
 * @author Oscar Gonzalez (oscar@tacitknowledge.com)
 */

public interface MigrationRunnerStrategy
{
    /**
     * Determines if a <code>MigrationTask</code> is able to run.
      * @param migrationLevel of the <code>MigrationTask</code> to be check as in int
     * @param currentLevel of the migrations as an int
     * @return boolean value telling us if we should run the migration or not.
     */
   public boolean shouldMigrationRun(int migrationLevel , int currentLevel);
}

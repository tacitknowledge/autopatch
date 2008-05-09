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
 * A single, idempotent and migration task, which also supports rollbacks.
 * 
 * @author  Artie Pesh-Imam (apeshimam@tacitknowledge.com)
 */
public interface RollbackableMigrationTask extends MigrationTask {

	/**
	 * Performs a migration
	 * 
	 * @param context the <code>MigrationContext</code> for this run.
	 * @throws MigrationException if an unexpected error occurs
	 */
    public void up(MigrationContext context) throws MigrationException;

    /**
     * Performs a rollback
     * @param context the <code>MigrationContext</code> for this run.
     * @throws MigrationException if an unexpected error occurrs
     */
    public void down(MigrationContext context) throws MigrationException;

    /**
     * Returns a boolean indicating if this task can be rolled back.
     * 
     * @return a boolean indicating if the task can be rolled back.
     */
    public boolean isRollbackSupported();
}

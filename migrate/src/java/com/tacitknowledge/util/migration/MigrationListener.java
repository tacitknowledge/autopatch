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
 * Receives notifications regarding migration task migrations.
 * 
 * @author  Scott Askew (scott@tacitknowledge.com)
 */
public interface MigrationListener
{
    /**
     * Notifies the listener that the given task is about to start execution.
     * 
     * @param  task the recently finished task
     * @param  context the migration context
     * @throws MigrationException if an unrecoverable error occurs
     */
    public void migrationStarted(MigrationTask task, MigrationContext context)
        throws MigrationException;
    
    /**
     * Notifies the listener that the given task has completed execution. 
     * 
     * @param  task the recently finished task
     * @param  context the migration context
     * @throws MigrationException if an unrecoverable error occurs
     */
    public void migrationSuccessful(MigrationTask task, MigrationContext context)
        throws MigrationException;

    /**
     * Notifies the listener that the given task has completed execution. 
     * 
     * @param  task the recently finished task
     * @param  context the migration context
     * @param  e the <code>MigrationException</code> thrown by the task
     * @throws MigrationException if an unrecoverable error occurs
     */
    public void migrationFailed(MigrationTask task,
        MigrationContext context, MigrationException e) throws MigrationException;

}

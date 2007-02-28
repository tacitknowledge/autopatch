/* 
 * Copyright 2007 Tacit Knowledge LLC
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

import com.mockrunner.jdbc.JDBCTestCaseAdapter;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationListener;
import com.tacitknowledge.util.migration.MigrationTask;

/**
 * All the stuff you need to write a test that is a MigrationListener
 * 
 * @author  Mike Hardy (mike@tacitknowledge.com)
 */
public class MigrationListenerTestBase extends JDBCTestCaseAdapter implements MigrationListener
{
    /** the count of times "migration started" was broadcast */
    private int migrationStartedCount = 0;
    
    /** the count of times "migration success" was broadcast */
    private int migrationSuccessCount = 0;
    
    /** the count of times "migration failed" was broadcast */
    private int migrationFailedCount = 0;
    
    /**
     * Constructor for MigrationTest.
     * 
     * @param name the name of the test to run
     */
    public MigrationListenerTestBase(String name)
    {
        super(name);
    }
    
    /**
     * Implements the migration started listener
     *
     * @param task the task that ran
     * @param con the context for the task
     */
    public void migrationStarted(MigrationTask task, MigrationContext con)
    {
        setMigrationStartedCount(getMigrationStartedCount() + 1);
    }
    
    /**
     * Implements the migration succeeded listener
     *
     * @param task the task that ran
     * @param con the context for the task
     */
    public void migrationSuccessful(MigrationTask task, MigrationContext con)
    {
        setMigrationSuccessCount(getMigrationSuccessCount() + 1);
    }
    
    /**
     * Implements the migration failed listener
     *
     * @param task the task that ran
     * @param con the context for the task
     * @param exception the exception that ocurred
     */
    public void migrationFailed(MigrationTask task, 
                                MigrationContext con, 
                                MigrationException exception)
    {
        setMigrationFailedCount(getMigrationFailedCount() + 1);
    }
    
    /**
     * Reset all of the counters
     */
    public void resetMigrationListenerState()
    {
        setMigrationFailedCount(0);
        setMigrationStartedCount(0);
        setMigrationSuccessCount(0);
    }
    
    /**
     * @return Returns the migrationFailedCount.
     */
    public int getMigrationFailedCount()
    {
        return migrationFailedCount;
    }
    
    /**
     * @param migrationFailedCount The migrationFailedCount to set.
     */
    public void setMigrationFailedCount(int migrationFailedCount)
    {
        this.migrationFailedCount = migrationFailedCount;
    }
    
    /**
     * @return Returns the migrationStartedCount.
     */
    public int getMigrationStartedCount()
    {
        return migrationStartedCount;
    }
    
    /**
     * @param migrationStartedCount The migrationStartedCount to set.
     */
    public void setMigrationStartedCount(int migrationStartedCount)
    {
        this.migrationStartedCount = migrationStartedCount;
    }
    
    /**
     * @return Returns the migrationSuccessCount.
     */
    public int getMigrationSuccessCount()
    {
        return migrationSuccessCount;
    }
    
    /**
     * @param migrationSuccessCount The migrationSuccessCount to set.
     */
    public void setMigrationSuccessCount(int migrationSuccessCount)
    {
        this.migrationSuccessCount = migrationSuccessCount;
    }
    
    /**
     * Get JUnit to shut up.
     */
    public void testNoOp()
    {
        // does nothing
    }
}

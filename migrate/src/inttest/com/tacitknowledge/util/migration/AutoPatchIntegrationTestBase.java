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

import com.tacitknowledge.util.migration.jdbc.DistributedJdbcMigrationLauncherFactory;
import com.tacitknowledge.util.migration.jdbc.JdbcMigrationLauncher;

import junit.framework.TestCase;

/**
 * Superclass for other integration test cases. Sets up a test database etc.
 * 
 * @author  Mike Hardy (mike@tacitknowledge.com)
 */
public abstract class AutoPatchIntegrationTestBase extends TestCase
{
    /** The DistributedLauncher we're testing */
    protected JdbcMigrationLauncher launcher = null;
    
    /**
     * Constructor 
     * 
     * @param name the name of the test to run
     */
    public AutoPatchIntegrationTestBase(String name)
    {
        super(name);
    }
    
    /**
     * Sets up a test database
     * 
     * @exception Exception if anything goes wrong
     */
    public void setUp() throws Exception
    {
        super.setUp();
        DistributedJdbcMigrationLauncherFactory launcherFactory =
            new DistributedJdbcMigrationLauncherFactory();
        launcher = launcherFactory.createMigrationLauncher("integration_test", 
                                                           "inttest-migration.properties");
    }
    
    /**
     * Tears down the test database
     * 
     * @exception Exception if anything goes wrong
     */
    public void tearDown() throws Exception
    {
        super.tearDown();
    }
}

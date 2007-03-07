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

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.Statement;

import com.tacitknowledge.util.migration.jdbc.DistributedJdbcMigrationLauncher;
import com.tacitknowledge.util.migration.jdbc.DistributedJdbcMigrationLauncherFactory;
import com.tacitknowledge.util.migration.jdbc.JdbcMigrationLauncher;
import com.tacitknowledge.util.migration.jdbc.JdbcMigrationLauncherFactory;

import junit.framework.TestCase;

/**
 * Superclass for other integration test cases. Sets up a test database etc.
 * 
 * @author  Mike Hardy (mike@tacitknowledge.com)
 */
public abstract class AutoPatchIntegrationTestBase extends TestCase
{
    /** The DistributedLauncher we're testing */
    protected DistributedJdbcMigrationLauncher distributedLauncher = null;
    
    /** A regular launcher we can test */
    protected JdbcMigrationLauncher launcher = null;
    
    /** A multi-node launcher we can test */
    protected JdbcMigrationLauncher multiNodeLauncher = null;
    
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
        DistributedJdbcMigrationLauncherFactory dlFactory =
            new DistributedJdbcMigrationLauncherFactory();
        distributedLauncher = 
            (DistributedJdbcMigrationLauncher)dlFactory.createMigrationLauncher("integration_test", 
                                                                                "inttest-migration.properties");
        
        JdbcMigrationLauncherFactory lFactory = new JdbcMigrationLauncherFactory();
        launcher = lFactory.createMigrationLauncher("orders", "inttest-migration.properties");
        multiNodeLauncher = lFactory.createMigrationLauncher("catalog", "inttest-migration.properties");
    }
    
    /**
     * Tears down the test database
     * 
     * @exception Exception if anything goes wrong
     */
    public void tearDown() throws Exception
    {
        super.tearDown();
        destroyDatabase("core");
        destroyDatabase("orders");
        destroyDatabase("catalog1");
        destroyDatabase("catalog2");
    }
    
    /**
     * Destroys a database so a future test can use it
     * 
     * @param database the name of the database to destroy
     * @exception Exception if anything goes wrong
     */
    private void destroyDatabase(String database) throws Exception
    {
        Connection conn = DriverManager.getConnection("jdbc:hsqldb:mem:" + database, "sa", "");
        Statement stmt = conn.createStatement();
        stmt.execute("SHUTDOWN");
    }
}

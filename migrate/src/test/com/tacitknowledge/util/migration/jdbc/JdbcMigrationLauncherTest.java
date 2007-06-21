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
package com.tacitknowledge.util.migration.jdbc;

import java.util.Iterator;
import java.util.Map;

import org.easymock.MockControl;

import com.mockrunner.jdbc.PreparedStatementResultSetHandler;
import com.mockrunner.mock.jdbc.MockConnection;
import com.mockrunner.mock.jdbc.MockResultSet;
import com.tacitknowledge.util.migration.MigrationListenerTestBase;
import com.tacitknowledge.util.migration.PatchInfoStore;
import com.tacitknowledge.util.migration.jdbc.util.ConnectionWrapperDataSource;

/**
 * Exercise the data source migration context
 *
 * @author Mike Hardy <mailto:mike@tacitknowledge.com/>
 */
public class JdbcMigrationLauncherTest extends MigrationListenerTestBase
{
    /** The mock JDBC connection to use during the tests */
    private MockConnection conn = null;
    
    /** The launcher we're testing */
    private JdbcMigrationLauncher launcher = null;
    
    /**
     * The <code>JDBCMigrationConteext</code> used for testing
     */
    private DataSourceMigrationContext context = new DataSourceMigrationContext(); 
    
    /**
     * constructor that takes a name
     *
     * @param name of the test to run
     */
    public JdbcMigrationLauncherTest(String name)
    {
        super(name);
    }
    
    /**
     * @see com.mockrunner.jdbc.JDBCTestCaseAdapter#setUp()
     */
    protected void setUp() throws Exception
    {
        super.setUp();
        
        // Set up a mock context for some of the tests
        conn = getJDBCMockObjectFactory().getMockConnection();
        context = new DataSourceMigrationContext();
        context.setDataSource(new ConnectionWrapperDataSource(conn));
        context.setSystemName("milestone");
        context.setDatabaseType(new DatabaseType("postgres"));
        
        // Set up a complete launcher from a known properties file for some other tests
        
        // Make sure we load our test launcher factory, which fakes out the data source context
        System.getProperties()
           .setProperty("migration.factory", 
                        "com.tacitknowledge.util.migration.jdbc.TestJdbcMigrationLauncherFactory");
        JdbcMigrationLauncherFactory factory = new TestJdbcMigrationLauncherFactory();
        
        // Create the launcher (this does configure it as a side-effect)
        launcher = factory.createMigrationLauncher("catalog");
        
        // Make sure we get notification of any migrations
        launcher.getMigrationProcess().addListener(this);
    }
    
    /**
     * Verify that the configuration for multi-node made it through
     * 
     * @exception Exception if there is a problem
     */
    public void testMultiNodeConfiguration() throws Exception
    {
        Map contexts = launcher.getContexts();
        assertEquals(2, contexts.size());
        Iterator contextIter = contexts.keySet().iterator();
        JdbcMigrationContext context1 = (JdbcMigrationContext) contextIter.next();
        JdbcMigrationContext context2 = (JdbcMigrationContext) contextIter.next();
        assertNotSame(context1, context2);
        assertEquals("sybase", context1.getDatabaseType().getDatabaseType());
        assertEquals("postgres", context2.getDatabaseType().getDatabaseType());
    }
    
    /**
     * Test doing migrations
     * 
     * @exception Exception if there is a problem
     */
    public void testDoMigrationsWithLockRace() throws Exception
    {
        // Setup enough for the first
        PreparedStatementResultSetHandler h = conn.getPreparedStatementResultSetHandler();
        MockResultSet rs = h.createResultSet();
        rs.addRow(new Integer[] {new Integer(0)});
        h.prepareGlobalResultSet(rs);
        
        
        MockControl mockControl = MockControl.createStrictControl(PatchInfoStore.class);
        PatchInfoStore patchStore = (PatchInfoStore) mockControl.getMock();
        
        // First they see if it is locked, and it is, so they spin
        patchStore.isPatchStoreLocked();
        mockControl.setReturnValue(true);
        
        // Second they see if it is locked again, and it isn't, so they try and fail and spin
        patchStore.isPatchStoreLocked();
        mockControl.setReturnValue(false);
        patchStore.getPatchLevel();
        mockControl.setReturnValue(0);
        patchStore.lockPatchStore();
        mockControl.setThrowable(new IllegalStateException("The table is already locked"));
        
        // Finally they see if it is locked again, and it isn't, and it works
        patchStore.isPatchStoreLocked();
        mockControl.setReturnValue(false);
        patchStore.getPatchLevel();
        mockControl.setReturnValue(2);
        patchStore.lockPatchStore();
        patchStore.getPatchLevel();
        mockControl.setReturnValue(2);
        patchStore.updatePatchLevel(4);
        patchStore.updatePatchLevel(5);
        patchStore.updatePatchLevel(6);
        patchStore.updatePatchLevel(7);
        patchStore.unlockPatchStore();
        mockControl.replay();
        
        TestJdbcMigrationLauncher testLauncher = new TestJdbcMigrationLauncher(context);
        testLauncher.setLockPollMillis(0);
        testLauncher.setLockPollRetries(4);
        testLauncher.setIgnoreMigrationSuccessfulEvents(false);
        testLauncher.setPatchStore(patchStore);
        testLauncher.setPatchPath("com.tacitknowledge.util.migration.tasks.normal");
        testLauncher.doMigrations();
        mockControl.verify();
    }

    /**
     * The Launcher to test
     * 
     * @return JdbcMigrationLauncher to test
     */
    public JdbcMigrationLauncher getLauncher()
    {
        return launcher;
    }

    /**
     * Set the Launcher to test
     * 
     * @param launcher JdbcMigrationLauncher to use for testing
     */
    public void setLauncher(JdbcMigrationLauncher launcher)
    {
        this.launcher = launcher;
    }
}

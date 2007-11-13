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
import java.util.LinkedHashMap;
import java.util.Map;

import org.easymock.MockControl;

import com.mockrunner.jdbc.PreparedStatementResultSetHandler;
import com.mockrunner.mock.jdbc.MockConnection;
import com.mockrunner.mock.jdbc.MockResultSet;
import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.MigrationListenerTestBase;
import com.tacitknowledge.util.migration.MigrationTask;
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
     * Test doing migrations with a lock race from a quick cluster
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
        mockControl.setReturnValue(2, MockControl.ONE_OR_MORE);
        patchStore.lockPatchStore();
        patchStore.getPatchLevel();
        mockControl.setReturnValue(2, MockControl.ONE_OR_MORE);
        patchStore.updatePatchLevel(4);
        patchStore.getPatchLevel();
        mockControl.setReturnValue(4);
        patchStore.updatePatchLevel(5);
        patchStore.getPatchLevel();
        mockControl.setReturnValue(5);
        patchStore.updatePatchLevel(6);
        patchStore.getPatchLevel();
        mockControl.setReturnValue(6);
        patchStore.updatePatchLevel(7);
        // getPatchLevel() not called, but in case code changes in future, it will report the
        // correct level.
        patchStore.getPatchLevel();
        mockControl.setReturnValue(7, MockControl.ZERO_OR_MORE);
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
     * Test doing migrations with a lock override
     * 
     * @exception Exception if there is a problem
     */
    public void testLockOverride() throws Exception
    {
        // Setup enough for the first
        PreparedStatementResultSetHandler h = conn.getPreparedStatementResultSetHandler();
        MockResultSet rs = h.createResultSet();
        rs.addRow(new Integer[] {new Integer(0)});
        h.prepareGlobalResultSet(rs);
        
        
        MockControl mockControl = MockControl.createStrictControl(PatchInfoStore.class);
        PatchInfoStore patchStore = (PatchInfoStore) mockControl.getMock();
        
        // First they see if it is locked three times, and it is, so they spin
        patchStore.isPatchStoreLocked();
        mockControl.setReturnValue(true);
        patchStore.isPatchStoreLocked();
        mockControl.setReturnValue(true);
        patchStore.isPatchStoreLocked();
        mockControl.setReturnValue(true);
        patchStore.isPatchStoreLocked();
        mockControl.setReturnValue(true);

        // after the third time, they unlock it
        patchStore.unlockPatchStore();
        
        // now the lock succeeds
        patchStore.isPatchStoreLocked();
        mockControl.setReturnValue(false);
        patchStore.getPatchLevel();
        mockControl.setReturnValue(2);
        patchStore.lockPatchStore();
        
        // now the migrations proceed
        patchStore.getPatchLevel();
        mockControl.setReturnValue(2, MockControl.ONE_OR_MORE);
        patchStore.updatePatchLevel(4);
        patchStore.getPatchLevel();
        mockControl.setReturnValue(4, MockControl.ONE_OR_MORE);
        patchStore.updatePatchLevel(5);
        patchStore.getPatchLevel();
        mockControl.setReturnValue(5, MockControl.ONE_OR_MORE);
        patchStore.updatePatchLevel(6);
        patchStore.getPatchLevel();
        mockControl.setReturnValue(6, MockControl.ONE_OR_MORE);
        patchStore.updatePatchLevel(7);
        // getPatchLevel() not called, but in case code changes in future, it will report the
        // correct level.
        patchStore.getPatchLevel();
        mockControl.setReturnValue(7, MockControl.ZERO_OR_MORE);
        patchStore.unlockPatchStore();
        mockControl.replay();
        
        TestJdbcMigrationLauncher testLauncher = new TestJdbcMigrationLauncher(context);
        testLauncher.setLockPollMillis(0);
        testLauncher.setLockPollRetries(3);
        testLauncher.setIgnoreMigrationSuccessfulEvents(false);
        testLauncher.setPatchStore(patchStore);
        testLauncher.setPatchPath("com.tacitknowledge.util.migration.tasks.normal");
        testLauncher.doMigrations();
        mockControl.verify();
    }
 
    /**
     * Test that when a migrationSuccessful event fires.  If the 
     * 'successful' patch level is less than the current patch level
     * for the context's that is updating, then do not blow away the
     * existing patch level.
     * This case occurs when running patching in force sync mode.
     * Example:
     * node 1 is at patch level 2
     * node 2 is out of sync (or patch level zero).
     * When node 2 is forcesync'ed, node2 applies patch 1, 
     * it succeeds and all migration listeners are notified.
     * If we do not protect the nodes, then node 1 would have it's patch level set
     * to 1.  Then when patch 2 is executed, node 1 now thinks it needs to apply
     * it, and chaos ensues.
     * @throws MigrationException 
     */
    public void testMigrationSuccessfulDoesNotOverWritePatchLevel() throws MigrationException
    {
        // not using the 'setup' method's initilization because I really just want
        // to unit test the migration successful method.
        launcher = new JdbcMigrationLauncher();
        
        int migrationSuccessfulPatchLevel = 1;
        int node1PatchLevel = 2;
        int node2PatchLevel = 0;
        
        // create mocks
        MockControl node1ContextControl = MockControl.createControl(JdbcMigrationContext.class);
        MockControl node2ContextControl = MockControl.createControl(JdbcMigrationContext.class);
        JdbcMigrationContext node1Context = (JdbcMigrationContext) node1ContextControl.getMock();
        JdbcMigrationContext node2Context = (JdbcMigrationContext) node2ContextControl.getMock();
        
        MockControl node1PatchInfoStoreControl = MockControl.createControl(PatchInfoStore.class);
        MockControl node2PatchInfoStoreControl = MockControl.createControl(PatchInfoStore.class);
        PatchInfoStore node1PatchInfoStore = (PatchInfoStore) node1PatchInfoStoreControl.getMock();
        PatchInfoStore node2PatchInfoStore = (PatchInfoStore) node2PatchInfoStoreControl.getMock();
        
        LinkedHashMap contexts = new LinkedHashMap();
        contexts.put(node1Context, node1PatchInfoStore);
        contexts.put(node2Context, node2PatchInfoStore);
        launcher.setContexts(contexts);
        
        MockControl taskControl = MockControl.createControl(MigrationTask.class);
        MockControl contextControl = MockControl.createControl(MigrationContext.class);
        MigrationTask task = (MigrationTask) taskControl.getMock();
        MigrationContext context = (MigrationContext) contextControl.getMock();
        
        // set expectations
        // the migration successful event is for patch level 1
        task.getLevel();
        taskControl.setDefaultReturnValue(new Integer(migrationSuccessfulPatchLevel));
        task.getName();
        taskControl.setDefaultReturnValue("patch001_test.sql");
        
        taskControl.replay();

        // node1 is at patchlevel 2
        node1PatchInfoStore.getPatchLevel();
        node1PatchInfoStoreControl.setDefaultReturnValue(node1PatchLevel);
        
        node1PatchInfoStoreControl.replay();

        // node2 is at patchlevel 1
        node2PatchInfoStore.getPatchLevel();
        node2PatchInfoStoreControl.setDefaultReturnValue(node2PatchLevel);
        node2PatchInfoStore.updatePatchLevel(migrationSuccessfulPatchLevel);
        node2PatchInfoStoreControl.setVoidCallable();
        
        node2PatchInfoStoreControl.replay();
        
        // test and validate
        launcher.migrationSuccessful(task, context);
        taskControl.verify();
        node1PatchInfoStoreControl.verify();
        node2PatchInfoStoreControl.verify();
    }
}

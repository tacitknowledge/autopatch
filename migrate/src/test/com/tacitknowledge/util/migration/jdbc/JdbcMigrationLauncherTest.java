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
package com.tacitknowledge.util.migration.jdbc;

import org.easymock.MockControl;

import com.mockrunner.jdbc.JDBCTestCaseAdapter;
import com.mockrunner.jdbc.PreparedStatementResultSetHandler;
import com.mockrunner.mock.jdbc.MockConnection;
import com.mockrunner.mock.jdbc.MockResultSet;
import com.tacitknowledge.util.migration.PatchInfoStore;
import com.tacitknowledge.util.migration.jdbc.util.ConnectionWrapperDataSource;

/**
* Exercise the data source migration context
*
* @author Mike Hardy <mailto:mike@tacitknowledge.com/>
*/
public class JdbcMigrationLauncherTest extends JDBCTestCaseAdapter
{
    /**
     * The mock JDBC connection to use during the tests
     */
    private MockConnection conn = null;
    
    /**
     * The <code>JDBCMigrationConteext</code> used for testing
     */
    private DataSourceMigrationContext context = new DataSourceMigrationContext(); 
    
    /**
     * @see com.mockrunner.jdbc.JDBCTestCaseAdapter#setUp()
     */
    protected void setUp() throws Exception
    {
        super.setUp();
        
        conn = getJDBCMockObjectFactory().getMockConnection();
        
        context = new DataSourceMigrationContext();
        context.setDataSource(new ConnectionWrapperDataSource(conn));
        context.setSystemName("milestone");
        context.setDatabaseType(new DatabaseType("postgres"));
    }
    
    /**
    * Test doing migrations
    */
    public void testDoMigrationsWithLockRace() throws Exception
    {
        // Setup enough for the first
        PreparedStatementResultSetHandler h = conn.getPreparedStatementResultSetHandler();
        MockResultSet rs = h.createResultSet();
        rs.addRow(new Integer[] {new Integer(0)});
        h.prepareGlobalResultSet(rs);
        
        
        MockControl mockControl = MockControl.createControl(PatchInfoStore.class);
        PatchInfoStore patchStore = (PatchInfoStore)mockControl.getMock();
        
        // First they see if it is locked, and it is, so they spin
        patchStore.isPatchStoreLocked();
        mockControl.setReturnValue(true);
        
        // Second they see if it is locked again, and it isn't, so they try and fail and spin
        patchStore.isPatchStoreLocked();
        mockControl.setReturnValue(false);
        patchStore.lockPatchStore();
        mockControl.setThrowable(new IllegalStateException("The table is already locked in this test"));
        
        // Finally they see if it is locked again, and it isn't, and it works
        patchStore.isPatchStoreLocked();
        mockControl.setReturnValue(false);
        patchStore.getPatchLevel();
        mockControl.setReturnValue(2);
        patchStore.lockPatchStore();
        patchStore.getPatchLevel();
        mockControl.setReturnValue(2);
        patchStore.updatePatchLevel(3);
        patchStore.updatePatchLevel(4);
        patchStore.unlockPatchStore();
        mockControl.replay();
        
        
        
        TestJdbcMigrationLauncher launcher = new TestJdbcMigrationLauncher(context);
        launcher.setLockPollMillis(0);
        launcher.setPatchStore(patchStore);
        launcher.setPatchPath("com.tacitknowledge.util.migration.tasks.normal");
        launcher.doMigrations();
    }
}

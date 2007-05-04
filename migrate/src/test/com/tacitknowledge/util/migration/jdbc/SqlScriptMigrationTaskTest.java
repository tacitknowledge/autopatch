/* Copyright 2005 Tacit Knowledge LLC
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

import java.io.IOException;
import java.io.InputStream;
import java.sql.Connection;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.List;

import javax.sql.DataSource;

import org.easymock.MockControl;

import com.mockrunner.jdbc.JDBCTestCaseAdapter;
import com.mockrunner.mock.jdbc.MockConnection;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.jdbc.util.ConnectionWrapperDataSource;

/**
 * Tests the <code>SqlScriptMigrationTask</code>.
 *
 * @author Scott Askew (scott@tacitknowledge.com)
 */
public class SqlScriptMigrationTaskTest extends JDBCTestCaseAdapter
{
    /**
     * The task to test.
     */
    private SqlScriptMigrationTask task = null;

    /**
     * The mock JDBC connection to use during the tests
     */
    private MockConnection conn = null;
    
    /**
     * The <code>JDBCMigrationConteext</code> used for testing
     */
    private DataSourceMigrationContext context = new DataSourceMigrationContext(); 
    
    /**
     * {@inheritDoc}
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
     * Test doing a migration (with the connection silently succeeding)
     * 
     * @throws IOException if the test patch file doesn't load correctly
     */
    public void testMigrate() throws IOException
    {
        InputStream is = getClass().getResourceAsStream("test/patch0003_third_patch.sql");
        task = new SqlScriptMigrationTask("test", 1, is);
        is.close();
        
        try
        {
            task.migrate(context);
        }
        catch (MigrationException me)
        {
            fail("unexpected exception");
        }
    }
    
    /**
     * Ensures that the task can correctly parse multiple SQL statements from a single file,
     * with embedded comments. 
     * 
     * @throws IOException if an unexpected error occurs while attempting to read the 
     *         test SQL patch file; it's a system resource, so this shouldn't happen
     */
    public void testParsingMultipleStatement() throws IOException
    {
        InputStream is = getClass().getResourceAsStream("test/patch0001_first_patch.sql");
        task = new SqlScriptMigrationTask("test", 1, is);
        is.close();
        
        context.setDatabaseType(new DatabaseType("oracle"));
        List l = task.getSqlStatements(context);
        assertEquals(3, l.size());
        assertEquals("insert into user_role_assoc (user_role_id, application_user_id, "
            + "role_code, project_id) \n\t\t\tvalues (nextval('role_id_seq'),2, 'SYSA', 3)",
            l.get(0).toString());
        assertEquals("insert into user_role_assoc (user_role_id, application_user_id, "
            + "role_code, project_id) \n\t\t\tvalues (nextval('role_id_seq'),3, 'SYSA', 3)",
            l.get(1).toString());
        assertEquals("insert into user_role_assoc (user_role_id, application_user_id, "
            + "role_code, project_id) \n\t\t\tvalues (nextval('role_--id_seq;'),4, 'SYSA', 3)",
            l.get(2).toString());

        is = getClass().getResourceAsStream("test/patch0002_second_patch.sql");
        task = new SqlScriptMigrationTask("test", 1, is);
        is.close();
        
        l = task.getSqlStatements(context);
        assertEquals(1, l.size());
        assertEquals("insert into user_role_assoc (user_role_id, application_user_id, "
            + "role_code, project_id) \n\t\t\tvalues (nextval('role_id_seq'),2, 'SYSA', 3)",
            l.get(0).toString());
    }
    
    /**
     * Make sure that if we can do one big statement, it correctly does one big statement
     * 
     * @exception IOException if an unexpected error happens while reading test SQL
     */
    public void testParsingSingleStatement() throws IOException
    {
        InputStream is = getClass().getResourceAsStream("test/patch0003_third_patch.sql");
        task = new SqlScriptMigrationTask("test", 1, is);
        is.close();

        List l = task.getSqlStatements(context);
        assertEquals(1, l.size());
        assertEquals("select * from dual;\nselect * from dual;\n",
                     l.get(0).toString());
    }
    
    /**
     * See that the name and toString are the same, given a file name to load
     * 
     * @exception IOException if an unexpected error happens while reading test SQL
     */
    public void testTaskName() throws IOException
    {
        InputStream is = getClass().getResourceAsStream("test/patch0003_third_patch.sql");
        task = new SqlScriptMigrationTask("patch0003_third_patch", 1, is);
        is.close();
        assertEquals("patch0003_third_patch", task.toString());
    }
    
    /**
     * Tests that sybase tsql statements are parsed correctly
     * @throws IOException if an unexpected error occurs.
     */
    public void testParsesSybaseTSql() throws IOException
    {
        InputStream is = getClass().getResourceAsStream("test/sybase_tsql.sql");
        assertNotNull(is);
        task = new SqlScriptMigrationTask("sybase_tsql.sql", 1, is);
    
        MockDatabaseType dbType = new MockDatabaseType("sybase");
        dbType.setMultipleStatementsSupported(false);
        context.setDatabaseType(dbType);
        List statements = task.getSqlStatements(context);
        assertEquals(8, statements.size());
    }

    /**
     * Test that sybase database patches are committed when illegal
     * multi statement transaction commands are used.
     * @throws IOException if an unexpected error occurs
     * @throws MigrationException if an unexpected error occurs
     * @throws SQLException if an unexpected error occurs
     */
    public void testSybasePatchesCommitsOnEveryStatement() 
        throws IOException, MigrationException, SQLException
    {
        InputStream is = getClass().getResourceAsStream("test/sybase_tsql.sql");
        assertNotNull(is);
        task = new SqlScriptMigrationTask("sybase_tsql.sql", 1, is);
        
        MockDatabaseType dbType = new MockDatabaseType("sybase");
        dbType.setMultipleStatementsSupported(false);
        context.setDatabaseType(dbType);
        int numStatements = task.getSqlStatements(context).size();
        
        
        // setup mocks to verify commits are called
        MockControl dataSourceControl = MockControl.createControl(DataSource.class);
        DataSource dataSource = (DataSource) dataSourceControl.getMock();
        context.setDataSource(dataSource);
        
        MockControl connectionControl = MockControl.createControl(Connection.class);
        Connection connection = (Connection) connectionControl.getMock();
        
        dataSourceControl.expectAndReturn(dataSource.getConnection(), connection);
        
        MockControl statementControl = MockControl.createControl(Statement.class);
        Statement statement = (Statement) statementControl.getMock();
        statement.execute("");
        statementControl.setMatcher(MockControl.ALWAYS_MATCHER);
        statementControl.setReturnValue(true, MockControl.ONE_OR_MORE);
        statement.close();
        statementControl.setVoidCallable(MockControl.ONE_OR_MORE);
        
        connectionControl.expectAndReturn(connection.isClosed(), false, MockControl.ONE_OR_MORE);
        connectionControl.expectAndReturn(
                connection.createStatement(), statement, numStatements);
        connectionControl.expectAndReturn(connection.getAutoCommit(), false, MockControl.ONE_OR_MORE);
        connection.commit();
        /*
         * Magic Number 4 derived from the assumption that the fixture sql
         * contains only one statement that is not allowed in a 
         * multi statement transaction:
         * commit at beginning of migrate method
         * commit prior to running the command not allowed in multi statement transaction
         *   to clear the transaction state.
         * commit after running the multi statement transaction to clear transaction state
         *   for upcoming statements.
         * commit at end of migrate method once all statements executed.
         * 
         * Therefore, if you add more illegal statements to the fixture, add 2 more
         * commit call's for each illegal statement.
         */
        connectionControl.setVoidCallable(4);
        
        dataSourceControl.replay();
        connectionControl.replay();
        statementControl.replay();
        
        // run tests
        task.migrate(context);
        dataSourceControl.verify();
        connectionControl.verify();
    }
    

}

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
import java.sql.ResultSet;
import java.sql.Statement;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.jdbc.util.SqlUtil;

/**
 * Test AutoPatch MultiNode functionality
 * 
 * @author  Mike Hardy (mike@tacitknowledge.com)
 */
public class MultiNodeAutoPatchTest extends AutoPatchIntegrationTestBase
{
    /** Class logger */
    private static Log log = LogFactory.getLog(MultiNodeAutoPatchTest.class);
    
    /**
     * Constructor 
     * 
     * @param name the name of the test to run
     */
    public MultiNodeAutoPatchTest(String name)
    {
        super(name);
    }
    
    /**
     * Test that all the tables were created successfully in all of the databases
     * 
     * @exception Exception if anything goes wrong
     */
    public void testMultiNodePatch() throws Exception
    {
        log.debug("Testing multi node patching");
        try
        {
            distributedLauncher.doMigrations();
        }
        catch (Exception e)
        {
            log.error("Unexpected error", e);
            fail("shouldn't have thrown any exceptions");
        }
        
        // Make sure everything worked out okay
       Connection core = DriverManager.getConnection("jdbc:hsqldb:mem:core", "sa", "");
       Connection orders = DriverManager.getConnection("jdbc:hsqldb:mem:orders", "sa", "");
       Connection catalog1 = DriverManager.getConnection("jdbc:hsqldb:mem:catalog1", "sa", "");
       Connection catalog2 = DriverManager.getConnection("jdbc:hsqldb:mem:catalog2", "sa", "");
       
       // 4 patches should have executed
       assertEquals(4, getPatchLevel(core));
       assertEquals(4, getPatchLevel(orders));
       assertEquals(4, getPatchLevel(catalog1));
       assertEquals(4, getPatchLevel(catalog2));
       
       
       // we should have test values in each table
       verifyTestTable(core, "core_table_1");
       verifyTestTable(orders, "order_table_1");
       verifyTestTable(orders, "order_table_2");
       verifyTestTable(catalog1, "catalog_table_1");
       verifyTestTable(catalog2, "catalog_table_1");
       
       SqlUtil.close(core, null, null);
       SqlUtil.close(orders, null, null);
       SqlUtil.close(catalog1, null, null);
       SqlUtil.close(catalog2, null, null);
    }
    
    /**
     * Get the patch level for a given database
     * 
     * @param conn the database connection to use
     * @exception Exception if getting the patch level fails
     */
    private int getPatchLevel(Connection conn) throws Exception
    {
        Statement stmt = conn.createStatement();
        ResultSet rs = stmt.executeQuery("SELECT patch_level FROM patches");
        rs.next();
        int patchLevel = rs.getInt("patch_level");
        SqlUtil.close(null, stmt, rs);
        return patchLevel;
    }
    
    /**
     * Verify that a given table exists and that it contains one row
     * with a value equal to the name of the table (this matches the inttest patches)
     * 
     * @param conn the Connection to use
     * @param tableName the table name (and row value) to look for
     * @exception Exception if anything goes wrong
     */
    private void verifyTestTable(Connection conn, String tableName) throws Exception
    {
        Statement stmt = conn.createStatement();
        ResultSet rs = stmt.executeQuery("SELECT value FROM " + tableName);
        rs.next();
        assertEquals(tableName, rs.getString("value"));
    }
}

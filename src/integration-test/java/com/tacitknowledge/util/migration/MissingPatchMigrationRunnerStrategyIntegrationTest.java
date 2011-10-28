/* Copyright 2011 Tacit Knowledge
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.tacitknowledge.util.migration;

import com.tacitknowledge.util.migration.jdbc.JdbcMigrationLauncher;
import com.tacitknowledge.util.migration.jdbc.JdbcMigrationLauncherFactory;
import com.tacitknowledge.util.migration.jdbc.util.SqlUtil;
import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.Statement;


public class MissingPatchMigrationRunnerStrategyIntegrationTest extends AutoPatchIntegrationTestBase {

    /**
     * Class logger
     */
    private static Log log = LogFactory.getLog(MissingPatchMigrationRunnerStrategyIntegrationTest.class);

    /**
     * Constructor
     *
     * @param name the name of the test to run
     */
    public MissingPatchMigrationRunnerStrategyIntegrationTest(String name) {
        super(name);
    }

    public void setUp() throws Exception {
    }

    public void testMigrationRunsFromBranches() throws Exception {
        log.debug("Testing migration from branch 1");
        try {
            JdbcMigrationLauncherFactory lFactory = new JdbcMigrationLauncherFactory();

            JdbcMigrationLauncher launcherBatch1 = lFactory.createMigrationLauncher("orders", "missingpatchstrategybatch1-inttest-migration.properties");
            launcherBatch1.doMigrations();

            assertEquals(4, getPatchLevel(getOrderConnection()));
            assertTrue(isPatchApplied(getOrderConnection(), 1));
            assertTrue(isPatchApplied(getOrderConnection(), 4));
            assertFalse(isPatchApplied(getOrderConnection(), 2));
            assertFalse(isPatchApplied(getOrderConnection(), 3));


            JdbcMigrationLauncher launcherBatch2 = lFactory.createMigrationLauncher("orders", "missingpatchstrategybatch2-inttest-migration.properties");
            launcherBatch2.doMigrations();
            currentPatches("orders");

            assertEquals(4, getPatchLevel(getOrderConnection()));
            assertTrue(isPatchApplied(getOrderConnection(), 1));
            assertTrue(isPatchApplied(getOrderConnection(), 3));
            assertTrue(isPatchApplied(getOrderConnection(), 4));
            assertTrue(isPatchApplied(getOrderConnection(), 2));

        } catch (Exception e) {
            log.error("Unexpected error", e);
            fail("shouldn't have thrown any exceptions");
        }

    }

    private void currentPatches(String database) throws Exception {
        Connection conn = DriverManager.getConnection("jdbc:hsqldb:mem:orders", "sa", "");
        Statement stmt = conn.createStatement();
        ResultSet resultSet = stmt.executeQuery("SELECT * FROM patches");
        while (resultSet.next()) {
            log.info("Result " + resultSet.getInt(2));
        }
        SqlUtil.close(conn, stmt, null);

    }

    private boolean isPatchApplied(Connection conn, int patch_level) throws Exception {

        Statement stmt = conn.createStatement();
        String sql = "SELECT patch_level FROM patches WHERE system_name= 'orders' and patch_level=" + patch_level + "";
        ResultSet rs = stmt.executeQuery(sql);
        return rs.next();
    }

}

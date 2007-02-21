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

import javax.naming.InitialContext;
import javax.naming.NamingException;
import javax.servlet.ServletContextEvent;

import org.mockejb.jndi.MockContextFactory;

import com.mockrunner.mock.jdbc.MockDataSource;
import com.mockrunner.mock.web.MockServletContext;
import com.tacitknowledge.util.migration.MigrationException;

import junit.framework.TestCase;

/**
 * Tests that the factory successfully returns parameters found in the 
 * servlet context.
 * 
 * @author Chris A. (chris@tacitknowledge.com)
 */
public class WebAppServletContextFactoryTest extends TestCase
{
    /**
     * Tests that the new mechanism for configuring a launcher from a 
     * servlet context works.
     * 
     * @throws NamingException a problem with the test
     */
    public void testConfiguredContext() throws NamingException
    {
        JdbcMigrationLauncherFactory launcherFactory = new JdbcMigrationLauncherFactory();
        MockServletContext sc = new MockServletContext();
        
        String dbType = "mysql";
        String sysName = "testSystem";
        
        sc.setInitParameter("migration.systemname", sysName);
        sc.setInitParameter("migration.readonly", "true");
        sc.setInitParameter("migration.databasetype", dbType);
        sc.setInitParameter("migration.patchpath", "patches");
        sc.setInitParameter("migration.datasource", "jdbc/testsource");
        
        MockDataSource ds = new MockDataSource();
        MockContextFactory.setAsInitial();
        InitialContext context = new InitialContext();
        context.createSubcontext("java");
        context.bind("java:comp/env/jdbc/testsource", ds);
        ServletContextEvent sce = new ServletContextEvent(sc);
        JdbcMigrationLauncher launcher = null;
        try
        {
            launcher = launcherFactory.createMigrationLauncher(sce);
        } 
        catch (MigrationException e)
        {
            e.printStackTrace();
            fail("There should not have been an exception");
        }

        JdbcMigrationContext jdbcContext = launcher.getContext();
        assertEquals(dbType, jdbcContext.getDatabaseType().getDatabaseType());
        assertEquals(sysName, jdbcContext.getSystemName());
        assertEquals(true, launcher.isReadOnly());
    }

}

/* 
 * Copyright 2006 Tacit Knowledge LLC
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


import java.util.HashMap;
import java.util.Iterator;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.jdbc.DistributedJdbcMigrationLauncher;
import com.tacitknowledge.util.migration.jdbc.DistributedJdbcMigrationLauncherFactory;
import com.tacitknowledge.util.migration.jdbc.JdbcMigrationLauncher;
import com.tacitknowledge.util.migration.jdbc.TestDataSourceMigrationContext;
import com.tacitknowledge.util.migration.jdbc.TestDistributedJdbcMigrationLauncherFactory;
import com.tacitknowledge.util.migration.tasks.normal.TestMigrationTask2;

/**
 * Test the distributed launcher factory
 * 
 * @author Mike Hardy (mike@tacitknowledge.com)
 */
public class DistributedJdbcMigrationLauncherFactoryTest extends MigrationListenerTestBase
{
    /** Class logger */
    private static Log log = LogFactory.getLog(DistributedJdbcMigrationLauncherFactoryTest.class);
    
    /** The launcher we're testing */
    private DistributedJdbcMigrationLauncher launcher = null;
    
    /** A MigrationContext for us */
    private TestMigrationContext context = null;
    
    /**
     * constructor that takes a name
     *
     * @param name of the test to run
     */
    public DistributedJdbcMigrationLauncherFactoryTest(String name)
    {
        super(name);
    }
    
    /**
     * @see junit.framework.TestCase#setUp()
     */
    protected void setUp() throws Exception
    {
        super.setUp();
        
        // Make sure we load our test launcher factory, which fakes out the data source context
        System.getProperties().setProperty("migration.factory", 
                                           "com.tacitknowledge.util.migration.jdbc.TestJdbcMigrationLauncherFactory");
        DistributedJdbcMigrationLauncherFactory factory = new TestDistributedJdbcMigrationLauncherFactory();
        
        // Create the launcher (this does configure it as a side-effect)
        launcher = (DistributedJdbcMigrationLauncher)factory.createMigrationLauncher("orchestration");
        
        // Make sure we get notification of any migrations
        launcher.getMigrationProcess().addListener(this);
    }

    /**
     * @see junit.framework.TestCase#tearDown()
     */
    protected void tearDown() throws Exception
    {
        super.tearDown();
        log.info("tearing down");
    }
    
    /**
     * Test the configuration of the launchers versus a known property file
     */
    public void testDistributedLauncherConfiguration()
    {
        HashMap controlledSystems = 
            ((DistributedMigrationProcess)launcher.getMigrationProcess()).getControlledSystems();
        assertEquals(3, controlledSystems.size());
    }
    
    /**
     * Make sure that the task loading works correctly
     * 
     * @exception MigrationException if anything goes wrong
     */
    public void testDistributedMigrationTaskLoading() throws MigrationException
    {
        MigrationProcess process = launcher.getMigrationProcess();
        assertEquals(7, process.getMigrationTasks().size());
    }
    
    /**
     * Ensure that overlapping tasks even among sub-launchers are detected
     * 
     * @exception MigrationException if anything goes wrong
     */    
    public void testDistributedMigrationTaskValidation() throws Exception
    {
        MigrationProcess process = launcher.getMigrationProcess();
        process.validateTasks(process.getMigrationTasks());
        
        // Make one of the sub-tasks conflict with a sub-task from another launcher
        TestMigrationTask2.setPatchLevelOverride(new Integer(3));
        try
        {
            process.validateTasks(process.getMigrationTasks());
            fail("We should have thrown an exception - " 
                 + "there were overlapping tasks among sub-launchers");
        }
        catch (MigrationException me)
        {
            // we expect this
        }
        finally
        {
            // make sure future tests work
            TestMigrationTask2.reset();
        }
    }
    
    /**
     * Make sure we get notified of patch application
     * 
     * @exception MigrationException if anything goes wrong
     */    
    public void testDistributedMigrationEvents() throws Exception
    {
        // There should be five listener on the main process
        //  1) the distributed launcher
        //  2) this test object
        //  3-5) the three sub-launchers
        assertEquals(5, launcher.getMigrationProcess().getListeners().size());
        
        // The sub-MigrationProcesses should have one listener each - the sub-launcher
        HashMap controlledSystems = 
            ((DistributedMigrationProcess)launcher.getMigrationProcess()).getControlledSystems();
        
        for (Iterator controlledSystemIter = controlledSystems.keySet().iterator();
        controlledSystemIter.hasNext();)
        {
            String controlledSystemName = (String)controlledSystemIter.next();
            JdbcMigrationLauncher subLauncher = 
                (JdbcMigrationLauncher)controlledSystems.get(controlledSystemName);
            MigrationProcess subProcess = subLauncher.getMigrationProcess();
            assertEquals(1, subProcess.getListeners().size());
        }
        
        // Now do the migrations, and make sure we get the right number of events
        MigrationProcess process = launcher.getMigrationProcess();
        int patches = process.doMigrations(3, context);
        assertEquals(4, patches);
        assertEquals(4, getMigrationStartedCount());
        assertEquals(4, getMigrationSuccessCount());
    }
    
    /**
     * Make sure we the right patches go in the right spot
     * 
     * @exception MigrationException if anything goes wrong
     */    
    public void testDistributedMigrationContextTargetting() throws Exception
    {
        HashMap controlledSystems = 
            ((DistributedMigrationProcess)launcher.getMigrationProcess()).getControlledSystems();
        
        // Now do the migrations, and make sure we get the right number of events
        MigrationProcess process = launcher.getMigrationProcess();
        process.doMigrations(3, context);
        
        // The orders schema has four tasks that should go, make sure they did
        JdbcMigrationLauncher ordersLauncher = 
            (JdbcMigrationLauncher)controlledSystems.get("orders");
        TestDataSourceMigrationContext ordersContext = 
            (TestDataSourceMigrationContext)ordersLauncher.getContext();
        assertEquals("orders", ordersContext.getSystemName());
        assertTrue(ordersContext.hasExecuted("TestTask1"));
        assertTrue(ordersContext.hasExecuted("TestTask2"));
        assertTrue(ordersContext.hasExecuted("TestTask3"));
        assertTrue(ordersContext.hasExecuted("TestTask4"));
        
        // The core schema has three tasks that should not go, make sure they did not
        JdbcMigrationLauncher coreLauncher = 
            (JdbcMigrationLauncher)controlledSystems.get("core");
        TestDataSourceMigrationContext coreContext = 
            (TestDataSourceMigrationContext)coreLauncher.getContext();
        assertEquals("core", coreContext.getSystemName());
        assertFalse(coreContext.hasExecuted("patch0001"));
        assertFalse(coreContext.hasExecuted("patch0002"));
        assertFalse(coreContext.hasExecuted("patch0003"));
    }
}

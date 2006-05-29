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

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.jdbc.DistributedJdbcMigrationLauncher;
import com.tacitknowledge.util.migration.jdbc.DistributedJdbcMigrationLauncherFactory;
import com.tacitknowledge.util.migration.jdbc.TestDistributedJdbcMigrationLauncherFactory;
import com.tacitknowledge.util.migration.tasks.normal.TestMigrationTask2;

import junit.framework.TestCase;

/**
 * Test the distributed launcher factory
 * 
 * @author Mike Hardy (mike@tacitknowledge.com)
 */
public class DistributedJdbcMigrationLauncherFactoryTest extends TestCase
{
    /** Class logger */
    private static Log log = LogFactory.getLog(DistributedJdbcMigrationLauncherFactoryTest.class);
    
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
        log.info("setting up for test run");
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
     * 
     * @exception MigrationException if something goes horribly awry
     */
    public void testDistributedLauncherConfiguration() throws MigrationException
    {
        log.info("testing distributed configuration");
        System.getProperties().setProperty("migration.factory", 
                                           "com.tacitknowledge.util.migration.jdbc.TestJdbcMigrationLauncherFactory");
        DistributedJdbcMigrationLauncherFactory factory = new TestDistributedJdbcMigrationLauncherFactory();
        DistributedJdbcMigrationLauncher launcher = 
            (DistributedJdbcMigrationLauncher)factory.createMigrationLauncher("orchestration");
        
       HashMap controlledSystems = 
           ((DistributedMigrationProcess)launcher.getMigrationProcess()).getControlledSystems();
       assertEquals(3, controlledSystems.size());
       
       MigrationProcess process = launcher.getMigrationProcess();
       assertEquals(7, process.getMigrationTasks().size());
       
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
}

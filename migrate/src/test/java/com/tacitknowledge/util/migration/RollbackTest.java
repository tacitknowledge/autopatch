/* 
 * Copyright 2008 Tacit Knowledge LLC
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

import java.util.List;

/**
 * This class defines unit tests for the Rollback functionality.
 * @author Artie Pesh-Imam (apeshimam@tacitknowledge.com)
 * 
 */
public class RollbackTest extends MigrationListenerTestBase
{
    /** The class under test */
    private MigrationProcess runner = null;
    
    /** Test migration context */
    private TestMigrationContext context = null;
    
    /**
     * Constructor for RollbackTest.
     * 
     * @param name the name of the test to run
     */
    public RollbackTest(String name)
    {
        super(name);
    }
    
    /**
     * @see junit.framework.TestCase#setUp()
     */
    protected void setUp() throws Exception
    {
        super.setUp();
        runner = new MigrationProcess();
        runner.addPatchResourceDirectory(getClass().getPackage().getName()
                + ".tasks.rollback");
        runner.addPostPatchResourceDirectory(getClass().getPackage().getName()
                + ".tasks.post");
        runner.addListener(this);
        
        
        context = new TestMigrationContext();
    }
    
    /**
     * @see junit.framework.TestCase#tearDown()
     */
    protected void tearDown() throws Exception
    {
        super.tearDown();
        // Reset the "fail" bit that may have been set on a previous run.
        // TestMigrationTask2.reset();
        // TestMigrationTask3.setFail(false);
    }
    
    /**
     * This method tests the basic rollback functionality.
     * 
     * @throws MigrationException
     */
    public void testRollbackAllTasks() throws MigrationException
    {
        List l = runner.getMigrationTasks();
        assertEquals(5, l.size());
        
        int level = runner.doMigrations(1, context);
        runner.doPostPatchMigrations(context);
        assertEquals(5, level);
        assertTrue(context.hasExecuted("TestRollbackableTask1"));
        assertTrue(context.hasExecuted("TestRollbackableTask2"));
        assertTrue(context.hasExecuted("TestRollbackableTask3"));
        assertTrue(context.hasExecuted("TestRollbackableTask4"));
        assertTrue(context.hasExecuted("TestRollbackableTask5"));
        
        // check that the migrations occurred successfully
        assertEquals(5, getMigrationStartedCount());
        assertEquals(5, getMigrationSuccessCount());
        
        // execute the rollback
        level = runner.doRollbacks(12, 8, context, false);
        assertEquals(4, level);
        assertEquals(4, getRollbackSuccessCount());
        
    }
    
    /**
     * This method tests the basic rollback functionality.
     * 
     * @throws MigrationException
     */
    public void testRollbackPartialTasks() throws MigrationException
    {
        List l = runner.getMigrationTasks();
        assertEquals(5, l.size());
        
        int level = runner.doMigrations(8, context);
        runner.doPostPatchMigrations(context);
        assertEquals(4, level);
        assertEquals(4, getMigrationSuccessCount());
        
        assertFalse(context.hasExecuted("TestRollbackableTask1"));
        assertTrue(context.hasExecuted("TestRollbackableTask2"));
        assertTrue(context.hasExecuted("TestRollbackableTask3"));
        assertTrue(context.hasExecuted("TestRollbackableTask4"));
        assertTrue(context.hasExecuted("TestRollbackableTask5"));
        
        // execute the rollback
        level = runner.doRollbacks(12, 8, context, false);
        assertEquals(4, level);
        assertEquals(4, getRollbackSuccessCount());
    }
    
    /**
     * This method tests the scneario when a non rollbackable task is attempted to rollback.
     * Note that in this scenario, the forceRollback is false.
     * 
     * @throws MigrationException
     */
    public void testRollbackNotRollbackableTask() throws MigrationException
    {
        doInitialMigrations();
        
        // execute the rollback
        try
        {
            runner.doRollbacks(12, 7, context, false);
        } 
        catch (MigrationException me)
        {
            // expecting exception
        }
        assertEquals(0, getRollbackSuccessCount());
    }
    
    /**
     * This tests the forceRollback functionality.
     * @throws MigrationException
     */
    public void testForceRollback() throws MigrationException
    {
        doInitialMigrations();
        
        // execute the rollback
        try
        {
            runner.doRollbacks(12, 7, context, true);
        } 
        catch (MigrationException me)
        {
            // expecting exception
        }
        assertEquals(5, getRollbackSuccessCount());
    }
    
    /**
     * this is a private helper method to perform initial migrations
     * @throws MigrationException
     */
    private void doInitialMigrations() throws MigrationException
    {
        List l = runner.getMigrationTasks();
        assertEquals(5, l.size());
        
        int level = runner.doMigrations(0, context);
        runner.doPostPatchMigrations(context);
        assertEquals(5, level);
        assertEquals(5, getMigrationSuccessCount());
        
        assertTrue(context.hasExecuted("TestRollbackableTask1"));
        assertTrue(context.hasExecuted("TestRollbackableTask2"));
        assertTrue(context.hasExecuted("TestRollbackableTask3"));
        assertTrue(context.hasExecuted("TestRollbackableTask4"));
        assertTrue(context.hasExecuted("TestRollbackableTask5"));
    }
    
    /**
     * this tests the scenario when the user tries to rollback to a level
     * which is greater than the current patch level.
     * 
     * @throws MigrationException
     */
    public void testInvalidRollbackLevel() throws MigrationException
    {
        List l = runner.getMigrationTasks();
        assertEquals(5, l.size());
        
        int level = runner.doMigrations(0, context);
        runner.doPostPatchMigrations(context);
        assertEquals(5, level);
        assertEquals(5, getMigrationSuccessCount());
        
        assertTrue(context.hasExecuted("TestRollbackableTask1"));
        assertTrue(context.hasExecuted("TestRollbackableTask2"));
        assertTrue(context.hasExecuted("TestRollbackableTask3"));
        assertTrue(context.hasExecuted("TestRollbackableTask4"));
        assertTrue(context.hasExecuted("TestRollbackableTask5"));
        
        try
        {
            level = runner.doRollbacks(level, 7, context, false);
        } 
        catch (IllegalArgumentException iae)
        {
            // expected
        }
        
    }
    
    /**
     * this test checks that the system correctly when a rollback is attempted 
     * on a non-rollbackable task.
     * 
     * @throws MigrationException
     */
    public void testMigrationTaskRollback() throws MigrationException
    {
        // add additional directory containing MigrationTask
        runner.addPatchResourceDirectory(getClass().getPackage().getName()
                + ".tasks.rollback.migrationtasks");
        List l = runner.getMigrationTasks();
        
        assertEquals(6, l.size());
        
        int level = runner.doMigrations(7, context);
        runner.doPostPatchMigrations(context);
        assertEquals(6, level);
        assertEquals(6, getMigrationSuccessCount());
        assertTrue(context.hasExecuted("TestRollbackableTask1"));
        assertTrue(context.hasExecuted("TestRollbackableTask2"));
        assertTrue(context.hasExecuted("TestRollbackableTask3"));
        assertTrue(context.hasExecuted("TestRollbackableTask4"));
        assertTrue(context.hasExecuted("TestRollbackableTask5"));
        assertTrue(context.hasExecuted("TestMigrationTaskRollback1"));
        
        try
        {
            level = runner.doRollbacks(13, 12, context, false);
        } 
        catch (MigrationException me)
        {
            // expected
        }
        assertEquals(0, level);
    }
}

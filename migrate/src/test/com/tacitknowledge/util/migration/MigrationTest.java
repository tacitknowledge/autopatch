/* Copyright 2004 Tacit Knowledge LLC
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

import junit.framework.TestCase;

/**
 * 
 * 
 * @author  Scott Askew (scott@tacitknowledge.com)
 * @version $Id$
 */
public class MigrationTest extends TestCase
{
    /**
     * The class under test
     */
    private MigrationProcess runner = null;
    
    /**
     * Test migration context
     */
    private TestMigrationContext context = null;
    
    /**
     * Constructor for MigrationTest.
     * 
     * @param name the name of the test to run
     */
    public MigrationTest(String name)
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
        runner.addResourcePackage(getClass().getPackage().getName());
        
        context = new TestMigrationContext();
    }

    /**
     * @see junit.framework.TestCase#tearDown()
     */
    protected void tearDown() throws Exception
    {
        super.tearDown();
        // Reset the "fail" bit that may have been set on a previous run.
        TestMigrationTask2.reset();
        TestMigrationTask3.setFail(false);
    }
    
    /**
     * Validates that the migration runner can run the all patches.
     * 
     * @throws MigrationException if an unexpected error occurs
     */
    public void testRunAllMigrationTasks() throws MigrationException
    {
        List l = runner.getMigrationTasks();
        assertEquals(4, l.size());
        
        int level = runner.doMigrations(0, context);
        assertEquals(4, level);
        assertTrue(context.hasExecuted("TestTask1"));
        assertTrue(context.hasExecuted("TestTask2"));
        assertTrue(context.hasExecuted("TestTask3"));
        assertTrue(context.hasExecuted("TestTask4"));
    }

    /**
     * Validates that the migration runner will only run the necessary patches
     * to bring the system current 
     * 
     * @throws MigrationException if an unexpected error occurs
     */
    public void testRunPartialMigrationTasks() throws MigrationException
    {
        List l = runner.getMigrationTasks();
        assertEquals(4, l.size());
        
        int level = runner.doMigrations(2, context);
        assertEquals(2, level);
        assertFalse(context.hasExecuted("TestTask1"));
        assertFalse(context.hasExecuted("TestTask2"));
        assertTrue(context.hasExecuted("TestTask3"));
        assertTrue(context.hasExecuted("TestTask4"));
    }
    
    /**
     * Verifies that the migration runner checks for patch level uniqueness
     * between migration tasks
     */
    public void testMigrationPatchLevelUniqueness()
    {
        try
        {
            TestMigrationTask2.setPatchLevelOverride(new Integer(3));
            runner.doMigrations(0, context);
            fail("Expected a MigrationException due to a task patch level conflict");
        }
        catch (MigrationException e)
        {
            assertTrue("Expected a patch conflict migration exception, not \""
                + e.getMessage() + "\"",
                e.getMessage().indexOf("conflicting patch level") > -1);
        }
    }

    /**
     * Validates that the migration runner will flag tasks with null patch levels
     */
    public void testNullPatchLevel()
    {
        try
        {
            TestMigrationTask2.setPatchLevelOverride(null);
            runner.doMigrations(0, new TestMigrationContext());
            fail("Expected a MigrationException due to a null patch level");
        }
        catch (MigrationException e)
        {
            assertTrue("Expected a patch level missing exception, not \""
                + e.getMessage() + "\"",
                e.getMessage().indexOf("patch level defined") > -1);
        }
    }
    
    /**
     * Validates <code>Migration.getNextPatchLevel</code>.
     * 
     * @throws MigrationException if an unexpected error occurs
     */
    public void testGetNextPatchLevel() throws MigrationException
    {
        int level = runner.getNextPatchLevel();
        assertEquals(5, level);
    }
}

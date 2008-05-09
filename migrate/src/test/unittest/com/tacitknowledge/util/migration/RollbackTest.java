package com.tacitknowledge.util.migration;

import java.util.List;

import com.tacitknowledge.util.migration.tasks.normal.TestMigrationTask2;
import com.tacitknowledge.util.migration.tasks.normal.TestMigrationTask3;

public class RollbackTest extends RollbackListenerTestBase 
{
	/** The class under test */
	private MigrationProcess runner = null;

	/** Test migration context */
	private TestMigrationContext context = null;

	/**
	 * Constructor for RollbackTest.
	 * 
	 * @param name
	 *            the name of the test to run
	 */
	public RollbackTest(String name) {
		super(name);
	}

    /**
     * @see junit.framework.TestCase#setUp()
     */
    protected void setUp() throws Exception
    {
        super.setUp();
        runner = new MigrationProcess();
        runner.addPatchResourceDirectory(getClass().getPackage().getName() + ".tasks.rollback");
        runner.addPostPatchResourceDirectory(getClass().getPackage().getName() + ".tasks.post");
        runner.addRollbackListener(this);
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
        //TestMigrationTask2.reset();
        //TestMigrationTask3.setFail(false);
    }  
    
    public void testRollbackAllTasks() throws MigrationException {
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
        
        //check that the migrations occurred successfully
        assertEquals(5, getMigrationStartedCount());
        assertEquals(5, getMigrationSuccessCount());
        
        //execute the rollback
        level = runner.doRollbacks(12, 8, context);
        assertEquals(4, level);
        assertEquals(4, getRollbackSuccessCount());
    	
    }
    
    public void testRollbackPartialTasks() throws MigrationException {
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
        
        //execute the rollback
        level = runner.doRollbacks(12, 8, context);
        assertEquals(4, level);
        assertEquals(4, getRollbackSuccessCount());
    }
    
    public void testRollbackNotRollbackableTask() throws MigrationException {
    	int level;
		doInitialMigrations();
        
        //execute the rollback
        
        try {
        level = runner.doRollbacks(12, 7, context);
        } catch(MigrationException me) {
        	//expecting exception
        }
        assertEquals(0, getRollbackSuccessCount());
    }

	private void doInitialMigrations() throws MigrationException {
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
    
    public void testInvalidRollbackLevel() throws MigrationException {
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
        
        try {
        level = runner.doRollbacks(level, 7, context);
        } catch (IllegalArgumentException iae) {
        	//expected
        }  
        
    }
    
    public void testMigrationTaskRollback() throws MigrationException {
    	//add additional directory containing MigrationTask 
    	runner.addPatchResourceDirectory(getClass().getPackage().getName() + ".tasks.rollback.migrationtasks");
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
        
        try {
        	level = runner.doRollbacks(13, 12, context);
        } catch (MigrationException me) {
        	//expected
        }
    	assertEquals(0, level);
    }
    
}

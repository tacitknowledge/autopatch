package com.tacitknowledge.util.migration;

import java.util.Properties;

import com.mockrunner.jdbc.JDBCTestCaseAdapter;

public class RollbackListenerTestBase extends MigrationListenerTestBase implements RollbackListener {
	
    /** the count of times "rollback started" was broadcast */
    private int rollbackStartedCount = 0;
    
    /** the count of times "rollback success" was broadcast */
    private int rollbackSuccessCount = 0;
    
    /** the count of times "rollback failed" was broadcast */
    private int rollbackFailedCount = 0;

    /**
     * Constructor for MigrationTest.
     * 
     * @param name the name of the test to run
     */
    public RollbackListenerTestBase(String name)
    {
        super(name);
    }
    
	public void initialize(String systemName, Properties properties)
			throws MigrationException {
		
	}

	public void rollbackFailed(RollbackableMigrationTask task, MigrationContext context,
			MigrationException e) throws MigrationException {
		setRollbackFailedCount(getRollbackFailedCount() + 1);
		
	}

	public void rollbackStarted(RollbackableMigrationTask task, MigrationContext context)
			throws MigrationException {
		setRollbackStartedCount(getRollbackStartedCount() + 1);
		
	}

	public void rollbackSuccessful(RollbackableMigrationTask task, MigrationContext context)
			throws MigrationException {
		setRollbackSuccessCount(getRollbackSuccessCount() + 1);
		
	}

	public int getRollbackStartedCount() {
		return rollbackStartedCount;
	}

	public void setRollbackStartedCount(int rollbackStartedCount) {
		this.rollbackStartedCount = rollbackStartedCount;
	}

	public int getRollbackSuccessCount() {
		return rollbackSuccessCount;
	}

	public void setRollbackSuccessCount(int rollbackSuccessCount) {
		this.rollbackSuccessCount = rollbackSuccessCount;
	}

	public int getRollbackFailedCount() {
		return rollbackFailedCount;
	}

	public void setRollbackFailedCount(int rollbackFailedCount) {
		this.rollbackFailedCount = rollbackFailedCount;
	}

}

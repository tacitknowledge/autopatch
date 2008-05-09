package com.tacitknowledge.util.migration.tasks.rollback;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.RollbackableMigrationTask;
import com.tacitknowledge.util.migration.TestMigrationContext;
import com.tacitknowledge.util.migration.tasks.BaseTestMigrationTask;

public class TestRollbackableTask1 extends BaseTestMigrationTask implements RollbackableMigrationTask {

	public TestRollbackableTask1() {
		super("TestRollbackableTask1", 8);
	}
	
	public TestRollbackableTask1(int level) {
	    super("TestRollbackableTask1", level);
	}


	public boolean isRollbackSupported() {
		return false;
	}
	
	
}

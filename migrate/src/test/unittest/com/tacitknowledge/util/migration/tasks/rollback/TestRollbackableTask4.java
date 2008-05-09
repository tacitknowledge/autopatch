package com.tacitknowledge.util.migration.tasks.rollback;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.RollbackableMigrationTask;
import com.tacitknowledge.util.migration.TestMigrationContext;
import com.tacitknowledge.util.migration.tasks.BaseTestMigrationTask;

public class TestRollbackableTask4 extends BaseTestMigrationTask implements RollbackableMigrationTask {

	public TestRollbackableTask4() {
		super("TestRollbackableTask4", 11);
	}
	public void down(MigrationContext context) throws MigrationException {
        if (context instanceof TestMigrationContext)
        {
            TestMigrationContext ctx = (TestMigrationContext) context;
            ctx.recordExecution(getName());
        }
	}
}

package com.tacitknowledge.util.migration.tasks.rollback.migrationtasks;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.MigrationTask;
import com.tacitknowledge.util.migration.MigrationTaskSupport;
import com.tacitknowledge.util.migration.TestMigrationContext;

public class TestMigrationTaskRollback1 extends MigrationTaskSupport implements MigrationTask {

	public TestMigrationTaskRollback1() {
		setName("TestMigrationTaskRollback1");
		setLevel(13);
	}

	public void migrate(MigrationContext context) throws MigrationException {
        if (context instanceof TestMigrationContext)
        {
            TestMigrationContext ctx = (TestMigrationContext) context;
            ctx.recordExecution(getName());
        }
		
	}
}

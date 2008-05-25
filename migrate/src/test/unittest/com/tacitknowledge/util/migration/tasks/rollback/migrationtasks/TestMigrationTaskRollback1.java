package com.tacitknowledge.util.migration.tasks.rollback.migrationtasks;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.RollbackableMigrationTask;
import com.tacitknowledge.util.migration.MigrationTaskSupport;
import com.tacitknowledge.util.migration.TestMigrationContext;

public class TestMigrationTaskRollback1 extends MigrationTaskSupport implements RollbackableMigrationTask 
{
    
    public TestMigrationTaskRollback1() 
    {
        setName("TestMigrationTaskRollback1");
        setLevel(new Integer(13));
    }
    
    public void migrate(MigrationContext context) throws MigrationException 
    {
        if (context instanceof TestMigrationContext)
        {
            TestMigrationContext ctx = (TestMigrationContext) context;
            ctx.recordExecution(getName());
        }
        
    }
}

using System;
using com.tacitknowledge.util.migration;

namespace com.tacitknowledge.testhelpers
{
    public class MigrationTask1 : MigrationTaskSupport
    {
        public MigrationTask1()
        {
            Name = "dummy_patch";
            Level = 1;
        }

        public override void Migrate(IMigrationContext context)
        {
            TestMigrationContext ctx = (TestMigrationContext)context;
            ctx.RecordExecution(Name);
        }
    }
}

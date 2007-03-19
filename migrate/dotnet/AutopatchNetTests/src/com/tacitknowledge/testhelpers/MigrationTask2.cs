using System;
using com.tacitknowledge.util.migration;

namespace com.tacitknowledge.testhelpers
{
    public class MigrationTask2 : MigrationTaskSupport
    {
        private bool forceFail = false;

        public bool ForceFail
        {
            get { return forceFail; }
            set { forceFail = value; }
        }
        
        public MigrationTask2()
        {
            Name = "another_dummy_patch";
            Level = 2;
        }

        public override void Migrate(IMigrationContext context)
        {
            if (!ForceFail)
            {
                TestMigrationContext ctx = (TestMigrationContext)context;
                ctx.RecordExecution(Name);
            }
            else
            {
                throw new MigrationException("Something went wrong running '" + Name + "'");
            }
        }
    }
}

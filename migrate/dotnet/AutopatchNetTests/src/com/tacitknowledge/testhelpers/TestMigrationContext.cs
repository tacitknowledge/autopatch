using System;
using System.Collections.Generic;
using com.tacitknowledge.util.migration;

namespace com.tacitknowledge.testhelpers
{
    public class TestMigrationContext : IMigrationContext
    {
        /// <summary>
        /// A record of task executions.
        /// </summary>
        private IDictionary<string, bool> executionLog = new Dictionary<string, bool>();

        public void Commit()
        {
            // does nothing
        }
        
        public void Rollback()
        {
            // does nothing
        }

        public void RecordExecution(String taskName)
        {
            executionLog.Add(taskName, true);
        }

        public bool HasExecuted(String taskName)
        {
            return executionLog.ContainsKey(taskName);
        }
    }
}

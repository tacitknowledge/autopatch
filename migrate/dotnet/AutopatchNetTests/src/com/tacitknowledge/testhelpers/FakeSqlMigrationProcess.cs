/* 
 * Copyright 2007 Tacit Knowledge LLC
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
#region Imports
using System;
using com.tacitknowledge.util.migration;
using com.tacitknowledge.util.migration.ado;
#endregion

namespace com.tacitknowledge.testhelpers
{
    /// <summary>
    /// A sample migration process for testing SQL-file patches. It circumvents actual task migration.
    /// This allows us to test SQL-file patches in the overall framework.
    /// </summary>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public class FakeSqlMigrationProcess : MigrationProcess
    {
        /// <summary>
        /// Circumvents actual SQL task migration code and records its "execution" to the test migration
        /// context. It allows the actual execution of .NET code patches.
        /// </summary>
        /// <param name="context">the migration context</param>
        /// <param name="task">the task to migrate</param>
        public override void MigrateTask(IMigrationContext context, IMigrationTask task)
        {
            if (task is SqlScriptMigrationTask)
            {
                ((TestMigrationContext)context).RecordExecution(task.Name);
            }
            else
            {
                base.MigrateTask(context, task);
            }
        }
    }
}

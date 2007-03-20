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
#endregion

namespace com.tacitknowledge.testhelpers
{
    /// <summary>
    /// A sample migration task implemented through a .NET class.
    /// </summary>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
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

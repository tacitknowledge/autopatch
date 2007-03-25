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
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
    /// <summary>
    /// A unit test for verifying functionality of <code>SqlScriptMigrationTaskSource</code>.
    /// </summary>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    [TestFixture]
    public class SqlScriptMigrationTaskSourceTest
    {
        /// <summary>
        /// Make sure class instantiation fails on null resource path.
        /// </summary>
        [Test]
        [ExpectedException(typeof(MigrationException))]
        public void InstantiateTasksNullResourcePath()
        {
            SqlScriptMigrationTaskSource source = new SqlScriptMigrationTaskSource();
            source.GetMigrationTasks(null);
        }

        /// <summary>
        /// Make sure class instantiation does not fail on empty resource path.
        /// </summary>
        [Test]
        public void InstantiateTasksNoTasks()
        {
            SqlScriptMigrationTaskSource source = new SqlScriptMigrationTaskSource();
            IList<IMigrationTask> tasks =
                source.GetMigrationTasks(Directory.GetCurrentDirectory() + "\\..\\..\\sql\\empty-directory");

            Assert.AreEqual(0, tasks.Count);
        }

        /// <summary>
        /// Make sure class instantiation is working correctly.
        /// </summary>
        [Test]
        public void InstantiateTasksSuccess()
        {
            SqlScriptMigrationTaskSource source = new SqlScriptMigrationTaskSource();
            IList<IMigrationTask> tasks =
                source.GetMigrationTasks(Directory.GetCurrentDirectory() + "\\..\\..\\sql");

            Assert.Less(0, tasks.Count);
        }
    }
}

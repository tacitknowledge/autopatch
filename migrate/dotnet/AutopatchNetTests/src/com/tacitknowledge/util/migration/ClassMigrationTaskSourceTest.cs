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
using System.Collections.Generic;
using NUnit.Framework;
using com.tacitknowledge.testhelpers;
using com.tacitknowledge.util.migration;
#endregion

namespace com.tacitknowledge.util.migration
{
    /// <summary>
    /// A unit test for verifying functionality of <code>ClassMigrationTaskSource</code>.
    /// </summary>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    [TestFixture]
    public class ClassMigrationTaskSourceTest
    {
        /// <summary>
        /// Make sure class instantiation fails on null assembly path.
        /// </summary>
        [Test]
        [ExpectedException(typeof(MigrationException))]
        public void InstantiateTasksNullAssemblyPath()
        {
            ClassMigrationTaskSource source = new ClassMigrationTaskSource();
            source.GetMigrationTasks(null);
        }

        /// <summary>
        /// Make sure class instantiation does not fail on inexistent assembly.
        /// </summary>
        [Test]
        public void InstantiateTasksNoTasks()
        {
            ClassMigrationTaskSource source = new ClassMigrationTaskSource();
            IList<IMigrationTask> tasks = source.GetMigrationTasks("C:/Test.DLL");

            Assert.AreEqual(0, tasks.Count);
        }

        /// <summary>
        /// Make sure class instantiation is working correctly.
        /// </summary>
        [Test]
        public void InstantiateTasksSuccess()
        {
            ClassMigrationTaskSource source = new ClassMigrationTaskSource();
            IList<IMigrationTask> tasks =
                //source.GetMigrationTasks(typeof(MigrationTask1).Assembly.CodeBase);
                source.GetMigrationTasks(typeof(MigrationTask1).Assembly.Location);

            Assert.Less(0, tasks.Count);
        }
    }
}

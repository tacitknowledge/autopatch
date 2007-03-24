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
using System.IO;
using NUnit.Framework;
using com.tacitknowledge.testhelpers;
using com.tacitknowledge.util.migration;
using com.tacitknowledge.util.migration.ado;
#endregion

namespace com.tacitknowledge.util.migration
{
    /// <summary>
    /// A unit test for verifying core functionality of <code>MigrationProcess</code>.
    /// </summary>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    [TestFixture]
    public class MigrationProcessTest
    {
        /// <summary>
        /// Make sure adding null task sources fails.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddNullMigrationTaskSource()
        {
            MigrationProcess process = new MigrationProcess();
            process.AddMigrationTaskSource(null);
        }

        /// <summary>
        /// Make sure that all assembly migrations were run from level 0.
        /// </summary>
        [Test]
        public void DoMigrationsFromLevelZero()
        {
            TestMigrationContext context = new TestMigrationContext();
            MigrationProcess process = new MigrationProcess();
            //process.AddPatchResourceAssembly(typeof(MigrationTask1).Assembly.CodeBase);
            process.AddPatchResourceAssembly(typeof(MigrationTask1).Assembly.Location);
            process.DoMigrations(0, context);
            
            Assert.IsTrue(context.HasExecuted(new MigrationTask1().Name), "MigrationTask1 was supposed to be run");
            Assert.IsTrue(context.HasExecuted(new MigrationTask2().Name), "MigrationTask2 was supposed to be run");
        }
        
        /// <summary>
        /// Make sure that only assembly migrations with level more than 1 were run.
        /// </summary>
        [Test]
        public void DoMigrationsFromLevelOne()
        {
            TestMigrationContext context = new TestMigrationContext();
            MigrationProcess process = new MigrationProcess();
            //process.AddPatchResourceAssembly(typeof(MigrationTask1).Assembly.CodeBase);
            process.AddPatchResourceAssembly(typeof(MigrationTask1).Assembly.Location);
            process.DoMigrations(1, context);

            Assert.IsFalse(context.HasExecuted(new MigrationTask1().Name), "MigrationTask1 was not supposed to be run");
            Assert.IsTrue(context.HasExecuted(new MigrationTask2().Name), "MigrationTask2 was supposed to be run");
        }

        /// <summary>
        /// Make sure that no assembly migrations are run on an up-to-date system.
        /// </summary>
        [Test]
        public void DoMigrationsFromLevelOneHundred()
        {
            TestMigrationContext context = new TestMigrationContext();
            MigrationProcess process = new MigrationProcess();
            //process.AddPatchResourceAssembly(typeof(MigrationTask1).Assembly.CodeBase);
            process.AddPatchResourceAssembly(typeof(MigrationTask1).Assembly.Location);
            int count = process.DoMigrations(100, context);

            Assert.IsTrue(0 == count, "No migration tasks were supposed to be run on an up-to-date system");
            Assert.IsFalse(context.HasExecuted(new MigrationTask1().Name), "MigrationTask1 was not supposed to be run");
            Assert.IsFalse(context.HasExecuted(new MigrationTask2().Name), "MigrationTask2 was not supposed to be run");
        }

        /// <summary>
        /// Make sure that we validate the fact that a task's level might not be set.
        /// </summary>
        [Test]
        [ExpectedException(typeof(MigrationException))]
        public void ValidateTasksNoLevelSet()
        {
            IList<IMigrationTask> migrations = new List<IMigrationTask>();
            MigrationTask1 task = new MigrationTask1();
            task.Level = null;
            migrations.Add(task);
            
            try
            {
                MigrationProcess process = new MigrationProcess();
                process.ValidateTasks(migrations);
            }
            catch (MigrationException me)
            {
                Assert.AreEqual(me.Message, "Migration task '" + task.Name + " ["
                    + typeof(MigrationTask1).FullName + "]' does not have a patch level defined.");
                throw me;
            }
        }

        /// <summary>
        /// Make sure that we validate the fact that no two tasks can have the same level.
        /// </summary>
        [Test]
        [ExpectedException(typeof(MigrationException))]
        public void ValidateTasksTwoSameLevels()
        {
            IList<IMigrationTask> migrations = new List<IMigrationTask>();
            MigrationTask1 task1 = new MigrationTask1();
            task1.Level = 1;
            MigrationTask2 task2 = new MigrationTask2();
            task2.Level = 1;
            
            migrations.Add(task1);
            migrations.Add(task2);

            try
            {
                MigrationProcess process = new MigrationProcess();
                process.ValidateTasks(migrations);
            }
            catch (MigrationException me)
            {
                Assert.AreEqual(me.Message, "Migration task '" + task2.Name + " ["
                    + typeof(MigrationTask2).FullName + "]' has a conflicting patch level with '"
                    + task1.Name + " [" + typeof(MigrationTask1).FullName
                    + "]'; both are configured for patch level 1");
                throw me;
            }
        }

        /// <summary>
        /// Make sure that good scenario passes.
        /// </summary>
        [Test]
        public void ValidateTasksSuccessful()
        {
            IList<IMigrationTask> migrations = new List<IMigrationTask>();
            MigrationTask1 task1 = new MigrationTask1();
            MigrationTask2 task2 = new MigrationTask2();

            migrations.Add(task1);
            migrations.Add(task2);

            try
            {
                MigrationProcess process = new MigrationProcess();
                process.ValidateTasks(migrations);
            }
            catch (MigrationException)
            {
                Assert.Fail("We should not have got an exception");
            }
        }

        /// <summary>
        /// Make sure that post-patch migration process does not run when no tasks are found.
        /// </summary>
        [Test]
        public void DoPostPatchMigrationsNoTasks()
        {
            TestMigrationContext context = new TestMigrationContext();
            FakeSqlMigrationProcess process = new FakeSqlMigrationProcess();
            SqlScriptMigrationTaskSource taskSource = new SqlScriptMigrationTaskSource();
            process.AddPostPatchResourceDirectory(Directory.GetCurrentDirectory() + "\\..\\..\\sql\\empty-directory");
            process.AddMigrationTaskSource(taskSource);
            int tasksRun = process.DoPostPatchMigrations(context);

            Assert.AreEqual(0, tasksRun);
        }

        /// <summary>
        /// Make sure that SQL script post-patch migrations work in conjunction with .NET code
        /// post-patch migrations.
        /// </summary>
        [Test]
        public void DoPostPatchMigrationsFromSqlFileAndClasses()
        {
            TestMigrationContext context = new TestMigrationContext();
            FakeSqlMigrationProcess process = new FakeSqlMigrationProcess();
            SqlScriptMigrationTaskSource taskSource = new SqlScriptMigrationTaskSource();
            process.AddPostPatchResourceAssembly(typeof(MigrationTask1).Assembly.Location);
            process.AddPostPatchResourceDirectory(Directory.GetCurrentDirectory() + "\\..\\..");
            process.AddMigrationTaskSource(taskSource);
            process.DoPostPatchMigrations(context);

            // There currently are 2 .NET code and 3 SQL patches for tests
            Assert.IsTrue(context.HasExecuted(new MigrationTask1().Name),
                "MigrationTask1 was supposed to be run");
            Assert.IsTrue(context.HasExecuted(new MigrationTask2().Name),
                "MigrationTask2 was supposed to be run");
            Assert.IsTrue(context.HasExecuted("patch0003_dummy_SQL_file"),
                "patch0003_dummy_SQL_file was supposed to be run");
            Assert.IsTrue(context.HasExecuted("patch0004_fourth_patch"),
                "patch0004_fourth_patch was supposed to be run");
            Assert.IsTrue(context.HasExecuted("patch0005_fifth_patch"),
                "patch0005_fifth_patch was supposed to be run");
        }

        /// <summary>
        /// Make sure that patch level calculation is working correctly when no tasks are found.
        /// </summary>
        [Test]
        public void GetNextPatchLevelNoTasks()
        {
            TestMigrationContext context = new TestMigrationContext();
            FakeSqlMigrationProcess process = new FakeSqlMigrationProcess();
            SqlScriptMigrationTaskSource taskSource = new SqlScriptMigrationTaskSource();
            process.AddPatchResourceDirectory(Directory.GetCurrentDirectory() + "\\..\\..\\sql\\empty-directory");
            process.AddMigrationTaskSource(taskSource);
            int nextLevel = process.NextPatchLevel;

            Assert.AreEqual(nextLevel, 1);
        }

        /// <summary>
        /// Make sure that patch level calculation is working correctly.
        /// </summary>
        [Test]
        public void GetNextPatchLevel()
        {
            TestMigrationContext context = new TestMigrationContext();
            FakeSqlMigrationProcess process = new FakeSqlMigrationProcess();
            SqlScriptMigrationTaskSource taskSource = new SqlScriptMigrationTaskSource();
            process.AddPatchResourceDirectory(Directory.GetCurrentDirectory() + "\\..\\..\\sql");
            process.AddMigrationTaskSource(taskSource);
            int nextLevel = process.NextPatchLevel;

            Assert.AreEqual(nextLevel, 6);
        }

        /// <summary>
        /// Make sure that SQL script migrations work.
        /// </summary>
        [Test]
        public void DoMigrationsFromSqlFile()
        {
            TestMigrationContext context = new TestMigrationContext();
            FakeSqlMigrationProcess process = new FakeSqlMigrationProcess();
            SqlScriptMigrationTaskSource taskSource = new SqlScriptMigrationTaskSource();
            process.AddPatchResourceDirectory(Directory.GetCurrentDirectory() + "\\..\\..");
            process.AddMigrationTaskSource(taskSource);
            process.DoMigrations(2, context);

            Assert.IsTrue(context.HasExecuted("patch0003_dummy_SQL_file"),
                "patch0003_dummy_SQL_file was supposed to be run");
            Assert.IsTrue(context.HasExecuted("patch0004_fourth_patch"),
                "patch0004_fourth_patch was supposed to be run");
            Assert.IsTrue(context.HasExecuted("patch0005_fifth_patch"),
                "patch0005_fifth_patch was supposed to be run");
        }

        /// <summary>
        /// Make sure that SQL script migrations work in conjunction with .NET code migrations.
        /// </summary>
        [Test]
        public void DoMigrationsFromSqlFileAndClasses()
        {
            TestMigrationContext context = new TestMigrationContext();
            FakeSqlMigrationProcess process = new FakeSqlMigrationProcess();
            SqlScriptMigrationTaskSource taskSource = new SqlScriptMigrationTaskSource();
            process.AddPatchResourceAssembly(typeof(MigrationTask1).Assembly.Location);
            process.AddPatchResourceDirectory(Directory.GetCurrentDirectory() + "\\..\\..");
            process.AddMigrationTaskSource(taskSource);
            process.DoMigrations(0, context);

            // There currently are 2 .NET code and 3 SQL patches for tests
            Assert.IsTrue(context.HasExecuted(new MigrationTask1().Name),
                "MigrationTask1 was supposed to be run");
            Assert.IsTrue(context.HasExecuted(new MigrationTask2().Name),
                "MigrationTask2 was supposed to be run");
            Assert.IsTrue(context.HasExecuted("patch0003_dummy_SQL_file"),
                "patch0003_dummy_SQL_file was supposed to be run");
            Assert.IsTrue(context.HasExecuted("patch0004_fourth_patch"),
                "patch0004_fourth_patch was supposed to be run");
            Assert.IsTrue(context.HasExecuted("patch0005_fifth_patch"),
                "patch0005_fifth_patch was supposed to be run");
        }
    }
}

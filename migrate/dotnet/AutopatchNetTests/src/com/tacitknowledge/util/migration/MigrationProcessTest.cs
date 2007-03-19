using System;
using System.Collections.Generic;
using NUnit.Framework;
using com.tacitknowledge.testhelpers;
using com.tacitknowledge.util.migration;

namespace com.tacitknowledge.util.migration
{
    [TestFixture]
    public class MigrationProcessTest
    {
        /// <summary>
        /// Make sure adding null task sources fails.
        /// </summary>
        [Test]
        public void AddNullMigrationTaskSource()
        {
            MigrationProcess process = new MigrationProcess();

            try
            {
                process.AddMigrationTaskSource(null);

                Assert.Fail("We should have gotten an exception for the null assembly path");
            }
            catch (ArgumentException)
            {
                // we expect this
            }
        }

        /// <summary>
        /// Make sure that all assembly migrations were run from level 0.
        /// </summary>
        [Test]
        public void DoMigrationsFromLevelZero()
        {
            TestMigrationContext context = new TestMigrationContext();
            MigrationProcess process = new MigrationProcess();
            process.AddPatchResourceAssembly(typeof(MigrationTask1).Assembly.CodeBase);
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
            process.AddPatchResourceAssembly(typeof(MigrationTask1).Assembly.CodeBase);
            int tasksRun = process.DoMigrations(1, context);

            Assert.IsFalse(context.HasExecuted(new MigrationTask1().Name), "MigrationTask1 was not supposed to be run");
            Assert.IsTrue(context.HasExecuted(new MigrationTask2().Name), "MigrationTask2 was supposed to be run");
        }

        /// <summary>
        /// Make sure that we validate the fact that a task's level might not be set.
        /// </summary>
        [Test]
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

                Assert.Fail("We should have gotten an exception for the null migration task level");
            }
            catch (MigrationException me)
            {
                Assert.AreEqual(me.Message, "Migration task '" + task.Name + " ["
                    + typeof(MigrationTask1).FullName + "]' does not have a patch level defined.");
            }
        }

        /// <summary>
        /// Make sure that we validate the fact that no two tasks can have the same level.
        /// </summary>
        [Test]
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

                Assert.Fail("We should have gotten an exception for miltiple migration tasks with same level");
            }
            catch (MigrationException me)
            {
                Assert.AreEqual(me.Message, "Migration task '" + task2.Name + " ["
                    + typeof(MigrationTask2).FullName + "]' has a conflicting patch level with '"
                    + task1.Name + " [" + typeof(MigrationTask1).FullName
                    + "]'; both are configured for patch level 1");
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
            catch (MigrationException me)
            {
                Assert.Fail("We should not have gotten an exception");
            }
        }
    }
}

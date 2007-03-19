using System;
using System.Collections.Generic;
using NUnit.Framework;
using com.tacitknowledge.testhelpers;
using com.tacitknowledge.util.migration;

namespace com.tacitknowledge.util.migration
{
    [TestFixture]
    public class ClassMigrationTaskSourceTest
    {
        /// <summary>
        /// Make sure class instantiation fails on null assembly path.
        /// </summary>
        [Test]
        public void InstantiateTasksNullAssemblyPath()
        {
            ClassMigrationTaskSource source = new ClassMigrationTaskSource();

            try
            {
                source.GetMigrationTasks(null);

                Assert.Fail("We should have gotten an exception for the null assembly path");
            }
            catch (MigrationException)
            {
                // we expect this
            }
        }

        /// <summary>
        /// Make sure class instantiation does not fail on inexistent assembly.
        /// </summary>
        [Test]
        public void InstantiateTasksNoTasks()
        {
            ClassMigrationTaskSource source = new ClassMigrationTaskSource();

            try
            {
                IList<IMigrationTask> tasks = source.GetMigrationTasks("C:/Test.DLL");

                Assert.AreEqual(0, tasks.Count);
            }
            catch (MigrationException)
            {
                Assert.Fail("We should not have gotten an exception");
            }
        }

        /// <summary>
        /// Make sure class instantiation is working correctly.
        /// </summary>
        [Test]
        public void InstantiateTasksSuccess()
        {
            ClassMigrationTaskSource source = new ClassMigrationTaskSource();

            try
            {
                IList<IMigrationTask> tasks =
                    source.GetMigrationTasks(typeof(MigrationTask1).Assembly.CodeBase);

                Assert.Less(0, tasks.Count);
            }
            catch (MigrationException)
            {
                Assert.Fail("We should not have gotten an exception");
            }
        }
    }
}

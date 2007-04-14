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
using Rhino.Mocks;
using com.tacitknowledge.testhelpers;
using com.tacitknowledge.util.migration;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
    /// <summary>
    /// A unit test for verifying core functionality of <code>AdoMigrationLauncher</code>.
    /// </summary>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    [TestFixture]
    public class AdoMigrationLauncherTest
    {
        private static readonly String pathCode = Directory.GetCurrentDirectory();
        private static readonly String pathSql = pathCode + "\\..\\..\\sql";
        private static readonly String paths = pathSql + ";" + pathCode;

        [Test]
        public void TestTokenizePatchPath()
        {
            AdoMigrationLauncher launcher = new AdoMigrationLauncher();
            launcher.PatchPath = paths;
            MigrationProcess process = launcher.MigrationProcess;

            Assert.Less(0, process.MigrationTasks.Count);
        }

        [Test]
        public void TestTokenizePostPatchPath()
        {
            AdoMigrationLauncher launcher = new AdoMigrationLauncher();
            launcher.PostPatchPath = paths;
            MigrationProcess process = launcher.MigrationProcess;

            Assert.Less(0, process.PostPatchMigrationTasks.Count);
        }

        [Test]
        public void TestNextPatchLevel()
        {
            AdoMigrationLauncher launcher = new AdoMigrationLauncher();
            launcher.PatchPath = paths;

            Assert.Less(5, launcher.NextPatchLevel);
        }

        [Test]
        [ExpectedException(typeof(MigrationException))]
        public void TestDoMigrationsNoContexts()
        {
            AdoMigrationLauncher launcher = new AdoMigrationLauncher();
            launcher.DoMigrations();
        }

        [Test]
        public void TestDoMigrationsSingleContext()
        {
            TestAdoMigrationContext context = new TestAdoMigrationContext();
            context.DatabaseType = new DatabaseType("sqlserver");

            MockRepository mocks = new MockRepository();
            PatchTable piStore = mocks.CreateMock<PatchTable>(new object[] { context });

            TestAdoMigrationLauncher launcher = new TestAdoMigrationLauncher(context);
            launcher.PatchPath = paths;

            // Make the launcher work with mock objects
            launcher.Contexts.Clear();
            launcher.Contexts.Add(context, piStore);

            Expect.Call(piStore.PatchLevel).Return(0).Repeat.Times(3);
            Expect.Call(piStore.PatchStoreLocked).Return(false);
            piStore.LockPatchStore();
            LastCall.On(piStore).Repeat.Once();
            piStore.UnlockPatchStore();
            LastCall.On(piStore).Repeat.Once();
            piStore.UpdatePatchLevel(1);
            LastCall.On(piStore).Repeat.Once();
            piStore.UpdatePatchLevel(2);
            LastCall.On(piStore).Repeat.Once();
            piStore.UpdatePatchLevel(3);
            LastCall.On(piStore).Repeat.Once();
            piStore.UpdatePatchLevel(4);
            LastCall.On(piStore).Repeat.Once();
            piStore.UpdatePatchLevel(5);
            LastCall.On(piStore).Repeat.Once();
            mocks.ReplayAll();

            int executedPatches = launcher.DoMigrations();

            Assert.IsTrue(5 <= executedPatches, "There were 5 patches supposed to be run");
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

            mocks.VerifyAll();
        }

        [Test]
        public void TestDoMigrationsMultipleContexts()
        {
            TestAdoMigrationContext context1 = new TestAdoMigrationContext();
            context1.DatabaseType = new DatabaseType("sqlserver");
            TestAdoMigrationContext context2 = new TestAdoMigrationContext();
            context2.DatabaseType = new DatabaseType("sqlserver");

            MockRepository mocks = new MockRepository();
            PatchTable piStore1 = mocks.CreateMock<PatchTable>(new object[] { context1 });
            PatchTable piStore2 = mocks.CreateMock<PatchTable>(new object[] { context2 });

            TestAdoMigrationLauncher launcher = new TestAdoMigrationLauncher(context1);
            launcher.AddContext(context2);
            launcher.PatchPath = paths;

            // Make the launcher work with mock objects
            launcher.Contexts.Clear();
            launcher.Contexts.Add(context1, piStore1);
            launcher.Contexts.Add(context2, piStore2);

            Expect.Call(piStore1.PatchLevel).Return(0).Repeat.Times(3);
            Expect.Call(piStore1.PatchStoreLocked).Return(false);
            piStore1.LockPatchStore();
            LastCall.On(piStore1).Repeat.Once();
            piStore1.UnlockPatchStore();
            LastCall.On(piStore1).Repeat.Once();
            piStore1.UpdatePatchLevel(1);
            LastCall.On(piStore1).Repeat.Twice();
            piStore1.UpdatePatchLevel(2);
            LastCall.On(piStore1).Repeat.Twice();
            piStore1.UpdatePatchLevel(3);
            LastCall.On(piStore1).Repeat.Twice();
            piStore1.UpdatePatchLevel(4);
            LastCall.On(piStore1).Repeat.Twice();
            piStore1.UpdatePatchLevel(5);
            LastCall.On(piStore1).Repeat.Twice();
            
            Expect.Call(piStore2.PatchLevel).Return(0).Repeat.Times(3);
            Expect.Call(piStore2.PatchStoreLocked).Return(false);
            piStore2.LockPatchStore();
            LastCall.On(piStore2).Repeat.Once();
            piStore2.UnlockPatchStore();
            LastCall.On(piStore2).Repeat.Once();
            piStore2.UpdatePatchLevel(1);
            LastCall.On(piStore2).Repeat.Twice();
            piStore2.UpdatePatchLevel(2);
            LastCall.On(piStore2).Repeat.Twice();
            piStore2.UpdatePatchLevel(3);
            LastCall.On(piStore2).Repeat.Twice();
            piStore2.UpdatePatchLevel(4);
            LastCall.On(piStore2).Repeat.Twice();
            piStore2.UpdatePatchLevel(5);
            LastCall.On(piStore2).Repeat.Twice();
            mocks.ReplayAll();

            launcher.DoMigrations();

            // There currently are 2 .NET code and 3 SQL patches for tests
            Assert.IsTrue(context1.HasExecuted(new MigrationTask1().Name),
                "MigrationTask1 was supposed to be run");
            Assert.IsTrue(context1.HasExecuted(new MigrationTask2().Name),
                "MigrationTask2 was supposed to be run");
            Assert.IsTrue(context1.HasExecuted("patch0003_dummy_SQL_file"),
                "patch0003_dummy_SQL_file was supposed to be run");
            Assert.IsTrue(context1.HasExecuted("patch0004_fourth_patch"),
                "patch0004_fourth_patch was supposed to be run");
            Assert.IsTrue(context1.HasExecuted("patch0005_fifth_patch"),
                "patch0005_fifth_patch was supposed to be run");

            Assert.IsTrue(context2.HasExecuted(new MigrationTask1().Name),
                "MigrationTask1 was supposed to be run");
            Assert.IsTrue(context2.HasExecuted(new MigrationTask2().Name),
                "MigrationTask2 was supposed to be run");
            Assert.IsTrue(context2.HasExecuted("patch0003_dummy_SQL_file"),
                "patch0003_dummy_SQL_file was supposed to be run");
            Assert.IsTrue(context2.HasExecuted("patch0004_fourth_patch"),
                "patch0004_fourth_patch was supposed to be run");
            Assert.IsTrue(context2.HasExecuted("patch0005_fifth_patch"),
                "patch0005_fifth_patch was supposed to be run");

            mocks.VerifyAll();
        }
    }
}

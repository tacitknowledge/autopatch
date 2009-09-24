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
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using Microsoft.Practices.EnterpriseLibrary.Data;
using NUnit.Framework;
using Rhino.Mocks;
using com.tacitknowledge.testhelpers;
using com.tacitknowledge.util.migration;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
    /// <summary>
    /// A unit test for verifying functionality of <code>SqlScriptMigrationTask</code>.
    /// </summary>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    [TestFixture]
    public class SqlScriptMigrationTaskTest
    {
        /// <summary>
        /// Make sure that a string-initialized task succeeds.
        /// </summary>
        [Test]
        public void MigrateSqlString()
        {
            String sql = "INSERT INTO order_table_1 (id, value) VALUES (1, 'order_table_1')";
            MockRepository mocks = new MockRepository();
            IAdoMigrationContext context = mocks.CreateMock<IAdoMigrationContext>();
            FakeDatabase db =
                mocks.CreateMock<FakeDatabase>(new object[] {"connString", SqlClientFactory.Instance});
            DbTransaction trans = mocks.CreateMock<DbTransaction>();
            DbCommand cmd = mocks.CreateMock<DbCommand>();
            DatabaseType dbType = mocks.CreateMock<DatabaseType>("sqlserver");

            Expect.Call(context.DatabaseType).Return(dbType);
            Expect.Call(context.Database).Return(db);
            context.Commit();
            LastCall.On(context).Repeat.Once();
            Expect.Call(context.Transaction).Return(trans);
            Expect.Call(db.ExecuteNonQuery(cmd, trans)).IgnoreArguments().Return(1);

            mocks.ReplayAll();

            SqlScriptMigrationTask task = new SqlScriptMigrationTask("test", 1, sql);

            try
            {
                task.Migrate(context);
            }
            catch (MigrationException)
            {
                Assert.Fail("We should not have got an exception");
            }

            mocks.VerifyAll();
        }

        /// <summary>
        /// Make sure that StreamReader-initialized task with multiple SQL statements succeeds.
        /// </summary>
        [Test]
        public void MigrateSqlScript()
        {
            MockRepository mocks = new MockRepository();
            IAdoMigrationContext context = mocks.CreateMock<IAdoMigrationContext>();
            FakeDatabase db =
                mocks.CreateMock<FakeDatabase>(new object[] { "connString", SqlClientFactory.Instance });
            DbTransaction trans = mocks.CreateMock<DbTransaction>();
            DbCommand cmd = mocks.CreateMock<DbCommand>();
            DatabaseType dbType = mocks.CreateMock<DatabaseType>("sqlserver");

            // We have 3 DatabaseType property access on the execution path
            Expect.Call(context.DatabaseType).Return(dbType).Repeat.Times(3);
            Expect.Call(context.Database).Return(db);
            context.Commit();
            LastCall.On(context).Repeat.Once();
            // CREATE TABLE statement
            Expect.Call(context.Transaction).Return(trans);
            Expect.Call(db.ExecuteNonQuery(cmd, trans)).IgnoreArguments().Return(1);
            // INSERT statement
            Expect.Call(context.Transaction).Return(trans);
            Expect.Call(db.ExecuteNonQuery(cmd, trans)).IgnoreArguments().Return(1);

            mocks.ReplayAll();

            using (StreamReader sr = File.OpenText("..\\..\\sql\\oracle\\patch0003_dummy_SQL_file.sql"))
            {
                try
                {
                    SqlScriptMigrationTask task =
                        new SqlScriptMigrationTask("patch0003_dummy_SQL_file", 3, sr);
                    task.Migrate(context);
                }
                catch (MigrationException)
                {
                    Assert.Fail("We should not have got an exception");
                }
                finally
                {
                    sr.Close();
                }
            }

            mocks.VerifyAll();
        }

        /// <summary>
        /// Make sure that the task can correctly parse multiple SQL statements from a single file,
        /// with embedded comments.
        /// </summary>
        [Test]
        public void ParsingMultipleStatements1()
        {
            MockRepository mocks = new MockRepository();
            IAdoMigrationContext context = mocks.CreateMock<IAdoMigrationContext>();
            DatabaseType dbType = mocks.CreateMock<DatabaseType>("sqlserver");

            using (mocks.Ordered())
            {
                // We have 4 DatabaseType property accesses on the execution path
                Expect.Call(context.DatabaseType).Return(dbType).Repeat.Times(4);
            }

            mocks.ReplayAll();

            using (StreamReader sr = File.OpenText("..\\..\\sql\\oracle\\patch0004_fourth_patch.sql"))
            {
                try
                {
                    SqlScriptMigrationTask task =
                        new SqlScriptMigrationTask("patch0004_fourth_patch", 4, sr);
                    IList<String> list = task.GetSqlStatements(context);

                    Assert.AreEqual(3, list.Count);
                    Assert.AreEqual("insert into user_role_assoc (user_role_id, application_user_id, "
                        + "role_code, project_id) " + Environment.NewLine
                        + "\t\t\tvalues (nextval('role_id_seq'),2, 'SYSA', 3)", list[0]);
                    Assert.AreEqual("insert into user_role_assoc (user_role_id, application_user_id, "
                        + "role_code, project_id) " + Environment.NewLine
                        + "\t\t\tvalues (nextval('role_id_seq'),3, 'SYSA', 3)", list[1]);
                    Assert.AreEqual("insert into user_role_assoc (user_role_id, application_user_id, "
                        + "role_code, project_id) " + Environment.NewLine
                        + "\t\t\tvalues (nextval('role_--id_seq;'),4, 'SYSA', 3)", list[2]);
                }
                catch (MigrationException)
                {
                    Assert.Fail("We should not have got an exception");
                }
                finally
                {
                    sr.Close();
                }
            }

            mocks.VerifyAll();
        }

        /// <summary>
        /// Make sure that the task can correctly parse multiple SQL statements from a single file,
        /// with embedded comments.
        /// </summary>
        [Test]
        public void ParsingMultipleStatements2()
        {
            MockRepository mocks = new MockRepository();
            IAdoMigrationContext context = mocks.CreateMock<IAdoMigrationContext>();
            DatabaseType dbType = mocks.CreateMock<DatabaseType>("sqlserver");

            using (mocks.Ordered())
            {
                Expect.Call(context.DatabaseType).Return(dbType);
            }

            mocks.ReplayAll();

            using (StreamReader sr = File.OpenText("..\\..\\sql\\oracle\\patch0005_fifth_patch.sql"))
            {
                try
                {
                    SqlScriptMigrationTask task =
                        new SqlScriptMigrationTask("patch0005_fifth_patch", 4, sr);
                    IList<String> list = task.GetSqlStatements(context);

                    Assert.AreEqual(1, list.Count);
                    Assert.AreEqual("insert into user_role_assoc (user_role_id, application_user_id, "
                        + "role_code, project_id) " + Environment.NewLine
                        + "\t\t\tvalues (nextval('role_id_seq'),2, 'SYSA', 3)", list[0]);
                }
                catch (MigrationException)
                {
                    Assert.Fail("We should not have got an exception");
                }
                finally
                {
                    sr.Close();
                }
            }

            mocks.VerifyAll();
        }

        /// <summary>
        /// Make sure that the code wraps <code>DbException</code>s into <code>MigrationException</code>s.
        /// </summary>
        [Test]
        [ExpectedException(typeof(MigrationException))]
        public void MigrateSqlStringWithDbException()
        {
            String sql = "INSERT INTO order_table_1 (id, value) VALUES (1, 'order_table_1')";
            MockRepository mocks = new MockRepository();
            IAdoMigrationContext context = mocks.CreateMock<IAdoMigrationContext>();
            FakeDatabase db =
                mocks.CreateMock<FakeDatabase>(new object[] { "connString", SqlClientFactory.Instance });
            DbTransaction trans = mocks.CreateMock<DbTransaction>();
            DbCommand cmd = mocks.CreateMock<DbCommand>();
            DatabaseType dbType = mocks.CreateMock<DatabaseType>("sqlserver");

            Expect.Call(context.DatabaseType).Return(dbType);
            Expect.Call(context.Database).Return(db);
            context.Commit();
            LastCall.On(context).Repeat.Once();
            Expect.Call(context.Transaction).Return(trans);
            Expect.Call(db.ExecuteNonQuery(cmd, trans)).IgnoreArguments()
                .Throw(new MigrationException("Something bad happened"));

            mocks.ReplayAll();

            try
            {
                SqlScriptMigrationTask task = new SqlScriptMigrationTask("test", 1, sql);
                task.Migrate(context);
            }
            catch (MigrationException me)
            {
                Assert.AreEqual("Something bad happened", me.InnerException.Message);
                mocks.VerifyAll();
                throw me;
            }
        }
    }
}

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
using NUnit.Framework;
using Rhino.Mocks;
using com.tacitknowledge.util.migration;
using com.tacitknowledge.util.migration.ado;
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
            String sql = "select * from authors";
            MockRepository mocks = new MockRepository();
            IAdoMigrationContext context = mocks.CreateMock<IAdoMigrationContext>();
            DbConnection conn = mocks.CreateMock<DbConnection>();
            DbCommand cmd = mocks.CreateMock<DbCommand>();
            DatabaseType dbType = mocks.CreateMock<DatabaseType>("postgres");

            using (mocks.Ordered())
            {
                Expect.Call(context.DatabaseType).Return(dbType);
                Expect.Call(context.Connection).Return(conn);
                context.Commit();
                LastCall.On(context).Repeat.Once();
                Expect.Call(conn.CreateCommand()).Return(cmd);
                cmd.CommandText = sql;
                Expect.Call(cmd.ExecuteNonQuery()).Return(1);
                cmd.Dispose();
                LastCall.On(cmd).Repeat.Once();
                context.Commit();
                LastCall.On(context).Repeat.Once();
            }

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
            DbConnection conn = mocks.CreateMock<DbConnection>();
            DbCommand cmd = mocks.CreateMock<DbCommand>();
            DatabaseType dbType = mocks.CreateMock<DatabaseType>("postgres");

            using (mocks.Ordered())
            {
                // We have 3 DatabaseType property access on the execution path
                Expect.Call(context.DatabaseType).Return(dbType).Repeat.Times(3);
                Expect.Call(context.Connection).Return(conn);
                context.Commit();
                LastCall.On(context).Repeat.Once();
                
                // CREATE TABLE statement
                Expect.Call(conn.CreateCommand()).Return(cmd);
                cmd.CommandText = "";
                LastCall.IgnoreArguments();
                Expect.Call(cmd.ExecuteNonQuery()).Return(1);
                cmd.Dispose();
                LastCall.On(cmd).Repeat.Once();
                
                // INSERT statement
                Expect.Call(conn.CreateCommand()).Return(cmd);
                cmd.CommandText = "";
                LastCall.IgnoreArguments();
                Expect.Call(cmd.ExecuteNonQuery()).Return(1);
                cmd.Dispose();
                LastCall.On(cmd).Repeat.Once();
                
                context.Commit();
                LastCall.On(context).Repeat.Once();
            }

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
            DatabaseType dbType = mocks.CreateMock<DatabaseType>("postgres");

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
            DatabaseType dbType = mocks.CreateMock<DatabaseType>("postgres");

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
            String sql = "select * from authors";
            MockRepository mocks = new MockRepository();
            IAdoMigrationContext context = mocks.CreateMock<IAdoMigrationContext>();
            DbConnection conn = mocks.CreateMock<DbConnection>();
            DbCommand cmd = mocks.CreateMock<DbCommand>();
            DatabaseType dbType = mocks.CreateMock<DatabaseType>("postgres");

            using (mocks.Ordered())
            {
                Expect.Call(context.DatabaseType).Return(dbType);
                Expect.Call(context.Connection).Return(conn);
                context.Commit();
                LastCall.On(context).Repeat.Once();
                Expect.Call(conn.CreateCommand()).Return(cmd);
                cmd.CommandText = sql;
                Expect.Call(cmd.ExecuteNonQuery()).Throw(new MigrationException("Something bad happened"));
                cmd.Dispose();
                LastCall.On(cmd).Repeat.Once();
                //context.Commit();
                //LastCall.On(context).Repeat.Once();
            }

            mocks.ReplayAll();

            SqlScriptMigrationTask task = new SqlScriptMigrationTask("test", 1, sql);
            task.Migrate(context);

            mocks.VerifyAll();
        }
    }
}

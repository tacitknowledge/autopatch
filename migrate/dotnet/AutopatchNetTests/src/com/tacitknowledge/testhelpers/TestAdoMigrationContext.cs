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
using System.Text;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using com.tacitknowledge.util.migration.ado;
#endregion

namespace com.tacitknowledge.testhelpers
{
    /// <summary>
    /// A sample ADO.NET migration context to be used in unit tests.
    /// </summary>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public class TestAdoMigrationContext : TestMigrationContext, IAdoMigrationContext
    {
        private Database database = null;
        private DbConnection connection = null;
        private DbTransaction transaction = null;
        private DatabaseType dbType = null;
        private String systemName = null;

        public Database Database
        {
            get { return database; }
            set { database = value; }
        }

        public DbConnection Connection
        {
            get { return connection; }
            set { connection = value; }
        }

        public DbTransaction Transaction
        {
            get { return transaction; }
            set { transaction = value; }
        }

        public DatabaseType DatabaseType
        {
            get { return dbType; }
            set { dbType = value; }
        }

        public String SystemName
        {
            get { return systemName; }
            set { systemName = value; }
        }

        public void CloseConnection(bool commitTransaction)
        {
            // does nothing
        }
    }
}

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
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
#endregion

namespace com.tacitknowledge.testhelpers
{
    /// <summary>
    /// A sample database for testing SQL-file patches. It circumvents actual task migration.
    /// This allows us to test SQL-file patches in the overall framework.
    /// </summary>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public class FakeDatabase : Database
    {
        public FakeDatabase(string connectionString, DbProviderFactory dbProviderFactory)
            : base(connectionString, dbProviderFactory)
        {}

        protected override void DeriveParameters(DbCommand discoveryCommand)
        {}
    }
}

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
#endregion

namespace com.tacitknowledge.util.migration.ado
{
    /// <summary>
    /// Contains the configuration and resources for a database patch run.
    /// </summary>
    /// <author>Scott Askew (scott@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public interface IAdoMigrationContext : IMigrationContext
    {
        #region Public properties
		/// <summary>
        /// The database connection to use.
		/// </summary>
		DbConnection Connection
		{
			get;
		}
		
        /// <summary>
        /// The name of the system to patch.
        /// </summary>
        String SystemName
        {
            get;
        }
		
		/// <summary>
        /// The database type.
		/// </summary>
        DatabaseType DatabaseType
        {
            get;
        }
        #endregion
    }
}

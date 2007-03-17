/*
 * Copyright 2006 Tacit Knowledge LLC
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
#endregion

namespace com.tacitknowledge.util.migration
{
    /// <summary>
    /// A single, idempotent migration task.
    /// </summary>
    /// <author>Scott Askew (scott@tacitknowledge.com)</author>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public interface IMigrationTask : IComparable<IMigrationTask>
    {
        #region Properties
        /// <summary>
        /// The name of this migration task.
        /// </summary>
        String Name
        {
            get;
        }

        /// <summary>
        /// The relative order in which this migration should occur.
        /// </summary>
        int? Level
        {
            get;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Performs a migration.
        /// </summary>
        /// <param name="context">the <code>IMigrationContext</code> for this run</param>
        /// <exception cref="MigrationException">if an unexpected error occurred</exception>
        void Migrate(IMigrationContext context);
        #endregion
    }
}

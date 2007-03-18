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
    /// Provides system resources to migration tasks.
    /// </summary>
    /// <author>Scott Askew (scott@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public interface IMigrationContext
    {
        #region Public methods
        /// <summary>
        /// Commits the current migration transaction.
        /// </summary>
        /// <exception cref="MigrationException">if there was an unrecoverable error committing the transaction</exception>
        void Commit();

        /// <summary>
        /// Rolls back the current migration transaction.
        /// </summary>
        /// <exception cref="MigrationException">if there was an unrecoverable error rolling the transaction back</exception>
        void Rollback();
        #endregion
    }
}

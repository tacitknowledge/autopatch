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
#endregion

namespace com.tacitknowledge.util.migration
{
    /// <summary>
    /// A source of <code>IMigrationTask</code>s.
    /// </summary>
    /// <author>Scott Askew (scott@tacitknowledge.com)</author>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public interface IMigrationTaskSource
    {
        #region Public methods
        /// <summary>
        /// Returns a list of <code>IMigrationTask</code>s that are in the given assembly.
        /// </summary>
        /// <param name="assemblyPath">assembly to search for migration tasks</param>
        /// <returns>A list of migration tasks. If no tasks were found, then an empty list must be returned.</returns>
        /// <exception cref="MigrationException">if an unrecoverable error occurred</exception>
        IList<IMigrationTask> GetMigrationTasks(String assemblyPath);
        #endregion
    }
}

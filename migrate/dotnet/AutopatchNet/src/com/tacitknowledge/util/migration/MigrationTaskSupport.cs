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
#endregion

namespace com.tacitknowledge.util.migration
{
    /// <summary>
    /// Convenience base class for migration tasks.
    /// </summary>
    /// <author>Scott Askew (scott@tacitknowledge.com)</author>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public abstract class MigrationTaskSupport : IMigrationTask
    {
        #region Member variables
        private String name;
        private int? level = null;
        #endregion

        #region Public properties
        /// <seealso cref="IMigrationTask.Name"/>
        public virtual String Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <seealso cref="IMigrationTask.Level"/>
        public int? Level
        {
            get { return level; }
            set { level = value; }
        }
        #endregion

        #region Public methods
        /// <seealso cref="IComparable.CompareTo(Object)"/>
        public int CompareTo(IMigrationTask task)
        {
            if (!task.Level.HasValue)
            {
                return 1;
            }

            Int32 taskLevel = task.Level.Value;
            Int32 curLevel = Level.Value;

            return curLevel.CompareTo(taskLevel);
        }
        #endregion

        #region Abstract methods
        /// <see cref="IMigrationTask.Migrate(IMigrationContext)"/>
        public abstract void Migrate(IMigrationContext context);
        #endregion
    }
}

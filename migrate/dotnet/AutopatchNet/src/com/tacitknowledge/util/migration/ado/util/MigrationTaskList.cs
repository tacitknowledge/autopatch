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
using log4net;
using log4net.Config;
using com.tacitknowledge.util.migration;
using com.tacitknowledge.util.migration.ado.util;
#endregion

namespace com.tacitknowledge.util.migration.ado.util
{
    /// <summary>
    /// A list of MigrationTasks
    /// </summary>
    public class MigrationTaskList : Hashlist
    {
        /// <summary>
        /// returns a IMigrationTask based on a key
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public new IMigrationTask this[string Key]
        {
            get { return (IMigrationTask)base[Key]; }
        }

        public new IMigrationTask this[int Index]
        {
            get
            {
                object oTemp = base[Index];
                return (IMigrationTask)oTemp;
            }
        }

        public MigrationTaskList()
        {

        }



    }
}

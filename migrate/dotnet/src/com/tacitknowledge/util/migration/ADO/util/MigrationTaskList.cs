using System;
using System.Collections.Generic;
using System.Text;

using log4net;
using log4net.Config;

using com.tacitknowledge.util.migration;
using com.tacitknowledge.util.migration.ado.util;

namespace AutopatchNET.src.com.tacitknowledge.util.migration.ADO.util
{
    /// <summary>
    /// A list of MigrationTasks
    /// </summary>
    public class MigrationTaskList : Hashlist
    {
        /// <summary>
        /// returns a MigrationTask based on a key
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public new MigrationTask this[string Key]
        {
            get { return (MigrationTask)base[Key]; }
        }

        public new MigrationTask this[int Index]
        {
            get
            {
                object oTemp = base[Index];
                return (MigrationTask)oTemp;
            }
        }

        public MigrationTaskList()
        {

        }



    }
}

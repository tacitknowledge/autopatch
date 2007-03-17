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

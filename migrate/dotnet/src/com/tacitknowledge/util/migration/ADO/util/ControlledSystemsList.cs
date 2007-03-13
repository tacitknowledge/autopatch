using System;
using System.Collections.Generic;
using System.Text;

using log4net;
using log4net.Config;

using com.tacitknowledge.util.migration.ado.util;

namespace AutopatchNET.src.com.tacitknowledge.util.migration.ADO.util
{
    /// <summary>
    /// A Hashlist of ControlledSystems. 
    /// Inherits from Hashlist which implements IDictionary and IEnumerable for easier looping.
    /// </summary>
   public class ControlledSystemsList : Hashlist
    {
        /// <summary>
        /// Returns a ControlledSystem object based on key
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public new ControlledSystem this[String Key]
        {

            get { return (ControlledSystem)base[Key]; }
        }
        /// <summary>
        /// Returns a ControlledSystem object based on number in the index
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public new ControlledSystem this[int Index]
        {
            get
            {
                object oTemp = base[Index];
                return (ControlledSystem)oTemp;
            }
        }

       public ControlledSystemsList()
       {

       }







    }
}

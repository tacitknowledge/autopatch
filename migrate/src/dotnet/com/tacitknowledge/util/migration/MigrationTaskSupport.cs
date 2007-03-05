/* Copyright 2006 Tacit Knowledge LLC
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
	
	/// <summary> Convenience base class for migration tasks.
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)x
	/// </author>
	public abstract class MigrationTaskSupport : MigrationTask
	{
		/// <summary> The name of this migration task</summary>
		private System.String name;
		
		/// <summary> The relative order in which this test should run</summary>
		private System.Int32 level;
		
		/// <seealso cref="MigrationTask.getName()">
		/// </seealso>
		public virtual System.String getName()
		{
			return name;
		}
		
		/// <summary> Sets the name of this migration task.
		/// 
		/// </summary>
		/// <param name="name">the name of this migration task
		/// </param>
		public virtual void  setName(System.String name)
		{
			this.name = name;
		}
		
		/// <seealso cref="MigrationTask.getLevel()">
		/// </seealso>
		public virtual System.Int32 getLevel()
		{
			return level;
		}
		
		/// <summary> Sets the relative order in which this test should run
		/// 
		/// </summary>
		/// <param name="lvl">the relative order in which this test should run
		/// </param>
		//UPGRADE_NOTE: ref keyword was added to struct-type parameters. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1303'"
		public virtual void  setLevel(ref System.Int32 lvl)
		{
			this.level = lvl;
		}
		
		/// <seealso cref="Comparable.compareTo(Object)">
		/// </seealso>
		public virtual int CompareTo(System.Object o)
		{
			MigrationTask task = (MigrationTask) o;
			//UPGRADE_TODO: The 'System.Int32' structure does not have an equivalent to NULL. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1291'"
			if (task.getLevel() == null)
			{
				return 1;
			}
			//UPGRADE_NOTE: Exceptions thrown by the equivalent in .NET of method 'java.lang.Integer.compareTo' may be different. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1099'"
			return getLevel().CompareTo(task.getLevel());
		}
		public abstract void  migrate(com.tacitknowledge.util.migration.MigrationContext param1);
	}
}
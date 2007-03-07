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
using log4net;
using log4net.Config;

#endregion
namespace com.tacitknowledge.util.migration
{
	
	/// <summary> Returns a list of all public, concrete classes that implement the
	/// <code>MigrationTask</code> in a specific package.
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	public class ClassMigrationTaskSource : MigrationTaskSource
    {
        #region Members
        /// <summary>Class logger </summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ClassMigrationTaskSource'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
        #endregion

        #region Methods
        /// <seealso cref="MigrationTaskSource.getMigrationTasks(String)">
		/// </seealso>
		public virtual System.Collections.IList getMigrationTasks(System.String packageName)
		{
			if (packageName == null)
			{
				throw new MigrationException("You must specify a package to get tasks for");
			}
			
			//System.Type[] taskClasses = ClassDiscoveryUtil.getClasses(packageName, typeof(MigrationTask));
			log.debug("Found " + taskClasses.Length + " patches in " + packageName);
			return instantiateTasks(taskClasses);
		}
		
		/// <summary> Instantiates the given classes
		/// 
		/// </summary>
		/// <param name="taskClasses">the classes instantiate
		/// </param>
		/// <returns> a list of <code>MigrationTasks</code>
		/// </returns>
		/// <throws>  MigrationException if a class could not be instantiated; this </throws>
		/// <summary>         is most likely due to the abscense of a default constructor
		/// </summary>
		private System.Collections.IList instantiateTasks(System.Type[] taskClasses)
		{
			System.Collections.IList tasks = new System.Collections.ArrayList();
			for (int i = 0; i < taskClasses.Length; i++)
			{
				System.Type taskClass = taskClasses[i];
				try
				{
					//UPGRADE_TODO: Method 'java.lang.Class.newInstance' was converted to 'System.Activator.CreateInstance' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangClassnewInstance'"
					System.Object o = System.Activator.CreateInstance(taskClass);
					
					// It's not legal to have a null name.
					MigrationTask task = (MigrationTask) o;
					if (task.getName() != null)
					{
						tasks.Add(o);
					}
					else
					{
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Class.getName' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
						log.warn("MigrationTask " + taskClass.FullName + " had no migration name. Is that intentional? Skipping task.");
					}
				}
				catch (System.Exception e)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Class.getName' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
					throw new MigrationException("Could not instantiate MigrationTask " + taskClass.FullName, e);
				}
			}
			return tasks;
		}
		static ClassMigrationTaskSource()
		{
			log = LogManager.GetLogger(typeof(ClassMigrationTaskSource));
		}
    }
        #endregion
}
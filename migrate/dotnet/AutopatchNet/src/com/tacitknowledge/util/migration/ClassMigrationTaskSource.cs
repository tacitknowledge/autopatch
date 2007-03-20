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
using System.Reflection;
using System.Runtime.Remoting;
using log4net;
#endregion

namespace com.tacitknowledge.util.migration
{
    /// <summary>
    /// <para>
    /// Returns a list of all public, concrete classes that implement the <code>IMigrationTask</code>
    /// in a specific assembly.
    /// <remarks>
    /// Class patches are deployed in assemblies, so this class expects a direct location (path)
    /// of the assembly to be passed in to query its information for classes it is interested in.
    /// </remarks>
    /// </para>
    /// <para>
    /// <example>Example 1 (using a hard-coded direct path):
    /// <code>
    /// IList &lt;IMigrationTask&gt; tasks = null;
    /// ClassMigrationTaskSource source = new ClassMigrationTaskSource();
    /// tasks = source.GetMigrationTasks("D:/Some/Project/Folder/TestLibrary.DLL");
    /// </code>
    /// </example>
    /// </para>
    /// <para>
    /// <example>Example 2 (extracting the path of a referenced assembly from one of its classes):
    /// <code>
    /// IList &lt;IMigrationTask&gt; tasks = null;
    /// ClassMigrationTaskSource source = new ClassMigrationTaskSource();
    /// tasks = source.GetMigrationTasks(typeof(TestLibrary.Class1).Assembly.CodeBase);
    /// </code>
    /// </example>
    /// </para>
    /// </summary>
    /// <author>Scott Askew (scott@tacitknowledge.com)</author>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public class ClassMigrationTaskSource : IMigrationTaskSource
    {
        #region Member variables
        private static ILog log;
        #endregion

        #region Costructors
        /// <summary>
        /// Static constructor.
        /// </summary>
        static ClassMigrationTaskSource()
        {
            log = LogManager.GetLogger(typeof(ClassMigrationTaskSource));
        }
        #endregion

        #region Public methods
        /// <seealso cref="IMigrationTaskSource.GetMigrationTasks(String)"/>
        public IList<IMigrationTask> GetMigrationTasks(String assemblyPath)
        {
            if (assemblyPath == null)
            {
                throw new MigrationException("You must specify an assembly file to get tasks for");
            }

            IList<Type> taskTypes = GetClasses(assemblyPath, typeof(IMigrationTask));
            log.Debug("Found " + taskTypes.Count + " patches in " + assemblyPath);
            //Console.WriteLine("Found " + taskTypes.Count + " patches in " + assemblyPath);

            return InstantiateTasks(assemblyPath, taskTypes);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Instantiates given types in the supplied assembly.
        /// </summary>
        /// <param name="assemblyPath">the path to load the assembly</param>
        /// <param name="taskTypes">the types to instantiate</param>
        /// <returns>a list of <code>IMigrationTask</code> objects</returns>
        /// <exception cref="MigrationException">if an unrecoverable error occurs</exception>
        private IList<IMigrationTask> InstantiateTasks(String assemblyPath, IList<Type> taskTypes)
        {
            IList<IMigrationTask> tasks = new List<IMigrationTask>();

            foreach (Type taskType in taskTypes)
            {
                try
                {
                    ObjectHandle oh = Activator.CreateInstanceFrom(assemblyPath, taskType.FullName);
                    IMigrationTask task = oh.Unwrap() as IMigrationTask;

                    if (task == null)
                    {
                        log.Debug("IMigrationTask " + taskType.FullName + " is of wrong type. Skipping");
                        //Console.WriteLine("IMigrationTask " + taskType.FullName + " is of wrong type. Skipping");
                    }
                    else if (task.Name != null)
                    {
                        tasks.Add(task);
                        log.Debug("Added IMigrationTask " + taskType.FullName);
                        //Console.WriteLine("Added IMigrationTask: " + taskType.FullName);
                        //Console.WriteLine("IMigrationTask.Name = " + task.Name);
                        //Console.WriteLine("IMigrationTask.Level = " + task.Level);
                    }
                    else
                    {
                        log.Warn("IMigrationTask " + taskType.FullName + " had no migration name. Is that intentional? Skipping task.");
                        //Console.WriteLine("IMigrationTask " + taskType.FullName + " had no migration name. Is that intentional? Skipping task.");
                    }
                }
                catch (Exception e)
                {
                    throw new MigrationException("Could not instantiate IMigrationTask " + taskType.FullName, e);
                }
            }

            return tasks;
        }

        /// <summary>
        /// Retrieves all types that inherit/implement the specified type in an assembly
        /// (identified by the supplied path).
        /// </summary>
        /// <param name="assemblyPath">the path to load the assembly</param>
        /// <param name="baseType">the base type to work with</param>
        /// <returns>a list of types that inherit/implement the supplied type in the supplied assembly</returns>
        private IList<Type> GetClasses(String assemblyPath, Type baseType)
        {
            IList<Type> types = new List<Type>();
            Assembly assembly = null;

            try
            {
                assembly = Assembly.LoadFrom(assemblyPath);
            }
            catch (Exception e)
            {
                log.Warn("Could not load assembly from " + assemblyPath + ". Skipping", e);
                //Console.WriteLine("Could not load assembly from " + assemblyPath + ". Skipping");
                return types;
            }

            Type[] assemblyTypes = assembly.GetTypes();

            for (int i = 0; i < assemblyTypes.Length; i++)
            {
                if (assemblyTypes[i].IsPublic
                    && assemblyTypes[i].IsClass
                    && !assemblyTypes[i].IsAbstract
                    && assemblyTypes[i].GetInterface(baseType.FullName) != null)
                {
                    types.Add(assemblyTypes[i]);
                }
            }

            return types;
        }
        #endregion
    }
}

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
using System.IO;
using System.Text.RegularExpressions;
using log4net;
using com.tacitknowledge.util.migration;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
	/// <summary>
    /// Searches a package (directory) for SQL scripts that have a specific pattern
	/// and returns corresponding <code>SqlScriptMigrationTasks</code>. The name
	/// of each script must follow the pattern of &quot;patch(\d+)(_.+)?\.sql&quot;.
	/// </summary>
	/// <author>Scott Askew (scott@tacitknowledge.com)</author>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public class SqlScriptMigrationTaskSource : IMigrationTaskSource
    {
        #region Member variables
        private static readonly ILog log = LogManager.GetLogger(typeof(SqlScriptMigrationTaskSource));

        /// <summary>
        /// The regular expression used to match SQL patch files.
        /// </summary>
        private static readonly String SQL_PATCH_REGEX = "patch(\\d+)(_.+)?\\.sql$";
        #endregion

        #region Public methods
        /// <seealso cref="IMigrationTaskSource.GetMigrationTasks(String)"/>
        public IList<IMigrationTask> GetMigrationTasks(String dir)
		{
            if (dir == null)
            {
                throw new MigrationException("Supplied directory cannot be null");
            }

            String path = dir;

            log.Debug("Trying to process directory with path: " + path);
            //Console.WriteLine("Trying to process directory with path: " + path);

            if (!Directory.Exists(path))
            {
                log.Debug("The path " + path + " does not point to a directory on the filesystem. Skipping");
                //Console.WriteLine("The path " + path + " does not point to a directory on the filesystem. Skipping");
                return new List<IMigrationTask>();
            }

            IList<String> scripts = GetResources(path, SQL_PATCH_REGEX);
			
            if (log.IsDebugEnabled)
			{
				log.Debug("Found " + scripts.Count + " SQL patches in path: " + path);
				for (int i = 0; i < scripts.Count; i++)
				{
					log.Debug(" -- \"" + scripts[i] + "\"");
				}
			}

            //Console.WriteLine("Found " + scripts.Count + " SQL patches in path: " + path);
            //for (int i = 0; i < scripts.Count; i++)
            //{
            //    Console.WriteLine(" -- \"" + scripts[i] + "\"");
            //}
			
            return CreateMigrationScripts(scripts);
		}
        #endregion

        #region Private methods
        /// <summary>
        /// Returns the names of the resources in the given directory (and its subdirectories)
        /// that match the given regular expression.
        /// <remarks>
        /// This method will return full paths to files satisfying the supplied regular expression.
        /// </remarks>
        /// </summary>
        /// <param name="path">the directory containing the resources</param>
        /// <param name="regex">the regular expression used to filter the resources</param>
        /// <returns>
        /// the names of the resources in the given directory that match the given
        /// regular expression
        /// </returns>
        private IList<String> GetResources(String path, String regex)
        {
            IList<String> resourceNames = new List<String>();
            String[] allFiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            Regex regExpression = new Regex(regex, RegexOptions.Compiled);

            foreach (String fileName in allFiles)
            {
                Match matcher = regExpression.Match(fileName.ToLower());

                if (matcher.Success)
                {
                    resourceNames.Add(fileName);
                    log.Debug("Adding resource with path: " + fileName);
                    //Console.WriteLine("Adding resource with path: " + fileName);
                }
            }

            return resourceNames;
        }
        
        /// <summary>
        /// Creates a list of <code>SqlScriptMigrationTask</code>s based on the array
		/// of SQL scripts.
		/// </summary>
		/// <param name="scripts">
        /// the classpath-relative array of SQL migration scripts
		/// </param>
		/// <returns>
        /// a list of <code>SqlScriptMigrationTask</code>s based on the array of SQL scripts
		/// </returns>
        /// <exception cref="MigrationException">
        /// if a <code>SqlScriptMigrationTask</code> could no be created
        /// </exception>
        private IList<IMigrationTask> CreateMigrationScripts(IList<String> scripts)
		{
            IList<IMigrationTask> tasks = new List<IMigrationTask>();
            //Pattern p = Pattern.compile(SQL_PATCH_REGEX);
            Regex p = new Regex(SQL_PATCH_REGEX, RegexOptions.Compiled);

            foreach (String script in scripts)
            {
                //script = script.Replace('\\', '/');
                log.Debug("Examining possible SQL patch file \"" + script + "\"");
                //Console.WriteLine("Examining possible SQL patch file \"" + script + "\"");

                FileInfo fileInfo = new FileInfo(script);

                if (!fileInfo.Exists)
                {
                    log.Warn("File \"" + script + "\" does not exist. Skipping");
                    //Console.WriteLine("File \"" + script + "\" does not exist. Skipping");
                    continue;
                }

                String fileName = fileInfo.Name;
                Match matcher = p.Match(fileName.ToLower());

                // Get the version out of the script name
                if (!matcher.Success || matcher.Groups.Count != 3)
                {
                    throw new MigrationException("Invalid SQL script name: " + fileName);
                }

                int level = 0;

                if (!Int32.TryParse(matcher.Groups[1].Value, out level))
                {
                    log.Warn("Could not parse patch level. Skipping");
                    //Console.WriteLine("Could not parse patch level. Skipping");
                    continue;
                }

                using (StreamReader sr = File.OpenText(script))
                {
                    try
                    {
                        // We should send in the script file location so
                        // it doesn't have to buffer the whole thing into RAM
                        SqlScriptMigrationTask task = new SqlScriptMigrationTask(fileName.Replace(".sql", ""), level, sr);

                        tasks.Add(task);
                    }
                    catch (Exception e)
                    {
                        throw new MigrationException("Error reading script " + script, e);
                    }
                    finally
                    {
                        sr.Close();
                    }
                }
            }
			
            return tasks;
		}
        #endregion
	}
}

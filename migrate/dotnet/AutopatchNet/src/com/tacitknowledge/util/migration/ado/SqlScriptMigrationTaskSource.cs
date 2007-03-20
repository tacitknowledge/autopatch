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
//UPGRADE_TODO: The type 'java.util.regex.Matcher' could not be found. If it was not included in the conversion, there may be compiler issues. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1262'"
//using Matcher = java.util.regex.Matcher;
//UPGRADE_TODO: The type 'java.util.regex.Pattern' could not be found. If it was not included in the conversion, there may be compiler issues. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1262'"
//using Pattern = java.util.regex.Pattern;
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
        private static ILog log;

        /// <summary>
        /// The regular expression used to match SQL patch files.
        /// </summary>
        private const String SQL_PATCH_REGEX = "patch(\\d+)(_.+)?\\.sql";
        #endregion

        #region Constructors
        /// <summary>
        /// Static constructor.
        /// </summary>
        static SqlScriptMigrationTaskSource()
        {
            log = LogManager.GetLogger(typeof(SqlScriptMigrationTaskSource));
        }
        #endregion
        
        #region Public methods
        /// <seealso cref="IMigrationTaskSource.GetMigrationTasks(String)"/>
        public IList<IMigrationTask> GetMigrationTasks(String packageName)
		{
            String path = packageName;//packageName.Replace('.', '/');
            IList<String> scripts = getResources(path, SQL_PATCH_REGEX);
			
            if (log.IsDebugEnabled)
			{
				log.Debug("Found " + scripts.Count + " patches in path: " + path);
				for (int i = 0; i < scripts.Count; i++)
				{
					log.Debug(" -- \"" + scripts[i] + "\"");
				}
			}

            //Console.WriteLine("Found " + scripts.Count + " patches in path: " + path);
            //for (int i = 0; i < scripts.Count; i++)
            //{
            //    Console.WriteLine(" -- \"" + scripts[i] + "\"");
            //}
			
            return createMigrationScripts(scripts);
		}
        #endregion

        #region Public methods
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
        private IList<String> getResources(String path, String regex)
        {
            IList<String> resourceNames = new List<String>();
            String[] allFiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            Regex regExpression = new Regex(regex, RegexOptions.Compiled);

            foreach (String fileName in allFiles)
            {
                Match match = regExpression.Match(fileName.ToLower());

                if (match.Success)
                {
                    resourceNames.Add(fileName);
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
        private IList<IMigrationTask> createMigrationScripts(IList<String> scripts)
		{
            IList<IMigrationTask> tasks = new List<IMigrationTask>();
            //Pattern p = Pattern.compile(SQL_PATCH_REGEX);
            
            //for (int i = 0; i < scripts.Length; i++)
            //{
            //    System.String script = scripts[i];
            //    script = script.Replace('\\', '/');
            //    log.Debug("Examining possible SQL patch file \"" + script + "\"");
            //    //UPGRADE_ISSUE: Method 'java.lang.Class.getResourceAsStream' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javalangClassgetResourceAsStream_javalangString'"
            //    System.IO.Stream is_Renamed = GetType().getResourceAsStream("/" + script);
				
            //    if (is_Renamed == null)
            //    {
            //        log.warn("Could not open input stream for file \"/" + script + "\"");
            //    }
            //    else
            //    {
            //        System.IO.FileInfo scriptFile = new System.IO.FileInfo(script);
            //        System.String scriptFileName = scriptFile.Name;
            //        try
            //        {
            //            // Get the version out of the script name
            //            Matcher matcher = p.matcher(scriptFileName);
            //            if (!matcher.matches() || matcher.groupCount() != 2)
            //            {
            //                throw new MigrationException("Invalid SQL script name: " + script);
            //            }
            //            int order = Integer.parseInt(matcher.group(1));
						
            //            // We should send in the script file location so
            //            // it doesn't have to buffer the whole thing into RAM
            //            SqlScriptMigrationTask task = new SqlScriptMigrationTask(scriptFileName, order, is_Renamed);
						
            //            // Free the resource
            //            is_Renamed.Close();
            //            task.Name = scriptFileName;
            //            tasks.Add(task);
            //        }
            //        catch (System.IO.IOException e)
            //        {
            //            throw new MigrationException("Error reading script " + script, e);
            //        }
            //    }
            //}
			
            return tasks;
		}
        #endregion
	}
}

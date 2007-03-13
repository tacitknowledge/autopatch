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
//UPGRADE_TODO: The type 'java.util.regex.Matcher' could not be found. If it was not included in the conversion, there may be compiler issues. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1262'"
//using Matcher = java.util.regex.Matcher;
//UPGRADE_TODO: The type 'java.util.regex.Pattern' could not be found. If it was not included in the conversion, there may be compiler issues. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1262'"
//using Pattern = java.util.regex.Pattern;
using log4net;
using log4net.Config;

using MigrationException = com.tacitknowledge.util.migration.MigrationException;
using MigrationTaskSource = com.tacitknowledge.util.migration.MigrationTaskSource;
#endregion
namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Search a package (directory) for SQL scripts that a specific pattern 
	/// and returns corresponding <code>SqlScriptMigrationTasks</code>.  The name
	/// of each script must follow the pattern of &quot;patch(\d+)(_.+)?\.sql&quot;.
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	public class SqlScriptMigrationTaskSource : MigrationTaskSource
    {
        #region Methods
        /// <summary>Class logger </summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.SqlScriptMigrationTaskSource'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
		
		/// <summary> The regular expression used to match SQL patch files.</summary>
		private const System.String SQL_PATCH_REGEX = "^patch(\\d+)(_.+)?\\.sql";
		
		/// <seealso cref="MigrationTaskSource.getMigrationTasks(String)">
		/// </seealso>
		public virtual System.Collections.IList getMigrationTasks(System.String packageName)
		{
			System.String path = packageName.Replace('.', '/');
			System.String[] scripts = ClassDiscoveryUtil.getResources(path, SQL_PATCH_REGEX);
			if (log.IsDebugEnabled())
			{
				log.Debug("Found " + scripts.Length + " patches in path: " + path);
				for (int i = 0; i < scripts.Length; i++)
				{
					log.Debug(" -- \"" + scripts[i] + "\"");
				}
			}
			return createMigrationScripts(scripts);
		}
		
		/// <summary> Creates a list of <code>SqlScriptMigrationTask</code>s based on the array
		/// of SQL scripts.
		/// 
		/// </summary>
		/// <param name="scripts">the classpath-relative array of SQL migration scripts
		/// </param>
		/// <returns> a list of <code>SqlScriptMigrationTask</code>s based on the array
		/// of SQL scripts
		/// </returns>
		/// <throws>  MigrationException if a SqlScriptMigrationTask could no be created </throws>
		private System.Collections.IList createMigrationScripts(System.String[] scripts)
		{
			Pattern p = Pattern.compile(SQL_PATCH_REGEX);
			System.Collections.IList tasks = new System.Collections.ArrayList();
			for (int i = 0; i < scripts.Length; i++)
			{
				System.String script = scripts[i];
				script = script.Replace('\\', '/');
				log.Debug("Examining possible SQL patch file \"" + script + "\"");
				//UPGRADE_ISSUE: Method 'java.lang.Class.getResourceAsStream' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javalangClassgetResourceAsStream_javalangString'"
				System.IO.Stream is_Renamed = GetType().getResourceAsStream("/" + script);
				
				if (is_Renamed == null)
				{
					log.warn("Could not open input stream for file \"/" + script + "\"");
				}
				else
				{
					System.IO.FileInfo scriptFile = new System.IO.FileInfo(script);
					System.String scriptFileName = scriptFile.Name;
					try
					{
						// Get the version out of the script name
						Matcher matcher = p.matcher(scriptFileName);
						if (!matcher.matches() || matcher.groupCount() != 2)
						{
							throw new MigrationException("Invalid SQL script name: " + script);
						}
						int order = Integer.parseInt(matcher.group(1));
						
						// We should send in the script file location so
						// it doesn't have to buffer the whole thing into RAM
						SqlScriptMigrationTask task = new SqlScriptMigrationTask(scriptFileName, order, is_Renamed);
						
						// Free the resource
						is_Renamed.Close();
						task.setName(scriptFileName);
						tasks.Add(task);
					}
					catch (System.IO.IOException e)
					{
						throw new MigrationException("Error reading script " + script, e);
					}
				}
			}
			return tasks;
		}
		static SqlScriptMigrationTaskSource()
		{
			log = LogManager.GetLogger(typeof(SqlScriptMigrationTaskSource));
		}
	}
#endregion
}
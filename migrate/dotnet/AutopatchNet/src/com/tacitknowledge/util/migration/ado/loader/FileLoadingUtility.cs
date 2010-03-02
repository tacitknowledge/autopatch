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
using log4net;
using log4net.Config;
#endregion

namespace com.tacitknowledge.util.migration.ado.loader
{
	
	/// <summary> This is a very simple utility that looks for a file 
	/// based upon its existence in the classpath or the 
	/// absolute path if provided.
	/// 
	/// </summary>
	/// <author>  Chris A. (chris@tacitknowledge.com)
	/// </author>
	public class FileLoadingUtility
    {
        #region Member Variables
        /// <summary> Class logger</summary>
        //UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.loader.FileLoadingUtility'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
        private static ILog log;

        /// <summary> The name of the file to load</summary>
        private System.String fileName = null;
        #endregion

        #region Methods
        /// <summary> Gets an input stream by first checking the current classloader, 
		/// then trying to use the system classloader, and finally, trying 
		/// to access the file on the file system.  If the file is not found, 
		/// an <code>IllegalArgumentException</code> will be thrown.
		/// 
		/// </summary>
		/// <returns> the file as an input stream
		/// </returns>
		virtual public System.IO.Stream ResourceAsStream
		{
			get
			{
                System.IO.Stream stream = null;

                System.IO.FileInfo f = new System.IO.FileInfo(fileName);
                try
                {
                    stream = new System.IO.FileStream(f.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                }
                catch (System.IO.FileNotFoundException e)
                {
                    log.Error("The file: " + fileName + " was not found.", e);
                    throw new System.ArgumentException("Must have a valid file name.", e);
                }
                return stream;
			}
			
		}
		
		
		/// <summary> Creates a new <code>FileLoadingUtility</code>.
		/// 
		/// </summary>
		/// <param name="fileName">the name of the file to load
		/// </param>
		public FileLoadingUtility(System.String fileName)
		{
			this.fileName = fileName;
		}
		static FileLoadingUtility()
		{
			
            log = LogManager.GetLogger(typeof(FileLoadingUtility));
        }
        #endregion
    }
}
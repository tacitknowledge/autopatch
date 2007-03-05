/* Copyright (c) 2004 Tacit Knowledge LLC  
* See licensing terms below.
* 
* THIS SOFTWARE IS PROVIDED "AS IS" AND ANY  EXPRESSED OR IMPLIED 
* WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
* MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
* IN NO EVENT SHALL TACIT KNOWLEDGE LLC OR ITS CONTRIBUTORS BE LIABLE
* FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
* CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
* SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
* BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
* WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
* OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
* EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  THIS HEADER MUST
* BE INCLUDED IN ANY DISTRIBUTIONS OF THIS CODE.
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
				//UPGRADE_ISSUE: Method 'java.lang.Class.getResourceAsStream' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javalangClassgetResourceAsStream_javalangString'"
				System.IO.Stream stream = GetType().getResourceAsStream(fileName);
				if (stream == null)
				{
					//UPGRADE_ISSUE: Method 'java.lang.ClassLoader.getSystemResourceAsStream' was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1000_javalangClassLoader'"
					stream = ClassLoader.getSystemResourceAsStream(fileName);
				}
				if (stream == null)
				{
					System.IO.FileInfo f = new System.IO.FileInfo(fileName);
					try
					{
						//UPGRADE_TODO: Constructor 'java.io.FileInputStream.FileInputStream' was converted to 'System.IO.FileStream.FileStream' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioFileInputStreamFileInputStream_javaioFile'"
						stream = new System.IO.FileStream(f.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
					}
					catch (System.IO.FileNotFoundException e)
					{
						log.error("The file: " + fileName + " was not found.", e);
						throw new System.ArgumentException("Must have a valid file name.");
					}
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
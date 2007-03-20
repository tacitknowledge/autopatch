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
using HSSFWorkbook = org.apache.poi.hssf.usermodel.HSSFWorkbook;
using POIFSFileSystem = org.apache.poi.poifs.filesystem.POIFSFileSystem;
using MigrationContext = com.tacitknowledge.util.migration.IMigrationContext;
using MigrationException = com.tacitknowledge.util.migration.MigrationException;
using MigrationTaskSupport = com.tacitknowledge.util.migration.MigrationTaskSupport;
using DataSourceMigrationContext = com.tacitknowledge.util.migration.ado.DataSourceMigrationContext;
#endregion

namespace com.tacitknowledge.util.migration.ado.loader
{
	
	/// <summary> This is a utility class for reading excel files and 
	/// performing a database insert based upon a cell value 
	/// provided. 
	/// 
	/// </summary>
	/// <author>  Chris A. (chris@tacitknowledge.com)
	/// </author>
	/// <version>  $Id$
	/// </version>
	public abstract class ExcelFileLoader:MigrationTaskSupport
    {
        #region Member Variables
        /// <summary> Class logger</summary>
		//UPGRADE_NOTE: The initialization of  'log' was moved to static method 'com.tacitknowledge.util.migration.ado.loader.ExcelFileLoader'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private static ILog log;
        #endregion

        #region Methods
        /// <summary> Obtains a database connection, reads a file assumed to be in Excel 
		/// format based on the name provided <code>getName()</code>.  Calls the 
		/// abstract method <code>processWorkbook()</code>
		/// 
		/// </summary>
		/// <param name="ctx">the <code>ADOMigrationContext</code>
		/// </param>
		/// <throws>  MigrationException if an unexpected error occurs </throws>
		public override void  migrate(MigrationContext ctx)
		{
			DataSourceMigrationContext context = (DataSourceMigrationContext) ctx;
			FileLoadingUtility utility = new FileLoadingUtility(getName());
			try
			{
				//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
				System.Data.OleDb.OleDbConnection conn = context.Connection;
				POIFSFileSystem fs = new POIFSFileSystem(utility.ResourceAsStream);
				HSSFWorkbook wb = new HSSFWorkbook(fs);
				processWorkbook(wb, conn);
			}
			catch (System.IO.IOException e)
			{
				log.error("An IO Exception occurred while trying to parse the Excel file.", e);
				throw new MigrationException("Error reading file.", e);
			}
			catch (System.Data.OleDb.OleDbException e)
			{
				log.error("Caught a SQLException when trying to obtain a database connection");
				throw new MigrationException("Error obtaining database connection", e);
			}
		}
		
		/// <summary> Process workbook by overwriting this method
		/// 
		/// </summary>
		/// <param name="wb">the excel workbook to process
		/// </param>
		/// <param name="conn">the database connection to use for data loading
		/// </param>
		/// <throws>  MigrationException if something goes wrong </throws>
		//UPGRADE_NOTE: There are other database providers or managers under System.Data namespace which can be used optionally to better fit the application requirements. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1208'"
		public abstract void  processWorkbook(HSSFWorkbook wb, System.Data.OleDb.OleDbConnection conn);
		static ExcelFileLoader()
		{
			log = LogManager.GetLogger(typeof(ExcelFileLoader));
        }
        #endregion
    }
        
}
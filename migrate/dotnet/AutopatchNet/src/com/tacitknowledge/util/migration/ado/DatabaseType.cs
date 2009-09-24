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
using System.Reflection;
using System.Resources;
using System.Collections;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary> Defines a type of database (e.g. <code>oracle</code> or <code>postgres</code>.  This
	/// is used to help define the SQL that is used to update the patch table and as a hint
	/// used while parsing SQL patch files.
	/// 
	/// Each type of database that this AutoPatch supports (currently PostgreSQL and Oracle) has its
	/// own <i>database-type</i>.properties file containing the database-specific SQL and DDL.  The
	/// required keys are:
	/// <ul>
    /// <li>createPatches - DDL that creates the patches table</li>
    /// <li>createLevel - SQL that inserts a new patch level record for the system</li>
    /// <li>readLevel - SQL tahat selects the current patch level of the system</li>
    /// <li>updateLevel - SQL that updates the current patch level of the system</li>
    /// <li>readLock - Returns 'T' if the system patch lock is in use, 'F' otherwise</li>
    /// <li>obtainLock - SQL that selects the patch lock for the system</li>
    /// <li>releaseLock - SQL that releases the patch lock for the system</li>
	/// </ul>
	/// 
	/// Use <i>postgres.properties</i> or <i>oracle.properties</i> as a baseline for adding
	/// additional database types.
	/// 
	/// </summary>
	/// <author>  Scott Askew (scott@tacitknowledge.com)
	/// </author>
	public class DatabaseType
    {

        #region Members
        /// <summary> The SQL statements and properties that are unique to this database flavor.</summary>
        private System.Collections.Specialized.NameValueCollection properties = new System.Collections.Specialized.NameValueCollection();

        /// <summary> The database type</summary>
        private System.String databaseType = "";

        #endregion

        #region Methods

        /// <summary> Determines if the database supports multiple SQL and DDL statements in a single
		/// <code>Statement.execute</code> call. 
		/// 
		/// </summary>
		/// <returns> if the database supports multiple SQL and DDL statements in a single
		/// <code>Statement.execute</code> call.
		/// </returns>
		public bool MultipleStatementsSupported
		{
			get
			{
				System.String multiStatement = properties["supportsMultipleStatements"] == null?"false":properties["supportsMultipleStatements"];
				//UPGRADE_NOTE: Exceptions thrown by the equivalent in .NET of method 'java.lang.Boolean.valueOf' may be different. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1099'"
				return System.Boolean.Parse(multiStatement);
			}
			
		}
		
		
		/// <summary> Creates a new <code>DatabaseType</code>.
		/// 
		/// </summary>
		/// <param name="databaseType">the type of database
		/// </param>
		public DatabaseType(System.String databaseType)
		{
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = string.Format("AutopatchNet.{0}.resources", databaseType.ToLower());
            System.IO.Stream rs = assembly.GetManifestResourceStream(resourceName);
            if (rs == null)
            {
                throw new System.ArgumentException("Could not find SQL resources file for database '" + databaseType + "'; make sure that there is a '" + databaseType.ToLower() + ".resources' file in package.");
            }
            try
            {
                ResourceReader reader = new ResourceReader(rs);
                IDictionaryEnumerator en = reader.GetEnumerator();
                while(en.MoveNext())
                {
                    string sqlkey = (string)en.Key;
                    string sql = (string)en.Value;
                    properties.Add(sqlkey, sql);
                }

                reader.Close();
            }
            catch (System.IO.IOException e)
            {
                throw new System.ArgumentException("Could not read SQL resources file for database '" + databaseType + "'.", e);
            }
            finally
            {
                rs.Close();
            }

            this.databaseType = databaseType;
		}
		
		/// <summary> Return the name of the database type
		/// 
		/// </summary>
		/// <returns> String containing the name of the database type
		/// </returns>
		public System.String getDatabaseType()
		{
			return databaseType;
		}
		
		/// <summary> Returns the named property.
		/// 
		/// </summary>
		/// <param name="propertyName">the property to retrieve
		/// </param>
		/// <returns> the requested property, or <code>null</code> if it doesn't exist
		/// </returns>
		public System.String getProperty(System.String propertyName)
		{
			return properties.Get(propertyName);
		}

        /// <seealso cref="Object.ToString()"/>
        public override String ToString()
        {
            return "DatabaseType " + getDatabaseType();
        }
        #endregion
    }
}

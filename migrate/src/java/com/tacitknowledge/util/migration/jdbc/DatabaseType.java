/* Copyright 2005 Tacit Knowledge LLC
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

package com.tacitknowledge.util.migration.jdbc;

import java.io.IOException;
import java.io.InputStream;
import java.util.Enumeration;
import java.util.Properties;

/**
 * Defines a type of database (e.g. <code>oracle</code> or <code>postgres</code>.  This
 * is used to help define the SQL that is used to update the patch table and as a hint
 * used while parsing SQL patch files.
 * 
 * Each type of database that this AutoPatch supports (currently PostgreSQL and Oracle) has its
 * own <i>database-type</i>.properties file containing the database-specific SQL and DDL.  The
 * required keys are:
 * <ul>
 *    <li>patches.create - DDL that creates the patches table</li>
 *    <li>level.create - SQL that inserts a new patch level record for the system</li>
 *    <li>level.read - SQL tahat selects the current patch level of the system</li>
 *    <li>level.update - SQL that updates the current patch level of the system</li>
 *    <li>lock.read - Returns 'T' if the system patch lock is in use, 'F' otherwise</li>
 *    <li>lock.obtain - SQL that selects the patch lock for the system</li>
 *    <li>lock.release - SQL that releases the patch lock for the system</li>
 * </ul>
 * 
 * Use <i>postgres.properties</i> or <i>oracle.properties</i> as a baseline for adding
 * additional database types.
 *
 * @author Scott Askew (scott@tacitknowledge.com)
 */
public class DatabaseType
{
    /**
     * The SQL statements and properties that are unique to this database flavor.
     */
    private Properties properties = new Properties();
    
    /**
     * The database type
     */
    private String databaseType = "";
    
    /**
     * Creates a new <code>DatabaseType</code>.
     * 
     * @param databaseType the type of database
     */
    public DatabaseType(String databaseType)
    {
        InputStream is = getClass().getResourceAsStream(databaseType + ".properties");
        if (is == null)
        {
            throw new IllegalArgumentException("Could not find SQL properties "
                + " file for database '" + databaseType + "'; make sure that there "
                + " is a '" + databaseType + ".properties' file in package '"
                + getClass().getPackage().getName() + "'.");
        }
        try
        {
            properties.load(is);
        }
        catch (IOException e)
        {
            throw new IllegalArgumentException("Could not read SQL properties "
                + " file for database '" + databaseType + "'.");
        }
        finally
        {
            try
            {
                is.close();
            }
            catch (IOException e1)
            {
                // not important
            }
        }
        
        this.databaseType = databaseType;
    }
    
    /**
     * Return the name of the database type
     * 
     * @return String containing the name of the database type
     */
    public String getDatabaseType()
    {
        return databaseType;
    }
    
    /**
     * Returns the named property.
     * 
     * @param  propertyName the property to retrieve
     * @return the requested property, or <code>null</code> if it doesn't exist
     */
    public String getProperty(String propertyName)
    {
        return properties.getProperty(propertyName);
    }
    
    /**
     * Determines if the database supports multiple SQL and DDL statements in a single
     * <code>Statement.execute</code> call. 
     * 
     * @return if the database supports multiple SQL and DDL statements in a single
     *         <code>Statement.execute</code> call.
     */
    public boolean isMultipleStatementsSupported()
    {
        String multiStatement = properties.getProperty("supportsMultipleStatements", "false");
        return Boolean.valueOf(multiStatement).booleanValue();
    }
}

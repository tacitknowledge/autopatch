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

import javax.sql.DataSource;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationException;

/**
 * Automatically applies database DDL and SQL patches upon server startup.
 *
 * @author Scott Askew (scott@tacitknowledge.com)
 */
public class AutoPatchService
{
    /** Class logger */
    private static Log log = LogFactory.getLog(AutoPatchService.class);
    
    /** The name of the schema to patch */
    private String systemName = null;
    
    /** The data source used to patch the schema */
    private DataSource dataSource = null;
    
    /** The type of database */
    private String databaseType = null;
    
    /** The path to the SQL patches */
    private String patchPath = null;

    /**
     * Patches the database, if necessary.
     * 
     * @throws MigrationException if an unexpected error occurs
     */
    public void patch() throws MigrationException
    {
        DataSourceMigrationContext context = new DataSourceMigrationContext();
        context.setSystemName(getSystemName());
        context.setDatabaseType(new DatabaseType(getDatabaseType()));
        context.setDataSource(getDataSource());
        
        try
        {
            log.info("Applying patches....");
            JdbcMigrationLauncher launcher = new JdbcMigrationLauncher(context);
            launcher.setPatchPath(getPatchPath());
            int patchesApplied = launcher.doMigrations();
            log.info("Applied " + patchesApplied + " "
                + (patchesApplied == 1 ? "patch" : "patches") + ".");
        }
        catch (MigrationException e)
        {
            throw new MigrationException("Error applying patches", e);
        }
    }

    /**
     * @return Returns the dataSource.
     */
    public DataSource getDataSource()
    {
        return dataSource;
    }
    
    /**
     * @param dataSource The dataSource to set.
     */
    public void setDataSource(DataSource dataSource)
    {
        this.dataSource = dataSource;
    }
    
    /**
     * @return Returns the systemName.
     */
    public String getSystemName()
    {
        return systemName;
    }
    
    /**
     * @param systemName The systemName to set.
     */
    public void setSystemName(String systemName)
    {
        this.systemName = systemName;
    }
    
    /**
     * @return Returns the databaseType.
     */
    public String getDatabaseType()
    {
        return databaseType;
    }
    
    /**
     * @param dialect The databaseType to set.
     */
    public void setDatabaseType(String dialect)
    {
        this.databaseType = dialect;
    }
    
    /**
     * @return Returns the patchPath.
     */
    public String getPatchPath()
    {
        return patchPath;
    }
    
    /**
     * @param patchPath The patchPath to set.
     */
    public void setPatchPath(String patchPath)
    {
        this.patchPath = patchPath;
    }
}

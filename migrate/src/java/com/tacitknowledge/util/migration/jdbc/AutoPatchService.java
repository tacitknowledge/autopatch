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

package com.tacitknowledge.util.migration.jdbc;

import javax.sql.DataSource;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationException;

/**
 * Creates an AutoPatch environment using a configuration supplied by dependency
 * injection. Exports a hook that can be called to execute AutoPatch after configuration
 *
 * @author Scott Askew (scott@tacitknowledge.com)
 */
public class AutoPatchService extends JdbcMigrationLauncherFactory
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
    
    /** The patch to the post-patch tasks */
    private String postPatchPath = null;
    
    /** Whether we really want to apply the patches, or just look */
    private boolean readOnly = false;

    /**
     * Patches the database, if necessary.
     * 
     * @throws MigrationException if an unexpected error occurs
     */
    public void patch() throws MigrationException
    {
        JdbcMigrationLauncher launcher = getLauncher();
        
        try
        {
            log.info("Applying patches....");
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
     * Configure and return a JdbcMigrationLauncher to use for patching
     * 
     * @return JdbcMigrationLauncher configured from injected properties
     * @exception MigrationException if there is a problem setting the context
     */
    public JdbcMigrationLauncher getLauncher() throws MigrationException
    {
        JdbcMigrationLauncher launcher = getJdbcMigrationLauncher();
        launcher.setContext(getContext());
        launcher.setPatchPath(getPatchPath());
        launcher.setPostPatchPath(getPostPatchPath());
        launcher.setReadOnly(isReadOnly());
        return launcher;
    }

    /**
     * Configure and return a DataSourceMigrationContext from this object's
     * injected properties
     * 
     * @return DataSourceMigrationContext configured from injected properties
     */
    protected DataSourceMigrationContext getContext()
    {
        DataSourceMigrationContext context = getDataSourceMigrationContext();
        context.setSystemName(getSystemName());
        context.setDatabaseType(new DatabaseType(getDatabaseType()));
        context.setDataSource(getDataSource());
        return context;
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
    
    /**
     * @return Returns the postPatchPath.
     */
    public String getPostPatchPath()
    {
        return postPatchPath;
    }
    
    /**
     * @param postPatchPath The postPatchPath to set.
     */
    public void setPostPatchPath(String postPatchPath)
    {
        this.postPatchPath = postPatchPath;
    }

    /**
     * See if we are actually applying patches, or if it is just readonly
     * 
     * @return boolean true if we will skip application
     */
    public boolean isReadOnly()
    {
        return readOnly;
    }

    /**
     * Set whether or not to actually apply patches
     * 
     * @param readOnly boolean true if we should skip application
     */
    public void setReadOnly(boolean readOnly)
    {
        this.readOnly = readOnly;
    }
}

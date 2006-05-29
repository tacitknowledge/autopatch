/* 
 * Copyright 2006 Tacit Knowledge LLC
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

import java.sql.Connection;
import java.sql.SQLException;
import java.util.HashMap;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.MigrationProcess;
import com.tacitknowledge.util.migration.jdbc.util.SqlUtil;

/**
 * Core starting point for a distributed database migration run.  This class obtains a connection
 * to the orchestration database, checks its patch level, delegates the actual execution of the migration
 * tasks to a <code>MigrationProcess</code> instance, and then commits and cleans everything
 * up at the end.
 * <p>
 * This class is <b>NOT</b> threadsafe.
 *
 * @author  Scott Askew (scott@tacitknowledge.com)
 * @version $Id$
 */
public class DistributedJdbcMigrationLauncher extends JdbcMigrationLauncher
{
    /** Class logger */
    private static Log log = LogFactory.getLog(DistributedJdbcMigrationLauncher.class);

    /** The JdbcMigrationLaunchers we are controlling */
    private HashMap controlledSystems = new HashMap();

    /**
     * Create a new MigrationProcess and add a SqlScriptMigrationTaskSource
     */
    public DistributedJdbcMigrationLauncher()
    {
        setMigrationProcess(new MigrationProcess());
        getMigrationProcess().addMigrationTaskSource(new SqlScriptMigrationTaskSource());
    }
    
    /**
     * Create a new <code>MigrationLancher</code>.
     *
     * @param context the <code>JdbcMigrationContext</code> to use.
     * @throws MigrationException if an unexpected error occurs
     */
    public DistributedJdbcMigrationLauncher(JdbcMigrationContext context) throws MigrationException
    {
        super(context);
        setJdbcMigrationContext(context);
    }

    /**
     * Starts the application migration process across all configured contexts
     *
     * @return the number of patches applied
     * @throws MigrationException if an unrecoverable error occurs during
     *         the migration
     */
    public int doMigrations() throws MigrationException
    {
        if (getContext() == null)
        {
            throw new MigrationException("You must configure a migration context");
        }
        
        Connection conn = null;
        try
        {
            conn = getContext().getConnection();
            return -1; // FIXME
        }
        catch (SQLException e)
        {
            throw new MigrationException("SqlException during migration", e);
        }
        finally
        {
            SqlUtil.close(conn, null, null);
        }
    }

    /**
     * Get the list of systems we are controlling
     * 
     * @return HashMap of JdbcMigrationLauncher objects keyed by String system names
     */
    public HashMap getControlledSystems()
    {
        return controlledSystems;
    }

    /**
     * Set the list of systems to control
     * 
     * @param controlledSystems HashMap of JdbcMigrationLauncher objects keyed by String system names
     */
    public void setControlledSystems(HashMap controlledSystems)
    {
        this.controlledSystems = controlledSystems;
    }
}

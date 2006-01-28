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

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.PatchStore;
import com.tacitknowledge.util.migration.jdbc.util.SqlUtil;


/**
 * Manages interactions with the "patches" table.  The patches table stores
 * the current patch level for a given system, as well as a system-scoped lock
 * use to avoid concurrent patches to the system.  A system is defined as an
 * exclusive target of a patch.
 * <p>
 * This class is responsible for:
 * <ul>
 *    <li>Validating the existence of the patches table and creating it if it
 *        doesn't exist</li>
 *    <li>Determining if a patch is currently running on a given system</li>
 *    <li>Obtaining and releasing patch locks for a given system</li>
 *    <li>Obtaining and incrementing the patch level for a given system</li>
 * </ul>
 * <p>
 * <strong>TRANSACTIONS:</strong> Transactions should be committed by the calling
 * class as needed.  This class does not explictly commit or rollback transactions.
 * 
 * @author  Scott Askew (scott@tacitknowledge.com)
 */
public class PatchTable implements PatchStore
{
    /**
     * Class logger
     */
    private static Log log = LogFactory.getLog(PatchTable.class);
    
    /**
     * The migration configuration 
     */
    private JdbcMigrationContext context = null;
    
    /**
     * The database connection 
     */
    private Connection conn = null;
    
    /**
     * Keeps track of table validation (see #createPatchesTableIfNeeded)
     */
    private boolean tableExistenceValidated = false;
    
    /**
     * Create a new <code>PatchTable</code>.
     * 
     * @param migrationContext the migration configuration and connection source
     * @param connection the database connection to use; this will NOT be closed
     */
    public PatchTable(JdbcMigrationContext migrationContext, Connection connection)
    {
        this.context = migrationContext;
        this.conn = connection;
        
        if (context.getDatabaseType() == null)
        {
            throw new IllegalArgumentException("The JDBC database type is required");
        }
    }

    /**
     * {@inheritDoc}
     */
    public void createPatchStoreIfNeeded() throws MigrationException
    {
        if (tableExistenceValidated)
        {
            return;
        }
        
        PreparedStatement stmt = null;
        ResultSet rs = null;
        try
        {
            // TODO: Find a better, cross-platform way to determine if a table exists.
            //       Simply expecting a SQLException is kind of a hack
            stmt = conn.prepareStatement(getSql("level.read"));
            stmt.setString(1, context.getSystemName());
            rs = stmt.executeQuery();
            log.debug("'patches' table already exists.");
            tableExistenceValidated = true;
        }
        catch (SQLException e)
        {
            SqlUtil.close(null, stmt, rs);
            log.info("'patches' table must not exist; creating....");
            try
            {
                stmt = conn.prepareStatement(getSql("patches.create"));
                if (log.isDebugEnabled())
                {
                    log.debug("Creating patches table with SQL '" + getSql("patches.create") + "'");
                }
                stmt.execute();
            }
            catch (SQLException sqle)
            {
                throw new MigrationException("Unable to create patch table", sqle);
            }
            tableExistenceValidated = true;
            log.info("Created 'patches' table.");
        }
        finally
        {
            SqlUtil.close(null, stmt, rs);
        }
    }
    
    /**
     * {@inheritDoc}
     */
    public int getPatchLevel() throws MigrationException
    {
        createPatchStoreIfNeeded();

        PreparedStatement stmt = null;
        ResultSet rs = null;
        try
        {
            stmt = conn.prepareStatement(getSql("level.read"));
            stmt.setString(1, context.getSystemName());
            rs = stmt.executeQuery();
            if (rs.next())
            {
                return rs.getInt(1);
            }
            else
            {
                // We don't yet have a patch record for this system; create one
                createSystemPatchRecord();
                return 0;
            }
        }
        catch (SQLException e)
        {
            throw new MigrationException("Unable to get patch level", e);
        }
        finally
        {
            SqlUtil.close(null, stmt, rs);
        }
    }
    
    /**
     * {@inheritDoc}
     */
    public void updatePatchLevel(int level) throws MigrationException
    {
        // Make sure a patch record already exists for this system
        getPatchLevel();
        
        PreparedStatement stmt = null;
        try
        {
            stmt = conn.prepareStatement(getSql("level.update"));
            stmt.setInt(1, level);
            stmt.setString(2, context.getSystemName());
            stmt.execute();
        }
        catch (SQLException e)
        {
            throw new MigrationException("Unable to update patch level", e);
        }
        finally
        {
            SqlUtil.close(null, stmt, null);
        }
    }
    
    /**
     * {@inheritDoc}
     */
    public boolean isPatchStoreLocked() throws MigrationException
    {
        createPatchStoreIfNeeded();
        
        PreparedStatement stmt = null;
        ResultSet rs = null;
        try
        {
            stmt = conn.prepareStatement(getSql("lock.read"));
            stmt.setString(1, context.getSystemName());
            rs = stmt.executeQuery();
            
            if (rs.next())
            {
                return ("T".equals(rs.getString(1)));
            }
            else
            {
                return false;
            }
        }
        catch (SQLException e)
        {
            throw new MigrationException("Unable to determine if table is locked", e);
        }
        finally
        {
            SqlUtil.close(null, stmt, rs);
        }
    }
    
    /**
     * {@inheritDoc}
     */
    public void lockPatchStore() throws MigrationException, IllegalStateException
    {
        if (isPatchStoreLocked())
        {
            throw new IllegalStateException("Patch table is already locked!");
        }
        updatePatchLock(true);
    }

    /**
     * {@inheritDoc}
     */    
    public void unlockPatchStore() throws MigrationException
    {
        updatePatchLock(false);
    }

    /**
     * Returns the SQL to execute for the database type associated with this patch table.
     * 
     * @param  key the key within <code><i>database</i>.properties</code> whose
     *         SQL should be returned
     * @return the SQL to execute for the database type associated with this patch table
     */
    protected String getSql(String key)
    {
        return context.getDatabaseType().getProperty(key);
    }
    
    /**
     * Creates an initial record in the patches table for this system. 
     * 
     * @throws SQLException if an unrecoverable database error occurs
     */
    private void createSystemPatchRecord() throws SQLException
    {
        String systemName = context.getSystemName();
        PreparedStatement stmt = null;
        try
        {
            stmt = conn.prepareStatement(getSql("level.create"));
            stmt.setString(1, systemName);
            stmt.execute();
            log.info("Created patch record for " + systemName);
        }
        catch (SQLException e)
        {
            log.error("Error creating patch record for system '" + systemName + "'", e);
            throw e;
        }
        finally
        {
            SqlUtil.close(null, stmt, null);
        }
    }

    /**
     * Obtains or releases a lock for this system in the patches table. 
     * 
     * @param  lock <code>true</code> if a lock is to be obtained, <code>false</code>
     *         if it is to be removed 
     * @throws MigrationException if an unrecoverable database error occurs
     */        
    private void updatePatchLock(boolean lock) throws MigrationException
    {
        String sqlkey = (lock) ? "lock.obtain" : "lock.release";
        PreparedStatement stmt = null;
        
        try
        {
            stmt = conn.prepareStatement(getSql(sqlkey));
            if (log.isDebugEnabled())
            {
                log.debug("Updating patch table lock: " + getSql(sqlkey));
            }
            stmt.setString(1, context.getSystemName());
            stmt.execute();
        }
        catch (SQLException e)
        {
            throw new MigrationException("Unable to update patch lock to " + lock, e);
        }
        finally
        {
            SqlUtil.close(null, stmt, null);
        }
    }
}

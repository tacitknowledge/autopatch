/* Copyright 2004 Tacit Knowledge LLC
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
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.Properties;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;


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
 * One design choice made for this class is to eliminate dependencies on any
 * third-party database access API.  This class handles all database interaction
 * itself.  Support for multiple database dialects is enabled by the
 * externalization of the SQL used to interact with the patches table.  Each
 * type of database that this class supports (currently only PostgreSQL) has its
 * own <i>dialect</i>.properties file containing the relevant SQL.  The required
 * keys are:
 * <ul>
 *    <li>patches.create - Creates the patches table</li>
 *    <li>level.create - Inserts a new patch level record for the system</li>
 *    <li>level.read - Returns the current patch level of the system</li>
 *    <li>level.update - Updates the current patch level of the system</li>
 *    <li>lock.read - Returns 'T' if the system patch lock is in use, 'F' otherwise</li>
 *    <li>lock.obtain - Obtains the patch lock for the system</li>
 *    <li>lock.release - Releases the patch lock for the system</li>
 * </ul>
 * 
 * Use <i>postgres.properties</li> as a baseline for adding additional database
 * dialects.
 * <p>
 * <strong>TRANSACTIONS:</strong> Transactions should be committed by the calling
 * class as needed.  This class does not explictly commit or rollback transactions.
 * 
 * @author  Scott Askew (scott@tacitknowledge.com)
 * @version $Id$
 */
public class PatchTable
{
    /**
     * Class logger
     */
    private static Log log = LogFactory.getLog(PatchTable.class);
    
    /**
     * Max length for the systemName columne
     */
    private static final int MAX_SYSTEMNAME_LENGTH = 30;
    
    /**
     * The name of the logical system being patched
     */
    private String systemName = null;
    
    /**
     * Keeps track of table validation (see #createPatchesTableIfNeeded)
     */
    private boolean tableExistenceValidated = false;
    
    /**
     * DB-dialect specific SQL statements loaded from a resource bundle
     */
    private Properties sqlStatements = null;
    
    /**
     * Create a new <code>PatchTable</code>.
     * 
     * @param name the name of the logical system being patched
     * @param dialect the database type (e.g. <code>postgres</code>) containing
     *        the patches table
     */
    public PatchTable(String name, String dialect)
    {
        setSystemName(name);
        
        if (dialect == null)
        {
            throw new IllegalArgumentException("dialect cannot be null");
        }
        
        // Load up the SQL for the right kind of DB
        String filename = dialect + ".properties";
        sqlStatements = getResources(filename);
        if (sqlStatements == null)
        {
            throw new IllegalArgumentException("Could not find SQL properties "
                + " file for dialect '" + dialect + "'; make sure that there "
                + " is a '" + filename + "' file in package '"
                + getClass().getPackage().getName() + "'.");
        }
    }

    /**
     * Validates the existence of the "patches" table and creates it if it doesn't
     * exist.
     * 
     * @param  conn the database connection to use
     * @throws SQLException if an unrecoverable database error occurs 
     */
    public void createPatchesTableIfNeeded(Connection conn) throws SQLException
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
            stmt.setString(1, getSystemName());
            rs = stmt.executeQuery();
            log.debug("'patches' table already exists.");
            tableExistenceValidated = true;
        }
        catch (SQLException e)
        {
            SqlUtil.close(null, stmt, rs);
            log.info("'patches' table does not exist; creating....");
            stmt = conn.prepareStatement(getSql("patches.create"));
            if (log.isDebugEnabled())
            {
                log.debug("Creating patches table with SQL '" + getSql("patches.create") + "'");
            }
            stmt.execute();
            tableExistenceValidated = true;
            log.info("Created 'patches' table.");
        }
        finally
        {
            SqlUtil.close(null, stmt, rs);
        }
    }
    
    /**
     * Returns the current patch level of the system.
     * 
     * @param  conn the database connection to use
     * @return the current patch level of the system
     * @throws SQLException if an unrecoverable database error occurs
     */
    public int getPatchLevel(Connection conn) throws SQLException
    {
        createPatchesTableIfNeeded(conn);

        PreparedStatement stmt = null;
        ResultSet rs = null;
        try
        {
            stmt = conn.prepareStatement(getSql("level.read"));
            stmt.setString(1, getSystemName());
            rs = stmt.executeQuery();
            if (rs.next())
            {
                return rs.getInt(1);
            }
            else
            {
                // We don't yet have a patch record for this system; create one
                createSystemPatchRecord(conn);
                return 0;
            }
        }
        finally
        {
            SqlUtil.close(null, stmt, rs);
        }
    }
    
    /**
     * Updates the system patch level to the specified value.
     * 
     * @param  conn the database connection to use
     * @param  level the new system patch level
     * @throws SQLException if an unrecoverable database error occurs
     */
    public void updatePatchLevel(Connection conn, int level) throws SQLException
    {
        // Make sure a patch record already exists for this system
        getPatchLevel(conn);
        
        PreparedStatement stmt = null;
        try
        {
            stmt = conn.prepareStatement(getSql("level.update"));
            stmt.setInt(1, level);
            stmt.setString(2, getSystemName());
            stmt.execute();
        }
        finally
        {
            SqlUtil.close(null, stmt, null);
        }
    }
    
    /**
     * Determines if a process has a lock on the patches table.
     *  
     * @param  conn the database connection to use
     * @return <code>true</code> if the patches table is locked
     * @throws SQLException if an unrecoverable database error occurs
     */
    public boolean isPatchTableLocked(Connection conn) throws SQLException
    {
        createPatchesTableIfNeeded(conn);
        
        PreparedStatement stmt = null;
        ResultSet rs = null;
        try
        {
            stmt = conn.prepareStatement(getSql("lock.read"));
            stmt.setString(1, getSystemName());
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
        finally
        {
            SqlUtil.close(null, stmt, rs);
        }
    }
    
    /**
     * Places a lock for this system on the patches table.
     * 
     * @param  conn the database connection to use 
     * @throws SQLException if an unrecoverable database error occurs
     * @throws IllegalStateException if a lock already exists for this system
     */
    public void lockPatchTable(Connection conn) throws SQLException, IllegalStateException
    {
        if (isPatchTableLocked(conn))
        {
            throw new IllegalStateException("Patch table is already locked!");
        }
        updatePatchLock(conn, true);
    }

    /**
     * Removes any locks for this system in the patches table. 
     * 
     * @param  conn the database connection to use 
     * @throws SQLException if an unrecoverable database error occurs
     */    
    public void unlockPatchTable(Connection conn) throws SQLException
    {
        updatePatchLock(conn, false);
    }

    /**
     * Returns the name of the system being patched.
     * 
     * @return the name of the system being patched
     */
    public String getSystemName()
    {
        return systemName;
    }

    /**
     * Returns the SQL to execute for the SQL dialect associated with this
     * object.
     * 
     * @param  key the key within <code><i>dialect</i>.properties</code> whose
     *         SQL should be returned
     * @return the SQL to execute for the SQL dialect associated with this
     *         object
     */
    String getSql(String key)
    {
        /* NOTE: This method is left package-protected so that it can be used
           by the unit test */

        return sqlStatements.getProperty(key);
    }
    
    /**
     * Creates an initial record in the patches table for this system. 
     * 
     * @param  conn the database connection to use
     * @throws SQLException if an unrecoverable database error occurs
     */
    private void createSystemPatchRecord(Connection conn) throws SQLException
    {
        PreparedStatement stmt = null;
        ResultSet rs = null;
        try
        {
            log.info("Create patch record for " + systemName);
            stmt = conn.prepareStatement(getSql("level.create"));
            stmt.setString(1, systemName);
            stmt.execute();
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
     * @param  conn the database connection to use
     * @param  lock <code>true</code> if a lock is to be obtained, <code>false</code>
     *         if it is to be removed 
     * @throws SQLException if an unrecoverable database error occurs
     */        
    private void updatePatchLock(Connection conn, boolean lock) throws SQLException
    {
        String sqlkey = (lock) ? "lock.obtain" : "lock.release";
        PreparedStatement stmt = null;
        
        try
        {
            stmt = conn.prepareStatement(getSql(sqlkey));
            if (log.isDebugEnabled())
            {
                log.fatal("Updating patch table lock: " + getSql(sqlkey));
            }
            stmt.setString(1, systemName);
            stmt.execute();
        }
        finally
        {
            SqlUtil.close(null, stmt, null);
        }
    }

    /**
     * Returns the <code>Properties</code> contained in the given resource bundle.
     * 
     * @param  filename the name of the resource bundle to load, relative to this
     *         package
     * @return the properties contained in the specified resource bundle, or
     *         <code>null</code> if the bundle could not be loaded
     */
    private Properties getResources(String filename)
    {
        InputStream is = getClass().getResourceAsStream(filename);
        if (is == null)
        {
            return null;
        }
        
        Properties p = new Properties();
        try
        {
            p.load(is);
        }
        catch (IOException e)
        {
            log.error("Couldn't find resource '" + filename + "'");
            return null;
        }
        return p;
    }

    /**
     * Sets the system name.
     * 
     * @param name the system name for this patch table
     */
    private void setSystemName(String name)
    {
        if (name == null)
        {
            throw new IllegalArgumentException("systemName cannot be null");
        }
        if (name.length() > MAX_SYSTEMNAME_LENGTH)
        {
            throw new IllegalArgumentException("systemName cannot be longer than "
                + MAX_SYSTEMNAME_LENGTH + " characters");
        }
        this.systemName = name;
    }
}
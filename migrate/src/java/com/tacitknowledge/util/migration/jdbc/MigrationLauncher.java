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

import java.sql.Connection;
import java.sql.SQLException;
import java.util.StringTokenizer;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.MigrationListener;
import com.tacitknowledge.util.migration.Migration;
import com.tacitknowledge.util.migration.MigrationTask;

/**
 * Core starting point for a "patches" table-based application migration run.
 * 
 * @author  Scott Askew (scott@tacitknowledge.com)
 * @version $Id$
 */
public class MigrationLauncher implements MigrationListener
{
    /**
     * Class logger
     */
    private static Log log = LogFactory.getLog(MigrationLauncher.class);
    
    /**
     * The patch table in use
     */
    private PatchTable table = null;
    
    /**
     * The <code>Migration</code> responsible for applying the patches
     */
    private Migration manager = null;
    
    /**
     * The amount time, in milliseconds, between attempts to obtain a lock on the
     * patches table.  Defaults to 15 seconds.
     */
    private long lockPollMillis = 15000;
    
    /**
     * Creates a new <code>MigrationLauncher</code>.
     * 
     * @param table the patch table used to persist and coordinate patche info. 
     */
    public MigrationLauncher(PatchTable table)
    {
        this.table = table;
        manager = new Migration();
        manager.addMigrationTaskSource(new SqlScriptMigrationTaskSource());
    }
    
    /**
     * Starts the application migration process.
     * 
     * @param  conn the connection to use
     * @return the number of patches applied
     * @throws SQLException if an unrecoverable database error occurs while
     *         working with the patches table.
     * @throws MigrationException if an unrecoverable error occurs during
     *         the migration
     */
    public int doMigrations(Connection conn) throws SQLException, MigrationException
    {
        
        // Make sure the table is created first
        table.getPatchLevel(conn);
        conn.commit();
        
        // Turn off auto-commit
        boolean b = conn.getAutoCommit();
        conn.setAutoCommit(false);
        
        // Patch locks ensure that only one system sharing a database will patch
        // it at the same time.
        waitForFreeLock(conn);
        
        try
        {
            table.lockPatchTable(conn);
            
            int patchLevel = table.getPatchLevel(conn);
        
            // Make sure this class is notified when a patch is applied so that
            // the patch level can be updated (see #migrationSuccessful).
            manager.addListener(this);
            
            JdbcMigrationContext context = new JdbcMigrationContext();
            context.setConnection(conn);
            return manager.doMigrations(patchLevel, context);
        }
        // Handle the two checked exceptions and any runtime exceptions.  This is
        // written this way so that the exception can be rethrown without requiring
        // this method to declare something vague like "throws Exception".
        catch (SQLException e)
        {
            handleException(conn, e);
            throw e;
        }
        catch (MigrationException e)
        {
            handleException(conn, e);
            throw e;
        }
        catch (RuntimeException e)
        {
            handleException(conn, e);
            throw e;
        }
        finally
        {
            try
            {
                table.unlockPatchTable(conn);
                conn.commit();
            }
            catch (SQLException e)
            {
                log.error("Error unlocking patch table: ", e);
            }
        }
    }
    
    /**
     * Sets the colon-separated path of packages and directories within the
     * search path that are sources of patches. 
     * 
     * @param searchPath a colon-separated path of packages and directories within the
     *        search path that are sources of patches
     */
    public void setSearchPath(String searchPath)
    {
        StringTokenizer st = new StringTokenizer(searchPath, ":");
        while (st.hasMoreTokens())
        {
            String path = st.nextToken();
            if (path.indexOf('/') > -1)
            {
                manager.addResourceDirectory(path);
            }
            else
            {
                manager.addResourcePackage(path);
            }
        }
    }

    /**
     * @see MigrationListener#migrationStarted(MigrationTask, MigrationContext)
     */
    public void migrationStarted(MigrationTask task, MigrationContext context)
        throws MigrationException
    {
        // Nothing to do
    }

    /**
     * @see MigrationListener#migrationSuccessful(MigrationTask, MigrationContext)
     */
    public void migrationSuccessful(MigrationTask task, MigrationContext context)
        throws MigrationException
    {
        int patchLevel = task.getLevel().intValue();
        try
        {
            JdbcMigrationContext jdbcContext = (JdbcMigrationContext) context;
            table.updatePatchLevel(jdbcContext.getConnection(), patchLevel);
        }
        catch (SQLException e)
        {
            throw new MigrationException("Could not update patch level", e);
        }
    }

    /**
     * @see MigrationListener#migrationFailed(MigrationTask, MigrationContext, MigrationException)
     */
    public void migrationFailed(MigrationTask task, MigrationContext context, MigrationException e)
        throws MigrationException
    {
        // Nothing to do
    }
    
    /**
     * Pauses until the patch lock become available.
     * 
     * @param  conn the database connection to use to access the patches table
     * @throws SQLException if an unrecoverable error occurs
     */
    private void waitForFreeLock(Connection conn) throws SQLException
    {
        while (table.isPatchTableLocked(conn))
        {
            log.info("Waiting for migration lock for system \"" + table.getSystemName() + "\"");
            try
            {
                Thread.sleep(lockPollMillis);
            }
            catch (InterruptedException e)
            {
                log.error("Recieved InterruptedException while waiting for patch lock", e);
            }
        }
    }
    
    /**
     * Remove any patch table locks and commit the transaction.
     * 
     * @param  conn the database connection in use
     * @param  e the root cause
     * @throws SQLException if the patch lock could not be revoked or committed
     */
    private void handleException(Connection conn, Exception e) throws SQLException
    {
        log.error("Error during migration; removing patch lock.", e);
        table.unlockPatchTable(conn);
        conn.commit();
    }
}

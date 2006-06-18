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

import java.sql.Connection;
import java.sql.SQLException;
import java.util.StringTokenizer;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.MigrationListener;
import com.tacitknowledge.util.migration.MigrationProcess;
import com.tacitknowledge.util.migration.MigrationTask;
import com.tacitknowledge.util.migration.PatchInfoStore;
import com.tacitknowledge.util.migration.jdbc.util.SqlUtil;

/**
 * Core starting point for a database migration run.  This class obtains a connection
 * to the database, checks its patch level, delegates the actual execution of the migration
 * tasks to a <code>MigrationProcess</code> instance, and then commits and cleans everything
 * up at the end.
 * <p>
 * This class is <b>NOT</b> threadsafe.
 *
 * @author  Scott Askew (scott@tacitknowledge.com)
 */
public class JdbcMigrationLauncher implements MigrationListener
{
    /**
     * Class logger
     */
    private static Log log = LogFactory.getLog(JdbcMigrationLauncher.class);

    /**
     * The patch level store in use
     */
    private PatchInfoStore patchTable = null;

    /**
     * The <code>MigrationProcess</code> responsible for applying the patches
     */
    private MigrationProcess migrationProcess = null;

    /**
     * The amount time, in milliseconds, between attempts to obtain a lock on the
     * patches table.  Defaults to 15 seconds.
     */
    private long lockPollMillis = 15000;

    /**
     * The path containing directories and packages to search through to locate patches.
     */
    private String patchPath = null;

    /**
     * The <code>MigrationContext</code> to use for all migrations.
     */
    private JdbcMigrationContext context = null;

    /**
     * Create a new MigrationProcess and add a SqlScriptMigrationTaskSource
     */
    public JdbcMigrationLauncher()
    {
        setMigrationProcess(getNewMigrationProcess());

        // Make sure this class is notified when a patch is applied so that
        // the patch level can be updated (see #migrationSuccessful).
        migrationProcess.addListener(this);
        
        getMigrationProcess().addMigrationTaskSource(new SqlScriptMigrationTaskSource());
    }

    /**
     * Create a new <code>MigrationLancher</code>.
     *
     * @param context the <code>JdbcMigrationContext</code> to use.
     * @throws MigrationException if an unexpected error occurs
     */
    public JdbcMigrationLauncher(JdbcMigrationContext context) throws MigrationException
    {
        this();
        setContext(context);
    }
    
    /**
     * Get the MigrationProcess we'll use to migrate things
     * 
     * @return MigrationProcess for migration control
     */
    public MigrationProcess getNewMigrationProcess()
    {
        return new MigrationProcess();
    }

    /**
     * Starts the application migration process.
     *
     * @return the number of patches applied
     * @throws MigrationException if an unrecoverable error occurs during
     *         the migration
     */
    public int doMigrations() throws MigrationException
    {
        if (context == null)
        {
            throw new MigrationException("You must configure a migration context");
        }
        
        Connection conn = null;
        try
        {
            conn = context.getConnection();
            return doMigrations(conn);
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
     * Returns the colon-separated path of packages and directories within the
     * class path that are sources of patches.
     *
     * @return a colon-separated path of packages and directories within the
     *         class path that are sources of patches
     */
    public String getPatchPath()
    {
        return patchPath;
    }

    /**
     * Sets the colon-separated path of packages and directories within the
     * class path that are sources of patches.
     *
     * @param searchPath a colon-separated path of packages and directories within the
     *        class path that are sources of patches
     */
    public void setPatchPath(String searchPath)
    {
        this.patchPath = searchPath;
        StringTokenizer st = new StringTokenizer(searchPath, ":");
        while (st.hasMoreTokens())
        {
            String path = st.nextToken();
            if (path.indexOf('/') > -1)
            {
                migrationProcess.addResourceDirectory(path);
            }
            else
            {
                migrationProcess.addResourcePackage(path);
            }
        }
    }

    /**
     * @see MigrationListener#migrationStarted(MigrationTask, MigrationContext)
     */
    public void migrationStarted(MigrationTask task, MigrationContext ctx)
        throws MigrationException
    {
        log.debug("Started task " + task.getName() + " for context " + ctx);
    }

    /**
     * @see MigrationListener#migrationSuccessful(MigrationTask, MigrationContext)
     */
    public void migrationSuccessful(MigrationTask task, MigrationContext ctx)
        throws MigrationException
    {
        log.debug("Task " + task.getName() + " was successful for context " + ctx);
        int patchLevel = task.getLevel().intValue();
        patchTable.updatePatchLevel(patchLevel);
    }

    /**
     * @see MigrationListener#migrationFailed(MigrationTask, MigrationContext, MigrationException)
     */
    public void migrationFailed(MigrationTask task, MigrationContext ctx, MigrationException e)
        throws MigrationException
    {
        log.debug("Task " + task.getName() + " failed for context " + ctx, e);
    }

    /**
     * Get the patch level from the database
     *
     * @return int representing the current database patch level
     * @exception MigrationException if there is a database connection error,
     *                         or the patch level can't be determined
     */
    public int getDatabasePatchLevel() throws MigrationException
    {
        return patchTable.getPatchLevel();
    }

    /**
     * Get the next patch level, for use when creating a new patch
     *
     * @return int representing the first unused patch number
     * @exception MigrationException if the next patch level can't be determined
     */
    public int getNextPatchLevel() throws MigrationException
    {
        return migrationProcess.getNextPatchLevel();
    }

    /**
     * Sets the <code>JdbcMigrationContext</code> used for the migrations.
     *
     * @param jdbcMigrationContext the <code>JdbcMigrationContext</code> used for the migrations
     * @throws MigrationException if a database connection cannot be obtained
     */
    public void setContext(JdbcMigrationContext jdbcMigrationContext) 
        throws MigrationException
    {
        this.context = jdbcMigrationContext;
        try
        {
            patchTable = new PatchTable(context, context.getConnection());
        }
        catch (SQLException e)
        {
            throw new MigrationException("Could not obtain JDBC Connection", e);
        }
    }

    /**
     * Returns the <code>JdbcMigrationContext</code> used for the migrations.
     *
     * @return the <code>JdbcMigrationContext</code> used for the migrations
     */
    public JdbcMigrationContext getContext()
    {
        return context;
    }

    /**
     * Performs the application migration process in one go
     *
     * @param  conn the connection to use
     * @return the number of patches applied
     * @throws SQLException if an unrecoverable database error occurs while
     *         working with the patches table.
     * @throws MigrationException if an unrecoverable error occurs during
     *         the migration
     */
    protected int doMigrations(Connection conn) throws SQLException, MigrationException
    {
        patchTable = createPatchStore(conn);

        // Save auto-commit state
        boolean b = true;
        try
        {
            lockPatchStore();

            // make sure we can at least attempt to roll back patches
            // DDL can usually don't rollback - we'd need compensating transactions
            b = conn.getAutoCommit();
            conn.setAutoCommit(false);
            
            try
            {
                int patchLevel = patchTable.getPatchLevel();
                return migrationProcess.doMigrations(patchLevel, context);
            }
            finally
            {
                try
                {
                    patchTable.unlockPatchStore();
                    conn.commit();
                }
                catch (SQLException e)
                {
                    log.error("Error unlocking patch table: ", e);
                }
            }
        }
        finally
        {
            // restore auto-commit setting
            conn.setAutoCommit(b);
        }
    }

    /**
     * Lock the patch store. This is done safely, such that we safely handle the case where 
     * other migration launchers are patching at the same time.
     * 
     * @throws MigrationException if the reading or setting lock state fails
     */
    private void lockPatchStore() throws MigrationException
    {
        // Patch locks ensure that only one system sharing a patch store will patch
        // it at the same time.
        boolean lockObtained = false;
        while (!lockObtained)
        {
            waitForFreeLock();

            try
            {   
                patchTable.lockPatchStore();
                lockObtained = true;
            }
            catch (IllegalStateException ise)
            {
                // this happens when someone woke up at the same time,
                // raced us to the lock and won. We re-sleep and try again.
            }
        }
    }
    
    /**
     * create a patch table object for use in migrations
     * 
     * @param conn the database connection to use for table access
     * @return PatchTable object for use in accessing patch state information
     * @throws MigrationException if unable to create the store
     */
    protected PatchInfoStore createPatchStore(Connection conn) throws MigrationException
    {
        PatchInfoStore piStore = new PatchTable(context, conn);

        // Make sure the table is created before claiming it exists by returning
        patchTable.getPatchLevel();
        try
        {
            if (!conn.getAutoCommit())
            {
                conn.commit();
            }
        }
        catch (SQLException sqle)
        {
            throw new MigrationException("Unable to commit connection after creating patch store");
        }
        
        return piStore;
    }

    /**
     * Pauses until the patch lock become available.
     *
     * @throws MigrationException if an unrecoverable error occurs
     */
    private void waitForFreeLock() throws MigrationException
    {
        while (patchTable.isPatchStoreLocked())
        {
            log.info("Waiting for migration lock for system \"" + context.getSystemName() + "\"");
            try
            {
                Thread.sleep(getLockPollMillis());
            }
            catch (InterruptedException e)
            {
                log.error("Received InterruptedException while waiting for patch lock", e);
            }
        }
    }
    
    /**
     * Get how long to wait for the patch store lock
     * 
     * @return the wait time for the patch store, in milliseconds
     */
    public long getLockPollMillis()
    {
        return lockPollMillis;
    }
    
    /**
     * Set how long to wait for the patch store lock
     * 
     * @param lockPollMillis the wait time for the patch store, in milliseconds
     */
    public void setLockPollMillis(long lockPollMillis)
    {
        this.lockPollMillis = lockPollMillis;
    }

    /**
     * Get the migration process to use for migrations
     * 
     * @return MigrationProcess to use for migrations
     */
    public MigrationProcess getMigrationProcess()
    {
        return migrationProcess;
    }

    /**
     * Set the migration process to use for migrations
     * 
     * @param migrationProcess the MigrationProcess to use for migrations
     */
    public void setMigrationProcess(MigrationProcess migrationProcess)
    {
        this.migrationProcess = migrationProcess;
    }

    /**
     * Get the patch table we use to store migration information
     * 
     * @return PatchStore with information about our migration state
     */
    public PatchInfoStore getPatchTable()
    {
        return patchTable;
    }

    /**
     * Set the patch table where we should put information about the migrationo
     * 
     * @param patchTable where we should put information about the migration
     */
    public void setPatchTable(PatchInfoStore patchTable)
    {
        this.patchTable = patchTable;
    }
}

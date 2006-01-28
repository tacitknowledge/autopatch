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
import java.sql.SQLException;
import java.util.StringTokenizer;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.MigrationListener;
import com.tacitknowledge.util.migration.MigrationProcess;
import com.tacitknowledge.util.migration.MigrationTask;
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
 * @version $Id$
 */
public class JdbcMigrationLauncher implements MigrationListener
{
    /**
     * Class logger
     */
    private static Log log = LogFactory.getLog(JdbcMigrationLauncher.class);

    /**
     * The patch table in use
     */
    private PatchTable patchTable = null;

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
        migrationProcess = new MigrationProcess();
        migrationProcess.addMigrationTaskSource(new SqlScriptMigrationTaskSource());
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
        setJdbcMigrationContext(context);
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
        try
        {
            patchTable.updatePatchLevel(patchLevel);
        }
        catch (SQLException e)
        {
            throw new MigrationException("Could not update patch level", e);
        }
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
     * @exception SQLException if there is a database connection error,
     *                         or the patch level can't be determined
     */
    public int getDatabasePatchLevel() throws SQLException
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
    public void setJdbcMigrationContext(JdbcMigrationContext jdbcMigrationContext) 
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
    public JdbcMigrationContext getJdbcMigrationContext()
    {
        return context;
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
    private int doMigrations(Connection conn) throws SQLException, MigrationException
    {
        patchTable = createPatchTable(conn);

        // Make sure the table is created first
        patchTable.getPatchLevel();
        if (!conn.getAutoCommit())
        {
            conn.commit();
        }

        // Turn off auto-commit
        boolean b = true;
        try
        {
            b = conn.getAutoCommit();
            conn.setAutoCommit(false);

            // Patch locks ensure that only one system sharing a database will patch
            // it at the same time.
            waitForFreeLock();

            try
            {
                patchTable.lockPatchTable();

                int patchLevel = patchTable.getPatchLevel();

                // Make sure this class is notified when a patch is applied so that
                // the patch level can be updated (see #migrationSuccessful).
                migrationProcess.addListener(this);

                return migrationProcess.doMigrations(patchLevel, context);
            }
            finally
            {
                try
                {
                    patchTable.unlockPatchTable();
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
     * create a patch table object for use in migrations
     * 
     * @param conn the database connection to use for table access
     * @return PatchTable object for use in accessing patch state information
     */
    protected PatchTable createPatchTable(Connection conn)
    {
        return new PatchTable(context, conn);
    }

    /**
     * Pauses until the patch lock become available.
     *
     * @throws SQLException if an unrecoverable error occurs
     */
    private void waitForFreeLock() throws SQLException
    {
        while (patchTable.isPatchTableLocked())
        {
            log.info("Waiting for migration lock for system \"" + context.getSystemName() + "\"");
            try
            {
                Thread.sleep(lockPollMillis);
            }
            catch (InterruptedException e)
            {
                log.error("Received InterruptedException while waiting for patch lock", e);
            }
        }
    }
}

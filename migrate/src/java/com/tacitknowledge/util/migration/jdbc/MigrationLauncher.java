/* Copyright 2004, 2005 Tacit Knowledge LLC
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

import javax.naming.NamingException;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.MigrationListener;
import com.tacitknowledge.util.migration.Migration;
import com.tacitknowledge.util.migration.MigrationTask;

/**
 * Core starting point for a database migration run.
 * <p>
 * This class is <b>NOT</b> threadsafe.
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
     * The <code>MigrationContext</code> to use for all migrations.
     */
    private JdbcMigrationContext context = null;
    
    /**
     * Create a new <code>MigrationLancher</code>.
     */
    public MigrationLauncher()
    {
        // Nothing to do
    }
    
    /**
     * Create a new <code>MigrationLancher</code>.
     * 
     * @param context the <code>JdbcMigrationContext</code> to use.
     */
    public MigrationLauncher(JdbcMigrationContext context)
    {
        setJdbcMigrationContext(context);
    }

    /**
     * Create a new <code>MigrationLancher</code>.
     * 
     * @param  context the <code>JdbcMigrationContext</code> to use.
     * @param  systemName the name of the system
     * @throws MigrationException if an unexpected error occurs during initialization 
     */
    public MigrationLauncher(JdbcMigrationContext context, String systemName)
        throws MigrationException
    {
        setJdbcMigrationContext(context);
        init(systemName);
    }
    
    /**
     * Initializes the <code>MigrationLauncher</code>.
     * 
     * @param  systemName the name of the system to patch
     * @throws MigrationException if the <code>MigrationLauncher</code> could
     *         not be created 
     */
    public void init(String systemName) throws MigrationException
    {
        if (getJdbcMigrationContext() == null)
        {
            throw new IllegalStateException("The JdbcMigrationContext must be set before init()");
        }
        
        String dialect = getRequiredParam(systemName
            + JdbcMigrationContext.DIALECT_PROPERTY_SUFFIX);
        this.table = new PatchTable(systemName, dialect);

        this.manager = new Migration();
        this.manager.addMigrationTaskSource(new SqlScriptMigrationTaskSource());

        String path = getRequiredParam(systemName + JdbcMigrationContext.PATCH_PATH_SUFFIX);
        this.setSearchPath(path);
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
        if (manager == null)
        {
            throw new IllegalStateException("init() must be called before doMigrations()");
        }
        
        Connection conn = null;
        try
        {
            conn = getConnection(table.getSystemName());
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
    public void migrationStarted(MigrationTask task, MigrationContext ctx)
        throws MigrationException
    {
        // Nothing to do
    }

    /**
     * @see MigrationListener#migrationSuccessful(MigrationTask, MigrationContext)
     */
    public void migrationSuccessful(MigrationTask task, MigrationContext ctx)
        throws MigrationException
    {
        int patchLevel = task.getLevel().intValue();
        try
        {
            JdbcMigrationContext jdbcContext = (JdbcMigrationContext) ctx;
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
    public void migrationFailed(MigrationTask task, MigrationContext ctx, MigrationException e)
        throws MigrationException
    {
        // Nothing to do
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
        return table.getPatchLevel(getConnection(table.getSystemName()));
    }
    
    /**
     * Get the next patch level, for use when creating a new patch
     *
     * @return int representing the first unused patch number
     * @exception MigrationException if the next patch level can't be determined
     */
    public int getNextPatchLevel() throws MigrationException
    {
        return manager.getNextPatchLevel();
    }

    /**
     * Sets the <code>JdbcMigrationContext</code> used for the migrations.
     * 
     * @param jdbcMigrationContext the <code>JdbcMigrationContext</code> used for the migrations
     */
    public void setJdbcMigrationContext(JdbcMigrationContext jdbcMigrationContext)
    {
        this.context = jdbcMigrationContext;
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
            
            context.setConnection(conn);
            return manager.doMigrations(patchLevel, context);
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
     * Returns the database connection for this system.
     * 
     * @param  systemName the name of the system being patched
     * @return the database connection to use during migration
     * @throws SQLException if an unrecoverable error occured while creating
     *         the database connection
     */
    protected Connection getConnection(String systemName)
        throws SQLException
    {
        // TODO: Improve validation and error handling
        try
        {
            String dsn = context.getConfiguration().getProperty(systemName + ".jdbc.datasource");
            if (dsn != null)
            {
                log.debug("Creating new connection from JNDI DataSource..");
                return SqlUtil.getConnection(dsn);
            }
            else
            {
                log.debug("Creating brand-new unpooled connection..");
                String driver = getRequiredParam(systemName + ".jdbc.driver");
                String url = getRequiredParam(systemName + ".jdbc.url");
                String user = getRequiredParam(systemName + ".jdbc.username");
                String pass = getRequiredParam(systemName + ".jdbc.password");
                return SqlUtil.getConnection(driver, url, user, pass);
            }
        }
        catch (NamingException e)
        {
            throw new RuntimeException("JNDI error creating connection", e);
        }
        catch (ClassNotFoundException e)
        {
            throw new RuntimeException("Could not find JDBC driver", e);
        }
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
                log.error("Received InterruptedException while waiting for patch lock", e);
            }
        }
    }
    
    /**
     * Returns the given parameter from the context configuration.
     * 
     * @param  key the name of the property to return 
     * @return the value of the property
     * @throws IllegalArgumentException if the property could not be found
     */
    private String getRequiredParam(String key)
        throws IllegalArgumentException
    {
        String value = context.getConfiguration().getProperty(key);
        if (value == null)
        {
            throw new IllegalArgumentException("'" + key + "' is a required "
                + MigrationContext.MIGRATION_CONFIG_FILE + " parameter.  Aborting.");
        }
        return value;
    }
}

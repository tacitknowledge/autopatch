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

import java.sql.SQLException;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationException;

/**
 * Support using AutoPatch via injection I think. Jacques Morel contributed this, 
 * and I'm not sure what it does actually.
 * 
 * @author Jacques Morel
 */
public class AutoPatchSupport
{
    /** Class logger */
    private static final Log log = LogFactory.getLog(AutoPatchSupport.class);

    /** The launcher we'll use */
    private JdbcMigrationLauncher launcher;

    /**
     * Create a new support component for the given system name. 
     * This will use create a factory for you.
     * 
     * @param systemName the name of the system to be patched
     * @throws MigrationException if there is a problem
     * @see JdbcMigrationLauncherFactoryLoader
     */
    public AutoPatchSupport(String systemName) throws MigrationException
    {
        this(JdbcMigrationLauncherFactoryLoader.createFactory(), systemName);
    }

    /**
     * Create support component with the given factory and system name
     * 
     * @param launcherFactory the factory to use for migrations
     * @param systemName the system to patch
     * @throws MigrationException if there is any problem
     */
    public AutoPatchSupport(JdbcMigrationLauncherFactory launcherFactory,
            String systemName) throws MigrationException
    {
        this(launcherFactory.createMigrationLauncher(systemName));
    }

    /**
     * Create a support component with the given configured launcher
     * 
     * @param launcher the launcher to use for the migrations
     */
    public AutoPatchSupport(JdbcMigrationLauncher launcher)
    {
        this.launcher = launcher;
    }

    /**
     * Create the patch table (if necessary)
     * 
     * @return PatchTable object for the configured migration launcher
     * @throws SQLException if there is a problem
     */
    public PatchTable makePatchTable() throws SQLException
    {
        JdbcMigrationContext jdbcMigrationContext = launcher.getJdbcMigrationContext();
        return new PatchTable(jdbcMigrationContext, jdbcMigrationContext.getConnection());
    }
    
    /**
     * Set the patch level to the specified level
     * 
     * @param patchLevel the level to set the patch table to
     * @throws SQLException if there is a problem creating the patch table
     * @throws MigrationException if the store can't be locked
     */
    public void setPatchLevel(int patchLevel) throws MigrationException, SQLException
    {
        PatchTable patchTable = makePatchTable();
        patchTable.lockPatchStore();
        patchTable.updatePatchLevel(patchLevel);
        log.info("Set the patch level to " + patchLevel);
        patchTable.unlockPatchStore();
    }

    /**
     * Get the current patch level
     * 
     * @return int with the patch level in the patch database
     * @throws SQLException if there is a problem creating the patch table
     * @throws MigrationException if there is a problem getting the patch level
     */
    public int getPatchLevel() throws SQLException, MigrationException
    {
        PatchTable patchTable = makePatchTable();
        return patchTable.getPatchLevel();
    }
    
    /**
     * Get the highest patch level of all the configured patches
     * 
     * @return int with the highest patch level
     * @throws MigrationException if there is problem getting the patch level
     */
    public int getHighestPatchLevel() throws MigrationException
    {
        return launcher.getNextPatchLevel() - 1;
    }
}
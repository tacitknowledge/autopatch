/* 
 * Copyright 2007 Tacit Knowledge LLC
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

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.jdbc.util.ConfigurationUtil;

/**
 * Launches the migration process as a standalone application.
 * <p>
 * This class expects the following Java environment parameters:
 * <ul>
 * <li>migration.systemname - the name of the logical system being migrated</li>
 * <li>migration.settings (optional) - the name of the settings file to use for
 * migration</li>
 * </ul>
 * <p>
 * Below is an example of how this class can be configured in build.xml:
 * 
 * <pre>
 *   ...
 *  &lt;target name=&quot;patch.database&quot; description=&quot;Runs the migration system&quot;&gt;
 *   &lt;java 
 *       fork=&quot;true&quot;
 *       classpathref=&quot;patch.classpath&quot; 
 *       failonerror=&quot;true&quot; 
 *       classname=&quot;com.tacitknowledge.util.migration.jdbc.StandaloneMigrationLauncher&quot;&gt;
 *     &lt;sysproperty key=&quot;migration.systemname&quot; value=&quot;${application.name}&quot;/&gt;
 *   &lt;/java&gt;
 * &lt;/target&gt;
 *   ...
 * </pre>
 * 
 * @author Mike Hardy (mike@tacitknowledge.com)
 * @see com.tacitknowledge.util.migration.MigrationProcess
 */
public final class StandaloneMigrationLauncher
{
    /**
     * The force rollback parameter
     */
    private static final String FORCE_ROLLBACK = "-force";

    /**
     * The rollback parameter
     */
    private static final String ROLLBACK = "-rollback";

    /**
     * Class logger
     */
    private static Log log = LogFactory.getLog(StandaloneMigrationLauncher.class);

    /**
     * Private constructor - this object shouldn't be instantiated
     */
    private StandaloneMigrationLauncher()
    {
        // does nothing
    }

    /**
     * Run the migrations for the given system name
     * 
     * @param arguments
     *                the command line arguments, if any
     * @exception Exception
     *                    if anything goes wrong
     */
    public static void main(final String[] arguments) throws Exception
    {

        String migrationSystemName = ConfigurationUtil.getRequiredParam("migration.systemname",
                System.getProperties(), arguments, 0);
        String migrationSettings = ConfigurationUtil.getOptionalParam("migration.settings", System
                .getProperties(), arguments, 1);

        boolean isRollback = false;
        int rollbackLevel = -1;
        boolean forceRollback = false;

        for (int i = 0; i < arguments.length; i++)
        {
            String argument1 = arguments[i];

            if (ROLLBACK.equals(argument1))
            {
                isRollback = true;

                if (i + 2 <= arguments.length)
                {
                    String argument2 = arguments[i + 1];

                    if (argument2 != null)
                    {
                        rollbackLevel = Integer.parseInt(argument2);
                    }
                }

                if (rollbackLevel == -1)
                {
                    // this indicates that the rollback level has not been set
                    throw new MigrationException(
                            "The rollback flag requires a following integer parameter to indicate the rollback level.");
                }
            }

            if (FORCE_ROLLBACK.equals(argument1))
            {
                forceRollback = true;
            }

        }

        // The MigrationLauncher is responsible for handling the interaction
        // between the PatchTable and the underlying MigrationTasks; as each
        // task is executed, the patch level is incremented, etc.
        try
        {
            if (isRollback)
            {
                log
                        .info("Found rollback flag. Autopatch will attempt to rollback the system to patch level "
                                + rollbackLevel + ".");
                doRollbacks(migrationSystemName, migrationSettings, rollbackLevel, forceRollback);
            }
            else
            {
                doMigrations(migrationSystemName, migrationSettings);
            }
        }
        catch (Exception e)
        {
            log.error(e);
            throw e;
        }
    }

    /**
     * Private helper method to initiate the migration process.
     * 
     * @param migrationSystemName
     *                the name of the system to migrate
     * @param migrationSettings
     *                additional properties for migration
     * @throws MigrationException
     */
    private static void doRollbacks(final String migrationSystemName,
            final String migrationSettings, final int rollbackLevel, final boolean forceRollback)
            throws MigrationException
    {
        JdbcMigrationLauncherFactory launcherFactory = JdbcMigrationLauncherFactoryLoader
                .createFactory();
        JdbcMigrationLauncher launcher = null;

        if (migrationSettings == null)
        {
            log.info("Using migration.properties (default)");
            launcher = launcherFactory.createMigrationLauncher(migrationSystemName);
        }
        else
        {
            log.info("Using " + migrationSettings);
            launcher = launcherFactory.createMigrationLauncher(migrationSystemName,
                    migrationSettings);
        }

        launcher.doRollbacks(rollbackLevel, forceRollback);
    }

    /**
     * Private helper method to initiate the migration process.
     * 
     * @param migrationSystemName
     *                the name of the system to migrate
     * @param migrationSettings
     *                additional properties for migration
     * @throws MigrationException
     */
    private static void doMigrations(final String migrationSystemName,
            final String migrationSettings) throws MigrationException
    {
        JdbcMigrationLauncherFactory launcherFactory = JdbcMigrationLauncherFactoryLoader
                .createFactory();
        JdbcMigrationLauncher launcher = null;

        if (migrationSettings == null)
        {
            log.info("Using migration.properties (default)");
            launcher = launcherFactory.createMigrationLauncher(migrationSystemName);
        }
        else
        {
            log.info("Using " + migrationSettings);
            launcher = launcherFactory.createMigrationLauncher(migrationSystemName,
                    migrationSettings);
        }

        launcher.doMigrations();
    }
}

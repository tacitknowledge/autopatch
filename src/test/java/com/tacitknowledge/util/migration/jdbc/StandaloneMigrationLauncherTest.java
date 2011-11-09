package com.tacitknowledge.util.migration.jdbc;

import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.jdbc.util.MigrationUtil;
import junit.framework.TestCase;
import org.easymock.EasyMock;
import org.easymock.classextension.IMocksControl;

import static org.easymock.EasyMock.eq;
import static org.easymock.classextension.EasyMock.createStrictControl;

/**
 * @author Hemri Herrera hemri@tacitknowledge.com
 * @author Ulises Pulido ulises@tacitknowledge.com
 */

public class StandaloneMigrationLauncherTest extends TestCase
{

    public void testShouldRunMigrationsForcingRollback() throws Exception
    {
        StandaloneMigrationLauncher migrationLauncher = new StandaloneMigrationLauncher();
        String[] arguments = new String[]{"orders","migration.properties","-force", "-rollback", "1"};

        IMocksControl mockControl = createStrictControl();
        MigrationUtil migrationUtil = mockControl.createMock(MigrationUtil.class);

        migrationUtil.doRollbacks(eq("orders"), eq("migration.properties"), EasyMock.<int[]>anyObject(), eq(true));
        mockControl.replay();

        migrationLauncher.setMigrationUtil(migrationUtil);
        migrationLauncher.run(arguments);

        mockControl.verify();
    }


    public void testShouldRunMigrationsMultipleRollbacks() throws Exception
    {
        StandaloneMigrationLauncher migrationLauncher = new StandaloneMigrationLauncher();
        String[] arguments = new String[]{"orders","migration.properties","-force", "-rollback", "1,2,3,4,5,6"};

        IMocksControl mockControl = createStrictControl();
        MigrationUtil migrationUtil = mockControl.createMock(MigrationUtil.class);

        migrationUtil.doRollbacks(eq("orders"), eq("migration.properties"), EasyMock.<int[]>anyObject(), eq(true));
        mockControl.replay();

        migrationLauncher.setMigrationUtil(migrationUtil);
        migrationLauncher.run(arguments);

        mockControl.verify();
    }


    public void testShouldRunMigrationsMultipleRollbacksInvalidRollbackLevels() throws Exception
    {
        StandaloneMigrationLauncher migrationLauncher = new StandaloneMigrationLauncher();
        String[] arguments = new String[]{"orders","migration.properties","-force", "-rollback", "1,2C,3B,4D,5A,600"};

        try {
            migrationLauncher.run(arguments);
            fail("Should have thrown migration exception");
        } catch (MigrationException me) {
            assertEquals("The rollbacklevels should be integers separated by a comma", me.getMessage());
        }

    }

}

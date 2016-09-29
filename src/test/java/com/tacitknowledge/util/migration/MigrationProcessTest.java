/* Copyright 2004 Tacit Knowledge
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.tacitknowledge.util.migration;

import com.tacitknowledge.util.migration.builders.MockBuilder;
import com.tacitknowledge.util.migration.tasks.normal.TestMigrationTask2;
import com.tacitknowledge.util.migration.tasks.normal.TestMigrationTask3;
import junit.framework.TestCase;
import org.easymock.IMocksControl;

import java.util.ArrayList;
import java.util.List;

import static org.easymock.EasyMock.expectLastCall;
import static org.easymock.classextension.EasyMock.expect;
import static org.easymock.classextension.EasyMock.createStrictControl;


/**
 * Test the {@link MigrationProcess} class.
 *
 * @author Oscar Gonzalez (oscar@tacitknowledge.com)
 */

public class MigrationProcessTest extends TestCase
{

    private MigrationProcess migrationProcess = null;

    private IMocksControl migrationContextControl = null;

    private MigrationContext migrationContextMock = null;

    private IMocksControl migrationTaskSourceControl = null;

    private MigrationTaskSource migrationTaskSourceMock = null;

    private IMocksControl patchInfoStoreControl = null;

    private PatchInfoStore patchInfoStoreMock = null;
    private PatchInfoStore patchInfoStore;

    public void setUp() throws Exception
    {
        super.setUp();
        migrationProcess = new MigrationProcess();
        migrationProcess.
                setMigrationRunnerStrategy(MigrationRunnerFactory.getMigrationRunnerStrategy(null));
        migrationContextControl = createStrictControl();
        migrationContextMock = migrationContextControl.createMock(MigrationContext.class);
        migrationTaskSourceControl = createStrictControl();
        migrationTaskSourceMock = migrationTaskSourceControl.createMock(MigrationTaskSource.class);
        migrationProcess.addPatchResourcePackage("testPackageName");
        patchInfoStoreControl = createStrictControl();
        patchInfoStoreMock = patchInfoStoreControl.createMock(PatchInfoStore.class);
        patchInfoStore = MockBuilder.getPatchInfoStore(3);
    }

    public void testAddMigrationTaskSourceWhenNullSourceIsPassed()
    {
        try
        {
            migrationProcess.addMigrationTaskSource(null);
        }
        catch (IllegalArgumentException iaex)
        {
            assertEquals("source cannot be null.", iaex.getMessage());
            return;
        }
        fail("We should have fail before this.");

    }

    public void testApplyPatchWithNoBroadCasters() throws MigrationException
    {
        migrationContextMock.commit();
        migrationContextControl.replay();
        TestMigrationTask2 migrationTask = new TestMigrationTask2();
        migrationProcess.applyPatch(migrationContextMock, migrationTask, false);
        migrationContextControl.verify();
    }

    public void testApplyPatchWithBroadcasters() throws MigrationException
    {
        migrationContextMock.commit();
        migrationContextControl.replay();
        TestMigrationTask2 migrationTask = new TestMigrationTask2();
        migrationProcess.setMigrationBroadcaster(new MigrationBroadcaster());
        migrationProcess.applyPatch(migrationContextMock, migrationTask, true);
        migrationContextControl.verify();
    }

    public void testDryRunWithEmptyMigrationList() throws MigrationException {

        int taskCount = migrationProcess.dryRun(patchInfoStore, migrationContextMock, new ArrayList());
        assertEquals("Task count should be zero with an empty MigrationList", 0, taskCount);
    }

    public void testDryRunWithNullMigrationList() throws MigrationException {
        try
        {
            migrationProcess.dryRun(patchInfoStore, migrationContextMock, null);
        }
        catch (NullPointerException npe)
        {
            return; // We expected this
        }
        fail("A null List of migrations should throw a NPE");
    }

    public void testDryRunWithMigrationsInOrder() throws MigrationException {
        expect(patchInfoStoreMock.getPatchLevel()).andReturn(3).times(2);
        patchInfoStoreControl.replay();
        int taskCount = migrationProcess.dryRun(patchInfoStoreMock, migrationContextMock, getMigrationTasks());
        assertEquals("TaskCount should be equal to 2", 2, taskCount);
        patchInfoStoreControl.verify();
    }

    private List getMigrationTasks()
    {
        RollbackableMigrationTask migrationTask2 = new TestMigrationTask2();
        RollbackableMigrationTask migrationTask3 = new TestMigrationTask3();
        List migrationsList = new ArrayList();
        migrationsList.add(migrationTask2);
        migrationsList.add(migrationTask3);
        return migrationsList;
    }


    public void testDoMigrationInReadOnlyWithExistingTasksThrowsError() throws MigrationException
    {
        try
        {
            migrationProcess.setReadOnly(true);
            expect(migrationTaskSourceMock.getMigrationTasks("testPackageName")).andReturn(getMigrationTasks());
            migrationTaskSourceControl.replay();
            expect(patchInfoStoreMock.getPatchLevel()).andReturn(2).times(2);
            patchInfoStoreControl.replay();
            migrationProcess.addMigrationTaskSource(migrationTaskSourceMock);
            migrationProcess.doMigrations(patchInfoStoreMock, migrationContextMock);
        }
        catch (MigrationException miex)
        {
            migrationTaskSourceControl.verify();
            return; // We expect this, succesful scenario
        }
        fail("We should have thrown an error since we have migrations but we are in " +
                "read only mode");
    }


    public void testDoMigrationInReadOnlyWithZeroTasks() throws MigrationException
    {
        migrationProcess.setReadOnly(true);
        expect(migrationTaskSourceMock.getMigrationTasks("testPackageName")).andReturn(new ArrayList());
        migrationTaskSourceControl.replay();
        migrationProcess.addMigrationTaskSource(migrationTaskSourceMock);
        expect(patchInfoStoreMock.getPatchLevel()).andReturn(0);
        patchInfoStoreControl.replay();
        migrationProcess.doMigrations(patchInfoStoreMock, migrationContextMock);
    }

    public void testDoTwoMigrations() throws MigrationException
    {
        migrationProcess.setReadOnly(false);
        expect(migrationTaskSourceMock.getMigrationTasks("testPackageName")).andReturn(getMigrationTasks());
        migrationTaskSourceControl.replay();
        migrationProcess.addMigrationTaskSource(migrationTaskSourceMock);
        expect(patchInfoStoreMock.getPatchLevel()).andReturn(2).times(4);
        patchInfoStoreControl.replay();
        assertEquals("We should have executed 2 migrations",
                2, migrationProcess.doMigrations(patchInfoStoreMock, migrationContextMock));
    }

    public void testDontDoMigrations() throws MigrationException
    {
        migrationProcess.setReadOnly(false);
        expect(migrationTaskSourceMock.getMigrationTasks("testPackageName")).andReturn(getMigrationTasks());
        migrationTaskSourceControl.replay();
        migrationProcess.addMigrationTaskSource(migrationTaskSourceMock);
        expect(patchInfoStoreMock.getPatchLevel()).andReturn(100).times(4);
        patchInfoStoreControl.replay();
        assertEquals("We should have executed no migrations",
                0, migrationProcess.doMigrations(patchInfoStoreMock, migrationContextMock));
    }

}

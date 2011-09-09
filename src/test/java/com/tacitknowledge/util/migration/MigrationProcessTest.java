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

import com.tacitknowledge.util.migration.tasks.normal.TestMigrationTask2;
import com.tacitknowledge.util.migration.tasks.rollback.TestRollbackableTask1;
import junit.framework.TestCase;
import org.easymock.AbstractMatcher;
import org.easymock.MockControl;


/**
 * Test the {@link MigrationProcess} class.
 *
 * @author Oscar Gonzalez (oscar@tacitknowledge.com)
 */

public class MigrationProcessTest extends TestCase
{

    private MigrationProcess migrationProcess = null;

    private MockControl migrationContextControl = null;

    private MigrationContext migrationContextMock = null;

    public void setUp() throws Exception
    {
        super.setUp();
        migrationProcess = new MigrationProcess();
        migrationContextControl = MockControl.createStrictControl(MigrationContext.class);
        migrationContextMock =
                (MigrationContext) migrationContextControl.getMock();
    }

    public void testAddMigrationTaskSourceWhenNullSourceIsPassed()
    {
        try
        {
            migrationProcess.addMigrationTaskSource(null);
        }
        catch (IllegalArgumentException iaex)
        {
            assertEquals("source cannot be null." , iaex.getMessage());
        }

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

}

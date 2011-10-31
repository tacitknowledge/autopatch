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
import junit.framework.TestCase;


/**
 * Test the {@link OrderedMigrationRunnerStrategy} class.
 *
 * @author Oscar Gonzalez (oscar@tacitknowledge.com)
 */
public class OrderedMigrationRunnerStrategyTest  extends TestCase
{

    private MigrationRunnerStrategy migrationRunnerStrategy;

    public void setUp() throws Exception
    {
        super.setUp();
        migrationRunnerStrategy = new OrderedMigrationRunnerStrategy();
    }

    public void testShouldMigrationsRunInOrder() throws MigrationException {
        PatchInfoStore patchInfoStore = MockBuilder.getPatchInfoStore(2);

        assertTrue("Should be able to run migration if current level is below migration level",
                migrationRunnerStrategy.shouldMigrationRun(3, patchInfoStore));

    }

    public void testShouldMigrationFailIfCurrentLevelIsAboveMigrationLevel() throws MigrationException {
        PatchInfoStore patchInfoStore = MockBuilder.getPatchInfoStore(3);
        assertFalse("Should not be able to run migration if current level is above migration level",
                migrationRunnerStrategy.shouldMigrationRun(2, patchInfoStore));

    }

    public void testShouldMigrationFailIfCurrentAndMigrationLevelAreEquals() throws MigrationException {

        PatchInfoStore patchInfoStore = MockBuilder.getPatchInfoStore(3);
        assertFalse("Should not be able to run migration if current level and migration level are equal",
                migrationRunnerStrategy.shouldMigrationRun(3, patchInfoStore));
    }

    public void testSystemIsSynchronized( ) throws MigrationException {

        PatchInfoStore patchInfoStore = MockBuilder.getPatchInfoStore(3);
        PatchInfoStore currentPatchInfoStore = MockBuilder.getPatchInfoStore(3);;

        boolean systemSync = migrationRunnerStrategy.isSynchronized( currentPatchInfoStore, patchInfoStore );

        assertTrue("System should be synchronized", systemSync );
    }

    public void testSystemIsNotSynchronized( ) throws MigrationException {
        PatchInfoStore patchInfoStore = MockBuilder.getPatchInfoStore(4);
        PatchInfoStore currentPatchInfoStore = MockBuilder.getPatchInfoStore(3);;


        boolean systemSync = migrationRunnerStrategy.isSynchronized( currentPatchInfoStore, patchInfoStore );

        assertFalse("System shouldn't be synchronized", systemSync);
    }

    public void testShouldMigrationThrowIllegalArgumentExceptionIfPatchInfoStoreParametersAreNullWhenIsSync( ) throws MigrationException {

        try{
            migrationRunnerStrategy.isSynchronized( null, null );
            fail("If arguments are null an Illegal Argument Exception should have been thrown");
        }catch(IllegalArgumentException exception ){

        }

    }

}

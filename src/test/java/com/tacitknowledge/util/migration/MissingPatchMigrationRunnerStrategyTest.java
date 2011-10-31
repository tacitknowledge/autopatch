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
import org.easymock.MockControl;

import java.util.HashSet;
import java.util.Set;

/**
 * Test the {@link MissingPatchMigrationRunnerStrategy} class.
 *
 * @author Hemri Herrera (hemri@tacitknowledge.com)
 */
public class MissingPatchMigrationRunnerStrategyTest extends TestCase{

    private MissingPatchMigrationRunnerStrategy strategy;
    private MockControl patchInfoStoreControl;
    private PatchInfoStore patchInfoStore;

    protected void setUp() throws Exception {
        strategy = new MissingPatchMigrationRunnerStrategy( );
        patchInfoStoreControl = MockControl.createControl(PatchInfoStore.class);
        patchInfoStore = (PatchInfoStore) patchInfoStoreControl.getMock();
    }

    public void testShouldMigrationRunReturnsTrueIfPatchWasNotApplied( ) throws MigrationException {
        patchInfoStoreControl.expectAndReturn(patchInfoStore.isPatchApplied(5),false);
        patchInfoStoreControl.replay();
        boolean actualResult = strategy.shouldMigrationRun( 5, patchInfoStore );
        assertTrue( "The patch was already applied.", actualResult);
    }

    public void testShouldMigrationRunReturnsFalseIfPatchWasApplied( ) throws MigrationException {
        patchInfoStoreControl.expectAndReturn(patchInfoStore.isPatchApplied(4),true);
        patchInfoStoreControl.replay();
        boolean actualResult = strategy.shouldMigrationRun( 4, patchInfoStore );
        assertFalse( "The patch was not applied.", actualResult);
    }

    public void testShouldMigrationThrowIllegalArgumentExceptionIfPatchInfoStoreParameterIsNull() throws MigrationException {

        try {

            strategy.shouldMigrationRun(3, null);
            fail("If parameter Is null an Illegal Argument Exception should have been thrown");

        } catch (IllegalArgumentException exception) {

        }

    }

    public void testSystemIsSynchronized( ) throws MigrationException {
        Set<Integer> patchInfoStorePatches = new HashSet<Integer>();
        patchInfoStorePatches.add(1);
        PatchInfoStore patchInfoStore = MockBuilder.getPatchInfoStore(3, patchInfoStorePatches);

        Set<Integer> currentPatchInfoStorePatches = new HashSet<Integer>();
        currentPatchInfoStorePatches.add(1);
        PatchInfoStore currentPatchInfoStore = MockBuilder.getPatchInfoStore(3, currentPatchInfoStorePatches);
        boolean systemSync = strategy.isSynchronized( currentPatchInfoStore, patchInfoStore );
        assertTrue("System should be synchronized", systemSync );
    }


    public void testSystemIsNotSynchronized( ) throws MigrationException {
        Set<Integer> patchInfoStorePatches = new HashSet<Integer>();
        patchInfoStorePatches.add(12);
        PatchInfoStore patchInfoStore = MockBuilder.getPatchInfoStore(3, patchInfoStorePatches);

        Set<Integer> currentPatchInfoStorePatches = new HashSet<Integer>();
        currentPatchInfoStorePatches.add(1);
        PatchInfoStore currentPatchInfoStore = MockBuilder.getPatchInfoStore(3, currentPatchInfoStorePatches);
        boolean systemSync = strategy.isSynchronized( currentPatchInfoStore, patchInfoStore );
        assertFalse("System shouldn't be synchronized", systemSync );
    }

    public void testShouldMigrationThrowIllegalArgumentExceptionIfPatchInfoStoreParametersAreNullWhenIsSync( ) throws MigrationException {

        try{
            strategy.isSynchronized( null, null );
            fail("If arguments are null an Illegal Argument Exception should have been thrown");
        }catch(IllegalArgumentException exception ){

        }

    }
}

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

package com.tacitknowledge.util.migration.builders;

import com.sun.xml.internal.xsom.impl.parser.Patch;
import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.MigrationProcess;
import com.tacitknowledge.util.migration.MigrationRunnerFactory;
import com.tacitknowledge.util.migration.MigrationRunnerStrategy;
import com.tacitknowledge.util.migration.PatchInfoStore;
import org.easymock.MockControl;

/**
 *
 * MockBuilder to retrieve simple objects used in different tests
 *
 * @author Oscar Gonzalez (oscar@tacitknowledge.com)
 */
public class MockBuilder
{

    public  static PatchInfoStore getPatchInfoStore(int patchLevel) throws MigrationException
    {
        MockControl patchInfoStoreControl = MockControl.createStrictControl(PatchInfoStore.class);
        PatchInfoStore patchInfoStoreMock = (PatchInfoStore) patchInfoStoreControl.getMock();
        patchInfoStoreMock.getPatchLevel();
        patchInfoStoreControl.setReturnValue(patchLevel);
        patchInfoStoreControl.replay();
        return patchInfoStoreMock;
    }

    public  MigrationRunnerStrategy getMigrationStrategy()
    {
        MigrationRunnerStrategy migrationRunnerStrategy = new MigrationRunnerStrategy()
        {
            public boolean shouldMigrationRun(int migrationLevel, int currentLevel)
            {
                return migrationLevel > currentLevel;
            }
        };
       return migrationRunnerStrategy;
    }

}

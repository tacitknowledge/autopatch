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
 *
 * $Id$
 */
package com.tacitknowledge.util.migration.tasks.instantiation;

import com.tacitknowledge.util.migration.tasks.BaseTestMigrationTask;

/**
 * 
 * 
 * @author Mike Hardy <mailto:mike@tacitknowledge.com/>
 */
public class TestMigrationTaskInstantiationException extends BaseTestMigrationTask
{
    /**
     * Constructor throws an exception - this task never works
     * 
     * @exception RuntimeException when instantiated
     */
    public TestMigrationTaskInstantiationException()
    {
        super("TestMigrationTaskInstantiationException", 1);
        throw new RuntimeException("This class always throws exceptions when instantiated");
    }
}

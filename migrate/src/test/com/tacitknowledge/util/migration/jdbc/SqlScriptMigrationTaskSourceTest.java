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
 */
package com.tacitknowledge.util.migration.jdbc;

import java.util.List;

import junit.framework.TestCase;

import com.tacitknowledge.util.migration.MigrationException;

/**
 * Exercise the SqlScriptMigrationTaskSource
 * 
 * @author Mike Hardy <mailto:mike@tacitknowledge.com/>
 */
public class SqlScriptMigrationTaskSourceTest extends TestCase
{
    /**
     * Test loading up all the scripts in our test package
     */
    public void testScriptLoad()
    {
        SqlScriptMigrationTaskSource source = new SqlScriptMigrationTaskSource();
        List tasks = null;
        try
        {
            tasks = source.getMigrationTasks(this.getClass().getPackage().getName());
        }
        catch (MigrationException me)
        {
            fail("There shouldn't have been a problem loading the tasks");
        }
        
        // There are 3 scripts in our package of scripts, make sure they are all here
        assertEquals(3, tasks.size());
    }
}

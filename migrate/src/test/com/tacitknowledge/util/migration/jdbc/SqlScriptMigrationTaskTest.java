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

import java.io.IOException;
import java.io.InputStream;
import java.util.List;

import junit.framework.TestCase;

/**
 * Tests the <code>SqlScriptMigrationTask</code>.
 *
 * @author Scott Askew (scott@tacitknowledge.com)
 */
public class SqlScriptMigrationTaskTest extends TestCase
{
    /**
     * The task to test.
     */
    private SqlScriptMigrationTask task = null;

    /**
     * Ensures that the task can correctly parse multiple SQL statements from a single file,
     * with embedded comments. 
     * 
     * @throws IOException if an unexpected error occurs while attempting to read the 
     *         test SQL patch file; it's a system resource, so this shouldn't happen
     */
    public void testParsingMultipleStatement() throws IOException
    {
        InputStream is = getClass().getResourceAsStream("patch0001.sql");
        task = new SqlScriptMigrationTask("test", 1, is);
        is.close();
        
        List l = task.getSqlStatements(false);
        assertEquals(3, l.size());
        assertEquals("insert into user_role_assoc (user_role_id, application_user_id, "
            + "role_code, project_id) \n\t\t\tvalues (nextval('role_id_seq'),2, 'SYSA', 3)",
            l.get(0).toString());
        assertEquals("insert into user_role_assoc (user_role_id, application_user_id, "
            + "role_code, project_id) \n\t\t\tvalues (nextval('role_id_seq'),3, 'SYSA', 3)",
            l.get(1).toString());
        assertEquals("insert into user_role_assoc (user_role_id, application_user_id, "
            + "role_code, project_id) \n\t\t\tvalues (nextval('role_--id_seq'),4, 'SYSA', 3)",
            l.get(2).toString());

        is = getClass().getResourceAsStream("patch0002.sql");
        task = new SqlScriptMigrationTask("test", 1, is);
        is.close();
        
        l = task.getSqlStatements(false);
        assertEquals(1, l.size());
        assertEquals("insert into user_role_assoc (user_role_id, application_user_id, "
            + "role_code, project_id) \n\t\t\tvalues (nextval('role_id_seq'),2, 'SYSA', 3)",
            l.get(0).toString());
    }
}

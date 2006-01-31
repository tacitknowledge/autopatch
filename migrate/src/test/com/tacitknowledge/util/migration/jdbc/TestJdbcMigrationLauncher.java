/* Copyright 2006 Tacit Knowledge LLC
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

import java.sql.Connection;

import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.PatchInfoStore;

/**
 * Override select things in the JdbcMigrationLauncher for testing purposes
 * 
 * @author Mike Hardy (mike@tacitknowledge.com)
 */
public class TestJdbcMigrationLauncher extends JdbcMigrationLauncher
{
    /** The PatchInfoStore to use for migrations */
    private PatchInfoStore patchStore = null;
    
    /**
     * Delegates to the superclass
     */
    public TestJdbcMigrationLauncher()
    {
        super();
    }
    
    /**
     * Delegating constructors
     * 
     * @param context the context to use for migration loading
     * @exception MigrationException if there is a problem configuring the context
     */
    public TestJdbcMigrationLauncher(JdbcMigrationContext context) throws MigrationException
    {
        super(context);
    }
    
    /**
     * Override the patch store creation to be the patch table we have
     * 
     * @param conn the database connection to use for patch table access
     * @return patchStore held internally
     */
    protected PatchInfoStore createPatchStore(Connection conn)
    {
        if (patchStore != null)
        {
            return patchStore;
        }
        
        return super.createPatchStore(conn);
    }
    
    /**
     * Set the PatchInfoStore object to use for migrations
     * 
     * @param patchStore the PatchInfoStore to use
     */
    public void setPatchStore(PatchInfoStore patchStore)
    {
        this.patchStore = patchStore;
    }
}

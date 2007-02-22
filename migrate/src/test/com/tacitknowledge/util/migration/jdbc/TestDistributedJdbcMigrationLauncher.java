/* Copyright 2007 Tacit Knowledge LLC
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

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.MigrationListener;
import com.tacitknowledge.util.migration.MigrationTask;
import com.tacitknowledge.util.migration.PatchInfoStore;

/**
 * Override select things in the JdbcMigrationLauncher for testing purposes
 * 
 * @author Mike Hardy (mike@tacitknowledge.com)
 */
public class TestDistributedJdbcMigrationLauncher extends DistributedJdbcMigrationLauncher
{
    /** Class logger */
    private static Log log = LogFactory.getLog(TestDistributedJdbcMigrationLauncher.class);
    
    /** The PatchInfoStore to use for migrations */
    private PatchInfoStore patchStore = null;
    
    /**
     * Delegates to the superclass
     */
    public TestDistributedJdbcMigrationLauncher()
    {
        super();
    }
    
    /**
     * Delegating constructors
     * 
     * @param context the context to use for migration loading
     */
    public TestDistributedJdbcMigrationLauncher(JdbcMigrationContext context)
    {
        super(context);
    }
    
    /**
     * Override the patch store creation to be the patch table we have
     * 
     * @return patchStore held internally
     * @throws MigrationException if creating the store fails
     */
    protected PatchInfoStore createPatchStore() throws MigrationException
    {
        if (patchStore != null)
        {
            return patchStore;
        }
        
        return super.createPatchStore();
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

    /**
     * @see MigrationListener#migrationSuccessful(MigrationTask, MigrationContext)
     */
    public void migrationSuccessful(MigrationTask task, MigrationContext ctx)
        throws MigrationException
    {
        log.debug(this + " silently ignoring a migrationSuccessful call");
    }
}

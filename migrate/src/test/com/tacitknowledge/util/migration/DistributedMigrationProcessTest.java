package com.tacitknowledge.util.migration;

import java.util.HashMap;
import java.util.LinkedHashMap;

import org.easymock.MockControl;

import com.tacitknowledge.util.migration.jdbc.JdbcMigrationContext;
import com.tacitknowledge.util.migration.jdbc.JdbcMigrationLauncher;

import junit.framework.TestCase;

/**
 * Test the {@link DistributedMigrationProcess} class.
 * @author Alex Soto <alex@tacitknowledge.com>
 * @author Alex Soto <apsoto@gmail.com>
 *
 */
public class DistributedMigrationProcessTest extends TestCase
{
    /** class under test */
    private DistributedMigrationProcess migrationProcess = null;
    
    
    /**
     * Setup our tests.
     */
    protected void setUp() throws Exception
    {
        super.setUp();
        migrationProcess = new DistributedMigrationProcess();
    }
    
    protected HashMap createSystems()
    {
        HashMap systems = new HashMap();
        String systemNames[] = {"system1", "system2"};
        
        for(int i = 0; i < systemNames.length; i++)
        {
            //JDBCMi
            
        }
        
        return systems;
    }
    
    public void testValidateControlledSystemsWhenNodePatchLevelsAreInSync() throws Exception
    {
        // the patch level the systems is at for test purposes.
        int currentPatchLevel = 4;
        
        // system has one node
        String systemName = "system1";
        JdbcMigrationLauncher launcher = new JdbcMigrationLauncher();
        
        MockControl contextControl = MockControl.createControl(JdbcMigrationContext.class);
        
        JdbcMigrationContext context = (JdbcMigrationContext) contextControl.getMock();
        MockControl patchInfoStoreControl = MockControl.createControl(PatchInfoStore.class);
        PatchInfoStore patchInfoStore = (PatchInfoStore) patchInfoStoreControl.getMock();
        // setup mock patch info store to return the patch level we want
        patchInfoStore.getPatchLevel();
        patchInfoStoreControl.setReturnValue(currentPatchLevel);
        patchInfoStoreControl.replay();

        // create the launcher's contexts collection
        LinkedHashMap contexts = new LinkedHashMap();
        contexts.put(context, patchInfoStore);
        launcher.setContexts(contexts);
        
        HashMap controlledSystems = new HashMap();
        controlledSystems.put(systemName, launcher);
        
        migrationProcess.setControlledSystems(controlledSystems);
        
        try
        {
            migrationProcess.validateControlledSystems(currentPatchLevel);
        }
        catch(Exception e)
        {
            fail("Unexpected exception when validating controlled systems.");
        }
    }
    
    public void testValidateControlledSystemsWhenNodePatchLevelsAreOutOfSync() throws Exception
    {
        // the patch level the systems is at for test purposes.
        int currentPatchLevel = 4;
        
        // system has one node
        String systemName = "system1";
        JdbcMigrationLauncher launcher = new JdbcMigrationLauncher();
        
        // first node is at the 'current' patch level
        MockControl node1ContextControl = MockControl.createControl(JdbcMigrationContext.class);
        JdbcMigrationContext node1Context = (JdbcMigrationContext) node1ContextControl.getMock();
        MockControl node1PatchInfoStoreControl = MockControl.createControl(PatchInfoStore.class);
        PatchInfoStore node1PatchInfoStore = (PatchInfoStore) node1PatchInfoStoreControl.getMock();
        // setup mock patch info store to return the patch level we want
        node1Context.getDatabaseName();
        node1ContextControl.setReturnValue("node1", MockControl.ONE_OR_MORE);
        node1ContextControl.replay();
        node1PatchInfoStore.getPatchLevel();
        node1PatchInfoStoreControl.setReturnValue(currentPatchLevel);
        node1PatchInfoStoreControl.replay();
        
        // second node simulates a newly added database instance, it has not been patched
        MockControl node2ContextControl = MockControl.createControl(JdbcMigrationContext.class);
        JdbcMigrationContext node2Context = (JdbcMigrationContext) node2ContextControl.getMock();
        MockControl node2PatchInfoStoreControl = MockControl.createControl(PatchInfoStore.class);
        PatchInfoStore node2PatchInfoStore = (PatchInfoStore) node2PatchInfoStoreControl.getMock();
        // setup mock patch info store to return the patch level we want
        node2Context.getDatabaseName();
        node2ContextControl.setReturnValue("node2", MockControl.ONE_OR_MORE);
        node2ContextControl.replay();
        node2PatchInfoStore.getPatchLevel();
        node2PatchInfoStoreControl.setReturnValue(0);
        node2PatchInfoStoreControl.replay();

        // create the launcher's contexts collection
        LinkedHashMap contexts = new LinkedHashMap();
        contexts.put(node1Context, node1PatchInfoStore);
        contexts.put(node2Context, node2PatchInfoStore);
        launcher.setContexts(contexts);
        
        HashMap controlledSystems = new HashMap();
        controlledSystems.put(systemName, launcher);
        
        migrationProcess.setControlledSystems(controlledSystems);
        try
        {
            migrationProcess.validateControlledSystems(currentPatchLevel);
            fail("Unexpected exception when validating controlled systems.");
        }
        catch(MigrationException me)
        {
        }
        catch(Exception e)
        {
            fail("Unexpected exception when validating controlled systems.");
        }
    }

}

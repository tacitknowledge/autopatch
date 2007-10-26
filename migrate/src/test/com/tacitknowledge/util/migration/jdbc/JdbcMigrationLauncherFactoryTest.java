package com.tacitknowledge.util.migration.jdbc;

import java.io.IOException;
import java.util.List;
import java.util.Properties;

import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.MigrationListener;

import junit.framework.TestCase;

public class JdbcMigrationLauncherFactoryTest extends TestCase
{
    /** class under test */
    JdbcMigrationLauncherFactory factory = null;
    
    protected void setUp() throws Exception
    {
        super.setUp();
        factory = new DistributedJdbcMigrationLauncherFactory();
    }
    
    public void testConfigureMigrationListeners() throws IOException
    {
        String systemName = "test-system";
        Properties properties = new Properties();
        properties.put(systemName + ".listeners", "com.tacitknowledge.util.migration.test.listeners.TestListener1");
        
        factory = new JdbcMigrationLauncherFactory();
        
        List listeners = null;
        try
        {
            listeners = factory.loadMigrationListeners(systemName, properties);
            assertNotNull(listeners);
            assertEquals(1, listeners.size());
            assertTrue(listeners.get(0) instanceof MigrationListener);
        } catch (MigrationException e)
        {
            fail("Unexpected exception while loading Migration Listeners for " + systemName);
        }
    }

    public void testConfigureMigrationListenersDetectsInvalidListeners()
    {
        String systemName = "test-system";
        Properties properties = new Properties();
        properties.put(systemName + ".listeners", "java.lang.String");
        
        factory = new JdbcMigrationLauncherFactory();
        
        try
        {
            factory.loadMigrationListeners(systemName, properties);
            fail("An exception reporting an invalid listener class should have occurred");
        } catch (MigrationException e)
        {
            // expected
            assertTrue(e.getCause() instanceof ClassCastException);
        }
    }
    
    public void testConfigureMigrationListenersReportsIfItCannotFindListeners()
    {
        String systemName = "test-system";
        Properties properties = new Properties();
        properties.put(systemName + ".listeners", "com.foo.listener");
        
        factory = new JdbcMigrationLauncherFactory();
        
        try
        {
            factory.loadMigrationListeners(systemName, properties);
            fail("An exception reporting an invalid listener class should have occurred");
        } catch (MigrationException e)
        {
            // expected
            assertTrue(e.getCause() instanceof ClassNotFoundException);
        }
    }
    
    public void testConfigureMigrationListenersLoadsMultipleListeners()
    {
        String systemName = "test-system";
        Properties properties = new Properties();
        // notice the spaces and the commas.  I want to make sure it can parse classnames out correctly regardless of whitespace, etc
        properties.put(systemName + ".listeners", " ,com.tacitknowledge.util.migration.test.listeners.TestListener1, com.tacitknowledge.util.migration.test.listeners.TestListener2 , ");
        
        factory = new JdbcMigrationLauncherFactory();
        
        List listeners = null;
        try
        {
            listeners = factory.loadMigrationListeners(systemName, properties);
            assertNotNull(listeners);
            assertEquals(2, listeners.size());
            assertTrue(listeners.get(0) instanceof MigrationListener);
            assertTrue(listeners.get(1) instanceof MigrationListener);
        } catch (MigrationException e)
        {
            fail("Unexpected exception while loading Migration Listeners for " + systemName);
        }
    }
    
    public void testConfigureMigrationListenersCanHandleNoConfiguredListeners()
    {
        String systemName = "test-system";
        Properties properties = new Properties();
        factory = new JdbcMigrationLauncherFactory();
        
        List listeners = null;
        try
        {
            listeners = factory.loadMigrationListeners(systemName, properties);
            assertNotNull(listeners);
            assertEquals(0, listeners.size());
        } catch (MigrationException e)
        {
            fail("Unexpected exception while loading Migration Listeners for " + systemName);
        }
    }
    
    public void testConfigureMigrationListenersCanHandleNullParameters()
    {
        factory = new JdbcMigrationLauncherFactory();
        
        try
        {
            factory.loadMigrationListeners(null, null);
            fail("An exception was expected because the parameters are null");
        } catch (MigrationException e)
        {
            // expected
        }
        
    }

}

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
package com.tacitknowledge.util.migration;

import junit.framework.TestCase;

/**
 * Exercise the MigrationBroadcaster
 * 
 * @author Mike Hardy <mailto:mike@tacitknowledge.com/>
 */
public class MigrationBroadcasterTest extends TestCase implements MigrationListener
{
    /** whether we've been started */
    private boolean started = false;
    
    /** whether we've succeeded */
    private boolean succeeded = false;
    
    /** whether we've failed */
    private boolean failed = false;

    /** The broadcaster being tested */
    MigrationBroadcaster broadcaster = null;
    
    /**
     * Set up our test state
     */
    public void setUp()
    {
        broadcaster = new MigrationBroadcaster();
        broadcaster.addListener(this);
    }
    
    /**
     * Test how the broadcaster deals with null listeners
     */
    public void testNullListeners()
    {
        try
        {
            broadcaster.addListener(null);
            fail("We should have failed - adding null listeners is illegal");
        }
        catch (IllegalArgumentException iae)
        {
            // we expect this
        }
        
        try
        {
            broadcaster.removeListener(null);
            fail("We should have failed - removing null listeners is illegal");
        }
        catch (IllegalArgumentException iae)
        {
            // we expect this
        }
    }
    
    /**
     * Test add/remove listeners
     */
    public void testAddRemoveListeners()
    {
        // we're already there (from setUp), so try removing us
        assertTrue(broadcaster.removeListener(this));
        
        // now we're not there, make sure it fails
        assertFalse(broadcaster.removeListener(this));
    }
    
    /**
     * Test broadcast of the basic events
     */
    public void testBroadcast() throws MigrationException
    {
        assertFalse(started);
        broadcaster.notifyListeners(null, null, MigrationBroadcaster.TASK_START);
        assertTrue(started);
        assertFalse(succeeded);
        broadcaster.notifyListeners(null, null, MigrationBroadcaster.TASK_SUCCESS);
        assertTrue(succeeded);
        assertFalse(failed);
        broadcaster.notifyListeners(null, null, MigrationBroadcaster.TASK_FAILED);
        assertTrue(failed);
        try
        {
            broadcaster.notifyListeners(null, null, Integer.MAX_VALUE);
            fail("We threw an unknown event - it should have failed");
        }
        catch (IllegalArgumentException iae)
        {
            // we expect this
        }
    }
    
    /**
     * Implement the hook for start messages
     */
    public void migrationStarted(MigrationTask task, MigrationContext context)
    {
        started = true;
    }

    /**
     * Implement the hook for success
     */
    public void migrationSuccessful(MigrationTask task, MigrationContext context)
    {
        succeeded = true;
    }

    /**
     * Implement the hook for failure
     */
    public void migrationFailed(MigrationTask task, 
                                MigrationContext context,
                                MigrationException e)
    {
        failed = true;
    }
}

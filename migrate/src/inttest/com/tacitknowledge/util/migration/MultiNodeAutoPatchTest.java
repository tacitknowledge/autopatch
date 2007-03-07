/* 
 * Copyright 2007 Tacit Knowledge LLC
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

package com.tacitknowledge.util.migration;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

/**
 * Test AutoPatch MultiNode functionality
 * 
 * @author  Mike Hardy (mike@tacitknowledge.com)
 */
public class MultiNodeAutoPatchTest extends AutoPatchIntegrationTestBase
{
    /** Class logger */
    private static Log log = LogFactory.getLog(MultiNodeAutoPatchTest.class);
    
    /**
     * Constructor 
     * 
     * @param name the name of the test to run
     */
    public MultiNodeAutoPatchTest(String name)
    {
        super(name);
    }
    
    /**
     * Test that all the tables were created successfully in all of the databases
     * 
     * @exception Exception if anything goes wrong
     */
    public void testMultiNodePatch() throws Exception
    {
        log.debug("Testing multi node patching");
        try
        {
            launcher.doMigrations();
        }
        catch (Exception e)
        {
            log.error("Unexpected error", e);
            fail("shouldn't have thrown any exceptions");
        }
    }
}

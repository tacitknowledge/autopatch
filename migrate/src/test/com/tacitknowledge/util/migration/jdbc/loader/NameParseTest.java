/* Copyright (c) 2004 Tacit Knowledge LLC  
 * See licensing terms below.
 * 
 * THIS SOFTWARE IS PROVIDED "AS IS" AND ANY  EXPRESSED OR IMPLIED 
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
 * IN NO EVENT SHALL TACIT KNOWLEDGE LLC OR ITS CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
 * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
 * OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  THIS HEADER MUST
 * BE INCLUDED IN ANY DISTRIBUTIONS OF THIS CODE.
 */
package com.tacitknowledge.util.migration.jdbc.loader;

import junit.framework.TestCase;

/**
 * Test used to verify that the table name is correctly 
 * parsed.
 * 
 * @author Chris A. (chris@tacitknowledge.com)
 * @version $Id$
 */
public class NameParseTest extends TestCase
{
    protected TestLoader loader = null;
    
    /**
     * Constructor that invokes its parent's constructor
     * 
     * @param name the test name
     */
    public NameParseTest(String name)
    {
        super(name);
    }
    
    /**
     * @see junit.framework.TestCase#setUp()
     */
    public void setUp() throws Exception
    {
        super.setUp();
        loader = new TestLoader();
    }
    
    /**
     * Tests several combinations of input names to insure things are 
     * working well.
     */
    public void testNameParsing()
    {
        String longName = loader.pathSeparator + "parent-dir" + loader.pathSeparator
        	+ "child-dir" + loader.pathSeparator + "table_db20040704.load";
        String medName = loader.pathSeparator + "table_db20040704.load";
        String names[] = {"table_db.dat", longName, medName};
        for (int i = 0; i < names.length; i++)
        {
            loader.setName(names[i]);
            String tb = loader.getTableFromName();
            assertTrue("table".equals(tb));
        }
    }
    
    /**
     * Inner class used to test file loader
     */
    private class TestLoader extends DelimitedFileLoader
    {
        private String name = null;
        
        public String getName()
        {
            return name;
        }
        
        public void setName(String name)
        {
            this.name = name;
        }
        
        public String getDelimiter()
        {
            return "|";
        }
    }
}

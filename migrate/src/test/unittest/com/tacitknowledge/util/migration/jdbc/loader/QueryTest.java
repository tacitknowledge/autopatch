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

import java.io.IOException;
import java.io.InputStream;

import junit.framework.TestCase;

/**
 * Test used to verify that the sql query is correctly 
 * created
 * 
 * @author Chris A. (chris@tacitknowledge.com)
 * @version $Id$
 */
public class QueryTest extends TestCase
{
    /**
     * The dummy loader used for testing
     */
    private TestLoader loader = null;
    
    /**
     * Constructor that invokes its parent's constructor
     * 
     * @param name the test name
     */
    public QueryTest(String name)
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
     * Tests query generation
     */
    public void testQuery()
    {
        loader.setName("mocktable_db.dat");
        String sql = loader.getStatmentSql();
        String answer = "INSERT INTO mocktable (col1, col2, col3, col4) "
            + "VALUES (?, ?, ?, ?)";
        assertTrue(answer.equals(sql));
    }
    
    /**
     * Inner class used to test file loader
     */
    private class TestLoader extends DelimitedFileLoader
    {
        /**
         * The name of the file
         */
        private String name = null;

        /**
         * @see com.tacitknowledge.util.migration.MigrationTask#getName()
         */
        public String getName()
        {
            return name;
        }

        /**
         * @see com.tacitknowledge.util.migration.MigrationTaskSupport#setName(java.lang.String)
         */
        public void setName(String name)
        {
            this.name = name;
        }

        /**
         * @see com.tacitknowledge.util.migration.jdbc.loader.DelimitedFileLoader#getDelimiter()
         */
        public String getDelimiter()
        {
            return "|";
        }
        
        /**
         * Returns the header (first line) of the file.
         * 
         * @param  is the input stream containing the data to load
         * @return the first row
         * @throws IOException if the input stream could not be read
         */
        protected String getHeader(InputStream is) throws IOException
        {
            return "col1|col2|col3|col4";
        }
       
        /**
         * @see com.tacitknowledge.util.migration.jdbc.SqlLoadMigrationTask#getResourceAsStream()
         */
        protected InputStream getResourceAsStream()
        {
            return null;
        }
    }
}


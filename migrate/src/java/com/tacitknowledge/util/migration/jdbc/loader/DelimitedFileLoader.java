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

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.sql.PreparedStatement;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.StringTokenizer;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.jdbc.SqlLoadMigrationTask;

/**
 * Loads files assumed to be in a delimited format and representing a 
 * table in the database.  The file name should begin with the prefix:  
 * "<tablename>_tb".  The file's first row should represent the name of 
 * each column in the table that the underlying data elements (rows 2 
 * through n) will be mapped to.  
 * 
 * @author Chris A. (chris@tacitknowledge.com)
 * @version $Id$
 */
public abstract class DelimitedFileLoader extends SqlLoadMigrationTask
{
    /**
     * Class logger
     */
    private static Log log = LogFactory.getLog(DelimitedFileLoader.class);
    
    /**
     * The path separator
     */
    public static final String PATH_SEPARATOR = System.getProperty("file.separator");
    
    /**
     * Private variable that indicates if the header has been parsed or not. 
     */
    private boolean parsedHeader = false;
    
    /**
     * Gets the expected file delimiter.  A pipe-delimited 
     * reader should return "|", for example.
     * 
     * @return the delimiter string used to separate columns
     */
    public abstract String getDelimiter();
    
    /**
     * Should return the name of the file to load.  This will 
     * support either an absolute name, or the name of a file 
     * in the classpath of the application in which this is executed.
     * 
     * @return the absolute path name of the table to load
     */
    public abstract String getName();
    
    /**
     * Parses a line of data, and sets the prepared statement with the 
     * values.  If a token contains "<null>" then a null value is passed 
     * in. 
     * 
     * @param  data the tokenized string that is mapped to a row
     * @param  stmt the statement to populate with data to be inserted
     * @return false if the header is returned, true otherwise
     * @throws SQLException if an error occurs while inserting data into the database
     */
    protected boolean insert(String data, PreparedStatement stmt) throws SQLException
    {
        if (!parsedHeader)
        {
            parsedHeader = true;
            log.info("Header returned: " + data);
            return false;
        }
        StringTokenizer st = new StringTokenizer(data, getDelimiter());
        int counter = 1;
        log.info("Row being parsed: " + data);
        while (st.hasMoreTokens())
        {
            String colVal = st.nextToken();
            if (colVal.equalsIgnoreCase("<null>"))
            {
                stmt.setString(counter, null);
            }
            else
            {
                stmt.setString(counter, colVal);
            }
            counter++;
        }
        return true;
    }
    
    /**
     * Returns the table name from the full path name 
     * by parsing it out of a file in the format
     * name_db<period>(some extension)
     * 
     * @return the name of the table to add data to
     */
    protected String getTableFromName()
    {
        String name = getName();
        int startTable = name.lastIndexOf(PATH_SEPARATOR);
        int endTable = name.indexOf("_db", startTable);
        return name.substring((startTable + 1), endTable);
    }
    
    /**
     * Parses the table name from the file name, and the column names 
     * from the header (first row) of the delimited file.  Creates an 
     * insert query to insert a single row. 
     * 
     * @return query in the format: INSERT INTO tablename (colname, colname)...
     */
    protected String getStatmentSql()
    {
        try
        {
            String columnHeader = getHeader(getResourceAsStream());
            String delimiter = getDelimiter();
            StringTokenizer st = new StringTokenizer(columnHeader, delimiter);
            ArrayList columnNames = new ArrayList();
            while (st.hasMoreTokens())
            {
                columnNames.add((st.nextToken().trim()));
            }
            StringBuffer query = new StringBuffer("INSERT INTO ");
            query.append(getTableFromName());
            query.append(" (");
            Iterator it = columnNames.iterator();
            boolean firstTime = true;
            while (it.hasNext())
            {
                if (!firstTime)
                {
                    query.append(", ");
                }
                else
                {
                    firstTime = false;
                }
                query.append((String) it.next());
            }
            query.append(") VALUES (");
            for (int i = 0; i < columnNames.size(); i++)
            {
                if (i > 0)
                {
                    query.append(", ");
                }
                query.append("?");
            }
            query.append(")");
            return query.toString();
        } 
        catch (IOException e)
        {
            log.error("No header was found for file: " + getName(), e);
        }
        return null;
    }
    
    /**
     * Gets an input stream by first checking the current classloader, 
     * then trying to use the system classloader, and finally, trying 
     * to access the file on the file system.  If the file is not found, 
     * an <code>IllegalArgumentException</code> will be thrown.
     * 
     * @return the file as an input stream
     */
    protected InputStream getResourceAsStream()
    {
        FileLoadingUtility utility = new FileLoadingUtility(getName());
        return utility.getResourceAsStream();
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
        List data = new ArrayList();
        BufferedReader reader = new BufferedReader(new InputStreamReader(is));
        return reader.readLine();   
    }
}

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

package com.tacitknowledge.util.migration.jdbc;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.sql.Connection;
import java.sql.Statement;
import java.sql.SQLException;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.MigrationTaskSupport;

/**
 *
 * <p>
 * Multiple statements can be provided, separated by semicolons. Individual
 * lines within the statements can be commented using either --, // or REM at
 * the start of the line.
 *  
 * @author  Scott Askew (scott@tacitknowledge.com)
 * @version $Id$
 */
public class SqlScriptMigrationTask extends MigrationTaskSupport
{
    /**
     * Class logger
     */
    private static Log log = LogFactory.getLog(SqlScriptMigrationTask.class);
    
    /**
     * The SQL to execute
     */
    private String sql = null;
    
    /**
     * Creates a new <code>SqlScriptMigrationTask</code>.
     */
    public SqlScriptMigrationTask()
    {
        // Nothing to do
    }
    
    /**
     * Creates a new <code>SqlScriptMigrationTask</code>
     *
     * @param  name the name of the SQL script to execute; this is just an
     *         identifier and does not have to correspond to a file name 
     * @param sql the SQL to execute
     * @param level the patch level of the migration tas 
     */ 
    public SqlScriptMigrationTask(String name, int level, String sql)
    {
        setName(name);
        setLevel(new Integer(level));
        this.sql = sql;
    }
    
    /**
     * Creates a new <code>SqlScriptMigrationTask</code> containing the SQL
     * contained in the given <code>InputStream</code>.
     *
     * @param  name the name of the SQL script to execute; this is just an
     *         identifier and does not have to correspond to a file name 
     * @param  level the patch level of the migration tas 
     * @param  is the source of the SQL to execute
     * @throws IOException if there was problem reading the input stream
     */
    public SqlScriptMigrationTask(String name, int level, InputStream is) throws IOException
    {
        setName(name);
        setLevel(new Integer(level));
        StringBuffer sqlBuffer = new StringBuffer();
        BufferedReader buf = new BufferedReader(new InputStreamReader(is));
        String line = buf.readLine();
        while (line != null)
        {
            sqlBuffer.append(line).append("\n");
            line = buf.readLine();
        }
        sql = sqlBuffer.toString();
    }

    /**
     * @see MigrationTaskSupport#migrate(MigrationContext)
     */
    public void migrate(MigrationContext ctx) throws MigrationException
    {
        JdbcMigrationContext context = (JdbcMigrationContext) ctx;
        Connection conn = context.getConnection();
        Statement stmt = null;
        
        try
        {
            if (log.isDebugEnabled())
            {
                log.debug(getName() + ": Attempting to execute: " + sql);
            }
            stmt = conn.createStatement();
            stmt.execute(sql);
        }
        catch (Exception e)
        {
            log.error(getName() + ": Error running SQL \"" + sql + "\"", e);
            
            if (e instanceof SQLException)
            {
                if (((SQLException)e).getNextException() != null)
                {
                    log.error("Chained SQL Exception", ((SQLException)e).getNextException());
                }
            }
            
            throw new MigrationException(e);
        }
        finally
        {
            SqlUtil.close(null, stmt, null);
        }
    }
    
    /**
     * @see java.lang.Object#toString()
     */
    public String toString()
    {
        return getName();
    }
}

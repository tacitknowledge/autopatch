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

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.sql.Connection;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.MigrationTaskSupport;
import com.tacitknowledge.util.migration.jdbc.util.SqlUtil;

/**
 * Adaptss a SQL or DDL database patch for use with the AutoPatch framework.  
 *  
 * @author  Scott Askew (scott@tacitknowledge.com)
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
        // do nothing
    }
    
    /**
     * Creates a new <code>SqlScriptMigrationTask</code>
     *
     * @param name the name of the SQL script to execute; this is just an
     *        identifier and does not have to correspond to a file name 
     * @param level the patch level of the migration task
     * @param sql the SQL to execute
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
     * @param  level the patch level of the migration task
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
        
        Connection conn = null;
        Statement stmt = null;
        String sqlStatement = "";
        try
        {
            conn = context.getConnection();
            List sqlStatements = getSqlStatements(context);
            for (Iterator i = sqlStatements.iterator(); i.hasNext();)
            {
                sqlStatement = (String) i.next();
                if (log.isDebugEnabled())
                {
                    log.debug(getName() + ": Attempting to execute: " + sqlStatement);
                }
                stmt = conn.createStatement();
                stmt.execute(sqlStatement);
                SqlUtil.close(null, stmt, null);
                stmt = null;
            }
        }
        catch (Exception e)
        {
            String message = getName() + ": Error running SQL \"" + sqlStatement + "\"";
            log.error(message, e);
            
            if (e instanceof SQLException)
            {
                if (((SQLException) e).getNextException() != null)
                {
                    log.error("Chained SQL Exception", ((SQLException) e).getNextException());
                }
            }
            
            throw new MigrationException(message, e);
        }
        finally
        {
            SqlUtil.close(null, stmt, null);
        }
    }
    
    /**
     * Parses the SQL/DDL to execute and returns a list of individual statements.  For database
     * types that support mulitple statements in a single <code>Statement.execute</code> call,
     * this method will return a one-element <code>List</code> containing the entire SQL
     * file.
     * 
     * @param  context the MigrationContext, to figure out db type and if it 
     *                 can handle multiple statements at once
     * @return a list of SQL and DDL statements to execute
     */
    public List getSqlStatements(JdbcMigrationContext context)
    {
        List statements = new ArrayList();
        if (context.getDatabaseType().isMultipleStatementsSupported())
        {
            statements.add(sql);
            return statements;
        }
        
        StringBuffer currentStatement = new StringBuffer();
        boolean inQuotedString = false;
        boolean inComment = false;
        char[] sqlChars = sql.toCharArray();
        for (int i = 0; i < sqlChars.length; i++)
        {
            if (sqlChars[i] == '\n')
            {
                inComment = false;
            }
            
            if (!inComment)
            {
                switch (sqlChars[i])
                {
                    case '-' :
                    case '/' :
                        if (!inQuotedString && i + 1 < sqlChars.length
                            && sqlChars[i + 1] == sqlChars[i])
                        {
                            inComment = true;
                        }
                        else
                        {
                            currentStatement.append(sqlChars[i]);
                        }
                        break;
                    case '\'' :
                        inQuotedString = !inQuotedString;
                        currentStatement.append(sqlChars[i]);
                        break;
                    case ';' :
                        if (!inQuotedString)
                        {
                            // If we're in a stored procedure, just keep rolling
                            if (context.getDatabaseType().getDatabaseType().equals("oracle") &&
                                    (currentStatement.toString().trim()
                                            .toLowerCase().startsWith("begin") ||
                                     currentStatement.toString().trim()
                                        .toLowerCase().startsWith("create or replace method") ||
                                     currentStatement.toString().trim()
                                        .toLowerCase().startsWith("create or replace function") ||
                                     currentStatement.toString().trim()
                                        .toLowerCase().startsWith("create or replace procedure") ||
                                     currentStatement.toString().toString()
                                        .toLowerCase().startsWith("create or replace package"))) {
                                currentStatement.append(sqlChars[i]);
                            }
                            else 
                            {
                                statements.add(currentStatement.toString().trim());
                                currentStatement = new StringBuffer();
                            }
                        }
                        else
                        {
                            currentStatement.append(sqlChars[i]);
                        }
                        break;
                    default :
                        currentStatement.append(sqlChars[i]);
                        break;
                }
            }
        }
        if (currentStatement.toString().trim().length() > 0)
        {
            statements.add(currentStatement.toString().trim());
        }
        
        return statements;
    }

    /**
     * @see java.lang.Object#toString()
     */
    public String toString()
    {
        return getName();
    }
}

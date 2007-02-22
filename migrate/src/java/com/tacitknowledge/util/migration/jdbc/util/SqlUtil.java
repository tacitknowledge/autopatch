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

package com.tacitknowledge.util.migration.jdbc.util;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

/**
 * Utility class for dealing with JDBC.
 * 
 * @author  Scott Askew (scott@tacitknowledge.com)
 */
public final class SqlUtil
{
    /** Class logger */
    private static Log log = LogFactory.getLog(SqlUtil.class);
    
    /** Hidden constructor for utility class */
    private SqlUtil()
    {
        // Hidden
    }

    /**
     * Ensures the given connection, statement, and result are properly closed.
     * 
     * @param conn the connection to close; may be <code>null</code>
     * @param stmt the statement to close; may be <code>null</code>
     * @param rs the result set to close; may be <code>null</code>
     */
    public static void close(Connection conn, Statement stmt, ResultSet rs)
    {
        if (rs != null)
        {
            try 
            {
            	log.debug("Closing resultset: " + rs.toString());
                rs.close();
            }
            catch (SQLException e)
            {
                log.error("Error closing ResultSet", e);
            }
        }

        if (stmt != null)
        {
            try 
            {
            	log.debug("Closing statement: " + stmt.toString());
                stmt.close();
            }
            catch (SQLException e)
            {
                log.error("Error closing Statment", e);
            }
        }

        if (conn != null)
        {
            try 
            {
                if (!conn.isClosed())
                {
                    log.debug("Closing connection " + conn.toString());
                    conn.close();
                }
            }
            catch (SQLException e)
            {
                log.error("Error closing Connection", e);
            }
        }
    }
    
    /**
     * Established and returns a connection based on the specified parameters. 
     * 
     * @param  driver the JDBC driver to use
     * @param  url the database URL
     * @param  user the username
     * @param  pass the password
     * @return a JDBC connection
     * @throws ClassNotFoundException if the driver could not be loaded
     * @throws SQLException if a connnection could not be made to the database
     */
    public static Connection getConnection(String driver, String url, String user, String pass) 
        throws ClassNotFoundException, SQLException
    {
        Class.forName(driver);
        log.debug("Getting connection to " + url);
        return DriverManager.getConnection(url, user, pass);
    }
}

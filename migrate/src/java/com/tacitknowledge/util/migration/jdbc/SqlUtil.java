/* Copyright 2004 Tacit Knowledge LLC
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

import java.sql.Connection;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

/**
 * Utility class for dealing with JDBC.
 * 
 * @author  Scott Askew (scott@tacitknowledge.com)
 * @version $Id$
 */
final class SqlUtil
{
    /**
     * Class logger
     */
    private static Log log = LogFactory.getLog(SqlUtil.class);
    
    /**
     * Hidden constructor for utility class
     */
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
                conn.close();
            }
            catch (SQLException e)
            {
                log.error("Error closing Connection", e);
            }
        }
    }
}

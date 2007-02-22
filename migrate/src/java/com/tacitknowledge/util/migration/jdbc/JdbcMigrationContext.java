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

package com.tacitknowledge.util.migration.jdbc;

import java.sql.Connection;
import java.sql.SQLException;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;

/**
 * Contains the configuration and resources for a database patch run.  
 * 
 * @author Scott Askew (scott@tacitknowledge.com)
 */
public interface JdbcMigrationContext extends MigrationContext
{
    /**
     * Max length for the systemName columne
     */
    public static final int MAX_SYSTEMNAME_LENGTH = 30;

    /**
     * Returns a database connection to use. The creator
     * of the JdbcMigrationContext are responsible for closing the connection.
     * 
     * @return the database connection to use
     * @throws SQLException if an unexpected error occurs
     */
    public Connection getConnection() throws SQLException;

    /**
     * @see MigrationContext#commit()
     */
    public void commit() throws MigrationException;

    /**
     * @see MigrationContext#rollback()
     */
    public void rollback() throws MigrationException;

    /**
     * @return the name of the system to patch
     */
    public String getSystemName();

    /**
     * @return Returns the database type.
     */
    public DatabaseType getDatabaseType();
}
/* 
 * Copyright 2006 Tacit Knowledge LLC
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

import com.tacitknowledge.util.migration.MigrationException;

/**
 * A DataSourceMigrationContext that doesn't actually talk to a database.
 * 
 * @author Mike Hardy (mike@tacitknowledge.com)
 */
public class TestDataSourceMigrationContext extends DataSourceMigrationContext
{
    /**
     * Always returns null, doesn't talk to a database
     * 
     * @return null every time
     * @exception SQLException never throws
     */
    public Connection getConnection() throws SQLException
    {
        return null;
    }
    
    /**
     * Always does nothing
     * 
     * @exception MigrationException never
     */
    public void commit() throws MigrationException
    {
        // do nothing
    }
    
    /**
     * Always does nothing
     * 
     * @exception MigrationException never
     */
    public void rollback() throws MigrationException
    {
        // do nothing
    }
}

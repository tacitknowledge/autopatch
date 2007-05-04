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

package com.tacitknowledge.util.migration.jdbc;

import java.sql.Connection;
import java.sql.SQLException;

import javax.sql.DataSource;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.migration.MigrationException;

/**
 * Provides JDBC resources to migration tasks.
 *
 * @author  Scott Askew (scott@tacitknowledge.com)
 */
public class DataSourceMigrationContext implements JdbcMigrationContext
{
    /** Class logger */
    private static Log log = LogFactory.getLog(JdbcMigrationContext.class);

    /** The database connection to use */
    private Connection connection = null;

    /** The DataSource to use */
    private DataSource dataSource = null;

    /** The name of the system being patched */
    private String systemName = null;

    /** The name of the system being patched */
    private DatabaseType databaseType = null;

    /**
     * Returns the database connection to use
     *
     * @return the database connection to use
     * @throws SQLException if an unexpected error occurs
     */
    public Connection getConnection() throws SQLException
    {
        if ((connection == null) || connection.isClosed())
        {
            DataSource ds = getDataSource();
            if (ds != null)
            {
                connection = ds.getConnection();
            }
            else
            {
                throw new SQLException("Datasource is null");
            }
        }
        return connection;
    }

    /** {@inheritDoc} */
    public void commit() throws MigrationException
    {
        try
        {
            if (getConnection().getAutoCommit())
            {
                log.warn("Attempt to call commit on connection with autoCommit engaged - breaks in JBoss");
                return;
            }
            getConnection().commit();
        }
        catch (SQLException e)
        {
            throw new MigrationException("Error committing SQL transaction", e);
        }
    }

    /** {@inheritDoc} */
    public void rollback() throws MigrationException
    {
        try
        {
            if (getConnection().getAutoCommit())
            {
                log.warn("Attempt to call rollback on connection with autoCommit engaged - breaks in JBoss");
                return;
            }
            getConnection().rollback();
        }
        catch (SQLException e)
        {
            throw new MigrationException("Could not rollback SQL transaction", e);
        }
    }

    /**
     * Returns the type of database being patched.
     *
     * @return the type of database being patched
     */
    public DatabaseType getDatabaseType()
    {
        return databaseType;
    }

    /**
     * Returns the type of database being patched.
     *
     * @param type the type of database being patched
     */
    public void setDatabaseType(DatabaseType type)
    {
        this.databaseType = type;
    }

    /**
     * @return Returns the dataSource.
     */
    public DataSource getDataSource()
    {
        return dataSource;
    }

    /**
     * @param dataSource The dataSource to set.
     */
    public void setDataSource(DataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    /**
     * @return Returns the systemName.
     */
    public String getSystemName()
    {
        return systemName;
    }

    /**
     * Sets the system name.
     *
     * @param name the name of the system to patch
     */
    public void setSystemName(String name)
    {
        if (name == null)
        {
            throw new IllegalArgumentException("systemName cannot be null");
        }
        if (name.length() > MAX_SYSTEMNAME_LENGTH)
        {
            throw new IllegalArgumentException("systemName cannot be longer than "
                + MAX_SYSTEMNAME_LENGTH + " characters");
        }
        this.systemName = name;
    }
    
    /**
     * Useful for debugging
     * 
     * @return String with state information
     */
    public String toString()
    {
        return 
            "DataSourceMigrationContext[" 
            + getDatabaseType() + "/"
            + getSystemName() + "/"
            + getDataSource() + "]";
    }
}

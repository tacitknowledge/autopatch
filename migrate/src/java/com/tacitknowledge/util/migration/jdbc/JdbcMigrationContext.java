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

import java.sql.Connection;
import java.sql.SQLException;

import com.tacitknowledge.util.migration.MigrationContext;
import com.tacitknowledge.util.migration.MigrationException;

/**
 * Provides JDBC resources to migration tasks. 
 * 
 * @author  Scott Askew (scott@tacitknowledge.com)
 * @version $Id$
 */
public class JdbcMigrationContext implements MigrationContext
{
    /**
     * The database connection to use
     */
    private Connection connection = null;
    
    /**
     * Returns the database connection to use
     * 
     * @return the database connection to use
     */
    public Connection getConnection()
    {
        return connection;
    }

    /**
     * Returns the database connection to use
     * 
     * @param connection the database connection to use
     */
    public void setConnection(Connection connection)
    {
        this.connection = connection;
    }
    
    /**
     * @see MigrationContext#commit()
     */
    public void commit() throws MigrationException
    {
        try
        {
            this.connection.commit();
        }
        catch (SQLException e)
        {
            throw new MigrationException("Error committing SQL transaction", e);
        }
    }

    /**
     * @see MigrationContext#rollback()
     */
    public void rollback() throws MigrationException
    {
        try
        {
            getConnection().rollback();
        }
        catch (SQLException e)
        {
            throw new MigrationException("Could not rollback SQL transaction", e);
        }
    }
}
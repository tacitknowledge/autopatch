package com.tacitknowledge.util.migration.jdbc;

/**
 * MockDatabaseType since DatabaseType is not interface based and
 * can't mock it via easymock (without upgrading version to use the 
 * class extension lib)
 * @author Alex Soto <apsoto@gmail.com>
 */
class MockDatabaseType extends DatabaseType
{
    /** does database type support multipe sql statements per stmt.execute() */
    private boolean multipleStatementsSupported;
    
    /**
     * constructor
     * @param databaseType set the type
     */
    public MockDatabaseType(String databaseType) 
    {
        super(databaseType);
    }

    /** {@inheritDoc} */
    public boolean isMultipleStatementsSupported() 
    {
        return multipleStatementsSupported;
    }

    /**
     * simple setter
     * @param multipleStatementsSupported the value to set
     */
    public void setMultipleStatementsSupported(boolean multipleStatementsSupported) 
    {
        this.multipleStatementsSupported = multipleStatementsSupported;
    }
    
}


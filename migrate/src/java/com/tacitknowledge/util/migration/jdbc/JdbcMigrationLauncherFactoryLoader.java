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

/**
 * Load a MigrationLauncherFactory. This will default to loading the
 * JdbcMigrationLauncherFactory, but will examine the system properties
 * for a property called "migration.factory" and load that one if specified
 * 
 * @author Jacque Morel
 */
public class JdbcMigrationLauncherFactoryLoader 
{
    /**
     * Create the JdbcMigrationLauncherFactory
     * 
     * @return JdbcMigrationLauncherFactory (or subclass)
     */
    public static JdbcMigrationLauncherFactory createFactory() 
    {
        // Get the factory name from the system properties if possible
        String factoryName = System.getProperties().getProperty("migration.factory");
        if (factoryName == null) 
        {
            factoryName = JdbcMigrationLauncherFactory.class.getName();   
        }
      
        // Load the factory
        Class factoryClass = null;
        try 
        {
            factoryClass = Class.forName(factoryName);
        } 
        catch (ClassNotFoundException e) 
        {
            throw new IllegalArgumentException("Migration factory class '" + factoryName + 
                                                "' not found.  Aborting.");
        }
        try 
        {
            return (JdbcMigrationLauncherFactory) factoryClass.newInstance();
        } 
        catch (Exception e) 
        {
            throw new RuntimeException("Problem while instantiating factory class '" + 
                                       factoryName + "'.  Aborting.", e);
        }
    }
}

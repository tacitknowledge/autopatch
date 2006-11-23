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

package com.tacitknowledge.util.migration;

/**
 * Convenience base class for migration tasks.
 * 
 * @author  Scott Askew (scott@tacitknowledge.com)x
 */
public abstract class MigrationTaskSupport implements MigrationTask
{
    /**
     * The name of this migration task
     */
    private String name;
    
    /**
     * The relative order in which this test should run
     */
    private Integer level;

    /**
     * @see MigrationTask#getName()
     */
    public String getName()
    {
        return name;
    }
    
    /**
     * Sets the name of this migration task.
     * 
     * @param name the name of this migration task
     */
    public void setName(String name)
    {
        this.name = name;
    }

    /**
     * @see MigrationTask#getLevel()
     */
    public Integer getLevel()
    {
        return level;
    }
    
    /**
     * Sets the relative order in which this test should run
     * 
     * @param lvl the relative order in which this test should run
     */
    public void setLevel(Integer lvl)
    {
        this.level = lvl;
    }

    /**
     * @see Comparable#compareTo(Object)
     */
    public int compareTo(Object o)
    {
        MigrationTask task = (MigrationTask) o;
        if (task.getLevel() == null)
        {
            return 1;
        }
        return getLevel().compareTo(task.getLevel());
    }
}

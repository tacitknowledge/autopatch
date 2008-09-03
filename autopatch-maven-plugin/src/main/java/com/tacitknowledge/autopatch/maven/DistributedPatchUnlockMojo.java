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
package com.tacitknowledge.autopatch.maven;

import org.apache.maven.plugin.MojoExecutionException;

import com.tacitknowledge.util.migration.jdbc.DistributedMigrationTableUnlock;

/**
 * Goal which forcibly unlocks a patch table that has an orphan lock.
 *
 * @goal distributed-unlock
 * @execute phase=compile
 * @requiresDependencyResolution compile
 */
public class DistributedPatchUnlockMojo extends AbstractAutoPatchMojo
{
	public void execute() throws MojoExecutionException
    {
    	try 
    	{    		
    	    addClasspathElementsClassLoader();
            
            DistributedMigrationTableUnlock mtu = new DistributedMigrationTableUnlock();
    		mtu.tableUnlock(systemName, migrationSettings);
        }
        catch (Exception e) 
        {
        	getLog().error(e);
		}
    }
}

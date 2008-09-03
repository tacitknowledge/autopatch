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
import org.apache.maven.plugin.MojoFailureException;

import com.tacitknowledge.util.migration.jdbc.JdbcMigrationLauncher;
import com.tacitknowledge.util.migration.jdbc.JdbcMigrationLauncherFactory;
import com.tacitknowledge.util.migration.jdbc.JdbcMigrationLauncherFactoryLoader;

/**
 * Goal which applies patches found in the classpath.
 *
 * @goal patch
 * @execute phase=compile
 * @requiresDependencyResolution compile
 */
public class PatchExecutionMojo extends AbstractAutoPatchMojo {

    public void execute() throws MojoExecutionException, MojoFailureException {
        try {
            addClasspathElementsClassLoader();

            JdbcMigrationLauncherFactory launcherFactory = JdbcMigrationLauncherFactoryLoader.createFactory();
            JdbcMigrationLauncher launcher = launcherFactory.createMigrationLauncher(systemName, migrationSettings);
            launcher.doMigrations();
        }
        catch (Exception e) {
            getLog().error(e);
            throw new MojoFailureException(e.getMessage());
        }
    }

}

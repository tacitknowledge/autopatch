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

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.tacitknowledge.util.discovery.ClassDiscoveryUtil;
import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.MigrationTaskSource;

/**
 * Search a package (directory) for SQL scripts that a specific pattern 
 * and returns corresponding <code>SqlScriptMigrationTasks</code>.  The name
 * of each script must follow the pattern of &quot;patch(\d+)(_.+)?\.sql&quot;.
 * 
 * @author  Scott Askew (scott@tacitknowledge.com)
 */
public class SqlScriptMigrationTaskSource implements MigrationTaskSource
{
    /** Class logger */
    private static Log log = LogFactory.getLog(SqlScriptMigrationTaskSource.class);
    
    /**
     * The regular expression used to match SQL patch files.
     */
    private static final String SQL_PATCH_REGEX = "^patch(\\d+)(_.+)?\\.sql";

    /** {@inheritDoc} */
    public List getMigrationTasks(String packageName) throws MigrationException
    {
        String path = packageName.replace('.', '/');
        String[] scripts = ClassDiscoveryUtil.getResources(path, SQL_PATCH_REGEX);
        if (log.isDebugEnabled())
        {
            log.debug("Found " + scripts.length + " patches in path: " + path);
            for (int i = 0; i < scripts.length; i++)
            {
                log.debug(" -- \"" + scripts[i] + "\"");
            }
        }
        return createMigrationScripts(scripts);
    }

    /**
     * Creates a list of <code>SqlScriptMigrationTask</code>s based on the array
     * of SQL scripts.
     *  
     * @param  scripts the classpath-relative array of SQL migration scripts
     * @return a list of <code>SqlScriptMigrationTask</code>s based on the array
     *         of SQL scripts
     * @throws MigrationException if a SqlScriptMigrationTask could no be created
     */
    private List createMigrationScripts(String[] scripts) throws MigrationException
    {
        Pattern p = Pattern.compile(SQL_PATCH_REGEX);
        List tasks = new ArrayList();
        for (int i = 0; i < scripts.length; i++)
        {
            String script = scripts[i];
            script = script.replace('\\', '/');
            log.debug("Examining possible SQL patch file \"" + script + "\"");
            InputStream is = getClass().getResourceAsStream("/" + script);

            if (is == null)
            {
                log.warn("Could not open input stream for file \"/" + script + "\"");
            }
            else
            {
                File scriptFile = new File(script);
                String scriptFileName = scriptFile.getName();
                try
                {
                    // Get the version out of the script name
                    Matcher matcher = p.matcher(scriptFileName);
                    if (!matcher.matches() || matcher.groupCount() != 2)
                    {
                        throw new MigrationException("Invalid SQL script name: " + script);
                    }
                    int order = Integer.parseInt(matcher.group(1));
                    
                    // We should send in the script file location so
                    // it doesn't have to buffer the whole thing into RAM
                    SqlScriptMigrationTask task
                        = new SqlScriptMigrationTask(scriptFileName, order, is);

                    // Free the resource
                    is.close();
                    task.setName(scriptFileName);
                    tasks.add(task);
                }
                catch (IOException e)
                {
                    throw new MigrationException("Error reading script " + script, e);
                }
            }
        }
        return tasks;
    }
}

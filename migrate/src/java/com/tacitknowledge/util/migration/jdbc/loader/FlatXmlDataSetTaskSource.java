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

package com.tacitknowledge.util.migration.jdbc.loader;

import java.io.File;
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
 * Search a package (directory) for xml files that match a specific pattern 
 * and returns corresponding {@link FlatXmlDataSetMigrationTask}s.  The name
 * of each script must follow the pattern of &quot;patch(\d+)(_.+)?\.xml&quot;.
 * 
 * @author Alex Soto <alex@tacitknowledge.com>
 * @author Alex Soto <apsoto@gmail.com>
 */
public class FlatXmlDataSetTaskSource implements MigrationTaskSource 
{

    /** Class logger */
    private static Log log = LogFactory.getLog(FlatXmlDataSetTaskSource.class);
    
    /**
     * The regular expression used to match XML patch files.
     */
    private static final String XML_PATCH_REGEX = "^patch(\\d+)(_.+)?\\.xml";


    /**
     * {@inheritDoc}
     */
    public List getMigrationTasks(String packageName) throws MigrationException 
    {
        String path = packageName.replace('.', '/');
        String[] xmlFiles = ClassDiscoveryUtil.getResources(path, XML_PATCH_REGEX);

        log.debug("Found " + xmlFiles.length + " xml patche(s) in path: " + path);
        for (int i = 0; i < xmlFiles.length; i++)
        {
            log.debug(" -- \"" + xmlFiles[i] + "\"");
        }
        return createMigrationTasks(xmlFiles);
    }

    /**
     * Creates a list of {@link FlatXmlDataSetMigrationTask}s based on the array
     * of xml files.
     *  
     * @param  xmlFiles the classpath-relative array of xml files
     * @return a list of {@link FlatXmlDataSetMigrationTask}
     * @throws MigrationException in unexpected error occurs 
     */
    private List createMigrationTasks(String[] xmlFiles) throws MigrationException 
    {
        Pattern p = Pattern.compile(XML_PATCH_REGEX);
        List tasks = new ArrayList();
        for (int i = 0; i < xmlFiles.length; i++)
        {
            String xmlPathname = xmlFiles[i];
            xmlPathname = xmlPathname.replace('\\', '/');
            log.debug("Examining possible xml patch file \"" + xmlPathname + "\"");

            File xmlFile = new File(xmlPathname);
            String xmlFilename = xmlFile.getName();

            // Get the patch number out of the file name
            Matcher matcher = p.matcher(xmlFilename);
            if (!matcher.matches() || matcher.groupCount() != 2) 
            {
                throw new MigrationException("Invalid XML patch name: " + xmlFilename);
            }
            
            FlatXmlDataSetMigrationTask task = new FlatXmlDataSetMigrationTask();
            task.setLevel(new Integer(Integer.parseInt(matcher.group(1))));
            task.setName(xmlPathname);
            tasks.add(task);                

        }
        return tasks;
    }

}

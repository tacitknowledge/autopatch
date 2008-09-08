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

import java.io.File;
import java.net.MalformedURLException;
import java.net.URL;
import java.net.URLClassLoader;
import java.util.Iterator;
import java.util.List;

import org.apache.maven.plugin.AbstractMojo;

/**
 * Abstract base class that provides some common functionality for autopatch plugins.
 * 
 * @author asoto
 */
public abstract class AbstractAutoPatchMojo extends AbstractMojo 
{
    /**
     * @parameter default-value="${project.compileClasspathElements}"
     * @required
     */
    protected List classpathElements;

    /**
     * The migration property file to get migration settings from
     * @parameter expression="${autopatch.migration.settings}"
     *
     */
    protected String migrationSettings = "migration.properties";

    /**
     * The system to get patch information about
     * @parameter expression="${autopatch.system.name}"
     * @required
     */
    protected String systemName;
    
    /**
     * Adds the classpath elements to a new class loader and sets it as the 
     * current thread context's class loader.
     * @throws MalformedURLException if a classpath element contains a malformed url.
     */
    protected void addClasspathElementsClassLoader() throws MalformedURLException
    {
        // create a new classloader, adding classpath elements to the classpath
        // because they aren't there for some reason.
        // And do it without generics to keep it source 1.3 compatible
        URL[] urls = new URL[classpathElements.size()];
        Iterator iter = classpathElements.iterator();
        int i = 0;
        while(iter.hasNext())
        {
            urls[i++] = new File((String)iter.next()).toURL(); 
        }
        
        URLClassLoader urlClassLoader = 
            new URLClassLoader(urls, Thread.currentThread().getContextClassLoader());
        Thread.currentThread().setContextClassLoader(urlClassLoader);
    }
}

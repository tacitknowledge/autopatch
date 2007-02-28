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

package com.tacitknowledge.util.migration.jdbc.util;

import java.util.Iterator;
import java.util.Properties;

import javax.servlet.ServletContext;
import javax.servlet.ServletContextEvent;

/**
 * A utility class for various configuration needs
 *
 * @author Mike Hardy (mike@tacitknowledge.com)
 */
public class ConfigurationUtil
{
    /**
     * Returns the value of the specified servlet context initialization parameter.
     * 
     * @param  param the parameter to return
     * @param  properties the <code>Properties</code> for the Java system
     * @param  arguments optionally takes the arguments passed into the main to 
     *          use as the migration system name
     * @return the value of the specified system initialization parameter
     * @throws IllegalArgumentException if the parameter does not exist
     */
    public static String getRequiredParam(String param, Properties properties, 
            String[] arguments) throws IllegalArgumentException
    {
        String value = properties.getProperty(param);
        if (value == null)
        {
            if ((arguments != null) && (arguments.length > 0))
            {
                value = arguments[0].trim();
            }
            else
            {
                throw new IllegalArgumentException("'" + param + "' is a required "
                        + "initialization parameter.  Aborting.");
            }
        }
        return value;
    }

    /**
     * Returns the value of the specified configuration parameter.
     *
     * @param  props the properties file containing the values
     * @param  param the parameter to return
     * @return the value of the specified configuration parameter
     * @throws IllegalArgumentException if the parameter does not exist
     */
    public static String getRequiredParam(Properties props, String param)
        throws IllegalArgumentException
    {
        String value = props.getProperty(param);
        if (value == null)
        {
            System.err.println("Parameter named: " + param + " was not found.");
            System.err.println("-----Parameters found-----");
            Iterator propNameIterator = props.keySet().iterator();
            while (propNameIterator.hasNext())
            {
                String name = (String) propNameIterator.next();
                String val = props.getProperty(name);
                System.err.println(name + " = " + val);
            }
            System.err.println("--------------------------");
            throw new IllegalArgumentException("'" + param + "' is a required "
                + "initialization parameter.  Aborting.");
        }
        return value;
    }

    /**
     * Returns the value of the specified configuration parameter.
     *
     * @param  props the properties file containing the values
     * @param  param the parameter to return
     * @param  alternate the alternate parameter to return
     * @return the value of the specified configuration parameter
     */
    public static String getRequiredParam(Properties props, String param, String alternate)
    {
        try
        {
            return getRequiredParam(props, param);
        }
        catch (IllegalArgumentException e1)
        {
            try
            {
                return getRequiredParam(props, alternate);
            }
            catch (IllegalArgumentException e2)
            {
                throw new IllegalArgumentException("Either '" + param + "' or '" + alternate
                    + "' must be specified as an initialization parameter.  Aborting.");
            }
        }
    }
    
    /**
     * Returns the value of the specified servlet context initialization parameter.
     * 
     * @param  param the parameter to return
     * @param  sce the <code>ServletContextEvent</code> being handled
     * @param  caller the calling object, used for printing debugging information if there is a problem
     * @return the value of the specified servlet context initialization parameter
     * @throws IllegalArgumentException if the parameter does not exist
     */
    public static String getRequiredParam(String param, ServletContextEvent sce, Object caller)
        throws IllegalArgumentException
    {
        ServletContext context = sce.getServletContext();
        String value = context.getInitParameter(param);
        if (value == null)
        {
            throw new IllegalArgumentException("'" + param + "' is a required "
                + "servlet context initialization parameter for the \""
                + caller.getClass().getName() + "\" class.  Aborting.");
        }
        return value;
    }
}
/* Copyright 2007 Tacit Knowledge LLC
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

package com.tacitknowledge.util.migration.jdbc.spring;

import javax.servlet.ServletContext;

import org.springframework.web.context.ServletContextAware;

import com.tacitknowledge.util.discovery.ClassDiscoveryUtil;
import com.tacitknowledge.util.discovery.WebAppResourceListSource;
import com.tacitknowledge.util.migration.jdbc.AutoPatchService;


/**
 * Specialization of the <code>AutoPatchService</code> that uses Spring's
 * <code>ServletContextAware</code> interface to add WEB-INF to the list of
 * places that are used while searching for patches.
 *
 * @author Scott Askew (scott@tacitknowledge.com)
 */
public class SpringAutoPatchService extends AutoPatchService implements ServletContextAware
{
    /**
     * @see ServletContextAware#setServletContext(ServletContext)
     */
    public void setServletContext(ServletContext context)
    {
        ClassDiscoveryUtil.addResourceListSource(
             new WebAppResourceListSource(context.getRealPath("/WEB-INF")));
    }
}

/*
 * Copyright (c) Shoreline Associates X, LLC. 444 High Street, Suite 400
 * Palo Alto, CA 94301.  All Rights Reserved. This software is the confidential
 * and proprietary information of Shoreline Associates X, LLC.
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

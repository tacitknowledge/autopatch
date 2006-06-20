/* 
 * Copyright 2006 Tacit Knowledge LLC
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
package com.tacitknowledge.util.migration.jdbc.atg;

import com.tacitknowledge.util.migration.MigrationException;
import com.tacitknowledge.util.migration.jdbc.DistributedAutoPatchService;

import atg.nucleus.Configuration;
import atg.nucleus.Nucleus;
import atg.nucleus.Service;
import atg.nucleus.ServiceEvent;
import atg.nucleus.ServiceException;

/**
 * Automatically applies database DDL and SQL patches to all schemas on server startup.
 *
 * @author Mike Hardy (mike@tacitknowledge.com)
 * @link http://autopatch.sf.net/
 */
public class ATGDistributedAutoPatchService extends DistributedAutoPatchService implements Service
{
    /** Our Nucleus */
    private Nucleus nucleus = null;
    
    /** The Configuration we have */
    private Configuration configuration = null;
    
    /** Whether we are running or not */
    private boolean running = false;
    
    /**
     * Handle patching the database on startup
     * 
     * @see atg.nucleus.ServiceListener#startService(atg.nucleus.ServiceEvent)
     */
    public void startService(ServiceEvent se) throws ServiceException
    {
        if ((se.getService() == this) && (isRunning() == false))
        {
            setRunning(true);
            setNucleus(se.getNucleus());
            setServiceConfiguration(se.getServiceConfiguration());
            doStartService();
        }
    }
    
    /**
     * @see com.tacitknowledge.util.migration.jdbc.AutoPatchService#patch()
     */
    public void doStartService() throws ServiceException
    {
        try
        {
            patch();
        }
        catch (MigrationException me)
        {
            throw new ServiceException("There was a problem patching the database", me);
        }
    }

    /**
     * Resets the "running" state to false
     * 
     * @see atg.nucleus.Service#stopService()
     */
    public void stopService() throws ServiceException
    {
        setRunning(false);
    }



    /**
     * Get the service configuration used to start us
     * @return Configuration
     */
    public Configuration getServiceConfiguration()
    {
        return configuration;
    }

    /**
     * Set the service configuration the Nucleus is using for us
     * @param configuration
     */
    public void setServiceConfiguration(Configuration configuration)
    {
        this.configuration = configuration;
    }

    /**
     * Get the Nucleus that started us
     * @return Nucleus
     */
    public Nucleus getNucleus()
    {
        return nucleus;
    }

    /**
     * Set the Nucleus that started us
     * @param nucleus
     */
    public void setNucleus(Nucleus nucleus)
    {
        this.nucleus = nucleus;
    }

    /**
     * Return boolean true if we are started
     * @return true if we are started and running
     */
    public boolean isRunning()
    {
        return running;
    }

    /**
     * Set whether we are running
     * @param running
     */
    public void setRunning(boolean running)
    {
        this.running = running;
    }
}

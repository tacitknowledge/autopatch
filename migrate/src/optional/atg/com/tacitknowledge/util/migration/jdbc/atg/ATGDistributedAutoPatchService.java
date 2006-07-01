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
    
    /** Should we fail the server if a patch fails on startup? */
    private boolean failServerOnError = true;
    
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
            if (isFailServerOnError())
            {
                if (isFailServerOnError())
                {
                    System.err.println("There was a problem patching the database");
                    me.printStackTrace(System.err);
                    System.err.println("Shutting down server to prevent inconsistency");
                    System.err.println("Change the 'failServerOnError' property if you want it to start anyway");
                    System.exit(1);
                }
            }
            
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

    /**
     * Determine whether we should fail the server on an error condition. This involves
     * throwing an Error instead of an Exception so ATG will halt module loading. It is
     * recommended to leave this set to true as allowing startup on patch failure may allow
     * your software to run despite an inconsistent state in the database
     * 
     * @return boolean true if the server should stop module load on patch failure
     */
    public boolean isFailServerOnError()
    {
        return failServerOnError;
    }

    /**
     * @see #isFailServerOnError()
     * @param failServerOnError boolean true to fail server on patch failure
     */
     public void setFailServerOnError(boolean failServerOnError)
    {
        this.failServerOnError = failServerOnError;
    }
}

/* Copyright (c) 2004 Tacit Knowledge LLC  
 * See licensing terms below.
 * 
 * THIS SOFTWARE IS PROVIDED "AS IS" AND ANY  EXPRESSED OR IMPLIED 
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
 * IN NO EVENT SHALL TACIT KNOWLEDGE LLC OR ITS CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
 * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
 * OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  THIS HEADER MUST
 * BE INCLUDED IN ANY DISTRIBUTIONS OF THIS CODE.
 */
package com.tacitknowledge.util.migration.jdbc.loader;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.InputStream;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

/**
 * This is a very simple utility that looks for a file 
 * based upon its existence in the classpath or the 
 * absolute path if provided.
 * 
 * @author Chris A. (chris@tacitknowledge.com)
 */
public class FileLoadingUtility
{
    /**
     * Class logger
     */
    private static Log log = LogFactory.getLog(FileLoadingUtility.class);
    
    /**
     * The name of the file to load
     */
    private String fileName = null;
    
    /**
     * Creates a new <code>FileLoadingUtility</code>.
     * 
     * @param fileName the name of the file to load
     */
    public FileLoadingUtility(String fileName)
    {
        this.fileName = fileName;
    }
    
    /**
     * Gets an input stream by first checking the current classloader, 
     * then trying to use the system classloader, and finally, trying 
     * to access the file on the file system.  If the file is not found, 
     * an <code>IllegalArgumentException</code> will be thrown.
     * 
     * @return the file as an input stream
     */
    public InputStream getResourceAsStream()
    {
        InputStream stream = getClass().getResourceAsStream(fileName);
        if (stream == null)
        {
            stream = ClassLoader.getSystemResourceAsStream(fileName);
            
        }
        if (stream == null)
        {
            File f = new File(fileName);
            try
            {
                stream = new FileInputStream(f);
            } 
            catch (FileNotFoundException e)
            {
                log.error("The file: " + fileName + " was not found.", e);
                throw new IllegalArgumentException("Must have a valid file name.");
            }
        }
        return stream;
    }
}

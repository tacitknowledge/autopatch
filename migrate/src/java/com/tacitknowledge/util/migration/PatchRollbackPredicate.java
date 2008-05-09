package com.tacitknowledge.util.migration;

import org.apache.commons.collections.Predicate;

/**
 * Created by IntelliJ IDEA.
 * User: apeshimam
 * Date: May 6, 2008
 * Time: 4:49:18 PM
 * To change this template use File | Settings | File Templates.
 */
public class PatchRollbackPredicate implements Predicate
{
    private int rollbackPatchLevel = Integer.MAX_VALUE;
    private int currentPatchLevel = Integer.MAX_VALUE;

    public PatchRollbackPredicate(int currentPatchLevel,int rollbackPatchLevel) {
        this.rollbackPatchLevel = rollbackPatchLevel;
        this.currentPatchLevel = currentPatchLevel;
    }

    public boolean evaluate(Object obj) {
    	
    	if(obj == null)
    		return false;
    	
        if(!(obj instanceof RollbackableMigrationTask))
            return false;

        RollbackableMigrationTask task = (RollbackableMigrationTask) obj;
        final int level = task.getLevel().intValue();
        
        return ((level > rollbackPatchLevel) && (level <= currentPatchLevel));
    }
}

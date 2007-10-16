<?php

include_once "includes/database.inc";
include_once "includes/auth.inc";

$patch_level = get_patch_level();
echo "The current patch level is " . $patch_level . "<P>";


$patches = get_patches();
foreach ($patches as $patch) {
    echo "Examining patch " . $patch . "...<BR>";

    // Get the patch number for this patch
    preg_match("/^patches\/patch(\d+)_.*$/", $patch, $matches);
    $patch_number = $matches[1];
    echo "&nbsp;&nbsp;Patch number is " . $patch_number . "...<BR>";

    // Skip the patch if we're already at this level or higher
    if ($patch_level >= $patch_number) {
        echo "&nbsp;&nbsp;This patch has already been applied. Skipping.<BR>";
        continue;
    }

    // Apply the patch by including it so it will execute
    echo "&nbsp;&nbsp;This patch has not been applied. Executing patch...<BR>";
    
    if (preg_match("/.*\.php/", $patch)) {
	    include_once($patch);
    } else if (preg_match("/.*\.sql/", $patch)) {
    	$result = mysql_query(file_get_contents($patch));
    	if (!result) {
    		die("Unable to run patch " . $patch . ". Aborting.");
    	}
    } else {
    	echo "Unknown patch type for patch " . $patch . " skipping.<BR>";
    }
    // Now set the new patch level
    set_patch_level($patch_number);
}

/**
 * Get the list of filenames to run as patches
 */
function get_patches()
{
    $patches = Array();
    $dir = opendir("patches");
    while (false !== ($file = readdir($dir))) {
    	echo "Considering " . $file . "<P>\n";
       if (preg_match("/^patch\d+_.*/", $file)) {
           array_push($patches, "patches/$file");
       }
    }
    
    closedir($dir);
    sort($patches, SORT_STRING);
    return $patches;
}

/**
 * Gets the current database patch level. Will create patch tables if necessary.
 */
function get_patch_level()
{
    $query = "SELECT patch_level FROM patches";
    $result = mysql_query($query);
    $patch_level = 0;

    // If we weren't able to get the patch level, maybe the table doesn't exist yet? Create it.
    if (!$result) {
        echo "Unable to get patch level, the patch tables may not exist. Attempting to create them.<P>";
        $query = "CREATE TABLE patches (patch_level INTEGER)";
        $result = mysql_query($query);

        // If we couldn't create it either, bail.
        if (!$result) {
            die("Unable to create patch table either. There must be a bigger problem. Aborting.");
        }
        
        // Initialize the database patch level
        set_patch_level(0);
    }
    else {
        $patch_level = mysql_result($result, 0, 0);
    }

    return $patch_level;
}

/**
 * Inserts the given patch level into the database as the current patch level
 */
function set_patch_level($patch_level)
{
    // Now set the patch level to the one we've gotten to.
    $query = "DELETE FROM patches";
    $result = mysql_query($query);
    if (!$result) {
        die("Unable to delete old patch level");
    }
    $query = "INSERT INTO patches (patch_level) VALUES (" . $patch_level . ")";
    $result = mysql_query($query);
    if (!$result) {
        die("Unable to set new patch level into database");
    }
    echo "Patch level set to " . $patch_level . "<BR>";
}

?>

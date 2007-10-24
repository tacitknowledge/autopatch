<?php

include_once "includes/database.inc";
include_once "includes/auth.inc";

// Get a handle to mysql
$mysql_handle = get_db_connection();

$patch_level = get_patch_level($mysql_handle);
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
        
        /* execute multi query */
        if (mysqli_multi_query($mysql_handle, file_get_contents($patch))) {
            do {
                /* store first result set */
                if ($result = mysqli_store_result($mysql_handle)) {
                    //do nothing since there's nothing to handle
                    mysqli_free_result($result);
                }
            } while (mysqli_next_result($mysql_handle));
            if (mysqli_errno($mysql_handle)) {
                if (strstr(mysqli_error($mysql_handle), "Couldn't fetch mysqli")) {
                    echo "<b>Fatal error detected, but it's a known type ('Couldn't fetch...') - reload autopatch and it will run one patch per reload (sorry)<br></b>";
                }
                die("Unable to run patch " . $patch . ". Aborting.<BR>" . mysqli_error($mysql_handle));
            }
        }
    } else {
        echo "Unknown patch type for patch " . $patch . " skipping.<BR>";
    }
    // Now set the new patch level
    set_patch_level($mysql_handle, $patch_number);
}

// close connection
mysqli_close($mysql_handle);

/**
 * Get the list of filenames to run as patches. Will fail/exit if there are duplicates.
 */
function get_patches()
{
    $patches = array();
    $patch_numbers = array();
    $dir = opendir("patches");
    while (false !== ($file = readdir($dir))) {
        echo "Considering " . $file . "<P>\n";
        $matches = array();
        if (preg_match("/^patch(\d+)_.*/", $file, &$matches)) {
            
            if (array_search($matches[1], array_keys($patch_numbers)))
            {
                echo "<B>Two patches found with the same patch number!</B><BR/>";
                echo "&nbsp;&nbsp;" . $patch_numbers[$matches[1]] . "<BR/>";
                echo "&nbsp;&nbsp;" . $file . "<BR/>";
                die("<B>Exiting while you fix your patch sequence do have unique numbers.</B>");
            }
            
            array_push($patches, "patches/$file");
            $patch_numbers[$matches[1]] = $file;
        }
    }
    
    closedir($dir);
    sort($patches, SORT_STRING);
    return $patches;
}

/**
 * Gets the current database patch level. Will create patch tables if necessary.
 */
function get_patch_level($db)
{
    $query = "SELECT patch_level FROM patches";
    $result = mysqli_query($db, $query);
    $patch_level = 0;

    // If we weren't able to get the patch level, maybe the table doesn't exist yet? Create it.
    if (!$result) {
        echo "Unable to get patch level, the patch tables may not exist. Attempting to create them.<P>";
        $query = "CREATE TABLE patches (patch_level INTEGER)";
        $result = mysqli_query($db, $query);

        // If we couldn't create it either, bail.
        if (!$result) {
            die("Unable to create patch table either. There must be a bigger problem. Aborting.");
        }
        
        // Initialize the database patch level
        set_patch_level(0);
    }
    else {
        $row = mysqli_fetch_row($result);
        $patch_level = $row[0];
    }

    return $patch_level;
}

/**
 * Inserts the given patch level into the database as the current patch level
 */
function set_patch_level($db, $patch_level)
{
    // Now set the patch level to the one we've gotten to.
    $query = "DELETE FROM patches";
    $result = mysqli_query($db, $query);
    if (!$result) {
        die("Unable to delete old patch level: " . mysqli_error($db));
    }
    $query = "INSERT INTO patches (patch_level) VALUES (" . $patch_level . ")";
    $result = mysqli_query($db, $query);
    if (!$result) {
        die("Unable to set new patch level into database");
    }
    echo "Patch level set to " . $patch_level . "<BR>";
}

?>

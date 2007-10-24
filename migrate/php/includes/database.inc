<?php 

$DB_NAME = "YOUR_DB_NAME_HERE";
define('DB_PASSWORD', 'YOUR_DB_PASSWORD_HERE');
define( 'DB_USER',    'YOUR_DB_USER_HERE' );

// Define our database host based on where we are running.
// The production site gets one setting on the old production host
if (($_SERVER['SERVER_NAME'] == "YOUR_STAGING_DB_HOST") ||
    ($_SERVER['SERVER_NAME'] == "YOUR_DEV_DB_HOST") ||
    ($_SERVER['SERVER_NAME'] == "YOUR_INTEGRATION_DB_HOST")) {
    define('DB_HOST', 'localhost'); 
} 
else {
    define('DB_HOST', 'YOUR_PROD_DB_HOST');
    $DB_NAME = "YOUR_PROD_DB_NAME";
}

define( 'DB_PORT',     '3306' );

function get_db_connection()
{
	$db = mysqli_connect(DB_HOST, DB_USER, DB_PASSWORD, $DB_NAME);
	if (mysqli_connect_errno()) {
	   die ( "<H3>DB Server Unreachable</H3><BR>" . mysqli_connect_error());
	}
    return $db;
}

// Get a "database safe" version of the post variable at the given index
function getDBSafe($string)
{
    $variable = "";
    if (isset($_POST[$string])) {
        $variable = $_POST[$string];
    }
    if (isset($_SERVER[$string])) {
        $variable = $_SERVER[$string];
    }
    if (isset($_GET[$string])) {
        $variable = $_GET[$string];
    }

    // useful for debugging.
    //error_log($string . " is " . $variable);

    return mysql_escape_string($variable);
}

?>

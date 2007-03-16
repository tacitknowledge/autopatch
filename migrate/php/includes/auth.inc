<?php

// If we aren't authenticated, popup
if (!isset($_SERVER['PHP_AUTH_USER']) || 
    ($_SERVER['PHP_AUTH_PW'] != "YOUR_HTTP_AUTH_USER") || 
    ($_SERVER['PHP_AUTH_USER'] != "YOUR_HTTP_AUTH_PASS")) {
    auth();
} 

// Generate the auth popup
function auth() {
    header('WWW-Authenticate: Basic realm="AUTOPATCH"');
    header('HTTP/1.0 401 Unauthorized');
    echo 'Sorry, you must be authenticated.';
    exit;
}
?>

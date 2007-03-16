<?php

// See if the tables already exist with a throw-away query
$query = "SELECT count(contestid) FROM contests";
$result = mysql_query($query);

if (!$result) {

    // contests
    //  month, contest title, contest page
    $query = 
        "CREATE TABLE contests (" .
        "contestid           integer not null auto_increment," .
        "contest_title       varchar(100) not null unique," .
        "contest_month       varchar(100) not null unique," .
        "contest_page        varchar(100) not null," .
        "PRIMARY KEY (contestid));";

    echo "Executing " . $query . "<P>";
    $result = mysql_query($query);
    if (!$result) {
        die("Unable to create contests table? Aborting.");
    }

    // films
    //  contest, film name, film
    $query = 
        "CREATE TABLE films (" .
        "filmid              integer not null auto_increment," .
        "contestid           integer not null REFERENCES contests.contestid," .
        "film_name           varchar(200) not null," .
        "PRIMARY KEY (filmid));";

    echo "Executing " . $query . "<P>";
    $result = mysql_query($query);
    if (!$result) {
        die("Unable to create films table? Aborting.");
    }

    // clicks
    //  ip, ad-clicked, date
    $query = 
        "CREATE TABLE clicks (" .
        "clickid             integer not null auto_increment," .
        "ip                  varchar(15) not null," .
        "advertisement       varchar(200) not null," .
        "clicktime           datetime not null," .
        "PRIMARY KEY (clickid));";

    echo "Executing " . $query . "<P>";
    $result = mysql_query($query);
    if (!$result) {
        die("Unable to create clicks table? Aborting.");
    }

    // votes
    //  film, ip, date
    $query = 
        "CREATE TABLE votes (" .
        "voteid              integer not null auto_increment," .
        "ip                  varchar(15) not null," .
        "filmid              integer not null REFERENCES films.filmid," .
        "votetime            datetime not null," .
        "PRIMARY KEY (voteid));";

    echo "Executing " . $query . "<P>";
    $result = mysql_query($query);
    if (!$result) {
        die("Unable to create votes table? Aborting.");
    }

}

?>
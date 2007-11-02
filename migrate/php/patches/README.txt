You should put your patches in this directory.
Files should contain valid PHP that executes a patch.
Files should have names like 'patchNNN_<patch_name>.php' - e.g. 'patch001_create_initial_tables.php'
A sample patch has been included, but note that it is *not* named correctly, so it will not execute - it is an example.

Please note that if you are trying to create stored procedures in MySQL (a common case, apparently)
you must not use 'DELIMITER $$' or put anything after 'END' (not even a semicolon). Just the raw
'CREATE PROCEDURE ... END'. If you need to drop the procedure, use two patches (one for drop, one to
create the new version) since we haven't tested whether you can do both at once...
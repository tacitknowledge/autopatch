<?php include("includes/header.inc"); ?>
<?php include("includes/navbar.inc"); ?>
<?= navbar("Quick Start"); ?>
<?php include("includes/updates.inc"); ?>

<h3>Get started using AutoPatch</h3>

Sometimes it is easiest to see an example of how to use AutoPatch in order to see how best to set it up.

We're working on an example, in the meantime, these are the steps we take to set it up:

<ol>
<li>Download autopatch, and put the jar file (as well as commons-logging and maybe log4j) in your library directory</li>
<li>Follow the <a href="documentation.php#development_integration">instructions</a> to integrate autopatch with ant</li>
<li>Make a "patches" directory somewhere in your classpath</li>
<li>Take the "migration.properties" file from the docs directory and alter it to suit your database environment and the location of the patches directory in your classpath, and put it in your classpath</li>
<li>Author any sql patch you need in that patches directory, with a name like "patch0001_test_patch.sql"</li>
<li>When you run the ant target "patchinfo" it should tell you that it found one patch, and the patch needs to be applied</li>
<li>When you run the ant target "patchdb" it should apply the patch and set your patch level to 1</li>
</ol>

<p>That's it!</p>

<p>From there, you can add a bunch more SQL patches, you can implement Java-language patches if you want, etc etc.</p>

<p>We typically implement the entire database schema as patches from the ground-up, that way installing the application in new environments is trivial.</p>

<?php include("includes/footer.inc"); ?>

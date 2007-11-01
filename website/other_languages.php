<?php include("includes/header.inc"); ?>
<?php include("includes/navbar.inc"); ?>
<?= navbar("Other Languages"); ?>
<?php include("includes/updates.inc"); ?>

<h3>AutoPatch in other languages</h3>

<p>The primary implementation of AutoPatch is in Java, but AutoPatch is an idea on how to apply an agile process to persistent storage more than an implementation.</p>

<p>As such, we have implemented this idea (to a larger or smaller degree) in other languages as well.

<p>We have pretty good / used in production implementations in:
<ul>
<li><a href="http://autopatch.cvs.sourceforge.net/autopatch/autopatch/migrate/php/">PHP</a></li>
</ul>

<p>We have basic implementations in:
<ul>
<li><a href="http://autopatch.cvs.sourceforge.net/autopatch/autopatch/migrate/ruby/">Ruby</a> (it's different than stock Migrations - it works on Windows/SQLServer)
<li><a href="http://autopatch.cvs.sourceforge.net/autopatch/autopatch/migrate/dotnet/">.NET</a>.
</ul>
</p>

<?php include("includes/footer.inc"); ?>

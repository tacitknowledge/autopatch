<?php include("includes/header.inc"); ?>
<?php include("includes/navbar.inc"); ?>
<?= navbar("FAQ"); ?>
<?php include("includes/updates.inc"); ?>

<h3>Frequently Asked Questions</h3>

There currently aren't many, but here's a start:

<ol>
<li><p>Is AutoPatch tested well?</p>
<p>We develop AutoPatch using JUnit and Clover, a coverage utility. Our unit tests contain a *lot* of assertions so
we know the tests themselves are useful, and you can see the coverage for yourself:</p>
<img src="images/AutoPatch_Coverage.jpg">
<p>In short, we believe AutoPatch is tested well (<b>especially</b> the complicated bits), and is very robust. Our experience with it in production bears that out.
</li>
<li><p>Is anyone using AutoPatch?</p>
<p>AutoPatch is in production use at almost every one of <a href="http://www.tacitknowledge.com/clients.html">our clients</a>. Something important to note is that
the total headcount of DBAs at all the clients we have that use AutoPatch is zero. Developer throughput is high though. That's efficiency.</p>
</li>
<li><p>What do DBAs do if they don't move patches around?</p>
<p>We believe DBAs should be involved in patch <b>creation</b> (where knowledge of SQL and SQL programming languages like PL/SQL is very important) and in database tuning. Synchronizing
environments and applying patches is something computers should be good at, DBAs shouldn't have to to be involved.</p>
</li>
</ol>

<?php include("includes/footer.inc"); ?>

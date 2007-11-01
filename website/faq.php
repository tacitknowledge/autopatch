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
<li><p>How do you manage patches on branches when you have emergency production changes?</p>
<p>This is a tough one but can be handled well. The best way we've heard so far is to implement two separate patch streams (different system names).<br>
You should configure the patch streams so that you can run them in distributed mode or with both as standalone.<br>
One patch stream is used for emergency patches on branches only. <br>
Any time you cut a branch, leave a gap in the patch sequence on the main patch stream.<br>
Any time you make an emergency patch, put it in the emergency patch stream on branch and trunk using the lowest available number from the gap.<br>
Any time you make a normal patch on trunk, put it in the main patch sequence using the first available patch number after the gap.<br>
During normal development, execute the two patch streams individually as standalone patch streams<br>
During full database rebuilds, execute the patch streams as a distributed system.<br>
This guarantees that emergency patches propagate to all systems immediately, 
but also guarantees that database rebuilds apply patches in the same order they applied when they were created.<br>
In the future we'll probably support a dot-notation (similar to CVS revision branch numbering) so you don't have to worry about leaving a patch sequence number gap.
</p>
</ol>

<?php include("includes/footer.inc"); ?>

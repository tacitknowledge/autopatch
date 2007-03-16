<?php include("includes/header.inc"); ?>
<?php include("includes/navbar.inc"); ?>
<?= navbar("How to Help"); ?>
<?php include("includes/updates.inc"); ?>

	<h3>How to Help</h3>

	<p>We have gotten a lot of developer interest in AutoPatch recently (thanks!) so it seemed like a good idea to list the exact steps you need to take if you would like to be a developer on the project</p>

        <P>
<ol>
  <li>It is always a good idea to <a href="contact_us.php">contact us</a> and let us know why you are interested in working on autopatch. What do you wish it did that it doesn't do? What would you change? How would you change it? Think of this as the design phase. This is very important to do prior to starting work so you don't duplicate effort in progress or do work that won't be accepted into the codebase</li>
  <li>Create a sourceforge.net account, and use it to sign up to the <a href="https://sourceforge.net/mailarchive/forum.php?forum_id=51711">autopatch-cvs</a> mailing list so you can see what other people are working on
  <li>After coordinating with the rest of the group and getting on the mailing list, propose some code - post a patch to autopatch-cvs for a bug or your new feature, and we will review it. There will likely be some changes requested, but this is a very important step to make sure that everyone understands the code, and the codebase will still look uniform once your changes are committed</li>
  <li>Get the final change committed. If you are not a developer on the project, post the final patch on autopatch-cvs and one of the developers will commit it for you, otherwise, commit it yourself, and it will show up in the next release.
</ol>
</p>

<p>After submitting a few patches, it might make sense to give you developer access to the autopatch project. To do that, follow these steps:
<ol>
<li>Make sure you have a sourceforge.net account, and that it is subscribed to the autopatch-cvs mailing list</li>
<li>We use CVS, and sourceforge.net CVS is done using SSH with shared keys. Make sure you have an SSH key, and that you have uploaded it to sourceforge.net, following <a href="https://sourceforge.net/docs/F02/en/">these instructions</a> - especially the part about key generation and "SSH Key Posting"</li>
<li>Once you have your SSH key uploaded, send email to the autopatch-cvs mailing list requesting to be added as a developer to the project - if you have been a reliable developer in the past, and have some good development plans for the future, we'll add you and you will be able to use CVS directly to work on the project</li>
</ol>
</p>

	<p>Thanks!</p>

<?php include("includes/footer.inc"); ?>

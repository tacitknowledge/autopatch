<?php include("includes/header.inc"); ?>
<?php include("includes/navbar.inc"); ?>
<?= navbar("Home"); ?>
<?php include("includes/updates.inc"); ?>
    
    <h2>Welcome to AutoPatch</h2>

		<p><b>AutoPatch automates the application of changes to persistent storage.</b></p>

		<p>AutoPatch was born from the needs of using an agile development process while working on systems that have persistent storage. 
		Without AutoPatch, developers usually can't afford the maintenance headache of their own database, and DBAs
		are required just to apply changes to all of the various environments a serious development effort requires.</p>

		<p>The very application of database changes becomes an inefficient, error-prone, expensive process, all conspiring to discourage
		any refactoring that touches the model, or being a bottleneck when model changes are made.

		<p><b>AutoPatch solves this problem, completely.</b></p>

		<p>With AutoPatch, an agile development process that requires a database change looks like this:</p>
			<ul>
				<li>Developer alters the model, which requires a change to the database</li>
				<li>Developer possibly consults a DBA, and develops a SQL patch against their personal database that implements the alteration</li>
				<li>Developer commits the patch to source control at the same time as they commit their dependent code</li>
				<li>Other developers' and environments' databases are automatically updated by AutoPatch the next time the new source is run</li>
			</ul>

		<p>This represents streamlined environment maintenance, allowing developers to cheaply have their own databases and all databases to stay in synch with massively lower costs and no environment skew.</p>

		<p><b>That's what AutoPatch does.</b></p>

		<p>Clusters with one database? Multiple schemas? Logical migrations, instead of just DDL changes? Need to do something special/custom? 
		Need to distribute your changes commercially? All without paying anything? No problem.</p>

		<p>Please take a look at the documentation, download it and give it a whirl, or <a href="contact_us.php">let us know</a> if you have any questions.</p>

		<p>Enjoy!</p>
		

<?php include("includes/footer.inc"); ?>

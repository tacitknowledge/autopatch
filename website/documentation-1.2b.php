<?php include("includes/header.inc"); ?>
<?php include("includes/navbar.inc"); ?>
<?= navbar("Documentation"); ?>
<?php include("includes/updates.inc"); ?>
    

<h3>AutoPatch Documentation</h3>

<ol>
<li><a href="#what">What is AutoPatch?</a></li>
<li><a href="#requirements">Design Requirements</a></li>
<li><a href="#basic_design">Basic Design</a></li>
<li><a href="#detailed_design">Detailed Design</a></li>
<li><a href="#application_integration">How to integrate AutoPatch with a web application</a></li>
<li><a href="#development_integration">How to integrate AutoPatch with your development cycle</a></li>
<li><a href="#patch_implementation">How to implement a patch</a></li>
<li><a href="#extension">How to extend AutoPatch with new patch types</a></li>
<li><a href="#rollback">How to rollback patches</a></li>
<li><a href="#migration">How to migrate from AutoPatch 1.1 to AutoPatch 1.2</a></li>
</ol>


<p><hr width="50%"></hr></p>

<ol>

<li>
<p><a name="what"></a><b>What is AutoPatch?</b></p>

<p>AutoPatch is an automated Java patching system. Among other things, it
allows a Java application to easily do things like keep a database
schema in sync with what new application versions require, thus
radically lowering the cost of database maintenance.</p>

<p>Thought of abstractly, you can think of a "patch" as any change that
must be applied to any system external to your Java application.
AutoPatch is a system that automates the application of these patches.</p>

<p>That allows you to use rapid methodologies (e.g. XP, spiral development
model) all the way down to the alteration of external systems, without
needing an army of DBAs or similar to go around and patch your databases 
when you release new code requiring schema changes.</p>

<p>Owing to a semi-recent name change, you will find uses of the word
"migrate" or "migration" everywhere. Please consider the words
"migrate" and "migration" as synonyms for "patch".</p>
</li>


<li>
<p><a name="requirements"></a><b>Design Requirements</b></p>

<p>AutoPatch was designed with a few very simple requirements in mind.</p>

<ul>
  <li>Patches should execute before any server logic
    <ul>
      <li>This is important, or the application logic couldn't trust the
       state of the external systems</li>
    </ul>
  </li>
  <li>AutoPatch must enforce a total natural ordering on all patches.
    <ul>
       <li>This allows a developer to depend on the results of previous
       patches, for instance a patch can use database tables definied 
       in patches that were executed previously</li>
    </ul>
  </li>
  <li>AutoPatch must ensure that each patch is applied once and only once
    <ul>
       <li>Having them apply multiple times would mean the patch author
       could never trust the initial state of the database, making
       patch development very difficult</li>
    </ul>
  </li>
  <li>AutoPatch should be able to globally order patches that execute
       against multiple external systems</li>
  <li>AutoPatch should be capable of executing post-patching maintenance
       after patching (things like recompiling pl/sql, etc)</li>
   
</ul>
</li>

<li>
<p><a name="basic_design"></a><b>Basic Design</b></p>


<p>Given the AutoPatch design requirements, a very simple design has been
created. There is a patch launcher, which serves as the controller
for all patch activity.</p>

<ol>
  <li><p><b>Startup</b></p>
  <p>Applications wishing to use AutoPatch must instantiate a
MigrationLauncher by implementing an adapter that starts the launcher
on application startup, or using an existing adapter.</p>
  </li>
  <li><p><b>Patch Level</b></p>
  <p>The launcher is able to determine the current patch level of the system
in a durable way, so that it knows the patch level across multiple runs.</p>
  </li>
  <li><p><b>Patch Loading</b></p>
  <p>The launcher searches the classpath for any available objects that
implement a common patch type, or any sql files that are in a
directory known to hold patches.</p>
  </li>
  <li><p><b>Patch Ordering</b></p>
  <p>The launcher loads all of the patches into a single list and orders them.</p>
  </li>
  <li><p><b>Patch Execution</b></p>
  <p>Each patch is executed, one after another. Any errors are considered
fatal, as the application logic will not be able to trust the state of
external systems, and is thus cause for total shutdown.</p>
  </li>
  <li><p><b>Post-Patch Tasks</b></p>
  <p>These have loading, ordering and execution steps similar to regular patches,
the only difference is that these run *every* time AutoPatch runs. This allows
you to do maintenance tasks (like recompiling PL/SQL or similar) after patch
runs, but means that the tasks in here *must* be re-runnable. They will run a lot.</p>
</ol>
</li>

<li>
<p><a name="detailed_design"></a><b>Detailed Design</b></p>

<p>Expanding on the Basic Design with implementation-specific details,
this section describes the current implementation of AutoPatch, with
references to actual classnames.</p>

<ol>
<li><p><b>Startup</b></p>
<p>The MigrationLauncher object is the main entry point of the AutoPatch
system. There are a few adaptors that have been implemented, one for web
application integration (WebAppMigrationLauncher), one for
command-line integration (StandaloneMigrationLauncher), as well as
a few for depencency injection containers (AutoPatchService). There is also
a "distributed" version of each launcher that allows you to configure
multiple sub-launchers together as one large system orchestrated as a single
unit with a single patch level.</p>

<p>The sole purpose of the adaptors is to instantiate the MigrationLauncher object,
and call the "doMigrations" method.</p>

<p>Once the StandaloneMigrationLauncher is executing, it will read a configuration
file from the classpath called migration.properties. An example
migration.properties is provided in the same directory as this
README. The configuration file is responsible for configuring the
database connection for the AutoPatch system, as well as for
specifying where in the classpath AutoPatch should search for patches.</p>

<p>Other MigrationLaunchers are configured in different ways, and you should
consult the javadoc information in the adapter that you wish to use.</p>
</li>

<li><p><b>Patch Level</b></p>

<p>Once the MigrationLauncher has started executing, and finished reading
in its configuration file, it will use the database parameters to
configure the PatchTable object. PatchTable will inspect the database
and attempt to get the current patch level of the system. If no patch
table is present, it will create one (named 'patches').</p>

<p>Currently, the configuration file specifies a "database type", and
the name of the type specifies which SQL property file to load, so
PatchTable can interact with multiple database types. For example, if
the type is "postgres", PatchTable will attempt to load the SQL
property file "postgres.properties" and use the SQL in that file to
interact with the database when persisting patch level information.</p>
</li>

<li><p><b>Patch Loading</b></p>

<p>Once the patch level is determined, the colon-separated patch path
specified in the configuration file is searched for all objects that
implement the MigrationTask type. Each directory in your classpath has
each part of the patch-path appended to it, and each of these combinations
is searched for patches.</p>

<p>There are a couple of MigrationTask sub-classes that are useful to extend, 
and in practice all patches use the sub-classes.</p>

<p>The first sub-class is the SqlScriptMigrationTask. It allows for raw
text files containing SQL to be used as migrations, so long as they
follow the naming convention 'patchNNNN_<name>.sql'
(e.g. 'patch0001_create_initial_schema.sql'), where the numbers are
globally unique among all patches.</p>

<p>The second sub-class is the SqlLoadMigrationTask, which allows you to
quickly implement domain data loads with the use of tab-delimited text
files and a sub-class that understands how to insert one row of the
data at a time.</p>

<p>The final sub-class currently implemented is
MigrationTaskSupport. This is a more basic patch object, and allows
for any arbitrary logic to be implemented in Java and executed under 
the guise of a patch. It is potentially very useful to perform a data 
migration after one patch adds new tables and columns, but before another 
patch removes other tables or columns.</p>
</li>

<li><p><b>Patch Ordering</b></p>

<p>Its important to realize that patches must have a total global ordering -
meaning that each patch must be associated with a unique level.</p>

<p>Java patches get the opportunity to specify their own level, but for
the raw sql patches, ordering is based on the naming convention
specified above. This may seem cavalier at first, but its simple and easy
to do in practice so long as you check the current patch level before
creating a new patch, and coordinate with your fellow patch authors if
there are multiple developers working on a system.</p>

<p>To determine the current patch level easily, its possible to execute 
the MigrationInformation object as a standalone Java application, 
which will tell you the current patch level in the database, and the 
current maximum patch level implemented by MigrationTasks in the classpath. 
A suggested ant target to implement this check looks like this:</p>

<p><pre><code>
  &lt;target name="patch.information" description="Gets patch information from code and the database"&gt;
    &lt;java
        fork="true"
        classpathref="inttest.classpath"
        failonerror="true" 
        classname="com.tacitknowledge.util.migration.jdbc.MigrationInformation"&gt;
      &lt;sysproperty key="migration.systemname" value="${application.name}"/&gt;
    &lt;/java&gt;
  &lt;/target&gt;
</code></pre></p>

<p>The classpath must contain the migration.properties, the AutoPatch
jar, as well as all of the patch objects, for the information to be
accurate.</p>

<p>For distributed systems, it is important to note that you may not have
two patches with the same patch number in multiple sub-launchers. Each
patch must have a globally unique patch level across all sub-launchers.</p>
</li>

<li><p><b>Patch Execution</b></p>

<p>Patch execution is relatively straightforward, except for the
possibility that application servers in a cluster may all start at the
same time, and attempt to execute patches at the same time.</p>

<p>In order to ensure that each patch only executes once, the PatchTable
object has logic that locks the patch state table ('patches'),
guaranteeing that multiple servers in a cluster will serialize through
the AutoPatch part of their startup, and thus insuring that the set of 
patches is only executed once.</p>

<p>Other than that, each patch is executed once, with the system patch
level incremented for each successful patch application. Any
unsuccessful patch application will immediately result in voluminous
logging and fatal errors propoaged to the adapter that started AutoPatch, 
as this is a very serious problem and would prevent the correct execution 
of the application, and urgent troubleshooting will usually be required.</p>
</li>
</ol>
</li>

<li>
<p><a name="application_integration"></a><b>How to integrate AutoPatch with a web application</b></p>

<p>The easiest way to integrate AutoPatch with a web application is to
add the WebAppMigrationLauncher as a ContextListener for your
application context. This can be done with a web.xml snippet that
looks like this:</p>

<p><pre><code>
    &lt;!-- in the context-params section --&gt;
    &lt;context-param&gt;
        &lt;param-name&gt;migration.systemname&lt;/param-name&gt;
        &lt;param-value&gt;mysystemname&lt;/param-value&gt;
    &lt;/context-param&gt;

    &lt;!-- in the listeners section --&gt;
    &lt;listener&gt;
        &lt;listener-class&gt;com.tacitknowledge.util.migration.jdbc.WebAppMigrationLauncher&lt;/listener-class&gt;
    &lt;/listener&gt;
</code></pre></p>

<p>"migration.systemname" is used to differentiate different systems
storing their state in the same database (and thus the same patches
table). It also used as a prefix for all migration.properties property
keys.</p>

<p>So long as migration.properties, the AutoPatch library, , its dependent libraries
(from the lib directory of the AutoPatch distribution) and all of
your patches (with their paths specified in migration.properties) are
in the web application's classpath, AutoPatch should work fine.</p>

<p>There is also a way to configure your web 
application.  You can use JNDI to supply a data source and eliminate the need for a 
migration.properties file altogether.  Do this by adding the following to your 
web.xml file: </p>

<p><pre><code>
    &lt;!-- in the context-params section --&gt;
    &lt;context-param&gt;
        &lt;param-name&gt;migration.systemname&lt;/param-name&gt;
		&lt;param-value&gt;beekeeper&lt;/param-value&gt;
    &lt;/context-param&gt;
    &lt;context-param&gt;
        &lt;param-name&gt;migration.databasetype&lt;/param-name&gt;
		&lt;param-value&gt;mysql&lt;/param-value&gt;
    &lt;/context-param&gt;
    &lt;context-param&gt;
        &lt;param-name&gt;migration.patchpath&lt;/param-name&gt;
		&lt;param-value&gt;patches:com.stylehive.profile.db.patch&lt;/param-value&gt;
    &lt;/context-param&gt;
    &lt;context-param&gt;
        &lt;param-name&gt;migration.datasource&lt;/param-name&gt;
		&lt;param-value&gt;jdbc/profile&lt;/param-value&gt;
    &lt;/context-param&gt;
	
	&lt;!-- in the listeners section --&gt;
	&lt;listener&gt;
        &lt;listener-class&gt;com.tacitknowledge.util.migration.jdbc.WebAppJNDIMigrationLauncher&lt;/listener-class&gt;
    &lt;/listener&gt;
</code></pre></p>

<p>Note that there are similar parameters supplied in the servlet context that once 
existed in the migration.properties file.  All database connection properties have 
been replaced by a parameter called migration.datasource.  This uses an existing 
datasource to perform the autopatch updates.</p>
</li>

<li>
<p><a name="development_integration"></a><b>How to integrate AutoPatch with your development cycle</b></p>

<p>Its possible to run AutoPatch completely from the command-line, so
that patch development and deployment may take place without need for
a web application container.</p>

<p>The StandaloneMigrationLauncher is used for this, and an ant task to
run this would look like this:</p>

<p><pre><code>
  &lt;target name="patch.database" depends="compile" description="Runs the patch system"&gt;
    &lt;java
        fork="true"
        failonerror="true" 
        classname="com.tacitknowledge.util.migration.jdbc.StandaloneMigrationLauncher"&gt;
      &lt;classpath refid="inttest.classpath"/&gt;
      &lt;sysproperty key="migration.systemname" value="${application.name}"/&gt;
    &lt;/java&gt;
  &lt;/target&gt;
</code></pre></p>

<p>If you combine this rule with the patch.information ant target given
above, it should be relatively easy to point at a database, determine
its current patch level in a non-destructive way, and then execute patches.</p>
</li>

<li>
<p><a name="patch_implementation"></a><b>How to implement a patch</b></p>

<ul>
<li><p>SQL data load patch</p>

<p>A SQL data load patch can be implemented by extending
SqlLoadMigrationTask and including the delimited data file...</p>
</li>

<li><p>SQL patch</p>

<p>SQL patches should be implemented by simply following the SQL file
naming convention outlined above as you create the new SQL patch, 
putting the SQL commands in the patch file, and then by putting the 
patches in the classpath in a location specified in the AutoPatch 
configuration file.</p>
</li>

<li><p>Java patch</p>

<p>Java patches may be implemented by subclassing MigrationTaskSupport.</p>
</ul>
</li>

<li>
<p><a name="extension"></a><b>How to extend AutoPatch with new patch types</b></p>

<ul>
<li><p>Adding New Task Types</p>

<p>By examining SqlScriptMigrationTask, you should be able to get some
ideas on how to extend the system. There's no reason why there
couldn't be an LdapScriptMigrationTask that read .ldif files, or an
XmlScriptMigrationTask that issued SOAP or XML-RPC calls. As long as they
allow themselves to be compared such that they fit into the natural patch
ordering, and errors are propagated out after logging, they will work fine.</p>
</li>

<li><p>Patch Information Persistence</p>

<p>Currently the patch level of the system is stored in a database, however
the idea of the "system patch level" is a separate subsystem in AutoPatch, and
its not necessary for the patch table to be persisted in a
database. Its entirely possible that a flat file would be sufficient
for some applications, or that LDAP or a web server was the only
external system with persistence.</p>
</li>
</ul>
</li>

<li><a name="rollback"></a><p>How to rollback patches</p>

<p>One of the new features in AutoPatch 1.1 is that patches can now be rolled back. 
A rollback is when the patch level of a system is reduced. AutoPatch can execute 
rollback anywhere from one patch to all the patches on a system. All you have to do 
is let AutoPatch know what patch level you'd like to rollback to and then AutoPatch 
will do the rest. </p>

<p>To execute a rollback, reuse the StandaloneMigrationLauncher and specify a few 
additional parameters.  These are: -rollback to indicate that the desired migration
is a rollback and an integer indicating the desired patch level. So, to run a rollback
from the command line java StandaloneMigrationLauncher -rollback 5.  This command
would instruct AutoPatch to rollback the system to patch level 5. Additionally,
there is a -force parameter.  This parameter tells AutoPatch to ignore a check 
to see if all patches are rollbackable and to force a rollback.  Note that forcing
a rollback is dangergous as it can quite easily cause an exception scenario.</p>

<p>Similar to the patch process, applications that use rollbacks need to instantiate a 
MigrationLauncher. You can accomplish this by implementing an Adapter that executes upon 
application startup or using one of the provided launchers.  Instead of calling doMigrations, 
the Launcher should call doRollbacks.  This method takes in an int and a boolean.  
The integer indicates the level that the system should rollback to and the boolean 
indicates if the system should attempt to force a rollback.  Why would you need to force 
a rollback?  Because the people may have implemented their own tasks by implementing the 
MigrationTask interface, it is possible that there are migrations that do not currently 
support rollbacks.  Moreover, there are "destructive" migrations that are fundamentally not 
rollbackable. By default, a rollback will only occur if all of the rollback tasks are rollbackable.  
This check happens before any of the rollbacks are executed, so if you attempt to initiate a 
rollback for a set of tasks and one of the tasks is not rollbackable, no rollbacks will execute.  
The logs generated by AutoPatch will indicate which tasks are not rollbackable.  To provide the 
greatest level of flexibility, we have included a way of getting around this rule, which is using the 
"-force" flag.  This flag tells AutoPatch that it should ignore the rollbackable check and 
proceed with the rollback.  Please keep in mind that this is  risky as it's possible to try 
to rollback tasks that throw exceptions if a rollback is attempted.  The best option is to 
make your tasks adhere to the new RollbackableMigrationTask interface.  If you need to be able 
to force rollbacks, implement the down method to be a no-op method and then also set 
isRollbackSupported to true.  Following these steps will allow for AutoPatch to successfully 
execute the task as a rollback.</p>

<p>The important thing to recognize with patch levels is that a rollback will reduce the patch 
level.  For example, if you execute a rollback of patch level 5, the patch level of the 
system will be at patch level 4 after the rollback has completed.  Rollbacks are executed in 
a descending manner.
</p>

<p>The next step is to find the patches.  To make this easy, what we've done is deprecated the 
existing MigrationTask object and added a new interface called RollbackableMigrationTask.  Additionally, 
we've extended the functionality that loads SQL scripts into tasks to include rollback scripts.  
So, for your existing patches, you'll just need to create a new SQL script and make it available for 
AutoPatch to load.  Rollback scripts are named as follows:   patch&lt;patch level&gt;-rollback_&lt;patch name&gt;.sql.   
An example of a valid rollback file name is patch111-rollback_testname.sql.  In this case, the 111 is the level 
of the patch.  Conceptually, you should think of this script as inherently linked to the forward patch.  
The mechanism that associates a patch SQL script and a rollback SQL script is the level. 
So, patch111_testname.sql and patch111-rollback_testname.sql are linked.  Please note that the 
name of the patch does not have any role in linking a patch and its rollback.  
In fact, patch111.sql and patch111-rollback_foo.sql would be associated.</p>

<p>Just as with the up process for patching, the ordering in which rollbacks execute is important. 
Autopatch executes rollbacks in a descending manner based upon the patch level. So, if you were to 
issue a rollback to level 10 and the system is at level 13, the patches that rollback would be 13, 12 and 11.</p>
</li>		
<li><a name="migration"></a><p>How to migrate from AutoPatch 1.1 to AutoPatch 1.2</p>
<p>AutoPatch 1.2 completely supports AutoPatch 1.0, but you'll need to make some changes if you'd like to 
take advantage of the new features.</p>
<p>If you have implemented the MigrationTask interface, you'll need to change the implementation to 
RollbackableMigrationTask.  This will require you to implement three new methods.  They are up, down and 
isRollbackSupported.  The up method performs the exact same purpose as migrate in MigrationTask.   
That is, it performs the actual migration.  For simplicity, you can implement up with one line to just 
call migrate.  The down method performs a rollback, and the isRollbackSupported method indicates if 
this task can be rolled back.  If this method returns false, then the task will not rollback unless the 
force flag is set to true.</p>
<p>If you have extended MigrationTaskSupport, there is actually very little to do.  The MigrationTaskSupport 
object already provides a lot of base functionality. By default, MigrationTaskSupport provides the following 
functionality.  First, the class provides a few default implementations.   isRollbackSupported returns the 
value of a boolean variable isRollbackSupported.  Note that by default this value is initialized false.  
Secondly, the class provides a base implementation of down, which simply throws UnsupportedOperationException.  
The migrate method simply calls up and the up method is a default no op method.  To migrate to AutoPatch 1.2,  
simply do the following:  override the up and down methods to perform the migration and rollback and then 
secondly set isRollBackSupported to true if appropriate.  Once you've taken these steps, then your task
will be Rollbackable.
</p>
</li>
</ol>

<?php include("includes/footer.inc"); ?>

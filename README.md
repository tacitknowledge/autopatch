AutoPatch
=========

AutoPatch automates the application of changes to persistent storage.

AutoPatch was born from the needs of using an agile development process
while working on systems that have persistent storage. Without
AutoPatch, developers usually can't afford the maintenance headache of
their own database, and DBAs are required just to apply changes to all
of the various environments a serious development effort requires.

The very application of database changes becomes an inefficient,
error-prone, expensive process, all conspiring to discourage any
refactoring that touches the model, or being a bottleneck when model
changes are made.

AutoPatch solves this problem, completely.

With AutoPatch, an agile development process that requires a database
change looks like this:

* Developer alters the model, which requires a change to the database
* Developer possibly consults a DBA, and develops a SQL patch against
  their personal database that implements the alteration
* Developer commits the patch to source control at the same time as they
  commit their dependent code
* Other developers' and environments' databases are automatically updated
  by AutoPatch the next time the new source is run

This represents streamlined environment maintenance, allowing developers
to cheaply have their own databases and all databases to stay in synch
with massively lower costs and no environment skew.


Where do I get AutoPatch?
-------------------------
AutoPatch is open source and is hosted at [Github](http://github.com/tacitknowledge/autopatch).

The documentation for AutoPatch is on the [AutoPatch Wiki](https://github.com/tacitknowledge/autopatch/wiki)


How do I build AutoPatch?
-------------------------
We're in the process of submitting AutoPatch and associated projects to the Central Maven
Repository ([see ticket](https://github.com/tacitknowledge/autopatch/issues/1)).  Until
that occurs you'll need to download, build, and install the
[Discovery](https://github.com/tacitknowledge/discovery) project before building AutoPatch.

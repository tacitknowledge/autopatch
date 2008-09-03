------------------------------------------------------------------------------------------------------------------------
Maven Autopatch plugin
------------------------------------------------------------------------------------------------------------------------
Autopatch plugin allows you to apply database patches during any maven project lifetime cycle.

------------------------------------------------------------------------------------------------------------------------
External prerequisites
------------------------------------------------------------------------------------------------------------------------
Apart from maven and the plugin itself one has to make sure that autopatch lib is available in a local repository, or

Since autopatch is _not_ 'mavenized' (yet) you need to install the tk-autopatch and tk-util jars
into your local repository using the deploy plugin <http://maven.apache.org/plugins/maven-deploy-plugin/>.

Example:
mvn deploy:deploy-file \
-DgeneratePom=true \
-Dfile=./tk-util-1.0.1.jar \
-DgroupId=com.tacitknowledge.util \
-DartifactId=tk-util \
-Dversion=1.0.1 \
-Dpackaging=jar \
-Durl=scp://<YOUR REPOSITORY HOSTNAME>/<PATH TO YOUR REPO> \
-Duser=<SSH USER> \
-Dpassword=<SSH PASSWORD>

mvn deploy:deploy-file \
-DpomFile=../migrate/pom.xml \
-Dfile=../migrate/build/tk-autopatch-1.0.0.jar \
-DgroupId=com.tacitknowledge.autopatch \
-DartifactId=tk-autopatch -Dversion=1.0.0 \
-Dpackaging=jar \
-Durl=scp://<YOUR REPOSITORY HOSTNAME>/<PATH TO YOUR REPO> \
-Duser=<SSH USER> \
-Dpassword=<SSH PASSWORD>

One other thing of note, is that to publish this to a maven repository, at least at one of our client's sites,
you should run a command like this:

<maven home>/maven/trunk/maven-2.0.4/bin/mvn \
deploy:deploy-file \
-DpomFile=pom.xml \
-Dfile=target/tk-autopatch-maven-plugin-1.0-SNAPSHOT.jar \
-DgroupId=com.tacitknowledge.autopatch \
-DartifactId=tk-autopatch-maven-plugin \
-Dversion=1.0-SNAPSHOT \
-Dpackaging=jar \
-Durl=scp://<repo hostname>/<repo path> \
-DrepositoryId=<repo id> \
-Duser=<repo user> \
-Dpassword=<repo pass>

------------------------------------------------------------------------------------------------------------------------
Description
------------------------------------------------------------------------------------------------------------------------

------------------------------------------------------------------------------------------------------------------------
Goals
------------------------------------------------------------------------------------------------------------------------
patch - Goal which applies patches found in the classpath
info - Goal which provides patch information.
unlock -  Goal which forcibly unlocks a patch table that has an orphan lock.

Next goals work for multiple system names at once, you should specify orchestration patch store system name.
distributed-patch - Goal which applies patches found in the classpath.
distributed-info -  Goal which provides patch information.
distributed-unlock - Goal which forcibly unlocks a patch table that has an orphan lock.

------------------------------------------------------------------------------------------------------------------------
Properties
------------------------------------------------------------------------------------------------------------------------
skip -- skips the execution. Default to false. TODO: implement

classpathElements -- a list specifying classpath folders where plugin will look for patches.
    default value="${project.compileClasspathElements}"

migrationSettings -- the migration settings file. defaults to "migration.properties"

protected String systemName -- The system to get patch information about

------------------------------------------------------------------------------------------------------------------------
Examples
------------------------------------------------------------------------------------------------------------------------

------------------------------------------------------------------------------------------------------------------------
Command line
------------------------------------------------------------------------------------------------------------------------

------------------------------------------------------------------------------------------------------------------------
POM usage:
------------------------------------------------------------------------------------------------------------------------

<properties>
<autopatch.system.name>integration_test</autopatch.system.name>
<autopatch.migration.settings></autopatch.migration.settings>
</properties>


<plugin>
    <groupId>com.tacitknowledge.autopatch</groupId>
    <artifactId>tk-autopatch-maven-plugin</artifactId>
    <version>1.0-SNAPSHOT</version>
    <configuration>
        <!-- The location of the patches -->
        <classpathElements>
            <param>${basedir}/target/test-classes</param>
        </classpathElements>
        <!-- Custom migration settings file -->
        <migrationSettings>migration-distributed.properties</migrationSettings>
        <!-- The system name to run tests on -->
        <systemName>integration_test</systemName>
    </configuration>
    <executions>
      <execution>
        <phase>unlock</phase>
        <goals>
          <goal>distributed-info</goal>
        </goals>
      </execution>
    </executions>
</plugin>


<build>
    <plugins>
      <plugin>
        <groupId>com.tacitknowledge.autopatch</groupId>
        <artifactId>tk-autopatch-maven-plugin</artifactId>
        <version>1.0-SNAPSHOT</version>
        <executions>
          <execution>
            <phase>install</phase>
            <goals>
              <goal>distributed-patch</goal>
            </goals>
          </execution>
        </executions>
      </plugin>
    </plugins>
</build>





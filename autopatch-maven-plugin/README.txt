Since autopatch is _not_ 'mavenized' (yet) you need to install the tk-autopatch and tk-util jars 
into your local repository using the deploy plugin <http://maven.apache.org/plugins/maven-deploy-plugin/>.

Example:
mvn deploy:deploy-file -DgeneratePom=true -Dfile=./tk-util-1.0.1.jar -DgroupId=com.tacitknowledge.util
-DartifactId=tk-util -Dversion=1.0.1 -Dpackaging=jar 
-Durl=scp://<YOUR REPOSITORY HOSTNAME>/<PATH TO YOUR REPO>  
-Duser=<SSH USER> -Dpassword=<SSH PASSWORD>

mvn deploy:deploy-file -DpomFile=../migrate/pom.xml -Dfile=../migrate/build/tk-autopatch-1.0.0.jar -DgroupId=com.tacitknowledge.autopatch
-DartifactId=tk-autopatch -Dversion=1.0.0 -Dpackaging=jar 
-Durl=scp://<YOUR REPOSITORY HOSTNAME>/<PATH TO YOUR REPO>  
-Duser=<SSH USER> -Dpassword=<SSH PASSWORD>


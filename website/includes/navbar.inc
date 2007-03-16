<?php

function navbar()
{
	# Get the current page
	$current_heading = func_get_arg(0);

	# This lists all of our possible pages
	$content = array(
			"Home" => "index.php", 
			"About Us" => "about_us.php", 
			"Contact Us" => "contact_us.php",
			"Documentation" => "documentation.php", 
			"FAQ" => "faq.php", 
			"Links" => "links.php",
			"Other Languages" => "other_languages.php",
			"Quick Start" => "quick_start.php",
			"How to Help" => "how_to_help.php",
			"SourceForge Download" => "https://sourceforge.net/project/showfiles.php?group_id=106637"
			
	);

	$content_headings = array_keys($content);
	$content_links = array_values($content);


	print "\t<div id=\"left\">\n\t\t<div id=\"menu\">\n\t\t\t<ul>\n";

	for ($i = 0; $i < count($content_headings); $i++)
	{
		# Start the navbar link
		print "\t\t\t\t<li><a ";

		# Is this the current page? If so, highlight it
		if (strcmp($content_headings[$i], $current_heading) == 0)
		{
			print "class=\"active\" ";
		}
		
		# finish the navbar link
		print "href=\"" . $content_links[$i] . "\"><span class=\"navpad\">" . $content_headings[$i] . "</span></a></li>\n";
	}
    
       	print "\t\t\t</ul>\n\t\t</div><!-- /menu -->\n\n";
}

?>

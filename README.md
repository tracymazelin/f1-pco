Fellowship One to Planning Center Online Sync.
=

Demo at http://f1topco.apphb.com

This ASP.NET MVC web app uses the [Fellowship One API](http://developer.fellowshipone.com) and [Planning Center Online API](http://get.planningcenteronline.com/api/general-details/) to sync demographic data from F1 to PCO.  Currently this is a one way sync only.

Requirements
-

+  ASP.NET MVC 2.0
+  MS SQL Server 2008 

Config
-
The database script for the application is located in the scripts directory.  It includes tables and procedures for the application as well as for the [ELMAH error logging](http://code.google.com/p/elmah/) component.

You will need to apply for a key for the [Fellowship One API](http://developer.fellowshipone.com/index.php/key/) as well as the [Planning Center Online API](http://get.planningcenteronline.com/api/general-details/).  Once you have your consumer keys and secrets you will need to add them to the web.config for the application.  There are settings for both the key and secret for both APIs.

In order for the application to know which users it needs to sync an attribute with the name of "SyncMe" needs to be added to each individual record in Fellowship One.  After the initial sync the Planning Center Online ID will be written as data to the attribute for the F1 individual to make syncing easier on subsequent runs.

Workflow
-

The application will query the Fellowship One API for all individuals that contain the "SyncMe" attribute.  It will then take this collection of individuals and query the PCO API in an effort to find matches.  This search is done my name and email address. If a direct match is found then name, address and communication values from F1 are synced to PCO.  If multiple matches are found, the user is presented with a screen to select the correct match or create a new use in PCO.  If no match is found then a new person is created in PCO.
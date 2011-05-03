<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<div id="instructions">
    In order for you church to be able to sync data between Fellowship One and Planning Center online, you need to follow these setup intructions:

    <h1>1. Enable access to the Fellowship One/Planning Center Online application.</h1>
    <p>
        As part of Fellowship One's security control's you must explicitly allow access to you churches database by a third party application.
    </p>
        <ul>
            <li>Login to F1 as an administrator.</li>
            <li>Click Admin and then under Integration section click Applications.</li>
            <li>You’ll be presented with a list of 1st, 2nd and 3rd party applications available for your church. Scroll down to 3rd party apps and click on the Fellowship One for Android application.
            <img src="Content/images/instructions_1.png" width="630" /></li>
            <li>If the application is not already enabled, click on the button that says Grant Access. If you have already enabled access to the application and now want to revoke access, click on the Revoke Access button.</li>
        </ul>
    <h1>2. Create new "SyncMe" attribute</h1>
    <p>
        You'll need to create a new attribute called "SyncMe" that will be assigned to all individuals that you want to sync with Planning Center Online.
    </p>
    <ul>
        <li>Login to F1 as an administrator.</li>
        <li>Click Admin and then under People Setup section click <b>Individual Attributes</b>.</li>
        <li>Type “Planning Center Online” in the field for <b>Attribute group name</b> and then click <b>Add attribute group</b>.</li> 
        <li>Click on the tab <b>Individual Attributes</b>, type "SyncMe" (with no quotes and no space between the words) in the field for Individual attribute name, check off <b>Record comment</b> & <b>Record end date</b> and then click <b>Add individual attribute</b>.
        <img src="Content/images/instructions_2.png" />
        </li>
    </ul>
    <h1>3. Add "SyncMe" attribute to individuals</h1>
    <p>
        Finally, now that you have created the SyncMe attribute you need to assign it to all the individuals that you want to sync with Planning Center Online.
    </p>
    <ul>
        <li>Search for an individual in Fellowship One and goto the Individual detail record.</li>
        <li>Under the attributes section click the green + sign</li>
        <li>Choose the "SyncMe" attribute and click Add Attribute (do not put anything in comment or end date).</li>
    </ul>   
</div>

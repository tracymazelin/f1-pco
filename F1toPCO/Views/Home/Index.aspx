<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Fellowship One to Planning Center Online Sync
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="frontimage">
        <img src="Content/images/F1PCO.png" style="display:block;margin:auto;">
    </div>
    <div>
        <h2>
            Two great things that go great together!
        </h2>
        <p style="font-size:1.2em;">
            Finally, you can now sync your Fellowship One data with Planning Center Online keeping your volunteers contact data up to date with the click of a button allowing you
            to further streamline you minitry tasks.  If you've already setup your church, the click the Get Started button below.  Otherwise follow the instructions to setup
            Fellowship One in order to allow for syncing.
        </p>
    </div>
    <hr/>
    <div>
        <div id="getstarted" >
            <h1>
                If you've already setup your church to be able to sync then you are ready to go!
            </h1>
            <div style="display:block;text-align:center;width:100%">
                <a href="ChurchCode" class="myButton">Get Started</a>
            </div>
        </div>
        <% Html.RenderPartial("_Instructions"); %>
        <p>
            Now you should be ready to go!  Click the get started button above.
        </p>
    </div>
    <div class="clear"></div>
</asp:Content>

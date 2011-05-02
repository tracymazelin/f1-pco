<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Fellowship One to Planning Center Online Sync
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="frontimage">
        <img src="/F1toPCO/Content/images/F1PCO.png">
        <div>
            <h1>
                Two great things that go great together!
            </h1>
            <p>
                Finally, you can now sync your Fellowship One data with Planning Center Online keeping you volunteers contact data up to date with the click of a button.
            </p>
        </div>
        <div>
            <div style="float:right;width:25%">
                <p>
                    If you've already setup your church to be able to sync then you are ready to go!
                </p>
                <a href="ChurchCode" class="myButton">Get Started</a>
            </div>
            <div style="float:left">
                <p>
                    instructions
                </p>
            <div>
        </div>
        <div class="clear"></div>
    </div>
</asp:Content>

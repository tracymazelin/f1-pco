<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<System.Web.Mvc.HandleErrorInfo>" %>

<asp:Content ID="errorTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Error
</asp:Content>

<asp:Content ID="errorContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Oh noes, teh error!</h2>
    <p>An error occurred while syncing people.  The error has been logged and a team of hamsters are hard at work to determine
    the issue.  We are sorry for the inconvenience.
    </p>
</asp:Content>

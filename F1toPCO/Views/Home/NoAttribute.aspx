<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	No Attribute
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

   <h2>Oh Noes!</h2>
   <p>  
        We could not find a "SyncMe" attribute for your church.  If we can't find the "SyncMe" attribute then we don't know who 
        needs to be synced with Planning Center Online.
    </p>
    <p>
        Follow these instructions and then try again:
    </p>
    <% Html.RenderPartial("_Instructions"); %>
</asp:Content>

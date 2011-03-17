<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<results>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	StartSync
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>StartSync</h2>
    <% foreach (person p in Model.Person) {  %>
        <%= p.firstName %><br />
    <% } %>
</asp:Content>

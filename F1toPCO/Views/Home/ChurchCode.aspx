<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	F1toPCO - ChurchCode
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>ChurchCode</h2>
     <% using (Html.BeginForm(FormMethod.Post)) { %>
        Church Code: <%= Html.TextBox("ChurchCode") %>
        <br/>
        <input type=submit class="myButton" value="Login">
    <% } %>
</asp:Content>

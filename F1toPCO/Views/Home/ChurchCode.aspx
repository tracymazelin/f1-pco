<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Church Code
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div>
     <% using (Html.BeginForm(FormMethod.Post)) { %>
        <p>
            Please enter your church code: <%= Html.TextBox("ChurchCode") %>
        </p>
        <p>
            <input type=submit class="myButton" value="Login">
        </p>
    <% } %>
    </div>
</asp:Content>

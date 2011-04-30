<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<List<F1toPCO.Model.F1.person>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Success
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Success</h2>
    <div>
        Congratulations.  You have successfully synced your data from Fellowship One to Planning Center online.
    </div>
    <br/>
    <% if (Model.Count > 0) { %>
    <div id="errors">
        <p>
        The following people had errors when trying to sync:
        </p>
        <% foreach(F1toPCO.Model.F1.person p in Model) { %>
             <%= p.firstName %> <%=p.lastName %><br/>
        <% } %>
    </div>
    <% } %>

</asp:Content>

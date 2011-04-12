<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<List<F1toPCO.Model.MatchHelper>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	NoMatch
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% using (Html.BeginForm("ProcessNoMatches", "Home", FormMethod.Post)) { %>
    <table border="0" padding="5">
    <%foreach (F1toPCO.Model.MatchHelper m in Model) { %>
        <tr>
            <td>
                <b><%=m.F1Person.firstName %>&nbsp;<%=m.F1Person.lastName %><b><br/>
                <%= m.F1Person %>
            </td>
            <td>
               <%= Html.RadioButton("SyncIt-" + m.F1Person.id.ToString(), 1, true, new { id = "SyncIt-" + m.F1Person.id.ToString() })%><label for="SyncIt-" + m.F1Person.id.ToString()>Sync It</lablel>
               <%= Html.RadioButton("SyncIt-" + m.F1Person.id.ToString(), 0, false, new { id = "SyncIt-" + m.F1Person.id.ToString() })%><label for="SyncIt-" + m.F1Person.id.ToString()>Remove</lablel>
            </td>
       </tr>
      <% } %>
      </table>
      <input type="submit" value="Save changes">
      <% } %>
</asp:Content>

<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<F1toPCO.Model.MatchHelper>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	No Match Found
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="instructions">
        The following individual(s) have the sync attribute in Fellowship One but do not exist in Planning Center Online.  Please select whether you
        would like to add the individual to Planning Center Online or remove the sync attribute from Fellowship One.
    </div>
    <% using (Html.BeginForm("ProcessNoMatches", "Home", FormMethod.Post)) { %>
    <table border="0" padding="5">
    <%foreach (F1toPCO.Model.MatchHelperData m in Model) { %>
        <tr>
            <td>
                <% Html.RenderPartial("_F1Person", m.F1Person); %>
            </td>
            <td>
                <div>
                    <%= Html.RadioButton("SyncIt-" + m.F1Person.id.ToString(), 1, true, new { id = "SyncIt-" + m.F1Person.id.ToString() })%>
                    <label for="SyncIt-" + m.F1Person.id.ToString()>Add to Planning Center Online</lablel>
                </div>
                <div>
                    <%= Html.RadioButton("SyncIt-" + m.F1Person.id.ToString(), 0, false, new { id = "SyncIt-" + m.F1Person.id.ToString() })%>
                    <label for="SyncIt-" + m.F1Person.id.ToString()>Remove attribute from Fellowship One</lablel>
                </div>
            </td>
       </tr>
       <tr><td colspan="2"><hr/></td></tr>
      <% } %>
      </table>
      <input type="submit" value="Save changes">
      <% } %>
</asp:Content>

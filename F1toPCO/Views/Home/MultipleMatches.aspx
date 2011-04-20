<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<F1toPCO.Model.MatchHelper>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Matches
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% using (Html.BeginForm("ProcessMatches", "Home", FormMethod.Post)) { %>
    <table>
       <%foreach (F1toPCO.Model.MatchHelperData m in Model) { %>
       <tr>
            <td colapsn="2">
                <% Html.RenderPartial("_F1Person", m.F1Person); %>
            </td>
            <td>
            <%= Html.RadioButton(m.F1Person.id, -1, false, new { id = m.F1Person.id }) %><label for="<%= m.F1Person.id %>">Create new person</label><br/>
            <table>
            <%foreach (F1toPCO.Model.PCO.person p in m.PCOPeople.person) { %>
                <tr>
                    <td>
                        <%= Html.RadioButton(m.F1Person.id, p.id.Value, false, new { id = m.F1Person.id }) %>
                        <% Html.RenderPartial("_PCOPerson", p); %>
                    </td>
                </tr>
            <% } %>
            </table>
            <%= Html.RadioButton(m.F1Person.id, 0, false, new { id = m.F1Person.id }) %><label for="<%= m.F1Person.id %>">Remove</label>
            </td>
       </tr>
       <% } %>
       <tr><td colspan="2"><input type="submit" value="Save"></td></tr>
       </table>
       <% } %>
</asp:Content>
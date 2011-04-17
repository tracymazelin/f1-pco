<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<F1toPCO.Model.MatchHelper>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Matches
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% using (Html.BeginForm("ProcessMatches", "Home", FormMethod.Post)) { %>
    <table>
       <%foreach (F1toPCO.Model.MatchHelperData m in Model) { %>
       <tr>
            <td>
                <b><%=m.F1Person.firstName%>&nbsp;<%=m.F1Person.lastName%><b><br/>
                <%= m.F1Person%>
            </td>
            <td>
            <%foreach (F1toPCO.Model.PCO.person p in m.PCOPeople.person) { %>
                <%= p.firstname%>&nbsp;<%=p.lastname%><br/>
                <%= Html.RadioButton(m.F1Person.id, p.id.Value, false, new { id = m.F1Person.id }) %><label for="<%= m.F1Person.id %>"><%=p.id.Value %></label><br/>
            <% } %>
            <%= Html.RadioButton(m.F1Person.id, 0, false, new { id = m.F1Person.id }) %><label for="<%= m.F1Person.id %>">Remove</label>
            </td>
       </tr>
       <% } %>
       <tr><td colspan="2"><input type="submit" value="Save"></td></tr>
       </table>
       <% } %>
</asp:Content>
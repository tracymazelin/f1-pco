<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<List<F1toPCO.Model.MatchHelper>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Matches
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
   <div style="width:50%">
       <%foreach (F1toPCO.Model.MatchHelper m in Model) { %>
       <div id="f1Person" style="float:left">
            <b><%=m.F1Person.firstName %>&nbsp;<%=m.F1Person.lastName %><b><br/>
            <%= m.F1Person %>
       </div>
       <div style="float:right">
            <%foreach (F1toPCO.Model.PCO.person p in m.PCOPeople.person) { %>
                <%= p.firstname %>&nbsp;<%=p.lastname %><br/>
            <% } %>
       </div>
       <% } %>
   </div>
</asp:Content>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="_F1Person.ascx.cs" Inherits="System.Web.Mvc.ViewUserControl<F1toPCO.Model.F1.person>" %>
<b><%= Model.firstName%>&nbsp;<%=Model.lastName%></b><br/>
<%= F1toPCO.Util.UIHelper.FormatAddress(Model.addresses.FindByType("Primary"))%><br/>
<b>Email</b>
<table width="100%" border="2">
    <% foreach (F1toPCO.Model.F1.communication c in Model.communications.FindByGeneralCommunicationType(F1toPCO.Model.F1.communicationGeneralType.Email)) { %>
        <tr>
            <td width="50%">
                <%= c.communicationType.name %>
            </td>
            <td width="50%">
                <%= c.communicationValue %>
            </td>
        </tr>
    <% } %>
</table>
<b>Phone</b>
<table width="100%" border="2">
    <% foreach (F1toPCO.Model.F1.communication c in Model.communications.FindByGeneralCommunicationType(F1toPCO.Model.F1.communicationGeneralType.Telephone)) { %>
        <tr>
            <td width="50%">
                <%= c.communicationType.name %>
            </td>
            <td width="50%">
                <%= c.communicationValue %>
            </td>
        </tr>
    <% } %>
</table>
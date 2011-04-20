<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="_PCOPerson.ascx.cs" Inherits="System.Web.Mvc.ViewUserControl<F1toPCO.Model.PCO.person>" %>
<b><%= Model.firstname%>&nbsp;<%=Model.lastname%></b><br/>
<%= F1toPCO.Util.UIHelper.FormatAddress(Model.contactData.addresses.FindByLocation("Home"))%><br/>
<b>Email</b>
<table width="100%" border="2">
    <% foreach (F1toPCO.Model.PCO.emailAddress c in Model.contactData.emailAddresses.emailAddress) { %>
        <tr>
            <td width="50%">
                <%= c.location %>
            </td>
            <td width="50%">
                <%= c.address %>
            </td>
        </tr>
    <% } %>
</table>
<b>Phone</b>
<table width="100%" border="2">
    <% foreach (F1toPCO.Model.PCO.phoneNumber c in Model.contactData.phoneNumbers.phoneNumber) { %>
        <tr>
            <td width="50%">
                <%= c.location %>
            </td>
            <td width="50%">
                <%= c.number %>
            </td>
        </tr>
    <% } %>
</table>
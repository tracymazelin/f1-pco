<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="_PCOPerson.ascx.cs" Inherits="System.Web.Mvc.ViewUserControl<F1toPCO.Model.PCO.person>" %>
<b><%= Model.firstname%>&nbsp;<%=Model.lastname%></b><br/>
<%var address = Model.contactData.addresses.FindByLocation("Home"); %>
<% if (address != null) {  %>
<div class="section">
    <h2>Address</h2>
    <div style="margin-left: 10px;">
        <%= F1toPCO.Util.UIHelper.FormatAddress(address)%><br/>
    </div>
</div>
<% } %>

<% if (Model.contactData.emailAddresses.emailAddress != null && Model.contactData.emailAddresses.emailAddress.Count > 0) {  %>
<div class="section">
    <h2>Email</h2>
    <table>
        <% foreach (F1toPCO.Model.PCO.emailAddress c in Model.contactData.emailAddresses.emailAddress) { %>
            <tr>
                <th>
                    <%= c.location %>
                </th>
                <td>
                    <%= c.address %>
                </td>
            </tr>
        <% } %>
    </table>
</div>
<% } %>

<% if (Model.contactData.phoneNumbers.phoneNumber != null && Model.contactData.phoneNumbers.phoneNumber.Count > 0) {  %>
<div class="section">
    <h2>Phone</h2>
    <table>
        <% foreach (F1toPCO.Model.PCO.phoneNumber c in Model.contactData.phoneNumbers.phoneNumber) { %>
            <tr>
                <th>
                    <%= c.location%>
                </th>
                <td>
                    <%= c.number%>
                </td>
            </tr>
        <% } %>
    </table>
</div>
<% } %>
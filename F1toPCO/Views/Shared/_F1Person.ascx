<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="_F1Person.ascx.cs" Inherits="System.Web.Mvc.ViewUserControl<F1toPCO.Model.F1.person>" %>
<h2>
    <%= Model.firstName%>&nbsp;<%=Model.lastName%>
</h2>
<% var address = Model.addresses.FindByType("Primary"); %>
<% if (address != null) {  %>
<div class="section">
    <h2>Address</h2>
    <div style="margin-left: 10px;">
        <%= F1toPCO.Util.UIHelper.FormatAddress(Model.addresses.FindByType("Primary"))%><br/>
    </div>
</div>
<% } %>

<% var emails = Model.communications.FindByGeneralCommunicationType(F1toPCO.Model.F1.communicationGeneralType.Email); %>
<% if (emails != null && emails.Count > 0) {  %>
<div class="section">
    <h2>Email</h2>
    <table>
        <% foreach (F1toPCO.Model.F1.communication c in emails) { %>
            <tr>
                <th>
                    <%= c.communicationType.name %>
                </th>
                <td>
                    <%= c.communicationValue %>
                </td>
            </tr>
        <% } %>
    </table>
</div>
<% } %>

<% var phones = Model.communications.FindByGeneralCommunicationType(F1toPCO.Model.F1.communicationGeneralType.Telephone); %>
<% if (phones != null && phones.Count > 0) %>
<div class="section">
    <h2>Phone</h2>
    <table>
        <% foreach (F1toPCO.Model.F1.communication c in phones) { %>
            <tr>
                <th>
                    <%= c.communicationType.name.Replace("Phone", "") %>
                </th>
                <td>
                    <%= c.communicationValue %>
                </td>
            </tr>
        <% } %>
    </table>
</div>
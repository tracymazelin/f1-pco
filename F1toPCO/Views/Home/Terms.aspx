<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Terms
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Terms of use</h2>
    <div>
        <p>By using the f1topco.funkypantssoftware.com web site (“Service”), all services of Funkypants Software, 
        you are agreeing to be bound by the following terms and conditions (“Terms of Service”).</p>

        <p>Funkypants Software reserves the right to update and change the Terms of Service from time to time without 
        notice. Any new features that augment or enhance the current Service, including the release of new tools and 
        resources, shall be subject to the Terms of Service. Continued use of the Service after any such changes shall 
        constitute your consent to such changes. You can review the most current version of the Terms of Service at any 
        time at: http://f1topco.funkypantssoftware.com/Terms.</p>

        <p>Violation of any of the terms below will result in your access to the Service being terminated.  You are also
        agreeing to be bound by the Terms of Use of the Planning Center Online website ("Planning Center Online") as well as the
        Fellowship One website ("Fellowship One") and a violation of any of the terms in said agreements will result in termination
        of you access to the Servce.  You agree to use the Service at your own risk.</p>
    </div>
    <h2>General Conditions</h2>
    <div>
    <ul>
        <li>Your use of the Service is at your sole risk. The service is provided on an “as is” and “as available” 
            basis.</li>       
        <li>You must not modify, adapt or hack the Service or modify another website so as to falsely imply that it 
            is associated with the Service, Funkypants Software, Ministry Centered Technologies, Fellowship Technologies 
            or any other service provided by said companies.</li>
        <li>You agree not to reproduce, duplicate, copy, sell, resell or exploit any portion of the Service, use of the 
            Service, or access to the Service without the express written permission by Funkypants Software.</li>                
        <li>You understand that the technical processing and transmission of the Service, including your Content, may 
            be transfered unencrypted and involve (a) transmissions over various networks; and (b) changes to conform 
            and adapt to technical requirements of connecting networks or devices.</li>        
        <li>Funkypants Software does not warrant that (i) the service will meet your specific requirements, (ii) the 
            service will be uninterrupted, timely, secure, or error-free, (iii) the results that may be obtained from 
            the use of the service will be accurate or reliable, (iv) the quality of any products, services, information, 
            or other material purchased or obtained by you through the service will meet your expectations, and (v) any 
            errors in the Service will be corrected.</li>
        <li>You expressly understand and agree that Funkypants Software shall not be liable for any direct, indirect, 
            incidental, special, consequential or exemplary damages, including but not limited to, damages for loss of 
            profits, goodwill, use, data or other intangible losses (even if Funkypants Software has been advised of 
            the possibility of such damages), resulting from: (i) the use or the inability to use the service; (ii) the 
            cost of procurement of substitute goods and services resulting from any goods, data, information or services 
            purchased or obtained or messages received or transactions entered into through or from the service; (iii) 
            unauthorized access to or alteration of your transmissions or data; (iv) statements or conduct of any third 
            party on the service; (v) or any other matter relating to the service.</li>
        <li>The failure of Funkypants Software to exercise or enforce any right or provision of the Terms of Service 
            shall not constitute a waiver of such right or provision. The Terms of Service constitutes the entire 
            agreement between you and Funkypants Software and govern your use of the Service, superceding 
            any prior agreements between you and Funkypants Software (including, but not limited to, any 
            prior versions of the Terms of Service).</li>
        </ul>
        <p>
            Questions about the Terms of Service should be sent to support@funkypantssoftware.com
        </p>
    </div>
    <p>  
        <a href="Authenticate" id="button" class="myButton">I agree</a>
        <a href="Index" id="cancel" class="myButton">Decline</a>
    </p>
</asp:Content>

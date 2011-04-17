<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	F1toPCO - Ready
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

     <p>
        We have all the information we need and are ready to go!
    </p>
    <span style="display:none">
        <img id="loading" src="/F1toPCO/Content/images/ajax-loader.gif"/> Processing.  Please wait...
    </span>
    <span>
        <a href="Sync" id="button" class="myButton">Lets Go!</a>
    </span>
    <script type="text/javascript">
        $(document).ready(function() {
            $('#button').click(function() {
                $("span").toggle();
            });
        });
       
    </script>
</asp:Content>

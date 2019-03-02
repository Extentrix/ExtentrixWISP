<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls"
    Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page Language="C#" EnableViewState="true" AutoEventWireup="true" CodeBehind="ContentRedirectionAdminPage.aspx.cs"
    Inherits="ExtentrixWebIntrface.Layouts.ExtentrixWebIntrface.Pages.ContetentRedirectionAdminPage"
    DynamicMasterPageFile="~masterurl/default.master" EnableSessionState="True" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <script type="text/javascript">
        function openPopup(type) {
            var options = SP.UI.$create_DialogOptions();
            options.width = 350;
            options.height = 150;
            options.title = 'New File Type';
            options.url = "/_layouts/ExtentrixWebIntrface/Pages/FileTypePage.aspx?Type=New";
            options.dialogReturnValueCallback = Function.createDelegate(
                        null, popup_modalDialogClosedCallback);
            SP.UI.ModalDialog.showModalDialog(options);
        }
        function popup_modalDialogClosedCallback(result, value) {
            if (result == '1') {
                url = "/_layouts/ExtentrixWebIntrface/Pages/ContentRedirectionAdminPage.aspx";
                window.navigate(url);
            }
        }

    </script>
</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <asp:UpdatePanel ID="fileTypesPanel" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <div>
                <asp:CheckBox ID="chkAlwaysGoThrough" Text="Always enable client redirection" OnCheckedChanged="GoThrough_CheckedChanged" AutoPostBack="true" runat="server" />
            </div>
            <div>
                <asp:Label ID="FileTypesLabel" Text="Content Redirection File Types" runat="server" />
            </div>
            <div>
                <asp:HyperLink ID="linkAddFileType" Text="Add" NavigateUrl="javascript:openPopup('New');"
                    runat="server" />
                <asp:LinkButton ID="linkDeleteFileType" Text="Delete" OnClick="DeleteFileType_Button"
                    runat="server" />
            </div>
            <asp:ListBox ID="lstFileTypes" Width="150" Height="150" AutoPostBack="true" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
    Content Redirection Admin Page
</asp:Content>
<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea"
    runat="server">
    Content Redirection
</asp:Content>

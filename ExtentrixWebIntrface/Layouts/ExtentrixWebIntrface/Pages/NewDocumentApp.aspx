<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls"
    Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page EnableViewState="true" Language="C#" AutoEventWireup="true" CodeBehind="NewDocumentApp.aspx.cs"
    Inherits="ExtentrixWebIntrface.Layouts.ExtentrixWebIntrface.Pages.NewDocumentApp"
    EnableSessionState="True" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    
     <asp:RadioButtonList ID="ApplicationsList" runat="server">
    </asp:RadioButtonList>
    <div>
        <table class="ms-formtoolbar">
            <tbody>
                <tr>
                    <td>
                        <asp:Button ID="OkButton" CssClass="ms-ButtonHeightWidth" Text="OK" runat="server"
                            OnClick="OkButton_Clicked" />
                    </td>
                    <td class="ms-separator" />
                    <td>
                        <asp:Button ID="CancelButton" CssClass="ms-ButtonHeightWidth" Text="Cancel" runat="server"
                         OnClientClick="window.frameElement.cancelPopUp();"/>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</asp:Content>
<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
    New Document Application
</asp:Content>
<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea"
    runat="server">
    New Document Application Page
</asp:Content>

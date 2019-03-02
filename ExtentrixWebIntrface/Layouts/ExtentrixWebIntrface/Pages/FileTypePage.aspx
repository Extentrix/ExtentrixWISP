<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls"
    Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page Language="C#" AutoEventWireup="true" EnableSessionState="True" CodeBehind="FileTypePage.aspx.cs" Inherits="ExtentrixWebIntrface.Layouts.ExtentrixWebIntrface.Pages.FileTypePage"
    DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <asp:UpdatePanel ID="mainPanel" runat="server">
        <ContentTemplate>
            <table>
                <tbody>
                    <tr>
                        <td>
                            <asp:Label ID="lblFileType" Text="File Type" runat="server" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtFileType" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="lblAppName" Text="Application Name" runat="server" />
                        </td>
                        <td>
                            <asp:DropDownList ID="dropDownAppName" AutoPostBack="true" runat="server" />
                        </td>
                    </tr>
                </tbody>
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>
    <div>
        <table class="ms-formtoolbar">
            <tbody>
                <tr>
                    <td>
                        <asp:Button ID="SaveButton" Text="Save" OnClick="Save_OnClick" CssClass="ms-ButtonHeightWidth"
                            runat="server" />
                    </td>
                    <td class="ms-separator" />
                    <td>
                        <asp:Button ID="CancelButton" Text="Cancel" CssClass="ms-ButtonHeightWidth" OnClientClick="window.frameElement.cancelPopUp();"
                            runat="server" />
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</asp:Content>
<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
    File Type
</asp:Content>
<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea"
    runat="server">
    File Type
</asp:Content>

<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls"
    Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages"
    Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ExtentrixWIWebPartUserControl.ascx.cs"
    Inherits="ExtentrixWebIntrface.ExtentrixWIWebPart.ExtentrixWIWebPartUserControl" %>
<%@ Register Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI.WebControls" TagPrefix="asp" %>
<SharePoint:ScriptLink ID="jQueryLink" Name="/_layouts/ExtentrixWebIntrface/js/jquery-1.4.1.js"
    runat="server" />
<link href="/_layouts/ExtentrixWebIntrface/css/webpart.css" rel="stylesheet" type="text/css" />
<script type="text/javascript">
    // Create an ActiveX ICA Object.
    function createIcaObj() {
        var obj = null;
        try {
            obj = new ActiveXObject("Citrix.ICAClient");
        } catch (e) {
        }
        return obj;
    }
    // For Win32 IE, we get the version from the ActiveX ICO.
    function getIcaClientVersion() {
        var result;
        var obj = createIcaObj();
        if (obj != null) {
            try {
                var temp = obj.ClientVersion;
                if (temp != null && temp != "") {
                    result = temp;
                }
            } catch (e) { }
        }
        return result;
    }

    function viewIcaClientMessage() {
        jQuery("#webpart").hide();
        jQuery("#icaclient").show();
    }
    function isIcaClientSettingsSet() {
        if (("<% =linkIcaClientUrl.NavigateUrl %>" != "") && ("<% =fldIcaClientVersion.Value %>" != "")) {
            return true;
        }
        return false;
    }

    function isVersionOld(clientVersion, definedVersion) {
        var isEnded = false;
        do {
            strComp1 = clientVersion;
            strComp2 = definedVersion;
            idx1 = strComp1.indexOf('.');
            idx2 = strComp2.indexOf('.');
            if (idx1 != -1) {
                clientVersion = strComp1.substring(idx1 + 1, strComp1.length);
                strComp1 = strComp1.substring(0, idx1);
            }
            if (idx2 != -1) {
                definedVersion = strComp2.substring(idx2 + 1, strComp2.length);
                strComp2 = strComp2.substring(0, idx2);
            }
            if (parseInt(strComp1) < parseInt(strComp2)) {
                return true;
            } else if (parseInt(strComp1) > parseInt(strComp2))
            {
                return false;
            }
            if ((idx1 == -1) && (idx1 == -1)) {
                isEnded = true;
            }
        } while (!isEnded)
        return false;
    }
    function onChangeValidation() {
        jQuery("#<%= lblCredentialsExpired.ClientID %>").hide();
	    ValidatorValidate(<%=credentialsValidator.ClientID %>);
    }
    jQuery(document).ready(function () {
        /*if (isIcaClientSettingsSet()) {
            var ica = createIcaObj();
            if (ica == null) {
                viewIcaClientMessage();
            }
            else {
                var clientVersion = getIcaClientVersion();
                definedVersion = "<% =fldIcaClientVersion.Value %>";
                if (isVersionOld(clientVersion, definedVersion)) {
                    viewIcaClientMessage();
                }
            }

        }*/
        if ("<% =fldShowExpirationMessage.Value %>" != "") 
        {
            alert("License expires less than 10 days.");
        }
    });
</script>
<asp:HiddenField ID="fldShowExpirationMessage" Value="<% #LicenseExpirationDaysLeft %>" runat="server" />
<div class="main" id="webpart">
    <asp:Panel ID="WebPartPanel" runat="server">
        <asp:Panel ID="ToolBarPanel" runat="server">
						<table class="toolbar" runat="server" id="searchToolbar">
                <tr>
                    <td>
                        <asp:TextBox ID="SearchTextBox" runat="server" />
                    </td>
                    <td>
                        <asp:Button ID="SearchButton" Text="Search" OnClick="SearchButton_Click" runat="server" />
                    </td>
                </tr>
            </table>
						<div class="clear">
						</div>
						<table class="breadcrumb">
                            <tbody>
                                <tr>
                                    <td>
                                        <asp:ImageButton ID="HomeImageButton" runat="server" ImageUrl="~/_layouts/images/ExtentrixWebIntrface/top.gif"
                                            OnClick="homeClicked" ToolTip="Home" />
                                    </td>
                                    <td>
                                        <asp:ImageButton ID="UpImageButton" runat="server" ImageUrl="~/_layouts/images/ExtentrixWebIntrface/up.gif"
                                            OnClick="upClicked" ToolTip="Up" />
                                    </td>
                                    <td>
                                        <asp:Table ID="BreadCrumbsTable" runat="server">
                                            <asp:TableRow>
                                                <asp:TableCell>
                                                    Home:\
                                                </asp:TableCell>
                                            </asp:TableRow>
                                        </asp:Table>
                                        <asp:UpdatePanel ID="FavoritesConfigurationUpdatePanel" runat="server" UpdateMode="Conditional">
                                            <ContentTemplate>
                                                <asp:Table ID="FavoritesConfiguration" runat="server">
                                                    <asp:TableRow>
                                                        <asp:TableCell>Mode:</asp:TableCell>
                                                        <asp:TableCell Visible="true">
                                                            <asp:ImageButton ID="EditModeImageButton" runat="server" ImageUrl="~/_layouts/images/ExtentrixWebIntrface/edit.ico"
                                                                OnClick="FavoritesConfigurationChoice_ImgClick" ToolTip="Manager Favorites" />
                                                        </asp:TableCell>
                                                        <asp:TableCell>
                                                            <asp:RadioButtonList ID="FavoritesConfigurationChoice" AutoPostBack="true" RepeatDirection="Horizontal"
                                                                runat="server" OnSelectedIndexChanged="FavoritesConfigurationChoice_SelectedIndexChanged"
                                                                Visible="false">
                                                                <asp:ListItem Text="Display" Value="0" Selected="true" />
                                                                <asp:ListItem Text="Configuration" Value="1" />
                                                            </asp:RadioButtonList>
                                                        </asp:TableCell>
                                                    </asp:TableRow>
                                                </asp:Table>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
            <table runat="server" id="switcher" class="switcher">
                <tr>
                    <td>
                        <asp:Label ID="ViewLabel" Text="View:" runat="server" />
                    </td>
                    <td>
                        <asp:DropDownList ID="DropDownViewSwitcher" AutoPostBack="true" runat="server" OnTextChanged="DropDownViewSwitcher_SelectedIndexChanged">
                            
                        </asp:DropDownList>
                    </td>
                </tr>
            </table>
						<div class="clear">
						</div>
        </asp:Panel>
        <asp:Panel ID="MainBodyPanel" runat="server">
            <table style="width: 100%;">
                <tr>
                    <td>
                        <asp:Panel ID="LinksPanel" Visible="false" runat="server">
                            <div>
                                <asp:LinkButton ID="DisconnectLinkButton" Text="Disconnect Sessions" runat="server"
                                    OnClick="DisconnectSessionsButton_Click" />
                                <asp:LinkButton ID="ReconnectLinkButton" runat="server" Visible="false" Text="Reconnect Sessions"
                                    OnClick="ReconnectSessionsButton_Click" />
                            </div>
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:MultiView ID="AppMultiView" runat="server" ActiveViewIndex="0">
                            <asp:View ID="IconsView" runat="server">
                                <asp:UpdatePanel ID="IconsUpdatePanel" UpdateMode="Conditional" runat="server">
                                    <ContentTemplate>
																		<ul ID="IconsList" runat="server" class="icons">
																		</ul>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </asp:View>
                            <asp:View ID="ListView" runat="server">
                                <asp:Table ID="ListTable" runat="server" class="main" >
                                </asp:Table>
                            </asp:View>
                            <asp:View ID="TreeView" runat="server">
                                <asp:TreeView ID="TreeViewApp" runat="server" ShowLines="True" ExpandDepth="0" OnTreeNodeExpanded="TreeViewApp_TreeNodeExpanded"
                                    OnSelectedNodeChanged="TreeViewApp_SelectedNodeChanged" ShowExpandCollapse="false">
                                    <Nodes>
                                        <asp:TreeNode Text="Citrix Applications" Expanded="true" Value="" SelectAction="None"
                                            PopulateOnDemand="true"></asp:TreeNode>
                                    </Nodes>
                                </asp:TreeView>
                            </asp:View>
                            <asp:View ID="FavoritesView" runat="server">
                                <asp:UpdatePanel ID="FavoritesUpdatePanel" runat="server" UpdateMode="Conditional">
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="EditModeImageButton" EventName="Click" />
                                    </Triggers>
                                    <ContentTemplate>
                                        <asp:Table ID="FavoritesTableContainer" class="favourites-view" runat="server">
                                            <asp:TableRow>
                                                <asp:TableCell class="favourites-app" >
                                                    <div>
                                                        <asp:Label ID="FavoritesLabel" Text="Favorites" runat="server" />
                                                        <asp:Table ID="FavoritesTable" runat="server" />
                                                    </div>
                                                </asp:TableCell>
                                                <asp:TableCell class="notfavourites-app">
                                                    <div>
                                                        <asp:Label ID="NotFavoritesLabel" Text="All Apps" runat="server" />
                                                        <asp:Table ID="NotFavoritesTable" runat="server" />
                                                    </div>
                                                </asp:TableCell></asp:TableRow>
                                        </asp:Table>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </asp:View>
                            <asp:View ID="SearchView" runat="server">
                                <asp:Table ID="SearchTable" runat="server" />
                            </asp:View>
                            <asp:View ID="ErrorView" runat="server">
                                Error occurs during page loading. For more details see Event Log.</asp:View>
                        </asp:MultiView>
                    </td>
                </tr>
            </table>
        </asp:Panel>
    </asp:Panel>
    <asp:Panel ID="CredentialsPanel" Visible="false" runat="server">
        <asp:Label runat="server" ID="lblCredentialsExpired" 
            Text="Credentials have been expired." class="credentials-expired" Visible="false"/>
        <table class="credentials">
            <tr>
                <td>
                    <asp:Label runat="server" ID="lblUserName" Text="User Name" />
                </td>
                <td>
                    <asp:TextBox runat="server" ID="txtUserName" onkeypress="onChangeValidation();" />
                    <asp:RequiredFieldValidator runat="server" ID="txtUserNameRequiredValidator"
                    ValidationGroup="credentials" ControlToValidate="txtUserName" Text="You must specify value for this field." />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label runat="server" ID="lblPassword" Text="Password" />
                </td>
                <td>
                    <asp:TextBox runat="server" ID="txtPassword" TextMode="Password" onkeypress="onChangeValidation();"  />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label runat="server" ID="lblDomain" Text="Domain" />
                </td>
                <td>
                    <asp:TextBox runat="server" ID="txtDomain" onkeypress="onChangeValidation();"  />
                    <asp:RequiredFieldValidator runat="server" ID="txtDomainRequiredValidator"
                     ValidationGroup="credentials" ControlToValidate="txtDomain" Text="You must specify value for this field." />
                </td>
            </tr>
        </table>        
        <asp:Button ID="btnValidate" Text="Connect" ValidationGroup="credentials" 
            OnClick="ValidateButton_Click" runat="server" />
        <asp:CustomValidator runat="server" ID="credentialsValidator" Text="Credentials are not valid." 
            ValidationGroup="credentials" OnServerValidate="OnValidateCredentials" />
    </asp:Panel>
</div>
<div id="icaclient" style="display: none;">
    <asp:HiddenField ID="fldIcaClientVersion" Value="<% #IcaClientVersion %>" runat="server" />
    <div>
        You don't have Citrix Ica Client installed on you machine or version of the client
        is old.
        <br />
        You can install from
        <asp:HyperLink ID="linkIcaClientUrl" Text="here" NavigateUrl="<% #IcaClientUrl %>"
            runat="server" />.
    </div>
</div>

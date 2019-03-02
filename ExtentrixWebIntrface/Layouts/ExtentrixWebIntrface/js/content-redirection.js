var extensions = new Array();
var goThroughFileUrl = null;
var goThrough = false;
function include(arr, obj) {
    for (var i = 0; i < arr.length; i++) {
        if (arr[i] == obj) return true;
    }
}
jQuery(document).ready(function () {
    if ((typeof (ctx) != "undefined") && (ctx.listTemplate == '101')) {
        SP.SOD.executeOrDelayUntilScriptLoaded(GetGoThroughValue, 'SP.js');      
    }
    goThroughFileUrl = null;
});

function documentApp_openModalDialog(action, url) {

    if (url != null) {
        goThroughFileUrl = url;
    }

    var options = SP.UI.$create_DialogOptions();
    options.width = 350;    
    options.title = action + ' Document Application';
    options.url = "/_layouts/ExtentrixWebIntrface/Pages/NewDocumentApp.aspx?Action=" + action;
    options.dialogReturnValueCallback = Function.createDelegate(
                        null, documentApp_modalDialogClosedCallback);

    SP.UI.ModalDialog.showModalDialog(options);

}
function documentApp_modalDialogClosedCallback(result, value) {
    if (value == '1') {
        var options = result.split(';');
        if (options[0] == "New") {
            url = "/_layouts/ExtentrixWebIntrface/Pages/ContentRedirection.aspx?Action=New&App=" + options[1];
        }
        else if (options[0] == "Edit") {
            url = "/_layouts/ExtentrixWebIntrface/Pages/ContentRedirection.aspx?Action=EditThrough&App=" + options[1] + "&FarmName=" + options[2] + "&FileUrl=" + goThroughFileUrl;
        }

        window.navigate(url);
    }
}

function openEditDocument() {
    var selectedItems = SP.ListOperation.Selection.getSelectedItems();
    var item = selectedItems[0];

    var link = jQuery("table.ms-listviewtable tr.ms-itmhover[iid*=," + item['id'] + ",] td.ms-vb-title div[id=" + item['id'] + "] a");
    goThroughFileUrl = link[0].href;
    if (goThroughFileUrl.indexOf("javascript:window.navigate") != -1) {
        goThroughFileUrl = goThroughFileUrl.substring(goThroughFileUrl.lastIndexOf("&FileUrl=") + 9);
        goThroughFileUrl = goThroughFileUrl.substring(0, goThroughFileUrl.length - 3);
    } else if (goThroughFileUrl.indexOf("javascript:documentApp_openModalDialog") != -1) {
        goThroughFileUrl = goThroughFileUrl.substring(goThroughFileUrl.lastIndexOf("('Edit', '") + 10);
        goThroughFileUrl = goThroughFileUrl.substring(0, goThroughFileUrl.length - 3);
    }
    extension = GetFileExtension(goThroughFileUrl);

    if (include(extensions, extension)) {
        window.navigate(GetRedirectionUrl("Edit", extension, goThroughFileUrl));
    }
    else if (goThrough) {
        documentApp_openModalDialog('Edit');
    }
}


function GetFileExtension(url) {
    var lastIndex = url.lastIndexOf(".");
    var extension = null;
    if (lastIndex != -1) {
        extension = url.substring(lastIndex + 1);

    }
    return extension;
}


function GetGoThroughValue() {

    var spCtx = new SP.ClientContext.get_current();
    var settingsList = spCtx.get_site().get_rootWeb().get_lists().getByTitle('Citrix Settings');

    var query = new SP.CamlQuery();
    query.set_viewXml("<View><Query><Where><Eq><FieldRef Name='Title'/><Value Type='Text'>GoThrough</Value></Eq></Where></Query></View>");
    selectedSettingsItems = settingsList.getItems(query);

    spCtx.load(selectedSettingsItems, "Include(Value)");
    spCtx.executeQueryAsync(getSettingsItemsWithQuerySuccess, getSettingsItemsWithQueryFailure);
    function getSettingsItemsWithQuerySuccess(sender, args) {
        var listEnumerator = selectedSettingsItems.getEnumerator();
        while (listEnumerator.moveNext()) {
            var value = listEnumerator.get_current().get_item("Value");
            goThrough = (value == "true");
        }
        GetRedirectionFileTypes();
    }

    function getSettingsItemsWithQueryFailure(sender, args) {
        alert('Failed to get list items. \nError: ' + args.get_message() + '\nStackTrace: ' + args.get_stackTrace());
    }
}

function GetRedirectionFileTypes() {

    var spCtx = new SP.ClientContext.get_current();
    var fileTypesList = spCtx.get_site().get_rootWeb().get_lists().getByTitle('File Types');

    var query = new SP.CamlQuery();
    //query.set_viewXml("<View><Query><Where><Eq><FieldRef Name='Title'/><Value Type='Text'>" + extension + "</Value></Eq></Where></Query></View>");
    selectedItems = fileTypesList.getItems(query);

    spCtx.load(selectedItems, "Include(Title)");
    spCtx.executeQueryAsync(getItemsWithQuerySuccess, getItemsWithQueryFailure);
    function getItemsWithQuerySuccess(sender, args) {
        var listEnumerator = selectedItems.getEnumerator();
        while (listEnumerator.moveNext()) {
            var extension = listEnumerator.get_current().get_item("Title");
            extensions.push(extension);
        }

        var links = jQuery("table.ms-listviewtable tr.ms-itmhover td.ms-vb-title div[class='ms-vb itx'] a");        
        changeHrefs(links);

        links = jQuery("table.ms-listviewtable tr.ms-itmhover td.ms-vb2 a[onFocus='OnLink(this)']");
        changeHrefs(links);

    }

    function getItemsWithQueryFailure(sender, args) {
        alert('Failed to get list items. \nError: ' + args.get_message() + '\nStackTrace: ' + args.get_stackTrace());
    }
}

function changeHrefs(links) {
    jQuery(links).each(function () {
        var extension = GetFileExtension(this.href)
        if (include(extensions, extension)) {
            //this.target = "_blank";
            jQuery(this).attr('onmousedown', null);
            jQuery(this).attr('onclick', null);
            var url = "javascript:window.navigate('" + GetRedirectionUrl("Edit", extension, this.href) + "');";
            this.href = url;
        }
	else if (goThrough) {
            if (this.href.indexOf("FolderCTID=") == -1) {
            jQuery(this).attr('onmousedown', null);
            jQuery(this).attr('onclick', null);
            var url = "javascript:documentApp_openModalDialog('Edit', '" + this.href + "');";
            this.href = url;
        }
        }
    });
}

function Custom_AddDocLibMenuItems(m, ctx) {
    currentItemFileUrl = GetAttributeFromItemTable(itemTable, "Url", "ServerUrl");
    var extension = GetAttributeFromItemTable(itemTable, "Ext", "Ext");

    if (extension != "") {

        var link = jQuery("table.ms-listviewtable tr.ms-itmhover[iid*=," + currentItemID + ",] td.ms-vb-title div[class='ms-vb'] a")[0];
    if (include(extensions, extension)) {
            if (link.href.indexOf("javascript:window.navigate('") == -1) {
                jQuery(link).attr('onmousedown', null);
                jQuery(link).attr('onclick', null);
                var url = "javascript:window.navigate('" + GetRedirectionUrl("Edit", extension, link.href) + "');";
                link.href = url;
            }

        strDisplayText = "Edit with Application Delivered by Citrix";
        strAction = "javascript:window.open('" + GetRedirectionUrl("Edit", extension, ctx.HttpRoot + currentItemFileUrl) + "');";
        strImagePath = ctx.imagesPath + "ExtentrixWebIntrface/doc.png";
        menuOption = CAMOpt(m, strDisplayText, strAction, strImagePath, null, 250);
        menuOption.id = "ID_EditWithCitrixApp";
    }
    else if (goThrough) {
            if ((link.href.indexOf("javascript:documentApp_openModalDialog('Edit', '") == -1) && (link.href.indexOf("FolderCTID=") == -1)){
                jQuery(link).attr('onmousedown', null);
                jQuery(link).attr('onclick', null);
                var url = "javascript:documentApp_openModalDialog('Edit', '" + link.href + "');";
                link.href = url;
            }

        strDisplayText = "Edit with Application Delivered by Citrix";
        strAction = "javascript:documentApp_openModalDialog('Edit','" + ctx.HttpRoot + currentItemFileUrl + "');";
        strImagePath = ctx.imagesPath + "ExtentrixWebIntrface/doc.png";
        menuOption = CAMOpt(m, strDisplayText, strAction, strImagePath, null, 250);
        menuOption.id = "ID_EditWithCitrixApp";
    }
    }

}

function GetRedirectionUrl(type, extension, url) {
    var url = ctx.HttpRoot + "/_layouts/ExtentrixWebIntrface/Pages/ContentRedirection.aspx?Action=" + type + "&FileType=" + extension + "&FileUrl=" + url
    return url;
}

function enableDisableButton() {
    if (CountDictionary(SP.ListOperation.Selection.getSelectedItems()) != 1) {
        return false;
    }
    var item = SP.ListOperation.Selection.getSelectedItems()[0];
    return item['fsObjType'] == '0'; 
}

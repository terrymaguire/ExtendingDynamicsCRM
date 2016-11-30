/// <reference path="MSXRMTOOLS.Xrm.Page.2016.js" />

function Test() {
    Alert.show('Would you like to perform some action on this Contact?', 'This will perform a background action', [new Alert.Button("OK"), new Alert.Button("Cancel")], "QUESTION", 500, 250, "", true, 30);
}


function Evaluate() {
    return true;
}

function mobilePhoneChange() {
    var businessPhone = Xrm.Page.getAttribute("telephone1").getValue();
    var mobilePhoneField = Xrm.Page.getAttribute("mobilephone");
    
    if (businessPhone != null)
    {
        var mobilePhone = mobilePhoneField.getValue();

        if (mobilePhone == null)
            mobilePhoneField.setValue(businessPhone);
    }
}
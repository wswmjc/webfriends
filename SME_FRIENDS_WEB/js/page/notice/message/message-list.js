Message = function () { }
Message.registerClass("Message");
Message.Init = function init() {
    objPub.IsMain = false;
    if (Message.AddressBook.Count == 0) {
        $("#spnAddressBookMessageCount").text("0").hide();
    }
    else {
        $("#spnAddressBookMessageCount").text(Message.AddressBook.Count).show();
    }
    if (Message.Community.Count == 0) {
        $("#spnCommunityMessageCount").text("0").hide();
    }
    else {
        $("#spnCommunityMessageCount").text(Message.Community.Count).show();
    }
    Message.AddressBook.GetValidationMessageList();
    $("#liAddressBookMessage").on("click", function (event) {
        $(event.target).addClass("selected").siblings().removeClass("selected");
        Message.AddressBook.GetValidationMessageList();
    });
    $("#liCommunityMessage").on("click", function (event) {
        $(event.target).addClass("selected").siblings().removeClass("selected");
        Message.Community.GetValidationMessageList();
    });
   
    $("#aGoBack").off("click").on("click", Message.BackEvent);
}
//返回主页面
Message.BackEvent = function BackEvent(event) {
    objPub.InitLeftMain(true);
}
Message.GetValidationMessageCount = function get_validation_message_count() {
    $.when(Message.AddressBook.GetValidationMessageCount(), Message.Community.GetValidationMessageCount()).done(function (addressBookCount, communityCount) {
        if (addressBookCount[0].d != 0 || communityCount[0].d != 0) {
            $("#spnMessageCount").show();
            Message.AddressBook.Count = addressBookCount[0].d;
            Message.Community.Count = communityCount[0].d;
        }
        else {
            $("#spnMessageCount").hide();
        }

    });

}
Message.IsShowMessage = function is_show_message() {
    if (Message.Community.Count == 0 && Message.AddressBook.Count == 0) {
        $("#spnMessageCount").hide();
    }
    else {
        $("#spnMessageCount").show();
    }
}


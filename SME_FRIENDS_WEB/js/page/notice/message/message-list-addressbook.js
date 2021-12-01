Message.AddressBook = function () {

}
Message.AddressBook.registerClass("Message.AddressBook");
//待处理信息数
Message.AddressBook.Count = 0;
//获取通讯录列表事件
Message.AddressBook.GetValidationMessageList = function GetValidationMessageList() {
    $.SimpleAjaxPost("service/AddressBookService.asmx/GetMyValidationMessageList", true,
       function (json) {
           var result = $.Deserialize(json.d);
           var temp = "";
           if (result != null) {
               $.each(result, function (index, item) {
                   temp += "<li id='liMessage" + index + "' class='clear-fix'>";
                   temp += "<div class='contacts-info'>";
                   temp += "<div class='contacts-photo'><img src='" + item.UserUrl + "'></div>";
                   temp += "<div class='notice-info'>";
                   temp += "<p class='notice-name'><span>" + item.UserName + "</span><span class='notice-time'>" + item.ApplicationTime.format("yyyy-MM-dd") + "</span></p>";
                   if (item.Remark != "") {
                       temp += "<p class='notice-text'>" + item.Remark + "</p>";
                   }
                   else {
                       temp += "<p class='notice-text'>请求添加您为好友~</p>";
                   }
                   temp += "</div>";
                   temp += "</div>";
                   temp += "<div class='contacts-option'>";
                   temp += "<input id='btnAddressBookAgree" + index + "' type='button' value='同意'>";
                   temp += "<input id='btnAddressBookIgnore" + index + "' type='button' value='忽略'>";
                   temp += "<input id='btnAddressBookRefuse" + index + "' type='button' value='拒绝' class='btn-exit'>";
                   temp += "</div>";
                   temp += "</li>";
                   $(document).off("click", "#btnAddressBookAgree" + index);
                   $(document).on("click", "#btnAddressBookAgree" + index, { ApplicantID: item.UserID, ApplicantName: item.UserName, ApplicationTime: item.ApplicationTime }, Message.AddressBook.AgreeEvent);
                   $(document).off("click", "#btnAddressBookRefuse" + index);
                   $(document).on("click", "#btnAddressBookRefuse" + index, { ApplicantID: item.UserID, ApplicantName: item.UserName, ApplicationTime: item.ApplicationTime }, Message.AddressBook.RefuseEvent);
                   $(document).off("click", "#btnAddressBookIgnore" + index);
                   $(document).on("click", "#btnAddressBookIgnore" + index, { Index: index, ID: item.ID }, Message.AddressBook.IgnoreEvent);
               });
               $("#divEmptyMessage").hide();
           }
           else {
               $("#divEmptyMessage").show();
               $("#spnEmptyMessage").text("暂没有通讯录验证消息哦~");
           }
           $("#ulMyMessageList").empty().append(temp);
       });
}
//同意
Message.AddressBook.AgreeEvent = function AgreeEvent(event) {
    var approve_view = {
        ApplicantID: event.data.ApplicantID,
        ApplicantName: event.data.ApplicantName,
        ApplicationTime: event.data.ApplicationTime
    };
    $.SimpleAjaxPost("service/AddressBookService.asmx/Agree", true,
      "{approveView:" + $.Serialize(approve_view) + "}",
       function (json) {
           var result = json.d;
           if (result == true) {
               $.Alert("设置添加好友成功！", function () {
                   Message.AddressBook.GetValidationMessageList();
                   Message.AddressBook.GetValidationMessageCount().done(function (json) {
                       var result = json.d;
                       Message.AddressBook.Count = result;
                       if (Message.AddressBook.Count == 0) {
                           $("#spnAddressBookMessageCount").text("0").hide();
                           Message.IsShowMessage();
                       }
                       else {
                           $("#spnAddressBookMessageCount").text(Message.AddressBook.Count).show();
                       }
                   });

               });
           }
           else {
               console.log("设置添加好友失败！");
           }
       });

}
//拒绝
Message.AddressBook.RefuseEvent = function RefuseEvent(event) {
    var approve_view = {
        ApplicantID: event.data.ApplicantID,
        ApplicantName: event.data.ApplicantName,
        ApplicationTime: event.data.ApplicationTime
    };
    $.SimpleAjaxPost("service/AddressBookService.asmx/Refuse", true,
      "{approveView:" + $.Serialize(approve_view) + "}",
       function (json) {
           var result = json.d;
           if (result == true) {
               $.Alert("拒绝邀请成功！", function () {
                   Message.AddressBook.GetValidationMessageList();
                   Message.AddressBook.GetValidationMessageCount().done(function (json) {
                       var result = json.d;
                       Message.AddressBook.Count = result;
                       if (Message.AddressBook.Count == 0) {
                           $("#spnAddressBookMessageCount").text("0").hide();
                           Message.IsShowMessage();
                       }
                       else {
                           $("#spnAddressBookMessageCount").text(Message.AddressBook.Count).show();
                       }
                   });
               });
           }
           else {
               console.log("拒绝邀请失败！");
           }
       });

}
//忽略
Message.AddressBook.IgnoreEvent = function IgnoreEvent(event) {
    var index = event.data.Index;
    var id = event.data.ID;
    $.SimpleAjaxPost("service/AddressBookService.asmx/Ignore", true,
        "{ID:'" + id + "'}",
     function (json) {
         var result = json.d;
         if (result == true) {
             $("#liMessage" + index).remove();
             Message.AddressBook.Count = Message.AddressBook.Count - 1;
             if (Message.AddressBook.Count == 0) {
                 $("#spnAddressBookMessageCount").text("0").hide();
                 Message.IsShowMessage();
             }
             else {
                 $("#spnAddressBookMessageCount").text(Message.AddressBook.Count).show();
             }
         }
         else {
             console.log("设置忽略失败！");
         }
     });
}
//获取通讯录待校验信息数
Message.AddressBook.GetValidationMessageCount = function get_validation_message_count() {
    return $.SimpleAjaxPost("service/AddressBookService.asmx/GetMyValidationMessageCount", true);
}

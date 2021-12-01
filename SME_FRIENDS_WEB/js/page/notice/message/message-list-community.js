Message.Community = function () {

}
Message.Community.registerClass("Message.Community");
//待处理数
Message.Community.Count = 0;
//获取行业圈子列表事件
Message.Community.GetValidationMessageList = function GetValidationMessageList() {
    $.SimpleAjaxPost("service/CommunityService.asmx/GetMyValidationMessageList", true,
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
                   temp += "<p class='notice-text'>" + item.Remark + "</p>";
                   temp += "</div>";
                   temp += "</div>";
                   temp += "<div class='contacts-option'>";
                   temp += "<input id='btnCommunityAgree" + index + "' type='button' value='同意'>";
                   temp += "<input id='btnCommunityIgnore" + index + "' type='button' value='忽略'>";
                   temp += "<input id='btnCommunityRefuse" + index + "' type='button' value='拒绝' class='btn-exit'>";
                   temp += "</div>";
                   temp += "</li>";
                   $(document).off("click", "#btnCommunityAgree" + index);
                   $(document).on("click", "#btnCommunityAgree" + index, { MemberID: item.MemberID, MemberName: item.MemberName, CommunityID: item.CommunityID }, Message.Community.AgreeEvent);
                   $(document).off("click", "#btnCommunityRefuse" + index);
                   $(document).on("click", "#btnCommunityRefuse" + index, { MemberID: item.MemberID, CommunityID: item.CommunityID }, Message.Community.RefuseEvent);
                   $(document).off("click", "#btnCommunityIgnore" + index);
                   $(document).on("click", "#btnCommunityIgnore" + index, { Index: index, ID: item.ID }, Message.Community.IgnoreEvent);
               });
               $("#divEmptyMessage").hide();
           }
           else {
               $("#divEmptyMessage").show();
               $("#spnEmptyMessage").text("暂没有行业圈子验证消息哦~");
           }
           $("#ulMyMessageList").empty().append(temp);
       });
}
//获取通讯录待校验信息数
Message.Community.GetValidationMessageCount = function get_validation_message_count() {
    return $.SimpleAjaxPost("service/CommunityService.asmx/GetMyValidationMessageCount", true);
}
//同意
Message.Community.AgreeEvent = function AgreeEvent(event) {
    var approve_view = {
        CommunityID: event.data.CommunityID,
        MemberName: event.data.MemberName,
        MemberID: event.data.MemberID
    };
    $.SimpleAjaxPost("service/CommunityService.asmx/Agree", true,
      "{approveView:" + $.Serialize(approve_view) + "}",
       function (json) {
           var result = json.d;
           if (result == true) {
               $.Alert("设置添加圈子成员成功！", function () {
                   Message.Community.GetValidationMessageList();
                   Message.Community.GetValidationMessageCount().done(function (json) {
                       var result = json.d;
                       Message.Community.Count = result;
                       if (Message.Community.Count == 0) {
                           $("#spnCommunityMessageCount").text("0").hide();
                           Message.IsShowMessage();
                       }
                       else {
                           $("#spnCommunityMessageCount").text(Message.Community.Count).show();
                       }
                       $("#divHasCommunity,#divHasCommunityList,#divHasCommunityLabel").show(); 
                   });
                   PublishRight.MyCommunityBind();

               });
           }
           else {
               console.log("设置添加圈子成员失败！申请已被他人处理。", function () {
                   Message.Community.GetValidationMessageList();
                   Message.Community.GetValidationMessageCount().done(function (json) {
                       var result = json.d;
                       Message.Community.Count = result;
                       if (Message.Community.Count == 0) {
                           $("#spnCommunityMessageCount").text("0").hide();
                           Message.IsShowMessage();
                       }
                       else {
                           $("#spnCommunityMessageCount").text(Message.Community.Count).show();
                       }
                   });
                   PublishRight.MyCommunityBind();
               });
           }
       });

}
//拒绝
Message.Community.RefuseEvent = function RefuseEvent(event) {
    var approve_view = {
        MemberID: event.data.MemberID,
        CommunityID: event.data.CommunityID
    };
    $.SimpleAjaxPost("service/CommunityService.asmx/Refuse", true,
      "{approveView:" + $.Serialize(approve_view) + "}",
       function (json) {
           var result = json.d;
           if (result == true) {
               $.Alert("拒绝邀请成功！", function () {
                   Message.Community.GetValidationMessageList();
                   Message.Community.GetValidationMessageCount().done(function (json) {
                       var result = json.d;
                       Message.Community.Count = result;
                       if (Message.Community.Count == 0) {
                           $("#spnCommunityMessageCount").text("0").hide();
                           Message.IsShowMessage();
                       }
                       else {
                           $("#spnCommunityMessageCount").text(Message.Community.Count).show();
                       }
                   });
               });
           }
           else {
               $.Alert("拒绝邀请失败！申请已被他人处理。", function () {
                   Message.Community.GetValidationMessageList();
                   Message.Community.GetValidationMessageCount().done(function (json) {
                       var result = json.d;
                       Message.Community.Count = result;
                       if (Message.Community.Count == 0) {
                           $("#spnCommunityMessageCount").text("0").hide();
                           Message.IsShowMessage();
                       }
                       else {
                           $("#spnCommunityMessageCount").text(Message.Community.Count).show();
                       }
                   });
               });
           }
       });
}
//忽略
Message.Community.IgnoreEvent = function IgnoreEvent(event) {
    var index = event.data.Index;
    var id = event.data.ID;
    $.SimpleAjaxPost("service/CommunityService.asmx/Ignore", true,
        "{ID:'" + id + "'}",
     function (json) {
         var result = json.d;
         if (result == true) {
             $("#liMessage" + index).remove();
             Message.Community.Count = Message.Community.Count - 1;
             if (Message.Community.Count == 0) {
                 $("#spnCommunityMessageCount").text("0").hide();
                 Message.IsShowMessage();
             }
             else {
                 $("#spnCommunityMessageCount").text(Message.Community.Count).show();
             }
         }
         else {
             $.Alert("设置忽略失败！申请已被他人处理。", function () {
                 $("#liMessage" + index).remove();
                 Message.Community.Count = Message.Community.Count - 1;
                 if (Message.Community.Count == 0) {
                     $("#spnCommunityMessageCount").text("0").hide();
                     Message.IsShowMessage();
                 }
                 else {
                     $("#spnCommunityMessageCount").text(Message.Community.Count).show();
                 }
             });
         }
     });
}
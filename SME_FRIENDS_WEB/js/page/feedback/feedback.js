Feedback = function () { }
Feedback.registerClass("Feedback");
Feedback.Init = function init() {
    objPub.GetUserInfo(objPub.UserID).done(function (json) {
        var result = json.d;
        $("#txtFeedBackEmail").val(result.Email);
        //提交反馈事件
        $("#aFeedbackSubmit").off("click").on("click", Feedback.SubmitEvent);
    });
}

//提交反馈事件
Feedback.SubmitEvent = function SubmitEvent(event) {
    if ($("#txtFeedBackEmail").val() == "") {
        $.Alert("邮箱不能为空！", function () {
            $("#txtFeedBackEmail").focus();
        });
        return;
    } else if (!/^[a-z0-9_.%-]+@([a-z0-9-]+\.)+[a-z]{2,4}$/.test($("#txtFeedBackEmail").val())) {
        $.Alert("邮箱格式不正确！", function () {
            $("#txtFeedBackEmail").focus();
        });
        return;
    }

    if ($("#txtFeedBackContent").val() == "") {
        $.Alert("请填写反馈意见！", function () {
            $("#txtFeedBackContent").focus();
        });
        return;
    }
    $.Confirm({ content: "您确定您的反馈意见已经整理书写完毕，等待提交?", width: "auto" }, function () {
        var feedback = {
            ID: $.NewGuid(),
            PlatformType: "1",
            Email: $("#txtFeedBackEmail").val(),
            Content: $("#txtFeedBackContent").val()
        };
        var url = new Array();
        url.push(objPub.ManageUrl);
        url.push("service/FeedbackService.asmx/Submit");
        $.SimpleAjaxCors(url, "POST", "{feedbackInfo:" + $.Serialize(feedback) + "}").done(function (json) {
            var result = json.d;
            if (result == true) {
                $.Alert("感谢您的反馈!", function () {
                    $("#txtFeedBackEmail,#txtFeedBackContent").val("");
                    window.location.href = objPub.FriendsUrl + "biz/main.html"
                });
            }
            else {
                console.log("反馈失败，请及时联系管理员！");
            }
        });
    });
}
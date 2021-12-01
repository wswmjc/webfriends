var login_out_count = 0;
$(window).on("message", function (event) {
    if (event.originalEvent.data !== "FALSE") {
        if (event.originalEvent.data.MessageData !== undefined) {
            if (event.originalEvent.data.MessageData.MessageType == "ResponseInfo") {
                login_out_count++;
                if (login_out_count == objPub.Cors.length - 1) {
                    login_out_count = 0;
                    //交流区登出
                    //Chat.Logout();
                    window.location.href = "/index.html";
                }
            }
        }
    }
});
var connection = $.hubConnection("/share", { useDefaultPath: false });
var shareHubProxy = connection.createHubProxy("IMService");
$(document).ready(function () {
    connection.start()
.done(function () {
    console.log("Now connected, connection ID=" + connection.id);
    init();

    shareHubProxy.on("Recieve", function (item) {
        $(".sticky-close").trigger("click");
        var index = 0;
        if ($(".sticky-note li:last").length !== 0) {
            index = parseInt($(".sticky-note li:last").prop("id").replace("liOffline", "")) + 1;
        }
        var temp = "";
        temp += "<li id='liOffline" + index + "'>";
        temp += "<div class='apply_info'>";
        temp += "<span class='apply_user'>" + item.UserName + "</span>&nbsp;在" + item.Title + "@你";
        temp += "</div>";
        temp += "<div class='apply_btn'>";
        temp += "<input id='btnShow" + index + "' type='button' class='btn_view' value='查看'>";
        temp += "<input id='btnIgnore" + index + "' type='button' class='btn_ignore' value='忽略'>";
        temp += "</div>";
        temp += "<div style='clear:both;'></div>";
        temp += "</li>";
        $(document).off("click", "#btnShow" + index + ",#btnIgnore" + index);
        $(document).on("click", "#btnShow" + index, { NoticeID: item.ID, ID: item.PublishID, Source: item.BusinessType, From: "IM" }, AtInfo.ShowDetailMessageEvent);
        $(document).on("click", "#btnIgnore" + index, { Index: index, ID: item.ID }, AtInfo.OffLineIgnoreEvent);
        $(".sticky-note").prepend(temp);
        $(".sticky").css("height", "");
        var height = $(".sticky").height();
        $(".sticky").css("height", height);
        $("#liOffline" + index).data({
            "ID": item.ID,
            "Source": item.BusinessType
        });
        $(".sticky-up").trigger("click");
        //header红点出现
        $("#spnAtCount").addClass("user-dot");
    });
    shareHubProxy.on("ReceiveLineNotice", function (item) {
        if ($("#sctChat").length > 0) {
            if ($("#liOftenUsedAddressBookChat").hasClass("selected") == true) {
                $.each($("li[id^=liOftenUsedAddressBookChatItem]"), function (index, item) {
                    if ($(item).data("UserID") == item) {
                        $(item).children(".offline").remove();
                    }
                });
            }
            else if ($("#liMyAddressBookChat").hasClass("selected") == true) {
                $.each($("li[id^=liMyAddressBookChatItem]"), function (index, item) {
                    if ($(item).data("UserID") == item) {
                        $(item).children(".offline").remove();
                    }
                });
            }
        }
    });
    shareHubProxy.on("ReceiveOfflineNotice", function (item) {
        if ($("#sctChat").length > 0) {
            if ($("#liOftenUsedAddressBookChat").hasClass("selected") == true) {
                $.each($("li[id^=liOftenUsedAddressBookChatItem]"), function (index, item) {
                    if ($(item).data("UserID") == item) {
                        $(item).append("<div class='offline'><img src='../images/off.png'></div>");
                    }
                });
            }
            else if ($("#liMyAddressBookChat").hasClass("selected") == true) {
                $.each($("li[id^=liMyAddressBookChatItem]"), function (index, item) {
                    if ($(item).data("UserID") == item) {
                        $(item).append("<div class='offline'><img src='../images/off.png'></div>");
                    }
                });
            }
        }
    });
})
.fail(function () { console.log("Could not connect"); });

});
function init() {
    $.CreateCorsLocalStorage(objPub.Cors);
    //设置全局变量-进入我的朋友圈
    objPub.IsMain = true;
    objPub.GetTheme(objPub.UserThemeID);
    //Chat.Login();
    $("#aLogout").on("click", function (event) {
        $.Confirm("确认要退出朋友圈系统吗？",
          function () {
              objPub.Logout();
          });
    });
    // 点击头像改设置
    $("#imgHeadUserUrl").attr("src", objPub.UserUrl).attr("title", "修改个人信息可点击进入个人中心看看~~");
    $("#aUserSetting").attr("title", "修改个人信息可点击进入个人中心看看~~").on("click", function (event) {
        $(".main-left").load("../biz/left/account.html", function (response, status) {
            if (status == "success") {
                User.Setting.Init();
            }
        });
    });

    $("#spnHeadSocialCode").text(objPub.UserName);
    //设置主题
    $("#sctSetTheme").dialog({
        autoOpen: false,
        resizable: false,
        width: 750,
        modal: true,
        title: "主题设置",
        closeText: "点击关闭主题样式设置展示",
        buttons: {
            "取　消": function (event, ui) {
                $(this).dialog("close");
            },
            "设　置": function (event, ui) {
                $.each($("li[id^='liTheme']"), function (index, item) {
                    if ($(item).hasClass("selected") == true) {
                        Moments.SetTheme($(item).data("ThemeID"));
                    }
                });
                $(this).dialog("close");
            }
        }
    });

    //初始化短篇对话框
    $("#sctShort").dialog({
        resizable: false,
        autoOpen: false,
        width: 750,
        height: "auto",
        modal: true,
        title: "发布消息",
        closeText: "点击关闭发送消息窗口",
        create: function () {
            $.SimpleAjaxPost("service/CommunityService.asmx/HaveCommunityAndGroup", true).done(function (json) {
                var result = $.Deserialize(json.d);
                if (result.CommunityResult == false) {
                    $("#divHasCommunity").hide();
                } else {
                    $("#divHasCommunity").show();
                }
                if (result.GroupResult == false) {
                    $("#divHasGroup").hide();
                } else {
                    $("#divHasGroup").show();
                }
            });
        },
        open: Moments.Publish.ResetShortEditorEvent,
        close: function () {
            $(this).dialog("option", "appendTo", "body");
            $(".ui-dialog").css("position", "fixed");
            $(".dialog-mask").hide().css("overflow-y", "hidden").scrollTop(0);
            $("html").css("overflow-y", "scroll");
        }
    });
    //初始化长篇对话框
    $("#sctLong").dialog({
        resizable: false,
        autoOpen: false,
        width: 750,
        height: "auto",
        modal: true,
        title: "发布长篇文章",
        closeText: "点击关闭发布长篇文章窗口",
        closeOnEscape: false,
        create: function () {
            $.SimpleAjaxPost("service/CommunityService.asmx/HaveCommunity", true).done(function (json) {
                var result = json.d; 
                if (result == false) {
                    $("#divHasCommunityLabel").hide();
                    $("#divHasCommunityList").hide();
                } else {
                    $("#divHasCommunityList").show();
                    $("#divHasCommunityLabel").show();
                }
            });
        },
        open: Moments.Publish.ResetLongEditorEvent,
        close: function () {
            $(this).dialog("option", "appendTo", "body");
            $(".ui-dialog").css("position", "fixed");
            $(".dialog-mask").hide().css("overflow-y", "hidden").scrollTop(0);
            $("html").css("overflow-y", "scroll");
        }
    });
    //初始化草稿箱
    $("#sctDraft").dialog({
        resizable: false,
        autoOpen: false,
        width: 750,
        height: "auto",
        modal: true,
        title: "长篇编辑",
        closeText: "点击关闭草稿箱窗口",
        position: {
            my: "center",
            at: "top+35%",
            of: window
        }

    });

    //长篇展示
    $("#sctLongDetail,#sctDraftLongDetail").dialog({
        autoOpen: false,
        resizable: false,
        width: 750,
        height: "auto",
        modal: true,
        title: "文章详细",
        closeOnEscape: false,
        dialogClass: "no-close",
        open: function (event, ui) {
            event.cancelBubble = true;
            $(window).scrollTop(objPub.WindowScrollTop);
            $(this).dialog("option", "appendTo", ".dialog-mask");
            $(".dialog-mask").show();
            var dHeight = $(this).height();
            var wHeight = $(window).height();
            $(this).parent().find(".ui-dialog-titlebar-close").hide();
            if (dHeight >= wHeight) {
                //文章很长替换滚动条
                $("html").css("overflow-y", "hidden");
                $(".ui-dialog").css({
                    "position": "absolute",
                    "top": "0"
                });
                $(".dialog-mask").css({
                    "overflow-y": "scroll",
                    "height": wHeight
                }).scrollTop(0);
                if (window.removeEventListener) {
                    window.removeEventListener("DOMMouseScroll", wheel, false);
                }
                window.onmousewheel = document.onmousewheel = null;
            } else {
                //文章短居中显示
                $(this).dialog("option", "position", { my: "center", at: "center center", of: window });
            }
            objPub.WindowScrollTop = 0;
            return false;
        },
        buttons: {
            "关闭": function (event, ui) {
                event.cancelBubble = true;
                event.stopPropagation();
                $(this).dialog("option", "appendTo", "body");
                $(".ui-dialog").css("position", "fixed");
                $(".dialog-mask").hide().css("overflow-y", "hidden").scrollTop(0);
                $("html").css("overflow-y", "scroll");
                $(this).dialog("close");
                return false;
            }
        }
    });

    //@展示
    $("#sctAtLongDetail").dialog({
        autoOpen: false,
        resizable: true,
        draggable: false,
        width: 750,
        height: "auto",
        modal: true,
        title: "我的@展示",
        closeOnEscape: false,
        dialogClass: "no-close",
        buttons: {
            "关闭": function (event, ui) {
                event.cancelBubble = true;
                event.stopPropagation();
                $(this).dialog("option", "appendTo", "body");
                $(".ui-dialog").css("position", "fixed");
                $(".dialog-mask").hide().css("overflow-y", "hidden").scrollTop(0);
                $("html").css("overflow-y", "scroll");
                $(this).dialog("close");
                return false;
            }
        }
    });


    //初始化编辑框 at对话框的
    $("#txtCommentContentAt").miicWebEdit({
        id: "txtCommentContentAt",
        css: 'weibo-editor-comment',
        placeholder: "对他说些什么...",
        faceid: "aEmotionAt",
        submit: "commentSendAt",
        facePath: '../../images/arclist/', //表情存放的路径
        charAllowed: -1
    });

    //发布初始化
    Moments.Publish.Init();
    //初始化首页面
    objPub.InitLeftMain(true);
    //右侧内容加载
    $(".main-right").load("../biz/right.html", function (response, status) {
        if (status == "success") {
            PublishRight.Init();
        }
    });

    //打开消息
    $("#aMyNotice").on("click", function (event) {
        $(".main-left").load("../biz/left/notice/notice-list.html", function (response, status) {
            if (status == "success") {
                Notice.Init();
            }
        });
    });
    AtInfo.GetAtValidationCount();
    //打开@
    $("#aMyAtInfo").on("click", function (event) {
        $(".main-left").load("../biz/left/notice/at-list.html", function (response, status) {
            if (status == "success") {
                AtInfo.Init();
            }
        });
    });
    //是否有我的验证消息
    Message.GetValidationMessageCount();
    //打开验证消息
    $("#aMyMessage").on("click", function (event) {
        $(".main-left").load("../biz/left/notice/message-list.html", function (response, status) {
            if (status == "success") {
                Message.Init();
            }
        });
    });

    //在线交流
    $(".comments-online").on("click", function (event) {
        $(".main-left").load("../biz/left/chat/chat.html", function (response, status) {
            if (status == "success") {
                Chat.Init();
            }
        });
    });


    //浏览器中Backspace不可用 
    $(document).keydown(function (e) {
        var keyEvent;
        if (e.keyCode == 8) {
            var d = e.srcElement || e.target;
            if (d.tagName.toUpperCase() == 'INPUT' || d.tagName.toUpperCase() == 'TEXTAREA' || (d.tagName.toUpperCase() == 'DIV' && $(d).attr("contenteditable") == "true")) {
                keyEvent = d.readOnly || d.disabled;
            } else {
                keyEvent = true;
            }
        } else {
            keyEvent = false;
        }
        if (keyEvent) {
            e.preventDefault();
        }
    });

    //修改密码
    $("#aChangePs").on("click", User.Password.CheckPasswordEvent);
    $("#txtNewPsword1").on("keyup", function (event) {
        User.Password.SetPasswordStrength(this.value);
    });
    $("#sctChangePW").dialog({
        autoOpen: false,
        resizable: false,
        width: 380,
        height: "auto",
        modal: true,
        title: "修改密码",
        closeText: "点击关闭修改密码窗口",
        buttons: {
            "确　定": User.Password.ChangePasswordEvent,
            "取　消": function () {
                $(this).find("input").val("");
                $(this).find(".error-text").html("&nbsp;");
                $("#passwordText").empty();
                $("#passwordStrength").removeClass().addClass("strength0");
                $(this).dialog("close");
            }
        }
    });
    //关于我们
    $("#aAboutUs").on("click", function (event) {
        $(window).scrollTop(0);
        $("main").load("../biz/public/aboutus.html");
    });

    //联系我们
    $("#aContactUs").on("click", function (event) {
        $(window).scrollTop(0);
        $("main").load("../biz/public/contactus.html");
    });

    //意见反馈
    $("#aFeedBack").on("click", function (event) {
        $(window).scrollTop(0);
        $("main").load("../biz/public/feedback.html", function (response, status) {
            if (status == "success") {
                Feedback.Init();
            }
        });
    });

    //常见问题
    $("#aFaq").on("click", function (event) {
        $(window).scrollTop(0);
        $("main").load("../biz/public/faq.html");
    });

    //公告对话框
    $("#sctAnnounce").dialog({
        autoOpen: false,
        resizable: false,
        width: 750,
        height: "auto",
        modal: true,
        title: "公告详细",
        closeText: "点击关闭公告展示窗口",
        buttons: {
            "关　闭": function (event, ui) {
                $(this).dialog("close");
            }
        }
    });

    //提醒消息数据
    objPub.GetMyOfflineList().done(function (json) {
        var result = $.Deserialize(json.d);
        var temp = "";
        if (result != null) {
            //console.log(result);
            $.each(result, function (index, item) {
                if (item.Content!=null) {
                    temp += "<li id='liOffline" + index + "'>";
                    temp += "<div class='apply_info'>";
                    temp += "<span class='apply_user'>" + item.UserName + "</span>&nbsp;在" + item.Content + (item.NoticeType == Enum.NoticeType.Message.toString() ? "" : "@你");
                    temp += "</div>";
                    temp += "<div class='apply_btn'>";
                    temp += "<input id='btnShow" + index + "' type='button' class='btn_view' value='查看'>";
                    temp += "<input id='btnIgnore" + index + "' type='button' class='btn_ignore' value='忽略'>";
                    temp += "</div>";
                    temp += "<div style='clear:both;'></div>";
                    temp += "</li>";
                    $(document).off("click", "#btnShow" + index + ",#btnIgnore" + index);
                    $(document).on("click", "#btnShow" + index, { NoticeID: item.ID, ID: item.PublishID, Source: item.BusinessType, From: "IM" }, AtInfo.ShowDetailMessageEvent);
                    $(document).on("click", "#btnIgnore" + index, { Index: index, NoticeID: item.ID }, AtInfo.OffLineIgnoreEvent);

                }
            });
            $(document).sticky(temp);
            $.each(result, function (index, item) {
                $("#liOffline" + index).data({
                    "ID": item.ID,
                    "Source": item.BusinessType
                });
            });
        }
        else {
            $(document).sticky();
        }

    });

    //正在建设中
    $("#aFriendsAttention,#aEnterpriseRecommend,#aApplyIdentification,#aOpenPlatform,#aAdService,#aSelfService").on("click", function (event) {
        $.Alert({ widht: "auto", content: "正在建设中,敬请期待..." });
    });
}

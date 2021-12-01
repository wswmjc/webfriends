Community.Message = function () { }
Community.Message.registerClass("Community.Message");

Community.Message.Topic = null;
Community.Message.PageSize = 5;

Community.Message.Init = function init(topic) {
    Community.Message.Topic = topic;
    $("#sctCommunityTopicList").ReadTemplate(Template.CommunityTopicDetailTpl, function () {
        $("#hTopicTitle").html(Community.Message.Topic.Title);
        $("#spnTopicCreateTime").text(Community.Message.Topic.CreateTime.Format("yyyy-MM-dd"));
        $("#spnTopicMessageCount").text(Community.Message.Topic.MessageCount);
        $("#spnTopicCreater").text(Community.Message.Topic.CreaterName);
        //加载列表
        Community.Message.CommentInit({
            pageStart: 1,
            pageEnd: Community.Message.PageSize * 1
        });
    });
    $("#aGoBack").off("click").on("click", Community.Message.BackEvent);
}

Community.Message.CommentInit = function comment_init(page) {
    //init list
    Community.Message.GetComment(page);
    //init editor
    $("#txtCommunityTopicCommentContent").miicWebEdit({
        id: "txtCommunityTopicCommentContent",
        css: 'weibo-editor-comment',
        placeholder: "说说我的想法...",
        faceid: "aCommunityTopicEmotion",
        submit: "commentCommunityTopicSend",
        facePath: "../../images/arclist/", //表情存放的路径
        charAllowed: -1
    });
    $("#commentCommunityTopicSend").on("click", Community.Message.CommentEvent);
    $("#commentCommunityTopicCancel").on("click", Community.Message.CancelCommentEvent).hide();
    $("#divCommunityTopicMessage").show();
}

Community.Message.GetComment = function get_comment(page) {
    Community.Message.CommentBind(page);
    $.SimpleAjaxPost("service/CommunityService.asmx/GetMessageInfoCount",
        true,
        "{topicID:'" + Community.Message.Topic.ID + "'}",
        function (json) {
            var result = json.d;
            //重写反馈数目
            Community.Message.Topic.MessageCount = result;
            $("#spnTopicMessageCount").text(Community.Message.Topic.MessageCount);

            if (result <= Community.Message.PageSize) {
                $("#divCommunityTopicMessagePage").wPaginate("destroy");
            }
            else {
                $("#divCommunityTopicMessagePage").wPaginate("destroy").wPaginate({
                    theme: "grey",
                    first: "首页",
                    last: "尾页",
                    total: result,
                    index: 0,
                    limit: Community.Message.PageSize,//一页显示数目
                    ajax: true,
                    url: function (i) {
                        var page = {
                            pageStart: i * this.settings.limit + 1,
                            pageEnd: (i + 1) * this.settings.limit
                        };
                        Community.Message.CommentBind(page);
                    }
                });
            }
        });
}

Community.Message.CommentEvent = function CommentEvent(event) {
    var offset = new RegExp("<br\s{0,1}/?>|<reply[^>]*?>.*?</reply>");
    var to_comment_view = {
        PublishID: Community.Message.Topic.ID,
        Content: $("#txtCommunityTopicCommentContent").getContents().replace(offset,"")
    };

    if ($.IsNullOrEmpty(to_comment_view.Content)) {
        $.Alert("评论内容不能为空！");
        return;
    }

    if ($("#txtCommunityTopicCommentContent").data("ToUserID") !== undefined) {
        to_comment_view.ToCommenterID = $("#txtCommunityTopicCommentContent").data("ToUserID");
        to_comment_view.ToCommenterName = $("#txtCommunityTopicCommentContent").data("ToUserName");
    }
    $.SimpleAjaxPost("service/CommunityService.asmx/SubmitMessage",
       true,
       "{commentView:" + $.Serialize(to_comment_view) + "}",
       function (json) {
           var result = json.d;
           if (result.result == true) {
               Community.Message.GetComment({
                   pageStart: 1,
                   pageEnd: Community.Message.PageSize
               });
               $("#txtCommunityTopicCommentContent").setContents("");
               if (!$("#commentCommunityTopicCancel").is(":hidden")) {
                   var $place_holder = $("#txtCommunityTopicCommentContent").next(".weibo-editor-comment");
                   $place_holder.html("说说我的想法...");
                   $("#commentCommunityTopicCancel").hide();
               }
               $("#txtCommunityTopicCommentContent").blur();
               AtInfo.GetAtValidationCount();
           } else {
               console.log("评论失败");
           }
       });
    //阻止冒泡事件
    event.stopPropagation();
}
//取消评论事件
Community.Message.CancelCommentEvent = function CancelCommentEvent(event) {
    $("#txtCommunityTopicCommentContent").setContents("");
    $("#txtCommunityTopicCommentContent").removeData("ToUserID").removeData("ToUserName");
    $("#txtCommunityTopicCommentContent").next(".weibo-editor-comment").html("说说我的想法...");
    $("#txtCommunityTopicCommentContent").blur();
    $("#commentCommunityTopicCancel").hide();
}


Community.Message.CommentBind = function comment_bind(page) {
    $.SimpleAjaxPost("service/CommunityService.asmx/GetMessageInfoList", true,
    "{topicID:'" + Community.Message.Topic.ID + "',page:" + $.Serialize(page) + "}",
    function (json) {
        var result = $.Deserialize(json.d);
        var temp = "";
        if (Array.isArray(result)) {
            $.each(result, function (index, item) {
                temp += "<li>";
                //评论内容
                temp += "<div class='comment-cover'>";
                //头像
                $(document).off("click", "#aShowCommunityMessageListFrom" + index);
                $(document).off("click", "#aShowCommunityMessageListFromName" + index);
                $(document).off("click", "#aShowCommunityMessageListTo" + index);
                if (item.FromCommenterID != window.objPub.UserID && item.FromCommenterIsFriend == Enum.YesNo.No.toString()) {
                    temp += "<a href='javascript:void(0);' id='aShowCommunityMessageListFrom" + index + "' style='cursor:default'>";
                } else {
                    temp += "<a href='javascript:void(0);' id='aShowCommunityMessageListFrom" + index + "' style='cursor:pointer'>";
                    $(document).on("click", "#aShowCommunityMessageListFrom" + index, { UserID: item.FromCommenterID }, Community.Message.ShowDetailUserEvent);
                }
                temp += "<img src='" + item.FromCommenterUrl + "'/></a></div>";
                temp += "<div class='comment-content'>";
                temp += "<div class='friend-name'>";
                if (item.FromCommenterID != window.objPub.UserID && item.FromCommenterIsFriend == Enum.YesNo.No.toString()) {
                    temp += "<a href='javascript:void(0);'  id='aShowCommunityMessageListFromName" + index + "' style='cursor:default'>" + item.FromCommenterName + "</a>";
                } else {
                    temp += "<a href='javascript:void(0);'  id='aShowCommunityMessageListFromName" + index + "' style='cursor:pointer'>" + item.FromCommenterName + "</a>";
                    $(document).on("click", "#aShowCommunityMessageListFromName" + index, { UserID: item.FromCommenterID }, Community.Message.ShowDetailUserEvent);
                }
                if (item.ToCommenterID != null) {
                    temp += "<span>回复</span>";
                    if (item.ToCommenterIsFriend == Enum.YesNo.No.toString()) {
                        temp += "<a href='javascript:void(0);' id='aShowCommunityMessageListTo" + index + "' style='cursor:default'>" + item.ToCommenterName + "</a>";
                    } else {
                        temp += "<a href='javascript:void(0);' id='aShowCommunityMessageListTo" + index + "' style='cursor:pointer'>" + item.ToCommenterName + "</a>";
                        $(document).on("click", "#aShowCommunityMessageListTo" + index, { UserID: item.ToCommenterID }, Community.Message.ShowDetailUserEvent);
                    }
                }
                temp += "</div>";
                temp += "<div class='friend-msg-text'>" + item.Content + "</div>";
                temp += "<div class='friend-msg-info clear-fix'>"
                temp += "<div class='article-info'>" + objPub.GetSimpleTimeFormat(item.CommentTime) + "<span></span></div>";
                temp += "<div class='article-opts-block'><div class='article-opts clear-fix'>";
                temp+="<ul>";
                if (item.FromCommenterID == objPub.UserID) {
                    temp += "<li><span class='icon-optSet icon-img icon-opt-delete'></span><span class='opt-text' id='spnDelCommunityTopicMessageComment" + index + "'>删除</span></li>";
                    $(document).off("click", "#spnDelCommunityTopicMessageComment" + index);
                    $(document).on("click","#spnDelCommunityTopicMessageComment" + index, { ID: item.ID }, Community.Message.DeleteCommentEvent);
                } else {
                    temp += "<li><span class='icon-optSet icon-img icon-opt-comment'></span><span class='opt-text' id='spnCommunityTopicMessageReply" + index + "'>回复</span></li>";
                    $(document).off("click", "#spnCommunityTopicMessageReply" + index);
                    $(document).on("click", "#spnCommunityTopicMessageReply" + index, { ToID: item.FromCommenterID, ToName: item.FromCommenterName }, Community.Message.RecallCommentEvent);
                }
                temp += "</ul>";
                temp+="</div></div></div></div></li>"
            });
            $("#ulCommunityTopicMessageList").empty().append(temp);
            $("#ulCommunityTopicMessageList").closest(".comment-list").show();
        }
        else {
            $("#ulCommunityTopicMessageList").closest(".comment-list").hide();
        }
    });
}
//回复评论事件
Community.Message.RecallCommentEvent = function RecallCommentEvent(event) {
    var reply_str = "<reply>&nbsp;<b>回复</b>&nbsp;" + event.data.ToName + "：</reply>"
    $("#txtCommunityTopicCommentContent").next(".weibo-editor-comment").html(reply_str);
    $("#txtCommunityTopicCommentContent").data({
        "ToUserID": event.data.ToID,
        "ToUserName": event.data.ToName
    });
    //显示取消按钮
    $("#commentCommunityTopicCancel").show();
}
//删除评论事件
Community.Message.DeleteCommentEvent = function DeleteCommentEvent(event) {
    var message_id = event.data.ID;
    $.SimpleAjaxPost("service/CommunityService.asmx/RemoveMessage",
            true,
            "{messageID:'" + message_id + "'}",
             function (json) {
                 var result = json.d;
                 if (result == true) {
                     Community.Message.GetComment({
                         pageStart: 1,
                         pageEnd: Community.Message.PageSize
                     });
                     AtInfo.GetAtValidationCount();
                 } else {
                     console.log("删除评论失败");
                 }
             });
}


Community.Message.BackEvent = function BackEvent(event) {
    $("#ulSubType>li.circle-topic").trigger("click");
    $("#aGoBack").off("click").on("click", Community.Label.BackEvent);
}

Community.Message.ShowDetailUserEvent = function ShowDetailUserEvent(event) {
    var user_id = event.data.UserID;
    $(".main-left").load("../biz/left/moments.html", function (response, status) {
        if (status == "success") {
            objPub.IsMain = true;
            Moments.List.Person.Init(user_id);
        }
    });
}
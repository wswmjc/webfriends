Group.Message = function () { };
Group.Message.registerClass("Group.Message");
Group.Message.Topic = null;
Group.Message.PageSize = 5;

Group.Message.Init = function init(topic, group_id) {
    Group.Message.Topic = topic;
    $("#sctGroupList").ReadTemplate(Template.GroupTopicDetailTpl, function () {
        //read topic
        $("#hTopicTitle").html(Group.Message.Topic.Title);
        $("#spnTopicCreateTime").text(Group.Message.Topic.CreateTime.Format("yyyy-MM-dd"));
        $("#spnTopicMessageCount").text(Group.Message.Topic.MessageCount);
        $("#spnTopicCreater").text(Group.Message.Topic.CreaterName);
        //加载列表
        Group.Message.CommentInit({
            pageStart: 1,
            pageEnd: Group.Message.PageSize * 1
        });
    });

    //返回事件
    $("#aGoBack").off("click").on("click", { ID: group_id }, Group.List.BackEvent);
}

Group.Message.CommentInit = function comment_init(page) {
    //init list
    Group.Message.GetComment(page);
    //init editor
    $("#txtGroupTopicCommentContent").miicWebEdit({
        id: "txtGroupTopicCommentContent",
        css: 'weibo-editor-comment',
        placeholder: "说说我的想法...",
        faceid: "aGroupTopicEmotion",
        submit: "commentGroupTopicSend",
        facePath: '../../images/arclist/', //表情存放的路径
        charAllowed: -1
    });
    //提交评论事件
    $("#commentGroupTopicSend").on("click", Group.Message.CommentEvent);
    $("#commentGroupTopicCancel").on("click", Group.Message.CancelCommentEvent).hide();
    $("#divGroupTopicMessage").show();
}

Group.Message.GetComment = function get_comment(page) {
    Group.Message.CommentBind(page);
    $.SimpleAjaxPost("service/GroupService.asmx/GetMessageInfoCount",
        true,
        "{topicID:'" + Group.Message.Topic.ID + "'}",
        function (json) {
            var result = json.d;
            //重写反馈数目
            Group.Message.Topic.MessageCount = result;
            $("#spnTopicMessageCount").text(Group.Message.Topic.MessageCount);

            if (result <= Group.Message.PageSize) {
                $("#divGroupTopicMessagePage").wPaginate("destroy");
            }
            else {
                $("#divGroupTopicMessagePage").wPaginate("destroy").wPaginate({
                    theme: "grey",
                    first: "首页",
                    last: "尾页",
                    total: result,
                    index: 0,
                    limit: Group.Message.PageSize,//一页显示数目
                    ajax: true,
                    url: function (i) {
                        var page = {
                            pageStart: i * this.settings.limit + 1,
                            pageEnd: (i + 1) * this.settings.limit
                        };
                        Group.Message.CommentBind(page);
                    }
                });
            }
        });
}
//提交评论事件
Group.Message.CommentEvent = function CommentEvent(event) {
    var offset = new RegExp("<br\s{0,1}/?>|<reply[^>]*?>.*?</reply>");
    var to_comment_view = {
        PublishID: Group.Message.Topic.ID,
        Content: $("#txtGroupTopicCommentContent").getContents().replace(offset,"")
    };
    console.log(to_comment_view.Content);
    if ($.IsNullOrEmpty(to_comment_view.Content)) {
        $.Alert("评论内容不能为空！");
        return;
    }

    if ($("#txtGroupTopicCommentContent").data("ToUserID") !== undefined) {
        to_comment_view.ToCommenterID=$("#txtGroupTopicCommentContent").data("ToUserID");
        to_comment_view.ToCommenterName=$("#txtGroupTopicCommentContent").data("ToUserName");
    }
    $.SimpleAjaxPost("service/GroupService.asmx/SubmitMessage",
       true,
       "{commentView:" + $.Serialize(to_comment_view) + "}",
       function (json) {
           var result = json.d;
           if (result.result == true) {
               Group.Message.GetComment({
                   pageStart: 1,
                   pageEnd: Group.Message.PageSize
               });

               $("#txtGroupTopicCommentContent").setContents("");
               if (!$("#commentGroupTopicCancel").is(":hidden")) {
                   var $place_holder = $("#txtGroupTopicCommentContent").next(".weibo-editor-comment");
                   $place_holder.html("说说我的想法...");
                   $("#commentGroupTopicCancel").hide();
               }
               $("#txtGroupTopicCommentContent").blur();
               AtInfo.GetAtValidationCount();
           } else {
               console.log("评论失败");
           }
       });
    //阻止冒泡事件
    event.stopPropagation();
}
//取消评论事件
Group.Message.CancelCommentEvent = function CancelCommentEvent(event) {
    $("#txtGroupTopicCommentContent").setContents("");
    $("#txtGroupTopicCommentContent").removeData("ToUserID").removeData("ToUserName");
    $("#txtGroupTopicCommentContent").next(".weibo-editor-comment").html("说说我的想法...");
    $("#txtGroupTopicCommentContent").blur();
    $("#commentGroupTopicCancel").hide();
}


Group.Message.CommentBind = function comment_bind(page) {
    $.SimpleAjaxPost("service/GroupService.asmx/GetMessageInfoList", true,
    "{topicID:'" + Group.Message.Topic.ID + "',page:" + $.Serialize(page) + "}",
    function (json) {
        var result = $.Deserialize(json.d);
        var temp = "";
        if (Array.isArray(result)) {
            $.each(result, function (index, item) {
                temp += "<li>";
                //评论内容
                temp += "<div class='comment-cover'>";
                //头像
                $(document).off("click", "#aShowMessageListFrom" + index);
                $(document).off("click", "#aShowMessageListFromName" + index);
                $(document).off("click", "#aShowMessageListTo" + index);
                if (item.FromCommenterID != window.objPub.UserID && item.FromCommenterIsFriend == Enum.YesNo.No.toString()) {
                    temp += "<a href='javascript:void(0);' id='aShowMessageListFrom" + index + "' style='cursor:default'>";
                } else {
                    temp += "<a href='javascript:void(0);' id='aShowMessageListFrom" + index + "' style='cursor:pointer'>";
                    $(document).on("click", "#aShowMessageListFrom" + index, { UserID: item.FromCommenterID }, Group.Message.ShowDetailUserEvent);
                }
                temp += "<img src='" + item.FromCommenterUrl + "'/></a></div>";
                temp += "<div class='comment-content'>";
                temp += "<div class='friend-name'>";
                if (item.FromCommenterID != window.objPub.UserID && item.FromCommenterIsFriend == Enum.YesNo.No.toString()) {
                    temp += "<a href='javascript:void(0);'  id='aShowMessageListFromName" + index + "' style='cursor:default'>" + item.FromCommenterName + "</a>";
                } else {
                    temp += "<a href='javascript:void(0);'  id='aShowMessageListFromName" + index + "' style='cursor:pointer'>" + item.FromCommenterName + "</a>";
                    $(document).on("click", "#aShowMessageListFromName" + index, { UserID: item.FromCommenterID }, Group.Message.ShowDetailUserEvent);
                }
                if (item.ToCommenterID != null) {
                    temp += "<span>回复</span>";
                    if (item.ToCommenterIsFriend == Enum.YesNo.No.toString()) {
                        temp += "<a href='javascript:void(0);' id='aShowMessageListTo" + index + "' style='cursor:default'>" + item.ToCommenterName + "</a>";
                    } else {
                        temp += "<a href='javascript:void(0);' id='aShowMessageListTo" + index + "' style='cursor:pointer'>" + item.ToCommenterName + "</a>";
                        $(document).on("click", "#aShowMessageListTo" + index, { UserID: item.ToCommenterID }, Group.Message.ShowDetailUserEvent);
                    }
                }
                temp += "</div>";
                temp += "<div class='friend-msg-text'>" + item.Content + "</div>";
                temp += "<div class='friend-msg-info clear-fix'>"
                temp += "<div class='article-info'>" + objPub.GetSimpleTimeFormat(item.CommentTime) + "<span></span></div>";
                temp += "<div class='article-opts-block'>";
                temp+="<div class='article-opts clear-fix'>";
                temp+="<ul>";
                if (item.FromCommenterID == objPub.UserID) {
                    temp += "<li><span class='icon-optSet icon-img icon-opt-delete'></span><span class='opt-text' id='spnDelGroupTopicMessageComment" + index + "'>删除</span></li>";
                    $(document).off("click", "#spnDelGroupTopicMessageComment" + index);
                    $(document).on("click", "#spnDelGroupTopicMessageComment" + index, { ID: item.ID }, Group.Message.DeleteCommentEvent);
                } else {
                    temp += "<li><span class='icon-optSet icon-img icon-opt-comment'></span><span class='opt-text' id='spnGroupTopicMessageReply" + index + "'>回复</span></li>";
                    $(document).off("click", "#spnGroupTopicMessageReply" + index);
                    $(document).on("click", "#spnGroupTopicMessageReply" + index, { ToID: item.FromCommenterID, ToName: item.FromCommenterName }, Group.Message.RecallCommentEvent);
                }
                temp += "</ul>";
                temp+="</div></div></div></div></li>"
            });
            $("#ulGroupTopicMessageList").empty().append(temp);
            $("#ulGroupTopicMessageList").closest(".comment-list").show();
        }
        else {
            $("#ulGroupTopicMessageList").closest(".comment-list").hide();
        }
    });
}
//回复评论事件
Group.Message.RecallCommentEvent=function RecallCommentEvent(event){
    var reply_str = "<reply>&nbsp;<b>回复</b>&nbsp;" + event.data.ToName + "：</reply>"
    $("#txtGroupTopicCommentContent").next(".weibo-editor-comment").html(reply_str);
    $("#txtGroupTopicCommentContent").data({
        "ToUserID": event.data.ToID,
        "ToUserName": event.data.ToName
    });
    //显示取消按钮
    $("#commentGroupTopicCancel").show();
}
//删除评论事件
Group.Message.DeleteCommentEvent = function DeleteCommentEvent(event) {
    var message_id = event.data.ID;
    $.SimpleAjaxPost("service/GroupService.asmx/RemoveMessage",
            true,
            "{messageID:'" + message_id + "'}",
             function (json) {
                 var result = json.d;
                 if (result == true) {
                     Group.Message.GetComment({
                         pageStart: 1,
                         pageEnd: Group.Message.PageSize
                     });
                     AtInfo.GetAtValidationCount();
                 } else {
                     console.log("删除评论失败");
                 }
             });
}

Group.Message.ShowDetailUserEvent = function ShowDetailUserEvent(event) {
    var user_id = event.data.UserID;
    $(".main-left").load("../biz/left/moments.html", function (response, status) {
        if (status == "success") {
            objPub.IsMain = true;
            Moments.List.Person.Init(user_id);
        }
    });
}
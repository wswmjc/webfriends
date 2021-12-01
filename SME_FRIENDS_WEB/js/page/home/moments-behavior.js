﻿Moments.Behavior = function () { }
Moments.Behavior.registerClass("Moments.Behavior");
Moments.Behavior.CommentPageSize = 5;
//点赞事件
Moments.Behavior.PraiseEvent = function PraiseEvent(event) {
    $(this).addClass("selected").siblings().removeClass("selected");
    var $praise_content = $(this).find(".opt-text");
    var praise_num = parseInt($(this).find(".opt-text .behavior-num").text());
    $.SimpleAjaxPost("service/MomentsService.asmx/Praise",
       true,
       "{publishID:'" + event.data.PublishID + "'}",
        function (json) {
            var result = json.d;
            if (result.result == true) {
                if (result.PrimaryID == "") {//取消赞
                    var new_praise_num = praise_num - 1;
                    $praise_content.html("赞(<span class='behavior-num'>" + new_praise_num + "</span>)");
                } else {//点赞
                    var new_praise_num = praise_num + 1;
                    $praise_content.html("取消赞(<span class='behavior-num'>" + new_praise_num + "</span>)");
                }
            }
            else {
                if (result.PrimaryID == "") {//取消赞
                    console.log("取消赞失败");
                } else {//点赞
                    console.log("点赞失败");
                }
            }
        });
}
//点踩事件
Moments.Behavior.TreadEvent = function TreadEvent(event) {
    $(this).addClass("selected").siblings().removeClass("selected");
    var $thread_content = $(this).find(".opt-text");
    var thread_num = parseInt($(this).find(".opt-text .behavior-num").text());
    $.SimpleAjaxPost("service/MomentsService.asmx/Tread",
       true,
       "{publishID:'" + event.data.PublishID + "'}",
        function (json) {
            var result = json.d;
            if (result.result == true) {
                if (result.PrimaryID == "") {
                    var new_thread_num = thread_num - 1;
                    $thread_content.html("踩(<span class='behavior-num'>" + new_thread_num + "</span>)");
                } else {
                    var new_thread_num = thread_num + 1;
                    $thread_content.html("取消踩(<span class='behavior-num'>" + new_thread_num + "</span>)");
                }
            }
            else {
                if (result.PrimaryID == "") {
                    console.log("取消踩失败");
                } else {
                   console.log("点踩失败");
                }
            }
        });
}

//收藏事件
Moments.Behavior.CollectEvent = function CollectEvent(event) {
    $(this).addClass("selected").siblings().removeClass("selected");
    var $collect_content = $(this).find(".opt-text");
    var collect_num = parseInt($(this).find(".opt-text .behavior-num").text());
    $.SimpleAjaxPost("service/MomentsService.asmx/Collect",
       true,
       "{publishID:'" + event.data.PublishID + "'}",
        function (json) {
            var result = json.d;
            if (result.result == true) {
                if (result.PrimaryID == "") {
                    var new_collect_num = collect_num - 1;
                    $collect_content.html("收藏(<span class='behavior-num'>" + new_collect_num + "</span>)");
                } else {
                    var new_collect_num = collect_num + 1;
                    $collect_content.html("取消收藏(<span class='behavior-num'>" + new_collect_num + "</span>)");
                }
            }
            else {
                if (result.PrimaryID == "") {
                    console.log("取消收藏失败");
                } else {
                   console.log("收藏失败");
                }
            }
        });
}

//举报事件
Moments.Behavior.ReportEvent = function ReportEvent(event) {
    $(this).addClass("selected").siblings().removeClass("selected");
    var $report_content = $(this).find(".opt-text");
    $.SimpleAjaxPost("service/MomentsService.asmx/Report",
      true,
      "{publishID:'" + event.data.PublishID + "'}",
       function (json) {
           var result = json.d;
           if (result.result == true) {
               if (result.PrimaryID == "") {
                   $report_content.html("举报");
               } else {
                   $report_content.html("取消举报");
               }
           }
           else {
               if (result.PrimaryID == "") {
                   console.log("取消举报失败");
               } else {
                   console.log("举报失败");
               }
           }
       });
}

//评论事件
Moments.Behavior.CommentViewEvent = function CommentViewEvent(event) {
    $(this).addClass("selected").siblings().removeClass("selected");
    var publish_id = event.data.PublishID;
    var to_user_id = event.data.CreaterID;
    var to_user_name = event.data.CreaterName;
    var $open = $(this).find(".open-comment");
    var $close = $(this).find(".close-comment");
    var $comment_div = $(this).closest("ul").closest("li").find(".comment-block");
    if ($open.is(":hidden") == false) {
        //读取list
        var page = {
            pageStart: 1,
            pageEnd: Moments.Behavior.CommentPageSize
        };
        $comment_list = $(this).closest("ul").closest("li").find(".comment-list");
        Moments.Behavior.GetComment($comment_list, publish_id, to_user_id, page, event.data.PublishIndex);
        //展示
        $open.hide();
        $close.show();
        //初始化编辑框
        if ($(this).data("initial") == undefined) {
            $comment_div.find(".weibo-editor-comment").miicWebEdit({
                id: $comment_div.find(".weibo-editor-comment").attr("id"),
                css: 'weibo-editor-comment',
                placeholder: "对他说些什么...",
                faceid: $comment_div.find(".public_emotion").attr("id"),
                submit: $comment_div.find(".btn-public").attr("id"),
                facePath: '../../images/arclist/', //表情存放的路径
                charAllowed: -1
            });
            //绑定回复人员
            $comment_div.find(".weibo-editor-comment").data({
                "ToUserID": to_user_id,
                "ToUserName": to_user_name
            });
            $("#commentSend" + event.data.PublishIndex).on("click", { PublishID: publish_id, List: $comment_list, Editor: $comment_div.find(".weibo-editor-comment"), ToUserID: to_user_id, ToUserName: to_user_name, PublishIndex: event.data.PublishIndex }, Moments.Behavior.CommentEvent);
            $("#commentCancel" + event.data.PublishIndex).on("click", { Editor: $comment_div.find(".weibo-editor-comment"), ToUserID: to_user_id, ToUserName: to_user_name, PublishIndex: event.data.PublishIndex }, Moments.Behavior.CancelCommentEvent).hide();
            $(this).data("initial", true);
        }
        $comment_div.show();
    } else {
        //读取评论数目
        var comment_search_view = {
            PublishID: publish_id,
            WithAddress: Enum.YesNo.Yes
        };
        $.SimpleAjaxPost("service/MomentsService.asmx/GetCommentInfoCount",
        true,
        "{commentSearchView:" + $.Serialize(comment_search_view) + "}",
        function (json) {
            var result = json.d;
            $open.html("评论(<span class='behavior-num'>" + result + "</span>)");
            //收起
            $close.hide();
            $open.show();
        });
        $(this).removeClass("selected");
        $comment_div.hide();
    }
}

Moments.Behavior.CancelCommentEvent = function CancelCommentEvent(event) {
    var $editor = event.data.Editor;
    var $place_holder = $editor.next(".weibo-editor-comment");
    $editor.setContents("");
    $editor.data({
        "ToUserID": event.data.ToUserID,
        "ToUserName": event.data.ToUserName
    });
    $place_holder.html("对他说些什么...");
    $editor.blur();
    $("#commentCancel" + event.data.PublishIndex).hide();
}

Moments.Behavior.CommentEvent = function CommentEvent(event) {
    if ($(this).attr("disabled") == "disabled") {
        return;
    }
    var publish_id = event.data.PublishID;
    var publish_index = event.data.PublishIndex;
    var $editor = event.data.Editor;
    var offset = new RegExp("<br\s{0,1}/?>|<reply[^>]*?>.*?</reply>");
    var to_comment_view = {
        PublishID: publish_id,
        Content: $editor.getContents().replace(offset,""),
        ToCommenterID: $editor.data("ToUserID"),
        ToCommenterName: $editor.data("ToUserName")
    };
    if ($.IsNullOrEmpty(to_comment_view.Content)) {
        $.Alert("评论内容不能为空！");
        return;
    }
    var to_user_id = event.data.ToUserID;

    $.SimpleAjaxPost("service/MomentsService.asmx/PersonComment",
       true,
       "{commentView:" + $.Serialize(to_comment_view) + "}",
       function (json) {
           var result = json.d;
           if (result.result == true) {
               Moments.Behavior.GetComment(event.data.List, publish_id,to_user_id, {
                   pageStart: 1,
                   pageEnd: Moments.Behavior.CommentPageSize
               },publish_index);

               $editor.setContents("");
               $editor.data({
                   "ToUserID": event.data.ToUserID,
                   "ToUserName": event.data.ToUserName
               });
               if (!$("#commentCancel" + publish_index).is(":hidden")) {
                   var $place_holder = $editor.next(".weibo-editor-comment");
                   $place_holder.html("对他说些什么...");
                   $("#commentCancel" + publish_index).hide();
               }
               $editor.blur();
           } else {
               console.log("评论失败");
           }
       });
    //阻止冒泡事件
    event.stopPropagation();
}

Moments.Behavior.GetComment = function get_comment($comment_list, publish_id, to_user_id, page,publish_index) {
    Moments.Behavior.CommentBind($comment_list, publish_id, to_user_id, page, publish_index);
    //评论查询视图
    var comment_search_view = {
        PublishID : publish_id,
        WithAddress: Enum.YesNo.Yes
    };

    $.SimpleAjaxPost("service/MomentsService.asmx/GetCommentInfoCount",
        true,
        "{commentSearchView:" + $.Serialize(comment_search_view) + "}",
        function (json) {
            var result = json.d;
            if (result <= Moments.Behavior.CommentPageSize) {
                $comment_list.find(".wPaginate8nPosition").wPaginate("destroy");
            }
            else {
                $comment_list.find(".wPaginate8nPosition").wPaginate("destroy").wPaginate({
                    theme: "grey",
                    first: "首页",
                    last: "尾页",
                    total: result,
                    index: 0,
                    limit: Moments.Behavior.CommentPageSize,//一页显示数目
                    ajax: true,
                    url: function (i) {
                        var page = {
                            pageStart: i * this.settings.limit + 1,
                            pageEnd: (i + 1) * this.settings.limit
                        };
                        Moments.Behavior.CommentBind($comment_list, publish_id, to_user_id, page,publish_index);
                    }
                });
            }
        });
}

Moments.Behavior.CommentBind = function get_comment_bind($comment_list, publish_id, to_user_id, page, publish_index) {
    //评论查询视图
    var comment_search_view = {
        PublishID: publish_id,
        WithAddress: Enum.YesNo.Yes
    };

    $.SimpleAjaxPost("service/MomentsService.asmx/GetCommentInfos",
        true,
        "{commentSearchView:" + $.Serialize(comment_search_view) + ",page:" + $.Serialize(page) + "}",
        function (json) {
            var result = $.Deserialize(json.d);
            if (Array.isArray(result) == true) {
                var temp = "";
                $.each(result, function (index, item) {
                    temp += "<li>";
                    //评论内容
                    temp += "<div class='comment-cover'>";
                    //头像
                    temp += "<a href='javascript:void(0);' style='cursor:pointer' id='aCommentShowUser" + index + "'>";

                    $(document).off("click", "#aCommentShowUser" + index);
                    $(document).on("click", "#aCommentShowUser" + index, { UserID: item.FromCommenterID }, function (event) {
                        var user_id = event.data.UserID;
                        Moments.List.Person.Init(user_id);
                    });

                    temp += "<img src='" + item.FromCommenterUrl + "'/></a></div>";
                    temp += "<div class='comment-content'>";
                    temp += "<div class='friend-name'>";
                    temp += "<a href='javascript:void(0);' id='aCommentShowFromUser" + index + "' style='cursor:pointer'>" + item.FromCommenterRemark + "</a>";

                    $(document).off("click", "#aCommentShowFromUser" + index);
                    $(document).on("click", "#aCommentShowFromUser" + index, { UserID: item.FromCommenterID }, function (event) {
                        var user_id = event.data.UserID;
                        Moments.List.Person.Init(user_id);
                    });

                    $(document).off("click", "#aCommentShowToUser" + index);
                    if (item.ToCommenterID != to_user_id) {
                        temp += "<span>回复</span>";
                        if (item.ToCommenterIsFriend == Enum.YesNo.No.toString()) {
                            temp += "<a href='javascript:void(0);' id='aCommentShowToUser" + index + "'>" + item.ToCommenterRemark + "</a>";
                        } else {
                            temp += "<a href='javascript:void(0);' id='aCommentShowToUser" + index + "' style='cursor:pointer'>" + item.ToCommenterRemark + "</a>";
                            $(document).on("click", "#aCommentShowToUser" + index, { UserID: item.ToCommenterID }, function (event) {
                                var user_id = event.data.UserID;
                                Moments.List.Person.Init(user_id);
                            });
                        }
                    }
                    temp += "</div>";
                    temp += "<div class='friend-msg-text'>" + item.Content + "</div>";
                    temp += "<div class='friend-msg-info clear-fix'>"
                    temp += "<div class='article-info'>" + objPub.GetSimpleTimeFormat(item.CommentTime) + "<span></span></div>";
                    temp += "<div class='article-opts-block'><div class='article-opts clear-fix'><ul>";
                    if (item.FromCommenterID == objPub.UserID) {
                        temp += "<li><span class='icon-optSet icon-img icon-opt-delete'></span><span class='opt-text' id='sDelComment" + publish_index + "-" + index + "'>删除</span></li>";
                    } else {
                        temp += "<li><span class='icon-optSet icon-img icon-opt-comment'></span><span class='opt-text' id='sReply" + publish_index + "-" + index + "'>回复</span></li>";
                    }
                    temp += "</ul></div></div></div></div></li>"
                });
                $comment_list.find("ul").empty().append(temp);
                $.each(result, function (index, item) {
                    if (item.FromCommenterID == window.objPub.UserID) {
                        $("#sDelComment" + publish_index + "-" + index).on("click", { ID: item.ID, PublishID: publish_id, List: $comment_list, ToUserID: to_user_id, PublishIndex: publish_index }, Moments.Behavior.DeleteCommentEvent);
                    } else {
                        $("#sReply" + publish_index + "-" + index).on("click", { ToID: item.FromCommenterID, ToName: item.FromCommenterName }, function (event) {
                            var reply_str = "<reply>&nbsp;<b>回复</b>&nbsp;" + event.data.ToName + "：</reply>"
                            $($(this).closest(".comment-list")
                                    .prev()
                                    .find(".weibo-editor-comment")
                                    .get(1)).html(reply_str);
                            $($(this).closest(".comment-list")
                                    .prev()
                                    .find(".weibo-editor-comment")
                                    .get(0)).data({
                                        "ToUserID": event.data.ToID,
                                        "ToUserName": event.data.ToName
                                    });
                            //显示取消按钮
                            $("#commentCancel" + publish_index).show();
                        });
                    }
                });
                $comment_list.show();
            } else {
                $comment_list.hide();
            }
        });
}

//删除评论
Moments.Behavior.DeleteCommentEvent = function DeleteCommentEvent(event){
    var comment_id = event.data.ID;
    var publish_id = event.data.PublishID;
    var $comment_list = event.data.List;
    var to_user_id = event.data.ToUserID;
    var publish_index = event.data.PublishIndex;
    $.SimpleAjaxPost("service/MomentsService.asmx/RemoveMyComment",
            true,
            "{commentID:'" + comment_id + "'}",
             function (json) {
                 var result = json.d;
                 if (result == true) {
                     Moments.Behavior.GetComment($comment_list, publish_id, to_user_id, {
                         pageStart: 1,
                         pageEnd: Moments.Behavior.CommentPageSize
                     }, publish_index);
                 } else {
                     console.log("删除评论失败");
                 }
             });
}

//删除事件
Moments.Behavior.DeleteEvent = function DeleteEvent(event) {
    var $this = $(this).closest("ul").closest("li");
    var id = event.data.PublishID;
    $.Confirm("请确认是否删除该条信息？", function () {
        $.SimpleAjaxPost("service/MomentsService.asmx/Delete",
             true,
             "{publishID:'" + id + "'}",
              function (json) {
                  var result = json.d;
                  if (result == true) {
                      if (Moments.IsPerson == true) {
                          Moments.List.Person.SetDateList();
                          
                      } else {
                          Moments.List.SetDateList();
                      }
                      Moments.GetPersonStatisticsCount(objPub.UserID);
                      AtInfo.GetAtValidationCount();
                  }
                  else {
                      console.log("删除失败！");
                  }
              });
    });
}

//撤回事件
Moments.Behavior.WithdrawEvent = function WithdrawEvent(event) {
    var $this = $(this).closest("ul").closest("li");
    var edit_status_view = {
        ID: event.data.PublishID,
        EditStatus: Enum.YesNo.Yes
    };
    $.Confirm("请确认是否撤回该条信息？", function () {
        $.SimpleAjaxPost("service/MomentsService.asmx/SetEditStatus",
               true,
               "{editStatusView:" + $.Serialize(edit_status_view) + "}",
                function (json) {
                    var result = json.d;
                    if (result == true) {
                        if (Moments.IsPerson == true) {
                            Moments.List.Person.SetDateList();

                        } else {
                            Moments.List.SetDateList();
                        }
                        Moments.GetPersonStatisticsCount(objPub.UserID);
                        AtInfo.GetAtValidationCount();
                    }
                    else {
                        console.log("撤回失败");
                    }
                });
    });
}
Moments.Behavior.InitAtComment = function init_at_comment(publish, index) {
    var publish_id = publish.ID;
    var to_user_id = publish.CreaterID;
    var to_user_name = publish.CreaterName;
    var $comment_div = $("#sctAtLongDetail .comment-block");
    //读取list
    var page = {
        pageStart: 1,
        pageEnd: Moments.Behavior.CommentPageSize
    };
    $comment_list = $("#sctAtLongDetail .comment-list");
    Moments.Behavior.GetComment($comment_list, publish_id, to_user_id, page, index, function () {
        AtInfo.ResizeDialog();
    });

    //绑定回复人员
    $comment_div.find(".weibo-editor-comment").data({
        "ToUserID": to_user_id,
        "ToUserName": to_user_name
    });
    $("#commentSend" + index).off("click").on("click", {
        PublishID: publish_id, List: $comment_list, Editor: $comment_div.find(".weibo-editor-comment"), ToUserID: to_user_id, ToUserName: to_user_name, PublishIndex: index, CallBack: function () {
            AtInfo.ResizeDialog();
        }
    }, Moments.Behavior.CommentEvent);
    $("#commentCancel" + index).off("click").on("click", { Editor: $comment_div.find(".weibo-editor-comment"), ToUserID: to_user_id, ToUserName: to_user_name, PublishIndex: index }, Moments.Behavior.CancelCommentEvent).hide();
    $comment_div.show();
}

Moments.Behavior.InitImgComment = function init_at_comment(publish, index) {
    var publish_id = publish.ID;
    var to_user_id = publish.CreaterID;
    var to_user_name = publish.CreaterName;
    var $comment_div = $(".imglist-right .comment-block");
    //读取list
    var page = {
        pageStart: 1,
        pageEnd: Moments.Behavior.CommentPageSize
    };
    $comment_list = $(".imglist-right .comment-list");
    Moments.Behavior.GetComment($comment_list, publish_id, to_user_id, page, index);

    //绑定回复人员
    $comment_div.find(".weibo-editor-comment").data({
        "ToUserID": to_user_id,
        "ToUserName": to_user_name
    });
    $("#commentSend" + index).off("click").on("click", {
        PublishID: publish_id, List: $comment_list, Editor: $comment_div.find(".weibo-editor-comment"), ToUserID: to_user_id, ToUserName: to_user_name, PublishIndex: index
    }, Moments.Behavior.CommentEvent);
    $("#commentCancel" + index).off("click").on("click", { Editor: $comment_div.find(".weibo-editor-comment"), ToUserID: to_user_id, ToUserName: to_user_name, PublishIndex: index }, Moments.Behavior.CancelCommentEvent).hide();
    $comment_div.show();
}



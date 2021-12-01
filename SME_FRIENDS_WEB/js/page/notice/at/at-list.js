AtInfo = function () { }
AtInfo.registerClass("AtInfo");
AtInfo.BusinessType = Enum.BusinessType.Moments;
AtInfo.Init = function init() {
    objPub.IsMain = false;
    $("#aGoBack").off("click").on("click", AtInfo.BackEvent);
    if (AtInfo.Moments.Count == 0) {
        $("#spnAtMomentsCount").text("0").hide();
    }
    else {
        $("#spnAtMomentsCount").text(AtInfo.Moments.Count).show();
    }
    if (AtInfo.Community.Count == 0) {
        $("#spnAtCommunityCount").text("0").hide();
    }
    else {
        $("#spnAtCommunityCount").text(AtInfo.Community.Count).show();
    }
    if (AtInfo.Group.Count == 0) {
        $("#spnAtGroupCount").text("0").hide();
    }
    else {
        $("#spnAtGroupCount").text(AtInfo.Group.Count).show();
    }
    AtInfo.Moments.GetAtValidationList();
    //@切换事件
    $("#liAtMoments").on("click", AtInfo.Moments.TagEvent);
    $("#liAtCommunity").on("click", AtInfo.Community.TagEvent);
    $("#liAtGroup").on("click", AtInfo.Group.TagEvent);
    $("#spnReadAll").on("click", AtInfo.ReadAllEvent);
    //触发朋友圈点击事件
    $("#liAtMoments").trigger("click");
}
    AtInfo.ResizeDialog = function resize_dialog() {
    $(".dialog-mask").scrollTop(objPub.WindowScrollTop);
    var dHeight = $("#sctAtLongDetail").height();
    var wHeight = $(window).height();
    $("#sctAtLongDetail").dialog("option", "appendTo", ".dialog-mask");

    //文章很长替换滚动条
    $("html").css("overflow-y", "hidden");
    $(".ui-dialog").css("position", "absolute");
    $(".dialog-mask").show().css({
        "overflow-y": "scroll",
        "height": wHeight
    });
    if (window.removeEventListener) {
        window.removeEventListener('DOMMouseScroll', wheel, false);
    }
    window.onmousewheel = document.onmousewheel = null;
    setTimeout(function () {
        $("#sctAtLongDetail").dialog("option", "position", {
            my: "center",
            at: "center",
            of: window,
            collision: "fit",
            // Ensure the titlebar is always visible
            using: function (pos) {
                var topOffset = $(this).css(pos).offset().top;
                if (topOffset < 0) {
                    $(this).css("top", pos.top - topOffset);
                }
            }
        });
    }, 100);
}
    //返回主页面
    AtInfo.BackEvent = function BackEvent(event) {
        objPub.InitLeftMain(true);
    }
    //设置已读
    AtInfo.SetRead = function set_read(id, business_type) {
        $.SimpleAjaxPost("service/NoticeService.asmx/ReadNotice", true,
            "{id:'" + id + "'}",
            function (json) {
                var result = json.d;
                if (result == true) {
                    if (business_type == Enum.BusinessType.Community) {
                        AtInfo.Community.GetAtValidationList();
                        AtInfo.Community.GetAtValidationCount().done(function (json) {
                            var result = json.d;
                            AtInfo.Community.Count = result;
                            if (AtInfo.Moments.Count == 0) {
                                $("#spnAtCommunityCount").text("0").hide();
                                AtInfo.IsShow();
                            }
                            else {
                                $("#spnAtCommunityCount").text(AtInfo.Community.Count).show();
                            }
                        });
                    }
                    else if (business_type == Enum.BusinessType.Group) {
                        AtInfo.Group.GetAtValidationList();
                        AtInfo.Group.GetAtValidationCount().done(function (json) {
                            var result = json.d;
                            AtInfo.Group.Count = result;
                            if (AtInfo.Group.Count == 0) {
                                $("#spnAtGroupCount").text("0").hide();
                                AtInfo.IsShow();
                            }
                            else {
                                $("#spnAtGroupCount").text(AtInfo.Group.Count).show();
                            }
                        });
                    }
                    else {
                        AtInfo.Moments.GetAtValidationList();
                        AtInfo.Moments.GetAtValidationCount().done(function (json) {
                            var result = json.d;
                            AtInfo.Moments.Count = result;
                            if (AtInfo.Moments.Count == 0) {
                                $("#spnAtMomentsCount").text("0").hide();
                                AtInfo.IsShow();
                            }
                            else {
                                $("#spnAtMomentsCount").text(AtInfo.Moments.Count).show();
                            }
                        });
                    }
                }
                else {
                    console.log("已读失败！");
                }
            });
    }
    //已读事件
    AtInfo.ReadEvent = function ReadEvent(event) {
        var id = event.data.ID;
        var type = event.data.Type;
        $.SimpleAjaxPost("service/NoticeService.asmx/ReadNotice", true,
           "{id:'" + id + "'}",
         function (json) {
             var result = json.d;
             if (result == true) {
                 if (type == Enum.BusinessType.Community) {
                     AtInfo.Community.GetAtValidationList();
                     AtInfo.Community.GetAtValidationCount().done(function (json) {
                         var result = json.d;
                         AtInfo.Community.Count = result;
                         if (AtInfo.Moments.Count == 0) {
                             $("#spnAtCommunityCount").text("0").hide();
                             AtInfo.IsShow();
                         }
                         else {
                             $("#spnAtCommunityCount").text(AtInfo.Community.Count).show();
                         }
                     });
                 }
                 else if (type == Enum.BusinessType.Group) {
                     AtInfo.Group.GetAtValidationList();
                     AtInfo.Group.GetAtValidationCount().done(function (json) {
                         var result = json.d;
                         AtInfo.Group.Count = result;
                         if (AtInfo.Group.Count == 0) {
                             $("#spnAtGroupCount").text("0").hide();
                             AtInfo.IsShow();
                         }
                         else {
                             $("#spnAtGroupCount").text(AtInfo.Group.Count).show();
                         }
                     });
                 }
                 else {
                     AtInfo.Moments.GetAtValidationList();
                     AtInfo.Moments.GetAtValidationCount().done(function (json) {
                         var result = json.d;
                         AtInfo.Moments.Count = result;
                         if (AtInfo.Moments.Count == 0) {
                             $("#spnAtMomentsCount").text("0").hide();
                             AtInfo.IsShow();
                         }
                         else {
                             $("#spnAtMomentsCount").text(AtInfo.Moments.Count).show();
                         }
                     });
                 }
             }
             else {
                 console.log("已读失败！");
             }
         });
    }
    AtInfo.GetAtValidationCount = function get_at_validation_count() {
        $.when(AtInfo.Moments.GetAtValidationCount(), AtInfo.Community.GetAtValidationCount(),AtInfo.Group.GetAtValidationCount()).done(function (momentsCount, communityCount,groupCount) {
            if (momentsCount[0].d != 0 || communityCount[0].d != 0||groupCount[0].d!=0) {
                $("#spnAtCount").show();
                AtInfo.Moments.Count = momentsCount[0].d;
                AtInfo.Community.Count = communityCount[0].d;
                AtInfo.Group.Count = groupCount[0].d;
                AtInfo.Moments.Count == 0 ? $("#spnAtMomentsCount").text(AtInfo.Moments.Count).hide() : $("#spnAtMomentsCount").text(AtInfo.Moments.Count).show();
                AtInfo.Community.Count == 0 ? $("#spnAtCommunityCount").text(AtInfo.Community.Count).hide() : $("#spnAtCommunityCount").text(AtInfo.Community.Count).show();
                AtInfo.Group.Count == 0 ? $("#spnAtGroupCount").text(AtInfo.Group.Count).hide() : $("#spnAtGroupCount").text(AtInfo.Group.Count).show();
            }
            else {
                $("#spnAtMomentsCount,#spnAtCommunityCount,#spnAtGroupCount").text("0").hide();
                $("#spnAtCount").hide();
            }

        });
    }
    AtInfo.IsShow = function is_show() {
        if (AtInfo.Community.Count == 0 && AtInfo.Moments.Count == 0&&AtInfo.Group.Count==0) {
            $("#spnAtCount").hide();
        }
        else {
            $("#spnAtCount").show();
        }
    }


    AtInfo.ShowDetailMessageEvent = function ShowDetailMessageEvent(event) {
        var business_type = event.data.Source;
        var id = event.data.ID;
        var notice_id = event.data.NoticeID;
        var from = event.data.From;
        var url = "service/MomentsService.asmx/GetDetailInformation";
        AtInfo.SetRead(notice_id, business_type);
        $.each($("li[id^='liOffline']"), function (index, item) {
            if ($(item).data("ID") == event.data.NoticeID) {
                $(item).remove();
            }
        });
        if ($("li[id^='liOffline']").length == 0) {
            $("#spnAtCount").removeClass("user-dot");
        }
        $(".sticky").css("height", "auto");
        if (business_type == Enum.BusinessType.Moments) {
            url = "service/MomentsService.asmx/GetDetailInformation";
        }
        else if (business_type == Enum.BusinessType.Community) {
            url = "service/CommunityService.asmx/GetDetailInformation";
        }
        else {//跳转到讨论组
            $.SimpleAjaxPost("service/GroupService.asmx/GetGroupInfoByTopicID", true,
                         "{topicID:'" + id + "'}",
                         function (json) {
                             var result = $.Deserialize(json.d);
                             if (result != null) {
                                 var group_name = result[0].Name;
                                 var group_id = result[0].ID;
                                 $(".main-left").load("../biz/left/group/detail-list.html", function (response, status) {
                                     if (status == "success") {
                                         $("#divGroupName").html(group_name);
                                         Group.List.Init(group_id);
                                     }
                                 });
                             }
                         });
            return;
        }

        $.SimpleAjaxPost(url, true, "{publishID:'" + id + "'}", function (json) {
            var message_obj = $.Deserialize(json.d);
            if (message_obj != null) {
                var publish_info = message_obj.PublishInfo;
                $("#spnAtPerson").html(publish_info.CreaterName);
                if (publish_info.PublishType == Enum.PublishInfoType.Short) {
                    $("#divAtLongTitle").hide();
                    $("#spnAtPublishTime").text(objPub.GetSimpleTimeFormat(publish_info.CreateTime));
                    $("#divAtLongContent").html(publish_info.Content);
                } else {
                    $("#divAtLongTitle").html(publish_info.Title).show();
                    $("#spnAtPublishTime").text(objPub.GetSimpleTimeFormat(publish_info.CreateTime));
                    $("#divAtLongContent").html(publish_info.Content);
                }
                $("#sctAtLongDetail .user-attachment,#sctAtLongDetail .friend-img").remove();
                $("#divAtLongContent").after(Moments.List.GetAccStr(message_obj.AccList));
                //长图片处理
                objPub.Gallery();

                //为图片绑定标志
                $("#sctAtLongDetail .friend-img ul li a").each(function (index, item) {
                    $(item).data({
                        "Wait": true,
                        "PublishID": publish_info.ID
                    });
                });

                //读取行为
                //1、赞
                var $praise_text = $("#liPraiseAt .opt-text");
                if (message_obj.IsPraise == true) {
                    $praise_text.html("取消赞(<span class='behavior-num'>" + publish_info.PraiseNum + "</span>)");
                } else {
                    $praise_text.html("赞(<span class='behavior-num'>" + publish_info.PraiseNum + "</span>)");
                }
                if (business_type == Enum.BusinessType.Moments) {
                    $("#liPraiseAt").off("click").on("click", { PublishID: publish_info.ID }, Moments.Behavior.PraiseEvent);
                } else if (business_type == Enum.BusinessType.Community) {
                    $("#liPraiseAt").off("click").on("click", { PublishID: publish_info.ID }, Community.Behavior.PraiseEvent);
                }
                //2、踩
                var $tread_text = $("#liTreadAt .opt-text");
                if (message_obj.IsTread == true) {
                    $tread_text.html("取消踩(<span class='behavior-num'>" + publish_info.TreadNum + "</span>)");
                } else {
                    $tread_text.html("踩(<span class='behavior-num'>" + publish_info.TreadNum + "</span>)");
                }
                if (business_type == Enum.BusinessType.Moments) {
                    $("#liTreadAt").off("click").on("click", { PublishID: publish_info.ID }, Moments.Behavior.TreadEvent);
                } else if (business_type == Enum.BusinessType.Community) {
                    $("#liTreadAt").off("click").on("click", { PublishID: publish_info.ID }, Community.Behavior.TreadEvent);
                }
                //3、举报
                var $report_text = $("#liReportAt .opt-text");
                if (message_obj.IsReport == true) {
                    $report_text.html("取消举报");
                } else {
                    $report_text.html("举报");
                }
                if (business_type == Enum.BusinessType.Moments) {
                    $("#liReportAt").off("click").on("click", { PublishID: publish_info.ID }, Moments.Behavior.ReportEvent);
                } else if (business_type == Enum.BusinessType.Community) {
                    $("#liReportAt").off("click").on("click", { PublishID: publish_info.ID }, Community.Behavior.ReportEvent);
                }
                //4、收藏
                var $collect_text = $("#liCollectAt .opt-text");
                if (message_obj.IsCollect == true) {
                    $collect_text.html("取消收藏(<span class='behavior-num'>" + publish_info.CollectNum + "</span>)");
                } else {
                    $collect_text.html("收藏(<span class='behavior-num'>" + publish_info.CollectNum + "</span>)");
                }
                if (business_type == Enum.BusinessType.Moments) {
                    $("#liCollectAt").off("click").on("click", { PublishID: publish_info.ID }, Moments.Behavior.CollectEvent);
                } else if (business_type == Enum.BusinessType.Community) {
                    $("#liCollectAt").off("click").on("click", { PublishID: publish_info.ID }, Community.Behavior.CollectEvent);
                }
                //5、评论
                if (business_type == Enum.BusinessType.Moments) {
                    Moments.Behavior.InitAtComment(publish_info, "At");
                } else if (business_type == Enum.BusinessType.Community) {
                    Community.Behavior.InitAtComment(publish_info, "At");
                }
                //$("#sctAtLongDetail").dialog("option", "close", function () {
                //    //行为清除
                //    $("#sctAtLongDetail ul li").removeClass("selected");
                //    $("#sctAtLongDetail .comment-list ul").empty();
                //    $("#sctAtLongDetail .comment-block").hide();
                //});
                $("#sctAtLongDetail").dialog({
                    close: function (event, ui) {
                        //行为清除
                        $("#sctAtLongDetail ul li").removeClass("selected");
                        $("#sctAtLongDetail .comment-list ul").empty();
                        $("#sctAtLongDetail .comment-block").hide();
                    }
                });
                objPub.WindowScrollTop = $(window).scrollTop();
                $("#sctAtLongDetail").dialog("open");
                AtInfo.ResizeDialog();

                if (business_type == Enum.BusinessType.Moments) {
                    objPub.MomentsBrowse(publish_info.ID);
                } else if (business_type == Enum.BusinessType.Community) {
                    objPub.CommunityBrowse(publish_info.ID);
                }

            }
            else {//讨论跳转 跳转到行业圈子
                $.SimpleAjaxPost("service/CommunityService.asmx/GetTopic", true,
                           "{topicID:'" + id + "'}",
                           function (json) {
                               var result = json.d;
                               if (result != null) {
                                   var init_community_id = result.CommunityID;
                                   $(".main-left").load("../biz/left/moments.html", function (response, status) {
                                       if (status == "success") {
                                           objPub.IsMain = true;
                                           Moments.Init(true);
                                           Community.Init(init_community_id);
                                       }
                                   });
                               }
                               else {
                                   $.Alert("对不起，对方已经进行了撤回操作~~");
                               }
                           });
            }
        });
    }
//全部设置已读
    AtInfo.ReadAllEvent = function ReadAllEvent(event) {
        $.SimpleAjaxPost("service/NoticeService.asmx/ReadAllNotice", true,
         "{type:" + AtInfo.BusinessType + "}",
           function (json) {
               var result = json.d;
               if (result == true) {
                   if (AtInfo.BusinessType == Enum.BusinessType.Moments) {
                       AtInfo.Moments.Count = 0;
                   } else if (Enum.BusinessType.Community) {
                       AtInfo.Community.Count = 0;
                   } else if (Enum.BusinessType.Group) {
                       AtInfo.Group.Count = 0;
                   }
                   AtInfo.GetAtValidationCount();
                   $("#ulMyAtList").empty();
                   $("#divEmptyAt").show();
                   $("#spnReadAll").hide();
                   $("#spnEmptyAt").text("暂没有@您的" + Enum.BusinessType.GetDescription(AtInfo.BusinessType) + "信息哦~");
                   $.each($("li[id^='liOffline']"), function (index, item) {
                       if ($(item).data("Source") == AtInfo.BusinessType) {
                           $(item).remove();
                       }
                   });
                   if ($("li[id^='liOffline']").length == 0) {
                       $("#spnAtCount").removeClass("user-dot");
                   }
                   $(".sticky").css("height", "auto");
               } else {
                   console.log("设置全部已读失败");
               }
           });
    }
    //即时消息忽略事件
    AtInfo.OffLineIgnoreEvent = function OffLineIgnoreEvent(event) {
        var id = event.data.NoticeID;
        var index = event.data.Index;
        $.SimpleAjaxPost("service/NoticeService.asmx/IgnoreNotice", true,
        "{id:'" + id + "'}",
          function (json) {
              var result = json.d;
              if (result == true) {
                  $("#liOffline" + index).remove();
                  $(".sticky").css("height", "auto");
                  if ($("li[id^='liOffline']").length == 0) {
                      $("#spnAtCount").removeClass("user-dot");
                  }
              }
          });
    }
Community.PublishInfo = function () { };
//行业圈子话题详细列表
Community.PublishInfo.registerClass("Community.PublishInfo");
Community.PublishInfo.Year = new Date().getFullYear().toString();
Community.PublishInfo.Month = (new Date().getMonth() + 1).toString();
Community.PublishInfo.PageSize =5;
//是否能页面加载
Community.PublishInfo.CanPageLoad = false;
//记录总数
Community.PublishInfo.TotalCount = 0;
//当前索引
Community.PublishInfo.CurrentIndex = 0;
Community.PublishInfo.CommunityID = "";
Community.PublishInfo.LabelID = "";
//分页垃圾处理
Community.PublishInfo.GC = function GC() {
    Community.PublishInfo.TotalCount = 0;
    Community.PublishInfo.CurrentIndex = 0;
    Community.PublishInfo.CanPageLoad = false;
    Community.PublishInfo.OldDocumentHeight = 0;
}

Community.PublishInfo.Init = function init(community_id, label, is_quik_see) {
    //隐藏删除
    $("#spnRemoveCommunity,#spnAddLabel,#spnDeleteLabel").hide();
    Community.PublishInfo.GC();
    Community.PublishInfo.CommunityID = community_id;
    Community.PublishInfo.LabelID = label.ID;
    $("#divCommunityTimeAxis").show().find("ul").css("top","30px");
    //$("#sctShort").dialog({
    //    autoOpen: false,
    //    resizable: false,
    //    width: 750,
    //    modal: true,
    //    title: "发布消息",
    //    open: Moments.Publish.ResetShortEditorEvent,
    //});

    $("#sctCommunityContent").ReadTemplate(Template.CommunityPublishInfoListTpl, function () {
        $("#hSubjectTitle").text(label.Name);
        $("#spnSubjectCreateTime").text(label.CreateTime.Format("yyyy-MM-dd"));
        $("#spnSubjectCreater").text(label.CreaterName)
        $("#spnSubjectPublishInfoCount").text(label.PublishCount);
        Community.PublishInfo.SetDateList();
    });
    if (!is_quik_see) {
        $("#aGoBack").off("click").on("click",{CommunityID:community_id}, Community.PublishInfo.BackEvent);
    }
}

Community.PublishInfo.SetDateList = function set_date_list() {
    var date_view = {
        Year: Community.PublishInfo.Year,
        CommunityID: Community.PublishInfo.CommunityID,
        LabelID: Community.PublishInfo.LabelID
    };
    //返回顶部
    $(window).off("scroll").on("scroll", function (event) {
        if ($(this).scrollTop() >= 135) {
            $(".filter").css({
                "top": "30px",
                "position": "fixed"
            });
        } else {
            $(".filter").css({
                "top": "30px",
                "position": "absolute"
            });
        }
    });

    $.SimpleAjaxPost("service/CommunityService.asmx/GetYears",
       true,
       "{dateView:" + $.Serialize(date_view) + "}",
       function (json) {
           var result = json.d;
           var temp = "";
           if (Array.isArray(result) == true && result.length > 0) {
               //最新发布的信息按钮
               temp += "<li id='liNewest' title='点击查看最近当前月该话题的信息?'>";
               temp += "<a href='javascript:void(0);' class='year' id='aNewestButton'>最近</a>";
               temp += "</li>";
               $(document).off("click", "#aNewestButton");
               $(document).on("click", "#aNewestButton", Community.PublishInfo.GetNewestInfos);
               //循环加载年份
               $.each(result, function (index, item) {
                   temp += "<li id='liYear" + index + "'" + (index == 0 ? " class='selected' " : " ") + ">";
                   temp += "<a href='javascript:void(0);' class='year'>" + item + "</a>";
                   temp += "<ul class='month' id='ulMonthListOf" + item + "'></ul>";
                   temp += "</li>";
                   $(document).off("click", "#liYear" + index);
                   $(document).on("click", "#liYear" + index, { Year: item }, Community.PublishInfo.GetMonthListEvent);
               });
               //最早发布的信息按钮
               temp += "<li id='liOldest'>";
               temp += "<a href='javascript:void(0);' class='year' id='aOldestInfos'>开始</a>";
               temp += "</li>";
               $(document).off("click", "#aOldestInfos");
               $(document).on("click", "#aOldestInfos", Community.PublishInfo.GetOldestInfos);
               //goTop 按钮
               temp += "<li id='liGoTop'>";
               temp += "<div id='divGoTop' class='go-top'>回顶部</div>";
               temp += "</li>";
               $(document).off("click", "#divGoTop");
               $(document).on("click", "#divGoTop", Community.PublishInfo.GoTopEvent);
               $("#ulCommunityDateList").empty().append(temp);
               //时间轴切换年份
               $(".year").on("click", function (event) {
                   var $presentDot = $(event.target);
                   $presentDot.parent().siblings().find("ul").hide();
                   $presentDot.parent().addClass("selected").siblings().removeClass("selected");
               });
               if ($("li[id^='liYear']").length > 0) {
                   date_view.Year = $("li[id^='liYear'].selected .year").text();
                   date_view.Month = "";
               }
               else {
                   //当前月
                   date_view.Month = Moments.List.Person.Month;
               }
               var page = {
                   pageStart: 1,
                   pageEnd: Community.PublishInfo.PageSize * 1
               };
               Community.PublishInfo.Search(date_view, page);
           }
           else {
               //时间轴删除、列表刷新、空提示展示
               $("#divCommunityTimeAxis").remove();
               $("#ulCommunityList").empty();
               $("#divCommunityEmpty").show();
           }
       });
}

Community.PublishInfo.GetMonthListEvent = function GetMonthListEvent(event) {
    var year = event.data.Year;
    var date_view = {
        Year: year,
        CommunityID: Community.PublishInfo.CommunityID,
        LabelID: Community.PublishInfo.LabelID
    };
    $.SimpleAjaxPost("service/CommunityService.asmx/GetMonths",
       true,
       "{dateView:" + $.Serialize(date_view) + "}",
       function (json) {
           var result = json.d;
           var temp = "";
           if (Array.isArray(result)) {
               $.each(result, function (index, item) {
                   temp += "<li id='liMonth" + year + "-" + item + "'><a href='javascript:void(0);'><em class='s-dot'></em>" + item + "月</a></li>";
                   $(document).off("click", "#liMonth" + year + "-" + item).on("click", "#liMonth" + year + "-" + item, { Year: year, Month: item }, Community.PublishInfo.GetMonthInfoListEvent);
               });
           }
           $("#ulMonthListOf" + year).empty().append(temp).show();
       });

    //加载信息列表
   date_view.Month = "";
    var page = {
        pageStart: 1,
        pageEnd: Community.PublishInfo.PageSize * 1
    };
    Community.PublishInfo.Search(date_view, page);
    event.stopPropagation();
}

Community.PublishInfo.GetMonthInfoListEvent = function GetMonthInfoListEvent(event) {
    var year = event.data.Year;
    var month = event.data.Month;
    var $this = $("#liMonth" + year + "-" + month);
    $this.addClass("selected").siblings().removeClass("selected");
    var page = {
        pageStart: 1,
        pageEnd: Community.PublishInfo.PageSize * 1
    };
    var date_view = {
        Year: year,
        Month: month,
        CommunityID: Community.PublishInfo.CommunityID,
        LabelID: Community.PublishInfo.LabelID
    };
    Community.PublishInfo.Search(date_view,page);
    event.stopPropagation();
}


//初始搜索 和 分页
Community.PublishInfo.Search = function search(date_view,page) { 

    Community.PublishInfo.SearchBind(date_view,page,0);
    $.SimpleAjaxPost("service/CommunityService.asmx/GetCommunityPublishInfoCount",
        true,
        "{dateView:" + $.Serialize(date_view) + "}",
        function (json) {
            var result = json.d;
            Community.PublishInfo.TotalCount = result;
            if (result > Community.PublishInfo.PageSize) {
                Community.PublishInfo.CanPageLoad = true;
                $(document).off("scroll").on("scroll", { DateView: date_view }, Community.PublishInfo.ScrollEvent);
            }
        }); 
}
//滚轮事件
Community.PublishInfo.ScrollEvent = function ScrollEvent(event) {
    var date_view = event.data.DateView;
    if (($(document).scrollTop() >= $(document).height() - $(window).height()) && Community.PublishInfo.OldDocumentHeight != $(document).height()) {
        if (Community.PublishInfo.CanPageLoad == true) {
            Community.PublishInfo.CurrentIndex = Community.PublishInfo.CurrentIndex + 1;
            var page = {
                pageStart: Community.PublishInfo.CurrentIndex * Community.PublishInfo.PageSize + 1,
                pageEnd: (Community.PublishInfo.CurrentIndex + 1) * Community.PublishInfo.PageSize
            };
            Community.PublishInfo.SearchBind(date_view, page, Community.PublishInfo.CurrentIndex);
            if (page.pageEnd >= Community.PublishInfo.TotalCount) {
                Community.PublishInfo.CanPageLoad = false;
            }
            Community.PublishInfo.OldDocumentHeight = $(document).height();
        } else {
            if (Community.PublishInfo.TotalCount == 0) {//当没有任何记录的时候，直接注销事件
                $(document).off("scroll");
            } else if (parseInt(Community.PublishInfo.TotalCount / Community.PublishInfo.PageSize) > objPub.MinTipPage) {
                $.Alert("这已经是最后一页了哦~");
                $(document).off("scroll");
                setTimeout(function () {
                    $(".dialog-normal").dialog('close');
                }, 2000);
            }
        }
    }
}
//搜索我关注的用户（包括我的）的博文
Community.PublishInfo.SearchBind = function search_bind(date_view, page, current_index) {
    $("#divCommunityEmpty").hide();
    $.SimpleAjaxPost("service/CommunityService.asmx/GetCommunityPublishInfos",
                         true,
                         "{dateView:" + $.Serialize(date_view) + ",page:" + $.Serialize(page) + "}",
                         function (json) {
                             var result = $.Deserialize(json.d);
                             Community.PublishInfo.HandleResult(result, current_index);
                         });
}

Community.PublishInfo.GetNewestInfos = function get_newest_infos() {
    var page = {
        pageStart: 1,
        pageEnd: Community.PublishInfo.PageSize * 1
    };
    //初始列表加载 当前年当前月份
    //最新取当前年份月份内的所有信息
    var date_view = {
        Year: Community.PublishInfo.Year,
        Month: Community.PublishInfo.Month,
        CommunityID: Community.PublishInfo.CommunityID,
        LabelID: Community.PublishInfo.LabelID
    };
    Community.PublishInfo.Search(date_view, page);
}

Community.PublishInfo.GetOldestInfos = function get_oldest_infos() {
    //重置加载
    Community.PublishInfo.CurrentIndex == 0
    var top_view = {
        Top: 1,
        CommunityID: Community.PublishInfo.CommunityID,
        LabelID: Community.PublishInfo.LabelID
    };
    $("#divCommunityEmpty").hide();
    $.SimpleAjaxPost("service/CommunityService.asmx/GetOldestCommunityPublishInfo",
                         true,
                         "{topView:" + $.Serialize(top_view) + "}",
                         function (json) {
                             var result = $.Deserialize(json.d);
                             Community.PublishInfo.HandleResult(result,0);
                         });
}

Community.PublishInfo.GoTopEvent = function GoTopEvent(event) {
    $("html,body").animate({
        scrollTop: 0
    }, 500);
}
//获取附件表达式
Community.PublishInfo.GetAccStr = function get_acc_str(acc_infos) {
    var photo_str = "";
    var file_str = "";
    var result = "";
    if (acc_infos != null && $.isArray(acc_infos) == true) {
        $.each(acc_infos, function (index, item) {
            if (item.FileType == Enum.AccType.Photo.toString()) {
                photo_str += "<li class='item_img'><a href='" + item.FilePath + "'><img src='" + item.FilePath + "'/></a></li>";
            } else {
                file_str += "<div class='user-attachment'>";
                file_str += "<a target='_blank' href='" + item.FilePath + "'><span class='icon-optSet icon-img  " + Enum.FileType.GetIconClass(parseInt(item.FileType)) + "''></span></a>";
                file_str += "<a href='" + item.FilePath + "' target='_blank'>" + item.FileName + "</a>";
                file_str += "</div>";
            }
        });
    }

    if (photo_str != "") {
        result += "<div class='friend-img'><ul class='gallery gallery-img'>" + photo_str + "</ul></div>";
    }

    if (file_str != "") {
        result += file_str;
    }
    return result;
}

Community.PublishInfo.HandleResult = function handle_result(result,current_index) {
    var temp = "";
    if (result && $.isArray(result) == true && result.length != 0) {
        var photo_str = "";
        var file_str = "";
        $.each(result, function (index, item) {
            var Index = parseInt(current_index * Community.PublishInfo.PageSize) + index;
            photo_str = "";
            file_str = "";
            switch (item.PublishType) {
                case Enum.PublishInfoType.Short.toString():
                    temp += "<li id='liCommunityInfo" + Index + "'>";
                    temp += "<div class='friend-avatar'>";
                    if (item.CreaterID != objPub.UserID && item.IsFriend == Enum.YesNo.No.toString()) {
                        temp += "<div class='friend-avatar-cover' id='divCommunityShowUser" + Index + "' style='cursor:default'><img src='" + item.CreaterUserUrl + "'></div>";
                    } else {
                        temp += "<div class='friend-avatar-cover' id='divCommunityShowUser" + Index + "'><img src='" + item.CreaterUserUrl + "'></div>";
                    }
                    temp += "</div>";
                    temp += "<div class='friend-msg-content'>"
                    temp += "<div class='friend-name'>" + item.CreaterName + "</div>";
                    temp += "<div class='friend-msg-text'>" + item.Content + "</div>";

                    break;
                case Enum.PublishInfoType.Long.toString():
                    temp += "<li id='liCommunityInfo" + Index + "'>";
                    temp += "<div class='friend-avatar'>";
                    if (item.CreaterID != objPub.UserID && item.IsFriend == Enum.YesNo.No.toString()) {
                        temp += "<div class='friend-avatar-cover' id='divCommunityShowUser" + Index + "' style='cursor:default'><img src='" + item.CreaterUserUrl + "'></div>";
                    } else {
                        temp += "<div class='friend-avatar-cover' id='divCommunityShowUser" + Index + "'><img src='" + item.CreaterUserUrl + "'></div>";
                    }
                    temp += "</div>";
                    temp += "<div class='friend-msg-content'>"
                    temp += "<div class='friend-name'>" + item.CreaterName + "</div>";
                    temp += "<div class='article-title'>" + item.Title + "</div>";
                    temp += "<div class='friend-msg-text'>" + item.Content + "</div>";
                    temp += "<div class='friend-article'><a href='javascript:void(0);' id='aShowDetailLong" + Index + "'>查看全部</a></div>";
                    $(document).off("click", "#aShowDetailLong" + Index).on("click", "#aShowDetailLong" + Index, { LongObj: item }, Community.PublishInfo.ShowDetailLongEvent);
                    break;
            }

            $(document).off("click", "#divCommunityShowUser" + Index);
            if (!(item.CreaterID != objPub.UserID && item.IsFriend == Enum.YesNo.No.toString())) {
                $(document).on("click", "#divCommunityShowUser" + Index, { UserID: item.CreaterID }, Community.PublishInfo.ShowDetailUserEvent);
            }
            temp += Community.PublishInfo.GetAccStr(item.AccInfos);
            temp += "<div class='friend-msg-info clear-fix'>";
            temp += "<div class='article-info'>";
            temp += "<span>" + objPub.GetSimpleTimeFormat(item.PublishTime) + "</span>";
            temp += " </div>";
            temp += "<div class='article-opts-block'>";
            temp += "<div class='article-opts clear-fix'>";
            temp += "<ul>";
            //点赞
            temp += "<li id='liCommunityPraise" + Index + "'><span class='icon-optSet icon-img icon-opt-like'></span><span class='opt-text'>";
            if (item.IsPraise == false) {
                temp += "赞";
            } else {
                temp += "取消赞";
            }
            temp += "(<span class='behavior-num'>" + item.PraiseNum + "</span>)</span></li>";
            //点踩
            temp += "<li id='liCommunityTread" + Index + "'><span class='icon-optSet icon-img icon-opt-footprint'></span><span class='opt-text'>";
            if (item.IsTread == false) {
                temp += "踩";
            } else {
                temp += "取消踩";
            }
            temp += "(<span class='behavior-num'>" + item.TreadNum + "</span>)</span></li>";
            //举报
            temp += "<li id='liCommunityReport" + Index + "'><span class='icon-optSet icon-img icon-opt-alarm'></span><span class='opt-text'>";
            if (item.IsReport == false) {
                temp += "举报";
            } else {
                temp += "取消举报";
            }
            temp += "</span></li>";
            //收藏
            if (item.PublishType == Enum.PublishInfoType.Long.toString()) {
                temp += "<li id='liCommunityCollect" + Index + "'><span class='icon-optSet icon-img icon-opt-favorites'></span><span class='opt-text'>";
                if (item.IsCollect == false) {
                    temp += "收藏";
                } else {
                    temp += "取消收藏";
                }
                temp += "(<span class='behavior-num'>" + item.CollectNum + "</span>)</span></li>";
            }
            //评论
            temp += "<li id='liCommunityComment" + Index + "' class='comment-li'><span class='icon-optSet icon-img icon-opt-comment'></span><span class='opt-text'><span class='open-comment'>评论(<span class='behavior-num'>" + item.CommentNum + "</span>)</span><span class='close-comment'> 收起评论</span></span></li>";
            if (item.CreaterID == objPub.UserID) {
                temp += "<li id='liCommunityDelete" + Index + "'><span class='icon-optSet icon-img icon-opt-delete'></span><span class='opt-text'>删除</span></li>";
                if (item.PublishType == Enum.PublishInfoType.Long.toString()) {
                    temp += "<li id='liCommunityWithdraw" + Index + "'><span class='icon-optSet icon-img icon-opt-draft'></span><span class='opt-text'>下线</span></li>";
                }
            }
            temp += "</ul>";
            temp += "</div></div>";
            temp += "</div>";

            //评论列表
            temp += "<div class='comment-block'>";
            temp += "<div class='comment-message'>";
            temp += "<div style='position: relative;'>";
            temp += "<div class='weibo-editor-comment' id='txtCommunityCommentContent" + Index + "' contenteditable='true' tabindex='-1'></div>";
            temp += "<div class='editor-bottom'>";
            temp += "<div class='editor-options'>";
            temp += "<a href='javascript:void(0)' id='aEmotion" + Index + "' title='表情' class='public_emotion'><span class='icon-optSet icon-img icon-face'></span></a>";
            temp += "</div>";
            temp += "</div>";
            temp += "</div>";
            temp += "<div class='comment-submit'>";
            temp += "<input value='取消' type='button' id='commentCancel" + Index + "'>&nbsp;&nbsp;";
            temp += "<input value='提交' type='button' id='commentSend" + Index + "'>";
            temp += "</div>";
            temp += "</div>";
            temp += "<div class='comment-list'>";
            temp += "<ul>";
            temp += "</ul>";
            temp += "<div class='pagination'><div class='wPaginate8nPosition'></div></div>";
            temp += "</div>";
            temp += "</div>";
            //评论列表结束
            temp += "</div></li>";
        });
        if (Community.PublishInfo.CurrentIndex == 0) {
            $("#ulCommunityList").empty().append(temp);
        } else {
            $("#ulCommunityList").append(temp);
        }
        //绑定方法
        $.each(result, function (index, item) {
            var Index = parseInt(current_index * Community.PublishInfo.PageSize) + index;
            //点赞
            $("#liCommunityPraise" + Index).off("click").on("click", { PublishID: item.ID }, Community.Behavior.PraiseEvent);
            //点踩
            $("#liCommunityTread" + Index).off("click").on("click", { PublishID: item.ID }, Community.Behavior.TreadEvent);
            //举报
            $("#liCommunityReport" + Index).off("click").on("click", { PublishID: item.ID }, Community.Behavior.ReportEvent);
            $("#liCommunityCollect" + Index).off("click");
            if (item.PublishType == Enum.PublishInfoType.Long.toString()) {
                //收藏
                $("#liCommunityCollect" + Index).on("click", { PublishID: item.ID }, Community.Behavior.CollectEvent);
            }
            //评论
            $("#liCommunityComment" + Index).off("click").on("click", { PublishID: item.ID, CreaterID: item.CreaterID, CreaterName: item.CreaterName, PublishIndex: Index }, Community.Behavior.CommentViewEvent);
            if (item.CreaterID == objPub.UserID) {
                //删除
                $("#liCommunityDelete" + Index).off("click").on("click", { PublishID: item.ID, CommunityID: item.CommunityID }, Community.Behavior.DeleteEvent);
                if (item.PublishType == Enum.PublishInfoType.Long.toString()) {
                    $("#liCommunityWithdraw" + Index).off("click").on("click", { PublishID: item.ID }, Community.Behavior.WithdrawEvent);
                }
            }
            $("#liCommunityInfo" + Index + " .gallery img").each(function (index, sub_item) {
                $(sub_item).data({
                    "PublishID": item.ID,
                    "Source": Enum.BusinessType.Community,
                    "UserUrl": item.CreaterUserUrl,
                    "Index": Index
                });
            });
        });
        //长图片处理
        objPub.Gallery();
    } else {
        $("#ulCommunityList").empty();
        $("#divCommunityEmpty").show();
    }
}
//长篇博文展示
Community.PublishInfo.ShowDetailLongEvent = function ShowDetailLongEvent(event) {
    var long_obj = event.data.LongObj;
    $("#divLongTitle").html(long_obj.Title);
    $("#spnPublishTime").text(objPub.GetSimpleTimeFormat(long_obj.PublishTime));
    $("#divLongContent").html(long_obj.DetailContent);
    $("#sctLongDetail .user-attachment").remove();
    $("#divLongContent").after(Community.PublishInfo.GetAccStr(long_obj.AccInfos));
    objPub.WindowScrollTop = $(window).scrollTop();
    $("#sctLongDetail").dialog("open");
    
}

Community.PublishInfo.BackEvent = function BackEvent(event) {
    var community_id = event.data.CommunityID;
    $("#ulSubType>li.circle-subject").trigger("click");
    $("#aGoBack").off("click").on("click", Community.Label.BackEvent);
    $("#divCommunityTimeAxis").hide();
    if ($("#spnRemoveCommunity").length != 0) {
        Community.IsCreater(community_id, objPub.UserID).done(function (json) {
            Community.Label.IsCreater = json.d;
            if (Community.Label.IsCreater == true) {
                $("#spnRemoveCommunity").hide();
            }
            else {
                $("#spnRemoveCommunity").show().on("click", { ID: community_id }, Community.Manage.RemoveCommunityEvent);
            }
        });

        Community.IsAdmin(community_id, objPub.UserID).done(function (json) {
            Community.Label.IsAdmin = json.d;
            if (Community.Label.IsAdmin == true) {
                $("#spnAddLabel,#spnDeleteLabel").show();
                var page = { pageStart: 1, pageEnd: Community.Label.PageSize * 1 };
                //添加标签
                $("#spnAddLabel").on("click", { ID: community_id, Page: page }, Community.Label.AddEvent);
                //删除标签
                $("#spnDeleteLabel").on("click", { ID: community_id, Page: page }, Community.Label.RemoveEvent);
            }
            else {
                $("#spnAddLabel,#spnDeleteLabel").hide();
            }
        });
    }
}

Community.PublishInfo.ShowDetailUserEvent = function ShowDetailUserEvent(event) {
    var user_id = event.data.UserID;
    $(".main-left").load("../biz/left/moments.html", function (response, status) {
        if (status == "success") {
            objPub.IsMain = true;
            Moments.List.Person.Init(user_id);
        }
    });
}

Community.PublishInfo.SetDetailInImglist = function setDetailInImglist(publish_id, user_url, index) {
    $("#imgListUrl").attr("src", user_url);
    objPub.CommunityBrowse(publish_id);
    $.SimpleAjaxPost("service/CommunityService.asmx/GetDetailInformation", true, "{publishID:'" + publish_id + "'}", function (json) {
        var message_obj = $.Deserialize(json.d);
        if (message_obj != null) {
            var publish_info = message_obj.PublishInfo;
            $("#divImglistTime").html(objPub.GetSimpleTimeFormat(publish_info.CreateTime));
            $("#aImglistUserName").html(publish_info.CreaterName);
            //读取行为
            //1、赞
            var $praise_text = $("#liPraiseImg .opt-text");
            if (message_obj.IsPraise == true) {
                $praise_text.html("取消赞(<span class='behavior-num'>" + publish_info.PraiseNum + "</span>)");
            } else {
                $praise_text.html("赞(<span class='behavior-num'>" + publish_info.PraiseNum + "</span>)");
            }

            $("#liPraiseImg").off("click").on("click", { PublishID: publish_info.ID, Index: index }, Community.Behavior.PraiseEvent);

            // 2、踩
            var $tread_text = $("#liTreadImg .opt-text");
            if (message_obj.IsTread == true) {
                $tread_text.html("取消踩(<span class='behavior-num'>" + publish_info.TreadNum + "</span>)");
            } else {
                $tread_text.html("踩(<span class='behavior-num'>" + publish_info.TreadNum + "</span>)");
            }

            $("#liTreadImg").off("click").on("click", { PublishID: publish_info.ID, Index: index }, Community.Behavior.TreadEvent);

            //3、举报
            //var $report_text = $("#liReportImg .opt-text");
            //if (message_obj.IsReport == true) {
            //    $report_text.html("取消举报");
            //} else {
            //    $report_text.html("举报");
            //}

            //$("#liReportImg").off("click").on("click", { PublishID: publish_info.ID, Index: index }, Community.Behavior.ReportEvent);

            ////4、收藏
            //var $collect_text = $("#liCollectImg .opt-text");
            //if (message_obj.IsCollect == true) {
            //    $collect_text.html("取消收藏(<span class='behavior-num'>" + publish_info.CollectNum + "</span>)");
            //} else {
            //    $collect_text.html("收藏(<span class='behavior-num'>" + publish_info.CollectNum + "</span>)");
            //}

            //$("#liCollectImg").off("click").on("click", { PublishID: publish_info.ID, Index: index }, Community.Behavior.CollectEvent);

            //5、评论
            Community.Behavior.InitImgComment(publish_info, "Img");
        }
    });
}
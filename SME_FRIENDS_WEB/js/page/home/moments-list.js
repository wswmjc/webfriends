Moments.List = function () { }
Moments.List.registerClass("Moments.List");
//分页
Moments.List.PageSize = 5;
//当前年
Moments.List.Year = new Date().getFullYear().toString();
Moments.List.Month = (new Date().getMonth() + 1).toString();
//是否能页面加载
Moments.List.CanPageLoad = false;
//记录总数
Moments.List.TotalCount = 0;
//当前索引
Moments.List.CurrentIndex = 0;
Moments.List.OldDocumentHeight = 0;
//分页垃圾处理
Moments.List.GC = function GC() {
    Moments.List.TotalCount = 0;
    Moments.List.CurrentIndex = 0;
    Moments.List.CanPageLoad = false;
    Moments.List.OldDocumentHeight = 0;
}
Moments.List.Init = function init() {
    Moments.IsPerson = false;
    Moments.List.GC();
    $("#divTimeAxis").show();
    $(".friend-tabs-content").ReadTemplate(Template.MomentsList, function () {
        //初始化 提示
        $("#spnEmptyTip").text("暂没有朋友圈最新动态哦~");
    });
    Moments.List.SetDateList();
    //滚轮事件

    $(document).off("scroll").on("scroll", { WithTimeAxis: true, FromPerson: false }, window.objPub.ScorllEvent);
}

Moments.List.SetDateList = function set_date_list() {
    var date_view = {
        //当前年
        Year: Moments.List.Year
    };
    $.SimpleAjaxPost("service/MomentsService.asmx/GetYears",
       true,
       "{dateView:" + $.Serialize(date_view) + "}",
       function (json) {
           var result = json.d;
           var temp = "";
           //快速发表按钮
           temp += "<li id='liMyQuickEdit' class='swift-edit'>";
           temp += "<span class='icon-optSet icon-img icon-editor-black' id='spnQuickSend'></span>";
           temp += "</li>";
           $(document).off("click", "#spnQuickSend").on("click", "#spnQuickSend", function (event) {
               $("#sctShort").dialog("open");
           });
           if (Array.isArray(result) == true) {
               //最新发布的信息按钮
               temp += "<li id='liNewest' title='点击查看最近当前月的信息?'>";
               temp += "<a href='javascript:void(0);' class='year' id='aNewestButton'>最近</a>";
               temp += "</li>";
               $(document).off("click", "#aNewestButton").on("click", "#aNewestButton", Moments.List.GetNewestInfos);
               //循环加载年份
               $.each(result, function (index, item) {
                   temp += "<li id='liYear" + index + "'" + (index == 0 ? " class='selected' " : " ") + " >";
                   temp += "<a href='javascript:void(0);' class='year'>" + item + "</a>";
                   temp += "<ul class='month' id='ulMonthListOf" + item + "'></ul>";
                   temp += "</li>";
                   $(document).off("click", "#liYear" + index).on("click", "#liYear" + index, { Year: item }, Moments.List.GetMonthListEvent);
               });

               //最早发布的信息按钮
               temp += "<li id='liOldest'>";
               temp += "<a href='javascript:void(0);' class='year' id='aOldestInfos'>开始</a>";
               temp += "</li>";
               $(document).off("click", "#aOldestInfos").on("click", "#aOldestInfos", Moments.List.GetOldestInfos);
               //goTop 按钮
               temp += "<li id='liGoTop'>";
               temp += "<div id='divGoTop' class='go-top'>回顶部</div>";
               temp += "</li>";
               $(document).off("click", "#divGoTop").on("click", "#divGoTop", Moments.List.GoTopEvent);
               $("#ulDateList").empty().append(temp);
               //时间轴切换年份
               $(".year").on("click", function (event) {
                   var $presentDot = $(event.target);
                   $presentDot.parent().siblings().find("ul").hide();
                   $presentDot.parent().addClass("selected").siblings().removeClass("selected");
               });
               var page = {
                   pageStart: 1,
                   pageEnd: Moments.List.PageSize * 1
               };
               if ($("li[id^='liYear']").length > 0) {
                   date_view.Year = $("li[id^='liYear'].selected .year").text();
                   date_view.Month = "";
               }
               else {
                   //当前月
                   date_view.Month = Moments.List.Month;
               }
               Moments.List.Search(date_view, page);
           }
           else {
               $("#ulDateList").empty().append(temp);
           }
       });
}

Moments.List.GetMonthListEvent = function GetMonthListEvent(event) {
    Moments.List.GC();
    var year = event.data.Year;
    var date_view = {
        Year: year,
        Month:""
    };
    $.SimpleAjaxPost("service/MomentsService.asmx/GetMonths",
       true,
       "{dateView:" + $.Serialize(date_view) + "}",
       function (json) {
           var result = json.d;
           var temp = "";
           if (Array.isArray(result)) {
               $.each(result, function (index, item) {
                   temp += "<li id='liMonth" + year + "-" + item + "'><a href='javascript:void(0);'><em class='s-dot'></em>" + item + "月</a></li>";
                   $(document).off("click", "#liMonth" + year + "-" + item).on("click", "#liMonth" + year + "-" + item, { Year: year, Month: item }, Moments.List.GetMonthInfoListEvent);
               });
           }
           $("#ulMonthListOf" + year).empty().append(temp).show();
       });
    var page = {
        pageStart: 1,
        pageEnd: Moments.List.PageSize * 1
    };
    Moments.List.Search(date_view,page);
}

Moments.List.GetMonthInfoListEvent = function GetMonthInfoListEvent(event) {
    Moments.List.GC();
    var year = event.data.Year;
    var month = event.data.Month;
    var date_view = {
        Year:year,
        Month: month
    };
    var $this = $("#liMonth"+year+"-"+month);
    $this.addClass("selected").siblings().removeClass("selected");
    //加载信息列表
    var page = {
        pageStart: 1,
        pageEnd: Moments.List.PageSize * 1
    };
    Moments.List.Search(date_view,page);
    event.stopPropagation();
}

//初始搜索 和 分页
Moments.List.Search = function search(date_view,page) {
    Moments.List.CanPageLoad = false;
    Moments.List.TotalCount = 0;
    Moments.List.CurrentIndex = 0;
    $(document).off("scroll");
    $("html,body").animate({
        scrollTop: 0
    });
    Moments.List.SearchBind(date_view, page,0);
    $.SimpleAjaxPost("service/MomentsService.asmx/GetSearchMyMomentsCount",
        true,
        "{dateView:" + $.Serialize(date_view) + "}",
        function (json) {
            Moments.List.TotalCount = json.d;
            if (Moments.List.TotalCount > Moments.List.PageSize) {
                Moments.List.CanPageLoad = true;
                $(document).off("scroll").on("scroll", { DateView: date_view }, Moments.List.ScrollEvent);
            }
        });
    }
//滚轮事件
Moments.List.ScrollEvent = function ScrollEvent(event) {
    var date_view = event.data.DateView;
    if ($(document).scrollTop() >= $(document).height() - $(window).height()) { 
        if (Moments.List.CanPageLoad == true) { 
            Moments.List.CurrentIndex = Moments.List.CurrentIndex + 1;
            var page = {
                pageStart: Moments.List.CurrentIndex * Moments.List.PageSize + 1,
                pageEnd: (Moments.List.CurrentIndex + 1) * Moments.List.PageSize
            };
            Moments.List.SearchBind(date_view, page, Moments.List.CurrentIndex);
            if (page.pageEnd >= Moments.List.TotalCount) {
                Moments.List.CanPageLoad = false;
            }
            Moments.List.OldDocumentHeight = $(document).height();
        } else {
            if (Moments.List.TotalCount == 0) {//当没有任何记录的时候，直接注销事件
                $(document).off("scroll");
            } else if (parseInt(Moments.List.TotalCount / Moments.List.PageSize) > objPub.MinTipPage) {
                $.Alert("这已经是最后一页了哦~");
                $(document).off("scroll");
                setTimeout(function () {
                    $(".dialog-normal").dialog('close');
                }, 2000);
            }
            Moments.List.OldDocumentHeight = 0;
        }
    }
}
//搜索我关注的用户（包括我的）的博文
Moments.List.SearchBind = function search_bind(date_view, page, current_index) {
    $("#divMomentsEmpty").hide();
    $.SimpleAjaxPost("service/MomentsService.asmx/SearchMyMoments",
                         true,
                         "{dateView:" + $.Serialize(date_view) + ",page:" + $.Serialize(page) + "}").done(function(json){
                             Moments.List.HandleResult(json, current_index);
                         });  
}
Moments.List.GetNewestInfos = function get_newest_infos() {
    Moments.List.GC();
    var page = {
        pageStart: 1,
        pageEnd: Moments.List.PageSize * 1
    };
    //最新取当前年份月份内的所有信息
    //初始列表加载 当前年当前月份
    var date_view = {
        Year: Moments.List.Year,
        Month: Moments.List.Month
    };
    Moments.List.Search(date_view,page);
}

Moments.List.GetOldestInfos = function get_oldest_infos() {
    //重置加载
    Moments.List.GC();
    $("html,body").animate({
        scrollTop: 0
    }, function () {
        var top_view = {
            Top: 1,
            Belong: Enum.PublishInfoBelong.Self
        };
        $("#divMomentsEmpty").hide();
        $.SimpleAjaxPost("service/MomentsService.asmx/GetMyNewestMoments",
                             true,
                             "{topView:" + $.Serialize(top_view) + "}").done(function (json) {
                                 Moments.List.HandleResult(json,0);
                             });
    });
                       
}

Moments.List.GoTopEvent = function GoTopEvent(event) {
    $("html,body").animate({
        scrollTop: 0
    }, 500);
}
//获取附件表达式
Moments.List.GetAccStr = function get_acc_str(acc_infos) {
    var photo_str = "";
    var file_str = "";
    var result = "";
    if (acc_infos != null && $.isArray(acc_infos) == true && acc_infos.length != 0) {
        $.each(acc_infos, function (index, item) {
            if (item.FileType == Enum.AccType.Photo.toString()) {
                photo_str += "<li class='item_img'><a href='" + item.FilePath + "'><img src='" + item.FilePath + "'/></a></li>";
            } else {
                file_str += "<div class='user-attachment'>";
                file_str += "<a target='_blank' href='" + item.FilePath + "'><span class='icon-optSet icon-img  " + Enum.FileType.GetIconClass(parseInt(item.FileType)) + "''></span></a>";
                file_str +="<a href='" + item.FilePath + "' target='_blank'>" + item.FileName + "</a>";
                file_str +="</div>";
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

Moments.List.HandleResult = function handle_result(json, current_index) {
    var result = $.Deserialize(json.d);
    var temp = "";
    if (result && $.isArray(result) == true && result.length != 0) {
        var photo_str = "";
        var file_str = "";
        $.each(result, function (index, item) { 
            var Index = parseInt(current_index * Moments.List.PageSize) + index;
            photo_str = "";
            file_str = "";
            switch (item.PublishType) {
                case Enum.PublishInfoType.Short.toString():
                    temp += "<li id='liMomentInfo" + Index + "'>";
                    temp += "<div class='friend-avatar' id='divShowUser" + Index + "'>";
                    temp += "<div class='friend-avatar-cover'><img src='" + item.CreaterUserUrl + "'></div>";
                    temp += "</div>";
                    temp += "<div class='friend-msg-content'>"
                    temp += "<div class='friend-name'>" + item.Remark + "</div>";
                    temp += "<div class='friend-msg-text'>" + item.Content + "</div>";
                    
                    break;
                case Enum.PublishInfoType.Long.toString():
                    temp += "<li id='liMomentInfo" + Index + "'>";
                    temp += "<div class='friend-avatar' id='divShowUser"+Index+"'>";
                    temp += "<div class='friend-avatar-cover'><img src='" + item.CreaterUserUrl + "'></div>";
                    temp += "</div>";
                    temp += "<div class='friend-msg-content'>"
                    temp += "<div class='friend-name'>" + item.Remark + "</div>";
                    temp += "<div class='article-title'>" + item.Title + "</div>";
                    temp += "<div class='friend-msg-text'>" + item.Content + "</div>";
                    temp += "<div class='friend-article'><a href='javascript:void(0);' id='aShowDetailLong" + Index + "'>查看全部</a></div>";
                    $(document).off("click", "#aShowDetailLong" + Index).on("click", "#aShowDetailLong" + Index, { LongObj: item }, Moments.List.ShowDetailLongEvent);
                    break;
            }

            $(document).off("click", "#divShowUser" + Index);
            $(document).on("click", "#divShowUser" + Index, { UserID: item.CreaterID }, function (event) {
                var user_id = event.data.UserID;
                Moments.List.Person.Init(user_id);
            });
            temp += Moments.List.GetAccStr(item.AccInfos);
            temp += "<div class='friend-msg-info clear-fix'>";
            temp += "<div class='article-info'>";
            temp += "<span>" + objPub.GetSimpleTimeFormat(item.PublishTime) + "</span>";
            temp += " </div>";
            temp += "<div class='article-opts-block'>";
            temp += "<div class='article-opts clear-fix'>";
            temp += "<ul>";
            //点赞
            temp += "<li id='liMomentsPraise"+Index+"'><span class='icon-optSet icon-img icon-opt-like'></span><span class='opt-text'>";
            if (item.IsPraise == false) {
                temp += "赞";
            } else {
                temp += "取消赞";
            }
            temp += "(<span class='behavior-num'>" + item.PraiseNum + "</span>)</span></li>";
            $(document).off("click", "#liMomentsPraise" + Index);
            $(document).on("click", "#liMomentsPraise" + Index,{ PublishID: item.ID }, Moments.Behavior.PraiseEvent);
            //点踩
            temp += "<li id='liMomentsTread"+Index+"'><span class='icon-optSet icon-img icon-opt-footprint'></span><span class='opt-text'>";
            if (item.IsTread == false) {
                temp += "踩";
            } else {
                temp += "取消踩";
            }
            temp += "(<span class='behavior-num'>" + item.TreadNum + "</span>)</span></li>";
            $(document).off("click", "#liMomentsTread" + Index);
            $(document).on("click","#liMomentsTread" + Index, { PublishID: item.ID }, Moments.Behavior.TreadEvent);
            //举报
            temp += "<li id='liMomentsReport"+Index+"'><span class='icon-optSet icon-img icon-opt-alarm'></span><span class='opt-text'>";
            if (item.IsReport == false) {
                temp += "举报";
            } else {
                temp += "取消举报";
            }
            temp += "</span></li>";
            $(document).off("click", "#liMomentsReport" + Index);
            $(document).on("click","#liMomentsReport" + Index, { PublishID: item.ID }, Moments.Behavior.ReportEvent);
            //收藏
            if (item.PublishType == Enum.PublishInfoType.Long.toString()) {
                temp += "<li id='liMomentsCollect"+Index+"'><span class='icon-optSet icon-img icon-opt-favorites'></span><span class='opt-text'>";
                if (item.IsCollect == false) {
                    temp += "收藏";
                } else {
                    temp += "取消收藏";
                }
                temp += "(<span class='behavior-num'>" + item.CollectNum + "</span>)</span></li>";
                $(document).off("click", "#liMomentsCollect" + Index);
                $(document).on("click","#liMomentsCollect" + Index, { PublishID: item.ID }, Moments.Behavior.CollectEvent);
            }
            //评论
            temp += "<li id='liMomentsComment"+Index+"' class='comment-li'><span class='icon-optSet icon-img icon-opt-comment'></span><span class='opt-text'><span class='open-comment'>评论(<span class='behavior-num'>" + item.CommentNum + "</span>)</span><span class='close-comment'> 收起评论</span></span></li>";
            $(document).off("click", "#liMomentsComment" + Index);
            $(document).on("click", "#liMomentsComment" + Index, { PublishID: item.ID, CreaterID: item.CreaterID, CreaterName: item.CreaterName, PublishIndex: Index }, Moments.Behavior.CommentViewEvent);
            if (item.CreaterID == objPub.UserID) {
                //删除
                temp += "<li id='liMomentsDelete" + Index + "'><span class='icon-optSet icon-img icon-opt-delete'></span><span class='opt-text'>删除</span></li>";
                $(document).off("click", "#liMomentsDelete" + Index);
                $(document).on("click", "#liMomentsDelete" + Index, { PublishID: item.ID }, Moments.Behavior.DeleteEvent);
                if (item.PublishType == Enum.PublishInfoType.Long.toString()) {
                    //撤回
                    temp += "<li id='liMomentsWithdraw" + Index + "'><span class='icon-optSet icon-img icon-opt-draft'></span><span class='opt-text'>下线</span></li>";
                    $(document).off("click", "#liMomentsWithdraw" + Index);
                    $(document).on("click", "#liMomentsWithdraw" + Index, { PublishID: item.ID }, Moments.Behavior.WithdrawEvent);
                }
            }
            temp += "</ul>";
            temp += "</div></div>";
            temp += "</div>";

            //评论列表
            temp += "<div class='comment-block'>";
            temp += "<div class='comment-message'>";
            temp += "<div style='position: relative;'>";
            temp += "<div class='weibo-editor-comment' id='txtMomentsCommentContent" + Index + "' contenteditable='true' tabindex='-1'></div>";
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
        if (Moments.List.CurrentIndex == 0) {
            $("#ulMomentsList").empty().append(temp);
        } else {
            $("#ulMomentsList").append(temp);
        }
        $.each(result, function (index, item) {
            var Index = parseInt(current_index * Moments.List.PageSize) + index;
            $("#liMomentInfo" + Index + " .gallery img").each(function (index, sub_item) {
                $(sub_item).data({
                    "PublishID": item.ID,
                    "Source": Enum.BusinessType.Moments,
                    "UserUrl": item.CreaterUserUrl,
                    "Index": Index
                });
            });
        });
        //长图片处理
        objPub.Gallery();

    } else {
        $("#ulMomentsList").empty();
        $("#divMomentsEmpty").show();
    }
}
//长篇博文展示
Moments.List.ShowDetailLongEvent = function ShowDetailLongEvent(event) {
    var long_obj = event.data.LongObj;
    $("#divLongTitle").html(long_obj.Title);
    $("#spnPublishTime").text(objPub.GetSimpleTimeFormat(long_obj.PublishTime));
    $("#divLongContent").html(long_obj.DetailContent);
    $("#sctLongDetail .user-attachment").remove();
    $("#divLongContent").after(Moments.List.GetAccStr(long_obj.AccInfos));
    objPub.WindowScrollTop = $(window).scrollTop();
    $("#sctLongDetail").dialog("open");
}
//显示图片详情
Moments.List.SetDetailInImglist = function setDetailInImglist(publish_id, user_url, index) {
    $("#imgListUrl").attr("src", user_url);
    objPub.MomentsBrowse(publish_id);
    $.SimpleAjaxPost("service/MomentsService.asmx/GetDetailInformation", true, "{publishID:'" + publish_id + "'}", function (json) {
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

            $("#liPraiseImg").off("click").on("click", { PublishID: publish_info.ID, Index: index }, Moments.Behavior.PraiseEvent);

            // 2、踩
            var $tread_text = $("#liTreadImg .opt-text");
            if (message_obj.IsTread == true) {
                $tread_text.html("取消踩(<span class='behavior-num'>" + publish_info.TreadNum + "</span>)");
            } else {
                $tread_text.html("踩(<span class='behavior-num'>" + publish_info.TreadNum + "</span>)");
            }

            $("#liTreadImg").off("click").on("click", { PublishID: publish_info.ID, Index: index }, Moments.Behavior.TreadEvent);

            //3、举报
            //var $report_text = $("#liReportImg .opt-text");
            //if (message_obj.IsReport == true) {
            //    $report_text.html("取消举报");
            //} else {
            //    $report_text.html("举报");
            //}

            //$("#liReportImg").off("click").on("click", { PublishID: publish_info.ID, Index: index }, Moments.Behavior.ReportEvent);

            ////4、收藏
            //var $collect_text = $("#liCollectImg .opt-text");
            //if (message_obj.IsCollect == true) {
            //    $collect_text.html("取消收藏(<span class='behavior-num'>" + publish_info.CollectNum + "</span>)");
            //} else {
            //    $collect_text.html("收藏(<span class='behavior-num'>" + publish_info.CollectNum + "</span>)");
            //}

            //$("#liCollectImg").off("click").on("click", { PublishID: publish_info.ID, Index: index }, Moments.Behavior.CollectEvent);

            //5、评论
            Moments.Behavior.InitImgComment(publish_info, "Img");
        }
    });
}
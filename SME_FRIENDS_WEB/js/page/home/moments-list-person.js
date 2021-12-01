Moments.List.Person = function () { }
Moments.List.Person.registerClass("Moments.List.Person");
//分页
Moments.List.Person.PageSize = 10;
//当前年
Moments.List.Person.Year = new Date().getFullYear().toString();
Moments.List.Person.Month = (new Date().getMonth() + 1).toString();
//是否能页面加载
Moments.List.Person.CanPageLoad = false;
//记录总数
Moments.List.Person.TotalCount = 0;
//当前索引
Moments.List.Person.CurrentIndex = 0;

Moments.List.Person.UserID = ""; 
Moments.List.Person.OldDocumentHeight = 0;
//分页垃圾处理
Moments.List.Person.GC = function GC() {
    Moments.List.Person.TotalCount = 0;
    Moments.List.Person.CurrentIndex = 0;
    Moments.List.Person.CanPageLoad = false;
    Moments.List.Person.OldDocumentHeight = 0;
}
Moments.List.Person.Init = function init(user_id) {
    Moments.IsPerson = true;
    Moments.List.Person.GC();
    Moments.List.Person.UserID = user_id;
    Moments.List.Person.ReadCard(user_id);
    $(".friend-content").ReadTemplate(Template.MomentsList, function () {
        //初始化 提示
        if (user_id == objPub.UserID) {
            $("#spnEmptyTip").text("您尚未发布过信息哦~");
        } else {
            $("#spnEmptyTip").text("TA尚未发布过信息哦~");
        }
    });
    if (user_id != objPub.UserID) {
        $.SimpleAjaxPost("service/AddressBookService.asmx/CanSeeAddresser",
                            true,
                            "{addresserID:'" + user_id + "'}",
                            function (json) { 
                                var result = json.d;
                                if (result) {//联系人可见
                                    $("#divTimeAxis").show();
                                    Moments.List.Person.SetDateList();
                                   
                                } else {
                                    $("#ulMomentsList").empty();
                                    $("#divMomentsEmpty").show();
                                }
                            });
    } else {
        $("#divTimeAxis").show(); 
        Moments.List.Person.SetDateList();
        
    }
}

Moments.List.Person.ReadCard = function read_card(user_id) {
    $("#imgUserUrl").off("click").css("cursor", "default");

    objPub.GetDetailUserInfo(user_id).done(function (json) { 
        var result = $.Deserialize(json.d);
        $("#divSocialCode").html(result.UserInfo.UserName);
        $("#imgUserUrl").attr("src", result.SocialUserInfo.MicroUserUrl);
    });
    Moments.GetPersonStatisticsCount(user_id);
    if (user_id != objPub.UserID) {
        $("#aBackHome").show().siblings().hide();
        $("#spnMyUserLevel").closest("a").siblings().hide();
    } else {
        $("#aBackHome").show();
    }
    $("#aBackHome").on("click", function () {
        window.objPub.InitLeftMain(true);
    })
}

Moments.List.Person.SetDateList = function set_date_list() {
    var date_view = {
        Year: Moments.List.Person.Year,
        UserID: Moments.List.Person.UserID
    };

    //返回顶部
    $(window).off("scroll").on("scroll", { WithTimeAxis: true ,FromPerson:true}, window.objPub.ScorllEvent);
   

    $.SimpleAjaxPost("service/MomentsService.asmx/GetPersonYears",
       true,
       "{dateView:" + $.Serialize(date_view) + "}",
       function (json) { 
           var result = json.d;
           if (Array.isArray(result) == true&&result.length>0) {
               var temp = "";
               //最新发布的信息按钮
               temp += "<li id='liNewest' title='点击查看TA最近当前月的信息?'>";
               temp += "<a href='javascript:void(0);' class='year' id='aNewestButton'>最近</a>";
               temp += "</li>";
               $(document).off("click", "#aNewestButton").on("click", "#aNewestButton", Moments.List.Person.GetNewestInfos);
               //循环加载年份
                $.each(result, function (index, item) {
                    temp += "<li id='liYear" + index + "' " + (index == 0 ? " class='selected' " : " ") + ">";
                    temp += "<a href='javascript:void(0);' class='year'>" + item + "</a>";
                    temp += "<ul class='month' id='ulMonthListOf" + item + "'></ul>";
                    temp += "</li>";
                    $(document).off("click", "#liYear" + index).on("click", "#liYear" + index, { Year: item }, Moments.List.Person.GetMonthListEvent);
                });
               //最早发布的信息按钮
               temp += "<li id='liOldest'>";
               temp += "<a href='javascript:void(0);' class='year' id='aOldestInfos'>开始</a>";
               temp += "</li>";
               $(document).off("click", "#aOldestInfos").on("click", "#aOldestInfos", Moments.List.Person.GetOldestInfos);
               //goTop 按钮
               temp += "<li id='liGoTop'>";
               temp += "<div id='divGoTop' class='go-top'>回顶部</div>";
               temp += "</li>";
               $(document).off("click", "#divGoTop").on("click", "#divGoTop", Moments.List.Person.GoTopEvent);
               $("#ulDateList").empty().append(temp);
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
                   date_view = Moments.List.Person.Month;
               }
               var page = {
                   pageStart: 1,
                   pageEnd: Moments.List.Person.PageSize * 1
               };
               Moments.List.Person.Search(date_view, page);
           }
           else {
               $("#divTimeAxis").remove();
           }
       });
}

Moments.List.Person.GetMonthListEvent = function GetMonthListEvent(event) {
    var year = event.data.Year;
    var date_view = {
        Year: year,
        UserID: Moments.List.Person.UserID
    };
    $.SimpleAjaxPost("service/MomentsService.asmx/GetPersonMonths",
       true,
       "{dateView:" + $.Serialize(date_view) + "}",
       function (json) {
           var result = json.d;
           var temp = "";
           if (Array.isArray(result)) {
               $.each(result, function (index, item) {
                   temp += "<li id='liMonth" + year + "-" + item + "'><a href='javascript:void(0);'><em class='s-dot'></em>" + item + "月</a></li>";
                   $(document).off("click", "#liMonth" + year + "-" + item).on("click", "#liMonth" + year + "-" + item, { Year: year, Month: item }, Moments.List.Person.GetMonthInfoListEvent);
               });
           }
           $("#ulMonthListOf" + year).empty().append(temp).show();
       });

    //加载信息列表
    date_view.Month = "";
    var page = {
        pageStart: 1,
        pageEnd: Moments.List.Person.PageSize * 1
    };
    Moments.List.Person.Search(date_view,page);
}

Moments.List.Person.GetMonthInfoListEvent = function GetMonthInfoListEvent(event) {
    var year = event.data.Year;
    var month = event.data.Month;
    var date_view = {
        Year: year,
        Month: month,
        UserID: Moments.List.Person.UserID
    };
    var $this = $("#liMonth" + year + "-" + month);
    $this.addClass("selected").siblings().removeClass("selected");
    //加载信息列表
    var page = {
        pageStart: 1,
        pageEnd: Moments.List.Person.PageSize * 1
    };
    Moments.List.Person.Search(date_view,page);
    event.stopPropagation();
}

//初始搜索 和 分页
Moments.List.Person.Search = function search(date_view, page) {
    Moments.List.Person.TotalCount = 0;
    Moments.List.Person.CurrentIndex = 0;
    Moments.List.Person.CanPageLoad = false;
    $("html,body").animate({
        scrollTop: 0
    });
    Moments.List.Person.SearchBind(date_view,page,0);
    $.SimpleAjaxPost("service/MomentsService.asmx/GetSearchPersonMomentsCount",
        true,
        "{dateView:" + $.Serialize(date_view) + "}",
        function (data) { 
            var result = data.d;
            Moments.List.Person.TotalCount = result;
            if (result > Moments.List.Person.PageSize) { 
                Moments.List.Person.CanPageLoad = true;
                $(document).off("scroll").on("scroll", { DateView: date_view }, Moments.List.Person.ScrollEvent);
            }
        }); 
}
//滚轮事件
Moments.List.Person.ScrollEvent = function ScrollEvent(event) {
    var date_view = event.data.DateView;
    if (($(document).scrollTop() >= $(document).height() - $(window).height()) && Moments.List.Person.OldDocumentHeight != $(document).height()) {
        if (Moments.List.Person.CanPageLoad == true) {
            Moments.List.Person.CurrentIndex = Moments.List.Person.CurrentIndex + 1;
            var page = {
                pageStart: Moments.List.Person.CurrentIndex * Moments.List.Person.PageSize + 1,
                pageEnd: (Moments.List.Person.CurrentIndex + 1) * Moments.List.Person.PageSize
            };
            Moments.List.Person.SearchBind(date_view, page, Moments.List.Person.CurrentIndex); 
            if (page.pageEnd >= Moments.List.Person.TotalCount) {
                Moments.List.Person.CanPageLoad = false;
            }
            Moments.List.Person.OldDocumentHeight = $(document).height();
        } else {
            if (Moments.List.Person.TotalCount == 0) {//当没有任何记录的时候，直接注销事件
                $(document).off("scroll");
            } else if (parseInt(Moments.List.Person.TotalCount / Moments.List.Person.PageSize) > objPub.MinTipPage) {
                $.Alert("这已经是最后一页了哦~");
                $(document).off("scroll");
                setTimeout(function () {
                    $(".dialog-normal").dialog('close');
                }, 2000);
            }
            Moments.List.Person.OldDocumentHeight = 0;
        }
    }
}
//搜索我关注的用户（包括我的）的博文
Moments.List.Person.SearchBind = function search_bind(date_view, page, current_index) {
    $("#divMomentsEmpty").hide();
    $.SimpleAjaxPost("service/MomentsService.asmx/SearchPersonMoments",
                         true,
                         "{dateView:" + $.Serialize(date_view) + ",page:" + $.Serialize(page) + "}",
                         function (json) { 
                             var result = $.Deserialize(json.d);
                             Moments.List.Person.HandleResult(result, current_index);
                         });
}
//获取我关注的用户最新博文
Moments.List.Person.GetNewestInfos = function get_newest_infos() {
    //最新取当前年份月份内的所有信息
    var date_view = {
        Year: Moments.List.Person.Year,
        Month: Moments.List.Person.Month,
        UserID: Moments.List.Person.UserID
    };
    var page = {
        pageStart: 1,
        pageEnd: Moments.List.Person.PageSize * 1
    };
    //初始列表加载 当前年当前月份
    Moments.List.Person.Search(date_view,page);
}

Moments.List.Person.GetOldestInfos = function get_oldest_infos() {
    //重置加载
    Moments.List.Person.CurrentIndex == 0
    var top_view = {
        Top: 1,
        Belong: Enum.PublishInfoBelong.Self,
        UserID: Moments.List.Person.UserID
    };
    $("#divMomentsEmpty").hide();
    $.SimpleAjaxPost("service/MomentsService.asmx/GetPersonNewestMoments",
                         true,
                         "{topView:" + $.Serialize(top_view) + "}",
                         function (json) {
                             var result = $.Deserialize(json.d);
                             Moments.List.Person.HandleResult(result,0);
                         });
}

Moments.List.Person.GoTopEvent = function GoTopEvent(event) {
    $("html,body").animate({
        scrollTop: 0
    }, 500);
}
//获取附件表达式
Moments.List.Person.GetAccStr = function get_acc_str(acc_infos) {
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

Moments.List.Person.HandleResult = function handle_result(result,current_index) {
    var temp = "";
    if (result && $.isArray(result) == true && result.length != 0) {
        var photo_str = "";
        var file_str = "";
        $.each(result, function (index, item) {
            var Index = parseInt(current_index * Moments.List.Person.PageSize) + index;
            photo_str = "";
            file_str = "";
            switch (item.PublishType) {
                case Enum.PublishInfoType.Short.toString():
                    temp += "<li id='liMomentInfo" + Index + "'>";
                    temp += "<div class='friend-message-date'>";
                    temp += "<span class='friend-message-month'>" + item.PublishTime.getZhCnMonth() + "</span>";
                    temp += "<span class='friend-message-day'>" + (item.PublishTime.getDate() < 10 ? "0" + item.PublishTime.getDate() : item.PublishTime.getDate()) + "</span>";
                    temp += "</div>";
                    temp += "<div class='friend-msg-content'>"
                    temp += "<div class='friend-msg-text'>" + item.Content + "</div>";

                    break;
                case Enum.PublishInfoType.Long.toString():
                    temp += "<li id='liMomentInfo" + Index + "'>";
                    temp += "<div class='friend-message-date'>";
                    temp += "<span class='friend-message-month'>" + item.PublishTime.getZhCnMonth() + "</span>";
                    temp += "<span class='friend-message-day'>" + (item.PublishTime.getDate() < 10 ? "0" + item.PublishTime.getDate() : item.PublishTime.getDate()) + "</span>";
                    temp += "</div>";
                    temp += "<div class='friend-msg-content'>"
                    temp += "<div class='article-title'>" + item.Title + "</div>";
                    temp += "<div class='friend-msg-text'>" + item.Content + "</div>";
                    temp += "<div class='friend-article'><a href='javascript:void(0);' id='aShowDetailLong" + Index + "'>查看全部</a></div>";
                    $(document).off("click", "#aShowDetailLong" + Index).on("click", "#aShowDetailLong" + Index, { LongObj: item }, Moments.List.Person.ShowDetailLongEvent);
                    break;
            }
            temp += Moments.List.Person.GetAccStr(item.AccInfos);
            temp += "<div class='friend-msg-info clear-fix'>";
            temp += "<div class='article-info'>";
            temp += "<span>" + objPub.GetSimpleTimeFormat(item.PublishTime) + "</span>";
            temp += " </div>";
            temp += "<div class='article-opts-block'>";
            temp += "<div class='article-opts clear-fix'>";
            temp += "<ul>";
            //点赞
            temp += "<li id='liMomentsPraise" + Index + "'><span class='icon-optSet icon-img icon-opt-like'></span><span class='opt-text'>";
            if (item.IsPraise == false) {
                temp += "赞";
            } else {
                temp += "取消赞";
            }
            temp += "(<span class='behavior-num'>" + item.PraiseNum + "</span>)</span></li>";
            $(document).off("click", "#liMomentsPraise" + Index);
            $(document).on("click", "#liMomentsPraise" + Index, { PublishID: item.ID }, Moments.Behavior.PraiseEvent);
            //点踩
            temp += "<li id='liMomentsTread" + Index + "'><span class='icon-optSet icon-img icon-opt-footprint'></span><span class='opt-text'>";
            if (item.IsTread == false) {
                temp += "踩";
            } else {
                temp += "取消踩";
            }
            temp += "(<span class='behavior-num'>" + item.TreadNum + "</span>)</span></li>";
            $(document).off("click", "#liMomentsTread" + Index);
            $(document).on("click", "#liMomentsTread" + Index, { PublishID: item.ID }, Moments.Behavior.TreadEvent);
            //举报
            temp += "<li id='liMomentsReport" + Index + "'><span class='icon-optSet icon-img icon-opt-alarm'></span><span class='opt-text'>";
            if (item.IsReport == false) {
                temp += "举报";
            } else {
                temp += "取消举报";
            }
            temp += "</span></li>";
            $(document).off("click", "#liMomentsReport" + Index);
            $(document).on("click", "#liMomentsReport" + Index, { PublishID: item.ID }, Moments.Behavior.ReportEvent);
            //收藏
            if (item.PublishType == Enum.PublishInfoType.Long.toString()) {
                temp += "<li id='liMomentsCollect" + Index + "'><span class='icon-optSet icon-img icon-opt-favorites'></span><span class='opt-text'>";
                if (item.IsCollect == false) {
                    temp += "收藏";
                } else {
                    temp += "取消收藏";
                }
                temp += "(<span class='behavior-num'>" + item.CollectNum + "</span>)</span></li>";
                $(document).off("click", "#liMomentsCollect" + Index);
                $(document).on("click", "#liMomentsCollect" + Index, { PublishID: item.ID }, Moments.Behavior.CollectEvent);
            }
            //评论
            temp += "<li id='liMomentsComment" + Index + "' class='comment-li'><span class='icon-optSet icon-img icon-opt-comment'></span><span class='opt-text'><span class='open-comment'>评论(<span class='behavior-num'>" + item.CommentNum + "</span>)</span><span class='close-comment'> 收起评论</span></span></li>";
            $(document).off("click", "#liMomentsComment" + Index);
            $(document).on("click", "#liMomentsComment" + Index, { PublishID: item.ID, CreaterID: item.CreaterID, CreaterName: item.CreaterName, PublishIndex: Index }, Moments.Behavior.CommentViewEvent);
            //删除
            if (item.CreaterID == objPub.UserID) {
                temp += "<li id='liMomentsDelete" + Index + "'><span class='icon-optSet icon-img icon-opt-delete'></span><span class='opt-text'>删除</span></li>";
                $(document).off("click", "#liMomentsDelete" + Index);
                $(document).on("click", "#liMomentsDelete" + Index, { PublishID: item.ID }, Moments.Behavior.DeleteEvent);
                temp += "<li id='liMomentsWithdraw" + Index + "'><span class='icon-optSet icon-img icon-opt-draft'></span><span class='opt-text'>下线</span></li>";
                $(document).off("click", "#liMomentsWithdraw" + Index);
                $(document).on("click", "#liMomentsWithdraw" + Index, { PublishID: item.ID }, Moments.Behavior.WithdrawEvent);
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
        if (Moments.List.Person.CurrentIndex == 0) {
            $("#ulMomentsList").empty().append(temp);
        } else {
            $("#ulMomentsList").append(temp);
        }

        $.each(result, function (index, item) {
            var Index = parseInt(current_index * Moments.List.Person.PageSize) + index;
            $("#liMomentInfo" + Index + " .gallery img").each(function (index, sub_item) {
                $(sub_item).data({
                    "PublishID": item.ID,
                    "Source": Enum.BusinessType.Moments,
                    "UserUrl": item.CreaterUserUrl,
                    "Index": Index
                });
            });//
        });

        //长图片处理
        objPub.Gallery();


    } else {
        $("#ulMomentsList").empty();
        $("#divMomentsEmpty").show();
    }
}
//长篇博文展示
Moments.List.Person.ShowDetailLongEvent = function ShowDetailLongEvent(event) {
    var long_obj = event.data.LongObj;
    $("#divLongTitle").html(long_obj.Title);
    $("#spnPublishTime").text(objPub.GetSimpleTimeFormat(long_obj.PublishTime));
    $("#divLongContent").html(long_obj.DetailContent);
    $("#sctLongDetail .user-attachment").remove();
    $("#divLongContent").after(Moments.List.Person.GetAccStr(long_obj.AccInfos));
    objPub.WindowScrollTop = $(window).scrollTop();
    $("#sctLongDetail").dialog("open");
    objPub.MomentsBrowse(long_obj.ID);

}
Announce = function () { }
Announce.registerClass("Announce");
Announce.PageSize = 10;
//是否能页面加载
Announce.CanPageLoad = false;
//记录总数
Announce.TotalCount = 0;
//当前索引
Announce.CurrentIndex = 0;
Announce.OldDocumentHeight = 0;
Announce.SearchView = {
    Valid: Enum.Valid.Valid,
    PlatformType: Enum.PlatformTypeSetting.MiicFriends,
    IsPublish: Enum.YesNo.Yes,
    Year: "",
    Keyword: ""
};
Announce.GC = function GC() {
    Announce.CanPageLoad = false;
    Announce.TotalCount = 0;
    Announce.CurrentIndex = 0;
    Announce.OldDocumentHeight = 0;
}
Announce.Init = function init() {
    //滚轮事件初始化
    $("#divTimeAxis").hide();
    Announce.GC();
    $(".friend-tabs-content").ReadTemplate(Template.AnnounceList, function () {
        //初始化 提示
        $("#spnAnnounceEmptyTip").text("暂没有最新公告哦~");
    });

    $(document).off("scroll");
    $(window).off("scroll").on("scroll", { WithTimeAxis: false}, window.objPub.ScorllEvent);

    var page = {
        pageStart: 1,
        pageEnd: Announce.PageSize * 1
    };

    $("#spnSearch").on("click", { Page: page }, Announce.SearchEvent);
    Announce.Search(page);
}

Announce.SearchEvent = function SearchEvent(event) {
    var page = event.data.Page;
    Announce.Search(page);
}

Announce.SearchBind = function search_bind(page, current_index) {
    $("#divAnnounceEmpty").hide();
    var url = new Array();
    url.push(objPub.ManageUrl);
    url.push("service/AnnounceService.asmx/Search");
    $.SimpleAjaxCors(url, "POST", "{searchView:" + $.Serialize(Announce.SearchView) + ",page:" + $.Serialize(page) + "}").done(function (json) {
        var result = $.Deserialize(json.d);
        var temp = "";
        if (result != null && $.isArray(result) == true && result.length != 0) {
            $.each(result, function (index, item) {
                var Index = parseInt(current_index * Announce.PageSize) + index;
                temp += "<li>";
                temp += "<div class='friend-avatar' style='cursor:default'>";
                temp += "<div class='friend-avatar-cover'>";
                temp += "<a href='javascript:void(0);' style='cursor:default'>";
                temp += "<img src='"+item.CreaterUrl+"'/>";
                temp += "</a>";
                temp += "</div>";
                temp += "</div>";
                temp += "<div class='friend-msg-content'>";
                temp += "<div class='friend-name'>";
                temp += "<a href='javascript:void(0);' style='cursor:default'>"+item.CreaterName+"</a>";
                temp += "</div>";
                temp += "<div class='article-title'>";
                temp += item.Title;
                temp += "</div>";
                temp += "<div class='friend-msg-text'>";
                temp += item.AbbrContent;
                temp += "</div>";
                temp += "<div class='announcement-article'>";
                temp += "<a href='javascript:void(0);' id='aShowDetailAnnounce" + Index + "'>查看全部</a>";
                temp += "</div>";
                temp += "<div class='announcement-time'>" + ((item.PublishTime == null) ? "" : item.PublishTime.format("yyyy-MM-dd HH:mm:ss")) + "</div>";
                temp += " </div>";
                temp += "</li>";
                $(document).off("click", "#aShowDetailAnnounce" + Index);
                $(document).on("click", "#aShowDetailAnnounce" + Index, { AnnounceID: item.ID }, Announce.GetAnnounceEvent);
            });
            if (Announce.CurrentIndex == 0) {
                $("#ulAnnounceList").empty().append(temp);
            } else {
                $("#ulAnnounceList").append(temp);
            }
        }
        else {
            $("#divAnnounceEmpty").show();
            $("#ulAnnounceList").empty();
        }
    });
}

Announce.Search = function search(page) {
    Announce.CanPageLoad = false;
    Announce.TotalCount = 0;
    Announce.CurrentIndex = 0;
    $(document).off("scroll");
    $("html,body").animate({
        scrollTop: 0
    });
    Announce.SearchBind(page,0);

    var url = new Array();
    url.push(objPub.ManageUrl);
    url.push("service/AnnounceService.asmx/GetSearchCount");
    $.SimpleAjaxCors(url, "POST", "{searchView:" + $.Serialize(Announce.SearchView) + "}").done(function (json) {
        var result = json.d;
        Announce.TotalCount = json.d;
        if (result > Announce.PageSize) {
            Announce.CanPageLoad = true;
            $(document).off("scroll").on("scroll", Announce.ScrollEvent);
        }
    }); 
}

//滚轮事件
Announce.ScrollEvent = function ScrollEvent(event) {
    if ($(document).scrollTop() >= $(document).height() - $(window).height()) {
        if (Announce.CanPageLoad == true) {
            Announce.CurrentIndex = Announce.CurrentIndex + 1;
            var page = {
                pageStart: Announce.CurrentIndex * Announce.PageSize + 1,
                pageEnd: (Announce.CurrentIndex + 1) * Announce.PageSize
            };
            Announce.SearchBind(page, Announce.CurrentIndex);
            if (page.pageEnd >= Moments.List.TotalCount) {
                Announce.CanPageLoad = false;
            }
            Announce.OldDocumentHeight = $(document).height();
        } else {
            if (Announce.TotalCount == 0) {//当没有任何记录的时候，直接注销事件
                $(document).off("scroll");
            } else if (parseInt(Announce.TotalCount / Announce.PageSize) >= objPub.MinTipPage) {
                $.Alert("这已经是最后一页了哦~");
                $(document).off("scroll");
                setTimeout(function () {
                    $(".dialog-normal").dialog('close');
                }, 2000);
            }
            Announce.OldDocumentHeight = 0;
        }
    }
}

Announce.GetAnnounceEvent = function GetAnnounceEvent(event) {
    var announce_id = event.data.AnnounceID;
    //加载内容
    var url = new Array();
    url.push(objPub.ManageUrl);
    url.push("service/AnnounceService.asmx/GetDetailInformation");
    $.SimpleAjaxCors(url, "POST", "{announceID:'" + announce_id + "'}").done(function (json) {
        var result = $.Deserialize(json.d);
        if (result != null) {
            var announce = result.Announce[0];
            var access = announce.AccInfos;
            $("#divAnnounceTitle").text(announce.Title);
            $("#spAnnouncePublishTime").text(announce.PublishTime.format("yyyy-MM-dd HH:mm:ss"));
            $("#divAnnounceContent").html(announce.Content);
            if (access.length > 0 && access[0].ID != null) {
                var temp = "";
                $.each(access, function (index, item) {
                    temp += "<div class='user-attachment'>";
                    temp += "<span class='icon-optSet icon-img " + Enum.FileType.GetIconClass(parseInt(item.FileType)) + "'></span>";
                    temp += "<a href='" + item.FilePath + "' target='_blank'>" + item.FileName + "</a>";
                    temp += "</div>";
                });
                $("#divAnnounceAccessoryList").empty().append(temp);
            }
            else {
                $("#divAnnounceAccessoryList").empty();
            }
            $("#sctAnnounce").dialog("open");
        }
    });

}
Collect.Community = function () { }
Collect.Community.registerClass("Collect.Community");
Collect.Community.Init = function init() {
    Collect.Community.Search({ pageStart: 1, pageEnd: Collect.PageSize * 1 });
    //搜索框回车事件绑定
    $("#txtKeyword").off("keypress").on("keypress", function (event) {
        if (event.which == 13) {
            Collect.Community.Search({ pageStart: 1, pageEnd: Collect.PageSize * 1 });
        }
    });
    //搜索我关注的包括我的微博
    $("#spnSearch").off("click").on("click", { Page: { pageStart: 1, pageEnd: Collect.PageSize * 1 } }, Collect.Community.SearchEvent);
}

//搜索博文事件
Collect.Community.SearchEvent = function SearchEvent(event) {
    var page = event.data.Page;
    Collect.Community.Search(page);
}

//分页搜索
Collect.Community.Search = function search(page) {
    Collect.Community.SearchBind(page);
    var keyword_view = {
        Keyword: $("#txtKeyword").val()
    };
    $.SimpleAjaxPost("service/CommunityService.asmx/GetCollectInfoCount", true,
        "{keywordView:" + $.Serialize(keyword_view) + "}",
        function (json) {
            var result = json.d;
            if (result <= Collect.PageSize) {
                $("#divCollectPage").wPaginate("destroy");
            }
            else {
                $("#divCollectPage").wPaginate("destroy").wPaginate({
                    theme: "grey",
                    first: "首页",
                    last: "尾页",
                    total: result,
                    index: 0,
                    limit: Collect.PageSize,//一页显示数目
                    ajax: true,
                    url: function (i) {
                        var page = {
                            pageStart: i * this.settings.limit + 1,
                            pageEnd: (i + 1) * this.settings.limit
                        };
                        Collect.Community.SearchBind(page);
                    }
                });
            }
        });
}

//搜索我关注的用户（包括我的）的博文
Collect.Community.SearchBind = function search_bind(page) {
    var keyword_view = {
        Keyword: $("#txtKeyword").val()
    };
    $("#divCollectEmpty").hide();
    $.SimpleAjaxPost("service/CommunityService.asmx/GetCollectInfos", true,
                         "{keywordView:" + $.Serialize(keyword_view) + ",page:" + $.Serialize(page) + "}",
                         function (json) {
                             var result = $.Deserialize(json.d);
                             if (Array.isArray(result) && result.length != 0) {
                                 var temp = "";
                                 var photo_str = "";
                                 var file_str = "";
                                 $.each(result, function (index, item) {
                                     photo_str = "";
                                     file_str = "";
                                     temp += "<li id='liMomentInfo" + index + "'>";
                                     temp += "<div class='friend-avatar'>";
                                     temp += "<div class='friend-avatar-cover'><img src='" + item.CreaterUserUrl + "'></div>";
                                     temp += "</div>";
                                     temp += "<div class='friend-msg-content'>"
                                     temp += "<div class='friend-name'>" + item.CreaterName + "</div>";
                                     temp += "<div class='article-title'>" + item.Title + "</div>";
                                     temp += "<div class='friend-msg-text'>" + item.Content + "</div>";
                                     temp += "<div class='friend-article'><a href='javascript:void(0);' id='aShowDetailLong" + index + "'>查看全部</a></div>";
                                     $(document).off("click", "#aShowDetailLong" + index).on("click", "#aShowDetailLong" + index, { LongObj: item }, Collect.Community.ShowDetailLongEvent);
                                     temp += Moments.List.GetAccStr(item.AccInfos);
                                     temp += "<div class='friend-msg-info clear-fix'>";
                                     temp += "<div class='article-info'>";
                                     temp += "<span>" + objPub.GetSimpleTimeFormat(item.PublishTime) + "</span>";
                                     temp += " </div>";
                                     temp += "<div class='article-opts-block'>";
                                     temp += "<div class='article-opts clear-fix'>";
                                     temp += "<ul>";
                                     temp += "<li id='liCancelCollect" + index + "'><span class='icon-optSet icon-img icon-opt-favorites'></span><span class='opt-text'>取消收藏</span></li>";
                                     temp += "</ul></div>";
                                     temp += "</div>";
                                     temp += "</li>";
                                 });
                                 $("#ulCollectList").empty().append(temp);
                                 //绑定方法

                                 $.each(result, function (index, item) {
                                     //取消收藏
                                     $("#liCancelCollect" + index).on("click", { CollectID: item.ID }, Collect.Community.DeleteCollectEvent);
                                 });
                             } else {
                                 $("#ulCollectList").empty();
                                 $("#divCollectEmpty").show();
                             }
                         });
}

Collect.Community.DeleteCollectEvent = function DeleteCollectEvent(event) {
    var id = event.data.CollectID;
    var $this = $(this).closest("ul").closest("li");
    var has_sibling = $this.siblings().length == 0 ? false : true;
    $.Confirm({
        content: "请确认是否取消对该条信息的收藏？",
        width: 400
    }, function () {
        $.SimpleAjaxPost("service/CommunityService.asmx/CancelCollect",
             true,
             "{collectID:'" + id + "'}",
              function (json) {
                  var result = json.d;
                  if (result == true) {
                      $this.remove();
                      if (!has_sibling) {
                          $("#divCollectEmpty").show();
                      }
                  }
                  else {
                      console.log("Collect.Community.DeleteCollectEvent:取消失败！");
                  }
              });
    });
}

//长篇博文展示
Collect.Community.ShowDetailLongEvent = function ShowDetailLongEvent(event) {
    var long_obj = event.data.LongObj;
    $("#divLongTitle").html(long_obj.Title);
    $("#spnPublishTime").text(objPub.GetSimpleTimeFormat(long_obj.PublishTime));
    $("#divLongContent").html(long_obj.DetailContent);
    $("#sctLongDetail .user-attachment").remove();
    $("#divLongContent").after(Moments.List.GetAccStr(long_obj.AccInfos));
    objPub.WindowScrollTop = $(window).scrollTop();
    $("#sctLongDetail").dialog("open");
}
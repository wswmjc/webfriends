Community.Recommend = function () {
//行业圈子热门推荐业务逻辑
}
Community.Recommend.registerClass("Community.Recommend");
Community.Recommend.PageSize = 10;
//点击热门活动事件
Community.Recommend.RecommendCommunityEvent = function RecommendCommunityEvent(event) {
    var page = event.data.Page;
    $(".circle-tabs>ul>li").removeClass("selected");
    $(event.target).addClass("selected");
    var keyword = {
        Keyword: $("#txtKeyword").val()
    };
    Community.Recommend.Search(keyword, page);
    //查询热门推荐事件
    $("#spnSearch").off("click").on("click", { Page: page }, Community.Recommend.SearchEvent);
    $("#txtKeyword").off("keypress").on("keypress", function (event) {
        if (event.which == 13) {
            Community.Recommend.Search(keyword, { pageStart: 1, pageEnd: Community.Manage.PageSize * 1 });
            return false;
        }
    });
}
Community.Recommend.Search = function search(keyword, page) {
    Community.Recommend.SearchBind(keyword, page);
    $.SimpleAjaxPost("service/CommunityService.asmx/GetRecommendSearchCount", true,
      "{keywordView:" + $.Serialize(keyword) + "}",
       function (json) {
           var result = json.d;
           if (result <= Community.Recommend.PageSize) {
               $("#wPaginate8nPosition").wPaginate("destroy");
           }
           else {
               $("#wPaginate8nPosition").wPaginate("destroy").wPaginate({
                   theme: "grey",
                   first: "首页",
                   last: "尾页",
                   total: result,
                   index: 0,
                   limit: Community.Recommend.PageSize,
                   ajax: true,
                   url: function (i) {
                       var page = {
                           pageStart: i * this.settings.limit + 1,
                           pageEnd: (i + 1) * this.settings.limit
                       };
                       Community.Recommend.SearchBind(keyword, page);
                   }
               });
           }
       });
}
Community.Recommend.SearchEvent = function SearchEvent(event) {
    var page = event.data.Page;
    var keyword = {
        Keyword: $("#txtKeyword").val()
    };
    Community.Recommend.Search(keyword, page);
}
Community.Recommend.SearchBind = function search_bind(keyword, page) {
    $.SimpleAjaxPost("service/CommunityService.asmx/RecommendSearch", true, "{keywordView:" + $.Serialize(keyword) + ",page:" + $.Serialize(page) + "}",
        function (json) {
            var result = $.Deserialize(json.d);
            var temp = "";
            if (result != null) {
                $.each(result, function (index, item) {
                    temp += "<li class='clear-fix clear-fix'>";
                    temp += "<div class='circle-manage-photo' id='divRecommendPhoto" + index + "' " + ((item.CreaterID != objPub.UserID && item.JoinTime == null) ? "style='cursor:default;'" : "") + "> <img src='" + item.LogoUrl + "' " + ((item.CreaterID != objPub.UserID && item.JoinTime == null) ? "" : "title='点击进"+item.Name+"圈子看看？'") + "></div>";
                    temp += "<div class='circle-info'>";
                    temp += "<div class='circle-title clear-fix'>";
                    temp += "<div id='divRecommendName" + index + "' class='circle-name'" + ((item.CreaterID != objPub.UserID && item.JoinTime == null) ? "" : "style='cursor:pointer;'") + ((item.CreaterID != objPub.UserID && item.JoinTime == null) ? "" : "title='点击进" + item.Name + "圈子看看？'") + ">" + item.Name + "</div>";
                    $(document).off("click", "#divRecommendPhoto" + index+",#divRecommendName"+index);
                    if (!(item.CreaterID != objPub.UserID && item.JoinTime == null)) {
                        $(document).on("click", "#divRecommendPhoto" + index + ",#divRecommendName" + index, { ID: item.ID, Name: item.Name }, function (event) {
                            var community_id = event.data.ID;
                            var community_name = event.data.Name;
                            $(".main-left").ReadTemplate(Template.DetailCommunityTpl, function () {
                                $("#divCommunityName").text(community_name);
                                Community.Label.Init(community_id);
                            });
                        });
                    }
                    else {
                       
                    }
                    temp += "<div class='circle-time'>";
                    if (item.CreaterID == objPub.UserID) {
                        temp += "<span class='icon-optSet icon-img icon-manager'></span><span>创建时间：</span><span>" + item.CreateTime.format("yyyy-MM-dd") + "</span>";
                    }
                    else {
                        if (item.JoinTime != null) {
                            temp += "<span>加入时间：</span><span>" + item.CreateTime.format("yyyy-MM-dd") + "</span>";
                        }

                    }
                    temp += "</div>";
                    temp += "</div>";
                    temp += "<div class='circle-intro'>";
                    temp += "<div class='circle-text' id='divRecommendRemark" + index + "'>" + item.Remark + "</div>";
                    temp += "<div class='circle-text-full' id='divRecommendRemarkFull" + index + "'>" + item.Remark + "</div>";
                    temp += "<div class='circle-total clear-fix'>";
                    temp += "<ul>";
                    temp += "<li><span>成员数</span><span>" + item.MemberCount + "</span></li>";
                    temp += "<li><span>话题数</span><span>" + item.TopicCount + "</span></li>";
                    temp += "<li><span>讨论数</span><span>" + item.MessageCount + "</span></li>";
                    temp += "</ul>";
                    temp += "<span class='show-all' title='查看简介全文' id='spnRecommendShowFullRemark" + index + "'>展开</span>";
                    temp += "</div>";
                    temp += "<div class='circle-btns'>";
                    if (item.CreaterID != objPub.UserID && item.JoinTime == null) {
                        temp += "<input id='btnApply" + index + "' type='button' value='申请加入' class='apply-circle'>";
                        $(document).off("click", "#btnApply" + index);
                        $(document).on("click", "#btnApply" + index, { CommunityID: item.ID, CommunityName: item.Name, CreaterName: item.CreaterName, CreateTime: item.CreateTime }, Community.Recommend.AddEvent);
                    }

                    temp += "</div>";
                    temp += "</div>";
                    temp += "</div>";
                    temp += "</li>";

                });
                $("#divEmptyCommunity").hide();
            }
            else {
                $("#divEmptyCommunity").show();

            }
            $("#ulCommunityList").empty().append(temp);
            $.each(result, function (index, item) {
                var temp_remark = $("#divRecommendRemark" + index);
                var temp_remark_noover = $("#divRecommendRemarkFull" + index);
                $("#spnRecommendShowFullRemark" + index).off("click");
                if (temp_remark_noover.height() > temp_remark.height() + 1) {//剔除结尾回车情况
                    $("#spnRecommendShowFullRemark" + index).show().on("click", { Index: index }, Community.Recommend.ShowDetailRemarkEvent);
                }
            });
        });
}
//展开/收起备注事件
Community.Recommend.ShowDetailRemarkEvent = function ShowDetailRemarkEvent(event) {
    var index = event.data.Index;
    if ($(event.target).hasClass("isopen")) {
        $(event.target).removeClass("isopen");
        $("#divRecommendRemark" + index).show();
        $("#divRecommendRemarkFull" + index).hide();
        $(event.target).text("展开");
        $(event.target).attr("title", "查看所有简介");
    } else {
        $(event.target).addClass("isopen");
        $("#divRecommendRemark" + index).hide();
        $("#divRecommendRemarkFull" + index).show();
        $(event.target).text("收起");
        $(event.target).removeAttr("title");
    }
}
Community.Recommend.AddEvent = function AddEvent(event) {
    var community_id = event.data.CommunityID;
    var community_name = event.data.CommunityName;
    var creater_name = event.data.CreaterName;
    var create_time = event.data.CreateTime;
    $("#spnApplyCommunityName").text(community_name);
    $("#spnApplyCreaterName").text(creater_name + " (创建时间：" + create_time.format("yyyy-MM-dd") + ")");
    //打开管理圈窗口
    $("#sctApplyMemberCommunity").dialog({
        resizable: false,
        width: 600,
        modal: true,
        title: "申请加入圈子",
        appendTo: ".main-left",
        buttons: {
            "取　消": function () {
                Community.Recommend.Clear();
                $(this).dialog("close");
            },
            "申　请": function () {
                Community.Recommend.Apply(community_id, community_name);
                Community.Recommend.Clear();
                $(this).dialog("close");
            }
        }
    });
}
//申请加入行业圈子
Community.Recommend.Apply = function apply(community_id, community_name) {

    var application_info = {
        ID: $.NewGuid(),
        CommunityID: community_id,
        Remark: $("#txtApplyRemark").val() == "" ? objPub.SocialCode + "申请加入" + community_name + "行业圈子~" : $("#txtApplyRemark").val()
    };

    $.SimpleAjaxPost("service/CommunityService.asmx/MemberApplication", true,
        "{communityApplicationInfo:" + $.Serialize(application_info) + "}",
        function (json) {
            var result = json.d;
            if (result == true) {
                $.Alert({
                    content: "您的申请请求已发出,请耐心等待...",
                    width: "auto"
                });
            }
            else {
                console.log("发送申请请求失败！");
            }
        });
}
Community.Recommend.Clear = function clear() {
    $("#txtApplyRemark").val("");
    $("#spnApplyCreaterName,#spnApplyCommunityName").text("");
}

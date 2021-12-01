Community.Topic = function () { }
Community.Topic.registerClass("Community.Topic");
Community.Topic.PageSize = 10;
Community.Topic.CurrentIndex = 0;
Community.Topic.CanPageLoad = false;
Community.Topic.TotalCount = 0;
Community.Topic.Manager = "";
Community.Topic.GC = function () {
    Community.Topic.CurrentIndex = 0;
    Community.Topic.CanPageLoad = false;
    Community.Topic.TotalCount = 0;
    Community.Topic.OldDocumentHeight = 0;
    Community.Topic.Manager = "";
}
Community.Topic.Init = function init(community_id, community_manager) {
    Community.Topic.GC();
    Community.Topic.Manager = community_manager;
    var keyword = {
        Keyword: "",
        CommunityID: community_id
    };
    var page = { pageStart: 1, pageEnd: Community.Topic.PageSize * 1 };
    Community.Topic.Search(keyword, page);
}

Community.Topic.Search = function search(keyword, page) {
    Community.Topic.SearchBind(keyword, page,0);
    $.SimpleAjaxPost("service/CommunityService.asmx/GetTopicSearchCount", true,
        "{searchView:" + $.Serialize(keyword) + "}",
         function (json) {
             Community.Topic.TotalCount = json.d;
             if (Community.Topic.TotalCount > Community.Topic.PageSize) {
                 Community.Topic.CanPageLoad = true;
                 $(document).off("scroll").on("scroll", { Keyword: keyword }, Community.Topic.ScrollEvent);
             } 
         });
}
//滚轮事件
Community.Topic.ScrollEvent = function ScorllEvent(event) {
    var keyword = event.data.Keyword;
    if ($(document).scrollTop() >= $(document).height() - $(window).height()){
        if (Community.Topic.CanPageLoad == true) {
            Community.Topic.CurrentIndex = Community.Topic.CurrentIndex + 1;
            var page = {
                pageStart: Community.Topic.CurrentIndex * Community.Topic.PageSize + 1,
                pageEnd: (Community.Topic.CurrentIndex + 1) * Community.Topic.PageSize
            };
            Community.Topic.SearchBind(keyword, page, Community.Topic.CurrentIndex);
            if (page.pageEnd >= Community.Topic.TotalCount) {
                Community.Topic.CanPageLoad = false;
            }
            Community.Topic.OldDocumentHeight = $(document).height();
        }
        else { 
            if (parseInt(Community.Topic.TotalCount / Community.Topic.PageSize) >= objPub.MinTipPage) {
                $.Alert("这已经是最后一页了哦~");
                $(document).off("scroll");
                setTimeout(function () {
                    $(".dialog-normal").dialog('close');
                }, 2000);
                Community.Topic.OldDocumentHeight = 0;
            }
        }
    }
}
Community.Topic.SearchBind = function search_bind(keyword, page, current_index) {
    $.SimpleAjaxPost("service/CommunityService.asmx/TopicSearch", true,
      "{searchView:" + $.Serialize(keyword) + ",page:" + $.Serialize(page) + "}",
    function (json) {
        var result = $.Deserialize(json.d);
        var temp = "";
        if (result != null) {
            $.each(result, function (index, item) {
                var Index = parseInt(current_index * Community.Topic.PageSize) + index; 
                temp += "<div class='discuss-block'>";
                temp += "<span class='discuss-num'>" + item.MessageCount + "</span>";
                temp += "<a class='discuss-title' href='javascript:void(0);' id='aTopic" + Index + "'>" + item.Content + "</a>";
                //删除按钮
                $(document).off("click", "#aCommunityTopicDel" + Index);
                if (item.CreaterID == objPub.UserID || Community.Topic.Manager == objPub.UserID) {
                    temp += "<a href='javascript:void(0);' style='float:right;height:17px;line-height:17px;' id='aCommunityTopicDel" + Index + "'><span class='icon-optSet icon-img icon-opt-delete' title='删除'></span></a>";
                    $(document).on("click", "#aCommunityTopicDel" + Index, { TopicID: item.ID }, Community.Topic.TopicDeleteEvent);
                }
                $(document).off("click", "#aTopic" + Index);
                var topic_event_info={
                    ID: item.ID,
                    Title: item.Content,
                    CreaterID: item.CreaterID,
                    CreaterName: item.CreaterName,
                    CreateTime: item.CreateTime,
                    MessageCount: item.MessageCount
                };
                $(document).off("click", "#aTopic" + Index);
                $(document).on("click", "#aTopic" + Index, { Topic: topic_event_info }, Community.Topic.GetMessageListEvent);
                temp += "<div class='discuss-message circle-discuss'>";
                if (item.MessageInfoList != null) {
                    temp += "<ul>";
                    $.each(item.MessageInfoList, function (sub_index, sub_item) {
                        temp += "<li class='clear-fix'>";
                        $(document).off("click", "#divShowCommunityListFrom" + Index + "_" + sub_index);
                        $(document).off("click", "#aShowCommunityListFrom" + Index + "_" + sub_index);
                        $(document).off("click", "#aShowCommunityListTo" + Index + "_" + sub_index);
                        if (sub_item.FromCommenterID != window.objPub.UserID && sub_item.FromCommenterIsFriend == Enum.YesNo.No.toString()) {
                            temp += "<div class='circle-photo' id='divShowCommunityListFrom" + Index + "_" + sub_index + "' style='cursor:default;'><img src='" + sub_item.FromCommenterUrl + "' /></div>";
                        } else {
                            temp += "<div class='circle-photo' id='divShowCommunityListFrom" + Index + "_" + sub_index + "' style='cursor:pointer;'><img src='" + sub_item.FromCommenterUrl + "' /></div>";
                            $(document).on("click", "#divShowCommunityListFrom" + Index + "_" + sub_index, { UserID: sub_item.FromCommenterID }, Community.Topic.ShowDetailUserEvent);
                        }
                        temp += "<div class='circle-content'>";
                        temp += "<div class='friend-name'>";
                        if (sub_item.FromCommenterID != window.objPub.UserID && sub_item.FromCommenterIsFriend == Enum.YesNo.No.toString()) {
                            temp += "<a href='javascript:void(0);' id='aShowCommunityListFrom" + Index + "_" + sub_index + "' style='cursor:default;'>" + sub_item.FromCommenterName + "</a>";
                        } else {
                            temp += "<a href='javascript:void(0);' id='aShowCommunityListFrom" + Index + "_" + sub_index + "' style='cursor:pointer;'>" + sub_item.FromCommenterName + "</a>";
                            $(document).on("click", "#aShowCommunityListFrom" + Index + "_" + sub_index, { UserID: sub_item.FromCommenterID }, Community.Topic.ShowDetailUserEvent);
                        }
                        if (sub_item.ToCommenterID != null) {
                            temp += "<span>回复</span>";
                            if (sub_item.ToCommenterIsFriend == Enum.YesNo.No.toString()) {
                                temp += "<a href='javascript:void(0);' id='aShowCommunityListTo" + Index + "_" + sub_index + "' style='cursor:default;'>" + sub_item.ToCommenterName + "</a>";
                            } else {
                                temp += "<a href='javascript:void(0);' id='aShowCommunityListTo" + Index + "_" + sub_index + "' style='cursor:pointer;'>" + sub_item.ToCommenterName + "</a>";
                                $(document).on("click", "#aShowCommunityListTo" + Index + "_" + sub_index, { UserID: sub_item.ToCommenterID }, Community.Topic.ShowDetailUserEvent);
                            }
                        }
                        temp += "</div>";
                        temp += "<div class='circle-content-intro'>" + sub_item.Content + "</div>";
                        if (sub_item.CommentTime.format("yyyy-MM-dd") == new Date().format("yyyy-MM-dd")) {
                            temp += "<div class='circle-content-time'>今天　" + sub_item.CommentTime.format("HH:mm") + "</div>";
                        }
                        else {
                            temp += "<div class='circle-content-time'>" + sub_item.CommentTime.format("yyyy-MM-dd") + "</div>";
                        }
                        temp += "</div>";
                        temp += "</li>";
                    });
                    temp += "</ul>";
                }
                temp += "</div>";
                temp += "</div>"; 
            });
            $("#divEmptyCommunityTopicList").hide();
            $("#sctCommunityTopicList").append(temp);
        }
        else {
            $("#divEmptyCommunityTopicList").show();
        }
    });
}


Community.Topic.GetMessageListEvent = function GetMessageListEvent(event) {
    Community.Message.Init(event.data.Topic);
}


Community.Topic.ShowDetailUserEvent = function ShowDetailUserEvent(event) {
    var user_id = event.data.UserID;
    $(".main-left").load("../biz/left/moments.html", function (response, status) {
        if (status == "success") {
            objPub.IsMain = true;
            Moments.List.Person.Init(user_id);
        }
    });
}

Community.Topic.TopicDeleteEvent = function TopicDeleteEvent(event) {
    var topic_id = event.data.TopicID;
    $.Confirm("确认删除此讨论么？", function () {
        $.SimpleAjaxPost("service/CommunityService.asmx/DeleteTopic", true,
       "{topicID:'" + topic_id + "'}",
        function (json) {
            var result = json.d;
            if (result == true) {
                //更新讨论数目
                $("#bTopicCount").text(parseInt($("#bTopicCount").text()) - 1);
                    //刷新讨论页列表
                $("#ulSubType>li.circle-topic").trigger("click");
                AtInfo.GetAtValidationCount();

            } else {
                $.Alert("删除讨论失败");
            }
        });
    });
}
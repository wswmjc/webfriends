Community.Trends = function () {
//主页圈子业务逻辑
}
Community.Trends.registerClass("Community.Trends");
Community.Trends.PageSize = 10;
Community.Trends.TotalCount = 0;
Community.Trends.CurrentIndex = 0;
Community.Trends.CanPageLoad = false;
Community.Trends.OldDocumentHeight = 0;
//分页垃圾处理
Community.Trends.GC = function GC() {
    Community.Trends.TotalCount = 0;
    Community.Trends.CurrentIndex = 0;
    Community.Trends.CanPageLoad = false;
    Community.Trends.OldDocumentHeight = 0;
}
Community.Trends.Search = function search(keyword, page, init_topic_community_id) {
    Community.Trends.GC();
    $(document).off("scroll");
    $("html,body").animate({
        scrollTop: 0
    });
    Community.Trends.SearchBind(keyword, page,0, init_topic_community_id);
    $.SimpleAjaxPost("service/CommunityService.asmx/GetTrendsSearchCount", true,
        "{searchView:" + $.Serialize(keyword) + "}",
         function (json) { 
             var result = json.d;
             //console.log(result);
             Community.Trends.TotalCount = result;
             if (result > Community.Trends.PageSize) {
                 Community.Trends.CanPageLoad = true;
                 $(document).off("scroll").on("scroll", { Keyword: keyword, ID: init_topic_community_id }, Community.Trends.ScrollEvent);
             }
         });
}
//滚轮事件
Community.Trends.ScrollEvent = function ScorllEvent(event) {
    var keyword = event.data.Keyword;
    var init_topic_community_id = event.data.ID;
    //var totalheight = $(window).height() +$(window).scrollTop();     //浏览器的高度加上滚动条的高度   
    //if (($(document).height()*9/10 <= totalheight)) {
    //console.log("第一个:" + ($(document).scrollTop() >= $(document).height() - $(window).height()));
    if ($(document).scrollTop() >= $(document).height() - $(window).height()) {
        if (Community.Trends.CanPageLoad == true) {
                Community.Trends.CurrentIndex = Community.Trends.CurrentIndex + 1;
                var page = {
                    pageStart: Community.Trends.CurrentIndex * Community.Trends.PageSize + 1,
                    pageEnd: (Community.Trends.CurrentIndex + 1) * Community.Trends.PageSize
                };
                Community.Trends.SearchBind(keyword, page, Community.Trends.CurrentIndex, init_topic_community_id); 
                if (page.pageEnd >= Community.Trends.TotalCount) {
                    Community.Trends.CanPageLoad = false; 
                }
                Community.Trends.OldDocumentHeight = $(document).height();
            }
        }
    else {
        //console.log("TotalCount:" + Community.Trends.TotalCount);
        //console.log("PageSize:" + Community.Trends.PageSize);
        //console.log("MinTipPage:" + objPub.MinTipPage);
        //console.log("第二个:" + (parseInt(Community.Trends.TotalCount / Community.Trends.PageSize) > objPub.MinTipPage));
            if (Community.Trends.TotalCount == 0) {//当没有任何记录的时候，直接注销事件
                $(document).off("scroll");
            } else if (parseInt(Community.Trends.TotalCount / Community.Trends.PageSize) > objPub.MinTipPage) {
                //console.log("这就要弹框了吗？？" + (parseInt(Community.Trends.TotalCount / Community.Trends.PageSize) > objPub.MinTipPage));
                $.Alert("这已经是最后一页了哦~");
                $(document).off("scroll");
                setTimeout(function () {
                    $(".dialog-normal").dialog('close');
                }, 2000);
                Community.Trends.OldDocumentHeight = 0;

            }
    }

}
Community.Trends.SearchBind = function search_bind(keyword, page, current_index, init_topic_community_id) {
    $.SimpleAjaxPost("service/CommunityService.asmx/TrendsSearch", true,
        "{searchView:" + $.Serialize(keyword) + ",page:" + $.Serialize(page) + "}",
      function (json) {
          var result = $.Deserialize(json.d);
          var temp = "";
          if (result != null) {
              var init_index = 0;
              $.each(result, function (index, item) {
                  var Index = parseInt(current_index * Community.Trends.PageSize) + index;
                  var community_event_info = {
                      ID: item.ID,
                      Name: item.Name,
                      LabelCount: item.LabelCount,
                      TopicCount: item.TopicCount,
                      Manager: item.Manager
                  };
                  //alert(item.LabelCount);
                  //console.log(item.LabelInfoList);
                  if (init_topic_community_id && item.ID == init_topic_community_id) {
                      init_index = Index;
                  }
                  temp += "<div class='friend-circle'>";
                  temp += "<div class='figure'><span class='icon-optSet icon-img icon-triangle-green'></span></div>";
                  temp += "<div class='circle-headline'><div class='circle-user-avatar'><img src='" + item.LogoUrl + "'/></div>";
                  temp += "<span id='spnCommunityName" + Index + "' class='circle-user-name'>" + item.Name + "</span>";
                  $(document).off("click", "#spnCommunityName" + Index);
                  $(document).on("click", "#spnCommunityName" + Index, {Community: community_event_info,Type: Enum.CommunitySubType.Subject,Labels: item.LabelInfoList}, Community.Label.GoDetailEvent);
                  temp += "</div>";
                  if (item.LabelInfoList != null) {
                      temp += "<div class='circle-news-item hot-circle clear-fix'>";
                      temp += "<div class='item-title' id='divCommunitySubject" + Index + "'>话题</div>";
                      $(document).off("click", "#divCommunitySubject" + Index);
                      $(document).on("click", "#divCommunitySubject" + Index, {Community: community_event_info,Type: Enum.CommunitySubType.Subject,Labels: item.LabelInfoList}, Community.Label.GoDetailEvent);
                      temp += "<ul class='clear-fix'>";
                      $.each(item.LabelInfoList, function (sub_label_index, sub_label_item) {
                          temp += "<li id='liLabel" + Index + "-" + sub_label_index + "'>" + sub_label_item.LabelName + "</li>";
                          $(document).off("click", "#liLabel" + Index + "-" + sub_label_index);
                          $(document).on("click", "#liLabel" + Index + "-" + sub_label_index, {ID: sub_label_item.LabelID,Name: sub_label_item.LabelName,Community: community_event_info,Labels: item.LabelInfoList}, Community.Label.GoDetailEvent);
                      });
                      temp += " </ul>";
                      temp += "</div>";
                  }
                  if (item.TopicInfoList != null) {
                      temp += "<div class='circle-news-item clear-fix'>";
                      temp += "<div class='item-title' id='divCommunityTopic" + Index + "'>讨论</div>";
                      $(document).off("click", "#divCommunityTopic" + Index);
                      $(document).on("click", "#divCommunityTopic" + Index, { Community: community_event_info, Type: Enum.CommunitySubType.Topic, Labels: item.TopicInfoList }, Community.Label.GoDetailEvent);
                      temp += "<ul class='newest-discuss'>";
                      $.each(item.TopicInfoList, function (sub_topic_index, sub_topic_item) {
                          temp += "<li class='clear-fix'>";
                          $(document).off("click", "#divCommunityTreadListUser" + Index + "_" + sub_topic_index);
                          if (sub_topic_item.CreaterID != window.objPub.UserID && sub_topic_item.IsFriend == Enum.YesNo.No.toString()) {
                              temp += "<div class='circle-photo' id='divCommunityTreadListUser" + Index + "_" + sub_topic_index + "' style='cursor:default'><img src='" + sub_topic_item.CreaterUrl + "'/></div>";
                          } else {
                              temp += "<div class='circle-photo' id='divCommunityTreadListUser" + Index + "_" + sub_topic_index + "' style='cursor:pointer'><img src='" + sub_topic_item.CreaterUrl + "'/></div>";
                              $(document).on("click", "#divCommunityTreadListUser" + Index + "_" + sub_topic_index, { UserID: sub_topic_item.CreaterID }, Community.Trends.ShowDetailUserEvent);
                          }
                          temp += "<div class='circle-content'>";
                          temp += "<div class='circle-content-intro'>" + sub_topic_item.Content + "</div>";
                          if (sub_topic_item.CreateTime.format("yyyy-MM-dd") == new Date().format("yyyy-MM-dd")) {
                              temp += "<div class='circle-content-time'>今天　" + sub_topic_item.CreateTime.format("HH:mm") + "</div>";
                          }
                          else {
                              temp += "<div class='circle-content-time'>" + sub_topic_item.CreateTime.format("yyyy-MM-dd") + "</div>";
                          }

                          temp += "</div>";
                          temp += "</li>";
                      });
                      temp += "</ul>";
                      temp += "</div>";
                  }
                  temp += "</div>";
              });
              $("#divTabList").append(temp);
              if (init_topic_community_id && $("#divCommunityTopic" + init_index).length != 0) {
                  $("#divCommunityTopic" + init_index).trigger("click");
              }
          }
          else {
              temp += "<div class='friend-list-empty'><span>暂没有行业圈子最新动态哦~</span></div>";
              $("#divTabList").empty().append(temp);
          }
      });
}

Community.Trends.ShowDetailUserEvent = function ShowDetailUserEvent(event) {
    var user_id = event.data.UserID;
    $(".main-left").load("../biz/left/moments.html", function (response, status) {
        if (status == "success") {
            objPub.IsMain = true;
            Community.Trends.Person.Init(user_id);
        }
    });
}
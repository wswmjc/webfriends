Group.Trends = function () {
//主页讨论组业务逻辑
}
Group.Trends.registerClass("Group.Trends");
Group.Trends.PageSize = 10;
Group.Trends.TotalCount = 0;
Group.Trends.CurrentIndex = 0;
Group.Trends.CanPageLoad = false;
Group.Trends.OldDocumentHeight = 0;
//分页垃圾处理
Group.Trends.GC = function GC() {
    Group.Trends.TotalCount = 0;
    Group.Trends.CurrentIndex = 0;
    Group.Trends.CanPageLoad = false;
    Group.Trends.OldDocumentHeight = 0;
}
Group.Trends.Search = function search(keyword, page) {
    Group.Trends.GC();
    $(document).off("scroll");
    $("html,body").animate({
        scrollTop: 0
    });
    Group.Trends.SearchBind(keyword, page,0);
    $.SimpleAjaxPost("service/GroupService.asmx/GetTrendsSearchCount", true,
        "{searchView:" + $.Serialize(keyword) + "}",
         function (json) {  
             var result = json.d;
             //console.log(result);
             Group.Trends.TotalCount = result;
             if (Group.Trends.TotalCount > Group.Trends.PageSize) {
                 Group.Trends.CanPageLoad = true;
                 $(document).off("scroll").on("scroll", { Keyword: keyword }, Group.Trends.ScrollEvent);
             }
         });
}
//滑轮事件
Group.Trends.ScrollEvent = function ScrollEvent(event) {
    var keyword = event.data.Keyword;
    //var totalheight = $(window).height() +$(window).scrollTop();     //浏览器的高度加上滚动条的高度   
    //if (($(document).height()*9/10 <= totalheight)) {
    if ($(document).scrollTop() >= $(document).height() - $(window).height()) {
        if (Group.Trends.CanPageLoad == true) {
            Group.Trends.CurrentIndex = Group.Trends.CurrentIndex + 1;
            var page = {
                pageStart: Group.Trends.CurrentIndex * Group.Trends.PageSize + 1,
                pageEnd: (Group.Trends.CurrentIndex + 1) * Group.Trends.PageSize
            };
            Group.Trends.SearchBind(keyword, page, Group.Trends.CurrentIndex);
            if (page.pageEnd >= Group.Trends.TotalCount) {
                Group.Trends.CanPageLoad = false;
            }
            Group.Trends.OldDocumentHeight = $(document).height();
        } else {
            if (Group.Trends.TotalCount == 0) {//当没有任何记录的时候，直接注销事件
                $(document).off("scroll");
            } else if (parseInt(Group.Trends.TotalCount / Group.Trends.PageSize) > objPub.MinTipPage) {
                $.Alert("这已经是最后一页了哦~");
                $(document).off("scroll");
                setTimeout(function () {
                    $(".dialog-normal").dialog('close');
                }, 2000);
            }
            Group.Trends.OldDocumentHeight = 0;
        }
    }
}
       
Group.Trends.SearchBind = function search_bind(keyword, page,current_index) {
    $.SimpleAjaxPost("service/GroupService.asmx/TrendsSearch", true,
        "{searchView:" + $.Serialize(keyword) + ",page:" + $.Serialize(page) + "}",
      function (json) {
          var result = $.Deserialize(json.d);
          //console.log(result);
          //console.log(result.length);
          var temp = "";
          if (result != null) {
              var a=0, b=0;
              $.each(result, function (index, item) {
                  //console.log("a:"+ (a++));
                  var Index = parseInt(current_index * Group.Trends.PageSize) + index;
                  temp += "<div class='friend-group'>";
                  temp += "<div class='figure'><span class='icon-optSet icon-img icon-triangle-yellow'></span></div>";
                  temp += "<div class='circle-headline'>";
                  temp += "<div id='divGroup" + Index + "' class='circle-user-name'>";
                  temp += "<span>" + item.Name + "</span>";
                  temp += "<span class='group-num'>" + item.MemberCount + "人</span>";
                  temp += "</div>";
                  temp += "</div>";
                  temp += "<ul>";
                  $.each(item.TopicInfo, function (sub_index, sub_item) {
                      //console.log("b:" + (b++));
                      temp += "<li class='clear-fix'>";
                      $(document).off("click", "#divShowTreadListUser" + Index + "_" + sub_index);
                      if (sub_item.CreaterID != window.objPub.UserID && sub_item.IsFriend == Enum.YesNo.No.toString()) {
                          temp += "<div class='circle-photo' id='divShowTreadListUser" + Index + "_" + sub_index + "' style='cursor:default'> <img src='" + sub_item.CreaterUrl + "'/></div>";
                      } else {
                          temp += "<div class='circle-photo' id='divShowTreadListUser" + Index + "_" + sub_index + "' style='cursor:pointer'> <img src='" + sub_item.CreaterUrl + "'/></div>";
                          $(document).on("click", "#divShowTreadListUser" + Index + "_" + sub_index, { UserID: sub_item.CreaterID }, Group.Trends.ShowDetailUserEvent);
                      }
                      temp += "<div class='circle-content'>";
                      temp += "<h4>" + sub_item.Content + "</h4>";
                      if (sub_item.CreateTime.format("yyyy-MM-dd") == new Date().format("yyyy-MM-dd")) {
                          temp += "<div class='circle-content-time'>今天　" + sub_item.CreateTime.format("HH:mm") + "</div>";
                      }
                      else {
                          temp += "<div class='circle-content-time'>" + sub_item.CreateTime.format("yyyy-MM-dd") + "</div>";
                      }
                      temp += "<div class='circle-content-info'>";
                      temp += "<span class='info-number'>" + sub_item.MessageCount + "</span>";
                      temp += "<span>条讨论</span>";
                      temp += "</div>";
                      temp += "</div>";
                      temp += "</li>";
                  });
                  temp += "</ul>";
                  temp += "</div>";
                  $(document).off("click", "#divGroup" + Index);
                  $(document).on("click", "#divGroup" + Index, { ID: item.ID, Name: item.Name,Manager:item.Manager }, Group.ShowGroupEvent);
              });
              //console.log(temp);
              $("#divTabList").append(temp);
          }
          else {
              temp += "<div class='friend-list-empty'><span>暂没有讨论组最新动态哦~</span></div>";
              $("#divTabList").empty().append(temp);
          }
      });
}

Group.Trends.ShowDetailUserEvent = function ShowDetailUserEvent(event) {
    var user_id = event.data.UserID;
    $(".main-left").load("../biz/left/moments.html", function (response, status) {
        if (status == "success") {
            objPub.IsMain = true;
            Moments.List.Person.Init(user_id);
        }
    });
}
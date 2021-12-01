AtInfo.Moments = function () {}
AtInfo.Moments.registerClass("AtInfo.Moments");
//待处理数
AtInfo.Moments.Count = 0;
AtInfo.Moments.BusinessType = Enum.BusinessType.Moments;
//@标签切换点击事件
AtInfo.Moments.TagEvent = function TagEvent(event) {
    AtInfo.BusinessType = Enum.BusinessType.Moments;
    $(event.target).addClass("selected").siblings().removeClass("selected");
    AtInfo.Moments.GetAtValidationList();
    if (AtInfo.Moments.Count != 0) {
        $("#spnReadAll").show();
    } else {
        $("#spnReadAll").hide();
    }
}
AtInfo.Moments.GetAtValidationList = function get_at_validation_list() {
    var my_notice_view = {
        BusinessType:AtInfo.Moments.BusinessType
    };
    $.SimpleAjaxPost("service/NoticeService.asmx/Search", true,
        "{myNoticeView:"+$.Serialize(my_notice_view)+",page:null}",
      function (json) {
          var result = $.Deserialize(json.d);
          var temp = "";
          if (result != null) {
              $.each(result, function (index, item) {
                  if (item.Content != null) {
                      temp += "<li class='clear-fix'>";
                      temp += "<div class='contacts-info'>";
                      temp += "<div class='contacts-photo'><img src='" + item.UserUrl + "'></div>";
                      temp += "<div class='notice-info'>";
                      temp += "<p class='notice-name'><a id='aRead" + index + "' title='设置已读，请点击~'>" + item.UserName + "@你关注</a></p>";
                      if (item.PublishType == Enum.PublishInfoType.Long.toString()) {
                          temp += "<p class='notice-text' id='pShowDetail" + index + "'>【文章】" + item.Content + "</p>";
                      }
                      else {
                          temp += "<p class='notice-text' id='pShowDetail" + index + "'>" + item.Content + "</p>";
                      }
                      $(document).off("click", "#pShowDetail" + index);
                      $(document).on("click", "#pShowDetail" + index, { ID: item.PublishID, Source: Enum.BusinessType.Moments }, AtInfo.ShowDetailMessageEvent);
                      temp += "</div>";

                      temp += "</div>";
                      temp += "<div class='contacts-option'>" + item.PublishTime.format("yyyy-MM-dd") + "</div>";
                      temp += "</li>";
                      $(document).off("click", "#aRead" + index);
                      $(document).on("click", "#aRead" + index, { ID: item.ID, Type: Enum.BusinessType.Moments }, AtInfo.ReadEvent);
                  } 
              });
              $("#divEmptyAt").hide();
          }
          else {
              $("#divEmptyAt").show();
              $("#spnEmptyAt").text("暂没有@您的朋友圈信息哦~");
          }
          $("#ulMyAtList").empty().append(temp);
      });
}
//获取通讯录待校验信息数
AtInfo.Moments.GetAtValidationCount = function get_at_validation_count() {
    var my_notice_view = {
        BusinessType: AtInfo.Moments.BusinessType
    };
    return $.SimpleAjaxPost("service/NoticeService.asmx/GetSearchCount", true,"{myNoticeView:"+$.Serialize(my_notice_view)+"}");
}
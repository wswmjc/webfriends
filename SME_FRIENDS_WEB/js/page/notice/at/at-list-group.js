AtInfo.Group = function () {

}
AtInfo.Group.registerClass("AtInfo.Group");
//待处理数
AtInfo.Group.Count = 0;
AtInfo.Group.BusinessType = Enum.BusinessType.Group;
//@标签切换点击事件
AtInfo.Group.TagEvent = function TagEvent(event) {
    AtInfo.BusinessType = Enum.BusinessType.Group;
    $(event.target).addClass("selected").siblings().removeClass("selected");
    AtInfo.Group.GetAtValidationList();
    if (AtInfo.Group.Count != 0) {
        $("#spnReadAll").show();
    } else {
        $("#spnReadAll").hide();
    }
}
AtInfo.Group.GetAtValidationList = function get_at_validation_list() {
    var my_notice_view = {
        BusinessType: AtInfo.Group.BusinessType
    };
    $.SimpleAjaxPost("service/NoticeService.asmx/Search", true,
        "{myNoticeView:" + $.Serialize(my_notice_view) + ",page:null}",
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
                      temp += "<p class='notice-text' id='pShowDetail" + index + "'>" + item.Content + "</p>";
                      $(document).off("click", "#pShowDetail" + index);
                      $(document).on("click", "#pShowDetail" + index, { ID: item.PublishID, Source: Enum.BusinessType.Group, NoticeID: item.ID }, AtInfo.ShowDetailMessageEvent);
                      temp += "</div>";

                      temp += "</div>";
                      temp += "<div class='contacts-option'>" + item.PublishTime.format("yyyy-MM-dd") + "</div>";
                      temp += "</li>";
                      $(document).off("click", "#aRead" + index);
                      $(document).on("click", "#aRead" + index, { ID: item.ID, Type: Enum.BusinessType.Group }, AtInfo.ReadEvent);
                  }
              });
              $("#divEmptyAt").hide();
          }
          else {
              $("#divEmptyAt").show();
              $("#spnEmptyAt").text("暂没有@您的讨论组信息哦~");
          }
          $("#ulMyAtList").empty().append(temp); });
}
AtInfo.Group.GetAtValidationCount = function get_at_validation_count() {
    var my_notice_view = {
        BusinessType: AtInfo.Group.BusinessType
    };
    return $.SimpleAjaxPost("service/NoticeService.asmx/GetSearchCount", true, "{myNoticeView:" + $.Serialize(my_notice_view) + "}");
}
AddressBook.AddBlackList = function () { }
AddressBook.AddBlackList.registerClass("AddressBook.AddBlackList");
AddressBook.AddBlackList.Search = function search(keyword, page) {
    AddressBook.AddBlackList.SearchBind(keyword, page);
    $.SimpleAjaxPost("service/AddressBookService.asmx/GetBlackListSearchCount", true,
      "{searchView:" + $.Serialize(keyword) + "}",
       function (json) {
           var result = json.d;
           if (result <= AddressBook.PageSize) {
               $("#divAddressBookPage").wPaginate("destroy");
           }
           else {
               $("#divAddressBookPage").wPaginate("destroy").wPaginate({
                   theme: "grey",
                   first: "首页",
                   last: "尾页",
                   total: result,
                   index: 0,
                   limit: AddressBook.PageSize,
                   ajax: true,
                   url: function (i) {
                       var page = {
                           pageStart: i * this.settings.limit + 1,
                           pageEnd: (i + 1) * this.settings.limit
                       };
                       AddressBook.AddBlackList.SearchBind(keyword, page);
                   }
               });
           }
       });
}
//搜索绑定
AddressBook.AddBlackList.SearchBind = function search_bind(keyword, page) {
    $("#divEmptyBlacklist").find("span").text("暂没有匹配的黑名单列表哦~");
    $.SimpleAjaxPost("service/AddressBookService.asmx/BlackListSearch", true,
     "{searchView:" + $.Serialize(keyword) + ",page:" + $.Serialize(page) + "}",
      function (json) {
          var result = $.Deserialize(json.d);
          var temp = "";
          if (result != null) {
              $.each(result, function (index, item) {
                  temp += "<li class='clear-fix'>";
                  temp += "<div class='contacts-info'><div class='contacts-photo'><img src='" + item.AddresserUrl + "'></div>";
                  temp += "<span class='contacts-name'>" + item.AddresserName + "</span>";
                  if (item.Remark != null) {
                      temp += "<span id='spnAddresserRemark" + index + "' class='contacts-memo'>" + item.Remark + "</span>";
                  }
                  else {
                      temp += "<span id='spnAddresserRemark" + index + "' ></span>";
                  }
                
                  temp += "</div>";
                  temp += "<div class='contacts-option'>";
                  temp += "<span id='spnAddRemark" + index + "'>备注</span>";
                  temp += "<span id='spnRemoveBlackList" + index + "'>还原</span>";
                  temp += "<span id='spnDeleteAddresser" + index + "'>删除</span>";
                  temp += "</div>";
                  temp += "</li>";
                  $(document).off("click", "#spnAddRemark" + index);
                  $(document).on("click", "#spnAddRemark" + index, { Index: index, ID: item.ID, AddresserName: item.AddresserName }, AddressBook.SetRemarkEvent);
                  $(document).off("click", "#spnRemoveBlackList" + index);
                  $(document).on("click", "#spnRemoveBlackList" + index, { ID: item.ID,Name:item.AddresserName, Page: page }, AddressBook.AddBlackList.RemoveEvent);
                  $(document).off("click", "#spnDeleteAddresser" + index);
                  $(document).on("click", "#spnDeleteAddresser" + index, { ID: item.ID, Name: item.AddresserName, Page: page, Flag: AddressBook.Flag.BlackList }, AddressBook.DeleteEvent);
              });
              $("#divEmptyBlacklist").hide();
          }
          else {
              $("#divEmptyBlacklist").show();
          }
          $("#ulAddressBookList").empty().append(temp);
      });


}
//添加黑名单事件
AddressBook.AddBlackList.AddEvent = function AddEvent(event) {
    var user_name = event.data.Name;
    var id = event.data.ID;
    var page = event.data.Page;
    $.Confirm({ content: "您确定将好友" + user_name + "加入黑名单么？", width: "auto"}, function () {
        $.SimpleAjaxPost("service/AddressBookService.asmx/AddBlackList", true,
         "{ID:'" + id + "'}",
          function (json) {
              var result = json.d;
              if (result == true) {
                  $.Alert("添加成功!", function () {
                      var keyword = {
                          Keyword: $("#txtKeyword").val()
                      };
                      AddressBook.Search(keyword, page);
                      //主页右侧刷新
                      PublishRight.OftenUsedAddressBookBind();
                  });
              }
              else {
                  console.log("添加失败！");
              }
          });
    });
   

}
//还原黑名单
AddressBook.AddBlackList.RemoveEvent = function RemoveEvent(event) {
    var name = event.data.Name;
    var id = event.data.ID;
    var page = event.data.Page;
    $.Confirm({ content: "您确定将" + name + "移除黑名单么？", width: "auto" }, function () {
        $.SimpleAjaxPost("service/AddressBookService.asmx/RemoveBlackList", true,
         "{ID:'" + id + "'}",
          function (json) {
              var result = json.d;
              if (result == true) {
                  $.Alert("还原成功!", function () {
                      var keyword = {
                          Keyword: $("#txtKeyword").val()
                      };
                      AddressBook.AddBlackList.Search(keyword, page);
                      //主页右侧刷新
                      PublishRight.OftenUsedAddressBookBind();
                  });
              }
              else {
                  console.log("还原失败！");
              }
          });
    });
   
}
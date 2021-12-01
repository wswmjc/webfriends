AddressBook.AddFriends = function () { }
AddressBook.AddFriends.registerClass("AddressBook.AddFriends");
AddressBook.AddFriends.PageSize = 15;
AddressBook.AddFriends.Search = function search(keyword, page) {
    AddressBook.AddFriends.SearchBind(keyword, page);
    $.SimpleAjaxPost("service/UserService.asmx/GetSearchFriendsCount", true,
      "{keywordView:" + $.Serialize(keyword) + "}",
       function (json) {
           var result = json.d;
           if (result <= AddressBook.AddFriends.PageSize) {
               $("#divFindFriendsPage").wPaginate("destroy");
           }
           else {
               $("#divFindFriendsPage").wPaginate("destroy").wPaginate({
                   theme: "grey",
                   first: "首页",
                   last: "尾页",
                   total: result,
                   index: 0,
                   limit: AddressBook.AddFriends.PageSize,
                   ajax: true,
                   url: function (i) {
                       var page = {
                           pageStart: i * this.settings.limit + 1,
                           pageEnd: (i + 1) * this.settings.limit
                       };
                       AddressBook.AddFriends.SearchBind(keyword, page);
                   }
               });
           }
       });
}
//搜索事件
AddressBook.AddFriends.SearchEvent = function SearchEvent(event) {
    var page = event.data.Page;
    var keyword = {
        Keyword: $("#txtSearchUserKeyword").val()
    };
    AddressBook.AddFriends.Search(keyword, page);
}
//搜索绑定
AddressBook.AddFriends.SearchBind = function search_bind(keyword, page) {
    $.SimpleAjaxPost("service/UserService.asmx/SearchFriends", true,
     "{keywordView:" + $.Serialize(keyword) + ",page:" + $.Serialize(page) + "}",
      function (json) {
          var result = $.Deserialize(json.d);
          var temp = "";
          if (result != null) {
              $.each(result, function (index, item) {
                  temp += "<li class='clear-fix'>";
                  temp += "<div class='contacts-info'>";
                  temp += "<div class='contacts-photo'><img src='" + item.UserUrl + "'></div>";
                  temp += "<div class='contrcts-text'>";
                  temp += "<div>";
                  if (item.Sex == Enum.SexType.Male.toString()) {
                      temp += "<span class='icon-optSet icon-img icon-male'></span>";
                  }
                  else {
                      temp += "<span class='icon-optSet icon-img icon-female'></span>";
                  }
                  temp += "<span class='contacts-name'>" + item.UserName + "</span>";
                  temp += "</div>";
                  
                  temp += "<div>";
                  temp += "<span class='contacts-memo'>" + Enum.UserType.GetDescription(parseInt(item.UserType)) + "</span>";
                  temp += "</div>";

                  if (item.IsMyFriend == false) {
                      temp += "<div class='contacts-option'><input id='btnApplication" + index + "' type='button' class='btn-submit' value='加入好友'></div>";
                      $(document).off("click", "#btnApplication" + index);
                      $(document).on("click", "#btnApplication" + index, { AddresserID: item.ID, AddresserName: item.UserName }, AddressBook.AddFriends.ApplicationEvent);
                  }else {
                      temp += "<div class='friend-notice'>已互为好友</div>";
                  }
                  temp += "</div>";
                  temp += "</div>";
                  temp += "</li>";
                  
              });
              $("#divEmptyFriendsFind").hide();
          }
          else {
              $("#divEmptyFriendsFind").show();
          }
          $("#ulFriendsFindList").empty().append(temp);
      });
}
//加好友申请
AddressBook.AddFriends.ApplicationEvent = function ApplicationEvent(event) {
    var addresser_id = event.data.AddresserID;
    var addresser_name = event.data.AddresserName;
    $("#spnUserName").text(addresser_name);
    $("#sctAddFriend").dialog({
        resizable: false,
        width: 500,
        modal: true,
        title: "附加消息",
        buttons: {
            "取　消": function () {
                $("#txtRemark").val("");
                $("#spnUserName").text("");
                $(this).dialog("close");
            },
            "提　交": function () {
                  var application = {
                      ID: $.NewGuid(),
                      AddresserID: addresser_id,
                      AddresserName: addresser_name,
                      Remark: $("#txtRemark").val()
                  };
                  $.SimpleAjaxPost("service/AddressBookService.asmx/Application", true,
                 "{applicationInfo:" + $.Serialize(application) + "}",
                  function (json) {
                      var result = json.d;
                      $("#sctAddFriend").dialog("close");
                      if (result == true) {
                          $.Alert({
                              content: "添加" + addresser_name + "好友请求发送成功!",
                              width: "auto"
                          });
                      }
                      else {
                          console.log("添加失败！");

                      }
                      $("#txtRemark").val("");
                      $("#spnUserName").text("");
                  });
              
            }
        }
    });
}
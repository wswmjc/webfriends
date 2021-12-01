AddressBook = function () { }
AddressBook.registerClass("AddressBook");
AddressBook.PageSize = 10;
AddressBook.Flag = function () {
    throw Error.notImplemented();
}
AddressBook.Flag.prototype = {
    // 通讯录
    AddressBookList: 0,
    // 黑名单
    BlackList: 1,
};
AddressBook.Flag.registerEnum("AddressBook.Flag");

AddressBook.Init = function init() {
    objPub.IsMain = false;
    $(document).off("scroll");
    //返回事件
    $("#aGoBack").off("click").on("click", AddressBook.BackEvent);
    //添加通讯录
    $("#spnAddFriends").on("click", AddressBook.AddEvent);
    var keyword = {
        Keyword: $("#txtKeyword").val()
    };
    var page = { pageStart: 1, pageEnd: AddressBook.PageSize * 1 };
    AddressBook.Search(keyword, page);
    $("#spnSearch").off("click").on("click", { Page: page }, AddressBook.SearchEvent);
    $("#txtKeyword").off("keypress").on("keypress", function (event) {
        if (event.which == 13) {
            AddressBook.Search({
                Keyword: $("#txtKeyword").val()
            }, { pageStart: 1, pageEnd: AddressBook.PageSize * 1 });
            return false;
        }
    });
    //搜索通讯录事件
    $("#spnShowBlacklistToMyFriendslist").on("click", { Page: page }, AddressBook.SearchEvent);
    //搜索黑名单事件
    $("#spnMyFriendslistToBlacklist").on("click", { Page: page }, AddressBook.SearchBlackListEvent);
    //初始化查找用户对话框
    $("#sctSearchUser").dialog({
        appendTo: ".main-left",
        autoOpen:false,
        resizable: false,
        width: 720,
        modal: true,
        title: "添加朋友"
    });
}
//搜索
AddressBook.Search = function search(keyword, page) {
    AddressBook.SearchBind(keyword, page);
    $.SimpleAjaxPost("service/AddressBookService.asmx/GetSearchCount", true,
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
                       AddressBook.SearchBind(keyword, page);
                   }
               });
           }
       });
}
//搜索事件
AddressBook.SearchEvent = function SearchEvent(event) {
    $("#divAddressBook").text("通讯录");
    $("#spnShowBlacklistToMyFriendslist").hide();
    $(".icon-user-list").css("display", "inline-block");
    var page = event.data.Page;
    var keyword = {
        Keyword: $("#txtKeyword").val()
    };
    AddressBook.Search(keyword, page);
    $("#spnSearch").off("click").on("click", { Page: page }, AddressBook.SearchEvent);
    $("#txtKeyword").off("keypress").on("keypress", function (event) {
        if (event.which == 13) {
            AddressBook.Search({
                Keyword: $("#txtKeyword").val()
            }, { pageStart: 1, pageEnd: AddressBook.PageSize * 1 });
            return false;
        }
    });
}
//搜索绑定
AddressBook.SearchBind = function search_bind(keyword, page) {
    $("#divEmptyBlacklist").find("span").text("暂没有匹配的联系人列表哦~");
    $("#divEmptyBlacklist").hide();
    $.SimpleAjaxPost("service/AddressBookService.asmx/Search", true, "{searchView:" + $.Serialize(keyword) + ",page:" + $.Serialize(page) + "}",
         function (json) {
             var result = $.Deserialize(json.d);
             var temp = "<li class='clear-fix'>";
             temp+="<div class='contacts-info'><div class='contacts-photo'><img src='../images/user/0.png'></div>";
             temp+="<span class='contacts-name'>朋友圈团队</span><span class='contacts-memo'>官方账号</span>"	
             temp+="</div>";
             temp+="</li>";
             if (result != null) {
                 $.each(result, function (index, item) {
                     temp += "<li class='clear-fix'>";
                     temp += "<div class='contacts-info'><div class='contacts-photo' id='divAddresserImg" + index + "'><img src='" + item.AddresserUrl + "'></div>";
                     $(document).off("click", "#divAddresserImg" + index);
                     $(document).on("click", "#divAddresserImg" + index, { UserID: item.AddresserID }, function (event) {
                         var user_id = event.data.UserID;
                         $(".main-left").load("../biz/left/moments.html", function (response, status) {
                             if (status == "success") {
                                 objPub.IsMain = true;
                                 Moments.List.Person.Init(user_id);
                             }
                         });
                     });
                     temp += "<span class='contacts-name'>" + item.AddresserNickName + "</span>";
                     if (item.Remark != null) {
                         temp += "<span id='spnAddresserRemark" + index + "' class='contacts-memo'>" + item.Remark + "</span>";
                     }
                     else {
                         temp += "<span id='spnAddresserRemark" + index + "' ></span>";
                     }
                     temp += "</div>";
                     temp += "<div class='contacts-option'>";
                     if (item.CanSeeMe == Enum.YesNo.Yes.toString()) {
                         temp += "<span id='spnCanSeeMe" + index + "' class='icon-optSet icon-img icon-eye' title='查看我的消息'></span>";
                     }
                     else {
                         temp += "<span id='spnCanSeeMe" + index + "' class='icon-optSet icon-img icon-eye-no' title='禁止查看我的消息'></span>";
                     }

                     if (item.CanSeeAddresser == Enum.YesNo.Yes.toString()) {
                         temp += "<span id='spnCanSeeAddresser" + index + "' class='icon-optSet icon-img icon-comment' title='接收他的消息'></span>";
                     }
                     else {
                         temp += "<span id='spnCanSeeAddresser" + index + "' class='icon-optSet icon-img icon-comment-no' title='禁止接收他的消息'></span>";

                     }
                     if (item.OftenUsed == Enum.YesNo.No.toString()) {
                         temp += "<span id='spnSetOftenUsed" + index + "' class='icon-optSet icon-img icon-user-nofav' title='设置经常联系人?'></span>";
                     }
                     else {
                         temp += "<span id='spnSetOftenUsed" + index + "' class='icon-optSet icon-img icon-user-fav' title='取消经常联系人?'></span>";
                     }

                     temp += "<span id='spnAddRemark" + index + "' calss='contacts-memo'>备注</span>";
                     temp += "<span id='spnAddBlackList" + index + "'>加入黑名单</span>";
                     temp += "<span id='spnDeleteAddresser" + index + "'>删除</span>";
                     temp += "</div>";
                     temp += "</li>";
                     $(document).off("click", "#spnAddBlackList" + index);
                     $(document).on("click", "#spnAddBlackList" + index, { ID: item.ID,Name:item.AddresserName ,Page: page }, AddressBook.AddBlackList.AddEvent);
                     $(document).off("click", "#spnDeleteAddresser" + index);
                     $(document).on("click", "#spnDeleteAddresser" + index, {UserID: item.AddresserID,Name:item.AddresserName, Page: page, Flag: AddressBook.Flag.AddressBookList }, AddressBook.DeleteEvent);
                 });
                
                 $("#ulAddressBookList").empty().append(temp);
                 $.each(result, function (index, item) {
                     $("#spnAddRemark" + index).on("click", { Index: index, ID: item.ID, AddresserName: item.AddresserName }, AddressBook.SetRemarkEvent);
                     $("#spnSetOftenUsed" + index).data("OftenUsed", item.OftenUsed).on("click", { ID: item.ID }, AddressBook.SetOftenUsedEvent);
                     $("#spnCanSeeMe" + index).data("CanSeeMe", item.CanSeeMe).on("click", { ID: item.ID }, AddressBook.SeeMeEvent);
                     $("#spnCanSeeAddresser" + index).data("CanSeeAddresser", item.CanSeeAddresser).on("click", { ID: item.ID }, AddressBook.SeeAddresserEvent);
                 });
             } else {
                 $("#ulAddressBookList").empty();
                 $("#divEmptyBlacklist").show();
             }
         });
}
AddressBook.SetOftenUsedEvent = function SetOftenUsedEvent(event){
    var often_used_view = 
    {
        ID:event.data.ID,
        OftenUsed: $(event.target).data("OftenUsed") == Enum.YesNo.Yes.toString() ? false : true
    }
    $.SimpleAjaxPost("service/AddressBookService.asmx/SetOftenUsed", true,
      "{oftenUsedView:" + $.Serialize(often_used_view) + "}",
     function (json) {
         var result = json.d;
         if (result == true) {
             $.Alert("设置成功！", function () {
                 if ($(event.target).hasClass("icon-user-fav") == true) {
                     $(event.target).addClass("icon-user-nofav").removeClass("icon-user-fav").attr("title", "取消经常联系人?");
                    
                 }
                 else {
                     $(event.target).addClass("icon-user-fav").removeClass("icon-user-nofav").attr("title", "设置经常联系人?");
                 }
                 $(event.target).data("OftenUsed", often_used_view.OftenUsed == true ? Enum.YesNo.Yes.toString() :  Enum.YesNo.No.toString());
                 PublishRight.OftenUsedAddressBookBind();
             });
         }
         else {
             console.log("设置失败！");
         }
     });
}
//添加备注事件
AddressBook.SetRemarkEvent = function SetRemarkEvent(event) {
    var id = event.data.ID;
    var addresser_name = event.data.AddresserName;
    var index = event.data.Index;
    $("#spnAddresser").text(addresser_name);
    $("#txtAddresserRemark").val($("#spnAddresserRemark" + index).text());
    $("#dialogAddresserRemark").dialog({
        appendTo:".main-left",
        resizable: false,
        width: 500,
        modal: true,
        title: "添加备注",
        buttons: {
            "取　消": function () {
                $(this).dialog("close");
            },
            "提　交": function () {
                AddressBook.SetRemark(id,index);
                $(this).dialog("close");
            }
        }
    });
}
AddressBook.SetRemark = function SetRemark(id,index) {
    var remark_view = {
        ID: id,
        Remark: $("#txtAddresserRemark").val()
    };
    $.SimpleAjaxPost("service/AddressBookService.asmx/SetRemark", true,
    "{remarkView:" + $.Serialize(remark_view) + "}",
    function (json) {
        var result = json.d;
        if (result == true) {
            $.Alert("添加通讯录备注成功！", function () {
                $("#spnAddresserRemark" + index).text(remark_view.Remark);
                if (remark_view.Remark != "") {
                    if ($("#spnAddresserRemark" + index).hasClass("contacts-memo") == false) {
                        $("#spnAddresserRemark" + index).addClass("contacts-memo");
                    }
                }
                else {
                    if ($("#spnAddresserRemark" + index).hasClass("contacts-memo") == true) {
                        $("#spnAddresserRemark" + index).removeClass("contacts-memo");
                    }
                }
            });
        }
        else {
            console.log("添加通讯录备注失败！");
        }
    });
}
//删除通讯录
AddressBook.DeleteEvent = function DeleteEvent(event) {
    var user_name = event.data.Name;
    var user_id = event.data.UserID;
    var page = event.data.Page;
    var flag = event.data.Flag;
    $.Confirm({content:"您确定移除好友" + user_name + "么？",width:"auto"}, function () {
        $.SimpleAjaxPost("service/AddressBookService.asmx/DuplexRemove", true,
        "{userID:'" + user_id + "'}",
       function (json) {
           var result = json.d;
           if (result == true) {
               $.Alert("删除通讯录成功!", function () {
                   var keyword = {
                       Keyword: $("#txtKeyword").val()
                   };
                   if (flag === AddressBook.Flag.AddressBookList) {
                       AddressBook.Search(keyword, page);
                   }
                   else if (flag === AddressBook.Flag.BlackList) {
                       AddressBook.AddBlackList.Search(keyword, page);
                   }
                   PublishRight.OftenUsedAddressBookBind();
               });
           }
           else {
               console.log("删除通讯录失败！");
           }
       });
    });
   
}
//接收他人信息事件
AddressBook.SeeAddresserEvent = function SetSeeAddresserEvent(event) {
    var can_see_addresser_view = {
        ID:event.data.ID,
        CanSeeAddresser: ($(event.target).data("CanSeeAddresser") == Enum.YesNo.Yes.toString()?false:true)
    }; 
    $.SimpleAjaxPost("service/AddressBookService.asmx/SetCanSeeAddresser", true,
  "{canSeeAddresserView:" + $.Serialize(can_see_addresser_view) + "}",
     function (json) {
         var result = json.d;
         if (result == true) {
             if ($(event.target).hasClass("icon-comment") == true) {
                 $(event.target).addClass("icon-comment-no").removeClass("icon-comment").attr("title", "禁止接收他的消息");

             }
             else {
                 $(event.target).addClass("icon-comment").removeClass("icon-comment-no").attr("title", "接收他的消息");
             }
             $(event.target).data("CanSeeAddresser", can_see_addresser_view == true ? Enum.YesNo.Yes.toString() : Enum.YesNo.No.toString());
         }
         else {
             console.log("设置接收他人信息失效");
         }
     });
    
  
}
//查看我的信息事件
AddressBook.SeeMeEvent = function SetSeeMeEvent(event) {
    var can_see_me_view = {
        ID: event.data.ID,
        CanSeeMe: ($(event.target).data("CanSeeMe") == Enum.YesNo.Yes.toString()?false:true)
    };
    $.SimpleAjaxPost("service/AddressBookService.asmx/SetCanSeeMe", true,
   "{canSeeMeView:" + $.Serialize(can_see_me_view) + "}",
      function (json) {
          var result = json.d;
          if (result == true) {
              if ($(event.target).hasClass("icon-eye") == true) {
                  $(event.target).addClass("icon-eye-no").removeClass("icon-eye").attr("title", "禁止查看我的消息");
              }
              else {
                  $(event.target).addClass("icon-eye").removeClass("icon-eye-no").attr("title", "查看我的消息");
              }
              $(event.target).data("CanSeeMe", can_see_me_view== true ? Enum.YesNo.Yes.toString() : Enum.YesNo.No.toString());
          }
          else {
              console.log("设置查看我的信息失效");
          }
  });
   
}
//返回主页
AddressBook.BackEvent = function BackEvent(event) {
    objPub.InitLeftMain(true);

}
AddressBook.AddEvent = function AddEvent(event) {
    var keyword = {
        Keyword: $("#txtSearchUserKeyword").val()
    };
    var page = { pageStart: 1, pageEnd: AddressBook.AddFriends.PageSize * 1 };
    AddressBook.AddFriends.Search(keyword, page);
    //查询通讯录事件
    $("#spnSearchUser").on("click", { Page: page }, AddressBook.AddFriends.SearchEvent);
    $("#txtSearchUserKeyword").off("keypress").on("keypress", function (event) {
        if (event.which == 13) {
            AddressBook.AddFriends.Search({
                Keyword: $("#txtSearchUserKeyword").val()
            }, { pageStart: 1, pageEnd: AddressBook.AddFriends.PageSize * 1 });
            return false;
        }
    });
    $("#sctSearchUser").dialog("open");
}
AddressBook.SearchBlackListEvent = function SearchBlackListEvent(event) {
    //搜索黑名单
    $("#divAddressBook").text("通讯录（黑名单）");
    $("#spnMyFriendslistToBlacklist").hide();
    $(".icon-no-user").css("display", "inline-block");
    var keyword = {
        Keyword: $("#txtKeyword").val()
    };
    var page = { pageStart: 1, pageEnd: AddressBook.PageSize * 1 };
    AddressBook.AddBlackList.Search(keyword, page);
    $("#spnSearch").off("click").on("click", { Page: page }, AddressBook.AddBlackList.SearchEvent);
    $("#txtKeyword").off("keypress").on("keypress", function (event) {
        if (event.which == 13) {
            AddressBook.AddBlackList.Search({
                Keyword: $("#txtKeyword").val()
            }, { pageStart: 1, pageEnd: AddressBook.PageSize * 1 });
            return false;
        }
    });
}


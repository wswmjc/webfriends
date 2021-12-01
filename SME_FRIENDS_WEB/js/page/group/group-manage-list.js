Group.Manage = function () { }
Group.Manage.registerClass("Group.Manage");
Group.Manage.PageSize = 10;
Group.Manage.TotalCount = 0;
Group.Manage.Init = function init() {
    objPub.IsMain = false;
    $(document).off("scroll");
    //返回事件
    $("#aGoBack").off("click").on("click", Group.Manage.BackEvent);
    var keyword = {
        Keyword: $("#txtKeyword").val()
    };
    var page = { pageStart: 1, pageEnd: Group.Manage.PageSize * 1 };
    Group.Manage.Search(keyword, page);
    //查询讨论组事件
    $("#spnSearch").on("click", { Page: page }, Group.Manage.SearchEvent);
    $("#txtKeyword").off("keypress").on("keypress", function (event) {
        if (event.which == 13) {
            Group.Manage.Search({
                Keyword: $("#txtKeyword").val()
            }, { pageStart: 1, pageEnd: Group.Manage.PageSize * 1 });
            return false;
        }
    });
    //打开建立组窗口
    $("#addGroup").on("click", {Page:page},Group.Manage.AddGroupEvent);

}
//添加讨论组
Group.Manage.AddGroupEvent = function AddGroupEvent(event) {
    var page = event.data.Page;
    Group.Manage.MyFriendsBind();
    //清空成员列表
    $("#sltGroupMemberList").html("");
    $("#spnLeftMove").off("click").on("click", Group.Manage.MoveGroupMemberEvent);
    $("#spnRightMove").off("click").on("click", Group.Manage.MoveMyFriendsEvent);
    $("#sctAddGroup").dialog({
        resizable: false,
        width: 800,
        modal: true,
        title: "新建讨论组",
        appendTo:".main-left",
        buttons: {
            "取　消": function () {
                $(this).dialog("close");
            },
            "组　建": function () {
                Group.Manage.CreateGroup(page);
                $(this).dialog("close");
            }
        }
    });
}
//返回主页事件
Group.Manage.BackEvent = function BackEvent(event) {
    objPub.InitLeftMain(true);
}
Group.Manage.Search = function search(keyword, page) {
    Group.Manage.SearchBind(keyword, page);
    $.SimpleAjaxPost("service/GroupService.asmx/GetSearchCount", true,
      "{searchView:" + $.Serialize(keyword) + "}",
       function (json) {
           var result = json.d;
           Group.Manage.TotalCount = result;
           if (result <= Group.Manage.PageSize) {
               $("#divGroupPage").wPaginate("destroy");
           }
           else {
               $("#divGroupPage").wPaginate("destroy").wPaginate({
                   theme: "grey",
                   first: "首页",
                   last: "尾页",
                   total: result,
                   index: 0,
                   limit: Group.Manage.PageSize,
                   ajax: true,
                   url: function (i) {
                       var page = {
                           pageStart: i * this.settings.limit + 1,
                           pageEnd: (i + 1) * this.settings.limit
                       };
                       Group.Manage.SearchBind(keyword, page);
                   }
               });
           }
       });
}
//搜索事件
Group.Manage.SearchEvent = function SearchEvent(event) {
    var page = event.data.Page;
    var keyword = {
        Keyword: $("#txtKeyword").val()
    };
    Group.Manage.Search(keyword, page);
}
//搜索绑定
Group.Manage.SearchBind = function search_bind(keyword, page) {
    $.SimpleAjaxPost("service/GroupService.asmx/Search", true, "{searchView:" + $.Serialize(keyword) + ",page:" + $.Serialize(page) + "}",
         function (json) {
             var result = $.Deserialize(json.d);
             var temp = "";
             if (result != null) {
                 $.each(result, function (index, item) {
                     temp += "<li class='clear-fix'>";
                     temp += "<div class='contacts-info'><div class='contacts-photo' id='divGroupPhoto" + index + "'><img src='" + item.LogoUrl + "' title='点击进入"+item.Name+"讨论组看看?'></div>";
                     $(document).off("click", "#divGroupPhoto" + index);
                     $(document).on("click", "#divGroupPhoto" + index, { ID: item.ID, Name: item.Name }, function (event) {
                         var group_id = event.data.ID;
                         var group_name = event.data.Name;
                         $(".main-left").load("../biz/left/group/detail-list.html", function (response, status) {
                             if (status == "success") {
                                 $("#divGroupName").text(group_name);
                                 Group.List.Init(group_id);
                             }
                         });
                     });
                     temp += "<span class='contacts-name'>"+item.Name+"</span>";
                     temp += "<span class='contacts-number'>" + item.MemberCount + "人</span>";
                     if (item.Remark != "") {
                         temp += "<span id='spnGroupRemark" + index + "' class='contacts-memo'>" + item.Remark + "</span>";
                     }
                     else {
                         temp += "<span id='spnGroupRemark" + index + "' ></span>";
                     }
                     temp += "</div>";
                     temp += " <div class='contacts-option'>";
                     if (objPub.UserID == item.CreaterID) {
                         temp += "<span class='user-manager'><span class='icon-optSet icon-img icon-manager' title='管理员'></span></span>";
                     }
                     temp += "<span id='spnInviteFriend" + index + "'>添加好友</span>";
                     $(document).off("click", "#spnInviteFriend" + index);
                     $(document).on("click", "#spnInviteFriend" + index, { GroupID: item.ID, GroupName: item.Name }, Group.Manage.InviteAddressEvent);
                     temp += "<span id='spnGroupInfo" + index + "'>小组成员</span>";
                     $(document).off("click", "#spnGroupInfo" + index);
                     $(document).on("click", "#spnGroupInfo" + index, { GroupID: item.ID, GroupName: item.Name, IsManage: (objPub.UserID == item.CreaterID) }, Group.Manage.DetailGroupEvent);
                     temp += "<span id='spnSetRemark"+index+"'>备注</span>";
                     if (objPub.UserID == item.CreaterID) {
                         temp += "<span id='spnDismiss" + index + "'>解散</span>";
                         $(document).off("click", "#spnDismiss" + index);
                         $(document).on("click","#spnDismiss"+index,{GroupID:item.ID,GroupName:item.Name,Page:page},Group.Manage.DismissEvent);
                     }
                     else {
                         temp += "<span id='spnRemove" + index + "'>退出</span>";
                         $(document).off("click", "#spnRemove" + index);
                         $(document).on("click", "#spnRemove" + index, { GroupID: item.ID, GroupName: item.Name, Page: page }, Group.Manage.RemoveEvent);
                     }
                     temp+="</div>";
                     temp += "</li>";
                     $(document).off("click", "#spnSetRemark" + index);
                     $(document).on("click", "#spnSetRemark" + index, { Index: index, ID: item.GroupMemberID, GroupName: item.Name }, Group.Manage.SetRemarkEvent);
                 });
                 $("#divEmptyGroup").hide();
             }
             else {
                 $("#divEmptyGroup").show();
             }
             $("#ulGroupList").empty().append(temp);
         });
}
Group.Manage.SetRemarkEvent =function SetRemarkEvent(event){
    var id = event.data.ID;
    var group_name = event.data.GroupName;
    var index = event.data.Index;
    $("#spnGroupName").text(group_name);
    $("#txtGroupRemark").val($("#spnGroupRemark" + index).text());
    $("#sctGroupRemark").dialog({
        resizable: false,
        width: 500,
        modal: true,
        title: "添加备注",
        buttons: {
            "取　消": function () {
                $(this).dialog("close");
            },
            "提　交": function () {
                var remark_view = {
                    ID: id,
                    Remark: $("#txtGroupRemark").val()
                };
                $.SimpleAjaxPost("service/GroupService.asmx/SetRemark", true,
                "{remarkView:" + $.Serialize(remark_view) + "}",
                function (json) {
                    var result = json.d;
                    if (result == true) {
                        $.Alert("添加讨论组备注成功！", function () {
                            $("#spnGroupRemark" + index).text(remark_view.Remark);
                            if (remark_view.Remark != "") {
                                if ($("#spnGroupRemark" + index).hasClass("contacts-memo") == false) {
                                    $("#spnGroupRemark" + index).addClass("contacts-memo");
                                }
                            }
                            else {
                                if ($("#spnGroupRemark" + index).hasClass("contacts-memo") == true) {
                                    $("#spnGroupRemark" + index).removeClass("contacts-memo");
                                }
                            }
                            PublishRight.MyGroupBind();
                        });
                    }
                    else {
                        console.log("添加讨论组备注失败！");
                    }
                });
                $(this).dialog("close");
            }
        }
    });
}
//解散事件
Group.Manage.DismissEvent = function DismissEvent(event) {
    var group_name = event.data.GroupName;
    var group_id = event.data.GroupID;
    var page = event.data.Page;
    $.Confirm({ content: "您确定要解散" + group_name + "讨论组么？" ,width:"auto"}, function () {
        $.SimpleAjaxPost("service/GroupService.asmx/Remove", true,
          "{groupID:'" + group_id + "'}",
          function (json) {
              var result = json.d;
              if (result == true) {
                  $.Alert({content:"解散"+group_name+"讨论组成功！",width:"auto"}, function () {
                      var keyword = {
                          Keyword: $("#txtKeyword").val()
                      };
                      Group.Manage.Search(keyword, page);
                      PublishRight.MyGroupBind();
                      AtInfo.GetAtValidationCount();
                      Group.Manage.TotalCount--;
                      if (Group.Manage.TotalCount == 0) {
                          $("#divHasGroup").hide();
                      }
                  });
              }
              else {
                  console.log("解散讨论组失败！");
              }
          });
    });
  
}
//退出事件
Group.Manage.RemoveEvent = function RemoveEvent(event) {
    var group_name = event.data.GroupName;
    var group_id = event.data.GroupID;
    var page = event.data.Page;
    $.Confirm({content:"您确定要退出"+group_name+"讨论组么？",width:"auto"},function(){
        var members = new Array();
        members.push(objPub.UserID);
        $.SimpleAjaxPost("service/GroupService.asmx/RemoveMember", true,
            "{groupID:'" + group_id + "',members:" + $.Serialize(members) + "}",
            function (json) {
                var result = json.d;
                if (result == true) {
                    $.Alert({ content: "退出" + group_name + "讨论组成功！" ,width:"auto"}, function () {
                        var keyword = {
                            Keyword: $("#txtKeyword").val()
                        };
                        Group.Manage.Search(keyword, page);
                        PublishRight.MyGroupBind();
                        AtInfo.GetAtValidationCount();
                        Group.Manage.TotalCount--;
                        if (Group.Manage.TotalCount == 0) {
                            $("#divHasGroup").hide();
                        }
                    });
                }
                else {
                    console.log("退出讨论组失败！");
                }
            });
    });
    
    
}
//绑定通讯录用户
Group.Manage.MyFriendsBind = function my_friends_bind() {
    var keyword = {
        Keyword: ""
    };
    $.SimpleAjaxPost("service/AddressBookService.asmx/Search", true,
      "{searchView:" + $.Serialize(keyword) + ",page:null}",
       function (json) {
           var result = $.Deserialize(json.d);
           var temp = "";
           if (result != null) {
               $.each(result, function (index, item) {
                   temp += "<option value='" + item.AddresserID + "'>" + item.AddresserNickName + ($.IsNullOrEmpty(item.Remark) ? "" : "(" + item.Remark + ")") + "</option>";
               });
           }
           $("#sltMyFriendsList").empty().append(temp);
       });
}
//→移
Group.Manage.MoveGroupMemberEvent = function MoveGroupMemberEvent(event) {  
    if ($("#sltMyFriendsList option:selected").length == 0) {
        $.Alert("请选择待移动对象");
    }
    else {
        var temp = "";
        $.each($("#sltMyFriendsList option:selected"), function (index, item) {
            temp += "<option value='" + $(item).val() + "'>" + $(item).text() + "</option>";
        });
        $("#sltMyFriendsList option:selected").remove();
        $("#sltGroupMemberList").append(temp);
    }
}
//←移
Group.Manage.MoveMyFriendsEvent = function MoveMyFriendsEvent(event) {
    if ($("#sltGroupMemberList option:selected").length == 0) {
        $.Alert("请选择待移动对象");
    }
    else {
        var temp = "";
        $.each($("#sltGroupMemberList option:selected"), function (index, item) {
            temp += "<option value='" + $(item).val() + "'>" + $(item).text() + "</option>";
        });
        $("#sltGroupMemberList option:selected").remove();
        $("#sltMyFriendsList").append(temp);
    }
}
//创建小组事件
Group.Manage.CreateGroup = function create_group(page) {
    var group_info = {
        ID:$.NewGuid(),
        Name: $("#txtGroupName").val()
        
    };
    var members = new Array();
    $.each($("#sltGroupMemberList option"), function (index, item) {
        members.push({
            ID:$.NewGuid(),
            GroupID: group_info.ID,
            MemberID: $(item).val(),
            MemberName: $(item).text().replace(/\([^\)]*\)/g,"")
        });
    });
    $.SimpleAjaxPost("service/GroupService.asmx/Add", true,
      "{groupInfo:" + $.Serialize(group_info) + ",members:"+(members.length==0?null:$.Serialize(members))+"}",
       function (json) {
           var result = json.d;
           if (result == true) {
               $.Alert("创建讨论组成功！", function () {
                       var keyword = {
                           Keyword: $("#txtKeyword").val()
                       };
                       Group.Manage.Search(keyword, page);
                       Group.Manage.Clear();
                       PublishRight.MyGroupBind();
                       $("#divHasGroup").show();
               });
              
           }
           else {
               console.log("创建讨论组失败！");
           }
       });
}
//清除弹出框内容
Group.Manage.Clear = function clear() {
    $("#txtGroupName").val("");
    $("#sltGroupMemberList option,#sltMyFriendsList option").remove();
}

Group.Manage.InviteAddressEvent = function InviteAddressEvent(event) {
    var group_id = event.data.GroupID;
    var group_name = event.data.GroupName;
    $("#spnInviteGroupName").text(group_name);
    $("#txtInviteAddresserKeyword").val("");

    $("#spnSearchInviteAddresser").off("click").on("click", {GroupID:group_id, Page: null }, Group.Manage.SearchInvitingAddressEvent);
    $("#txtInviteAddresserKeyword").off("keypress").on("keypress", function (event) {
        if (event.which == 13) {
            Group.Manage.SearchInvitingAddress({
                GroupID: group_id,
                Keyword: $("#txtInviteAddresserKeyword").val()
            }, { pageStart: 1, pageEnd: Group.Manage.PageSize * 1 });
            return false;
        }
    });
    Group.Manage.SearchInvitingAddress({
        GroupID: group_id,
        Keyword: $("#txtInviteAddresserKeyword").val()
    }, { pageStart: 1, pageEnd: Group.Manage.PageSize * 1 });
    $("#sctInviteAddresser").dialog({
        resizable: false,
        width: 600,
        modal: true,
        title: "添加好友",
        appendTo: ".main-left",
        buttons: {
            "取　消": function () {
                $(this).dialog("close");
            },
            "添  加": function () {
                Group.Manage.AddInvitingAddresser();       
            }
        }
    });
}

Group.Manage.SearchInvitingAddress = function search_inviting_address(search_view, page) {
    Group.Manage.SearchInvitingAddressBind(search_view, page);
    if (page) {
        $.SimpleAjaxPost("service/GroupService.asmx/GetInvitingAddressCount", true,
          "{groupSearchView:" + $.Serialize(search_view) + "}",
           function (json) {
               var result = json.d;
               if (result <= Group.Manage.PageSize) {
                   $("#divFindInviteAddresserPage").wPaginate("destroy");
               }
               else {
                   $("#divFindInviteAddresserPage").wPaginate("destroy").wPaginate({
                       theme: "grey",
                       first: "首页",
                       last: "尾页",
                       total: result,
                       index: 0,
                       limit: Group.Manage.PageSize,
                       ajax: true,
                       url: function (i) {
                           var page = {
                               pageStart: i * this.settings.limit + 1,
                               pageEnd: (i + 1) * this.settings.limit
                           };
                           Group.Manage.SearchInvitingAddressBind(search_view, page);
                       }
                   });
               }
           });
    }
}

Group.Manage.SearchInvitingAddressEvent = function SearchInvitingAddressEvent(event) {
    var group_id = event.data.GroupID;
    var page = event.data.Page;
    var search_view = {
        GroupID: group_id,
        Keyword: $("#txtInviteAddresserKeyword").val()
    };
    Group.Manage.SearchInvitingAddressBind(search_view, page);
}

Group.Manage.SearchInvitingAddressBind = function search_inviting_address_bind(search_view, page) {
    $("#divEmptyInviteAddresserFind").find("span").text("暂没有匹配的联系人列表哦~");
    $("#divEmptyInviteAddresserFind").hide();
    $.SimpleAjaxPost("service/GroupService.asmx/GetInvitingAddressList", true, "{groupSearchView:" + $.Serialize(search_view) + ",page:" + (page ? $.Serialize(page) : null) + "}",
         function (json) {
             var result = $.Deserialize(json.d);
             var temp = "";
             if (result != null) {
                 $.each(result, function (index, item) {
                     temp += "<li class='clear-fix'>";
                     temp += "<div class='contacts-choose'>";
                     temp += "<input type='checkbox' name='ckInviteAddresser' id='ckInviteAddresser"+index+"'>";
                     temp += "</div>";
                     temp += "<div class='contacts-info'>";
                     temp += "<div class='contacts-photo'><img src='" + item.UserUrl + "'></div>";
                     temp += "<div class='contrcts-text'>";
                     temp += "<div>";
                     temp += "<span class='contacts-name'>" + item.UserName + "</span>";
                     temp += "</div>";
   
                     temp += "</div>";
                     temp += "</div>";
                     temp += "</li>";
                 });
                 $("#ulInviteAddresserFindList").empty().append(temp);
                 $.each(result, function (index, item) {
                     $("#ckInviteAddresser" + index).removeData("addresser").data("addresser", {
                         GroupID: search_view.GroupID,
                         MemberID: item.UserID,
                         MemberName: item.UserCode
                     });
                 });
             } else {
                 $("#ulInviteAddresserFindList").empty();
                 $("#divEmptyInviteAddresserFind").show();
             }
         });
}

Group.Manage.AddInvitingAddresser = function add_inviting_addresser() {
    var members = new Array();
    $.each($("input[name='ckInviteAddresser']"), function (index, item) {
        if ($(item).prop("checked") == true) {
            members.push({
                ID: $.NewGuid(),
                GroupID: $(item).data("addresser").GroupID,
                MemberID: $(item).data("addresser").MemberID,
                MemberName: $(item).data("addresser").MemberName
            });
        }
    });
    if (members.length != 0) {
        $.SimpleAjaxPost("service/GroupService.asmx/AddMember", true,
         "{members:" + $.Serialize(members) + "}",
          function (json) {
              var result = json.d;
              if (result == false) {
                  console.log("邀请并添加联系人到讨论组失败！");
                  $("#spnSearchInviteAddresser").trigger("click");
              } else {
                  var keyword = {
                      Keyword: $("#txtKeyword").val()
                  };
                  Group.Manage.Search(keyword, { pageStart: 1, pageEnd: Group.Manage.PageSize * 1 });
                  $("#sctInviteAddresser").dialog("close");
                  $.Alert("邀请联系人成功!");
              }
          });
    } else {
        $.Alert("请选择要添加的联系人!");
    }
}

Group.Manage.GetGroupMember = function get_group_member(group_id,is_manage) {
    $.SimpleAjaxPost("service/GroupService.asmx/GetMemberInfoList", true, "{groupID:'" + group_id + "'}",
        function (json) {
            var result = $.Deserialize(json.d);
            var temp = "";
            if (result != null) {
                $("#divEmptyDetailGroupFind").hide();
                $.each(result, function (index, item) {
                    temp += "<li class='clear-fix'>";
                    if (is_manage) {
                        temp += "<div class='contacts-choose'>";
                        if (item.MemberID != objPub.UserID) {
                            temp += "<input type='checkbox' name='ckGroupMember' id='ckGroupMember" + index + "'>";
                        } else {
                            temp += "<input type='checkbox' name='ckGroupMember' id='ckGroupMember" + index + "' disabled='disabled' style='cursor:default;'>";
                        }
                        temp += "</div>";
                    }
                    temp += "<div class='contacts-info'>";
                    temp += "<div class='contacts-photo'><img src='" + item.MemberUrl + "'></div>";
                    temp += "<div class='contrcts-text'>";
                    temp += "<div>";
                    temp += "<span class='contacts-name'>" + item.Remark + "</span>";
                    temp += "</div>";
              
                    temp += "</div>";
                    temp += "</div>";
                    temp += "</li>";
                });
                $("#ulDetailGroupFindList").empty().append(temp);
                if (is_manage) {
                    $.each(result, function (index, item) {
                        $("#ckGroupMember" + index).removeData("memberID").data("memberID", item.MemberID);
                    });
                }
            } else {
                $("#ulDetailGroupFindList").empty();
                $("#divEmptyDetailGroupFind").show();
            }
        });
}

Group.Manage.DetailGroupEvent = function DetailGroupEvent(event) {
    var group_id = event.data.GroupID;
    var group_name = event.data.GroupName;
    var is_manage = event.data.IsManage;
    Group.Manage.GetGroupMember(group_id, is_manage);
    var buttons = {
        "确　定": function () {
            $(this).dialog("close");
        }
    };

    if (is_manage) {
        buttons = {
            "取　  消": function () {
                $(this).dialog("close");
            },
            "移除成员": function () {
                Group.Manage.RemoveGroupMember(group_id);
            }
        };
    }

    $("#sctDetailGroup").dialog({
        resizable: false,
        width: 450,
        modal: true,
        title: group_name+"组 小组成员",
        appendTo: ".main-left",
        buttons: buttons
    });
}

Group.Manage.RemoveGroupMember = function remove_group_member(group_id) {
    var members = new Array();
    $.each($("input[name='ckGroupMember']"), function (index, item) {
        if ($(item).prop("checked") == true) {
            members.push($(item).data("memberID"));
        }
    });
    if (members.length != 0) {
        $.SimpleAjaxPost("service/GroupService.asmx/RemoveMember", true,
         "{groupID:'"+group_id+"',members:" + $.Serialize(members) + "}",
          function (json) {
              var result = json.d;
              if (result == false) {
                  console.log("删除讨论组成员失败！");
              } else {
                  var keyword = {
                      Keyword: $("#txtKeyword").val()
                  };
                  Group.Manage.Search(keyword, { pageStart: 1, pageEnd: Group.Manage.PageSize * 1 });
                  $("#sctDetailGroup").dialog("close");
                  $.Alert("删除讨论组成员成功!");
              }
          });
    } else {
        $.Alert("请选择要删除的组成员!");
    }
}
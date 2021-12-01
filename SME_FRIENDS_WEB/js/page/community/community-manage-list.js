Community.Manage = function () {
//行业圈子管理业务逻辑-我的行业圈子
}
Community.Manage.registerClass("Community.Manage");
Community.Manage.PageSize = 10;
Community.Manage.DefaultPhtoUrl = "../images/user-default.png";
Community.Manage.OldPhotoUrl = Community.Manage.DefaultPhtoUrl;
Community.Manage.NewPhotoUrl = Community.Manage.DefaultPhtoUrl;
Community.Manage.TotalCount = 0;
Community.Manage.Init = function init(tab_index) {
    objPub.IsMain = false;
    $(document).off("scroll");
    //返回事件
    $("#aGoBack").off("click").on("click", Community.Manage.BackEvent);
    var keyword = {
        Keyword: $("#txtKeyword").val()
    };
    var page = { pageStart: 1, pageEnd: Community.Manage.PageSize * 1 };

    if (tab_index == 1) {
        //我的圈子
        $("#liMyCommunity").addClass("selected");
        Community.Manage.Search(keyword, page);
        //查询讨论组事件
        $("#spnSearch").on("click", { Page: page }, Community.Manage.SearchEvent);
        $("#txtKeyword").off("keypress").on("keypress", function (event) {
            if (event.which == 13) {
                Community.Manage.Search({
                    Keyword: $("#txtKeyword").val()
                }, { pageStart: 1, pageEnd: Community.Manage.PageSize * 1 });
                return false;
            }
        });
    }
    else {
        //热门圈子
        $("#liRecommendCommunity").addClass("selected");
        Community.Recommend.Search(keyword, page);
        //查询讨论组事件
        $("#spnSearch").on("click", { Page: page }, Community.Recommend.SearchEvent);
        $("#txtKeyword").off("keypress").on("keypress", function (event) {
            if (event.which == 13) {
                Community.Recommend.Search({
                    Keyword: $("#txtKeyword").val()
                }, { pageStart: 1, pageEnd: Community.Manage.PageSize * 1 });
                return false;
            }
        });
    }
    //点击我的圈子事件
    $("#liMyCommunity").on("click", { Page: page }, Community.Manage.MyCommunityEvent);
    //点击热门圈子事件
    $("#liRecommendCommunity").on("click", { Page: page }, Community.Recommend.RecommendCommunityEvent);
    //判断权限是否能够建立行业圈子，如果可以初始化链接，否则隐藏
    Community.Manage.CanCreateCommunity(page);
}
//点击我的圈子事件
Community.Manage.MyCommunityEvent = function MyCommunityEvent(event) {
    var page = event.data.Page;
    $(".circle-tabs>ul>li").removeClass("selected");
    $(event.target).addClass("selected");
    var keyword = {
        Keyword: $("#txtKeyword").val()
    };
    Community.Manage.Search(keyword, page);
    //查询讨论组事件
    $("#spnSearch").off("click").on("click", { Page: page }, Community.Manage.SearchEvent);
    $("#txtKeyword").off("keypress").on("keypress", function (event) {
        if (event.which == 13) {
            Community.Manage.Search(keyword, { pageStart: 1, pageEnd: Community.Manage.PageSize * 1 });
            return false;
        }
    });
}
//是否能够建立圈子
Community.Manage.CanCreateCommunity = function can_create_community(page) {
    $.SimpleAjaxPost("service/CommunityService.asmx/CanCreateCommunity", true,
       function (json) {
           var result = json.d;
           if (result == true) {
               $("#divAddCommunity").show();
               $("#addCommunity").off("click").on("click", { Page: page }, Community.Manage.AddEvent);
           }
           else {
               $("#divAddCommunity").remove();
           }
       });

}
//返回主页事件
Community.Manage.BackEvent = function BackEvent(event) {
    objPub.InitLeftMain(true);
}
//上传Logo
Community.Manage.UploadEvent = function UploadEvent(event) {
    var id = event.data.ID;
    var image_id = event.data.ImageID;
    if ($(event.target).val() != "") {
        $("#" + id).ajaxSubmit({
            url: $.GetBaseUrl() + "/service/CommunityLogoUploadHandlerService.ashx",
            type: "post",
            dataType: "json",
            timeout: 600000,
            success: function (data, textStatus) {
                if (data.result == true) {
                    Community.Manage.NewPhotoUrl = data.acc.FilePath;
                    $("#" + image_id).attr("src", data.acc.TempPath);
                } else {
                    $.Alert({
                        content: data.message,
                        width: "auto"
                    });
                }
            },
            error: function (data, status, e) {
                $.Alert("上传失败，错误信息：" + e);
            }
        });
    }
}
//添加圈子事件
Community.Manage.AddEvent = function AddEvent(event) {
    var page = event.data.Page;
    Community.Manage.MyFriendsBind();
    //清空成员列表
    $("#sltCommunityMemberList").html("");
    $("#spnLeftMove").off("click").on("click", Community.Manage.MoveCommunityMemberEvent);
    $("#spnRightMove").off("click").on("click", Community.Manage.MoveMyFriendsEvent);
    //上传头像
    $("#btnUploadCommunityImage").off("click").on("click", function (event) {
        $("#fileCommunityPhoto").trigger("click");
    });
    //上传头像
    $("#fileCommunityPhoto").off("change").on("change", { ID: "fmCommuntiyPhoto", ImageID: "imgCommunityPhoto" }, Community.Manage.UploadEvent);
    $("#sctAddCommunity").dialog({
        resizable: false,
        width: 650,
        modal: true,
        title: "新建圈子",
        appendTo: ".main-left",
        buttons: {
            "取　消": function () {
                Community.Manage.AddClear();
                $(this).dialog("close");
            },
            "组　建": function () {
                Community.Manage.CreateCommunity(page);
                $(this).dialog("close");
            }
        }
    });
}
//创建圈子
Community.Manage.CreateCommunity = function create_community(page) {
    var community_info = {
        ID: $.NewGuid(),
        Name: $("#txtCommunityName").val(),
        Remark: $("#txtCommunityRemark").val(),
        LogoUrl: Community.Manage.NewPhotoUrl
    };
    var members = new Array();
    $.each($("#sltCommunityMemberList option"), function (index, item) {
        members.push({
            ID: $.NewGuid(),
            CommunityID: community_info.ID,
            MemberID: $(item).val(),
            MemberName: $(item).text().replace(/\([^\)]*\)/g, "")
        });
        //console.log("ID:" + $(item).val());
        //console.log("ID:" + $(item).text());
    });
    console.log("communityInfo:" + $.Serialize(community_info));
    console.log("memberApplications:" + (members.length == 0 ? null : $.Serialize(members)));
    $.SimpleAjaxPost("service/CommunityService.asmx/Add", true,
       "{communityInfo:" + $.Serialize(community_info) + ",memberApplications:" + (members.length == 0 ? null : $.Serialize(members)) + "}",
        function (json) {
            var result = json.d;
            if (result == true) {
                $.Alert("创建圈子成功！", function () {
                    var keyword = {
                        Keyword: $("#txtKeyword").val()
                    };
                    Community.Manage.Search(keyword, page);
                    PublishRight.MyCommunityBind();
                    PublishRight.CommunityRecommendBind();
                    Community.Manage.AddClear();
                    $("#divHasCommunity,#divHasCommunityLabel,#divHasCommunityList").show(); 
                });
            }
            else {
                console.log("创建圈子失败！");
            }
        });
}
//绑定通讯录用户
Community.Manage.MyFriendsBind = function my_friends_bind() {
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
                   temp += "<option value='" + item.AddresserID + "'>" + item.AddresserNickName + (item.Remark == null ? "" : "(" + item.Remark + ")") + "</option>";
               });
           }
           $("#sltMyFriendsList").empty().append(temp);
       });
}
//→移
Community.Manage.MoveCommunityMemberEvent = function MoveCommunityMemberEvent(event) {
    if ($("#sltMyFriendsList option:selected").length == 0) {
        $.Alert("请选择待移动对象");
    }
    else {
        var temp = "";
        $.each($("#sltMyFriendsList option:selected"), function (index, item) {
            temp += "<option value='" + $(item).val() + "'>" + $(item).text() + "</option>";
        });
        $("#sltMyFriendsList option:selected").remove();
        $("#sltCommunityMemberList").append(temp);
    }
}
//←移
Community.Manage.MoveMyFriendsEvent = function MoveMyFriendsEvent(event) {
    if ($("#sltCommunityMemberList option:selected").length == 0) {
        $.Alert("请选择待移动对象");
    }
    else {
        var temp = "";
        $.each($("#sltCommunityMemberList option:selected"), function (index, item) {
            temp += "<option value='" + $(item).val() + "'>" + $(item).text() + "</option>";
        });
        $("#sltCommunityMemberList option:selected").remove();
        $("#sltMyFriendsList").append(temp);
    }
}
//清除添加圈子的信息
Community.Manage.AddClear = function add_clear() {
    $("#txtCommunityName,#txtCommunityRemark").val("");
    $("#imgCommunityPhoto").attr("src", Community.Manage.DefaultPhtoUrl);
    Community.Manage.OldPhotoUrl = Community.Manage.DefaultPhtoUrl;
}
Community.Manage.Search = function search(keyword, page) {
    Community.Manage.SearchBind(keyword, page);
    $.SimpleAjaxPost("service/CommunityService.asmx/GetSearchCount", true,
      "{searchView:" + $.Serialize(keyword) + "}",
       function (json) {
           var result = json.d;
           Community.Manage.TotalCount = result;
           if (result <= Community.Manage.PageSize) {
               $("#wPaginate8nPosition").wPaginate("destroy");
           }
           else {
               $("#wPaginate8nPosition").wPaginate("destroy").wPaginate({
                   theme: "grey",
                   first: "首页",
                   last: "尾页",
                   total: result,
                   index: 0,
                   limit: Community.Manage.PageSize,
                   ajax: true,
                   url: function (i) {
                       var page = {
                           pageStart: i * this.settings.limit + 1,
                           pageEnd: (i + 1) * this.settings.limit
                       };
                       Community.Manage.SearchBind(keyword, page);
                   }
               });
           }
       });
}
//搜索事件
Community.Manage.SearchEvent = function SearchEvent(event) {
    var page = event.data.Page;
    var keyword = {
        Keyword: $("#txtKeyword").val()
    };
    Community.Manage.Search(keyword, page);
}
//搜索绑定
Community.Manage.SearchBind = function search_bind(keyword, page) {
    $.SimpleAjaxPost("service/CommunityService.asmx/Search", true, "{searchView:" + $.Serialize(keyword) + ",page:" + $.Serialize(page) + "}",
         function (json) {
             var result = $.Deserialize(json.d);
             //console.log(result);
             var temp = "";
             if (result != null) {
                 $.each(result, function (index, item) {
                     temp += "<li class='clear-fix clear-fix'>";
                     temp += "<div class='circle-manage-photo' id='divCirclePhoto" + index + "'> <img src='" + item.LogoUrl + "' title='点击进"+item.Name+"圈子看看？'></div>";
                     temp += "<div class='circle-info'>";
                     temp += "<div class='circle-title clear-fix'>";
                     temp += "<div id='divCommunityName" + index + "' class='circle-name' style='cursor:pointer;' title='点击进" + item.Name + "圈子看看？'>" + item.Name + "</div>";
                     $(document).off("click", "#divCirclePhoto" + index+",#divCommunityName"+index);
                     $(document).on("click", "#divCirclePhoto" + index + ",#divCommunityName" + index, { ID: item.ID, Name: item.Name }, function (event) {
                         var community_id = event.data.ID;
                         var community_name = event.data.Name;
                         $(".main-left").ReadTemplate(Template.DetailCommunityTpl, function () {
                             $("#divCommunityName").text(community_name);
                             Community.Label.Init(community_id);
                         });
                     });
                     temp += "<div class='circle-time'>";
                     if (item.CreaterID == objPub.UserID) {
                         temp += "<span class='icon-optSet icon-img icon-manager'></span> <span>创建时间：</span><span>" + item.CreateTime.format("yyyy-MM-dd") + "</span>"
                     }
                     else {
                         temp += "<span>加入时间：</span><span>" + item.JoinTime.format("yyyy-MM-dd") + "</span>"
                     }
                     temp += "</div>";
                     temp += "</div>";
                     temp += "<div class='circle-intro'>";
                     temp += "<div class='circle-text' id='divCommunityRemark" + index + "'>" + item.Remark + "</div>";
                     temp += "<div class='circle-text-full' id='divCommunityRemarkFull" + index + "'>" + item.Remark + "</div>";
                     temp += "<div class='circle-total clear-fix'>";
                     temp += "<ul>";
                     temp += "<li><span>成员数</span><span>" + item.MemberCount + "</span></li>";
                     temp += "<li><span>话题数</span><span>" + item.TopicCount + "</span></li>";
                     temp += "<li><span>讨论数</span><span>" + item.MessageCount + "</span></li>";
                     temp += "</ul>";
                     temp += "<span class='show-all' title='查看简介全文' id='spnShowFullRemark"+index+"'>展开</span>";
                     temp += "</div>";
                     temp += "<div class='circle-btns'>";
                     if (item.CreaterID == objPub.UserID) {
                         temp += "<input id='btnManageMember" + index + "' type='button' value='圈主管理' class='manage-circle'>";
                         $(document).off("click", "#btnManageMember" + index);
                         $(document).on("click", "#btnManageMember" + index, { ID: item.ID, Page: page }, Community.Manage.ManageMemberEvent);
                     }
                     if (item.IsAdmin.toString() == Enum.YesNo.Yes.toString()) {

                         temp += "<input id='btnJoinMember" + index + "' type='button' value='邀请好友' class='manage-friend'>";
                         temp += "<input id='btnRemoveMember" + index + "' type='button' value='移除成员' class='delete-friend'>";
                         $(document).off("click", "#btnJoinMember" + index);
                         $(document).on("click", "#btnJoinMember" + index, { ID: item.ID, Name: item.Name }, Community.Manage.JoinMemberEvent);
                         $(document).off("click", "#btnRemoveMember" + index);
                         $(document).on("click", "#btnRemoveMember" + index, { ID: item.ID, IsCreater: (item.CreaterID == objPub.UserID ? true : false), Page: page }, Community.Manage.RemoveMemberEvent);
                     }

                     if (item.CreaterID == objPub.UserID) {
                         temp += "<input id='btnDismiss" + index + "' type='button' value='解散' class='btn-exit'>";
                         $(document).off("click", "#btnDismiss" + index);
                         $(document).on("click", "#btnDismiss" + index, { ID: item.ID, CommunityName: item.Name, Page: page }, Community.Manage.DismissEvent);
                     }
                     else {
                         temp += "<input id='btnExist" + index + "' type='button' value='退出' class='btn-exit'>";
                         $(document).off("click", "#btnExist" + index);
                         $(document).on("click", "#btnExist" + index, { ID: item.CommunityMemberID, CommunityName: item.Name, Page: page }, Community.Manage.RemoveEvent);
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
                 var temp_remark = $("#divCommunityRemark" + index);
                 var temp_remark_noover = $("#divCommunityRemarkFull" + index);
                 $("#spnShowFullRemark" + index).off("click");
                 if (temp_remark_noover.height() > temp_remark.height() + 1) {//剔除结尾回车情况
                     $("#spnShowFullRemark" + index).show().on("click", { Index: index }, Community.Manage.ShowDetailRemarkEvent);
                 }
             });
         });
}
//展开/收起备注事件
Community.Manage.ShowDetailRemarkEvent = function ShowDetailRemarkEvent(event) {
    var index = event.data.Index;
    if ($(event.target).hasClass("isopen")) {
        $(event.target).removeClass("isopen");
        $("#divCommunityRemark" + index).show();
        $("#divCommunityRemarkFull" + index).hide();
        $(event.target).text("展开");
        $(event.target).attr("title", "查看所有简介");
    } else {
        $(event.target).addClass("isopen");
        $("#divCommunityRemark" + index).hide();
        $("#divCommunityRemarkFull" + index).show();
        $(event.target).text("收起");
        $(event.target).removeAttr("title");
    }
}
//退出圈子事件
Community.Manage.RemoveEvent = function RemoveEvent(event) {
    var community_name = event.data.CommunityName;
    var id = event.data.ID;
    var page = event.data.Page;
    $.Confirm({ content: "您确定退出" + community_name + "行业圈子么？" ,width:"auto"}, function () {
        $.SimpleAjaxPost("service/CommunityService.asmx/RemoveMember", true, "{memberID:'" + id + "'}",
           function (json) {
               var result = json.d;
               if (result == true) {
                   $.Alert({content:"退出" + community_name + "行业圈子",width:"auto"}, function () {
                       var keyword = {
                           Keyword: $("#txtKeyword").val()
                       };
                       Community.Manage.Search(keyword, page);
                       AtInfo.GetAtValidationCount();
                       Community.Manage.TotalCount--;
                       if (Community.Manage.TotalCount == 0) {
                           $("#divHasCommunity,#divHasCommunityLabel,#divHasCommunityList").hide();
                       }
                   });
               }
               else {
                   console.log("退出行业圈子失败！");
               }
           });

    });
}
//解散行业圈子事件
Community.Manage.DismissEvent = function DismissEvent(event) {
    var id = event.data.ID;
    var community_name = event.data.CommunityName;
    var page = event.data.Page;
    $.Confirm({ content: "确认是否解散" + community_name + "行业圈子", width: "auto" }, function () {
        $.SimpleAjaxPost("service/CommunityService.asmx/Remove", true, "{communityID:'" + id + "'}",
           function (json) {
               var result = json.d;
               if (result == true) {
                   $.Alert({ content: "解散" + community_name + "行业圈子成功", width: "auto" }, function () {
                       var keyword = {
                           Keyword: $("#txtKeyword").val()
                       };
                       Community.Manage.Search(keyword, page);
                       AtInfo.GetAtValidationCount();
                       PublishRight.MyCommunityBind();
                       PublishRight.CommunityRecommendBind();
                       Community.Manage.TotalCount--;
                       if (Community.Manage.TotalCount== 0) {
                           $("#divHasCommunity,#divHasCommunityLabel,#divHasCommunityList").hide(); 
                       }
                   });
               }
               else {
                   console.log("解散行业圈子失败！");
               }
           });
    });
}

//邀请朋友事件
Community.Manage.JoinMemberEvent = function JoinMemberEvent(event) {
    var community_name = event.data.Name;
    var id = event.data.ID;
    Community.Manage.JoinMyFriendsBind(id);
    $("#spnJoinLeftMove").off("click").on("click", Community.Manage.MoveJoinMemberEvent);
    $("#spnJoinRightMove").off("click").on("click", Community.Manage.MoveMyAddressBookEvent);
    $("#sctJoinMemberCommunity").dialog({
        resizable: false,
        width: 800,
        modal: true,
        title: "好友设置",
        appendTo: ".main-left",
        buttons: {
            "取　消": function () {
                Community.Manage.JoinClear();
                $(this).dialog("close");
            },
            "邀　请": function () {
                if ($("#sltJoinMemberList option").length == 0) {
                    $.Alert("请选择待邀请的用户！");
                }
                else {
                    Community.Manage.Join(id, community_name);
                    $(this).dialog("close");
                }


            }
        }
    });
}
//绑定通讯录用户
Community.Manage.JoinMyFriendsBind = function join_my_friends_bind(id) {
    $.SimpleAjaxPost("service/CommunityService.asmx/GetMyAddressBookList", true,
      "{communityID:'" + id + "'}",
       function (json) {
           var result = $.Deserialize(json.d);
           var temp = "";
           if (result != null) {
               $.each(result, function (index, item) {
                   temp += "<option value='" + item.AddresserID + "'>" + item.AddresserNickName + (item.Remark == null ? "" : "(" + item.Remark + ")") + "</option>";
               });
           }
           $("#sltMyAddressBookList").empty().append(temp);
       });
}
//→移
Community.Manage.MoveJoinMemberEvent = function MoveAdminMemberEvent(event) {
    if ($("#sltMyAddressBookList option:selected").length == 0) {
        $.Alert("请选择待移动对象");
    }
    else {
        var temp = "";
        $.each($("#sltMyAddressBookList option:selected"), function (index, item) {
            temp += "<option value='" + $(item).val() + "'>" + $(item).text() + "</option>";
        });
        $("#sltMyAddressBookList option:selected").remove();
        $("#sltJoinMemberList").append(temp);
    }
}
//←移
Community.Manage.MoveMyAddressBookEvent = function MoveMyAddressBookEvent(event) {
    if ($("#sltJoinMemberList option:selected").length == 0) {
        $.Alert("请选择待移动对象");
    }
    else {
        var temp = "";
        $.each($("#sltJoinMemberList option:selected"), function (index, item) {
            temp += "<option value='" + $(item).val() + "'>" + $(item).text() + "</option>";

        });

        $("#sltJoinMemberList option:selected").remove();
        $("#sltMyAddressBookList").append(temp);
    }
}
//申请邀请加入圈子
Community.Manage.Join = function join(community_id, community_name) {
    var members = new Array();
    $.each($("#sltJoinMemberList option"), function (index, item) {
        members.push({
            ID: $.NewGuid(),
            CommunityID: community_id,
            MemberID: $(item).val(),
            MemberName: $(item).text().replace(/\([^\)]*\)/g, ""),
            Remark: $("#txtJoinRemark").val() == "" ? "邀请您加入" + community_name + "行业圈子" : $("#txtJoinRemark").val()
        });
    });
    $.SimpleAjaxPost("service/CommunityService.asmx/JoinMembers", true,
        "{memberApplications:" + $.Serialize(members) + "}",
      function (json) {
          var result = json.d;
          if (result == true) {
              $.Alert({
                  content: "您的邀请已发出，请耐心等待...",
                  width:"auto"
              }, function () {
                  Community.Manage.JoinClear();
              });
          }
          else {
              console.log("邀请发送失败！");
          }
      });
}
Community.Manage.JoinClear = function join_clear() {
    $("#txtJoinRemark").val("");
    $("#sltJoinMemberList,#sltMyAddressBookList").empty();
}
//移除圈内人员事件
Community.Manage.RemoveMemberEvent = function RemoveMemberEvent(event) {
    var community_id = event.data.ID;
    var page = event.data.Page;
    var is_creater = event.data.IsCreater;
    Community.Manage.RemoveMemberBind(community_id, is_creater);
    var member_ids = new Array();
    $("#sctRemoveMemberCommunity").dialog({
        resizable: false,
        width: 400,
        modal: true,
        title: "好友移除",
        appendTo: ".main-left",
        buttons: {
            "取　消": function () {
                $("#sltRemoveMemberList").empty();
                $(this).dialog("close");
            },
            "移　除": function () {
                $.Confirm({ content: "您确定要移除行业圈子成员么？", width: "auto" }, function () {
                    $.each($("#sltRemoveMemberList option:selected"), function (index, item) {
                        member_ids.push($(item).val());
                    });
                    if (member_ids.length > 0) {
                        Community.Manage.RemoveMembers(community_id, member_ids, page);
                        $("#sltRemoveMemberList").empty();
                        $("#sctRemoveMemberCommunity").dialog("close");
                    }
                    else {
                        $.Alert("请选择待移除用户对象！");
                    }
                });
            }
        }
    });

}
//移除用户
Community.Manage.RemoveMembers = function remove_members(community_id, member_ids, page) {
    $.SimpleAjaxPost("service/CommunityService.asmx/RemoveMembers", true,
        "{communityID:'" + community_id + "',memberIDs:" + $.Serialize(member_ids) + "}",
       function (json) {
           var result = json.d;
           if (result == true) {
               $.Alert("移除圈内用户成功！", function () {
                   var keyword = {
                       Keyword: $("#txtKeyword").val()
                   };
                   Community.Manage.Search(keyword, page);
               });
           }
           else {
               console.log("移除圈内用户失败！");
           }
       });
}
//待移除用户绑定
Community.Manage.RemoveMemberBind = function remove_member_bind(id, is_creater) {
    $.SimpleAjaxPost("service/CommunityService.asmx/GetMemberInfoList", true, "{communityID:'" + id + "'}",
           function (json) {
               var result = $.Deserialize(json.d);
               var temp = "";
               if (result != null) {
                   $.each(result, function (index, item) {
                       if (is_creater == false) {
                           //如果为管理员，只能删除非管理员成员
                           if (item.IsAdmin == Enum.YesNo.No.toString()) {
                               temp += "<option value='" + item.ID + "'>" + item.Remark + "</option>";
                           }
                       }
                       else {
                           //如果是创建者，可删除出自己之外的成员
                           if (item.MemberID != objPub.UserID) {
                               temp += "<option value='" + item.ID + "'>" + item.Remark + "</option>";
                           }

                       }
                   });
               }
               $("#sltRemoveMemberList").empty().append(temp);
           });
}
//管理成员事件
Community.Manage.ManageMemberEvent = function ManageMemberEvent(event) {
    var id = event.data.ID;
    var page = event.data.Page;
    Community.Manage.GetCommunityInfo(id).done(function (json) {
        var result = json.d;
        $("#txtManageCommunityName").val(result.Name);
        $("#txtManageCommunityRemark").val(result.Remark);
        $("#imgManageCommunityPhoto").attr("src", result.LogoUrl);
        if (result.CanSearch == Enum.YesNo.Yes.toString()) {
            $("#ckCanSearch").prop("checked", true);
        }
        else {
            $("#ckCanSearch").prop("checked", false);
        }
       
        Community.Manage.NewPhotoUrl = result.LogoUrl;
        Community.Manage.OldPhotoUrl = result.LogoUrl;
    });
    Community.Manage.CommunityMemberBind(id);
    $("#spnManageLeftMove").off("click").on("click", Community.Manage.MoveAdminMemberEvent);
    $("#spnManageRightMove").off("click").on("click", Community.Manage.MoveMyMemberEvent);
    //上传头像
    $("#btnUploadManageCommunityImage").off("click").on("click", function (event) {
        $("#fileManageCommunityPhoto").trigger("click");
    });
    //上传头像
    $("#fileManageCommunityPhoto").off("change").on("change", { ID: "fmManageCommuntiyPhoto", ImageID: "imgManageCommunityPhoto" }, Community.Manage.UploadEvent);
    //打开管理圈窗口
    $("#sctManageCommunity").dialog({
        resizable: false,
        width: 650,
        modal: true,
        title: "管理圈设置",
        appendTo: ".main-left",
        buttons: {
            "取　消": function () {
                Community.Manage.ManageClear();
                $(this).dialog("close");
            },
            "设　置": function () {
                Community.Manage.Update(id, page);
                Community.Manage.ManageClear();
                $(this).dialog("close");
            }
        }
    });
}
//行业圈子人员绑定
Community.Manage.CommunityMemberBind = function community_member_bind(id) {
    $.SimpleAjaxPost("service/CommunityService.asmx/GetMemberInfoList", true, "{communityID:'" + id + "'}",
        function (json) {
            var result = $.Deserialize(json.d);
            var temp_member = "";
            var temp_admin = "";
            if (result != null) {
                $.each(result, function (index, item) {
                    if (item.IsAdmin == Enum.YesNo.Yes.toString()) {
                        temp_admin += "<option value='" + item.ID + "'>" + item.MemberName + "</option>";
                    }
                    else {
                        temp_member += "<option value='" + item.ID + "'>" + item.MemberName + "</option>";
                    }
                });
            }
            $("#sltMyMemberList").empty().append(temp_member);
            $("#sltAdminMemberList").empty().append(temp_admin);
        });
}
//→移
Community.Manage.MoveAdminMemberEvent = function MoveAdminMemberEvent(event) {
    if ($("#sltMyMemberList option:selected").length == 0) {
        $.Alert("请选择待移动对象");
    }
    else {
        var temp = "";
        $.each($("#sltMyMemberList option:selected"), function (index, item) {
            temp += "<option value='" + $(item).val() + "'>" + $(item).text() + "</option>";
        });
        $("#sltMyMemberList option:selected").remove();
        $("#sltAdminMemberList").append(temp);
    }
}
//←移
Community.Manage.MoveMyMemberEvent = function MoveMyMemberEvent(event) {
    if ($("#sltAdminMemberList option:selected").length == 0) {
        $.Alert("请选择待移动对象");
    }
    else {
        var temp = "";
        $.each($("#sltAdminMemberList option:selected"), function (index, item) {
            if ($(item).val() == objPub.UserID) {
                $.Alert("禁止挪到行业圈子创建者为普通成员！", function () {
                    temp = "";
                    $("#sltAdminMemberList option").prop("selected", false);
                });

                return false;
            } else {
                temp += "<option value='" + $(item).val() + "'>" + $(item).text() + "</option>";
            }
        });
        if (temp != "") {
            $("#sltAdminMemberList option:selected").remove();
        }
        $("#sltMyMemberList").append(temp);
    }
}
//获取行业圈子基本信息
Community.Manage.GetCommunityInfo = function get_community_info(id) {
    return $.SimpleAjaxPost("service/CommunityService.asmx/GetCommunityInfo", true, "{id:'" + id + "'}");
}
//修改行业圈子信息
Community.Manage.Update = function update(id, page) {
    var community_info = {
        ID: id,
        Name: $("#txtManageCommunityName").val(),
        Remark: $("#txtManageCommunityRemark").val(),
        LogoUrl: Community.Manage.NewPhotoUrl,
        CanSearch: $("#ckCanSearch").prop("checked")==true?Enum.YesNo.Yes.toString():Enum.YesNo.No.toString()
    };
    var members = new Array();
    $.each($("#sltMyMemberList option"), function (index, item) {
        members.push({
            ID: $(item).val(),
            IsAdmin: Enum.YesNo.No
        });
    });
    $.each($("#sltAdminMemberList option"), function (index, item) {
        members.push({
            ID: $(item).val(),
            IsAdmin: Enum.YesNo.Yes
        });
    });
    $.SimpleAjaxPost("service/CommunityService.asmx/Update", true,
        "{communityInfo:" + $.Serialize(community_info) + ",member:" + $.Serialize(members) + "}",
        function (json) {
            var result = json.d;
            if (result == true) {
                $.Alert("修改行业圈子信息成功！", function () {
                    var keyword = {
                        Keyword: $("#txtKeyword").val()
                    };
                    Community.Manage.SearchBind(keyword, page);
                    PublishRight.MyCommunityBind();
                    PublishRight.CommunityRecommendBind();
                });
            }
            else {
                console.log("修改行业圈子信息失败！");
            }
        });
}
Community.Manage.ManageClear = function manage_clear() {
    $("#txtManageCommunityName,#txtManageCommunityRemark").val("");
    $("#imgManageCommunityPhoto").attr("src", Community.Manage.DefaultPhtoUrl);
    Community.Manage.OldPhotoUrl = Community.Manage.DefaultPhtoUrl;
    $("#sltMyMemberList option,#sltAdminMemberList option").remove();
}
//退出行业圈子
Community.Manage.RemoveCommunityEvent = function RemoveCommunityEvent(event) {
    var community_id = event.data.ID;
    $.Confirm("您确定退出该行业圈子么？", function () {
        $.SimpleAjaxPost("service/CommunityService.asmx/Quit", true, "{communityID:'" + community_id + "'}",
            function (json) {
                var result = json.d;
                if (result == true) {
                    $.Alert("退出行业圈子成功！", function () {
                        objPub.InitLeftMain(true);
                        PublishRight.MyCommunityBind();
                        Community.Manage.TotalCount--;
                        if (Community.Manage.TotalCount == 0) {
                            $("#divHasCommunity,#divHasCommunityLabel,#divHasCommunityList").hide();
                        }
                    });
                    
                }
                else {
                    console.log("退出行业圈子失败！");
                }
            });
    });


}

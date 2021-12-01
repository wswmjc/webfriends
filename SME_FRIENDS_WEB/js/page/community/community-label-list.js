Community.Label = function () { }
Community.Label.registerClass("Community.Label");
Community.Label.PageSize = 12;
Community.Label.IsCreater = false;
Community.Label.IsAdmin = false;
///切换入口枚举
Community.Label.TabChangeWay = function () {
    throw Error.notImplemented();
}

Community.Label.TabChangeWay.prototype = {
    //点击群组层层进入详细页
    FromGroup: 0,
    //点击标签直接进入详细页
    FromLabel: 1

};
Community.Label.TabChangeWay.registerEnum("Community.Label.TabChangeWay");
Community.Label.Init = function init(community_id) {
    objPub.IsMain = false;
    //返回事件
    $("#aGoBack").off("click").on("click", Community.Manage.BackEvent);
    var page = { pageStart: 1, pageEnd: Community.Label.PageSize * 1 };
    var keyword = {
        Keyword: "",
        CommunityID: community_id
    };
    $.when(Community.IsCreater(community_id, objPub.UserID), Community.IsAdmin(community_id, objPub.UserID))
        .done(function (is_creater, is_admin) {
            //是否为创建者逻辑部分
            Community.Label.IsCreater = is_creater[0].d;
            //话题-讨论Tab切换事件
            $("#ulSubType>li").off("click").on("click",
                {
                    CommunityID: community_id,
                    Creater: Community.Label.IsCreater == true ? objPub.UserID : "",
                    TabChangeWay: Community.Label.TabChangeWay.FromLabel
                },
                Community.LabelTopicTabChangeEvent);
            //退出圈子
            if (Community.Label.IsCreater == true) {
                $("#spnRemoveCommunity").hide();
            }
            else {
                $("#spnRemoveCommunity").show().on("click", { ID: community_id }, Community.Manage.RemoveCommunityEvent);
            }
            //是否为管理者逻辑部分
            Community.Label.IsAdmin = is_admin[0].d;
            if (Community.Label.IsAdmin == true) {
                $("#spnAddLabel,#spnDeleteLabel").show();
                //添加标签
                $("#spnAddLabel").off("click").on("click", { ID: community_id, Page: page }, Community.Label.AddEvent);
                //删除标签
                $("#spnDeleteLabel").off("click").on("click", { ID: community_id, Page: page }, Community.Label.RemoveEvent);
            }
            else {
                $("#spnAddLabel,#spnDeleteLabel").hide();
            }
            Community.Label.Search(keyword, page);
        });
   
    $.SimpleAjaxPost("service/CommunityService.asmx/GetTopicSearchCount", true,
    "{searchView:" + $.Serialize(keyword) + "}",
     function (json) {
         var result = json.d;
         $("#bTopicCount").text(result);
     });
}
//去标签详细页事件
Community.Label.GoDetailEvent = function GoDetailEvent(event) {
    var label_id = event.data.ID;
    var label_name = event.data.Name;
    var community = event.data.Community;
    var sub_type = event.data.Type;
    var label_ids = new Array();
    if (event.data.Labels) {
        for (var index in event.data.Labels) {
            label_ids.push(event.data.Labels[index].LabelID);
        }
    }
    
    //加载行业圈子详细页
    $(".main-left").ReadTemplate(Template.DetailCommunityTpl, function () {
        //显示时间轴
        $("#divCommunityTimeAxis").show();
        //读取行业圈子名称
        $("#divCommunityName").text(community.Name);
        //读取清空操作按钮
        $("#spnAddLabel,#spnDeleteLabel,#spnRemoveCommunity").remove();

        //返回事件
        $("#aGoBack").off("click").on("click", Community.Label.BackEvent);
        $("#bLabelCount").text(community.LabelCount);
        $("#bTopicCount").text(community.TopicCount);

        $("#ulSubType>li").off("click").on("click",
            {
            CommunityID: community.ID,
            TabChangeWay:Community.Label.TabChangeWay.FromGroup,
            LabelIDs:label_ids,
            Creater:community.Manager
        },
        Community.LabelTopicTabChangeEvent);
    });

    if (sub_type == Enum.CommunitySubType.Subject) {//查看所有话题
        $("#ulSubType>li.circle-subject").trigger("click");
    } else if (sub_type == Enum.CommunitySubType.Topic) {//查看所有讨论
        $("#ulSubType>li.circle-topic").trigger("click");
    } else if (!sub_type) {//查看具体的话题-标签
        Community.Label.GetReadLabels(label_ids, label_id);
    }
}
//添加标签事件
Community.Label.AddEvent = function AddEvent(event) {
    var page = event.data.Page;
    var community_id=event.data.ID;
    $("#sctAddLabel").dialog({
        resizable: false,
        width: 500,
        modal: true,
        title: "新增话题",
        buttons: {
            "取　消": function () {
                Community.Label.Clear();
                $(this).dialog("close");
            },
            "提　交": function () {
                Community.Label.Create(community_id, page);
                Community.Label.Clear();
                $(this).dialog("close");
            }
        }
    });
}
//清空新增标签
Community.Label.Clear = function clear_label() {
    $("#txtAddLabel").val("");
}
//新增标签
Community.Label.Create = function create_label(community_id,page) {
    var labelName = $("#txtAddLabel").val();
    //console.log($("#txtAddLabel"));
    //console.log(labelName);
    var label = {
        ID: $.NewGuid(),
        CommunityID: community_id,
        LabelName: labelName
    };
    $.SimpleAjaxPost("service/CommunityService.asmx/AddLabel", true,
     "{labelInfo:" + $.Serialize(label) + "}",
      function (json) {
          var result = json.d;
          if (result == true) {
              $.Alert("新增话题成功！", function () {
                  var keyword = {
                      Keyword: "",
                      CommunityID: community_id
                  };
                  Community.Label.Search(keyword, page);
              });
          }
          else {
              console.log("新增话题失败！");
          }
      });
}
//删除标签事件
Community.Label.RemoveEvent = function RemoveEvent(event) {
    var id = "";
    var page = event.data.Page;
    var community_id = event.data.ID;
    $.each($("div[id^='divLabel']"), function (index, item) {
        if ($(item).hasClass("selected") == true) {
            id = $(item).data("ID");
            return false;
        }
    });
    if (id == "") {
        $.Alert("请选中待删除话题！");
        return;
    }
    else {
        $.SimpleAjaxPost("service/CommunityService.asmx/RemoveLabel", true,
         "{id:'" + id + "'}",
          function (json) {
              var result = json.d;
              if (result == true) {
                  $.Alert("移除话题成功！", function () {
                      $("#ulSubType>li.circle-subject").trigger("click");
                  });
              }
              else {
                  console.log("移除话题失败！");
              }
          });
    }
}
Community.Label.Search = function search(keyword, page) {
    Community.Label.SearchBind(keyword, page);
    $.SimpleAjaxPost("service/CommunityService.asmx/GetLabelSearchCount", true,
      "{keywordView:" + $.Serialize(keyword) + "}",
       function (json) {
           var result = json.d;
           $("#bLabelCount").text(result);
           if (result <= Community.Label.PageSize) {
               $("#wPaginate8nPosition").wPaginate("destroy");
           }
           else {
               $("#wPaginate8nPosition").wPaginate("destroy").wPaginate({
                   theme: "grey",
                   first: "首页",
                   last: "尾页",
                   total: result,
                   index: 0,
                   limit: Community.Label.PageSize,
                   ajax: true,
                   url: function (i) {
                       var page = {
                           pageStart: i * this.settings.limit + 1,
                           pageEnd: (i + 1) * this.settings.limit
                       };
                       Community.Label.SearchBind(keyword, page);
                   }
               });
           }
       });
}
Community.Label.SearchBind = function search_bind(keyword, page) {
    $.SimpleAjaxPost("service/CommunityService.asmx/LabelSearch", true,
        "{keywordView:" + $.Serialize(keyword) + ",page:" + $.Serialize(page) + "}",
      function (json) {
          var result = $.Deserialize(json.d);
          var temp = "";
          if (result != null) {
              //写数量
              //$("#bLabelCount").text(result.length);
              $.each(result, function (index, item) {
                  temp += "<div id='divLabel" + index + "' class='topic-block'" + (Community.Label.IsCreater == true ? "" : " style='cursor:default;'") + ">";
                  $(document).off("click", "#divLabel" + index);
                  $(document).on("click", "#divLabel" + index, function (event) {
                      if ($(this).hasClass("selected")) {
                          $(this).removeClass("selected");
                      } else {
                          $(this).addClass("selected").siblings().removeClass("selected");
                      }
                  });
                  temp += "<a class='topic-title' href='javascript:void(0);' id='aDetailLabel" + index + "'>" + item.Name + "</a>";
                  var label_event_info = {
                      ID: item.ID,
                      Name: item.Name,
                      CreaterID: item.CreaterID,
                      CreaterName: item.CreaterName,
                      CreateTime: item.CreateTime,
                      PublishCount: item.PublishCount
                  };
                  $(document).off("click", "#aDetailLabel" + index);
                  $(document).on("click", "#aDetailLabel" + index, { CommunityID: item.CommunityID, Label: label_event_info }, function (event) {
                      $("#wPaginate8nPosition").wPaginate("destroy");
                      Community.PublishInfo.Init(event.data.CommunityID, event.data.Label, false);
                      event.stopPropagation();
                  });
                  temp += "<div class='topic-info'>";
                  temp += "<span>话题反馈</span>";
                  temp += "<span class='discuss-num'>" + item.PublishCount + "</span>";
                  temp += "</div>";
                  temp += "<hr>";
                  if (item.UserList != null) {
                      temp += "<div class='topic-user'>";
                      $.each(item.UserList, function (sub_index, sub_item) {
                          temp += "<div class='circle-photo'><img src='" + sub_item.UserUrl + "' title='" + sub_item.UserName + "'/></div>";
                      });
                      temp += "</div>";
                  }
                  temp += "</div>";
              });
          }
          else {
              temp += "  <div class='friend-list-empty'><span>暂没有行业圈子话题哦~</span></div>";
          }
          $("#sctCommunityContent").empty().append(temp);
          if (result != null) {
              $.each(result, function (index, item) {
                  $("#divLabel" + index).data("ID", item.ID);
              });
          }
      });
}

Community.Label.BackEvent = function BackEvent(event) {
    
    $(".main-left").load("../biz/left/moments.html",  function (response, status) {
        if (status == "success") {
            objPub.IsMain = true;
            Moments.Init(true);
            Moments.FriendsTabChangeEvent.apply($("a[name='listCircle']"));
        }
    });
    return false;
}

Community.Label.GetReadLabels = function get_read_labels(label_ids, init_label_id) {
    if (label_ids.length != 0) {
        $.SimpleAjaxPost("service/CommunityService.asmx/GetLabelListWithIDs", true,
            "{ids:" + $.Serialize(label_ids) + "}",
          function (json) {
              var result = $.Deserialize(json.d);
              var temp = "";
              if (Array.isArray(result)) {
                  $.each(result, function (index, item) {
                      temp += "<div id='divReadLabel" + index + "' class='topic-block'>";
                      $(document).off("click", "#divReadLabel" + index);
                      $(document).on("click", "#divReadLabel" + index, function (event) {
                          if ($(this).hasClass("selected")) {
                              $(this).removeClass("selected");
                          } else {
                              $(this).addClass("selected").siblings().removeClass("selected");
                          }
                      });
                      temp += "<a class='topic-title' href='javascript:void(0);' id='aReadLabel" + index + "'>" + item.Name + "</a>";
                      var label_event_info = {
                          ID: item.ID,
                          Name: item.Name,
                          CreaterID: item.CreaterID,
                          CreaterName: item.CreaterName,
                          CreateTime: item.CreateTime,
                          PublishCount: item.PublishCount
                      };
                      $(document).off("click", "#aReadLabel" + index);
                      $(document).on("click", "#aReadLabel" + index, { CommunityID: item.CommunityID, Label: label_event_info }, function (event) {
                          if (init_label_id) {
                              Community.PublishInfo.Init(event.data.CommunityID, event.data.Label, true);
                          } else {
                              Community.PublishInfo.Init(event.data.CommunityID, event.data.Label, false);
                          }
                          event.stopPropagation();
                      });
                      temp += "<div class='topic-info'>";
                      temp += "<span>话题反馈</span>";
                      temp += "<span class='discuss-num'>" + item.PublishCount + "</span>";
                      temp += "</div>";
                      temp += "<hr>";
                      if (item.UserList != null) {
                          temp += "<div class='topic-user'>";
                          $.each(item.UserList, function (sub_index, sub_item) {
                              temp += "<div class='circle-photo'><img src='" + sub_item.UserUrl + "' title='" + sub_item.UserName + "'/></div>";
                          });
                          temp += "</div>";
                      }
                      temp += "</div>";
                  });
              }
              else {
                  temp += "<div class='friend-list-empty'><span>暂没有行业圈子话题哦~</span></div>";
              }
              $("#sctCommunityContent").empty().append(temp);
              if (init_label_id) {
                  if (Array.isArray(result)) {
                      $.each(result, function (index, item) {
                          if (item.ID == init_label_id) {
                              $("#aReadLabel" + index).trigger("click");
                          }
                      });
                  }
              }
          });
    } else {
        $("#sctCommunityContent").empty().append("<div class='friend-list-empty'><span>暂没有行业圈子话题哦~</span></div>");
    }
}
Community = function () { }
Community.registerClass("Community");
Community.Init = function init(init_topic_community_id) {
    $("#divTimeAxis").hide();
    var keyword = {
        Keyword: ""
    };
    $("#divTabList").empty();
    var page = { pageStart: 1, pageEnd: Community.Trends.PageSize * 1 };
    Community.Trends.Search(keyword, page, init_topic_community_id);
}

//返回主页面
Community.BackEvent = function BackEvent(event) {
    objPub.InitLeftMain(true);
}
//是否为管理员
Community.IsAdmin = function is_admin(community_id,user_id) {
   return $.SimpleAjaxPost("service/CommunityService.asmx/IsAdmin", true,
      "{userID:'" + user_id + "',communityID:'"+community_id+"'}");
}
//是否为创建人
Community.IsCreater = function is_creater(community_id, user_id) {
    return $.SimpleAjaxPost("service/CommunityService.asmx/IsCreater", true,
       "{userID:'" + user_id + "',communityID:'" + community_id + "'}");
}
//行业圈子Tab话题、讨论切换事件
Community.LabelTopicTabChangeEvent = function LabelTopicTabChangeEvent(event) {
    var tab_change_way = event.data.TabChangeWay;
    
    if (!$(this).hasClass("selected")) {
        $(this).addClass("selected");
        $(this).siblings().removeClass("selected");
    }
    var community_id = event.data.CommunityID;
    var community_creater = event.data.Creater;
    //清空时间轴
    $("#divCommunityTimeAxis").hide();
    //清空分页
    $("#wPaginate8nPosition").wPaginate("destroy");
    //alert(tab_change_way);
    if (tab_change_way == Community.Label.TabChangeWay.FromLabel) {
        if ($(this).hasClass("circle-subject") == true) {
            //如果切换至话题列表页
            if (Community.Label.IsAdmin == true) {
                $("#spnDeleteLabel,#spnAddLabel").show();
            }
            Community.Label.Search({
                Keyword: "",
                CommunityID: community_id
            }, { pageStart: 1, pageEnd: Community.Label.PageSize * 1 });
        } else if ($(this).hasClass("circle-topic") == true) {
            //如果切换至讨论列表页
            if (Community.Label.IsAdmin == true) {
                $("#spnDeleteLabel,#spnAddLabel").hide();
            }
            $("#sctCommunityContent").ReadTemplate(Template.CommunityTopicListTpl, function () {
                Community.Topic.Init(community_id,community_creater);
            });
        }
    }
    else if (tab_change_way == Community.Label.TabChangeWay.FromGroup) {
        var label_ids = event.data.LabelIDs; 
        if ($(this).hasClass("circle-subject") == true) {
            //如果切换至话题列表页
            if (label_ids.length != 0) {
                Community.Label.GetReadLabels(label_ids);
            } else {
                $("#sctCommunityContent").empty().append("  <div class='friend-list-empty'><span>暂没有行业圈子话题哦~</span></div>");
            } 
        } else if ($(this).hasClass("circle-topic") == true) {
            //如果切换至讨论列表页
            $("#sctCommunityContent").ReadTemplate(Template.CommunityTopicListTpl, function () {
                Community.Topic.Init(community_id, community_creater);
            });
        }
    }
   
}
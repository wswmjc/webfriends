Group = function () { }
Group.registerClass("Group");
Group.Init = function init() {
    $("#divTimeAxis").hide();
    var keyword = {
        Keyword: ""
    };
    $("#divTabList").empty();
    var page = { pageStart: 1, pageEnd: Group.Trends.PageSize * 1 };
    Group.Trends.Search(keyword, page); 
}
//查看讨论组信息事件
Group.ShowGroupEvent = function ShowGroupEvent(event) {
    var group_id = event.data.ID;
    var group_name = event.data.Name;
    var group_manager = event.data.Manager;
    $(".main-left").load("../biz/left/group/detail-list.html", function (response, status) {
        if (status == "success") {
            $("#divGroupName").html(group_name);
            Group.List.Init(group_id, group_manager);
        }
    });
}
// 返回主页面
Group.BackEvent = function BackEvent(event) {
    $(".main-left").load("../biz/left/moments.html", function (response, status) {
        if (status == "success") {
            objPub.IsMain = true;
            Moments.Init(true); 
            Moments.FriendsTabChangeEvent.apply($("a[name='listGroup']"));
        }
    });
}
//是否为讨论组创建者
Group.IsCreater = function is_creater(group_id, user_id) {
    return $.SimpleAjaxPost("service/GroupService.asmx/IsCreater", true,
     "{userID:'" + user_id + "',groupID:'" + group_id + "'}");
}
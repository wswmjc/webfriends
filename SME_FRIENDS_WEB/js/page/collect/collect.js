Collect = function () { }
Collect.registerClass("Collect");
Collect.PageSize = 10;

Collect.Init = function init() {
    $(".friend-content").ReadTemplate(Template.CollectListTpl, function () {
        //隐藏时间轴
        $("#divTimeAxis").hide();
        Collect.SetTypeTab();
        Collect.Moments.Init();
        $("#aGoBack").off("click").on("click", Collect.GoBackEvent);
    });
}

Collect.SetTypeTab = function set_type_tab() {
    $("#ulCollectType>li").off("click").on("click", function (event) {
        $(this).addClass("selected").siblings().removeClass("selected");
        if ($(this).attr("name") == "moments") {
            Collect.Moments.Init();
        } else if ($(this).attr("name") == "community") {
            Collect.Community.Init();
        }
    });
}

Collect.GoBackEvent = function GoBackEvent(event) {
    $(".main-left").load("../biz/left/moments.html", function () {
        objPub.IsMain = true;
        Moments.Init(true);
        Moments.FriendsTabChangeEvent.apply($("a[name='listFriend']"));
    });
}
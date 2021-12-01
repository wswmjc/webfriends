Notice.Group = function () { }
Notice.Group.registerClass("Notice.Group");
Notice.Group.GetNoticeEvent = function GetNoticeEvent(event) {
    $(event.target).addClass("selected").siblings().removeClass("selected");
}
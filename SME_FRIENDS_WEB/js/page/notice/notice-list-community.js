Notice.Community = function () { }
Notice.Community.registerClass("Notice.Community");
Notice.Community.GetNoticeEvent = function GetNoticeEvent(event) {
    $(event.target).addClass("selected").siblings().removeClass("selected");
}
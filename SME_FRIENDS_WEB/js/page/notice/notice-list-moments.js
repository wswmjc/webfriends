Notice.Moments = function () { }
Notice.Moments.registerClass("Notice.Moments");
Notice.Moments.GetNoticeEvent = function GetNoticeEvent(event) {
    $(event.target).addClass("selected").siblings().removeClass("selected");
}
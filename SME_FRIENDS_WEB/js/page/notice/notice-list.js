Notice = function () { }
Notice.registerClass("Notice");
Notice.Init = function init() {
    $("#liMoments").on("click", Notice.Moments.GetNoticeEvent);
    $("#liGroup").on("click", Notice.Group.GetNoticeEvent);
    $("#liCommunity").on("click", Notice.Community.GetNoticeEvent);
 
}





$(document).ready(function () {
    $(window).on("message", function (event) {
        if (event.originalEvent.origin != "http://" + window.location.host + "/") {
            if (event.originalEvent.data.MessageData.MessageType == "ClearInfo") {
                if (event.originalEvent.data.MessageData.Remember == true) {
                    localStorage.removeItem("remember_social_code");
                    localStorage.removeItem("rsa");
                    localStorage.removeItem("social_code");
                }
                else {
                    localStorage.removeItem("rsa");
                }
                event.originalEvent.source.postMessage({ MessageData: { MessageType: "ResponseInfo" } }, event.originalEvent.origin);
            }
            else if (event.originalEvent.data.MessageData.MessageType == "PostInfo") {
                localStorage.setItem("rsa", event.originalEvent.data.MessageData.rsa);
                localStorage.setItem("social_code", event.originalEvent.data.MessageData.social_code);
            }
        }
    });
});
//用户注册
Singin = function () { }
Singin.registerClass("Singin");
Singin.Timer = null;

//注册页面初始化
Singin.Init = function init() {
    Singin.GC();
    $("#divRegistCodeForm").hide();
    $("#divRegitForm").show();
    //点击二维码登录绑定事件
    $(".login-code").off("click").on("click", Singin.QuickRegistEvent);
    //点击用户登录绑定事件
    $(".login-user").off("click").on("click", Singin.UsualRegistEvent);
    //用户注册输入框绑定事件
    $("#txtUserCode").off("blur").on("blur", Singin.UniqueEvent);
    $("#txtMobile").off("blur").on("blur", Singin.UniqueMobileEvent);
    $("#txtEmail").off("blur").on("blur", Singin.UniqueEmailEvent);
    //注册按钮绑定事件
    $("#btRegist").off("click").on("click", function (event) { 
        Singin.RegistEvent();
    });

     
}
//快速注册
Singin.QuickRegistEvent = function QuickRegistEvent() {
    //初始化
    Singin.GC();
    $(".login-user").show();
    $(".login-code").hide();
    $(".login-user-title").text("二维码注册");
    $("#divRegitForm").hide();
    $("#divRegistCodeForm").show();
    //获得二维码
    var social_user_info = {
        Redirect: objPub.FriendsUrl + "/biz/main.html",
        Platform: Enum.PlatformTypeSetting.MiicFriends.toString()
    };

    var uri = "regist_info=" + encodeURIComponent($.Serialize(social_user_info));
    var url = objPub.ManageUrl + "service/WeixinUserRegist.ashx?" + uri;
    console.log(encodeURI(url));
    objPub.GetOpenAppID().done(function (json) {
        var result = json.d;
        var obj = new WxLogin({
            id: "divWxUsualRegist",
            appid: result,
            scope: "snsapi_login",
            redirect_uri: encodeURI(url),
            state: "",
            style: "",
            href: ""
        });
    });
}
//用户注册
Singin.UsualRegistEvent = function UsualRegistEvent() {
    //初始化
    $(".login-code").show();
    $(this).hide();
    $(".login-user-title").text("用户注册");
    $("#divRegistCodeForm").hide();
    $("#divRegitForm").show();
    Singin.GC();
}
Singin.GC = function GC() {
    $("#txtMobile,#txtUserCode,#txtEmail").val("").blur();
    $("#ckRegistDisclaimer").prop("checked", false);
}
//手机发送验证码
Singin.SmsRequireEvent = function SmsRequireEvent(event) {
    var mobile = $("#txtMobile").val(); 
    if ($.IsNullOrEmpty(mobile) == false && /^(1[35847]\d{9})$/.test(mobile) == true) {
        var url = new Array();
        url.push(objPub.ManageUrl);
        url.push("service/UserService.asmx/SendSMS");
        $.SimpleAjaxCors(url, "POST", "{mobile:'" + mobile + "'}").done(function (json) {
            var result = json.d;
            if (result == true) {
                var leftTime = 60;
                $("#aSmsBtn").addClass("sms-unvalid").off("click");
                Singin.Timer = setInterval(function () {
                    if (leftTime > 0) {
                        $("#aSmsBtn").html("已发送" + leftTime + "S");
                    } else {
                        $("#aSmsBtn").html("短信验证").removeClass("sms-unvalid").off("click").on("click", Singin.SmsRequireEvent);
                        clearInterval(Singin.Timer);
                    }
                    leftTime--;
                }, 1000);
            } else {
                $("#txtMsgCode").testRemind("发送手机验证码失败");
            }
        });
    } else {
        $("#txtMobile").testRemind("请输入正确的手机号码");
    }
}

//用户名验证
Singin.UniqueEvent = function UniqueEvent(event) {
    var $this = $(event.target) || $(this);
    if ($this.val() !== "") {
        var url = new Array();
        url.push(objPub.ManageUrl);
        url.push("service/UserService.asmx/UniqueSocialCode");
        $.SimpleAjaxCors(url, "POST", "{socialCode:'" + $this.val() + "'}").done(
            function (json) {
                var result = json.d;
                if (result !== true) {
                    $this.val("").focus();
                    $this.testRemind("用户名已被注册，请重新输入");
                }
            });
    }
}

//邮箱验证
Singin.UniqueEmailEvent = function UniqueEmailEvent(event) {
    var $this = $(event.target) || $(this);
    if ($this.val() !== "") {
        var url = new Array();
        url.push(objPub.ManageUrl);
        url.push("service/UserService.asmx/UniqueEmail");
        $.SimpleAjaxCors(url, "POST", "{email:'" + $this.val() + "'}").done(
            function (json) {
                var result = json.d;
                if (result !== true) {
                    $this.val("").focus();
                    $this.testRemind("邮箱已被注册，请重新输入");
                }
            });
    }
}

//手机验证
Singin.UniqueMobileEvent = function UniqueMobileEvent(event) {
    var $this = $(event.target) || $(this);
    if ($this.val() !== "") {
        var url = new Array();
        url.push(objPub.ManageUrl);
        url.push("service/UserService.asmx/UniqueMobile");
        $.SimpleAjaxCors(url, "POST", "{mobile:'" + $this.val() + "'}").done(
            function (json) {
                var result = json.d;
                if (result !== true) {
                    $this.val("").focus();
                    $this.testRemind("手机已被注册，请重新输入");
                } else {
                    if ($("#txtUserCode").val() === "") {
                        $("#txtUserCode").val($this.val()).focus();
                    }
                    //激活
                    $("#aSmsBtn").removeClass("sms-unvalid").off("click").on("click", Singin.SmsRequireEvent);
                }
            });
    } else {
        $("#aSmsBtn").addClass("sms-unvalid").off("click");
    }
}
//注册按钮事件
Singin.RegistEvent = function RegistEvent(event) {
    //确认声明内容
    if ($("#ckRegistDisclaimer").prop("checked") == false) {
        $("#ckRegistDisclaimer").testRemind("请确认遵守声明内容！")
        return;
    }

    var mobile = $("#txtMobile").val();
    var mobile_code = $("#txtMsgCode").val();
    var url = new Array();
    url.push(objPub.ManageUrl);
    url.push("service/UserService.asmx/CheckMobileCode");
    $.SimpleAjaxCors(url, "POST", "{mobile:'" + mobile + "',mobileCode:'" + mobile_code + "'}").done(function (json) {
        var result = $.Deserialize(json.d);
        if (result.result == true) {
            $("#sigin-user,#divRegistCodeForm").hide();
            $("#divRegitForm").show();
            var social_user_info = {
                SocialCode: $("#txtUserCode").val(),
                Mobile: $("#txtMobile").val(),
                Email: $("#txtEmail").val(),
                MictalkDirect: window.location.href
            };
            var uri = "regist_info=" + encodeURIComponent($.Serialize(social_user_info));
            var url = objPub.ManageUrl + "service/WeixinUserRegist.ashx?" + uri;

            objPub.GetOpenAppID().done(function (json) {
                var result = json.d;
                $("#divRegitForm").hide();
                $("#divRegistCodeForm").show();
                var obj = new WxLogin({
                    id: "divWxUsualRegist",
                    appid: result,
                    scope: "snsapi_login",
                    redirect_uri: encodeURI(url),
                    state: "",
                    style: "",
                    href: ""
                });

            });
        } else {
            $("#txtMsgCode").testRemind(result.message);
            $("#txtMsgCode").focus();
        }
    });
}


//页面初始化
$(document).ready(function () {
    Singin.Init();
});
//Singin.RegisterEvent = function register_event() {
//    $(window).scrollTop(0);
//    $("main").load("../biz/signin.html", function (response, status) {
//        if (status == "success") {
//            Singin.Init();
//        }
//    });
//}
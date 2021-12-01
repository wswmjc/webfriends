Login = function () { }
Login.registerClass("Login");
Login.Login = function login(login_view) {
    $.SimpleAjaxPost("service/UserService.asmx/GetRSAPublicKey",
             true,
              function (json) {
                  var result = $.Deserialize(json.d);
                  if (result.publickeyexponent != "") {
                      pExponent = result.publickeyexponent;
                  }
                  if (result.publickeymodulus != "") {
                      pModulus = result.publickeymodulus;
                  }
                  setMaxDigits(129);
                  var key = new RSAKeyPair(pExponent, "", pModulus);
                  login_view.RSAPassword = encryptedString(key, login_view.RSAPassword);

              }).done(function () {
                  $.SimpleAjaxPost("service/UserService.asmx/Login",
                         true,
                        "{loginView:" + $.Serialize(login_view) + "}",
                         function (json) {
                             var result = json.d;
                             if (result.IsLogin == true) {

                                 if (localStorage.getItem("social_code") == undefined
                                     || ($("#txtUserCode").val() != "" && localStorage.getItem("social_code") != undefined)) {
                                     localStorage.setItem("social_code", $("#txtUserCode").val());
                                 }
                                 if (localStorage.getItem("rsa") == undefined) {
                                     localStorage.setItem("rsa", $.md5($.md5($("#txtPassword").val())));
                                 }
                                 //设置全局变量
                                 objPub.SetObjPub();
                                 window.location.href = "/biz/main.html";
                             }
                             else {
                                 if (result.CheckAdmin == false) {
                                     $("#txtUserCode").addClass("error-input");
                                     $("#txtUserCodeTip").html("非管理人员无法登录本系统").show();
                                 }
                                 else if (result.CheckUserCode == false) {
                                     $("#txtUserCode").addClass("error-input");
                                     $("#txtUserCodeTip").html("账号错误,请重新输入").show();
                                 }
                                 else if (result.CheckPassword == false) {
                                     $("#txtPassword").addClass("error-input");
                                     $("#txtPasswordTip").html("密码错误,请重新输入").show();
                                 } else if (result.CheckValid == false) {
                                     $("#txtUserCode,#txtPassword").addClass("error-input");
                                     $("#txtUserCodeTip").html("用户失效,请联系管理员").show();
                                 } else if (result.CheckDisabled == true) {
                                     $("#txtLoginUserCode,#txtLoginPassword").addClass("error-input");
                                     $("#txtLoginUserCodeTip").html("该用户已被禁用,请联系管理员").show();
                                 } else if (result.CheckActivated == false) {
                                     $("#txtLoginUserCode,#txtLoginPassword").addClass("error-input");
                                     $("#txtLoginUserCodeTip").html("该用户尚未激活,请联系管理员").show();
                                 }
                             }
                         });
              });
}

Login.LoginEvent = function LoginEvent(event) {
    if ($("#txtUserCode").val() != ""
       && $("#txtPassword").val() != "") {
        var login_view = {
            SocialCode: $("#txtUserCode").val(),
            RSAPassword: $.md5($.md5($("#txtPassword").val()))
        };
        Login.Login(login_view);
    } else {
        if ($("#txtUserCode").val() == ""
       && $("#txtPassword").val() == "") {
            Login.IsNotNull("txtUserCode");
            Login.IsNotNull("txtPassword");
        } else if ($("#txtUserCode").val() == "") {
            Login.IsNotNull("txtUserCode");
        } else if ($("#txtPassword").val() == "") {
            Login.IsNotNull("txtPassword");
        }
    }
    return false;
}
Login.LoginKeyPressEvent = function LoginKeyPressEvent(event) {
    if (event.which == 13) {
        if ($("#txtUserCode").val() != ""
             && $("#txtPassword").val() != "") {
            var login_view = {
                SocialCode: $("#txtUserCode").val(),
                RSAPassword: $.md5($.md5($("#txtPassword").val()))
            };
            Login.Login(login_view);
        } else {
            if ($("#txtUserCode").val() == "" && $("#txtPassword").val() == "") {
                Login.IsNotNull("txtUserCode");
                Login.IsNotNull("txtPassword");
            } else if ($("#txtUserCode").val() == "") {
                Login.IsNotNull("txtUserCode");
            } else if ($("#txtPassword").val() == "") {
                Login.IsNotNull("txtPassword");
            }
        }
    }
}
Login.RememberMeEvent = function RememberMeEvent(event) {
    if ($("#ckRememberMe").is(":checked") == true) {
        $("#ckRememberMe").attr("checked", true);
        localStorage.setItem("remember_social_code", Enum.YesNo.Yes.toString());
    }
    else {
        $("#ckRememberMe").attr("checked", false);
        if (localStorage.getItem("social_code") != undefined) {
            localStorage.removeItem("social_code");
        }
        if (localStorage.getItem("rsa") != undefined) {
            localStorage.removeItem("rsa");
        }
        localStorage.setItem("remember_social_code", Enum.YesNo.No.toString());
    }
}
Login.FindPassword = function find_password() {
    if (Login.IsNotNull("txtEmail")) {
        $.SimpleAjaxPost("service/UserService.asmx/FindPasswordByEmail",
              true,
             "{myEmail:'" + $("#txtEmail").val() + "'}",
              function (json) {
                  if (json.d == true) {
                      $.Alert({ width: "auto", content: "您的密码已找回，请登录邮箱进行查询！" }, function (event) {
                          $("#txtEmailTip").html("");
                          $("#txtUserCode,#txtPassword").removeClass("error-input");
                          $("#sctLoginForm").show();
                          $("#sctPasswordForm").hide();
                      });
                  }
                  else {
                      $("#txtEmail").addClass("error-input").val("");
                      $("#txtEmailTip").html("找回失败，请检查邮箱").show();
                  }
              });
    }
}
Login.GoEvent = function GoEvent(event) {
    $("#sctLoginForm").hide();
    $("#sctPasswordForm").show();
}

Login.BackEvent = function BackEvent(event) {
    $("#sctLoginForm").show();
    $("#sctPasswordForm").hide();
}
Login.FindPasswordEvent = function FindPasswordEvent(event) {
    Login.FindPassword();
}

Login.FindPasswordKeyPressEvent = function FindPasswordKeyPressEvent(event) {
    if (event.which == 13) {
        Login.FindPassword();
    }
}
Login.WeiChatLogin = function WeiChatLogin() {
    var url = window.location.href + "/biz/main.html";
    objPub.GetOpenAppID().done(function (json) {
        var result = json.d;
        var obj = new WxLogin({
            id: "divLoginCode",
            appid: result,
            scope: "snsapi_login",
            //redirect_uri: encodeURI(objPub.ManageUrl + "service/WeixinUserAuth.ashx?mictalk_redirect=" + encodeURIComponent(url)),
            redirect_uri: encodeURI(objPub.ManageUrl + "service/WeixinUserAuth.ashx?plat=" + Enum.PlatformTypeSetting.MiicFriends + "&redirect=" + encodeURIComponent(url)),
            state: "",
            style: "",
            href: ""
        });
        $(".login-user-title").text("二维码登录");
        //账号密码登录显示
        $(".login-form").hide();
        //二维码显示
        $(".login-qr-code").show();
    });
}
Login.IsNotNull = function is_not_null(id) {
    var result = false;
    if ($("#" + id).val().length != 0) {
        switch (id) {
            case "txtUserCode":
                $("#" + id).attr("placeholder", "用户名/已注册手机");
                break;
            case "txtPassword":
                $("#" + id).attr("placeholder", "密码");
                break;
            case "txtEmail":
                $("#" + id).attr("placeholder", "邮箱地址");
                break;
            default:
                tip = "";
                break;
        }

        if (id == "txtEmail") {
            if (!$("#" + id).val().match(/^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$/)) {
                $("#" + id + "Tip").html("邮箱格式不正确!")
                           .show();
                $("#" + id).addClass("error-input");
            } else {
                $("#" + id + "Tip").hide();
                $("#" + id).removeClass("error-input");
                result = true;
            }
        }
        else {
            $("#" + id + "Tip").hide();
            $("#" + id).removeClass("error-input");
            result = true;
        }
    }
    else {
        var tip = "";
        switch (id) {
            case "txtUserCode":
                tip = "请输入用户名/已注册手机";
                break;
            case "txtPassword":
                tip = "请输入密码";
                break;
            case "txtEmail":
                tip = "请输入邮箱";
                break;
            default:
                tip = "";
                break;
        }
        $("#" + id + "Tip").html(tip).show();
        $("#" + id).addClass("error-input");
        $("#" + id).removeAttr("placeholder");
    }
    return result;
}

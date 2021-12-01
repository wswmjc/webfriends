//修改密码业务
User.Password = function () { }
User.Password.registerClass("User.Password");
//修改密码
User.Password.ChangePassword = function change_password() {
    $("#sctChangePW").dialog("open");
}
//修改密码事件
User.Password.ChangePasswordEvent = function ChangePasswordEvent(event) {
    $("#txtOldPsword").focus(function () {
        $("#txtOldPswordTip").html("&nbsp;");
    });
    $("#txtNewPsword1").focus(function () {
        $("#txtNewPsword1Tip").html("&nbsp;");
    });
    $("#txtNewPsword2").focus(function () {
        $("#txtNewPsword2Tip").html("&nbsp;");
    });
    if ($.IsNullOrEmpty($("#txtNewPsword1").val())) {
        $("#txtNewPsword1Tip").html("新密码不能为空").show();
        return;
    } else if ($.IsNullOrEmpty($("#txtNewPsword2").val())) {
        $("#txtNewPsword2Tip").html("请确认新密码").show();
        return;
    } else if ($("#txtNewPsword1").val() != $("#txtNewPsword2").val()) {
        $("#txtNewPsword2Tip").html("两次输入不同").show();
        return;
    }
    var passwordView = {
        NewPassword: $("#txtNewPsword1").val(),
        Md5: $.md5($.md5($("#txtNewPsword1").val()))
    };
    var isEmail = false;
    if ($("#txtCkEmail").is(":checked") == true) {
        isEmail = true;
    }
    $.SimpleAjaxPost("service/UserService.asmx/ModifyPassword",
                    true,
                    "{passwordView:" + $.Serialize(passwordView) + ", isEmail:" + isEmail + "}",
                   function (json) {
                       var result = $.Deserialize(json.d);
                       if (result.result == false) {
                           console.log(result.message);
                       }
                       else {
                           $("#txtOldPsword,#txtNewPsword1,#txtNewPsword2").val("");
                           $("#txtOldPswordTip,#txtNewPsword1Tip,#txtNewPsword2Tip").html("&nbsp;");
                           $("#txtCkEmail").prop("checked", false);
                           $("#sctChangePW").dialog("close");
                           //登录成功
                           $.Alert({
                               width: "auto",
                               content: result.message + ",请重新登录系统！"
                           }, function () {
                               $("#sctChangePW").dialog("close");
                               objPub.Logout();
                             
                           });
                       }
                   });
}
//验证密码是否为使用者
User.Password.CheckPassword = function check_password() {
    if ($.IsNullOrEmpty($("#txtPsword").val())) {
        $("#txtPswordTip").html("密码不能为空");
        return;
    }
    var password_view = {
        OldPassword: $("#txtPsword").val()
    };
    $.SimpleAjaxPost("service/UserService.asmx/CheckPassword",
                    true,
                    "{passwordView:" + $.Serialize(password_view) + "}",
                   function (json) {
                       var result = json.d;
                       if (result == false) {
                           $("#txtPswordTip").html("密码不正确，请重新输入");
                       }
                       else {
                           $("#txtPswordTip").html("&nbsp;")
                           $("#txtPsword").val("");
                           $("#sctCheckPW").dialog("close");
                           User.Password.ChangePassword();
                       }
                   });
}
//密码验证事件
User.Password.CheckPasswordEvent = function CheckPasswordEvent(event) {
    $("#sctCheckPW").dialog({
        resizable: false,
        width: 320,
        height: "auto",
        modal: true,
        title: "密码验证",
        buttons: {
            "确　定": function () {
                $("#txtPsword").focus(function () {
                    $("#txtPswordTip").html("&nbsp;");
                });
                User.Password.CheckPassword();
            },
            "取消": function () {
                $(this).dialog("close");
            }
        }
    });
}

//密码强度
User.Password.SetPasswordStrength = function set_password_strength(password) {
    var desc = new Array();
    desc[0] = "很弱";
    desc[1] = "弱";
    desc[2] = "一般";
    desc[3] = "中等";
    desc[4] = "强";
    desc[5] = "非常强";

    var score = 0;

    //小于3位很弱
    if (password.length > 3) score++;

    //a-z,A-Z
    if ((password.match(/[a-z]/)) && (password.match(/[A-Z]/))) score++;

    //数字
    if (password.match(/\d+/)) score++;

    //特殊字符
    if (password.match(/.[!,@,#,$,%,^,&,*,?,_,~,-,(,)]/)) score++;

    //大于12
    if (password.length > 12) score++;

    document.getElementById("passwordText").innerHTML = desc[score];
    document.getElementById("passwordStrength").className = "strength" + score;
}
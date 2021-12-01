User.Setting = function () { }
User.Setting.registerClass("User.Setting");
//读取用户ID
User.Setting.ID = objPub.UserID;
//个人用户读取头像
User.Setting.OldUserUrl = "";
//个人用户新设置头像
User.Setting.NewUserUrl = "";
//读取邮箱
User.Setting.ReadEmail = "";
//读取手机
User.Setting.ReadMobile = "";
User.Setting.GoBack = false;
User.Setting.CheckPsdToType = {
    ChangePassword: 0,
    ResetWX: 1,
    SendSMS: 2
};
//初始化数据
User.Setting.Init = function init() {
    //获取用户信息
    User.Setting.GoBack = false;
    if (User.Setting.ID == "") {
        $.Alert("读取用户信息出错");
        return;
    }
    User.Setting.ReadDetailUser();
    $("#txtEmail").off("blur").on("blur", User.Setting.UniqueEmailEvent);
    $("#txtMobile").off("blur").on("blur", User.Setting.UniqueMobileEvent);
    //绑定提交方法
    $("#tbPersonUser").html5Validate(User.Setting.SubmitUserEvent, {
        submitID: "aUpdate",
        formContainer: "table"
    });
    //重置微信时间绑定
    $("#aResetWX").off("click").on("click", function () {
        User.Setting.CheckPsdTo = User.Setting.CheckPsdToType.ResetWX;
        $(".dialog-checkpw").dialog("open");
    });
    $(document).off("scroll");
    $(window).off("scroll").on("scroll", { WithTimeAxis: false}, window.objPub.ScorllEvent);
    //返回
    $("#aBack").on("click", User.Setting.BackEvent);
    //验证密码
    $(".dialog-checkpw").dialog({
        autoOpen: false,
        resizable: false,
        draggable: false,
        width: 320,
        height: "auto",
        modal: true,
        title: "密码验证",
        buttons: {
            "确　定": function () { 
                $("#txtPsword").focus(function () {
                    $("#txtPswordTip").html("&nbsp;");
                });
                User.Setting.CheckPassword();
            },
            "取消": function () {
                $(this).dialog("close");
            }
        },
        open: function () {
            $("#txtPsword").val("");
            $("#txtPsword").off("keyup").on("keyup", function (event) {
                if (event.which == 13) {
                    $("#txtPsword").focus(function () {
                        $("#txtPswordTip").html("&nbsp;");
                    });
                    User.Setting.CheckPassword();
                }
            });
        }
    });
    //微信重置弹框设置
    $("#sctWxRest").dialog({
        autoOpen: false,
        resizable: false,
        draggable: false,
        width: 360,
        height: "auto",
        modal: true,
        title: "微信重置",
        closeOnEscape: false,
        dialogClass: "no-close",
        create: function () {
            var url = new Array();
            url.push(objPub.ManageUrl);
            url.push("service/UserService.asmx/GetResetWXQrCode");
            $.SimpleAjaxCors(url, "POST").done(function (json) {
                var result = json.d;
                $("#imgResetCode").attr("src", result);
            });
        }
    });
    //微信重置成功点击事件
    $("#divResetSuccess").off("click").on("click", function (event) {
        $("#sctWxRest").dialog("close");
    });
}

//返回事件
User.Setting.BackEvent = function BackEvent(event) {
    $.Confirm({width:"auto",content:"返回前您是否需要保存当前用户信息？"}, function () {
        User.Setting.GoBack = true;
        $("#aUpdate").trigger("click");
    }, function () {
        //初始化首页面
        objPub.InitLeftMain(true);
    });
}

//邮箱验证
User.Setting.UniqueEmailEvent = function UniqueEmailEvent(event) {
    var $target = $(event.target);
    var email = $target.val();
    if (email != "" && email != User.Setting.ReadEmail) {
        var url = new Array();
        url.push(objPub.ManageUrl);
        url.push("service/UserService.asmx/UniqueEmail");
        $.SimpleAjaxCors(url, "POST", "{email:'" + email + "'}").done(function (json) {
            var result = json.d;
            if (result != true) {
                $.Alert({width:"auto",content:"邮箱已被注册，请重新输入~"}, function () {
                    $target.val(User.Setting.ReadEmail).focus();
                });
            }
        });
    }
}

//手机验证
User.Setting.UniqueMobileEvent = function UniqueMobileEvent(event) {
    var $target = $(event.target);
    var mobile = $target.val();
    if (mobile != "" && mobile != User.Setting.ReadMobile) {
        var url = new Array();
        url.push(objPub.ManageUrl);
        url.push("service/UserService.asmx/UniqueMobile");
        $.SimpleAjaxCors(url, "POST", "{mobile:'" + $(this).val() + "'}").done(function (json) {
            var result = json.d;
            if (result != true) {
                $.Alert("手机已被注册，请重新输入", function () {
                    $target.val(User.Setting.ReadMobile).focus();
                });
            } else {
                $("#aAccountSmsBtn").off("click").on("click", User.Setting.SmsRequireEvent);
                $("#aAccountSmsBtn").removeClass("sms-unvalid");
                //show
                $("#spnMiComplete").hide();
                $("#spnMobileIdentify").show();
            }
        });
    } else {
        $("#aAccountSmsBtn").off("click").on("click", User.Setting.SmsRequireEvent);
        $("#aAccountSmsBtn").removeClass("sms-unvalid");
        $("#spnMobileIdentify").hide();
        $("#spnMiComplete").show();
    }
}


User.Setting.UploadUserEvent = function UploadUserEvent(event) {
    if ($(event.target).val() != "") {
        $("#fmUser").ajaxSubmit({
            url: $.GetBaseUrl() + "/service/UserPhotoCrossDomainUploadService.ashx",
            type: "post",
            dataType: "json",
            timeout: 600000,
            success: function (data, textStatus) {
                if (data.result == true) {
                    User.Setting.NewUserUrl = data.acc.FilePath;
                    $("#imgUserPhoto").attr("src", data.acc.TempPath);
                    $.Confirm({ width: "auto", content: "您是否立刻保存一下您的新头像？" }, function () {
                        //保存
                        $("#aUpdate").trigger("click");
                    },
                    function () {
                        //取消
                    });
                } else {
                    $.Alert({width:"auto",content:data.message});
                }
            },
            error: function (data, status, e) {
                $.Alert("上传失败，错误信息：" + e);
            }
        });
    }
}

User.Setting.ReadDetailUser = function read_detail_user() {
    //上传头像
    $("#divUserAvatar").on("mouseenter", function (event) {
        $(".BtnUpload_photo").show();
    }).on("mouseleave", function (event) {
        $(".BtnUpload_photo").hide();
    });

    objPub.GetUserInfo(User.Setting.ID).done(function (json) {
        var result = json.d;
        if (result != null) {
            //头像
            if (result.MicroUserUrl != "") {
                $("#imgUserPhoto").attr("src", result.MicroUserUrl);
            }
            $("#fileUser").on("change", User.Setting.UploadUserEvent);
            User.Setting.OldUserUrl = User.Setting.NewUserUrl = result.MicroUserUrl;
            //用户名
            $("#tdSocialCode").text(result.SocialCode);
            User.Setting.ReadEmail = result.Email;
            $("#txtEmail").val(result.Email);
            //头衔
            $("#txtSocialRemark").val(result.Remark);
            if (result.CanSearch == Enum.YesNo.Yes.toString()) {
                $("#txtCanSearch").prop("checked", true);
            }
            else {
                $("#txtCanSearch").prop("checked", false);
            }
        }
    });

    objPub.GetDetailUserInfo(User.Setting.ID).done(function (json) {
        var result = $.Deserialize(json.d);
        if (result != null) {
            var user = result.UserInfo; 
            $("#txtName").val(user.RealName);
            $("#txtNick").val(user.UserName);
            $("#txtRemark").val(user.Remark);
            //绑定用户性别
            $.each($("input[name='rdUserSex']"), function (index, item) {
                if ($(item).val() == user.Sex) {
                    $(item).prop("checked", true);
                    return false;
                }
            }); 
            $("#txtTel").val(user.Mobile);
            $("#txtLocation").val(user.Location); 
            User.Setting.ReadMobile = user.Mobile; 
            if ($.IsNullOrEmpty(user.Mobile)) {
                $("#spnMiComplete").hide();
                $("#spnMobileIdentify").show();
            } else {
                $("#spnMiComplete").show();
                $("#spnMobileIdentify").hide();
            }
            User.Setting.SubjectBinging(user.SubjectID);
            $("#txtDuty").val(user.MajorDuty);
            $("#txtUniversity").val(user.University);
            $("#txtMotto").val(user.Motto);
            $("#txtOrgName").val(user.OrgName);
            $("#txtMobile").val(user.Mobile);
            $("#txtQQ").val(user.qq);
            $("#txtWeChat").val(user.WeChat);
        }
    });
}
 
User.Setting.SubmitUserEvent = function SubmitUserEvent(event) {
    if (User.Setting.ID != "") {
        var person = {
            UserID: User.Setting.ID,
            UserName: $("#txtNick").val(),
            RealName: $("#txtName").val(),
            Sex: $("input[name='rdUserSex']:checked").val(),
            Nation: $("#sltUserNation").val(),
            qq: $("#txtQQ").val(),
            Tel: $("#txtTel").val(),
            Fax: $("#txtFax").val(),
            Mobile: $("#txtMobile").val(),
            Email: $("#txtEmail").val(),
            OrgName: $("#txtOrgName").val(),
            SubjectID: $("#sltSubject").val(),
            MajorDuty: $("#txtDuty").val(),
            University: $("#txtUniversity").val(),
            Motto: $("#txtMotto").val(),
            Remark: $("#txtRemark").val()
        };
        var social_user_info = {
            ID: User.Setting.ID,
            Mobile: $("#txtMobile").val(),
            Email: $("#txtEmail").val(),
            Remark: $("#txtSocialRemark").val(),
            UpdaterID: objPub.UserID,
            UpdaterName: objPub.UserName,
            CanSearch: $("#txtCanSearch").prop("checked") == true ? Enum.YesNo.Yes.toString() : Enum.YesNo.No.toString()
        };

        //读取头像
        if (User.Setting.NewUserUrl != User.Setting.OldUserUrl) {
            if (User.Setting.NewUserUrl != "") {
                social_user_info.MicroUserUrl = User.Setting.NewUserUrl;
            } else {
                social_user_info.MicroUserUrl = objPub.DefaultPhotoUrl;
            }
        } 
        if (social_user_info.Mobile == User.Setting.ReadMobile) {
            var url = new Array();
            url.push(objPub.ManageUrl);
            url.push("service/UserService.asmx/Submit");
            $.SimpleAjaxCors(url, "POST", "{miicSocialUserInfo:" + $.Serialize(social_user_info) + ",userInfo:" + $.Serialize(person) + "}").done(function (json) {
                var result = json.d;
                if (result == true) {
                    if (User.Setting.GoBack == true) {
                        //初始化首页面
                        objPub.InitLeftMain(true);
                    } else {
                        $.Alert("保存用户信息成功!", function () {
                            User.Setting.OldUserUrl = User.Setting.NewUserUrl = social_user_info.MicroUserUrl;
                        });
                    }
                    //如果已登录人员 修改用户头像
                    $("#imgHeadUserUrl").attr("src", social_user_info.MicroUserUrl);
                } else {
                    $.Alert("保存用户信息失败，请联系管理员");
                }
            });
        } else {
            //检验手机验证码 
            var mobile = social_user_info.Mobile;
            var mobile_code = $("#txtAccountMsgCode").val();
            var url = new Array();
            url.push(objPub.ManageUrl);
            url.push("service/UserService.asmx/CheckMobileCode");
            $.SimpleAjaxCors(url, "POST", "{mobile:'" + mobile + "',mobileCode:'" + mobile_code + "'}").done(function (json) {
                var result = json.d;
                if (result == true) {
                    var url = new Array();
                    url.push(objPub.ManageUrl);
                    url.push("service/UserService.asmx/Submit");
                    $.SimpleAjaxCors(url, "POST", "{miicSocialUserInfo:" + $.Serialize(social_user_info) + ",userInfo:" + $.Serialize(person) + "}").done(function (json) {
                        var result = json.d;
                        if (result == true) {
                            if (User.Setting.GoBack == true) {
                                //初始化首页面
                                objPub.InitLeftMain(true);
                            } else {
                                $.Alert("保存用户信息成功!", function () {
                                    User.Setting.OldUserUrl = User.Setting.NewUserUrl = social_user_info.MicroUserUrl;
                                    $("#aAccountSmsBtn").addClass("sms-unvalid").off("click");
                                    $(window).scrollTop(0);
                                });
                            }
                            //如果已登录人员 修改用户头像
                            $("#imgHeadUserUrl").attr("src", social_user_info.MicroUserUrl);
                        } else {
                            $.Alert("保存用户信息失败，请联系管理员");
                        }
                    });
                } else {
                    $.Alert("手机已被注册，请重新输入", function () {
                        $("#txtMobile").val(User.Setting.ReadMobile).focus();
                    });
                }
            });
        }

    } else {
        $.Alert("丢失ID，请刷新页面");
    } 
}

User.Setting.CheckPassword = function check_password() {
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
                           $(".dialog-checkpw").dialog("close");

                           if (User.Setting.CheckPsdTo == User.Setting.CheckPsdToType.ChangePassword) {
                               $(".dialog-changepw").dialog("open");
                           } else if (User.Setting.CheckPsdTo == User.Setting.CheckPsdToType.ResetWX) {
                               //重置微信
                               $("#sctWxRest").dialog("open");
                           } else if (User.Setting.CheckPsdTo == User.Setting.CheckPsdToType.SendSMS) {
                               User.Setting.SmsRequire();
                           }
                       }
                   });
}
//民族绑定
User.Setting.UserNationBind = function user_nation_bind(init_nation) {
    var url = new Array();
    url.push(objPub.ManageUrl);
    url.push("service/BasicService.asmx/GetNationInfos");
    $.SimpleAjaxCors(url, "POST",null).done(function (json) {
        var result = json.d;
        var temp = "";
        if ($.isArray(result) == true && result.length != 0) {
            $.each(result, function (index, item) {
                temp += "<option value='" + item.Name + "'>" + item.Value + "</option>";
            });
        }
        else {
            temp += "<option value=''>暂无数据</option>";
        }
        $("#sltUserNation").empty().append(temp).val(init_nation);
    });
}
//擅长领域绑定
User.Setting.SubjectBinging = function subject_binding(subject_id) {
    objPub.GetSubjectList()
        .done(function (json) {
        var result = json.d;
        var temp = "<option  value=''>--- 请选择 ---</option>";
        if ($.isArray(result) && result.length != 0) {
            $.each(result, function (index, item) { 
                //if (item.Name == subject_id) {
                //    temp += "<option select='selected' value='" + item.Name + "'>" + item.Value + "</option>";
                //} else {
                    temp += "<option value='" + item.Name + "'>" + item.Value + "</option>";
                //}
            });

        }
        $("#sltSubject").empty().append(temp);
        if (subject_id != undefined && subject_id != null) {
            $("#sltSubject").val(subject_id);
        }
    });
}
//手机验证码
User.Setting.SmsRequireEvent = function SmsRequireEvent(event) {
    User.Setting.CheckPsdTo = User.Setting.CheckPsdToType.SendSMS;
    $(".dialog-checkpw").dialog("open"); 
}
//手机验证
User.Setting.SmsRequire = function sms_require() { 
    var mobile = $("#txtMobile").val();
    if ($.IsNullOrEmpty(mobile) == false && /^(1[35847]\d{9})$/.test(mobile) == true) {
        
        var url = new Array();
        url.push(objPub.ManageUrl);
        url.push("service/UserService.asmx/SendSMS"); 
        $.SimpleAjaxCors(url, "POST", "{mobile:'" + mobile + "'}").done(function (json) {
            var result = json.d; 
            if (result == true) {
                var leftTime = 60;
                $("#aAccountSmsBtn").addClass("sms-unvalid").off("click");
                //激活验证按钮
                $("#aAccountSmsConfirmBtn").removeClass("sms-unvalid").off("click").on("click", User.Setting.IdentifyMobileEvent);
                User.Setting.Timer = setInterval(function () {
                    if (leftTime > 0) {
                        $("#aAccountSmsBtn").html("已发送" + leftTime + "S");
                    } else {
                        $("#aAccountSmsBtn").html("短信验证").removeClass("sms-unvalid").off("click").on("click", User.Setting.SmsRequireEvent);
                        //验证按钮置为失效
                        $("#aAccountSmsConfirmBtn").addClass("sms-unvalid").off("click");
                        clearInterval(User.Setting.Timer);
                    }
                    leftTime--;
                }, 1000);
            } else {
                $.Alert("发送手机验证码失败");
            }
        });
    } else { 
        $("#txtMobile").testRemind("请输入正确的手机号码");
    }
}
User.Setting.IdentifyMobileEvent = function IdentifyMobileEvent(event) {
    if (User.Setting.UserID != "") {
        var person = {
            UserID: User.Setting.UserID,
            Mobile: $("#txtMobile").val()
        };

        var social_user_info = {
            ID: User.Setting.UserID,
            Mobile: $("#txtMobile").val(),
            UpdaterID: objPub.UserID,
            UpdaterName: objPub.UserName
        };

        var mobile = social_user_info.Mobile;
        var mobile_code = $("#txtAccountMsgCode").val();
        var url = new Array();
        url.push(objPub.ManageUrl);
        url.push("service/UserService.asmx/CheckMobileCode");
        $.SimpleAjaxCors(url, "POST", "{mobile:'" + mobile + "',mobileCode:'" + mobile_code + "'}").done(function (json) {
            var result = $.Deserialize(json.d);
            if (result.result == true) {
                var url = new Array();
                url.push(objPub.ManageUrl);
                url.push("service/UserService.asmx/Submit");
                $.SimpleAjaxCors(url, "POST",
                    "{miicSocialUserInfo:" + $.Serialize(social_user_info) + ",userInfo:" + $.Serialize(person) + "}")
                    .done(function (json) {
                        var result = json.d;
                        if (result == true) {
                            $("#aAccountSmsBtn,#aAccountSmsConfirmBtn").addClass("sms-unvalid").off("click");
                            $("#spnMobileIdentify").hide();
                            $("#spnMiComplete").show();
                            User.Setting.IdentifyMobile = true;
                            User.Setting.ReadMobile = social_user_info.Mobile;
                        } else {
                            $.Alert("验证手机信息失败");
                            User.Setting.IdentifyMobile = false;
                        }
                    });
            } else {
                $("#txtAccountMsgCode").testRemind(result.message);
                $("#txtAccountMsgCode").focus();
            }
        });

    } else {
        $.Alert("丢失ID，请刷新页面");
    }
}
User.Setting.MobileFieldMoveOutEvent = function MobileFieldMoveOutEvent(event) {
    if ($("#txtAccountMobile").val() != "" && $("#txtAccountMobile").val() == User.Setting.ReadMobile) {
        $("#aAccountSmsBtn").off("click").addClass("sms-unvalid");
        $("#spnMobileIdentify").hide();
        $("#spnMiComplete").show();
    }

}
User.Setting.MobileFieldKeyPressEvent = function MobileFieldKeyPressEvent(event) {
    var keynum = (event.keyCode ? event.keyCode : event.which);
    if (keynum == "9") {
        //alert($(this).attr("id"));
        if ($("#txtAccountMobile").val() != "" && $("#txtAccountMobile").val() == User.Setting.ReadMobile) {
            $("#aAccountSmsBtn").off("click").addClass("sms-unvalid");
            $("#spnMobileIdentify").hide();
            $("#spnMiComplete").show();
        }
    }


}
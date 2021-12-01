//---------------------------设置区域---------------------------//
$.SetBaseUrl("http://pyq.mictalk.cn/");
//避免缓存文件
$.ajaxSetup({
    cache: false
});

//---------------------------枚举变量---------------------------//
Type.registerNamespace("Enum");


///用户类别枚举@start///
Enum.UserType = function () {
    throw Error.notImplemented();
}

Enum.UserType.prototype = {
    // 超级管理员
    Administrator: 0,
    // 国家管理员
    CountryAdmin: 11,
    // 地方管理员
    LocalAdmin: 12,
    // 国家主管部门
    CountryDepartUser: 21,
    // 地方主管部门
    LocalDepartUser: 22,
    // 企业用户
    OrgUser: 31,
    // 个人用户
    PersonUser: 41,
    //所有管理员
    AllAdminPerson: 98,
    //所有管理机构
    AllAdminDeparter: 97,
    //所有
    All: 99

};

Enum.UserType.registerEnum("Enum.UserType");


Enum.UserType.GetDescription = function get_description(user_type) {
    var result = "";
    if (isNaN(user_type) == true) {
        throw Error.argumentType("user_type", null, Enum.UserType, "参数类型不符");
    }
    else {
        switch (user_type) {
            case Enum.UserType.PersonUser:
                result = "个人用户";
                break;
            case Enum.UserType.OrgUser:
                result = "企业用户";
                break;
            case Enum.UserType.CountryDepartUser:
                result = "国家管理机构";
                break;
            case Enum.UserType.LocalDepartUser:
                result = "地方管理机构";
                break;
            case Enum.UserType.CountryAdmin:
                result = "国家管理员";
                break;
            case Enum.UserType.LocalAdmin:
                result = "地方管理员";
                break;
            case Enum.UserType.Administrator:
                result = "超级管理员";
                break;
            case Enum.UserType.AllAdminPerson:
                result = "管理员";
                break;
            case Enum.UserType.AllAdminDeparter:
                result = "管理机构";
                break;
            default:
                break;
        }
    }
    return result;
};
///用户类别枚举@end///


///主题级别枚举@start///
Enum.ThemeLevel = function () {
    throw Error.notImplemented();
}
Enum.ThemeLevel.prototype = {
    //初级
    PrimaryLevel: 1,
    //中级
    IntermediateLevel: 10,
    //高级
    AdvancedLevel: 20
}
Enum.ThemeLevel.registerEnum("Enum.ThemeLevel");
///主题级别枚举@end///

///性别枚举@start///
Enum.SexType = function () {
    throw Error.notImplemented();
}
Enum.SexType.prototype = {
    Male: 0,
    Female: 1
};
Enum.SexType.registerEnum("Enum.SexType");
///性别枚举@end///

///样式固定值枚举@start///
Enum.FixedType = function () {
    throw Error.notImplemented();
}
Enum.FixedType.prototype = {
    Fixed: 1,
    NoFixed:3,
    Inherit:2
};
Enum.FixedType.registerEnum("Enum.FixedType");
Enum.FixedType.GetFixedType=function get_fixed_Type(fixed_type){
    var result = "";
    if (isNaN(fixed_type) == true) {
        throw Error.argumentType("fixed_type", null, Enum.FixedType, "参数类型不符");
    }
    else {
        switch (fixed_type) {
            case Enum.FixedType.Fixed:
                result = "fixed";
                break;
            case Enum.FixedType.Inherit:
                result = " inherit";
                break;
            case Enum.FixedType.NoFixed:
                result = "no-fixed";
            default:
                break;
        }
    }
    return result;
}
///样式固定值枚举@end///

///是否类别枚举@start///
Enum.YesNo = function () {
    throw Error.notImplemented();
}

Enum.YesNo.prototype = {
    Unknow: 0,
    Yes: 1,
    No: 2
};
Enum.YesNo.registerEnum("Enum.YesNo");

Enum.YesNo.GetDescription = function get_description(yes_no) {
    var result = "";
    if (isNaN(yes_no) == true) {
        throw Error.argumentType("yes_no", null, Enum.YesNo, "参数类型不符");
    }
    else {
        switch (yes_no) {
            case Enum.YesNo.No:
                result = "No";
                break;
            case Enum.YesNo.Yes:
                result = "Yes";
                break;

            default:
                break;
        }
    }
    return result;
};

///是否类别枚举@end///

///有效性类别枚举@start///
Enum.Valid = function () {
    throw Error.notImplemented();
}
Enum.Valid.prototype = {
    //未知
    Unknow: 0,
    //无效
    InValid: 1,
    //有效
    Valid: 2
};

Enum.Valid.registerEnum("Enum.Valid");

Enum.Valid.GetDescription = function get_description(valid) {
    var result = "";
    if (isNaN(valid) == true) {
        throw Error.argumentType("valid", null, Enum.Valid, "参数类型不符");
    }
    else {
        switch (valid) {
            case Enum.Valid.Valid:
                result = "有效";
                break;
            case Enum.Valid.InValid:
                result = "失效";
                break;

            default:
                break;
        }
    }
    return result;
};
///有效性类别枚举@end///

///激活状态类别枚举@start///
Enum.ActivateFlag = function () {
    throw Error.notImplemented();
}
Enum.ActivateFlag.prototype = {
    //激活
    Agree: 0,
    //拒绝激活
    Refuse: 1,
    //待激活
    Waiting: 2
};
Enum.ActivateFlag.registerEnum("Enum.ActivateFlag");
Enum.ActivateFlag.GetDescription = function get_description(activate_flag) {
    var result = "";
    if (isNaN(activate_flag) == true) {
        throw Error.argumentType("activate_flag", null, Enum.ActivateFlag, "参数类型不符");
    }
    else {
        switch (activate_flag) {
            case Enum.ActivateFlag.Agree:
                result = "已激活";
                break;
            case Enum.ActivateFlag.Refuse:
                result = "拒绝激活";
                break;
            case Enum.ActivateFlag.Waiting:
                result = "待激活";
            default:
                break;
        }
    }
    return result;
};
///激活状态类别枚举@end///

///附件类别枚举@start///
Enum.AccType = function () {
    throw Error.notImplemented();
}

Enum.AccType.prototype = {
    Photo: 0,
    File: 1
};

Enum.AccType.registerEnum("Enum.AccType");
///附件类别枚举@end///

///文件类别枚举@start///
Enum.FileType = function () {
    throw Error.notImplemented();
}

Enum.FileType.prototype = {
    Word: 1,
    Excel: 2,
    PowerPoint: 3,
    Pdf: 4,
    Text: 5,
    Accessory: 6,
    Xml:7

};

Enum.FileType.registerEnum("Enum.FileType");
Enum.FileType.GetIconClass = function get_icon_class(file_type) {
    var result = "icon-file";
    switch (file_type) {
        case Enum.FileType.Word:
            result = "icon-word";
            break;
        case Enum.FileType.Excel:
            result = "icon-excel";
            break;
        case Enum.FileType.PowerPoint:
            result = "icon-ppt";
            break;
        case Enum.FileType.Pdf:
            result = "icon-pdf";
            break;
        case Enum.FileType.Text:
            result = "icon-file";
            break;
        case Enum.FileType.Accessory:
            result = "icon-zip";
            break;
        default:
            result = "icon-file";
            break;
    }
    return result;
}
///文件类别枚举@end///

///平台类别枚举@start///
Enum.PlatformTypeSetting = function () {
    throw Error.notImplemented();
}

Enum.PlatformTypeSetting.prototype = {
    //微博
    MiicMicroblog: 0,
    //朋友圈
    MiicFriends: 1,
    //管理
    Manage: 2,
    //线上活动
    MicTalk: 3,
    //资讯
    Info: 4,
    //线下活动
    Activity: 5,
    //所有
    All: 9
};
Enum.PlatformTypeSetting.registerEnum("Enum.PlatformTypeSetting");
///平台类别枚举@end///

///发布状态类别枚举@start///
Enum.PublishStatus = function () {
    throw Error.notImplemented();
}
Enum.PublishStatus.prototype = {
    Yes: 1,
    No: 2
};

Enum.PublishStatus.registerEnum("Enum.PublishStatus");
Enum.PublishStatus.GetColor = function get_color(publish_status) {
    var result = "";
    if (isNaN(publish_status) == true) {
        throw Error.argumentType("PublishStatus", null, Enum.PublishStatus, "参数类型不符");
    }
    else {
        switch (publish_status) {
            case Enum.PublishStatus.Yes:
                result = "#7932ea";
                break;
            case Enum.PublishStatus.No:
                result = "#fa9d1a";
                break;
            default:
                break;
        }
    }
    return result;
}
Enum.PublishStatus.GetDescription = function get_description(publish_status) {
    var result = "";
    if (isNaN(publish_status) == true) {
        throw Error.argumentType("PublishStatus", null, Enum.PublishStatus, "参数类型不符");
    }
    else {
        switch (publish_status) {
            case Enum.PublishStatus.Yes:
                result = "发布";
                break;
            case Enum.PublishStatus.No:
                result = "未发布";
                break;
            default:
                break;
        }
    }
    return result;
};
///发布状态类别枚举@end///

///主题等级类别枚举@start///
Enum.ThemeLevel = function () {
    throw Error.notImplemented();
}
Enum.ThemeLevel.prototype = {
    PrimaryLevel: 1,
    IntermediateLevel: 10,
    AdvancedLevel: 20
};

Enum.ThemeLevel.registerEnum("Enum.ThemeLevel");
Enum.ThemeLevel.GetDescription = function get_description(theme_level) {
    var result = "";
    if (isNaN(theme_level) == true) {
        throw Error.argumentType("ThemeLevel", null, Enum.ThemeLevel, "参数类型不符");
    }
    else {
        switch (theme_level) {
            case Enum.ThemeLevel.PrimaryLevel:
                result = "普通";
                break;
            case Enum.ThemeLevel.IntermediateLevel:
                result = "会员";
                break;
            case Enum.ThemeLevel.AdvancedLevel:
                result = "VIP";
                break;
            default:
                break;
        }
    }
    return result;
};
///主题等级类别枚举@end///

///提醒人来源枚举@start///
Enum.BusinessType = function () {
    throw Error.notImplemented();
}

Enum.BusinessType.prototype = {
    //朋友圈，所有
    Moments: 0,
    //行业圈子
    Community: 1,
    //讨论组
    Group: 2
};

Enum.BusinessType.registerEnum("Enum.BusinessType");
///提醒人来源枚举@end///

///提醒类别枚举@start///
Enum.NoticeType = function () {
    throw Error.notImplemented();
}

Enum.NoticeType.prototype = {
    //@发表信息
    PublishInfo: 0,
    //@回复
    Message: 1
};

Enum.NoticeType.registerEnum("Enum.NoticeType");
///提醒类别枚举@end///

///消息类别枚举@start///
Enum.PublishInfoType = function () {
    throw Error.notImplemented();
}

Enum.PublishInfoType.prototype = {
    //长篇
    Long: 0,
    //短篇
    Short:1
}

Enum.PublishInfoType.registerEnum("Enum.PublishInfoType");
///消息类别枚举@end///

///消息所属页面@start///
Enum.PublishInfoBelong = function () {
    throw Error.notImplemented();
}

Enum.PublishInfoBelong.prototype = {
    //主页面
    Main: 0,
    //个人页面
    Self: 1,
    //他人页面
    Other:2
}

Enum.PublishInfoBelong.registerEnum("Enum.PublishInfoBelong");
///消息所属页面@end///

///行业圈子信息分类@start///
Enum.CommunitySubType = function () {
    throw Error.notImplement();
}

Enum.CommunitySubType.prototype = {
    //讨论
    Topic: 0,
    //话题
    Subject:1
}

Enum.CommunitySubType.registerEnum("Enum.CommunitySubType");
///行业圈子信息分类@end///
//---------------------------枚举变量---------------------------//

//---------------------------全局变量---------------------------//
var user_theme_default_id = "7E9E7B47-F184-465F-83F5-85FB69F8C330";
window.objPub = {
    UserID: $.GetCookie("SNS_ID") == undefined ? "" : $.GetCookie("SNS_ID"),
    SocialCode: $.GetCookie("SNS_SocialCode") == undefined ? "" : $.GetCookie("SNS_SocialCode"),
    UserType: $.GetCookie("SNS_UserType") == undefined ? "" : $.GetCookie("SNS_UserType"),
    UserUrl: $.GetCookie("SNS_UserUrl") == undefined ? "" : $.GetCookie("SNS_UserUrl"),
    UserName: $.GetCookie("SNS_UserName") == undefined ? "" : decodeURI($.GetCookie("SNS_UserName")),
    One: $.GetCookie("SNS_One") == undefined ? "" : $.GetCookie("SNS_One"),
    UserThemeID: $.GetCookie("SNS_FriendsUserThemeID") == undefined ? user_theme_default_id : $.GetCookie("SNS_FriendsUserThemeID"),
    //朋友圈
    FriendsUrl: "http://pyq.mictalk.cn/",
    //管理平台
    ManageUrl: "http://sns.mictalk.cn/",
    //微博平台
    MicroblogUrl: "http://weibo.mictalk.cn/",
    //talk平台
    TalkUrl:"http://talk.mictalk.cn/",
    CurrentUser: null,
    UserLevel: "1",
    IsMain: false,
    Cors: new Array(),
    WindowScrollTop: 0,
    MinTipPage: 1,
    ServiceID: "1"
};
//配置跨域单点登录信息
var cors_config = new Array();
cors_config.push({ Url: objPub.ManageUrl });
cors_config.push({ Url: objPub.MicroblogUrl });
objPub.Cors = cors_config;
if (objPub.CurrentUser == null
    && objPub.UserID != "") {
    if (objPub.UserType == Enum.UserType.PersonUser.toString()
        || objPub.UserType == Enum.UserType.Administrator.toString()
        ) {
        var url = new Array();
        url.push("http://sns.mictalk.cn/");
        url.push("service/UserService.asmx/GetUserDetailInformation");
        $.SimpleAjaxCorsWithSyn(url, false,"POST", "{userID:'" + objPub.UserID + "'}")
            .done(function (json) {
                var result = $.Deserialize(json.d);
                if (result != null) {
                    with (objPub) {
                        CurrentUser = result.UserInfo;
                        UserName = CurrentUser.UserName;
                        UserLevel = result.SocialUserInfo.UserLevel;
                    }
                }
            });
    }
}

window.objPub.ClearObjPub = function clear_obj_pub() {
    $.ClearCookie("SNS_ID");
    $.ClearCookie("SNS_SocialCode");
    $.ClearCookie("SNS_UserName");
    $.ClearCookie("SNS_UserUrl");
    $.ClearCookie("SNS_FriendsUserThemeID");
    $.ClearCookie("SNS_UserType");
    $.ClearCookie("SNS_One");
    localStorage.removeItem("rsa");
    with (objPub) {
        UserID = "";
        UserUrl = "";
        UserName = "";
        UserType = "";
        One = "";
        UserThemeID = user_theme_default_id;
        CurrentUser = null;
    }
}

window.objPub.SetObjPub = function set_objPub() {
    with (objPub) {
        UserID = $.GetCookie("SNS_ID");
        UserUrl = $.GetCookie("SNS_UserUrl");
        SocialCode = $.GetCookie("SNS_SocialCode");
        UserName = decodeURI($.GetCookie("SNS_UserName"));
        UserType = $.GetCookie("SNS_UserType");
        One = $.GetCookie("SNS_One");
        UserThemeID = $.GetCookie("SNS_FriendsUserThemeID");
    }
    $.PostCorsMessage(objPub.Cors, { MessageType: "PostInfo", rsa: localStorage.getItem("rsa"), social_code: localStorage.getItem("social_code") });
}
//获取主题样式
window.objPub.GetTheme = function get_theme(id) {
    var url = new Array();
    url.push(objPub.ManageUrl);
    url.push("service/FriendsThemeService.asmx/GetInformation");
    $.SimpleAjaxCors(url, "POST", "{id:'" + id + "'}").done(function (json) {
        var result = json.d;
        less.modifyVars({
            //大背景图片
            "@body-bg-img": "url('" + result.BackgroundLogoUrl + "')",
            //大背景颜色
            "@body-bg-color": result.BackgroundColor,
            //大背景图片重复
            "@body-bg-repeat": result.BackgroundRepeat == Enum.YesNo.Yes.toString() ? "repeat" : "no-repeat",
            //大背景图片是否固定
            "@body-bg-fixed": Enum.FixedType.GetFixedType(parseInt(result.BackgroundFixed )),
            //顶部区域背景色
            "@switch-header-bg": result.HeaderColor,
            //账户背景图片
            "@account-bg-img": "url('"+result.ThemeUrl+"')",
            //左侧背景色
            "@main-left-bgcolor": result.LeftColor,
            //左侧列表底边颜色
            "@main-left-bordercolor": result.LeftBorderColor,
            //列表操作图标
            "@main-options-icon": "url('" + result.OptionLogoUrl + "')",
            //右侧背景色
            "@main-right-bgcolor": result.RightColor,
            //正副文字体颜色
            "@main-text-color": result.MainTextColor,
            "@sub-text-color":result.SubTextColor,
            //主副元素颜色（背景、字体。。。）
            "@main-color": result.MainColor,
            "@sub-color": result.SubColor

        });

    });
}
window.objPub.GetUserInfo = function get_user(id) {
    var url = new Array();
    url.push(objPub.ManageUrl);
    url.push("service/UserService.asmx/GetInformation");
    return $.SimpleAjaxCors(url, "POST", "{userID:'" + id + "'}");
}
//初始化朋友圈主页
window.objPub.InitLeftMain = function init_left_main(is_mine, person) {
    objPub.IsMain = true;
    $(".main-left").load("../biz/left/moments.html", function () {
        if (is_mine == true) {
            Moments.Init(is_mine);
        }
        else {
            Moments.Init(is_mine, person);
        }
        //初始隐藏返回按钮
        $("#aBackHome").hide();
    });
   
}
//获取微信AppID
window.objPub.GetOpenAppID = function get_open_app_id() {
    var url = new Array();
    url.push(objPub.ManageUrl);
    url.push("service/UserService.asmx/GetOpenAppID");
    return $.SimpleAjaxCors(url, "POST");
}
//获取用户详细信息
window.objPub.GetDetailUserInfo = function get_detail_user(id) {
    var url = new Array();
    url.push(objPub.ManageUrl);
    url.push("service/UserService.asmx/GetUserDetailInformation");
    return $.SimpleAjaxCors(url, "POST", "{userID:'" + id + "'}");
}
//获取擅长领域的信息
window.objPub.GetSubjectList = function get_subject_list() {
    var url = new Array();
    url.push(objPub.TalkUrl);
    url.push("service/TalkService.asmx/GetAllSubjectList");
    return $.SimpleAjaxCors(url, "POST", "{withLabel:false}");
}
//获取离线信息
window.objPub.GetMyOfflineList = function get_my_off_line_list() {
    return $.SimpleAjaxPost("service/NoticeService.asmx/GetMyOfflineNoticeList", true);
}
window.objPub.MomentsBrowse = function moments_browse(publish_id) {
    $.SimpleAjaxPost("service/MomentsService.asmx/AddBrowse", true, "{publishID:'" + publish_id + "'}");
}
window.objPub.CommunityBrowse = function community_browse(publish_id) {
    $.SimpleAjaxPost("service/CommunityService.asmx/AddBrowse", true, "{publishID:'" + publish_id + "'}");
}
//登出
window.objPub.Logout = function logout() {
    objPub.ClearObjPub();
    if (localStorage.getItem("remember_social_code") == Enum.YesNo.No.toString()) {
        localStorage.removeItem("social_code");
        localStorage.removeItem("remember_social_code");
        $.PostCorsMessage(objPub.Cors, { MessageType: "ClearInfo",RememberMe:false });
    }
    else {
        $.PostCorsMessage(objPub.Cors, { MessageType: "ClearInfo", RememberMe: true });
    }

}
window.objPub.GetSimpleTimeFormat=function GetSimpleTime(time) {
    var result;
    var my_date = new Date().getTime() - time.getTime();
    if (my_date / (1000 * 60 * 60) < 24) {
        if (my_date / (1000 * 60) < 5) {
            result = "5分钟前";
        }
        else if (my_date / (1000 * 60) < 10) {
            result = "10分钟前";
        }
        else if (my_date / (1000 * 60) < 30) {
            result = "30分钟前";
        }
        else if (my_date / (1000 * 60 * 60) < 1) {
            result = "1小时前";
        }
        else if (my_date / (1000 * 60 * 60) < 5) {
            result = "5小时前";
        }
        else if (my_date / (1000 * 60 * 60) < 12) {
            result = "12小时前";
        }
        else {
            result = "1天前";
        }
    }
    else {
        result = time.Format("yyyy-MM-dd HH:mm:ss");
    }
    return result;
}

window.objPub.ReLogin = function re_login() {
    window.objPub.ClearObjPub();
    window.location.href = "../index.html";
}
//请求需要显示的平台服务
window.objPub.GetServiceList = function get_service_list(is_admin) {
    var url = new Array();
    url.push(objPub.ManageUrl);
    url.push("service/ServiceInfoService.asmx/GetShowServiceList");
    return $.SimpleAjaxCors(url, "POST", "{serviceID:'" + objPub.ServiceID + "',IsAdmin:" + is_admin + "}");
}
//windwow滚轮事件
window.objPub.ScorllEvent = function ScorllEvent(event) {
    var with_time_axis = event.data.WithTimeAxis;
    if (with_time_axis == true) {
        if ($(this).scrollTop() >= 390) {
            $("#divGoTop").fadeIn(30);
            $(".filter").css({
                "top": "30px",
                "position": "fixed"
            });
        } else {
            $("#divGoTop").fadeOut(30);
            var from_person = event.data.FromPerson;
            if (from_person == false) {
                //moments-list
                $(".filter").css({
                    "top": (390 - $(this).scrollTop()) + "px"
                });
            }
            else {
                //moments-list-person
                $(".filter").css({
                    "top": "296px",
                    "position": "absolute"
                });
            }
        }
    }
    if (PublishRight.ScrollTopNum != 0) {
        if ($(this).scrollTop() >= PublishRight.ScrollTopNum) {
            $("#sctHotCommunity").css({
                "top": "0",
                "position": "fixed"
            });
        }
        else {
            $("#sctHotCommunity").css({
                "top": PublishRight.ScrollTopNum,
                "position": "static"
            });
        }
    }
}
window.objPub.Gallery = function gallery() {
    var realWidth;
    var realHeight;
    $(".gallery").find("img").each(function (index, item) {
        var _this = $(this);
        $("<img/>").attr("src",$(this).attr("src")).load(function() {
            realWidth = this.width;
            realHeight = this.height;
            if(realHeight > realWidth){
                _this.closest("li").addClass("li-n-h");
            }
        });
    });
}
//---------------------------全局变量---------------------------//



//-----------------------------方法-----------------------------//
// 对Date的扩展，将 Date 转化为指定格式的String 
// 月(M)、日(d)、小时(h)、分(m)、秒(s)、季度(q) 可以用 1-2 个占位符， 
// 年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字) 
// 例子： 
// (new Date()).Format("yyyy-MM-dd hh:mm:ss.S") ==> 2006-07-02 08:09:04.423 
// (new Date()).Format("yyyy-M-d h:m:s.S")      ==> 2006-7-2 8:9:4.18 
Date.prototype.Format = function (fmt) { //author: meizz 
    var o = {
        "M+": this.getMonth() + 1,                 //月份 
        "d+": this.getDate(),                    //日 
        "h+": this.getHours(),                   //小写小时 
        "H+": this.getHours(),                   //大写小时 
        "m+": this.getMinutes(),                 //分 
        "s+": this.getSeconds(),                 //秒 
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
        "S": this.getMilliseconds()             //毫秒 
    };
    if (/(y+)/.test(fmt))
        fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt))
            fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
}

Date.prototype.getZhCnMonth = function () {
    var zh_cn_month_list = ['一月', '二月', '三月', '四月', '五月', '六月', '七月', '八月', '九月', '十月', '十一月', '十二月'];
    return zh_cn_month_list[this.getMonth()];
}

function handle_exception(x) {
    var exception_obj = deserialize(x.responseText, true);
    if (exception_obj.ExceptionType == "Miic.MiicException.MiicCookieArgumentNullException") {
        window.objPub.ReLogin();
    } else {
        console.log(exception_obj.Message);
    }
}
//中文日期 for datepicker
jQuery(function ($) {
    $.datepicker.regional['zh-CN'] = {
        clearText: '清除',
        clearStatus: '清除已选日期',
        closeText: '关闭',
        closeStatus: '不改变当前选择',
        prevText: '<',
        prevStatus: '显示上月',
        prevBigText: '<<',
        prevBigStatus: '显示上一年',
        nextText: '>',
        nextStatus: '显示下月',
        nextBigText: '>>',
        nextBigStatus: '显示下一年',
        currentText: '今天',
        currentStatus: '显示本月',
        monthNames: ['一月', '二月', '三月', '四月', '五月', '六月', '七月', '八月', '九月', '十月', '十一月', '十二月'],
        monthNamesShort: ['一月', '二月', '三月', '四月', '五月', '六月', '七月', '八月', '九月', '十月', '十一月', '十二月'],
        monthStatus: '选择月份',
        yearStatus: '选择年份',
        weekHeader: '周',
        weekStatus: '年内周次',
        dayNames: ['星期日', '星期一', '星期二', '星期三', '星期四', '星期五', '星期六'],
        dayNamesShort: ['周日', '周一', '周二', '周三', '周四', '周五', '周六'],
        dayNamesMin: ['日', '一', '二', '三', '四', '五', '六'],
        dayStatus: '设置 DD 为一周起始',
        dateStatus: '选择 m月 d日, DD',
        dateFormat: 'yy-mm-dd',
        firstDay: 1,
        initStatus: '请选择日期',
        isRTL: false
    };
    $.datepicker.setDefaults($.datepicker.regional['zh-CN']);
});

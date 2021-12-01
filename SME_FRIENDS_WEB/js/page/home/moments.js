Moments = function () { }
Moments.registerClass("Moments");
Moments.IsPerson = false;
Moments.Init = function init(is_mine, person) {
    if (is_mine == false) {
        if (arguments.length == 1) throw Error.parameterCount("Invalid argument count!");
        //访问他人
        $("#imgUserUrl").attr("src", person.UserUrl);
        $("#divSocialCode").html(person.UserName);
        //获取我的统计信息
        Moments.GetPersonStatisticsCount(person.UserID);
        //删除我的编辑内容、我的切换内容、快速编辑
        $("#divMyEdit,#divMyFriendsTabs,#liMyQuickEdit").remove();
    }
    else if (is_mine == true) {
        if (arguments.length > 1) throw Error.parameterCount("Invalid argument count!");
       
        //我的朋友圈
        $("#imgUserUrl").attr("src", objPub.UserUrl);
        $("#divSocialCode").text(objPub.UserName);
        $("#spnMyUserLevel,#spnHeadUserLevel").text(objPub.UserLevel=="9"?"VIP":"V" + objPub.UserLevel);
        //获取我的统计信息
        Moments.GetPersonStatisticsCount(objPub.UserID);
        //初始化首页面
       // $(".friend-tabs-content").load("../biz/left/home-message.html");
        //切换选项卡
        $(".friend-tabs>a").on("click", Moments.FriendsTabChangeEvent);
        //访问我发布的信息
        $("#aPublishCount").on("click", function (event) { 
            Moments.List.Person.Init(person ? person.UserID : objPub.UserID);
        });

        //访问通讯录
        $("#aAddressBookCount").on("click", function (event) {
            $(".main-left").load("../biz/left/addressbook/list.html", function (response, status) {
                if (status == "success") {
                    AddressBook.Init();
                }
            });
        });
        //访问讨论组
        $("#aGroupCount").on("click", function (event) {
            $(".main-left").load("../biz/left/group/manage-list.html", function (response, status) {
                if (status == "success") {
                    Group.Manage.Init();
                }
            });
        });
        //访问行业圈子
        $("#aCommunityCount").on("click", function (event) {
            $(".main-left").load("../biz/left/community/manage-list.html", function (response, status) {
                if (status == "success") {
                    Community.Manage.Init(1);
                }
            });
        });
        $("#aSetMyTheme").on("click", Moments.SetMyThemeEvent);
         
          
       
    }
    
    $("#imgUserUrl").on("click", function (event) { 
        Moments.List.Person.Init(person ? person.UserID : objPub.UserID);
    }).css("cursor","pointer");

    //打开通讯录
    $(".contacts").on("click", function(){
        $(".main-left").load("../biz/left/addressbook/list.html", function (response, status) {
            if (status == "success") {
                AddressBook.Init();
            }
        });
    });
    //打开发短消息窗口
    $("#aMessage").off("click").on("click", function (event) { 
        $("#sctShort").dialog("open"); 
    });
    //打开长篇窗口
    $("#aEditor").off("click").on("click", function (event) { 
        $("#sctLong").dialog("open");
    });
    Moments.List.Init();
    //打开收藏列表
    $("#aCollectList").on("click", function (event) {
        $(".user-account").hide();
        $("#filter").css({
            "top": "135px"
        });
        Collect.Init();
    });
    //打开草稿箱
    $("#aDraftList").on("click", function (event) {
        $(".user-account").hide();
        $("#filter").css({
            "top": "135px"
        });
        Draft.Init();
    });
}
Moments.FriendsTabChangeEvent = function FriendsTabChangeEvent(event) {
    var contentID = $(this).attr("name");
    $(this).addClass("selected").siblings().removeClass("selected");
    if (contentID === "listFriend") {
        Moments.List.Year = new Date().getFullYear().toString();
        Moments.List.Month = (new Date().getMonth() + 1).toString();
        Moments.List.Init();
    } else if (contentID === "listCircle") {
        // 行业圈子
        Community.Init();
    } else if (contentID === "listGroup") {
        // 行业圈子
        Group.Init();
    } else {
        // 公告
        Announce.Init();
    }
}
Moments.GetPersonStatisticsCount = function get_person_statistics_count(userID) {
    $.SimpleAjaxPost("service/UserService.asmx/GetPersonStatisticsCount",
                        true,
                        "{userID:'" + userID + "'}",
                        function (json) {
                            var result = json.d;
                            $.each(result, function (index, item) {
                                $("#div" + item.Name).text(item.Value);
                            });
                           
                        });
}
//设置我的样式主题事件
Moments.SetMyThemeEvent = function SetMyThemeEvent(event) {
    Moments.GetThemeList();
    $("#sctSetTheme").dialog("open");
}
Moments.GetThemeList = function get_theme_list() {
    var url = new Array();
    url.push(objPub.ManageUrl);
    url.push("service/FriendsThemeService.asmx/GetThemeInfosByLevel");
    $.SimpleAjaxCors(url, "POST", "{themeLevel:" + Enum.ThemeLevel.PrimaryLevel + ",page:" + null + "}").done(function (json) {
        var result = $.Deserialize(json.d);
        if (result != null) {
            var temp = "";
            $.each(result, function (index, item) {
                if (item.ID == objPub.UserThemeID) {
                    temp += "<li id='liTheme"+index+"' class='selected'>";
                }
                else {
                    temp += "<li id='liTheme" + index + "'>";
                }
                temp += "<div class='themes-cover'><img src='"+item.ThemeUrl+"'></div>";
                temp += "<div class='themes-name'>"+item.ThemeName+"</div>";
                temp += "</li>";
               
            });
            $("#ulThemeList").empty().append(temp);
            $.each(result, function (index, item) {
                $("#liTheme" + index).on("click", { Index: index }, function (event) {
                    $("#liTheme" + index).data("ThemeID", item.ID);
                    $("li[id^='liTheme']").removeClass("selected");
                    $("#liTheme" + index).addClass("selected");
                });
            });
        }
    });
  
}
//设置
Moments.SetTheme = function set_theme(id) {
    var url = new Array();
    url.push(objPub.ManageUrl);
    url.push("service/FriendsThemeService.asmx/SetMyTheme");
    $.SimpleAjaxCors(url, "POST", "{themeID:'" +id + "'}").done(function (json) {
        var result = json.d;
        if (result == true) {
            $.Alert("设置样式风格成功！", function () {
                objPub.GetTheme(id);
                objPub.UserThemeID = id;
            });
        }
        else {
            console.log("设置样式风格失败！");
        }
    });
   
}


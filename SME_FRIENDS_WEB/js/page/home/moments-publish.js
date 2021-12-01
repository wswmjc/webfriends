Moments.Publish = function () { }
Moments.Publish.registerClass("Moments.Publish");
//上传文件附件列表
Moments.Publish.AccFileList = new Array();
//上传图片附件列表
Moments.Publish.AccPhotoList = new Array();
Moments.Publish.Init = function init_publish() {
    //实例化短篇编辑器
    $("#txtMomentsInfo").miicWebEdit({
        id: "txtMomentsInfo",
        placeholder: "写下你此刻的想法...",
        faceid: "aEmotion",
        submit: "btnMomentsSend",
        facePath: "../../images/arclist/", //表情存放的路径
        charAllowed: 140,
        charWarning: 20,
        charCss: "message_info",
        charCounterElement: "div"
    });
    var um = UM.getEditor("myEditor", {
        imageUrl: "../service/PublishInfoAccUploadHandlerService.ashx?Type=LongInfoImage",
        imagePath: "../file/PublishInfoAcc/LongInfoImage/",
        toolbar: [
            'bold',
            'italic',
            'underline',
            'strikethrough',
            '|',
            'forecolor',
            'backcolor',
            'fontsize',
            '|',
            'justifyleft', //居左对齐
            'justifyright', //居右对齐
            'justifycenter', //居中对齐
              '|',
            'insertorderedlist',
            'insertunorderedlist',
            'image',
            'preview'
        ]
   });
        //长篇附件添加
        var long_editor_acc_str = "";
        long_editor_acc_str += "<div class='editor-top'>";
        long_editor_acc_str += "<form method='post' action='' enctype='multipart/form-data' id='fmLongFile'>";
        long_editor_acc_str += "<input type='file' accept='.doc,.docx,.xls,.xlsx,.ppt,.pptx,.pdf,.txt,.rar,.zip,.xml' name='file' id='txtLongFile'>";
        long_editor_acc_str += "<a href='javascript:;' title='附件'><span class='icon-optSet icon-img icon-link'></span></a>";
        long_editor_acc_str += "</form>";
        long_editor_acc_str += "</div>";
        um.ready(function (editor) {
            $("#sctLong .editor-top").remove();
            $(long_editor_acc_str).insertBefore("#sctLong .edui-toolbar");
            $("#sctLong .edui-container").width("98%");
            //绑定上传事件
            $("#txtLongFile").on("change", { PublishInfoType: Enum.PublishInfoType.Long }, Moments.Publish.FileUploadEvent);
        });

    //监听keyup事件，检测内容
    $("#myEditor,#txtLongTitle").on("keyup", function (event) {//检测特殊字符
        if ($("#txtLongTitle").val() != "" && um.getContent() != "") {
            //长篇提交事件
            $("#btnLongInfoSend,#btnLongInfoSave").attr("disabled", false);
        } else {
            $("#btnLongInfoSend,#btnLongInfoSave").attr("disabled", true);
        }
    });
    um.addListener("contentChange", function () {//正常内容检测
        if ($("#txtLongTitle").val() != "" && um.getContent() != "") {
            //长篇提交事件
            $("#btnLongInfoSend,#btnLongInfoSave").attr("disabled", false);
        } else {
            $("#btnLongInfoSend,#btnLongInfoSave").attr("disabled", true);
        }
    });
   



    $("#btnMomentsCancel").on("click", function (event) {
        $("#sctShort").dialog("close");
    });

    $("#btnMomentsSend").on("click", Moments.Publish.ShortSubmitEvent);
    $("#btnLongInfoSend").on("click", Moments.Publish.LongSubmitEvent);
    $("#btnLongInfoSave").on("click", Moments.Publish.SaveEvent);
    $("#btnLongInfoCancel").on("click", function (event) {
        $("#sctLong").dialog("close");
    });

    //绑定附件上传
    $("#txtMomentsPhoto").on("change", Moments.Publish.PhotoUploadEvent);
    $("#txtMomentsFile").on("change", { PublishInfoType: Enum.PublishInfoType.Short }, Moments.Publish.FileUploadEvent);

    $("input:radio[name='NoticeSource']").on("click", Moments.Publish.NoticeSourceChangeEvent);
    $("input:radio[name='LongNoticeSource']").on("click", Moments.Publish.NoticeSourceLongChangeEvent);
    $("input:radio[name='subchoose']").on("click", Moments.Publish.CommunitySubChangeEvent);
    Moments.Publish.MyAddressbookBind();
}

Moments.Publish.CommunitySubChangeEvent = function CommunitySubChangeEvent(event) {
    var sub_type = parseInt($("input:radio[name='subchoose']:checked").val());
    if (sub_type == Enum.CommunitySubType.Topic) {
        $("#divLabelContains").hide();
    } else if (sub_type == Enum.CommunitySubType.Subject) {
        if ($("#sltCommunityList").val() != "") {
            $("#divLabelContains").show();
            var community_id = $("#sltCommunityList").find("option:checked").data("ID");
            if (community_id) {
                $.SimpleAjaxPost("service/CommunityService.asmx/GetCommunityLabelList", true, "{communityID:'" + community_id + "'}",
                    function (json) {
                        var result = $.Deserialize(json.d);
                        if (Array.isArray(result)) {
                            var temp = "";
                            $.each(result, function (index, item) {
                                temp += "<li>";
                                temp += "<label for='" + item.Name + "'>";
                                temp += "<input type='checkbox' name='Label' id='Label" + index + "' value='" + item.Name + "'>";
                                temp += "<span>"+item.Name+"</span>";
                                temp += "</label>";
                                temp += "</li>";
                            });
                            $("#ulLabelList").empty().append(temp).show();
                            $.each(result, function (index, item) {
                                $("#Label" + index).data("Label", {
                                    LabelID: item.ID,
                                    LabelName: item.Name
                                });
                            });
                            $("input:checkbox[name='Label']").on("click", Moments.Publish.LabelHandleEvent);
                        } else {
                            $("#ulLabelList").empty().hide();
                        }
                    });
            } else {
                $("#ulLabelList").empty().hide();
            }
        } else {
            $("#divLabelContains").hide();
        }
    }
}

Moments.Publish.NoticeSourceChangeEvent = function NoticeSourceChangeEvent(event) {
    var source_type = parseInt($("input:radio[name='NoticeSource']:checked").val());
    // common init
    $("#divLabelContains").hide();
    $("#ulLabelList").empty();
    $("#txtLabel").val("");
    switch (source_type) {
        case Enum.BusinessType.Moments://朋友圈所有
            $("label[for='subchoose']").hide();
            $("input:radio[name='subchoose'][value='" + Enum.CommunitySubType.Topic + "']").prop("checked", true);
            Moments.Publish.MyAddressbookBind();
            break;
        case Enum.BusinessType.Community:
            $("label[for='subchoose']").show();
            Moments.Publish.CommunityBind();
            break;
        case Enum.BusinessType.Group:
            $("label[for='subchoose']").hide();
            $("input:radio[name='subchoose'][value='" + Enum.CommunitySubType.Topic + "']").prop("checked", true);
            Moments.Publish.GroupBind();
            break;
    }
}

//我的通讯录绑定，加载所有好友列表 隐藏组织列表
Moments.Publish.MyAddressbookBind = function my_addressbook_bind() {
    $("#txtAtPerson").val("");//清空@
    $("#sltCommunityList").empty().append("<option>--请选择圈子--</option>").prop("disabled", true);
    $("#sltGroupList").empty().append("<option>--请选择讨论组--</option>").prop("disabled", true);
    var keyword = {
        Keyword: ""
    };
    $.SimpleAjaxPost("service/AddressBookService.asmx/Search", true, "{searchView:" + $.Serialize(keyword) + ",page:" + null + "}",
         function (json) {
             var result = $.Deserialize(json.d);
             if (result!=null) {
                 var temp = "";
                 temp += "<li>";
                 temp += "<label for='AllMembers'>";
                 temp += "<input type='checkbox' id='ckAllNoticers' value='勾选@全部'>";
                 temp += "<span>勾选@全部</span>";
                 temp += "</label>";
                 temp += "</li>";
                 $.each(result, function (index, item) {
                     temp+="<li>";
                     temp+="<label for='"+item.AddresserName+"'>";
                     temp+="<input type='checkbox' name='Noticer' id='Noticer"+index+"' value='"+item.AddresserName+"'>";
                     temp+="<span class='user-select-photo'>";
                     temp += "<img src='" + item.AddresserUrl + "'>";
                     temp+="</span>";
                     temp += "<span>" + item.AddresserName + "</span>";
                     if (item.Remark && item.Remark != "") {
                         temp += "<span class='user-list-memo'>" + item.Remark + "</span>";
                     }
                     temp+="</label>";
                     temp+="</li>";
                 });
                 $("#ulUserList").empty().append(temp).show();
                 $.each(result, function (index, item) {
                     $("#Noticer" + index).data("Noticer", {
                         UserID: item.AddresserID,
                         UserName: item.AddresserName
                     });
                 });
                 $("#ckAllNoticers").on("click", Moments.Publish.AtAllHandlEvent);
                 $("input:checkbox[name='Noticer']").on("click", Moments.Publish.AtHandleEvent);
             } else {
                 $("#ulUserList").empty().hide();
             }
    });
}
//绑定行业圈子
Moments.Publish.CommunityBind = function community_bind() {
    $("#ulUserList").empty().hide();
    $("#sltGroupList").empty().append("<option>--请选择讨论组--</option>").prop("disabled", true);
    var keyword = {
        Keyword: ""
    };
    $.SimpleAjaxPost("service/CommunityService.asmx/Search", true, "{searchView:" + $.Serialize(keyword) + ",page:" + null + "}",
         function (json) {
             var result = $.Deserialize(json.d);
             if (result!=null) {
                 var temp = "<option>--请选择圈子--</option>";
                 $.each(result, function (index, item) {
                     temp += "<option id='Community" + index + "'>" + item.Name + "</option>";
                 });
                 $("#sltCommunityList").empty().append(temp).prop("disabled",false);
                 $.each(result, function (index, item) {
                     $("#Community" + index).data("ID", item.ID);
                 });
                 $("#sltCommunityList").on("change", Moments.Publish.GetAllCommunityMembersEvent);
             } else {
                 $("#sltCommunityList").empty();
             }
         });
}
//获取行业圈子所有成员事件
Moments.Publish.GetAllCommunityMembersEvent = function GetAllCommunityMembersEvent(event) {
    //标签初始化
    $("#divLabelContains").hide();
    $("#ulLabelList").empty();
    $("#txtLabel").val("");
    $("input:radio[name='subchoose'][value='" + Enum.CommunitySubType.Topic + "']").prop("checked", true);

    $("#txtAtPerson").val("");//清空@
    var community_id = $(event.target).find("option:checked").data("ID");
    if (community_id) {
        $.SimpleAjaxPost("service/CommunityService.asmx/GetMemberInfoList", true, "{communityID:'" + community_id + "'}",
            function (json) {
                var result = $.Deserialize(json.d);
                if (result != null) {
                    var temp = "";
                    if ($.isArray(result) == true &&
                     (result.length > 1 || result.length == 1 && result[0].MemberID != objPub.UserID)) {
                        temp += "<li>";
                        temp += "<label for='AllMembers'>";
                        temp += "<input type='checkbox' id='ckAllNoticers' value='勾选@全部'>";
                        temp += "<span>勾选@全部</span>";
                        temp += "</label>";
                        temp += "</li>";
                    }
                    $.each(result, function (index, item) {
                        if (item.MemberID != objPub.UserID) {
                            temp += "<li>";
                            temp += "<label for='" + item.MemberName + "'>";
                            temp += "<input type='checkbox' name='Noticer' id='Noticer" + index + "' value='" + item.MemberName + "'>";
                            temp += "<span class='user-select-photo'>";
                            temp += "<img src='" + item.MemberUrl + "'>";
                            temp += "</span>";
                            temp += "<span>" + item.MemberName + "</span>";
                            if (item.Remark && item.Remark != "") {
                                temp += "<span class='user-list-memo'>" + item.Remark + "</span>";
                            }
                            temp += "</label>";
                            temp += "</li>";
                        }
                    });
                    $("#ulUserList").empty().append(temp).show();
                    $.each(result, function (index, item) {
                        $("#Noticer" + index).data("Noticer", {
                            UserID: item.MemberID,
                            UserName: item.MemberName
                        });
                    });
                    $("#ckAllNoticers").on("click", Moments.Publish.AtAllHandlEvent);
                    $("input:checkbox[name='Noticer']").on("click", Moments.Publish.AtHandleEvent);
                } else {
                    $("#ulUserList").empty().hide();
                }
            });
    } else {
        $("#ulUserList").empty().hide();
    }
}
//讨论组绑定
Moments.Publish.GroupBind = function group_bind() {
    $("#ulUserList").empty().hide();
    $("#sltCommunityList").empty().append("<option>--请选择圈子--</option>").prop("disabled", true);
    var keyword = {
        Keyword: ""
    };
    $.SimpleAjaxPost("service/GroupService.asmx/Search", true, "{searchView:" + $.Serialize(keyword) + ",page:" + null + "}",
         function (json) {
             var result = $.Deserialize(json.d);
             if (result!=null) {
                 var temp = "<option>--请选择讨论组--</option>";
                 $.each(result, function (index, item) {
                     temp += "<option id='Group" + index + "'>" + item.Name + "</option>";
                 });
                 $("#sltGroupList").empty().append(temp).prop("disabled", false);
                 $.each(result, function (index, item) {
                     $("#Group" + index).data("ID", item.ID);
                 });
                 $("#sltGroupList").on("change", Moments.Publish.GetAllGroupMembersEvent);
             } else {
                 $("#sltGroupList").empty();
             }
         });

    $("#divMomentsOrgList").show();
}
//获取所有讨论组员事件
Moments.Publish.GetAllGroupMembersEvent = function GetAllGroupMembersEvent(event) {
    $("#txtAtPerson").val("");//清空@
    var group_id = $(event.target).find("option:checked").data("ID");
    if (group_id) {
        $.SimpleAjaxPost("service/GroupService.asmx/GetMemberInfoList", true, "{groupID:'" + group_id + "'}",
            function (json) {
                var result = $.Deserialize(json.d);
                if (result != null) {
                    var temp = "";
                    if ($.isArray(result) == true &&
                     (result.length > 1 || result.length == 1 && result[0].MemberID != objPub.UserID)) {
                        temp += "<li>";
                        temp += "<label for='AllMembers'>";
                        temp += "<input type='checkbox' id='ckAllNoticers' value='勾选@全部'>";
                        temp += "<span>勾选@全部</span>";
                        temp += "</label>";
                        temp += "</li>";
                    }
                    $.each(result, function (index, item) {
                        if (item.MemberID != objPub.UserID) {
                            temp += "<li>";
                            temp += "<label for='" + item.MemberName + "'>";
                            temp += "<input type='checkbox' name='Noticer' id='Noticer" + index + "' value='" + item.MemberName + "'>";
                            temp += "<span class='user-select-photo'>";
                            temp += "<img src='" + item.MemberUrl + "'>";
                            temp += "</span>";
                            temp += "<span>" + item.MemberName + "</span>";
                            if (item.Remark && item.Remark != "") {
                                temp += "<span class='user-list-memo'>" + item.Remark + "</span>";
                            }
                            temp += "</label>";
                            temp += "</li>";
                        }
                    });
                    $("#ulUserList").empty().append(temp).show();
                    $.each(result, function (index, item) {
                        $("#Noticer" + index).data("Noticer", {
                            UserID: item.MemberID,
                            UserName: item.MemberName
                        });
                    });
                    $("#ckAllNoticers").on("click", Moments.Publish.AtAllHandlEvent);
                    $("input:checkbox[name='Noticer']").on("click", Moments.Publish.AtHandleEvent);
                } else {
                    $("#ulUserList").empty().hide();
                }
            });
    } else {
        $("#ulUserList").empty().hide();
    }
}
//@勾选全部事件
Moments.Publish.AtAllHandlEvent = function AtAllHandlEvent(event) {
    if ($(event.target).is(":checked") == true) {
        $("input:checkbox[name='Noticer']").prop("checked", true);
        $.each($("input:checkbox[name='Noticer']"), function (index, item) {
            $("#txtAtPerson").val($("#txtAtPerson").val() + "@" + $(item).data("Noticer").UserName + ";");
        });
    }
    else {
        $("input:checkbox[name='Noticer']").prop("checked", false);
        $.each($("input:checkbox[name='Noticer']"), function (index, item) {
            var delete_atstr = "@" + $(item).data("Noticer").UserName + ";";
            var replace_reg = new RegExp(delete_atstr);
            $("#txtAtPerson").val($("#txtAtPerson").val().replace(replace_reg, ""));
        });
    }
    $("#txtAtPerson").attr("title", $("#txtAtPerson").val());
}
//@勾选单个事件
Moments.Publish.AtHandleEvent = function AtHandleEvent(event) {
    if ($(event.target).is(":checked")==true) {
        $("#txtAtPerson").val($("#txtAtPerson").val() + "@" + $(event.target).data("Noticer").UserName + ";");
    } else {
        var delete_atstr = "@" + $(event.target).data("Noticer").UserName + ";";
        var replace_reg = new RegExp(delete_atstr);
        $("#txtAtPerson").val($("#txtAtPerson").val().replace(replace_reg, ""));
    }
    $("#txtAtPerson").attr("title", $("#txtAtPerson").val());
}

Moments.Publish.LabelHandleEvent = function LabelHandleEvent(event) {
    if ($(event.target).is(":checked") == true) {
        $("#txtLabel").val($("#txtLabel").val() + $(event.target).data("Label").LabelName + ";");
    } else {
        var delete_labelstr = $(event.target).data("Label").LabelName + ";";
        var replace_reg = new RegExp(delete_labelstr,'g');
        $("#txtLabel").val($("#txtLabel").val().replace(replace_reg, ""));
    }
}

//上传图片
Moments.Publish.PhotoUploadEvent = function PhotoUploadEvent(event) {
    if ($(this).val() != "") {
        if ((Moments.Publish.AccPhotoList.length + 1) <= 9) {//上限为9张
            $("#fmMomentsPhoto").ajaxSubmit({
                url: "../service/PublishInfoAccUploadHandlerService.ashx?Type=Photo",
                type: "post",
                dataType: "json",
                timeout: 600000,
                success: function (data, textStatus) {
                    if (data.result == true) {
                        var photo = {
                            ID: $.NewGuid(),
                            FileName: data.acc.FileName,
                            FilePath: data.acc.FilePath,
                            FileExt: data.acc.FileExt,
                            TempPath: data.acc.TempPath
                        };
                        Moments.Publish.AccPhotoList.push(photo);
                        var temp = "<div class='editor-pics' id='divUploadImage" + Moments.Publish.AccPhotoList.length + "'>";
                        temp += " <img src='" + photo.TempPath + "'>";
                        temp += "<a id='aRemoveUploadImage" + Moments.Publish.AccPhotoList.length + "' href='javascript:void(0);' class='img-del'><span class='icon-optSet icon-img icon-opt-delete'></span><span class='opt-text'>删除</span></a>";
                        temp += "</div>";
                        $(".editor-pics-block").append(temp).show();
                        $("#aRemoveUploadImage" + Moments.Publish.AccPhotoList.length).on("click", { IsPhoto: Enum.YesNo.Yes, AccItem: photo, ID: "divUploadImage" + Moments.Publish.AccPhotoList.length }, Moments.Publish.DeleteTempAccEvent);
                    } else {
                        $.Alert(data.message);
                    }
                },
                error: function (data, status, e) {
                    console.log("上传失败，错误信息：" + e);
                }
            });
        } else {
            $.Alert("图片上传上限为9张");
        }
    }
}
//上传附件
Moments.Publish.FileUploadEvent = function FileUploadEvent(event) {
    var info_type = event.data.PublishInfoType;
    if ($(this).val() != "") {
        var $fm;
        if (info_type == Enum.PublishInfoType.Short) {
            $fm = $("#fmMomentsFile");
        } else if (info_type == Enum.PublishInfoType.Long) {
            $fm = $("#fmLongFile");
        }

        $fm.ajaxSubmit({
            url: "../service/PublishInfoAccUploadHandlerService.ashx?Type=File",
            type: "post",
            dataType: "json",
            timeout: 600000,
            success: function (data, textStatus) {
                if (data.result == true) {
                    var file = {
                        ID: $.NewGuid(),
                        FileName: data.acc.FileName,
                        FilePath: data.acc.FilePath,
                        FileExt: data.acc.FileExt,
                        FileType: data.acc.FileType,
                        TempPath: data.acc.TempPath
                    };
                    Moments.Publish.AccFileList.push(file);
                    var temp = "<div class='user-attachment' id='divUploadFile" + Moments.Publish.AccFileList.length + "'>";
                    temp += "<a target='_blank' href='#'><span class='icon-optSet icon-img " + Enum.FileType.GetIconClass(file.FileType) + "'></span></a>";
                    temp += "<a href='" + file.TempPath + "' title='" + file.FileName + "'>" + file.FileName + "." + file.FileExt + "</a>";
                    temp += "<div id='divRemoveUploadFile" + Moments.Publish.AccFileList.length + "' class='user-attachment-del'><span class='icon-optSet icon-img icon-opt-delete'></span><span class='opt-text'>删除</span></div>";
                    temp += "</div>";
                    if (info_type == Enum.PublishInfoType.Short) {
                        $("#divShortFileList").append(temp).show();
                    } else if (info_type == Enum.PublishInfoType.Long) {
                        $("#divLongFileList").append(temp).show();
                    }
                    $("#divRemoveUploadFile" + Moments.Publish.AccFileList.length).on("click", { IsPhoto: Enum.YesNo.No, AccItem: file, ID: "divUploadFile" + Moments.Publish.AccFileList.length, PublishInfoType: info_type }, Moments.Publish.DeleteTempAccEvent);
                } else {
                    $.Alert({ content: data.message ,width:"auto"});
                }
            },
            error: function (data, status, e) {
                console.log("上传失败，错误信息：" + e);
            }
        });
    }
}


//删除附件事件
Moments.Publish.DeleteTempAccEvent = function DeleteTempAccEvent(event) {
    var is_photo = event.data.IsPhoto;
    var acc_item = event.data.AccItem;
    var info_type = event.data.PublishInfoType;
    var id = event.data.ID;
    $.SimpleAjaxPostWithError("service/MomentsService.asmx/DeleteTempFile", true,"{tempPath:'" + acc_item.TempPath + "'}",
            function (x, e) {
                if (x.responseText === undefined) {
                    console.log(x.responseText);
                }
                else {
                    console.log($.Deserialize(x.responseText, true).Message);
                }
            }).done(function (json) {
                var result = json.d;
                if (result == true) {
                    $("#" + id).remove();
                    if (is_photo == Enum.YesNo.Yes) {
                        if ($(".editor-pics-block").find(".img-del").length == 0) {
                            $(".editor-pics-block").hide();
                        }
                        var find_index = 0;
                        $.each(Moments.Publish.AccPhotoList, function (index, item) {
                            if (item.ID == acc_item.ID) {
                                find_index = index;
                            }
                        });
                        Moments.Publish.AccPhotoList.splice(find_index, 1);
                    } else if (is_photo == Enum.YesNo.No) {
                        if (info_type == Enum.PublishInfoType.Short) {
                            if ($("#divShortImageList").find(".user-attachment-del").length == 0) {
                                $("#divShortImageList").hide();
                            }
                        } else if (info_type == Enum.PublishInfoType.Long) {
                            if ($("#divLongImageList").find(".user-attachment-del").length == 0) {
                                $("#divLongImageList").hide();
                            }
                        }                       
                        var find_index = 0;
                        $.each(Moments.Publish.AccFileList, function (index, item) {
                            if (item.ID == acc_item.ID) {
                                find_index = index;
                            }
                        });
                        Moments.Publish.AccFileList.splice(find_index, 1);
                    }
                }
            });
}

Moments.Publish.ResetShortEditorEvent = function ResetEditorEvent(event) {
    $(this).parent().find(".ui-dialog-titlebar-close").hide();
    $(this).dialog("option", "appendTo", ".dialog-mask");
    $("html").css("overflow-y", "hidden");
    $(".ui-dialog").css({
        "position": "absolute",
        "top": "0"
    });
    $(".dialog-mask").show().css({
        "overflow-y": "scroll",
        "height": $(window).height()
    }).scrollTop(0);
    if (window.removeEventListener) {
        window.removeEventListener("DOMMouseScroll", wheel, false);
    }
    window.onmousewheel = document.onmousewheel = null;
    $("#txtMomentsInfo").setContents("");
    //清空图片、文件
    $(".editor-pics-block,.editor-attachment-block").empty();
    Moments.Publish.AccPhotoList = new Array();
    Moments.Publish.AccFileList = new Array();
    //清空上传接口
    $("#txtMomentsPhoto,#txtMomentsFile").val("");
    $("input:radio[name='NoticeSource'][value='" + Enum.BusinessType.Moments + "']").prop("checked", true);
    // common init
    $("#divLabelContains").hide();
    $("#ulLabelList").empty();
    $("#txtLabel").val("");
    $("label[for='subchoose']").hide();
    $("input:radio[name='subchoose'][value='" + Enum.CommunitySubType.Topic + "']").prop("checked", true);

    Moments.Publish.MyAddressbookBind();
}
//短篇发布事件
Moments.Publish.ShortSubmitEvent = function ShortSubmitEvent(event) {
    var publish_type = Enum.PublishInfoType.Short;
    var source_type = parseInt($("input:radio[name='NoticeSource']:checked").val());
    var now_date = new Date().format("yyyy-MM-dd HH:mm:ss");

    var noticers = new Array();

    //读取提醒人
    $.each($("input:checkbox[name='Noticer']"), function (index, item) {
        if ($(item).is(":checked") == true) {
            noticers.push($(item).data("Noticer"));
        }
    });

    var notice_user_view = {
        NoticeType: Enum.NoticeType.PublishInfo,
        NoticeSource: source_type,
        Noticers: noticers
    };

    if (source_type == Enum.BusinessType.Moments) {
        var publish_info = {
            ID: $.NewGuid(),
            Content: $("#txtMomentsInfo").getContents(),
            PublishType: publish_type.toString()
        };
        var accs = new Array();
        //读取附件
        //添加文件附件
        $.each(Moments.Publish.AccFileList, function (index, item) {
            var acc_item = {
                ID: item.ID,
                FileName: item.FileName + "." + item.FileExt,
                FileType: item.FileType.toString(),
                FilePath: item.FilePath,
                PublishID: publish_info.ID,
                UploadTime: now_date
            };
            accs.push(acc_item);
        });
        //添加图片附件
        $.each(Moments.Publish.AccPhotoList, function (index, item) {
            var acc_item = {
                ID: item.ID,
                FileName: item.FileName + "." + item.FileExt,
                FileType: Enum.AccType.Photo.toString(),
                FilePath: item.FilePath,
                PublishID: publish_info.ID,
                UploadTime: now_date
            };
            accs.push(acc_item);
        });

        $.SimpleAjaxPost("service/MomentsService.asmx/Submit",
                         true,
                         "{publishInfo:" + $.Serialize(publish_info) +
                            ",noticeUserView:" + $.Serialize(notice_user_view, true) +
                             ",publishAccessoryInfos:" + $.Serialize(accs, true) +
                              ",removeSimpleAccessoryViews:" + null + "}",
                         function (json) {
                             var result = json.d;
                             if (result == true) {
                                 //if ($.isArray(notice_user_view.Noticers) == true && notice_user_view.Noticers.length > 0) {
                                 //    shareHubProxy.invoke("SendMessage", publish_info.ID, publish_info.Content, notice_user_view);
                                 //}
                                 if (Moments.IsPerson == true) {
                                     Moments.List.Person.SetDateList();
                                   
                                 } else {
                                     Moments.List.SetDateList();
                                 }
                                 //$("#aNewestButton").trigger("click");//Moments.List.Init();
                                 Moments.GetPersonStatisticsCount(objPub.UserID);
                                 $("#sctShort").dialog("close");
                             }
                         });
    } else if (source_type == Enum.BusinessType.Community) {
        var sub_type = parseInt($("input:radio[name='subchoose']:checked").val());
        if (sub_type == Enum.CommunitySubType.Topic) {
            var topic_info = {
                ID: $.NewGuid(),
                TopicContent: $("#txtMomentsInfo").getContents(),
                CommunityID: $("#sltCommunityList").find("option:checked").data("ID")
            };

            if (!topic_info.CommunityID) {
                $.Alert("请选择圈子");
                return;
            } else {
                $.SimpleAjaxPost("service/CommunityService.asmx/TopicSubmit",
                        true,
                        "{topicInfo:" + $.Serialize(topic_info) +
                           ",noticeUserView:" + $.Serialize(notice_user_view, true) + "}",
                        function (json) {
                            var result = json.d;
                            if (result == true) {
                                //if ($.isArray(notice_user_view.Noticers) == true && notice_user_view.Noticers.length > 0) {
                                //    shareHubProxy.invoke("SendMessage", topic_info.ID, topic_info.TopicContent, notice_user_view);
                                //}
                                //$("#aNewestButton").trigger("click");//Moments.List.Init();
                                Moments.GetPersonStatisticsCount(objPub.UserID);
                                $("#sctShort").dialog("close");
                            }
                        });
            }
        } else if (sub_type == Enum.CommunitySubType.Subject) {
            var publish_info = {
                ID: $.NewGuid(),
                Content: $("#txtMomentsInfo").getContents(),
                PublishType: publish_type.toString()
            };
            var community_id = $("#sltCommunityList").find("option:checked").data("ID")

            if (!community_id) {
                $.Alert("请选择圈子");
                return;
            }

            var accs = new Array();
            //读取附件
            //添加文件附件
            $.each(Moments.Publish.AccFileList, function (index, item) {
                var acc_item = {
                    ID: item.ID,
                    FileName: item.FileName + "." + item.FileExt,
                    FileType: item.FileType.toString(),
                    FilePath: item.FilePath,
                    PublishID: publish_info.ID,
                    UploadTime: now_date
                };
                accs.push(acc_item);
            });
            //添加图片附件
            $.each(Moments.Publish.AccPhotoList, function (index, item) {
                var acc_item = {
                    ID: item.ID,
                    FileName: item.FileName + "." + item.FileExt,
                    FileType: Enum.AccType.Photo.toString(),
                    FilePath: item.FilePath,
                    PublishID: publish_info.ID,
                    UploadTime: now_date
                };
                accs.push(acc_item);
            });

            var labels = new Array();

            //读取提醒人
            $.each($("input:checkbox[name='Label']"), function (index, item) {
                if ($(item).is(":checked") == true) {
                    var label_item = {
                        LabelID: $(item).data("Label").LabelID,
                        LabelName: $(item).data("Label").LabelName,
                        CommunityID: community_id
                    };
                    labels.push(label_item);
                }
            });

            if (labels.length == 0) {
                $.Alert("请选择话题标签");
                return;
            }

            $.SimpleAjaxPost("service/CommunityService.asmx/Submit",
                             true,
                             "{publishInfo:" + $.Serialize(publish_info) +
                                ",noticeUserView:" + $.Serialize(notice_user_view, true) +
                                 ",publishAccessoryInfos:" + $.Serialize(accs, true) +
                                 ",simpleLabelViews:" + $.Serialize(labels, true) +
                                  ",removeSimpleAccessoryViews:" + null +
                                  ",removeSimpleLabelViewIDs:" + null + "}",
                             function (json) {
                                 var result = json.d;
                                 if (result == true) {
                                     //if ($.isArray(notice_user_view.Noticers) == true && notice_user_view.Noticers.length > 0) {
                                     //    shareHubProxy.invoke("SendMessage", publish_info.ID, publish_info.Content, notice_user_view);
                                     //}
                                    // $("#aNewestButton").trigger("click");//Moments.List.Init();
                                     Moments.GetPersonStatisticsCount(objPub.UserID);
                                     $("#sctShort").dialog("close");
                                 }
                             });
        }

    } else if (source_type == Enum.BusinessType.Group) {
        var topic_info = {
            ID: $.NewGuid(),
            TopicContent: $("#txtMomentsInfo").getContents(),
            GroupID: $("#sltGroupList").find("option:checked").data("ID")
        };

        if (!topic_info.GroupID) {
            $.Alert("请选择讨论组");
            return;
        } else {
            $.SimpleAjaxPost("service/GroupService.asmx/Submit",
                    true,
                    "{topicInfo:" + $.Serialize(topic_info) +
                       ",noticeUserView:" + $.Serialize(notice_user_view, true) + "}",
                    function (json) {
                        var result = json.d;
                        if (result == true) {
                            //if ($.isArray(notice_user_view.Noticers) == true && notice_user_view.Noticers.length > 0) {
                            //    shareHubProxy.invoke("SendMessage", topic_info.ID, topic_info.TopicContent, notice_user_view);
                            //}
                            Moments.GetPersonStatisticsCount(objPub.UserID);
                            if ($("a[name='listGroup']").hasClass("selected")) {
                                //如果当前在朋友圈主页面且讨论组被选中 那么发表信息 则刷新讨论组>> 通过触发讨论组tab刷新
                                $("a[name='listGroup']").trigger("click");
                            }
                            $("#sctShort").dialog("close");
                        }
                    });
        }
    }
}

Moments.Publish.SuccesCallBack = function success_call_back() {
    Moments.List.Init();
}

//长篇信息发表
Moments.Publish.ResetLongEditorEvent = function ResetLongEditorEvent(event) {
    //取消右上角的X
    $(this).parent().find(".ui-dialog-titlebar-close").hide();
    //标题清空
    $("#txtLongTitle").val("");
    //内容清空
    UM.getEditor("myEditor").setContent("");

    //清空文件
    $("#divLongFileList").empty();
    Moments.Publish.AccPhotoList = new Array();
    Moments.Publish.AccFileList = new Array();

    //清空上传接口
    $("#txtLongFile").val("");
    $("input:radio[name='LongNoticeSource'][value='" + Enum.BusinessType.Moments + "']").prop("checked", true);

    // common init
    $("#divLongLabelContains").hide();
    $("#ulLongLabelList").empty();
    $("#txtLongLabel").val("");

    Moments.Publish.MyAddressbookLongBind();
}


Moments.Publish.NoticeSourceLongChangeEvent = function NoticeSourceLongChangeEvent(event) {
    var source_type = parseInt($("input:radio[name='LongNoticeSource']:checked").val());
    // common init
    $("#divLongLabelContains").hide();
    $("#ulLongLabelList").empty();
    $("#txtLongLabel").val("");
    switch (source_type) {
        case Enum.BusinessType.Moments://朋友圈所有
            Moments.Publish.MyAddressbookLongBind();
            break;
        case Enum.BusinessType.Community:
            $("label[for='subchoose']").show();
            Moments.Publish.CommunityLongBind();
            break;
    }
}

//我的通讯录绑定，加载所有好友列表 隐藏组织列表
Moments.Publish.MyAddressbookLongBind = function my_addressbook_long_bind() {
    $("#txtLongAtPerson").val("");//清空@
    $("#sltLongCommunityList").empty().append("<option>--请选择圈子--</option>").prop("disabled", true);
    var keyword = {
        Keyword: ""
    };
    $.SimpleAjaxPost("service/AddressBookService.asmx/Search", true, "{searchView:" + $.Serialize(keyword) + ",page:" + null + "}",
         function (json) {
             var result = $.Deserialize(json.d);
             if (result != null) {
                 var temp = "";
                 temp += "<li>";
                 temp += "<label for='AllMembers'>";
                 temp += "<input type='checkbox' id='ckAllLongNoticers' value='勾选@全部'>";
                 temp += "<span>勾选@全部</span>";
                 temp += "</label>";
                 temp += "</li>";
                 $.each(result, function (index, item) {
                     temp += "<li>";
                     temp += "<label for='" + item.AddresserName + "'>";
                     temp += "<input type='checkbox' name='ckLongNoticer' id='ckLongNoticer" + index + "' value='" + item.AddresserName + "'>";
                     temp += "<span class='user-select-photo'>";
                     temp += "<img src='" + item.AddresserUrl + "'>";
                     temp += "</span>";
                     temp += "<span>" + item.AddresserName + "</span>";
                     if (item.Remark && item.Remark != "") {
                         temp += "<span class='user-list-memo'>" + item.Remark + "</span>";
                     }
                     temp += "</label>";
                     temp += "</li>";
                 });
                 $("#ulLongUserList").empty().append(temp).show();
                 $.each(result, function (index, item) {
                     $("#ckLongNoticer" + index).data("Noticer", {
                         UserID: item.AddresserID,
                         UserName: item.AddresserName
                     });
                 });
                 $("#ckAllLongNoticers").on("click", Moments.Publish.AtAllHandlLongEvent);
                 $("input:checkbox[name='ckLongNoticer']").on("click", Moments.Publish.AtHandleLongEvent);
             } else {
                 $("#ulLongUserList").empty().hide();
             }
         });
}
//绑定行业圈子
Moments.Publish.CommunityLongBind = function community_long_bind() {
    $("#ulLongUserList").empty().hide();
    var keyword = {
        Keyword: ""
    };
    $.SimpleAjaxPost("service/CommunityService.asmx/Search", true, "{searchView:" + $.Serialize(keyword) + ",page:" + null + "}",
         function (json) {
             var result = $.Deserialize(json.d);
             if (result != null) {
                 var temp = "<option>--请选择圈子--</option>";
                 $.each(result, function (index, item) {
                     temp += "<option id='Community-long" + index + "'>" + item.Name + "</option>";
                 });
                 $("#sltLongCommunityList").empty().append(temp).prop("disabled", false);
                 $.each(result, function (index, item) {
                     $("#Community-long" + index).data("ID", item.ID);
                 });
                 $("#sltLongCommunityList").on("change", Moments.Publish.GetAllCommunityMembersLongEvent);
             } else {
                 $("#sltLongCommunityList").empty();
             }
         });
}

Moments.Publish.GetAllCommunityMembersLongEvent = function GetAllCommunityMembersLongEvent(event) {
    //标签初始化
    $("#divLongLabelContains").hide();
    $("#ulLongLabelList").empty();
    $("#txtLongLabel").val("");

    $("#txtLongAtPerson").val("");//清空@
    var community_id = $(event.target).find("option:checked").data("ID");
    if (community_id) {
        $("#divLongLabelContains").show();
        //读取标签
        $.SimpleAjaxPost("service/CommunityService.asmx/GetCommunityLabelList", true, "{communityID:'" + community_id + "'}",
                   function (json) {
                       var result = $.Deserialize(json.d);
                       if (Array.isArray(result)) {
                           var temp = "";
                           $.each(result, function (index, item) {
                               temp += "<li>";
                               temp += "<label for='" + item.Name + "'>";
                               temp += "<input type='checkbox' name='ckLongLabel' id='ckLongLabel" + index + "' value='" + item.Name + "'>";
                               temp += "<span>" + item.Name + "</span>";
                               temp += "</label>";
                               temp += "</li>";
                           });
                           $("#ulLongLabelList").empty().append(temp).show();
                           $.each(result, function (index, item) {
                               $("#ckLongLabel" + index).data("Label", {
                                   LabelID: item.ID,
                                   LabelName: item.Name
                               });
                           });
                           $("input:checkbox[name='ckLongLabel']").on("click", Moments.Publish.LabelHandleLongEvent);
                       } else {
                           $("#ulLongLabelList").empty().hide();
                       }
                   });

        //读取人
        $.SimpleAjaxPost("service/CommunityService.asmx/GetMemberInfoList", true, "{communityID:'" + community_id + "'}",
            function (json) {
                var result = $.Deserialize(json.d);
                if (result != null) {
                    var temp = "";
                    var temp = "";
                    if ($.isArray(result) == true &&
                        (result.length > 1 || result.length == 1 && result[0].MemberID != objPub.UserID)) {
                        temp += "<li>";
                        temp += "<label for='AllMembers'>";
                        temp += "<input type='checkbox' id='ckAllLongNoticers' value='勾选@全部'>";
                        temp += "<span>勾选@全部</span>";
                        temp += "</label>";
                        temp += "</li>";
                    }
                    $.each(result, function (index, item) {
                        if (item.MemberID != objPub.UserID) {
                            temp += "<li>";
                            temp += "<label for='" + item.MemberName + "'>";
                            temp += "<input type='checkbox' name='ckLongNoticer' id='ckLongNoticer" + index + "' value='" + item.MemberName + "'>";
                            temp += "<span class='user-select-photo'>";
                            temp += "<img src='" + item.MemberUrl + "'>";
                            temp += "</span>";
                            temp += "<span>" + item.MemberName + "</span>";
                            if (item.Remark && item.Remark != "") {
                                temp += "<span class='user-list-memo'>" + item.Remark + "</span>";
                            }
                            temp += "</label>";
                            temp += "</li>";
                        }
                    });
                    $("#ulLongUserList").empty().append(temp).show();
                    $.each(result, function (index, item) {
                        $("#ckLongNoticer" + index).data("Noticer", {
                            UserID: item.MemberID,
                            UserName: item.MemberName
                        });
                    });
                    $("#ckAllLongNoticers").on("click", Moments.Publish.AtAllHandlLongEvent);
                    $("input:checkbox[name='ckLongNoticer']").on("click", Moments.Publish.AtHandleLongEvent);
                } else {
                    $("#ulLongUserList").empty().hide();
                }
            });
    } else {
        $("#ulLongUserList,#ulLongLabelList").empty().hide();
        $("#divLongLabelContains").hide();
    }
}
//@勾选全部事件
Moments.Publish.AtAllHandlLongEvent = function AtAllHandlLongEvent(event) {
    if ($(event.target).is(":checked") == true) {
        $("input:checkbox[name='ckLongNoticer']").prop("checked", true);
        $.each($("input:checkbox[name='ckLongNoticer']"), function (index, item) {
            $("#txtLongAtPerson").val($("#txtLongAtPerson").val() + "@" + $(item).data("Noticer").UserName + ";");
        });
    }
    else {
        $("input:checkbox[name='ckLongNoticer']").prop("checked", false);
        $.each($("input:checkbox[name='ckLongNoticer']"), function (index, item) {
            var delete_atstr = "@" + $(item).data("Noticer").UserName + ";";
            var replace_reg = new RegExp(delete_atstr);
            $("#txtLongAtPerson").val($("#txtLongAtPerson").val().replace(replace_reg, ""));
        });
    }
    $("#txtLongAtPerson").attr("title", $("#txtLongAtPerson").val());
}
//@勾选单个事件
Moments.Publish.AtHandleLongEvent = function AtHandleLongEvent(event) {
    if ($(event.target).is(":checked") == true) {
        $("#txtLongAtPerson").val($("#txtLongAtPerson").val() + "@" + $(event.target).data("Noticer").UserName + ";");
    } else {
        var delete_atstr = "@" + $(event.target).data("Noticer").UserName + ";";
        var replace_reg = new RegExp(delete_atstr);
        $("#txtLongAtPerson").val($("#txtLongAtPerson").val().replace(replace_reg, ""));
    }
    $("#txtLongAtPerson").attr("title", $("#txtLongAtPerson").val());
}

Moments.Publish.LabelHandleLongEvent = function LabelHandleLongEvent(event) {
    if ($(event.target).is(":checked") == true) {
        $("#txtLongLabel").val($("#txtLongLabel").val() + $(event.target).data("Label").LabelName + ";");
    } else {
        var delete_labelstr = $(event.target).data("Label").LabelName + ";";
        var replace_reg = new RegExp(delete_labelstr, 'g');
        $("#txtLongLabel").val($("#txtLongLabel").val().replace(replace_reg, ""));
    }
}

Moments.Publish.LongSubmitEvent = function LongSubmitEvent(event) {
    var publish_type = Enum.PublishInfoType.Long;
    var source_type = parseInt($("input:radio[name='LongNoticeSource']:checked").val());
    var now_date = new Date().format("yyyy-MM-dd HH:mm:ss");

    var noticers = new Array();

    //读取提醒人
    $.each($("input:checkbox[name='ckLongNoticer']"), function (index, item) {
        if ($(item).is(":checked") == true) {
            noticers.push($(item).data("Noticer"));
        }
    });

    var notice_user_view = {
        NoticeType: Enum.NoticeType.PublishInfo,
        NoticeSource: source_type,
        Noticers: noticers
    };

    if (source_type == Enum.BusinessType.Moments) {
        var publish_info = {
            ID: $.NewGuid(),
            Title:$("#txtLongTitle").val(),
            Content: UM.getEditor("myEditor").getContent(),
            PublishType: publish_type.toString(),
            EditStatus: Enum.YesNo.No.toString(),
            PublishTime:now_date
        };
        var accs = new Array();
        //读取附件
        //添加文件附件
        $.each(Moments.Publish.AccFileList, function (index, item) {
            var acc_item = {
                ID: item.ID,
                FileName: item.FileName + "." + item.FileExt,
                FileType: item.FileType.toString(),
                FilePath: item.FilePath,
                PublishID: publish_info.ID,
                UploadTime: now_date
            };
            accs.push(acc_item);
        });

        $.SimpleAjaxPost("service/MomentsService.asmx/Submit",
                         true,
                         "{publishInfo:" + $.Serialize(publish_info) +
                            ",noticeUserView:" + $.Serialize(notice_user_view, true) +
                             ",publishAccessoryInfos:" + $.Serialize(accs, true) +
                              ",removeSimpleAccessoryViews:" + null + "}",
                         function (json) {
                             var result = json.d;
                             if (result == true) {
                                 //if ($.isArray(notice_user_view.Noticers) == true && notice_user_view.Noticers.length > 0) {
                                 //    shareHubProxy.invoke("SendMessage", publish_info.ID, publish_info.Title, notice_user_view);
                                 //}
                                 if (Moments.IsPerson == true) {
                                     Moments.List.Person.SetDateList();
                                     
                                 } else {
                                     Moments.List.SetDateList();
                                 }
                                 Moments.GetPersonStatisticsCount(objPub.UserID);
                                 $("#sctLong").dialog("close");
                             }
                         });
    } else if (source_type == Enum.BusinessType.Community) {
            var publish_info = {
                ID: $.NewGuid(),
                Title: $("#txtLongTitle").val(),
                Content: UM.getEditor("myEditor").getContent(),
                PublishType: publish_type.toString(),
                EditStatus: Enum.YesNo.No.toString(),
                PublishTime: now_date
            };
            var community_id = $("#sltLongCommunityList").find("option:checked").data("ID")

            if (!community_id) {
                $.Alert("请选择圈子~");
                return;
            }

            var accs = new Array();
            //读取附件
            //添加文件附件
            $.each(Moments.Publish.AccFileList, function (index, item) {
                var acc_item = {
                    ID: item.ID,
                    FileName: item.FileName + "." + item.FileExt,
                    FileType: item.FileType.toString(),
                    FilePath: item.FilePath,
                    PublishID: publish_info.ID,
                    UploadTime: now_date
                };
                accs.push(acc_item);
            });
          
            var labels = new Array();

            //读取标签
            $.each($("input:checkbox[name='ckLongLabel']"), function (index, item) {
                if ($(item).is(":checked") == true) {
                    var label_item = {
                        LabelID: $(item).data("Label").LabelID,
                        LabelName: $(item).data("Label").LabelName,
                        CommunityID: community_id
                    };
                    labels.push(label_item);
                }
            });

            if (labels.length == 0) {
                $.Alert("请选择话题标签~");
                return;
            }

            $.SimpleAjaxPost("service/CommunityService.asmx/Submit",
                             true,
                             "{publishInfo:" + $.Serialize(publish_info) +
                                ",noticeUserView:" + $.Serialize(notice_user_view, true) +
                                 ",publishAccessoryInfos:" + $.Serialize(accs, true) +
                                 ",simpleLabelViews:" + $.Serialize(labels, true) +
                                  ",removeSimpleAccessoryViews:" + null +
                                  ",removeSimpleLabelViewIDs:" + null + "}",
                             function (json) {
                                 var result = json.d;
                                 if (result == true) {
                                     //if ($.isArray(notice_user_view.Noticers) == true && notice_user_view.Noticers.length > 0) {
                                     //    shareHubProxy.invoke("SendMessage", publish_info.ID, publish_info.Title, notice_user_view);
                                     //}
                                     Moments.GetPersonStatisticsCount(objPub.UserID);
                                     $("#sctLong").dialog("close");
                                 }
                             });
    } 
}

    //临时保存博客
Moments.Publish.SaveEvent = function SaveEvent(event) {
    var publish_type = Enum.PublishInfoType.Long;
    var source_type = parseInt($("input:radio[name='LongNoticeSource']:checked").val());
    var now_date = new Date().format("yyyy-MM-dd HH:mm:ss");

    var noticers = new Array();

    if (source_type == Enum.BusinessType.Moments) {
        var publish_info = {
            ID: $.NewGuid(),
            Title: $("#txtLongTitle").val(),
            Content: UM.getEditor("myEditor").getContent(),
            PublishType: publish_type.toString(),
            EditStatus: Enum.YesNo.Yes.toString()
        };
        var accs = new Array();
        //读取附件
        //添加文件附件
        $.each(Moments.Publish.AccFileList, function (index, item) {
            var acc_item = {
                ID: item.ID,
                FileName: item.FileName + "." + item.FileExt,
                FileType: item.FileType.toString(),
                FilePath: item.FilePath,
                PublishID: publish_info.ID,
                UploadTime: now_date
            };
            accs.push(acc_item);
        });

        $.SimpleAjaxPost("service/MomentsService.asmx/Submit",
                         true,
                         "{publishInfo:" + $.Serialize(publish_info) +
                            ",noticeUserView:" + null +
                             ",publishAccessoryInfos:" + $.Serialize(accs, true) +
                              ",removeSimpleAccessoryViews:" + null + "}",
                         function (json) {
                             var result = json.d;
                             if (result == true) {
                                 Moments.GetPersonStatisticsCount(objPub.UserID);
                                 $("#sctLong").dialog("close");
                             }
                         });
    } else if (source_type == Enum.BusinessType.Community) {
        var publish_info = {
            ID: $.NewGuid(),
            Title: $("#txtLongTitle").val(),
            Content: UM.getEditor("myEditor").getContent(),
            PublishType: publish_type.toString(),
            EditStatus: Enum.YesNo.Yes.toString()
        };
        var community_id = $("#sltLongCommunityList").find("option:checked").data("ID")

        if (!community_id) {
            $.Alert("请选择圈子~");
            return;
        }

        var accs = new Array();
        //读取附件
        //添加文件附件
        $.each(Moments.Publish.AccFileList, function (index, item) {
            var acc_item = {
                ID: item.ID,
                FileName: item.FileName + "." + item.FileExt,
                FileType: item.FileType.toString(),
                FilePath: item.FilePath,
                PublishID: publish_info.ID,
                UploadTime: now_date
            };
            accs.push(acc_item);
        });

        var labels = new Array();

        //读取标签
        $.each($("input:checkbox[name='ckLongLabel']"), function (index, item) {
            if ($(item).is(":checked") == true) {
                var label_item = {
                    LabelID: $(item).data("Label").LabelID,
                    LabelName: $(item).data("Label").LabelName,
                    CommunityID: community_id
                };
                labels.push(label_item);
            }
        });

        if (labels.length == 0) {
            $.Alert("请选择话题标签~");
            return;
        }
        $.SimpleAjaxPost("service/CommunityService.asmx/Submit",
                         true,
                         "{publishInfo:" + $.Serialize(publish_info) +
                            ",noticeUserView:" + null +
                             ",publishAccessoryInfos:" + $.Serialize(accs, true) +
                             ",simpleLabelViews:" + $.Serialize(labels, true) +
                              ",removeSimpleAccessoryViews:" + null +
                              ",removeSimpleLabelViewIDs:" + null + "}",
                         function (json) {
                             var result = json.d;
                             if (result == true) {
                                 Moments.GetPersonStatisticsCount(objPub.UserID);
                                 $("#sctLong").dialog("close");
                             }
                         });
    }
}
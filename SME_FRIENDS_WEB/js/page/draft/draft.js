Draft = function () { }
Draft.registerClass("Draft");
Draft.PageSize = 10;
Draft.AccFileList = new Array();
Draft.RemoveAccFileList = new Array();
Draft.InitCommunity = true;

Draft.Init = function init() {
    $("#divTimeAxis").hide();//隐藏时间轴
    $(".friend-content").ReadTemplate(Template.DraftListTpl, function () {
        //切换Tab
        $("#ulDraftType>li").off("click").on("click", Draft.SetTypeTabEvent);
        Draft.Moments.Init();
        $("#aGoBack").off("click").on("click", Draft.GoBackEvent);
        //实例化长篇编辑器
        var um = UM.getEditor("draftEditor", {
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
        var draft_editor_acc_str = "";
        draft_editor_acc_str += "<div class='editor-top'>";
        draft_editor_acc_str += "<form method='post' action='' enctype='multipart/form-data' id='fmDraftFile'>";
        draft_editor_acc_str += "<input type='file' accept='.doc,.docx,.xls,.xlsx,.ppt,.pptx,.pdf,.txt,.rar,.zip,.xml' name='txtDraftFile' id='txtDraftFile'>";
        draft_editor_acc_str += "<a href='javascript:;' title='附件'><span class='icon-optSet icon-img icon-link'></span></a>";
        draft_editor_acc_str += "</form>";
        draft_editor_acc_str += "</div>";

        um.ready(function (editor) {
            $("#sctDraft .editor-top").remove();
            $(draft_editor_acc_str).insertBefore("#sctDraft .edui-toolbar");
            $("#sctDraft .edui-container").width("98%");
            //绑定上传事件
            $("#txtDraftFile").on("change", Draft.FileUploadEvent);
        });

        //监听keyup事件，检测内容
        $("#draftEditor,#txtDraftTitle").on("keyup", function (event) {//检测特殊字符
            if ($("#txtDraftTitle").val() != "" && um.getContent() != "") {
                //长篇提交事件
                $("#btnLongInfoSend-Draft,#btnLongInfoSave-Draft").attr("disabled", false);
            } else {
                $("#btnLongInfoSend-Draft,#btnLongInfoSave-Draft").attr("disabled", true);
            }
        });

        um.addListener("contentChange", function () {//正常内容检测
            if ($("#txtDraftTitle").val() != "" && um.getContent() != "") {
                //长篇提交事件
                $("#btnLongInfoSend-Draft,#btnLongInfoSave-Draft").attr("disabled", false);
            } else {
                $("#btnLongInfoSend-Draft,#btnLongInfoSave-Draft").attr("disabled", true);
            }
        });
    });
}
//切换Tab事件
Draft.SetTypeTabEvent = function SetTypeTabEvent(event) {
    $(this).addClass("selected").siblings().removeClass("selected");
    if ($(this).attr("name") == "moments") {
        Draft.Moments.Init();
    } else if ($(this).attr("name") == "community") {
        Draft.Community.Init();
    }
}

Draft.GoBackEvent = function GoBackEvent(event) {
    $(".main-left").load("../biz/left/moments.html", function () {
        objPub.IsMain = true;
        Moments.Init(true); 
    });
}

Draft.FileUploadEvent = function FileUploadEvent(event) {
    if ($(this).val() != "") {
        $("#fmDraftFile").ajaxSubmit({
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
                    Draft.AccFileList.push(file);
                    var temp = "<div class='user-attachment' id='divUploadFile" + Draft.AccFileList.length + "'>";
                    temp += "<a target='_blank' href='#'><span class='icon-optSet icon-img " + Enum.FileType.GetIconClass(file.FileType) + "'></span></a>";
                    temp += "<a href='" + file.TempPath + "' title='" + file.FileName + "'>" + file.FileName + "." + file.FileExt + "</a>";
                    temp += "<div id='divRemoveUploadFile" + Draft.AccFileList.length + "' class='user-attachment-del'><span class='icon-optSet icon-img icon-opt-delete'></span><span class='opt-text'>删除</span></div>";
                    temp += "</div>";
                    $("#divDraftFileList").append(temp).show();
                    $("#divRemoveUploadFile" + Draft.AccFileList.length).on("click", { AccItem: file, ID: "divUploadFile" + Draft.AccFileList.length}, Draft.DeleteTempAccEvent);
                } else {
                    $.Alert(data.message);
                }
            },
            error: function (data, status, e) {
                console.log("上传失败，错误信息：" + e);
            }
        });
    }
}

//删除附件事件
Draft.DeleteTempAccEvent = function DeleteTempAccEvent(event) {
    var acc_item = event.data.AccItem;
    var id = event.data.ID;
    $.SimpleAjaxPostWithError("service/MomentsService.asmx/DeleteTempFile", true, "{tempPath:'" + acc_item.TempPath + "'}",
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
                    if ($("#divDraftFileList").find(".user-attachment-del").length == 0) {
                        $("#divDraftFileList").hide();
                    }
                    var find_index = 0;
                    $.each(Draft.AccFileList, function (index, item) {
                        if (item.ID == acc_item.ID) {
                            find_index = index;
                        }
                    });
                    Draft.AccFileList.splice(find_index, 1);
                }
            });
}

//我的通讯录绑定，加载所有好友列表 隐藏组织列表
Draft.MyAddressbookBind = function my_addressbook_bind() {
    $("#txtDraftAtPerson").val("");//清空@
    $("#sltDraftCommunityList").empty().append("<option>--请选择圈子--</option>");
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
                 temp += "<input type='checkbox' id='ckAllDraftNoticers' value='勾选@全部'>";
                 temp += "<span>勾选@全部</span>";
                 temp += "</label>";
                 temp += "</li>";
                 $.each(result, function (index, item) {
                     temp += "<li>";
                     temp += "<label for='" + item.AddresserName + "'>";
                     temp += "<input type='checkbox' name='ckDraftNoticer' id='ckDraftNoticer" + index + "' value='" + item.AddresserName + "'>";
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
                 $("#ulDraftUserList").empty().append(temp).show();
                 $.each(result, function (index, item) {
                     $("#ckDraftNoticer" + index).data("Noticer", {
                         UserID: item.AddresserID,
                         UserName: item.AddresserName
                     });
                 });
                 $("#ckAllDraftNoticers").on("click", Draft.AtAllHandleEvent);
                 $("input:checkbox[name='ckDraftNoticer']").on("click", Draft.AtHandleEvent);
             } else {
                 $("#ulDraftUserList").empty().hide();
             }
         });
}
//@勾选全部事件
Draft.AtAllHandleEvent = function AtAllHandlDraftEvent(event) {
    if ($(event.target).is(":checked") == true) {
        $("input:checkbox[name='ckDraftNoticer']").prop("checked", true);
        $.each($("input:checkbox[name='ckDraftNoticer']"), function (index, item) {
            $("#txtDraftAtPerson").val($("#txtDraftAtPerson").val() + "@" + $(item).data("Noticer").UserName + ";");
        });
    }
    else {
        $("input:checkbox[name='ckDraftNoticer']").prop("checked", false);
        $.each($("input:checkbox[name='ckDraftNoticer']"), function (index, item) {
            var delete_atstr = "@" + $(item).data("Noticer").UserName + ";";
            var replace_reg = new RegExp(delete_atstr);
            $("#txtDraftAtPerson").val($("#txtDraftAtPerson").val().replace(replace_reg, ""));
        });
    }
    $("#txtDraftAtPerson").attr("title", $("#txtDraftAtPerson").val());
}
//@勾选单个事件
Draft.AtHandleEvent = function AtHandleDraftEvent(event) {
    if ($(event.target).is(":checked") == true) {
        $("#txtDraftAtPerson").val($("#txtDraftAtPerson").val() + "@" + $(event.target).data("Noticer").UserName + ";");
    } else {
        var delete_atstr = "@" + $(event.target).data("Noticer").UserName + ";";
        var replace_reg = new RegExp(delete_atstr);
        $("#txtDraftAtPerson").val($("#txtDraftAtPerson").val().replace(replace_reg, ""));
    }
}

//绑定行业圈子
Draft.CommunityDraftBind = function community_draft_bind(community) {
    $("#ulDraftUserList").empty().hide();
    Draft.InitCommunity = true;
    var keyword = {
        Keyword: ""
    };
    $.SimpleAjaxPost("service/CommunityService.asmx/Search", true, "{searchView:" + $.Serialize(keyword) + ",page:" + null + "}",
         function (json) {
             var result = $.Deserialize(json.d);
             if (result != null) {
                 var temp = "<option>--请选择圈子--</option>";
                 $.each(result, function (index, item) {
                     temp += "<option id='Community-Draft" + index + "'>" + item.Name + "</option>";
                 });
                 $("#sltDraftCommunityList").empty().append(temp).prop("disabled", false);
                 $.each(result, function (index, item) {
                     $("#Community-Draft" + index).data("ID", item.ID);
                 });
                 $("#sltDraftCommunityList").off("change").on("change", { Community: community }, Draft.GetAllCommunityMembersDraftEvent);
                 if (community && community.CommunityID) {
                     $.each(result, function (index, item) {
                         if (community.CommunityID == item.ID) {
                             $("#Community-Draft" + index).attr("selected", true);
                             $("#sltDraftCommunityList").trigger("change");
                         }
                     });
                 }
             } else {
                 $("#sltDraftCommunityList").empty();
             }
         });
}

Draft.GetAllCommunityMembersDraftEvent = function GetAllCommunityMembersDraftEvent(event) {
    //标签初始化
    $("#divLabelContains-Draft").hide();
    $("#ulDraftLabelList").empty();
    $("#txtDraftLabel,#txtDraftAtPerson").val("");
    var community_id = $(event.target).find("option:checked").data("ID");
    var init_community = event.data.Community;
    if (community_id) {
        $("#divLabelContains-Draft").show();
        //读取标签
        $.SimpleAjaxPost("service/CommunityService.asmx/GetCommunityLabelList", true, "{communityID:'" + community_id + "'}",
                   function (json) {
                       var result = $.Deserialize(json.d);
                       if (Array.isArray(result)) {
                           var temp = "";
                           $.each(result, function (index, item) {
                               temp += "<li>";
                               temp += "<label for='" + item.Name + "'>";
                               temp += "<input type='checkbox' name='ckDraftLabel' id='ckDraftLabel" + index + "' value='" + item.Name + "'>";
                               temp += "<span>" + item.Name + "</span>";
                               temp += "</label>";
                               temp += "</li>";
                           });
                           $("#ulDraftLabelList").empty().append(temp).show();
                           $.each(result, function (index, item) {
                               $("#ckDraftLabel" + index).data("Label", {
                                   LabelID: item.ID,
                                   LabelName: item.Name
                               });


                           });
                           $("input:checkbox[name='ckDraftLabel']").on("click", Draft.LabelHandleEvent);
                           if (Draft.InitCommunity == true) {
                               if (init_community && Array.isArray(init_community.LabelInfos)) {
                                   $.each(result, function (index, item) {
                                       $.each(init_community.LabelInfos, function (index1, item1) {
                                           if (item.ID == item1.LabelID) {
                                               $("#ckDraftLabel" + index).trigger("click");
                                           }
                                       });
                                   });
                               }
                               Draft.InitCommunity = false;
                           }

                       } else {
                           $("#ulDraftLabelList").empty().hide();
                       }
                   });

        //读取人
        $.SimpleAjaxPost("service/CommunityService.asmx/GetMemberInfoList", true, "{communityID:'" + community_id + "'}",
            function (json) {
                var result = $.Deserialize(json.d);
                if (result != null) {
                    var temp = "";
                    temp += "<li>";
                    temp += "<label for='AllMembers'>";
                    temp += "<input type='checkbox' id='ckAllDraftNoticers' value='勾选@全部'>";
                    temp += "<span>勾选@全部</span>";
                    temp += "</label>";
                    temp += "</li>";
                    $.each(result, function (index, item) {
                        temp += "<li>";
                        temp += "<label for='" + item.MemberName + "'>";
                        temp += "<input type='checkbox' name='ckDraftNoticer' id='ckDraftNoticer" + index + "' value='" + item.MemberName + "'>";
                        temp += "<span class='user-select-photo'>";
                        temp += "<img src='" + item.MemberUrl + "'>";
                        temp += "</span>";
                        temp += "<span>" + item.MemberName + "</span>";
                        if (item.Remark && item.Remark != "") {
                            temp += "<span class='user-list-memo'>" + item.Remark + "</span>";
                        }
                        temp += "</label>";
                        temp += "</li>";
                    });
                    $("#ulDraftUserList").empty().append(temp).show();
                    $.each(result, function (index, item) {
                        $("#ckDraftNoticer" + index).data("Noticer", {
                            UserID: item.MemberID,
                            UserName: item.MemberName
                        });
                    });
                    $("#ckAllDraftNoticers").on("click", Draft.AtAllHandleEvent);
                    $("input:checkbox[name='ckDraftNoticer']").on("click", Draft.AtHandleEvent);
                } else {
                    $("#ulDraftUserList").empty().hide();
                }
            });
    } else {
        $("#ulDraftUserList").empty().hide();
        $("#ulDraftLabelList").empty().hide();
        $("#divLabelContains-Draft").hide();
    }
}

Draft.LabelHandleEvent = function LabelHandleEvent(event) {
    if ($(event.target).is(":checked") == true) {
        $("#txtDraftLabel").val($("#txtDraftLabel").val() + $(event.target).data("Label").LabelName + ";");
    } else {
        var delete_labelstr = $(event.target).data("Label").LabelName + ";";
        var replace_reg = new RegExp(delete_labelstr, 'g');
        $("#txtDraftLabel").val($("#txtDraftLabel").val().replace(replace_reg, ""));
    }
}


Draft.DeleteOldAccEvent = function DeleteOldAccEvent(event) {
    var index = event.data.Index;
    var acc = event.data.Acc;
    $("#divRemoveDraftAcc" + index).remove();
    if (acc) {
        //删除的项添加进删除项list
        var simple_remove_acc_item = {
            ID: acc.ID,
            FilePath: acc.FilePath,
            FileType: acc.FileType
        };
        Draft.RemoveAccFileList.push(simple_remove_acc_item);
        if ($("#divDraftFileList").find(".user-attachment-del").length == 0) {
            $("#divDraftFileList").hide();
        }
    }
}


Draft.Community = function () {
//草稿箱行业圈子业务
}
Draft.Community.registerClass("Draft.Community");
Draft.Community.OldLabelList = new Array();
Draft.Community.OldLabelCount = 0;
Draft.Community.Init = function init() {
    Draft.Community.Search({ pageStart: 1, pageEnd: Draft.PageSize * 1 });
    $("#spnDraftSearch").on("click", { Page: { pageStart: 1, pageEnd: Draft.PageSize * 1 } }, Draft.Community.SearchEvent);
    //搜索框回车时间绑定
    $("#txtKeyword").off("keypress").on("keypress", function (event) {
        if (event.which == 13) {
            Draft.Community.Search({ pageStart: 1, pageEnd: Draft.PageSize * 1 });
        }
    });
    //草稿箱关闭事件
    $("#sctDraft").dialog("option", "close", function (event, ui) {
        //退出编辑状态，清空读取信息
        //新添加的附件列表
        Draft.AddAccFileList = new Array();
        //删除的附件列表
        Draft.RemoveAccFileList = new Array();
        Draft.Community.OldLabelList = new Array();
        Draft.Community.OldLabelCount = 0;
        $("#sltDraftCommunityList,#ulDraftLabelList,#ulDraftUserList").empty();
        $("#txtDraftAtPerson").val("");
    });
   
}

//搜索博文事件
Draft.Community.SearchEvent = function SearchEvent(event) {
    var page = event.data.Page;
    Draft.Community.Search(page);
}

//分页搜索
Draft.Community.Search = function search(page) {
    Draft.Community.SearchBind(page);
    var keyword_view = {
        Keyword: $("#txtKeyword").val()
    };
    $.SimpleAjaxPost("service/CommunityService.asmx/GetDraftInfoCount",
        true,
        "{keywordView:" + $.Serialize(keyword_view) + "}",
        function (json) {
            var result = json.d;
            if (result <= Draft.PageSize) {
                $("#divDraftPage").wPaginate("destroy");
            }
            else {
                $("#divDraftPage").wPaginate("destroy").wPaginate({
                    theme: "grey",
                    first: "首页",
                    last: "尾页",
                    total: result,
                    index: 0,
                    limit: Draft.PageSize,//一页显示数目
                    ajax: true,
                    url: function (i) {
                        var page = {
                            pageStart: i * this.settings.limit + 1,
                            pageEnd: (i + 1) * this.settings.limit
                        };
                        Draft.Community.SearchBind(page);
                    }
                });
            }
        });
}

//搜索我关注的用户（包括我的）的博文
Draft.Community.SearchBind = function search_bind(page) {
    var keyword_view = {
        Keyword: $("#txtKeyword").val()
    };
    $("#divDraftEmpty").hide();
    $.SimpleAjaxPost("service/CommunityService.asmx/GetDraftInfos",
        true,
                         "{keywordView:" + $.Serialize(keyword_view) + ",page:" + $.Serialize(page) + "}",
                         function (json) {
                             var result = $.Deserialize(json.d);
                             if (Array.isArray(result) && result.length != 0) {
                                 var temp = "";
                                 $.each(result, function (index, item) {
                                     temp += "<li id='liMomentInfo" + index + "'>";
                                     temp += "<div class='friend-avatar'>";
                                     temp += "<div class='friend-avatar-cover'><img src='" + item.CreaterUserUrl + "'></div>";
                                     temp += "</div>";
                                     temp += "<div class='friend-msg-content'>"
                                     temp += "<div class='friend-name'>" + item.CreaterName + "</div>";
                                     temp += "<div class='article-title'>" + item.Title + "</div>";
                                     temp += "<div class='friend-msg-text'>" + item.Content + "</div>";
                                     temp += "<div class='friend-article'><a href='javascript:void(0);' id='aShowDetailLong" + index + "'>查看全部</a></div>";
                                     $(document).off("click", "#aShowDetailLong" + index).on("click", "#aShowDetailLong" + index, { LongObj: item }, Draft.Community.ShowDetailLongEvent);
                                     temp += Moments.List.GetAccStr(item.AccInfos);
                                     temp += "<div class='friend-msg-info clear-fix'>";
                                     temp += "<div class='article-info'>";
                                     temp += "<span>" + objPub.GetSimpleTimeFormat(item.CreateTime) + "</span>";
                                     temp += " </div>";
                                     temp += "<div class='article-opts-block'>";
                                     temp += "<div class='article-opts clear-fix'>";
                                     temp += "<ul>";
                                     temp += "<li id='liPublish" + index + "'><span class='icon-optSet icon-img icon-opt-resend'></span><span class='opt-text'>发布</span></li>";
                                     temp += "<li id='liDelete" + index + "'><span class='icon-optSet icon-img icon-opt-delete'></span><span class='opt-text'>删除</span></li>";
                                     temp += "<li id='liEdit" + index + "'><span class='icon-optSet icon-img icon-opt-draft'></span><span class='opt-text'>编辑草稿</span></li>";
                                     temp += "</ul></div>";
                                     temp += "</div>";
                                     temp += "</li>";
                                 });
                                 $("#ulDraftList").empty().append(temp);
                                 $.each(result, function (index, item) {
                                     //发布
                                     $("#liPublish" + index).on("click", { DraftID: item.ID }, Draft.Community.PublishEvent);
                                     //删除
                                     $("#liDelete" + index).on("click", { DraftID: item.ID }, Draft.Community.DeleteEvent);
                                     //编辑草稿
                                     $("#liEdit" + index).on("click", { Draft: item }, Draft.Community.EditDraftEvent);
                                 });

                             } else {
                                 $("#ulDraftList").empty();
                                 $("#divDraftEmpty").show();
                             }
                         });
}
Draft.Community.PublishEvent = function PublishEvent(event) {
    var draft_id = event.data.DraftID;
    $.SimpleAjaxPost("service/CommunityService.asmx/SetPublish",
                      true,
                       "{publishID:'" + draft_id + "'}",
                       function (json) {
                           var result = json.d;
                           if (result == true) {
                               Draft.Community.Search({ pageStart: 1, pageEnd: Draft.PageSize * 1 });
                           } else {
                               console.log("发表草稿失败，请联系管理员");
                           }
                       });
}

Draft.Community.DeleteEvent = function DeleteEvent(event) {
    var draft_id = event.data.DraftID;
    $.SimpleAjaxPost("service/CommunityService.asmx/Delete",
                      true,
                       "{publishID:'" + draft_id + "'}",
                       function (json) {
                           var result = json.d;
                           if (result == true) {
                               Draft.Community.Search({ pageStart: 1, pageEnd: Draft.PageSize * 1 });
                           } else {
                               console.log("删除草稿失败，请联系管理员");
                           }
                       });
}

Draft.Community.EditDraftEvent = function EditDraftEvent(event) {
    var draft = event.data.Draft;
    $("#sctDraft").dialog("option", "open", function (event, ui) {
        //初始化
        $("#txtDraftTitle").val("");
        $("#divDraftFileList").empty().hide();
        UM.getEditor("draftEditor").reset();
        //读取内容
        Draft.Community.GetDetailDraft(draft);
    }).dialog("open");

}
Draft.Community.GetDetailDraft = function get_detail_draft(draft) {
    var draft = draft;
    var accs = draft.AccInfos;
    var community = draft.Community;
    if (community[0]) {
        if (Array.isArray(community[0].LabelInfos)) {
            Draft.Community.OldLabelCount = community[0].LabelInfos.length;
            $.each(community[0].LabelInfos, function (index, item) {
                Draft.Community.OldLabelList.push({
                    LabelID: item.LabelID,
                    LabelName: item.LabelName,
                    CommunityID: community[0].CommunityID
                });
            });
        }
    }
    Draft.CommunityDraftBind(community[0]);
    //隐藏朋友圈
    $("div[name='draft-moments']").hide();
    $("div[name='draft-community']").show();
    $("input:radio[name='NoticeSource-Draft'][value='" + Enum.BusinessType.Community + "']").prop("checked", true);
    //写入标题
    $("#txtDraftTitle").val(draft.Title);
    //写入内容
    UM.getEditor("draftEditor").setContent(draft.DetailContent, false);

    //写入附件
    if (accs != null && accs.length != 0) {
        var temp = "";
        $.each(accs, function (index, item) {
            //长篇无图片附件
            temp += "<div class='user-attachment'>";
            temp += "<a target='_blank' href='javascript:void(0);'><span class='icon-optSet icon-img " + Enum.FileType.GetIconClass(item.FileType) + "'></span></a>";
            temp += "<a href='" + item.FilePath + "' title='" + item.FileName.substr(0, item.FileName.lastIndexOf(".")) + "'>" + item.FileName + "</a>";
            temp += "<div class='user-attachment-del' id='divRemoveDraftAcc" + index + "'>";
            temp += "<span class='icon-optSet icon-img icon-opt-delete'></span><span class='opt-text'>删除</span>";
            temp += "</div>";
            temp += "</div>";
            $(document).off("click", "#divRemoveDraftAcc" + index);
            $(document).on("click", "#divRemoveDraftAcc" + index, { Index: index, Acc: item }, Draft.DeleteOldAccEvent);
        });
        $("#divDraftFileList").append(temp).show();
    }
    else {
        $("#divDraftFileList").empty().hide();
    }
    //保存发表绑定事件
    $("#btnLongInfoCancel-Draft").off("click").on("click", function (event) {
        $("#sctDraft").dialog("close");
    });
    $("#btnLongInfoSave-Draft").off("click").on("click", { IsPublish: false, DraftID: draft.ID }, Draft.Community.UploadEvent);
    $("#btnLongInfoSend-Draft").off("click").on("click", { IsPublish: true, DraftID: draft.ID }, Draft.Community.UploadEvent);

}

Draft.Community.ShowDetailLongEvent = function ShowDetailLongEvent(event) {
    var long_obj = event.data.LongObj;
    $("#divDraftLongTitle").html(long_obj.Title);
    $("#spnDraftCreateTime").text(objPub.GetSimpleTimeFormat(long_obj.CreateTime));
    $("#divDraftLongContent").html(long_obj.DetailContent);
    $("#sctDraftLongDetail .user-attachment").remove();
    $("#divDraftLongContent").after(Moments.List.GetAccStr(long_obj.AccInfos));
    objPub.WindowScrollTop = $(window).scrollTop();
    $("#sctDraftLongDetail").dialog("open");
   
}

Draft.Community.UploadEvent = function UploadEvent(event) {
    var id = event.data.DraftID;
    var is_publish = event.data.IsPublish;
    var publish_type = Enum.PublishInfoType.Long;
    var source_type = Enum.BusinessType.Community;
    var now_date = new Date().format("yyyy-MM-dd HH:mm:ss");

    var noticers = new Array();

    var community_id = $("#sltDraftCommunityList").find("option:checked").data("ID")

    if (!community_id) {
        $.Alert("请选择圈子!");
        return;
    }


    //读取提醒人
    $.each($("input:checkbox[name='ckDraftNoticer']"), function (index, item) {
        if ($(item).is(":checked") == true) {
            noticers.push($(item).data("Noticer"));
        }
    });

    var notice_user_view = null;
    if (noticers.length != 0 && is_publish) {
        var notice_user_view = {
            NoticeType: Enum.NoticeType.PublishInfo,
            NoticeSource: source_type,
            Noticers: noticers
        };
    }

    var publish_info = {
        ID: id,
        Title: $("#txtDraftTitle").val(),
        Content: UM.getEditor("draftEditor").getContent(),
        PublishType: publish_type.toString(),
        EditStatus: is_publish ? Enum.YesNo.No.toString() : Enum.YesNo.Yes.toString(),
        PublishTime: now_date
    };

    if (publish_info.ID) {
        var accs = new Array();
        //读取附件
        //添加文件附件
        $.each(Draft.AccFileList, function (index, item) {
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

        var remove_accs = new Array();
        //读取删除文件附件
        $.each(Draft.RemoveAccFileList, function (index, item) {
            var acc_item = {
                ID: item.ID,
                FileType: item.FileType.toString(),
                FilePath: item.FilePath
            };
            remove_accs.push(acc_item);
        });

        var labels = new Array();

        //读取标签
        $.each($("input:checkbox[name='ckDraftLabel']"), function (index, item) {
            if ($(item).is(":checked") == true) {
                var label_item = {
                    LabelID: $(item).data("Label").LabelID,
                    LabelName: $(item).data("Label").LabelName,
                    CommunityID: community_id
                };
                var is_exist = false;
                var find_index = 0;
                $.each(Draft.Community.OldLabelList, function (index, item) {
                    if (item.LabelID === label_item.LabelID
                        && item.LabelName === label_item.LabelName
                        && item.CommunityID == label_item.CommunityID) {
                        is_exist = true;
                        find_index = index;
                    }
                });

                if (is_exist) {
                    Draft.Community.OldLabelList.splice(find_index, 1);
                } else {
                    labels.push(label_item);
                }
            }
        });

        var remove_labels = new Array();

        //读取删除标签
        if (Draft.Community.OldLabelList.length != 0) {
            $.each(Draft.Community.OldLabelList, function (index, item) {
                remove_labels.push(item.LabelID);
            });
        }

        if ((labels.length + Draft.Community.OldLabelCount - remove_labels.length) == 0) {
            $.Alert("请选择话题标签");
            return;
        }
        console.log(Draft.Community.OldLabelList);
        console.log(labels);
        console.log(remove_labels);
        $.SimpleAjaxPost("service/CommunityService.asmx/Submit",
                         true,
                         "{publishInfo:" + $.Serialize(publish_info) +
                            ",noticeUserView:" + (notice_user_view != null ? $.Serialize(notice_user_view, true) : notice_user_view) +
                             ",publishAccessoryInfos:" + $.Serialize(accs, true) +
                             ",simpleLabelViews:" + $.Serialize(labels, true) +
                              ",removeSimpleAccessoryViews:" + $.Serialize(remove_accs, true) +
                              ",removeSimpleLabelViewIDs:" + $.Serialize(remove_labels, true) + "}",
                         function (json) {
                             var result = json.d;
                             if (result == true) {
                                 Draft.Community.Search({ pageStart: 1, pageEnd: Draft.PageSize * 1 });
                                 $("#sctDraft").dialog("close");
                             }
                         });
    } else {
        console.log("草稿ID异常丢失");
    }
}
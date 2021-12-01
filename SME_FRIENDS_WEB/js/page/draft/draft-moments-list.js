Draft.Moments = function () {
//草稿箱-朋友圈业务
}
Draft.Moments.registerClass("Draft.Moments");

Draft.Moments.Init = function init() {
    Draft.Moments.Search({ pageStart: 1, pageEnd: Draft.PageSize * 1 });
    $("#spnDraftSearch").on("click", { Page: { pageStart: 1, pageEnd: Draft.PageSize * 1 } }, Draft.Moments.SearchEvent);
    //搜索框回车时间绑定
    $("#txtKeyword").off("keypress").on("keypress", function (event) {
        if (event.which == 13) {
            Draft.Moments.Search({ pageStart: 1, pageEnd: Draft.PageSize * 1 });
        }
    });
    //草稿箱关闭事件
    $("#sctDraft").dialog("option", "close", function (event, ui) {
        //退出编辑状态，清空读取信息
        //新添加的附件列表
        Draft.AddAccFileList = new Array();
        //删除的附件列表
        Draft.RemoveAccFileList = new Array();
        $(".um-editor").hide();
    });
   
}


//搜索博文事件
Draft.Moments.SearchEvent = function SearchEvent(event) {
    var page = event.data.Page;
    Draft.Moments.Search(page);
}

//分页搜索
Draft.Moments.Search = function search(page) {
    Draft.Moments.SearchBind(page);

    var keyword_view = {
        Keyword: $("#txtKeyword").val()
    };

    $.SimpleAjaxPost("service/MomentsService.asmx/GetDraftInfoCount",
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
                        Draft.Moments.SearchBind(page);
                    }
                });
            }
        });
}

//搜索我关注的用户（包括我的）的博文
Draft.Moments.SearchBind = function search_bind(page) {
    var keyword_view = {
        Keyword: $("#txtKeyword").val()
    };
    $("#divDraftEmpty").hide();
    $.SimpleAjaxPost("service/MomentsService.asmx/GetDraftInfos",
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
                    $(document).off("click", "#aShowDetailLong" + index);
                    $(document).on("click", "#aShowDetailLong" + index, { LongObj: item }, Draft.Moments.ShowDetailLongEvent);
                    temp += Moments.List.GetAccStr(item.AccInfos);
                    temp += "<div class='friend-msg-info clear-fix'>";
                    temp += "<div class='article-info'>";
                    temp += "<span>" + objPub.GetSimpleTimeFormat(item.CreateTime) + "</span>";
                    temp += " </div>";
                    temp += "<div class='article-opts-block'>";
                    temp += "<div class='article-opts clear-fix'>";
                    temp += "<ul>";
                    temp += "<li id='liPublish"+index+"'><span class='icon-optSet icon-img icon-opt-resend'></span><span class='opt-text'>发布</span></li>";
                    temp += "<li id='liDelete"+index+"'><span class='icon-optSet icon-img icon-opt-delete'></span><span class='opt-text'>删除</span></li>";
                    temp += "<li id='liEdit"+index+"'><span class='icon-optSet icon-img icon-opt-draft'></span><span class='opt-text'>编辑草稿</span></li>";
                    temp += "</ul></div>";
                    temp += "</div>";
                    temp += "</li>";
                    $(document).off("click", "#liPublish" + index + ",#liDelete" + index + ",#liEdit" + index);
                    //发布
                    $(document).on("click", "#liPublish" + index, { DraftID: item.ID }, Draft.Moments.PublishEvent);
                    //删除
                    $(document).on("click", "#liDelete" + index, { DraftID: item.ID }, Draft.Moments.DeleteEvent);
                    //编辑草稿
                    $(document).on("click", "#liEdit" + index, { Draft: item }, Draft.Moments.EditDraftEvent);
                });
                $("#ulDraftList").empty().append(temp);
            } else {
                $("#ulDraftList").empty();
                $("#divDraftEmpty").show();
            }
        });
}

Draft.Moments.PublishEvent = function PublishEvent(event) {
    var draft_id = event.data.DraftID;
    $.SimpleAjaxPost("service/MomentsService.asmx/SetPublish",
                      true,
                       "{publishID:'" + draft_id + "'}",
                       function (json) {
                           var result = json.d;
                           if (result == true) {
                               Draft.Moments.Search({ pageStart: 1, pageEnd: Draft.PageSize * 1 });
                           } else {
                               console.log("发表草稿失败，请联系管理员");
                           }
                       });
}

Draft.Moments.DeleteEvent = function DeleteEvent(event) {
    var draft_id = event.data.DraftID;
    $.SimpleAjaxPost("service/MomentsService.asmx/Delete",
                      true,
                       "{publishID:'" + draft_id + "'}",
                       function (json) {
                           var result = json.d;
                           if (result == true) {
                               Draft.Moments.Search({ pageStart: 1, pageEnd: Draft.PageSize * 1 });
                           } else {
                               console.log("删除草稿失败，请联系管理员");
                           }
                       });
}

Draft.Moments.EditDraftEvent = function EditDraftEvent(event) {
    var draft = event.data.Draft;
    $("#sctDraft").dialog("option", "open", function (event, ui) {
        $(".um-editor").show();
        //初始化
        $("#txtDraftTitle").val("");
        $("#divDraftFileList").empty().hide();
        UM.getEditor("draftEditor").reset();
        //读取内容
        Draft.Moments.GetDetailDraft(draft);
    }).dialog("open");
   
}

Draft.Moments.GetDetailDraft = function get_detail_draft(draft) {
    Draft.MyAddressbookBind();
    var draft = draft;
    var accs = draft.AccInfos;
    //隐藏行业圈子
    $("div[name='draft-community']").hide();
    $("div[name='draft-moments']").show();
    $("input:radio[name='NoticeSource-Draft'][value='" + Enum.BusinessType.Moments + "']").prop("checked", true);
    //写入标题
    $("#txtDraftTitle").val(draft.Title);
    //写入内容
    UM.getEditor("draftEditor").setContent(draft.DetailContent, false);

    //写入附件
    if (accs != null && accs.length != 0) {
        var temp = "";
        $.each(accs, function (index, item) {//长篇无图片附件
            temp += "<div class='user-attachment'>";
            temp += "<a target='_blank' href='javascript:void(0);'><span class='icon-optSet icon-img " + Enum.FileType.GetIconClass(item.FileType) + "'></span></a>";
            temp += "<a href='" + item.FilePath + "' title='" + item.FileName.substr(0, item.FileName.lastIndexOf(".")) + "'>" + item.FileName + "</a>";
            temp+= "<div class='user-attachment-del' id='divRemoveDraftAcc" + index + "'>" ;
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
    $("#btnLongInfoSave-Draft").off("click").on("click", { IsPublish: false, DraftID: draft.ID }, Draft.Moments.UploadEvent);
    $("#btnLongInfoSend-Draft").off("click").on("click", { IsPublish: true, DraftID: draft.ID }, Draft.Moments.UploadEvent);

}

Draft.Moments.ShowDetailLongEvent = function ShowDetailLongEvent(event) {
    var long_obj = event.data.LongObj;
    $("#divDraftLongTitle").html(long_obj.Title);
    $("#spnDraftCreateTime").text(objPub.GetSimpleTimeFormat(long_obj.CreateTime));
    $("#divDraftLongContent").html(long_obj.DetailContent);
    $("#sctDraftLongDetail .user-attachment").remove();
    $("#divDraftLongContent").after(Moments.List.GetAccStr(long_obj.AccInfos));
    objPub.WindowScrollTop = $(window).scrollTop();
    $("#sctDraftLongDetail").dialog("open");
   
}

Draft.Moments.UploadEvent = function UploadEvent(event) {
    var id = event.data.DraftID;
    var is_publish = event.data.IsPublish;
    var publish_type = Enum.PublishInfoType.Long;
    var source_type = Enum.BusinessType.Moments;
    var now_date = new Date().format("yyyy-MM-dd HH:mm:ss");
    var noticers = new Array();
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


        $.SimpleAjaxPost("service/MomentsService.asmx/Submit",
                         true,
                         "{publishInfo:" + $.Serialize(publish_info) +
                          ",noticeUserView:" + (notice_user_view != null ? $.Serialize(notice_user_view, true) : notice_user_view) +
                          ",publishAccessoryInfos:" + $.Serialize(accs, true) +
                          ",removeSimpleAccessoryViews:" + $.Serialize(remove_accs, true) + "}",
                         function (json) {
                             var result = json.d;
                             if (result == true) {
                                 //如果发布
                                 //if (is_publish == true) {
                                 //    if ($.isArray(notice_user_view.Noticers) == true && notice_user_view.Noticers.length > 0) {
                                 //        shareHubProxy.invoke("SendMessage", publish_info.ID, publish_info.Title, notice_user_view);
                                 //    }
                                 //}
                                 Draft.Moments.Search({ pageStart: 1, pageEnd: Draft.PageSize * 1 });
                                 $("#sctDraft").dialog("close");
                             }
                         });
    } else {
        console.log("草稿ID异常丢失");
    }
}
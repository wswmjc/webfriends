Template = function () { }
Template.registerClass("Template");
//模板 朋友圈信息列表
//引用位置 
//1- moments-list.js -> Moments.List.Init
Template.MomentsList = "<div class='friend-list-empty' id='divMomentsEmpty'><span id='spnEmptyTip'>您的朋友圈还没有消息哦~</span></div>" +
                       "<div class='friend-messages'><ul id='ulMomentsList'></ul></div>";

//模板 讨论组讨论
//引用位置 
//1- group-message-list.js ->Group.Message.Init
Template.GroupTopicDetailTpl =   "<h3 class='topic-headline' id='hTopicTitle'></h3>" +
                            "<div class='topic-other clear-fix'>" +
                                "<div class='topic-time'>" +
                                    "<span id='spnTopicCreateTime'></span>" +
                                    "<span>&nbsp;&nbsp;建立&nbsp;&nbsp;by&nbsp;&nbsp;</span>" +
                                    "<span id='spnTopicCreater'></span>" +
                                "</div>" +
                                "<div class='topic-discuss-num'>" +
                                    "<span id='spnTopicMessageCount'></span>" +
                                    "<span>条反馈</span>" +
                                "</div>" +
                            "</div>" +
                            "<div class='message'>" +
                                "<div class='friend-messages'>" +
                                //评论列表
                                     "<div class='comment-block' id='divGroupTopicMessage'>" +
                                         "<div class='comment-message'>" +
                                             "<div style='position: relative;'>" +
                                                 "<div class='weibo-editor-comment' id='txtGroupTopicCommentContent' contenteditable='true' tabindex='-1'></div>" +
                                                 "<div class='editor-bottom'>" +
                                                     "<div class='editor-options'>" +
                                                          "<a href='javascript:void(0)' id='aGroupTopicEmotion' title='表情' class='public_emotion'><span class='icon-optSet icon-img icon-face'></span></a>" +
                                                     "</div>" +
                                                 "</div>" +
                                             "</div>" +
                                             "<div class='comment-submit'>" +
                                                 "<input value='取消' type='button' id='commentGroupTopicCancel'>&nbsp;&nbsp;" +
                                                 "<input value='提交' type='button' id='commentGroupTopicSend'>" +
                                             "</div>" +
                                         "</div>" +
                                         "<div class='comment-list'>" +
                                             "<ul id='ulGroupTopicMessageList'>" +
                                             "</ul>" +
                                             "<div class='pagination'><div class='wPaginate8nPosition' id='divGroupTopicMessagePage'></div></div>" +
                                         "</div>" +
                                     "</div>" +
                                "</div>" +
                            "</div>";

//模板 行业圈子讨论
//引用位置 
//1- community-message-list.js ->Community.Message.Init
Template.CommunityTopicDetailTpl = "<h3 class='topic-headline' id='hTopicTitle'></h3>" +
                            "<div class='topic-other clear-fix'>" +
                                "<div class='topic-time'>" +
                                    "<span id='spnTopicCreateTime'></span>" +
                                    "<span>&nbsp;&nbsp;建立&nbsp;&nbsp;by&nbsp;&nbsp;</span>" +
                                    "<span id='spnTopicCreater'></span>" +
                                "</div>" +
                                "<div class='topic-discuss-num'>" +
                                    "<span id='spnTopicMessageCount'></span>" +
                                    "<span>条反馈</span>" +
                                "</div>" +
                            "</div>" +
                            "<div class='message'>" +
                                "<div class='friend-messages'>" +
                                //评论列表
                                     "<div class='comment-block' id='divCommunityTopicMessage'>" +
                                         "<div class='comment-message'>" +
                                             "<div style='position: relative;'>" +
                                                 "<div class='weibo-editor-comment' id='txtCommunityTopicCommentContent' contenteditable='true' tabindex='-1'></div>" +
                                                 "<div class='editor-bottom'>" +
                                                     "<div class='editor-options'>" +
                                                          "<a href='javascript:void(0)' id='aCommunityTopicEmotion' title='表情' class='public_emotion'><span class='icon-optSet icon-img icon-face'></span></a>" +
                                                     "</div>" +
                                                 "</div>" +
                                             "</div>" +
                                             "<div class='comment-submit'>" +
                                                 "<input value='取消' type='button' id='commentCommunityTopicCancel'>&nbsp;&nbsp;" +
                                                 "<input value='提交' type='button' id='commentCommunityTopicSend'>" +
                                             "</div>" +
                                         "</div>" +
                                         "<div class='comment-list'>" +
                                             "<ul id='ulCommunityTopicMessageList'>" +
                                             "</ul>" +
                                             "<div class='pagination'><div class='wPaginate8nPosition' id='divCommunityTopicMessagePage'></div></div>" +
                                         "</div>" +
                                     "</div>" +
                                "</div>" +
                            "</div>";

//模板 行业圈子详细页
//引用位置 
//1- community-label-list.js -line 56 ->Community.Label.GoDetailEvent
Template.DetailCommunityTpl = "<div id='divCommunityTimeAxis' class='friend-filter' style='display: block;'>" +
                                  "<ul class='filter' id='ulCommunityDateList' style='top: 390px; position: absolute;'>" +
                                  "</ul>" +
                              "</div>" +
                              "<div class='friend-content'>" +
                                "<section class='circle-topline clear-fix'>"+
                                    "<div class='back-home'>"+
                                        "<a id='aGoBack' href='javascript:void(0);'>"+
                                            "<span class='icon-optSet icon-img icon-back'></span>"+
                                        "</a>"+
                                    "</div>"+
                                    "<div class='top-options'>"+
                                        "<span id='spnAddLabel' class='icon-optSet icon-img icon-plus' style='display:none;' title='新话题'></span>"+
                                        "<span id='spnDeleteLabel' class='icon-optSet icon-img icon-trash' style='display:none;' title='删除话题'></span>"+
                                        "<span id='spnRemoveCommunity' class='icon-optSet icon-img icon-quit' style='display:none;' title='退出圈子'></span>"+
                                    "</div>"+
                                    "<div id='divCommunityName' class='topline-name'></div>"+
                                "</section>"+
                                "<section class='circle-tabs clear-fix'>"+
                                    "<ul id='ulSubType'>"+
                                        "<li class='circle-subject selected'><b id='bLabelCount'>0</b>个话题</li>"+
                                        "<li class='circle-topic'><b id='bTopicCount'>0</b>个讨论</li>" +
                                    "</ul>"+
                                "</section>"+
                                "<section id='sctCommunityContent' class='friend-message-list clear-fix'></section>" +
                                "<div class='pagination' style='text-align: center; margin-bottom: 15px;'><div id='wPaginate8nPosition'></div></div>"+
                                "<section id='sctAddLabel' class='dialog-addtopic'>" +
                                    "<div>"+
                                        "<span>话题名</span>"+
                                        "<input id='txtAddLabel' type='text' class='search'>"+
                                    "</div>"+
                                "</section>"+
                              "</div>";

//模板 行业圈子讨论列表模板
//引用位置 
//1- community-label-list.js -line 90 ->Community.Label.GoDetailEvent
Template.CommunityTopicListTpl = "<section id='sctCommunityTopicList' class='friend-message-list'>" +
                                    "<div id='divEmptyCommunityTopicList' class='friend-list-empty'><span>暂没有人发表讨论哦~</span></div>" +
                                 "</section>";

//模板 行业圈子信息列表模板
//引用位置 
//1- community-publishinfo-list.js ->Community.PublishInfo.Init
Template.CommunityPublishInfoListTpl = "<h3 class='topic-headline' id='hSubjectTitle'></h3>" +
                            "<div class='topic-other clear-fix'>" +
                                "<div class='topic-time'>" +
                                    "<span id='spnSubjectCreateTime'></span>" +
                                    "<span>&nbsp;&nbsp;建立&nbsp;&nbsp;by&nbsp;&nbsp;</span>" +
                                    "<span id='spnSubjectCreater'></span>" +
                                "</div>" +
                                "<div class='topic-discuss-num'>" +
                                    "<span id='spnSubjectPublishInfoCount'></span>" +
                                    "<span>条信息</span>" +
                                "</div>" +
                            "</div>" +
                            "<div class='friend-list-empty' id='divCommunityEmpty'><span>这个话题还没有交流哦~</span></div>" +
                            "<div class='friend-messages'><ul id='ulCommunityList'></ul></div>";
                         
//模板 收藏列表模板
//引用位置
//1- collect.js ->Collect.Init
Template.CollectListTpl = "<div class='favorite-main'>" +
	                        "<section class='circle-topline clear-fix'>" +
                                "<div class='back-home'>" +
                                    "<a href='javascript:;' id='aGoBack'>" +
                                         "<span class='icon-optSet icon-img icon-back'></span>" +
                                    "</a>" +
                                "</div>" +
                                "<div class='topline-name'>我的收藏</div>" +
                            "</section>" +
                            "<section class='circle-tabs clear-fix'>" +
                                "<ul id='ulCollectType'>" +
                                    "<li class='circle-topic selected' name='moments'>朋友</li>" +
                                    "<li class='circle-discuss' name='community'>圈子</li>" +
                                "</ul>" +
                            "</section>" +
	                        "<section class='contacts-search'>" +
                                "<input type='text' class='search' placeholder='查找我的收藏的信息...' id='txtKeyword'>" +
                                "<span class='icon-optSet icon-img icon-search-small' id='spnSearch' style='cursor:pointer'></span>" +
                            "</section>" +
                            "<div class='friend-list-empty' id='divCollectEmpty'>" +
                            "<span>你还没有收藏的消息哦~</span>" +
                            "</div>" +
                            "<div class='friend-messages'>" +
                                "<ul id='ulCollectList'></ul>" +
                                "<div class='pagination'><div class='wPaginate8nPosition' id='divCollectPage'></div></div>" +
                            "</div>"+
                        "</div>";
//模板 草稿列表模板
//引用位置
//1- draft.js ->Draft.Init
Template.DraftListTpl = "<div class='favorite-main'>"+
	                        "<section class='circle-topline clear-fix'>"+
		                        "<div class='back-home'>"+
			                         "<a href='javascript:;' id='aGoBack'>"+
				                         "<span class='icon-optSet icon-img icon-back'></span>"+
			                         "</a>"+
		                        "</div>"+
		                        "<div class='topline-name'>我的草稿</div>"+
	                        "</section>"+
	                        "<section class='circle-tabs clear-fix'>"+
		                        "<ul id='ulDraftType'>"+
			                        "<li class='circle-topic selected' name='moments'>朋友</li>"+
			                        "<li class='circle-discuss' name='community'>圈子</li>" +
		                        "</ul>"+
	                        "</section>"+
	                        "<section class='contacts-search'>"+
		                        "<input type='text' class='search' placeholder='查找我的草稿箱信息...' id='txtKeyword'>"+
                                "<span class='icon-optSet icon-img icon-search-small' id='spnSearch' style='cursor:pointer'></span>"+
	                        "</section>" +
                            "<div class='friend-list-empty' id='divDraftEmpty'>" +
                            "<span>你还没有保存草稿哦~</span>" +
                            "</div>" +
	                        "<div class='friend-messages'>"+
	                            "<ul id='ulDraftList'></ul>" +
                                "<div class='pagination'><div class='wPaginate8nPosition' id='divDraftPage'></div></div>" +
	                        "</div>" +
                        "</div>";

//模板 公告信息列表
//引用位置 
//1- announce.js -> Announce.Init
Template.AnnounceList = "<div class='friend-list-empty' id='divAnnounceEmpty'><span id='spnAnnounceEmptyTip'>暂无公告信息~</span></div>" +
                       "<div class='friend-messages'><ul class='announcement-list' id='ulAnnounceList'></ul></div>";

(function ($) {
    //读取模板
    $.fn.ReadTemplate = function (template,callback) {
        $(this).empty().append(template);
        if (callback && typeof callback == "function") {
            callback();
        }
    };
})(jQuery);




    
   
        
        
    
   
    

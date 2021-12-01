(function (e) {
    function u(u) {
        if (!!$(this).data("Wait") && $(this).data("Wait") == true) {
            return false;
        }

        function c() {
            function h(e) {
                $("html").css("overflow-y", "hidden");
                $(".mask").show();
                $(".dialog-imglist").show();
                if (source == Enum.BusinessType.Moments) {
                    $("#liMomentsComment" + index).trigger("click");
                    if ($("#liMomentsComment" + index).find(".close-comment").is(":hidden") == false) {
                        $("#divMomentsCommentBlock" + index).hide();
                    }
                } else if (source == Enum.BusinessType.Community) {
                    $("#liCommunityComment" + index).trigger("click");
                    if ($("#liCommunityComment" + index).find(".close-comment").is(":hidden") == false) {
                        $("#divCommunityCommentBlock" + index).hide();
                    }
                }
                n.removeClass("loading");
                e.show();
            }
            var t = e(this),
				r = parseInt(n.css("borderLeftWidth")),
                //s-容器宽 o-容器高
                //r- 一般为0 
				i = s - r * 2,
				u = o - r * 2,
				a = t.width() == 0 ? this.width : t.width(),
				f = t.height() == 0 ? this.height : t.height();
            mwidth = t.width();
            mheight = t.height();

            if (a == n.width() && a <= i && f == n.height() && f <= u) {
                h(t);
                return
            }
            if (a > i || f > u) {
                var l = u < f ? u : f,
					c = i < a ? i : a;
                if (l / f <= c / a) {
                    t.width(a * l / f);
                    t.height(l)
                } else {
                    t.width(c);
                    t.height(f * c / a)
                }
            }

            var aw = t.width() == 0 ? this.width : t.width(),
			    ah = t.height() == 0 ? this.height : t.height();

            n.animate({
                width: aw,
                height: ah,
                marginTop: -(ah / 2) - r,
                marginLeft: -(aw / 2) - r
            }, 100, function () {
                //判断是否需要zoom-in/out
                var image = $("<img/>").attr("src", e("#zoom .content>img").attr("src"));
                var image_content = e("#zoom .content");
                //if ($("#zoom .zoom-in").length == 0 && $("#zoom .zoom-out").length != 0) {
                //    $(document).off("click", "#zoom .zoom-out");
                //    $("#zoom .zoom-out").removeClass("zoom-out").addClass("zoom-in");
                //    $(document).on("click", "#zoom .zoom-in", z);
                //}
                if (image_content.width() == image[0].width && image_content.height() == image[0].height) {
                    $("#zoom .zoom-in").hide();
                } else {
                    $("#zoom .zoom-in").show();
                }
                h(t);
            })
        }
        if (u) u.preventDefault();
        var a = e(this),
			f = a.attr("href"),
		    publish_id = a.find("img").data("PublishID"),
			userurl = a.find("img").data("UserUrl");
        source = a.find("img").data("Source");
        //console.log(a.find("img"));
        //console.log(publish_id);
        //console.log(userurl);
        //console.log(source);
        index = a.find("img").data("Index");
        if (source == Enum.BusinessType.Moments) {
            Moments.List.SetDetailInImglist(publish_id, userurl, index);
        } else if (source == Enum.BusinessType.Community) {
            Community.PublishInfo.SetDetailInImglist(publish_id, userurl, index);
        }
        if (!f) return;
        var l = e(new Image).hide();
        e("#zoom .previous, #zoom .next").show();
        if (a.hasClass("zoom") || a.parent("li").parent("ul").find("li").length == 1) e("#zoom .previous, #zoom .next").hide();
        if (!r) {
            r = true;
            t.show();
            e(".imglist-left").addClass("zoomed")
        }
        n.html(l).delay(500).addClass("loading");
        l.load(c).attr("src", f);
        i = a
    }

    // 上一页
    function a() {
        var t = i.parent("li").prev();
        if (t.length == 0) t = i.parent("li").parent("ul").find("li:last-child"); //e(".gallery li:last-child");
        t.find("a").trigger("click")
    }

    // 下一页
    function f() {
        var t = i.parent("li").next();
        if (t.length == 0) t = i.parent("li").parent("ul").find("li:first-child");// e(".gallery li:first-child");
        t.children("a").trigger("click")
    }


    // 关闭预览
    function l(s) {
        if (s) s.preventDefault();
        r = false;
        i = null;
        var lsStr = "";
        //lsStr += '<a class="close"></a>';
        lsStr += '<a class="zoom-in"></a>';
        lsStr += '<a href="#previous" class="previous"></a>';
        lsStr += '<a href="#next" class="next"></a>';
        lsStr += '<div class="content loading"></div>';
        lsStr += '</div>';
        t.hide().empty().append(lsStr);
        n = e("#zoom .content"),
		r = false,
		i = null,
		s = e(".imglist-left").width(),
		o = e(".imglist-left").height();
        e(".imglist-left").removeClass("zoomed").css("overflow", "auto");

        $(".mask").hide();
        $(".dialog-imglist").hide();
        $("html").css("overflow-y", "scroll");

        //清空回复列表
        $(".imglist-right .comment-list ul").empty();
        //收回列表
        if (source == Enum.BusinessType.Moments) {
            $("#liMomentsComment" + index).trigger("click");
        } else if (source == Enum.BusinessType.Community) {
            $("#liCommunityComment" + index).trigger("click");
        }
    }

    // 图片放大
    function z() {
        var image = $("<img/>").attr("src", e("#zoom .content>img").attr("src"));
        if (e(window).height() > image[0].height) {
            var imgTop = (e(window).height() / 2) - (image[0].height / 2);
            var imgLeft = (e(window).width() / 2) - (image[0].width / 2);
            if (imgLeft < 0) imgLeft = 0;
            image.css({
                "top": imgTop + "px",
                "left": imgLeft + "px",
                "position": "absolute"
            });
        }
        e(".mask").append(image);
        e(".dialog-imglist").hide();
        //e(".mask").css("overflow", "scroll");
        image.on("click", function () {
            e(this).remove();
            //e(".mask").css("overflow");
            e(".dialog-imglist").show();
        });
        //var n = e("#zoom .content>img");
        //var v = e("#zoom .content");
        //var oldwidth = v.width();
        //var oldheight = v.height();
        //n.width(mwidth);
        //n.height(mheight);
        //v.css({
        //    width: mwidth,
        //    height: mheight,
        //    marginLeft: -(mwidth / 2) - r
        //});
        //n.on("click", function (e) {
        //    n.width(oldwidth);
        //    n.height(oldheight);
        //    v.css({
        //        width: oldwidth,
        //        height: oldheight,
        //        marginLeft: -(oldwidth / 2)
        //    });
        //    $(this).off("click");
        //    $(document).off("click", "#zoom .zoom-out");
        //    $("#zoom .zoom-out").removeClass("zoom-out").addClass("zoom-in");
        //    $(document).on("click", "#zoom .zoom-in", z);
        //});

        //$(document).off("click", "#zoom .zoom-in");
        //$("#zoom .zoom-in").removeClass("zoom-in").addClass("zoom-out");
        //$(document).on("click", "#zoom .zoom-out", function (e) {
        //    n.width(oldwidth);
        //    n.height(oldheight);
        //    v.css({
        //        width: oldwidth,
        //        height: oldheight,
        //        marginLeft: -(oldwidth / 2)
        //    });
        //    $(document).off("click", "#zoom .zoom-out");
        //    $("#zoom .zoom-out").removeClass("zoom-out").addClass("zoom-in");
        //    $(document).on("click", "#zoom .zoom-in", z);
        //});
    }

    // 图片大小以区域为准
    function c() {
        s = e(window).width();
        o = e(window).height();
    }

    var zoomStr = "";
    zoomStr += '<div id="zoom">';
    //zoomStr += '<a class="close"></a>';
    zoomStr += "<a class='zoom-in'></a>";
    zoomStr += '<a href="#previous" class="previous"></a>';
    zoomStr += '<a href="#next" class="next"></a>';
    zoomStr += '<div class="content loading"></div>';
    zoomStr += '</div>';
    e(".imglist-left").append(zoomStr);

    var t = e("#zoom").hide(),
		n = e("#zoom .content"),
		r = false,
		i = null,
		s = e(".imglist-left").width(),
		o = e(".imglist-left").height(),
        index = 0,
	    source = Enum.BusinessType.Moments;

    (function () {
        e(document).on("click", "#zoom", function (t) {
            t.preventDefault();
            //if (e(t.target).attr("id") == "zoom") l()
        });
        e(document).on("click", ".imglist-close", l);
        e(document).on("click", "#zoom .zoom-in", z);
        e(document).on("click", "#zoom .previous", a);
        e(document).on("click", "#zoom .next", f);
        e(document).keydown(function (e) {
            if (!i) return;
            if (e.which == 38 || e.which == 40) e.preventDefault();
            if (e.which == 27) l();
            if (e.which == 37 && !i.hasClass("zoom")) a();
            if (e.which == 39 && !i.hasClass("zoom")) f()
        });
        if (e(".gallery li a").length == 1) e(".gallery li a")[0].addClass("zoom");
        e(document).on("click", ".zoom, .gallery li a", u)
    })();

    // 窗口变化
    (function () {
        e(".imglist-left").on("resize", c)
    })();

    //初始化编辑框
    $("#txtCommentContentImg").miicWebEdit({
        id: "txtCommentContentImg",
        css: 'weibo-editor-comment',
        placeholder: "对他说些什么...",
        faceid: "aEmotionImg",
        submit: "commentSendImg",
        facePath: '../../images/arclist/', //表情存放的路径
        charAllowed: -1
    });
})(jQuery);
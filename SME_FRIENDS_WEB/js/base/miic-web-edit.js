// miicWebEdit插件 (含表情、提交按钮、字数提醒)
(function ($) {
    $.fn.miicWebEdit = function (options) {
        var defaults = {
            id: 'miicWebEdit',
            css: 'weibo-editor-short',
            placeholder: "请输入内容",
            faceid: "emotion",//-1时没有表情
            facePath: 'face/',
            submit: "submitbtn",
            charAllowed: 140,//-1时没有字数提示
            charWarning: 25,
            charCss: 'counter',
            charCounterElement: 'span'
        };

        var option = $.extend(defaults, options);
        var id = option.id;
        var css = option.css;
        var placeholder = option.placeholder;
        var submit = option.submit;
        var faceid = option.faceid;
        var facepath = option.facePath;
        var char_allowed = option.charAllowed;
        var char_warning = option.charWarning;
        var char_css = option.charCss;
        var char_counter_element = option.charCounterElement;

        var $this = document.getElementById(id);
        $this.GetCharObj = function () {
            return {
                charAllowed: char_allowed,
                charWarning: char_warning,
                charCss: char_css,
                charCounterElement: char_counter_element
            };
        }

        $this.GetFaceObj = function () {
            return {
                faceid: faceid,
                facePath: facepath,
                submit: submit
            };
        }

        $this.GetEditObj = function () {
            return {
                id: id,
                css: css,
                placeholder: placeholder
            };
        }

        //div可编辑
        if ($(this).attr("contenteditable") != "true") {
            $(this).attr("contenteditable", true);
        }

        //加载class
        if ($(this).hasClass(css) == false) {
            $(this).addClass(css);
        }

        //设置placeholder
        var $this = $(this);
        var $placeholder = $(this).clone();
        $(this).after($placeholder.removeAttr("id")).hide();//清除复制对象ID
        $placeholder.text(placeholder).focus(function (event) {//添加提示信息
            $this.show().focus();
            $placeholder.hide();
        });


        if (faceid != -1) {
            if ($("#" + faceid).length > 0) {
                //设置表情
                $("#" + faceid).miicFace({
                    id: 'facebox',
                    submit: submit,
                    assign: id,
                    path: facepath,
                    allowed: char_allowed,
                    charCss: char_css
                });
            }

            $(this).blur(function () {
                setTimeout(function () {//判断表情框，延迟判断
                    if ($("#facebox").length == 0) {
                        if ($this.html() == "<br>" || $this.html() == "") {
                            $this.hide();
                            $placeholder.show();
                        }
                    }
                }, 100);
            });
        }

        if (char_allowed != -1) {
            //计算字数
            $(this).miicCharCount({
                allowed: char_allowed,
                warning: char_warning,
                css: char_css,
                counterElement: char_counter_element,
                counterText: "还能输入&nbsp;",
                overText: "已经超出了&nbsp;"
            });
        }

        $(this).keyup(function (event) {
            $(this).setSubmitButton(id, submit, char_css, char_allowed);
        });
    };

    $.fn.miicFace = function (options) {
        var defaults = {
            id: 'facebox',
            path: 'face/',
            assign: 'content',
            submit: 'submitbtn',
            allowed: 140,
            char_css: "counter"
        };

        var option = $.extend(defaults, options);
        var assign = $('#' + option.assign);
        var id = option.id;
        var path = option.path;
        var submit = option.submit;
        var char_allowed = option.allowed;
        var char_css = option.charCss;

        if (assign.length <= 0) {
            alert('缺少表情赋值对象。');
            return false;
        }

        $(this).click(function (e) {
            if (assign.next().is(":hidden") == false) {
                assign.next().hide();
                assign.show().focus();
            }
            var strFace, labFace;
            if ($('#' + id).length <= 0) {
                strFace = '<div id="' + id + '" style="position:absolute;display:none;z-index:1000;" class="qqFace">' +
							  '<table border="0" cellspacing="0" cellpadding="0"><tr>';
                for (var i = 1; i <= 75; i++) {
                    labFace = "<img class=\\'emotion\\' src=" + path + (i < 10 ? '0' + i : i) + ".gif />";
                    strFace += '<td><img src="' + path + (i < 10 ? '0' + i : i) + '.gif" onclick="$(\'#' + option.assign + '\').setCaret();$(\'#' + option.assign + '\').insertAtCaret(\'' + labFace + '\');$(\'#' + option.assign + '\').setSubmitButton(\'' + option.assign + '\',\'' + submit + '\',\'' + char_css + '\',\'' + char_allowed + '\');" /></td>';
                    if (i % 15 == 0) strFace += '</tr><tr>';
                }
                strFace += '</tr></table></div>';
            }
            $(this).parent().append(strFace);
            var offset = $(this).position();
            var top = offset.top + $(this).outerHeight();
            $('#' + id).css('top', top);
            $('#' + id).css('left', offset.left);
            $('#' + id).show();
            e.stopPropagation();
        });

        $(document).click(function () {
            $('#' + id).hide();
            $('#' + id).remove();
        });

        //初始化禁用按钮
        $("#" + option.assign).setSubmitButton(option.assign, submit, char_css, char_allowed);
    };


    $.fn.miicCharCount = function (options) {
        // default configuration properties
        var defaults = {
            allowed: 140,
            warning: 25,
            css: 'counter',
            counterElement: 'span',
            cssWarning: 'warning',
            cssExceeded: 'exceeded',
            counterText: '',
            overText: ''
        };

        var options = $.extend(defaults, options);

        var $char = $("." + options.css);

        function calculate(obj) {
            //表情处理-start
            var reg = "";
            var times = 0;
            reg = "/<img/g";
            reg = eval(reg);
            if ($(obj).html().match(reg) != null) {
                times = $(obj).html().match(reg).length;
            }

            if ($(obj).text().match(reg) != null) {
                times -= $(obj).text().match(reg).length;
            }

            var count = $(obj).text().length + times;

            if ($(obj).text().length > 0) {
                //汉字占位符1
                var insert_first_space = " ";
                //汉字占位符2
                var insert_other_space = " ";
                var content_end = $(obj).text().charAt($(obj).text().length - 1);;
                if ($(obj).text().length == 1) {
                    if (insert_first_space == content_end) {
                        count--;
                    }
                } else {
                    if (insert_other_space == content_end) {
                        count--;
                    }
                }
            }

            var available = options.allowed - count;
            if (available <= options.warning && available >= 0) {
                $char.addClass(options.cssWarning);
            } else {
                $char.removeClass(options.cssWarning);
            }
            if (available < 0) {
                $char.addClass(options.cssExceeded);
            } else {
                $char.removeClass(options.cssExceeded);
            }
            if (available >= 0) {
                $char.html(options.counterText + "<span class='charType'>" + available + "</span>" + '&nbsp;字');
            } else {
                $char.html(options.overText + "<span class='charType'>" + Math.abs(available) + "</span>" + '&nbsp;字');
            }

            //------------所输入的文字超出定义值时出现提示---------xn---------
            messageChar = available;
            //--------------------------------
        };

        //------------当功能放入事件(例：click)时，提示信息不能重复显示的---------xn---------
        if ($char.length == 0) {
            return;
        }
        //var $tip = $(this).parent().find("." + options.css);
        //var $pre = $tip.prev();
        //$tip.remove();
        ////--------------------------------
        //$pre.after('<' + options.counterElement + ' class="' + options.css + '">' + options.counterText + '</' + options.counterElement + '>');
        calculate(this);
        $(this).keyup(function (event) {
            calculate(this);
        });
    };

})(jQuery);

jQuery.extend({
    unselectContents: function () {
        if (window.getSelection)
            window.getSelection().removeAllRanges();
        else if (document.selection)
            document.selection.empty();
    }
});

jQuery.fn.extend({
    setContents: function (content) {
        var $this = $($(this).get(0));
        $this.next().hide();
        $this.html(content).show().focus();

        var select;
        if (document.selection) {
            select = document.selection.createRange();
        } else if (document.getSelection) {
            select = document.getSelection();
        } else if (window.getSelection) {
            select = window.getSelection();
        }

        var explorer = navigator.userAgent.toLowerCase();
        if (explorer.match(/msie/) != null || explorer.match(/trident/) != null) {
            var range = select.getRangeAt(0);
            range.setStartAfter(select.focusNode, 0);
            select.removeAllRanges();
            select.addRange(range);
        }

        select.selectAllChildren(select.focusNode);

        if (explorer.match(/msie/) != null || explorer.match(/trident/) != null) {
            select.collapse(select.focusNode.parentNode, select.focusNode.parentNode.childNodes.length);
        } else {
            select.collapseToEnd();
        }

        var assign = $this.attr("id");
        var edit = document.getElementById(assign);
        var char_obj = edit.GetCharObj();
        var face_obj = edit.GetFaceObj();
        if (char_obj) {
            $this.miicCharCount({
                allowed: char_obj.charAllowed,
                warning: char_obj.charWarning,
                css: char_obj.charCss,
                counterElement: char_obj.charCounterElement,
                counterText: "还能输入&nbsp;",
                overText: "已经超出了&nbsp;"
            });
        }
        $this.setSubmitButton(assign, face_obj.submit, char_obj.charCss, char_obj.charAllowed);
    },

    getContents: function () {
        var content = $(this).html();
        return content;
    },

    getCharState: function () {
        var result = true;
        var valLength = $(this).text().length;
        var allowed = 140;
        var reg = "";
        var times = 0;
        reg = "/<img/g";
        reg = eval(reg);
        if ($(this).html().match(reg) != null) {
            times = $(this).html().match(reg).length;
        }

        if ($(this).text().match(reg) != null) {
            times -= $(this).text().match(reg).length;
        }

        valLength += times;

        if (valLength > allowed || valLength == 0) {
            result = false;
        }

        return result;
    },

    selectContents: function () {
        $(this).each(function (i) {
            var node = this;
            var selection, range, doc, win;
            if ((doc = node.ownerDocument) && (win = doc.defaultView) && typeof win.getSelection != 'undefined' && typeof doc.createRange != 'undefined' && (selection = window.getSelection()) && typeof selection.removeAllRanges != 'undefined') {
                range = doc.createRange();
                range.selectNode(node);
                if (i == 0) {
                    selection.removeAllRanges();
                }
                selection.addRange(range);
            } else if (document.body && typeof document.body.createTextRange != 'undefined' && (range = document.body.createTextRange())) {
                range.moveToElementText(node);
                range.select();
            }
        });
    },

    setCaret: function () {
        var initSetCaret = function () {
            var textObj = $(this).get(0);
            if (document.selection != undefined) {
                textObj.caretPos = document.selection.createRange().duplicate();
            } else {
                textObj.caretPos = window.getSelection();
            }
        };
        $(this).click(initSetCaret).select(initSetCaret).keyup(initSetCaret);
    },

    insertAtCaret: function (textFeildValue) {
        var textObj = $(this).get(0);

        var select = window.getSelection();

        if (select.rangeCount != 0 && ($(this).find(select.focusNode).length != 0 || select.focusNode == textObj)) {//光标在输入框中
            var range = select.getRangeAt(0);
            range.deleteContents();
            var hasr = range.createContextualFragment(textFeildValue);
            var hasr_last_child = hasr.lastChild;
            while (hasr_last_child && hasr_last_child.nodeName.toLowerCase() == "br" && hasr_last_child.previousSibling && hasr_last_child.previousSibling.nodeName.toLowerCase() == "br") {
                var e = hasr_last_child;
                hasr_last_child = hasr_last_child.previousSibling;
                hasr.removeChild(e)
            }
            range.insertNode(hasr);
            if (hasr_last_child) {
                range.setEndAfter(hasr_last_child);
                range.setStartAfter(hasr_last_child)
            }
            select.removeAllRanges();
            select.addRange(range);
            $(this).focus();
        } else {
            $(this).html($(this).html() + textFeildValue);
        }
    },

    insertImage: function (path) {
        var img = "<img  src='" + path + "'/>";
        $(this).setCaret();
        $(this).insertAtCaret(img);
        var $this = $($(this).get(0));
        var assign = $this.attr("id");
        var edit = document.getElementById(assign);
        var char_obj = edit.GetCharObj();
        var face_obj = edit.GetFaceObj();
        $this.setSubmitButton(assign, face_obj.submit, char_obj.charCss, char_obj.charAllowed);
    },

    setSubmitButton: function (assign, submit, char_css, char_allowed) {
        var $editor = $("#" + assign);
        var $submit = $("#" + submit);
        var valLength = $editor.text().length;
        var allowed = char_allowed;
        if (allowed != -1) {
            var reg = "";
            var times = 0;
            reg = "/<img/g";
            reg = eval(reg);
            if ($editor.html().match(reg) != null) {
                times = $editor.html().match(reg).length;
            }

            if ($editor.text().match(reg) != null) {
                times -= $editor.text().match(reg).length;
            }

            valLength += times;

            if (valLength > allowed || valLength == 0) {
                $submit.removeClass('btn-public-focus').attr("disabled", true);
            } else {
                $submit.addClass('btn-public-focus').attr("disabled", false);
            }

            var available = allowed - valLength;
            var $char = $("." + char_css);
            if (available <= 20 && available >= 0) {
                $char.addClass("warning");
            } else {
                $char.removeClass("warning");
            }
            if (available < 0) {
                $char.addClass("exceeded");
            } else {
                $char.removeClass("exceeded");
            }
            if (available >= 0) {
                $char.html("还能输入&nbsp;" + "<span class='charType'>" + available + "</span>" + '&nbsp;字');
            } else {
                $char.html("已经超出了&nbsp;" + "<span class='charType'>" + Math.abs(available) + "</span>" + '&nbsp;字');
            }
        } else {
            $submit.attr("disabled", false);
        }
    }
});
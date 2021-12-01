(function ($) {

    //字数限制
    $.fn.charCount = function (options) {
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
        function calculate(obj) {
            var count = $(obj).val().length;
            //表情处理-start
            var reg = "";
            var times = 0;
            reg = "/em_/g";
            reg = eval(reg);
            if ($(obj).val().match(reg) != null) {
                times = $(obj).val().match(reg).length;
            }
            //表情处理-end

            var available = options.allowed + 6 * times - count;
            if (available <= options.warning && available >= 0) {
                $(obj).next().addClass(options.cssWarning);
            } else {
                $(obj).next().removeClass(options.cssWarning);
            }
            if (available < 0) {
                $(obj).next().addClass(options.cssExceeded);
            } else {
                $(obj).next().removeClass(options.cssExceeded);
            }
            if (available >= 0) {
                $(obj).next().html(options.counterText + "<span class='charType'>" + available + "</span>" + '&nbsp;字');
            } else {
                $(obj).next().html(options.overText + "<span class='charType'>" + Math.abs(available) + "</span>" + '&nbsp;字');
            }

            //------------所输入的文字超出定义值时出现提示---------xn---------
            messageChar = available;
            //--------------------------------
        };
        this.each(function () {
            //------------当功能放入事件(例：click)时，提示信息不能重复显示的---------xn---------
            $(this).next().remove();
            //--------------------------------
            $(this).after('<' + options.counterElement + ' class="' + options.css + '">' + options.counterText + '</' + options.counterElement + '>');
            calculate(this);
            $(this).keyup(function () {
                calculate(this)
            });
            $(this).change(function () {
                calculate(this)
            });
        });

    };



})(jQuery);

/*------需要jquery-ui 并且向主页面添加 对话框------*/
//            <section class="dialog-normal">
//                <p class="dialog-content"></p>
//            </section>
//参数：
//     option: content || {content:content,width:width,height:height}
//     callback: function
//示例：
//     $.Alert("对话框内容")；
//     $.Alert("对话框内容", function () {
//          console.log("调用对话框回调函数");
//     });
//     $.Alert({
//           content:"对话框内容",
//           width:500,
//           height:200
//       });
//     $.Alert({
//           content:"对话框内容",
//           width:500,
//           height:200
//     }, function () {
//          console.log("调用对话框回调函数");
//     });
/*-----------------------------------------------*/
function miic_alert(option, callback) {
	if (option == undefined) {
		console.log("参数无效，异常的调用");
		return;
	}
	var width = "auto";
	var height = "auto";
	var content = "";
	if (typeof (option) === "string") {
		content = option;
		$(".dialog-content").html(content);
		$(".dialog-normal").dialog({
			resizable: true,
			minWidth: 300,
			minHeight: 150,
			modal: true,
			title: "提示窗口",
			dialogClass: "no-close",
			buttons: {
				"确　定": function () {
					$(this).dialog("close");
					if (callback != null && callback != undefined) {
						callback();
					}

				}
			}
		});
	} else {
		if (option.content != undefined) {
			content = option.content;
		}

		if (option.width != undefined) {
			width = option.width;
		}

		if (option.height != undefined) {
			height = option.height;
		}
		$(".dialog-content").html(content);
		$(".dialog-normal").dialog({
			resizable: true,
			width: width,
			height: height,
			modal: true,
			title: "提示窗口",
			dialogClass: "no-close",
			buttons: {
				"确　定": function () {
					$(this).dialog("close");
					if (callback != null && callback != undefined) {
						callback();
					}

				}
			}
		});
	}
}

/*------需要jquery-ui 并且向主页面添加 对话框------*/
//            <section class="dialog-normal">
//                <p class="dialog-content"></p>
//            </section>
//参数：
//     option: content || {content:content,width:width,height:height}
//     callback: function
//     cancelCallback: function
//示例：
//     $.Confrim("对话框内容", function () {
//          console.log("调用确认回调函数");
//     }, function () {
//          console.log("调用取消回调函数");
//     });
//     $.Confrim({
//           content:"对话框内容",
//           width:500,
//           height:200
//     }, function () {
//          console.log("调用确认回调函数");
//     }, function () {
//          console.log("调用取消回调函数");
//     });
/*-----------------------------------------------*/
function miic_confirm(option, callback, cancelCallback) {
	if (option == undefined) {
		console.log("参数无效，异常的调用");
		return;
	}
	var width = "auto";
	var height = "auto";
	if (typeof (option) === "string") {
		content = option;
		$(".dialog-content").html(content);
		$(".dialog-normal").dialog({
			resizable: true,
			minWidth: 300,
			minHeight: 150,
			modal: true,
			title: "确认窗口",
			dialogClass: "no-close",
			buttons: {
				"确　定": function () {
					$(this).dialog("close");
					if (callback != null && callback != undefined) {
						callback();
					}
				},
				"取　消": function () {
					$(this).dialog("close");
					if (cancelCallback != null && cancelCallback != undefined) {
						cancelCallback();
					}
				}
			}
		});
	} else {
		if (option.content != undefined) {
			content = option.content;
		}

		if (option.width != undefined) {
			width = option.width;
		}

		if (option.height != undefined) {
			height = option.height;
		}
		$(".dialog-content").html(content);
		$(".dialog-normal").dialog({
			resizable: true,
			width: width,
			height: height,
			modal: true,
			title: "确认窗口",
			dialogClass: "no-close",
			buttons: {
				"确　定": function () {
					$(this).dialog("close");
					if (callback != null && callback != undefined) {
						callback();
					}
				},
				"取　消": function () {
					$(this).dialog("close");
					if (cancelCallback != null && cancelCallback != undefined) {
						cancelCallback();
					}
				}
			}
		});
	}
}

/*------需要jquery-ui 并且向主页面添加 对话框------*/
//            <section class="dialog-normal">
//                <p class="dialog-content"></p>
//            </section>
//参数：
//     option: content || {content:content,width:width,height:height}
//     btns: btn obj
//示例：
//     $.DIY("对话框内容")；
//     $.DIY("对话框内容", {
//	     "确　定": function () {
//		     console.log("点击确定")
//	     }
//     });
//     $.DIY({
//           content:"对话框内容",
//           width:500,
//           height:200,
//           title:"自定义对话框"
//       });
//     $.DIY({
//           content:"对话框内容",
//           width:500,
//           height:200,
//           title:"自定义确认窗口"
//     }, {
//	     "确　定": function () {
//		     console.log("点击确定")
//	     }
//     });
/*-----------------------------------------------*/
function miic_diy_dialog(option, btns) {
	if (option == undefined) {
		console.log("参数无效，异常的调用");
		return;
	}
	var width = "auto";
	var height = "auto";
	var content = "";
	var title = "提示窗口";
	var buttons = {};

	function getBtnFunc(btnfunc) {
	    return function () {
	        $(this).dialog("close");
	        btnfunc();
	    }
	}

	if (!$.isEmptyObject(btns)) {
		for (var item in btns) {
			if (!!btns[item] && typeof btns[item] == "function") {
			    buttons[item] = getBtnFunc(btns[item]);
			}
		}

		if ($.isEmptyObject(buttons)) {
			buttons = {
				"确　定": function () {
					$(this).dialog("close");
				}
			};
		}
	} else {
		buttons = {
			"确　定": function () {
				$(this).dialog("close");
			}
		};
	}

	if (typeof (option) === "string") {
		content = option;
		$(".dialog-content").html(content);
		$(".dialog-normal").dialog({
			resizable: true,
			minWidth: 300,
			minHeight: 150,
			modal: true,
			title: title,
			dialogClass: "no-close",
			buttons: buttons
		});
	} else {
		if (option.content != undefined) {
			content = option.content;
		}

		if (option.width != undefined) {
			width = option.width;
		}

		if (option.height != undefined) {
			height = option.height;
		}

		if (option.title != undefined) {
			title = option.title;
		}
		$(".dialog-content").html(content);
		$(".dialog-normal").dialog({
			resizable: true,
			width: width,
			height: height,
			modal: true,
			title: title,
			dialogClass: "no-close",
			buttons: buttons
		});
	}
}

$.extend({
	"Alert": miic_alert,
	"Confirm": miic_confirm,
	"DIY": miic_diy_dialog
});


// Sticky v1.0 by Daniel Raftery
// http://thrivingkings.com/sticky
//
// http://twitter.com/ThrivingKings

(function($) {

	// Using it without an object
	$.sticky = function(note, options, callback) {
		return $.fn.sticky(note, options, callback);
	};
	$.fn.sticky = function(note, options, callback) {
		// Default settings
		var position = 'bottom-right'; // top-left, top-right, bottom-left, or bottom-right

		var settings = {
			'speed': 'fast', // animations: fast, slow, or integer
			'duplicates': true, // true or false
			'autoclose': false // integer or false
		};

		// Passing in the object instead of specifying a note
		if (!note&&note!=undefined) {
			note = this.html();
		}

		if (options) {
			$.extend(settings, options);
		}

		// Variables
		var display = true;
		var duplicate = 'no';

		// Somewhat of a unique ID
		var uniqID = Math.floor(Math.random() * 99999);
		// Handling duplicate notes and IDs
		$('.sticky-note').each(function() {
			if ($(this).html() == note && $(this).is(':visible')) {
				duplicate = 'yes';
				if (!settings['duplicates']) {
					display = false;
				}
			}
			if ($(this).attr('id') == uniqID) {
				uniqID = Math.floor(Math.random() * 9999999);
			}
		});

		// Make sure the sticky queue exists
		if (!$('body').find('.sticky-queue').html()) {
			$('body').append('<div class="sticky-queue ' + position + '"></div>');
		}

		// Can it be displayed?
		if (display) {
			// Building and inserting sticky note
			var alertStr = "";
			alertStr += '<div class="sticky border-' + position + '" id="' + uniqID + '">';
			alertStr += '<div class="ui-dialog-titlebar ui-widget-header ui-corner-all ui-helper-clearfix" style="cursor: default;">';
			alertStr += '<span id="ui-id-1" class="ui-dialog-title">即时消息</span>';
			alertStr += '<sapn class="ui-dialog-titlebar-close ui-corner-all" role="button">';
			alertStr += '<img src="../images/down.png" class="sticky-close" rel="' + uniqID + '" title="收起" />';
			alertStr += '<img src="../images/up.png" class="sticky-up" rel="' + uniqID + '" title="展开" />';
			alertStr += '</span>';
			alertStr += '</div>';
			$('.sticky-queue').prepend(alertStr);
			$('#' + uniqID).append('<div class="sticky-note" rel="' + uniqID + '">' + (note===undefined?"":note) + '</div>');

			// Smoother animation
			var height = $('#' + uniqID).height();
			$('#' + uniqID).css('height', height);

			$('#' + uniqID).slideDown(settings['speed']);
			display = true;
		}

		// Listeners
		$('.sticky').ready(function() {
			// If 'autoclose' is enabled, set a timer to close the sticky
			if (settings['autoclose']) {
				$('#' + uniqID).delay(settings['autoclose']).fadeOut(settings['speed']);
			}
		});


		// Closing a sticky
		$('.sticky-close').click(function() {
			var noteHeight = '';
			noteHeight = "-" + String($(".sticky-queue").height() - 50) + 'px';
			$(".sticky-queue").animate({bottom:noteHeight,opacity:'1'},500, function(){
				//改变图标
				$(this).find('img.sticky-close').hide().siblings('img.sticky-up').show();
				$('#goTop').css('bottom', "50px");
			});
		});


		// Open a sticky
		$('.sticky-up').click(function() {
			// console.log("up");
			$(".sticky-queue").animate({bottom:'0',opacity:'1'},500, function(){
				//改变图标
			    $(this).find('img.sticky-up').hide().siblings('img.sticky-close').show();
				$('#goTop').css('bottom', $('.sticky').height() + 10);
			});
		});

		

		// Callback data
		var response = {
			'id': uniqID,
			'duplicate': duplicate,
			'displayed': display,
			'position': position
		}

		// Callback function?
		if (callback) {
			callback(response);
		} else {
			return (response);
		}

	}
})(jQuery);
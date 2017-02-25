function adjustCalendar() {
	var width = $($('.calendarInDayCell')[0]).css('width');
	if (width == undefined) return;
	var w = width.substring(0, width.length - 2);
    var h = parseInt(w) * .8;
	$('.calendarInDayCell').css('height', h);
	$('.calendarInDayText').css('width', w);
	$('.calendarInDayText').css('height', h);
	$('.calendarInDayText').css('top', -h*.05);
	$('.calendarInDayText').css('font-size', h * .7);
	$('.calendarInDayTextHour').css('font-size', h * .34);
	$('.calendarInDayTextHourMins').css('font-size', h * .21);
	$('.calendarText').css('font-size', h * .34);
	var cms = +$('.calendarMonthSwitch').css('width').substring(0, width.length - 2);
    $('.calendarMonthSwitch').css('height', (cms * .8) + 'px');

    $('.calendar').css('font-size', w / 3.5 + 'px');

    $('.googlemap iframe').css('height', $('.calendarcontainer').css('height'));
}
var calendarMonths = ['styczeń', 'luty', 'marzec', 'kwiecień', 'maj', 'czerwiec', 'lipiec', 'sierpień', 'wrzesień', 'październik', 'listopad', 'grudzień'];
function zeroFill(number, width) {
    if (width == undefined) {
        width = 2;
    }
    width -= number.toString().length;
    if (width > 0) {
        return new Array(width + (/\./.test(number) ? 2 : 1)).join('0') + number;
    }
    return number + ""; // always return a string
}
function drawLegend(calendarDiv) {
    var cellHeight = 40;
    var legendDiv = $("<div class='legend' style='width: 100%; display:table-row'></div>");
    var legendEntries = [
        { start: 0, end: 0, content: 'wolne', price: '', priceDecimal: '' },
        { start: 0, end: 24, content: 'zajęte', price: '', priceDecimal: '' },
        { start: 0, end: 12, content: 'zajęty', price: 'do 11', priceDecimal: '00' },
        { start: 12, end: 24, content: 'zajęty', price: 'od 15', priceDecimal: '00' }
    ];
    for (var i = 0; i < legendEntries.length; i++) {
        var legendEntryDiv = $("<div class='legendEntry' style='width: 100%'></div>");
        var j = legendEntries[i];
        drawDayCell(legendDiv, j.start, j.end, j.content, j.price, j.priceDecimal, cellHeight);
    }
    
    var legendContainer = $("<div class='legendContainer' style='display:table; width: 57%'></div>");
    legendDiv.appendTo(legendContainer);
    legendContainer.insertAfter(calendarDiv);
    $("<div class='calendarText legend'>legenda:</div>").insertAfter(calendarDiv);
}
function drawDayCell(parent, start, end, content, price, priceDecimal, cellHeight, currency) {
    if (currency == undefined) {
        currency = '';
    }
    var dayCell = $("<div class='calendarDay'></div>").appendTo(parent);
    var inDayCell = $("<div style='height:" + cellHeight + "px' class='calendarInDayCell'></div>").appendTo(dayCell);
    $("<div class='calendarDayFree' style='width:" + ((start / 24) * 100) + "%;'></div>").appendTo(inDayCell);
    $("<div class='calendarDayTaken' style='width:" + (((end - start) / 24) * 100) + "%;'></div>").appendTo(inDayCell);
    $("<div class='calendarDayFree' style='width:" + (((24 - end) / 24) * 100) + "%;'></div>").appendTo(inDayCell);
    var text = content + '<br>';
    text += price + currency + "<span class='calendarInDayTextHourMins'>" + priceDecimal + "</span><br>";
    $("<div class='calendarInDayText calendarInDayTextHour'>" + text + "</div>").appendTo(inDayCell);
}
function setupCalendarTimeDiv(control) {
    control.html('');
    var calendarMonthCurr = window.innerWidth > 768 ?
            $("<div class='calendarMonthCurr calendarText' style='height:30px;width:50%; display:inline-block'>current</div><div class='calendarMonthFollow calendarText' style='height:30px;width:50%; display:inline-block'>current</div>") :
            $("<div class='calendarMonthCurr calendarText' style='height:30px;width:100%; display:inline-block'>current</div>");
    calendarMonthCurr.appendTo(control);
}
function setCalendarDate(control, addition) {
    var month = +control.data('month') + addition;
    var year = +control.data('year');
    var date = new Date(year, month);
    $('.calendarMonthCurr').text(calendarMonths[date.getMonth()] + ' ' + date.getFullYear());
    var date2 = new Date(year, month + 1);
    $('.calendarMonthFollow').text(calendarMonths[date2.getMonth()] + ' ' + date2.getFullYear());
    control.data('month', date.getMonth());
    control.data('year', date.getFullYear());
    return date;
}
$(window).resize(function () {
    setupCalendarTimeDiv($(".calendarMonthMiddle"));
    setCalendarDate($('.calendarTimeDiv'), 0);
	adjustCalendar();
});
$.fn.calendar = function() {
	var cellHeight = 40;
	function getLastDay(month, year) {
		return (new Date(year, month + 1, 0)).getDate();
	}
    
	this.drawCalendar = function (date, calendarDiv, reservationUrl) {
	    var calendarCallback = function (data) {
	        var reservedDays = data.reservedDays;
	        var prices = data.prices;
	        var firstDay = new Date(date.getFullYear(), date.getMonth(), 1);
	        var weekDaysRow = $("<div class='calendarWeekRow'></div>");
	        weekDaysRow.appendTo(calendarDiv);
	        var weekDays = ['Pn', 'Wt', 'Sr', 'Czw', 'Pt', 'So', 'Nd'];
	        for (var i = 0; i < weekDays.length; i++) {
	            $("<div style='display: table-cell'>" + weekDays[i] + "</div>").appendTo(weekDaysRow);
	        }
	        var weekDay = firstDay.getDay() - 1;
	        if (weekDay == -1) weekDay = 6;
	        var week = $("<div class='calendarWeekRow'></div>");
	        for (var i = 0; i < weekDay; i++) {
	            $("<div class='calendarDay'><div class='calendarEmptyDay'></div></div>").appendTo(week);
	        }
	        week.appendTo(calendarDiv);
	        var maxDay = getLastDay(date.getMonth(), date.getFullYear());
	        var start = 0;
	        var end = 0;
	        for (var i = 1; i <= maxDay; i++) {
	            if (reservedDays[i] == undefined) {
	                start = 0; end = 0;
	            } else {
	                start = reservedDays[i][0];
	                end = reservedDays[i][1];
	            }
	            drawDayCell(week, start, end, i, Math.floor(prices[i - 1]), '', cellHeight, ' zł'); //zeroFill(Math.ceil((prices[i - 1] % 1) * 100))
	            
	            if (weekDay == 6) {
	                week = $("<div class='calendarWeekRow'></div>");
	                week.appendTo(calendarDiv);
	                weekDay = 0;
	            } else {
	                weekDay++;
	            }
	        }
	        drawLegend(calendarDiv);
	        adjustCalendar();
	        calendarDiv.attr('loading', 'false');
	    };
	    if (calendarDiv.attr('loading') === 'true') {
	        return;
	    }
	    calendarDiv.attr('loading', 'true');
	    var reservationsUrl = reservationUrl + '&year=' + date.getFullYear() + '&month=' + (parseInt(date.getMonth()) + 1);
	    $.getJSON(reservationsUrl, function(data) {
	        calendarCallback(data);
	    });
	}

    this.dateChanged = function (event) {
        var calendarParent = event.data.calendarDiv.parent();
        var addition = event.data.addition;
        var control = calendarParent.find('.calendarTimeDiv');
        if (window.innerWidth > 768) {
            addition *= 2;
        }
        var date = setCalendarDate(control, addition);
        event.data.calendarDiv.html('');
        $('.legendContainer').remove();
        $('.legend').remove();

        if (window.innerWidth <= 768) {
            event.data.drawCalendar(date, event.data.calendarDiv);
        } else {
            var left = $("<div style='width:50%; display:table'></div>");
            var right = $("<div style='width:50%; display:table'></div>");
            left.attr('loading', 'false');
            right.attr('loading', 'false');
            left.appendTo(event.data.calendarDiv);
            right.appendTo(event.data.calendarDiv);
            event.data.drawCalendar(date, left, event.data.reservationUrl);
            event.data.drawCalendar(new Date(date.getFullYear(), date.getMonth() + 1), right, event.data.reservationUrl);
        }
    }

    this.setupCalendar = function (date, calendarDiv) {
        calendarDiv.attr('loading', 'false');
	    var calendarTimeDiv = $("<div class='calendarTimeDiv'><div>");
	    var calendarMonthPrev = $("<button class='calendarMonthSwitch calendarText btn-u' style='height:30px;width:10%; display:inline-block'>&lt</button>");
	    var calendarMonthNext = $("<button class='calendarMonthSwitch calendarText btn-u' style='height:30px;width:10%; display:inline-block'>&gt</button>");
	    var calendarMonthMiddle = $("<div class='calendarMonthMiddle calendarText' style='height:30px;width:80%; display:inline-block'></div>");
	    setupCalendarTimeDiv(calendarMonthMiddle);
	    calendarTimeDiv.data('month', date.getMonth());
	    calendarTimeDiv.data('year', date.getFullYear());
        var reservationUrl = calendarDiv.data('reservations-url');
        calendarMonthPrev.on('click', { drawCalendar: this.drawCalendar, calendarDiv: calendarDiv, addition: -1, reservationUrl: reservationUrl }, this.dateChanged);
        calendarMonthNext.on('click', { drawCalendar: this.drawCalendar, calendarDiv: calendarDiv, addition: 1, reservationUrl: reservationUrl }, this.dateChanged);
	    calendarMonthPrev.appendTo(calendarTimeDiv);
	    calendarMonthMiddle.appendTo(calendarTimeDiv);
	    calendarMonthNext.appendTo(calendarTimeDiv);
	    calendarTimeDiv.insertBefore(calendarDiv);
	    this.dateChanged({ data: { drawCalendar: this.drawCalendar, calendarDiv: calendarDiv, addition: 0, reservationUrl: reservationUrl } });
	}

	this.addClass('calendar');
	this.setupCalendar(new Date, this);
	return this;
}
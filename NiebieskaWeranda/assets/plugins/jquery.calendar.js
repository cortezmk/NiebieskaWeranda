function adjustCalendar() {
    var width = $($('.calendarInDayCell')[0]).css('width');
    if (width == undefined) return;
    var w = width.substring(0, width.length - 2);
    var h = parseInt(w) * .8;
    $('.calendarEmptyDay').css('height', h);
    $('.calendarInDayCell').css('height', h);
    $('.calendarInDayText').css('width', w);
    $('.calendarInDayText').css('height', h);
    $('.calendarInDayText').css('top', -h * .05);
    $('.calendarInDayText').css('font-size', h * .7);
    $('.calendarInDayTextHour').css('font-size', h * .34);
    $('.calendarInDayTextHourMins').css('font-size', h * .15);
    $('.calendarText').css('font-size', h * .34);
    $('.calendar-price').css('font-size', h * 0.25);
    $('.calendar-day').css('font-size', h * 0.425);
    $('.calendar-legend-day').css('font-size', h * 0.35);
    $('.calendar_loading').css('height', h * 8.5);
    $('.loading_margin').css('height', h * 1.5);
    var cms = +$('.calendarMonthSwitch').css('width').substring(0, width.length - 2);
    $('.calendarMonthSwitch').css('height', (cms * .8) + 'px');

    $('.calendar').css('font-size', w / 3.5 + 'px');
    $('.calendar').css('min-height', (h*7) + 'px');
}
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
        { start: 0, end: 12, content: 'zajęte', price: 'do 11', priceDecimal: '00' },
        { start: 12, end: 24, content: 'zajęte', price: 'od 15', priceDecimal: '00' }
    ];
    for (var i = 0; i < legendEntries.length; i++) {
        var legendEntryDiv = $("<div class='legendEntry' style='width: 100%'></div>");
        var j = legendEntries[i];
        drawLegendDayCell(legendDiv, j.start, j.end, j.content, j.price, j.priceDecimal, cellHeight);
    }

    var legendContainer = $("<div class='legendContainer' style='display:table; width: 57%'></div>");
    legendDiv.appendTo(legendContainer);
    legendContainer.insertAfter(calendarDiv);
    $("<div class='calendarText legend'>legenda:</div>").insertAfter(calendarDiv);
}
function drawLegendDayCell(parent, start, end, content, price, priceDecimal, cellHeight, currency) {
    if (currency == undefined) {
        currency = '';
    }
    var dayCell = $("<div class='calendarDay'></div>").appendTo(parent);
    var inDayCell = $("<div style='height:" + cellHeight + "px' class='calendarInDayCell'></div>").appendTo(dayCell);
    $("<div class='calendarDayFree' style='width:" + ((start / 24) * 100) + "%;'></div>").appendTo(inDayCell);
    $("<div class='calendarDayTaken' style='width:" + (((end - start) / 24) * 100) + "%;'></div>").appendTo(inDayCell);
    $("<div class='calendarDayFree' style='width:" + (((24 - end) / 24) * 100) + "%;'></div>").appendTo(inDayCell);
    var text = "<span class='calendar-legend-day calendarText legend'>" + content + "</span><span class='calendar-price'>";
    text += price + currency + "<span class='calendarInDayTextHourMins'>" + priceDecimal + "</span></span>";
    $("<div class='calendarInDayText calendarInDayTextHour'>" + text + "</div>").appendTo(inDayCell);
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
    var text = "<span class='calendar-day'>" + content + "</span><span class='calendar-price'>";
//    text += price + currency + "<span class='calendarInDayTextHourMins'>" + priceDecimal + "</span></span>";
    $("<div class='calendarInDayText calendarInDayTextHour'>" + text + "</div>").appendTo(inDayCell);
}
$(window).resize(function () {
    adjustCalendar();
});
$.fn.calendar = function(options) {
    var cellHeight = 40;
    var calendarMonths = ['styczeń', 'luty', 'marzec', 'kwiecień', 'maj', 'czerwiec', 'lipiec', 'sierpień', 'wrzesień', 'październik', 'listopad', 'grudzień'];
    function getLastDay(month, year) {
        return (new Date(year, month + 1, 0)).getDate();
    }

    this.drawCalendar = function (date, calendarDiv) {
        var calendarCallback = function (data) {
            calendarDiv.parent().find('.calendar_loading').hide();
            calendarDiv.show();
            var reservedDays = data.reservedDays;
            var prices = data.prices;
            var firstDay = new Date(date.getFullYear(), date.getMonth(), 1);
            var weekDaysRow = $("<div class='calendarWeekRow'></div>");
            weekDaysRow.appendTo(calendarDiv);
            var weekDays = ['Pn', 'Wt', 'Śr', 'Czw', 'Pt', 'So', 'Nd'];
            for (var i = 0; i < weekDays.length; i++) {
                $("<div style='display: table-cell'>" + weekDays[i] + "</div>").appendTo(weekDaysRow);
            }
            var weekDay = firstDay.getDay() - 1;
            if (weekDay == -1) weekDay = 6;
            var week = $("<div class='calendarWeekRow'></div>");
            var totalDays = 0;
            for (var i = 0; i < weekDay; i++) {
                $("<div class='calendarDay'><div class='calendarEmptyDay'></div></div>").appendTo(week);
                totalDays++;
            }
            week.appendTo(calendarDiv);
            var maxDay = getLastDay(date.getMonth(), date.getFullYear());
            var start = 0;
            var end = 0;
            
            for (var i = 1; i <= maxDay; i++) {
                totalDays++;
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
            if (totalDays <= 35) {
                week = $("<div class='calendarWeekRow'></div>");
                $("<div class='calendarDay'><div class='calendarEmptyDay'></div></div>").appendTo(week);
                week.appendTo(calendarDiv);
            }
            if (calendarDiv.data('direction') !== 'right') {
                drawLegend(calendarDiv);
            }
            adjustCalendar();
            calendarDiv.attr('loading', 'false');
        };
        if (calendarDiv.attr('loading') === 'true') {
            return;
        }
        calendarDiv.attr('loading', 'true');
        calendarDiv.parent().find('.calendar_loading').show();
        calendarDiv.hide();
        var reservationsUrl = calendarDiv.data('reservations-url') + '&year=' + date.getFullYear() + '&month=' + (parseInt(date.getMonth()) + 1);
        $.getJSON(reservationsUrl, function (data) {
            calendarCallback(data);
        });
    }

    this.dateChanged = function (event) {
        var calendarDivs = $(".calendar");
        for (var i = 0; i < calendarDivs.length; i++) {
            var calendarParent = $(calendarDivs[i]).parent();
            var addition = event.data.addition;
            var control = calendarParent.find('.calendarMonthCurr');
            var month = +control.data('month') + addition;
            var year = +control.data('year');
            var date = new Date(year, month);
            var calendarAddition = $(calendarDivs[i]).data('addition');
            if (calendarAddition == undefined) {
                calendarAddition = 0;
            }
            var date2 = new Date(date.getFullYear(), date.getMonth() + calendarAddition, 1);
            control.text(calendarMonths[date2.getMonth()] + ' ' + date2.getFullYear());
            control.data('month', date.getMonth());
            control.data('year', date.getFullYear());
            $(calendarDivs[i]).html('');
            $('.legendContainer').remove();
            $('.legend').remove();
            event.data.drawCalendar(date2, $(calendarDivs[i]));
        }
    }

    this.setupCalendar = function (date, calendarDiv, options) {
        calendarDiv.addClass('calendar-' + options.direction);
        calendarDiv.data('direction', options.direction);
        if (options.direction === 'right') {
            calendarDiv.data('addition', 1);
        }
        calendarDiv.attr('loading', 'false');
        var loadingPanel = $("<div style='width: 100%; display: none; text-align: center;' class='calendar_loading'><div class='loading_margin'></div><img src='/assets/img/loading.gif'></div>");
        loadingPanel.appendTo(calendarDiv.parent());
        var calendarTimeDiv = $('<div><div>');
        var calendarMonthCurr = null;
        var calendarMonthPrev = $("<button class='calendarMonthSwitch calendarText' style='height:30px;width:10%; display:inline-block'>&lt</button>");
        var calendarMonthNext = $("<button class='calendarMonthSwitch calendarText' style='height:30px;width:10%; display:inline-block'>&gt</button>");
        if (options.direction === 'center') {
            calendarMonthCurr = $("<div class='calendarMonthCurr calendarText' style='height:30px;width:80%; display:inline-block'>current</div>");
        } else {
            calendarMonthCurr = $("<div class='calendarMonthCurr calendarText' style='height:30px;width:90%; display:inline-block'>current</div>");
        }
        calendarMonthCurr.data('month', date.getMonth());
        calendarMonthCurr.data('year', date.getFullYear());
        var additionMultiplier = options.direction === 'center' ? 1 : 2;
        calendarMonthPrev.on('click', { drawCalendar: this.drawCalendar, calendarDiv: calendarDiv, addition: -1 * additionMultiplier }, this.dateChanged);
        calendarMonthNext.on('click', { drawCalendar: this.drawCalendar, calendarDiv: calendarDiv, addition: 1 * additionMultiplier }, this.dateChanged);
        if (options.direction !== 'right') {
            calendarMonthPrev.appendTo(calendarTimeDiv);
        }
        calendarMonthCurr.appendTo(calendarTimeDiv);
        if (options.direction !== 'left') {
            calendarMonthNext.appendTo(calendarTimeDiv);
        }
        calendarTimeDiv.insertBefore(calendarDiv);
        this.dateChanged({ data: { drawCalendar: this.drawCalendar, calendarDiv: calendarDiv, addition: 0 } });
    }

    this.addClass('calendar');
    this.setupCalendar(new Date, this, options);
    return this;
}
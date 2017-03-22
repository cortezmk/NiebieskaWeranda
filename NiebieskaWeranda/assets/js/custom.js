/* Write here your custom javascript codes */
function setPrice() {
    if ($('#ArrivalDate').val() === '' || $('#DepartureDate').val() === '') {
        return;
    }
    $.ajax({
        type: 'POST',
        url: $('#totalPrice').data('get-price-url'),
        data: {
            apartmentName: $('#apartmentName').text(),
            dateFrom: $('#ArrivalDate').val(),
            dateTo: $('#DepartureDate').val(),
            numberOfPersons: $('#NumberOfPersons').val()
        },
        success: function(data) {
            if (data.success) {
                $('#totalPrice').text(data.totalPrice);
                $('#numNights').text(data.totalNights);
                $('.reservationStatistics').show();
            } else {
                $('.reservationStatistics').hide();
            }
        }
    });
}

$(function () {
    (function ($) {
        $.fn.goTo = function () {
            $('html, body').animate({
                scrollTop: $(this).offset().top + 'px'
            }, 'fast');
            return this; // for chaining...
        }
    })(jQuery);

    function applyFooter() {
        $('.wrapper').css('min-height', window.innerHeight - 70);
    }

    applyFooter();
    $(window).resize(function() {
        applyFooter();
    });
    $("input[type='datetime'], input[type='text'], input[type='number'], input[type='email'], input[type='datetime'], input[type='submit'], select").addClass("form-control");

    $('#ArrivalDate').on('change', function() { setPrice(); });
    $('#DepartureDate').on('change', function () { setPrice(); });
    $('#NumberOfPersons').on('change', function () { setPrice(); });

    $("#ArrivalDate").addClass("type-date");
    $("#DepartureDate").addClass("type-date");
    $("input[type='datetime']").attr("type", "text");
    $(".type-date").datepicker({
        format: 'dd.MM.yyyy',
        language: 'pl',
        autoclose: true,
        todayHighlight: true
    });

    if ($('.field-register-success').length > 0) {
        $('.field-register-success').goTo();
    }
    if ($('.generic-error-message').length > 0) {
        $('.generic-error-message').goTo();
    }
//    $('#NumberOfPersons').attr('max', '10').attr('min', 1);
    setPrice();
});

//function formatDate(control) {
//    var split = control.val().split(".");
//    control.val(split[2] + "-" + split[1] + "-" + split[0]);
//}
//
//$("#apartmentForm").submit(function() {
//    formatDate($("#ArrivalDate"));
//    formatDate($("#DepartureDate"));
//});
/* ========================================================================================================== */
//                      Mobile Menu   
/* ========================================================================================================== */
$(document).ready(function () {

    //Show Mobile Navbar
    $("#mobile-nav-open-btn").click(function () {

        $("#mobile-nav").css("height", "100%");
        $("#mobile-nav-open-btn").css("display", "none");

    });

    //Hide Mobile Navbar
    $("#mobile-nav-close-btn, #mobile-nav a").click(function () {

        $("#mobile-nav").css("height", "0");
        $("#mobile-nav-open-btn").css("display", "block");

    });

});

$(document).ready(function () {

    $("nav .navbar-toggler").click(function () {
        $("body").toggleClass("mobile-menu-opened");
        $("nav.navbar").toggleClass("navbar-scroll-content");
        $("nav.navbar").toggleClass("navbar-fixed-height");
    });

});

$(document).ready(function () {

    if ($(window).width() < 991) {
        $('#navbarSupportedContent').addClass('animate__animated animate__slideInLeft animate__faster');
    } else {
        $('#navbarSupportedContent').removeClass('animate__animated animate__slideInLeft animate__faster');
    }

});

/* ========================================================================================================== */
//                      Login, Signup, Forgot-Password, Change-Password
/* ========================================================================================================== */

//Password Show-Hide
$(document).ready(function () {

    $("body").on('click', '.toggle-password', function () {

        var input = $($(this).attr("toggle"));

        if (input.attr("type") === "password") {
            input.attr("type", "text");
        } else {
            input.attr("type", "password");
        }

    });

});

/* ========================================================================================================== */
//                      Rejected Notes
/* ========================================================================================================== */
$(document).ready(function () {

    $("#rejectednotes-seller").change(function () {
        this.form.submit();
    });

});

/* ========================================================================================================== */
//                      Published Notes
/* ========================================================================================================== */
$(document).ready(function () {

    $("#publishednotes-seller").change(function () {
        this.form.submit();
    });

});

/* ========================================================================================================== */
//                      Downloaded Notes
/* ========================================================================================================== */
$(document).ready(function () {

    $("#downloadednotes-note").change(function () {
        this.form.submit();
    });
    $("#downloadednotes-seller").change(function () {
        this.form.submit();
    });
    $("#downloadednotes-buyer").change(function () {
        this.form.submit();
    });

});

/* ========================================================================================================== */
//                      Notes Under Review Notes
/* ========================================================================================================== */
$(document).ready(function () {

    $("#notesunderreview-seller").change(function () {
        this.form.submit();
    });

});

/* ========================================================================================================== */
//                      Dashboard
/* ========================================================================================================== */
$(document).ready(function () {

    $("#dashboard-month").change(function () {
        this.form.submit();
    });

});
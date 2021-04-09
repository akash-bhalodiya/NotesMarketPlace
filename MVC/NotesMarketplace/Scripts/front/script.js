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
        $("nav.navbar").addClass("white-navbar");
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
//                      FAQ
/* ========================================================================================================== */
$(document).ready(function () {
    // Add minus icon for collapse element which is open by default
    $(".collapse.show").each(function () {
        $(this).prev(".card-header").find(".img-faq").attr("src", "../../images/FAQ/add.png");
        $(this).parentsUntil(".card").css({
            "border": "1px solid #d1d1d1"
        });

    });

    // Toggle plus minus icon on show hide of collapse element
    $(".collapse").on('show.bs.collapse', function () {

        $(this).prev(".card-header").find(".img-faq").attr("src", "../../images/FAQ/minus.png").css({
            "height": "23px",
            "width": "35px"
        });
        $(this).prev(".card-header").find("h6").css({
            "font-weight": "600"
        });
        $(this).prev(".card-header").css({
            "background": "white"
        });
        $(this).parent(".card").css("border", "1px solid #d1d1d1");

    }).on('hide.bs.collapse', function () {
        $(this).prev(".card-header").find(".img-faq").attr("src", "../../images/FAQ/add.png").css({
            "height": "38px",
            "width": "38px"
        });
        $(this).prev(".card-header").find("h6").css({
            "font-weight": "400"
        });
        $(this).prev(".card-header").css({
            "background": "#f3f3f3"
        });
        $(this).parent(".card").css("border", "none");

    });
});
/* ========================================================================================================== */
//                      Add Note
/* ========================================================================================================== */
$(document).ready(function () {
        $("#add-form input[name='IsPaid']").change(function () {
            if ($("#paid").is(":checked")) {
                $("#price").removeAttr("disabled");
                $("#price").focus();
                $("#note-preview").attr("required", "required");
            } else {
                $("#price").val(0);
                $("#price").attr("disabled", "disabled");
                $("#note-preview").removeAttr("required");
            }
        });
});

$(document).ready(function () {
    $("#edit-form input[name='IsPaid']").change(function () {
        if ($("#paid").is(":checked")) {
            $("#price").removeAttr("disabled");
            $("#price").focus();
        } else {
            $("#price").val(0);
            $("#price").attr("disabled", "disabled");
        }
    });
});

/* ========================================================================================================== */
//                      Profile
/* ========================================================================================================== */
$(document).ready(function () {
    $("#date-of-birth").each(function () {
        $(this).datepicker({
            changeMonth: true,
            changeYear: true,
            minDate: new Date(1960,1 - 1,1),
            yearRange: '1960:2030',
            dateFormat: 'yy-mm-dd'
        });
    });
});
/* ========================================================================================================== */
//                      Search Note
/* ========================================================================================================== */
$(document).ready(function () {

    $(".search-filter #type").change(function () {
        this.form.submit();
    });
    $(".search-filter #search").change(function () {
        this.form.submit();
    });
    $(".search-filter #category").change(function () {
        this.form.submit();
    });
    $(".search-filter #university").change(function () {
        this.form.submit();
    });
    $(".search-filter #course").change(function () {
        this.form.submit();
    });
    $(".search-filter #country").change(function () {
        this.form.submit();
    });
    $(".search-filter #rating").change(function () {
        this.form.submit();
    });

});
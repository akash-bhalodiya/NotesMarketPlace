/* ========================================================================================================== */
//                      White Navigation Bar   
/* ========================================================================================================== */
$(function () {

    navchange();

    $(window).scroll(function () {
        navchange();
    });

    function navchange() {

        if ($(window).scrollTop() > 50) {
            //  Show White Nav
            $("nav").addClass("white-navbar");
            //  Hide White Logo
            $(".navbar-brand img").attr("src", "../../images/Logos/top-logo-dark.png");
            //  Hide white Mobile Nav Open Button
            $(".mobile-nav-open-btn").css("color", "#6255a5");

        } else {
            //  Hide White Nav
            $("nav").removeClass("white-navbar");
            //  Show White Logo
            $(".navbar-brand img").attr("src", "../../images/Logos/top-logo-white.png");
            //  Show White Mobile Nav Open Button
            $(".mobile-nav-open-btn").css("color", "#fff");

        }
    }

});

$("nav .navbar-toggler").click(function () {

    if ($(window).scrollTop() > 50) {
        $("body").toggleClass("mobile-menu-opened");
        $("nav.navbar").toggleClass("navbar-fixed-height");
        $("nav.navbar").toggleClass("navbar-scroll-content");
        $("nav.navbar").addClass("white-navbar");

    } else {
        $("body").toggleClass("mobile-menu-opened");
        $("nav.navbar").toggleClass("white-navbar");
        $("nav.navbar").toggleClass("navbar-fixed-height");
        $("nav.navbar").toggleClass("navbar-scroll-content");

        if ($("nav.navbar").hasClass("white-navbar")) {
            $(".navbar-brand img").attr("src", "../../images/Logos/top-logo-dark.png");
        } else {
            $(".navbar-brand img").attr("src", "../../images/Logos/top-logo-white.png");
        }

    }

});

$(document).ready(function () {


    if ($(window).width() < 991) {
        $('#navbarSupportedContent').addClass('animate__animated animate__slideInLeft animate__faster');
    } else {
        $('#navbarSupportedContent').removeClass('animate__animated animate__slideInLeft animate__faster');
    }

});
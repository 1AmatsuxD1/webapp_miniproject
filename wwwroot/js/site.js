// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


const nav = document.getElementById("mainNav");
if (nav) {
    const setPad = () => {
        let height = nav.offsetHeight;
        height += parseInt(getComputedStyle(nav).marginBottom) || 0;
        document.body.style.paddingTop = height + "px";
    };
    setPad();
    new ResizeObserver(setPad).observe(nav);
    window.addEventListener("load", setPad);
}
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

document.addEventListener("click", (event) => {
    const trigger = event.target.closest('[data-bs-toggle="collapse"]');
    if (!trigger) {
        return;
    }

    const targetSelector = trigger.getAttribute("data-bs-target");
    if (!targetSelector) {
        return;
    }

    const target = document.querySelector(targetSelector);
    if (!target) {
        return;
    }

    event.preventDefault();

    const willShow = !target.classList.contains("show");
    target.classList.toggle("show", willShow);
    trigger.setAttribute("aria-expanded", willShow ? "true" : "false");
    trigger.classList.toggle("collapsed", !willShow);

    if (nav && target.id === "navbarNav") {
        requestAnimationFrame(() => {
            let height = nav.offsetHeight;
            height += parseInt(getComputedStyle(nav).marginBottom, 10) || 0;
            document.body.style.paddingTop = height + "px";
        });
    }
});
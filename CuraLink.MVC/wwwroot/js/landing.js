(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        var nav = document.getElementById("lpNav");
        var btn = document.getElementById("lpToggle");
        var menu = document.getElementById("lpMobile");
        if (!nav) return;

        function onScroll() {
            nav.classList.toggle("is-scrolled", window.scrollY > 8);
        }
        onScroll();
        window.addEventListener("scroll", onScroll, { passive: true });

        if (!btn || !menu) return;

        function setOpen(open) {
            menu.classList.toggle("open", open);
            btn.setAttribute("aria-expanded", String(open));
            btn.querySelector("i").className = open ? "bi bi-x-lg" : "bi bi-list";
        }

        btn.addEventListener("click", function () {
            setOpen(!menu.classList.contains("open"));
        });
        menu.addEventListener("click", function (e) {
            if (e.target.closest("a")) setOpen(false);
        });
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape") setOpen(false);
        });
        window.addEventListener("resize", function () {
            if (window.innerWidth >= 992) setOpen(false);
        });
    });
})();
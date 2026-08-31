(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        const sidebar = document.getElementById("adminSidebar");
        const toggle = document.getElementById("sidebarToggle");
        const backdrop = document.getElementById("sidebarBackdrop");

        if (!sidebar || !toggle || !backdrop) return;

        function closeSidebar() {
            sidebar.classList.remove("open");
            backdrop.classList.remove("open");
        }

        toggle.addEventListener("click", function () {
            sidebar.classList.toggle("open");
            backdrop.classList.toggle("open");
        });

        backdrop.addEventListener("click", closeSidebar);
    });
})();
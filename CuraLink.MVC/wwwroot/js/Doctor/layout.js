// =========================================================
// CuraLink — Doctor Dashboard Layout
// Scope: Views/Shared/_DoctorLayout.cshtml only.
// Handles the off-canvas sidebar toggle on tablet/mobile.
// Self-contained: does not bind to anything auth.js or
// admin/layout.js also touch, so there is no double-binding risk.
// =========================================================

(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        var sidebar = document.getElementById("doctorSidebar");
        var toggleBtn = document.getElementById("sidebarToggle");
        var backdrop = document.getElementById("sidebarBackdrop");

        if (!sidebar || !toggleBtn || !backdrop) return;

        function openSidebar() {
            sidebar.classList.add("open");
            backdrop.classList.add("open");
        }

        function closeSidebar() {
            sidebar.classList.remove("open");
            backdrop.classList.remove("open");
        }

        toggleBtn.addEventListener("click", function () {
            var isOpen = sidebar.classList.contains("open");
            if (isOpen) {
                closeSidebar();
            } else {
                openSidebar();
            }
        });

        backdrop.addEventListener("click", closeSidebar);

        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape") closeSidebar();
        });
    });
})();

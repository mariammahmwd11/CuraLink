// =========================================================
// CuraLink — Patient Dashboard Layout
// Scope: Views/Shared/_PatientLayout.cshtml only.
// Self-contained: no shared bindings with auth.js, admin/layout.js,
// or doctor/layout.js.
// =========================================================

(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        var sidebar = document.getElementById("patientSidebar");
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
            sidebar.classList.contains("open") ? closeSidebar() : openSidebar();
        });

        backdrop.addEventListener("click", closeSidebar);

        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape") closeSidebar();
        });
    });
})();
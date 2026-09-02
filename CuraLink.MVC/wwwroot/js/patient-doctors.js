// =========================================================
// CuraLink — Patient: Doctors Directory
// Scope: Views/Patient/Doctors.cshtml only.
//
// Search/filter and pagination are plain server-side GET requests
// (full page navigations) per spec — no client-side pagination and
// no direct API calls from here. This file only adds a subtle loading
// indicator so the page doesn't feel frozen while the next page loads.
// =========================================================

(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        var form = document.getElementById("doctorSearchForm");
        var applyBtn = document.getElementById("applyFiltersBtn");
        var spinner = document.getElementById("searchSpinner");

        if (!form || !applyBtn || !spinner) return;

        form.addEventListener("submit", function () {
            applyBtn.disabled = true;
            spinner.classList.remove("d-none");
        });

        // Pagination links are plain <a> tags that trigger a full navigation;
        // show the same subtle loading affordance on the button area they
        // came from isn't practical without a shared shell, so we rely on
        // the browser's own navigation/loading indicator for those.
    });
})();
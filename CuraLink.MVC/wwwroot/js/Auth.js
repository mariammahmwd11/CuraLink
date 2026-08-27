// =========================================================
// CuraLink — Auth Pages JS
// Scope: Views/Auth/Login.cshtml, Views/Auth/RegisterPatient.cshtml
// No framework dependency beyond the DOM. Safe to load once,
// site-wide — it no-ops on pages without these elements.
// =========================================================

(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        initPasswordToggles();
    });

    // ---------------------------------------------------
    // Show / hide password toggle buttons.
    // Works for any button with [data-toggle-target="<input id>"].
    // ---------------------------------------------------
    function initPasswordToggles() {
        var toggleButtons = document.querySelectorAll("[data-toggle-target]");

        toggleButtons.forEach(function (btn) {
            btn.addEventListener("click", function () {
                var targetId = btn.getAttribute("data-toggle-target");
                var input = document.getElementById(targetId);
                if (!input) return;

                var icon = btn.querySelector("i");
                var isHidden = input.getAttribute("type") === "password";

                input.setAttribute("type", isHidden ? "text" : "password");

                if (icon) {
                    icon.classList.toggle("bi-eye", !isHidden);
                    icon.classList.toggle("bi-eye-slash", isHidden);
                }

                btn.setAttribute("aria-label", isHidden ? "Hide password" : "Show password");
            });
        });
    }

   
   
})();

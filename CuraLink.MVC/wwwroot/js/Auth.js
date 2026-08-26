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
        initPasswordStrengthMeter();
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

    // ---------------------------------------------------
    // Password strength meter (Registration page only).
    // Purely a UX aid — real validation still happens via
    // DataAnnotations / jQuery unobtrusive validation and,
    // ultimately, server-side rules.
    // ---------------------------------------------------
    function initPasswordStrengthMeter() {
        var passwordInput = document.getElementById("registerPassword");
        var fill = document.getElementById("passwordStrengthFill");
        var label = document.getElementById("passwordStrengthLabel");
        var requirementItems = document.querySelectorAll("#passwordRequirements li");

        if (!passwordInput || !fill || !label) return;

        var rules = {
            length: function (v) { return v.length >= 8; },
            upperLower: function (v) { return /[a-z]/.test(v) && /[A-Z]/.test(v); },
            number: function (v) { return /\d/.test(v); },
            special: function (v) { return /[^A-Za-z0-9]/.test(v); }
        };

        var levels = [
            { min: 0, width: "0%", color: "var(--cl-border)", text: "Password strength" },
            { min: 1, width: "25%", color: "var(--cl-danger)", text: "Weak" },
            { min: 2, width: "50%", color: "var(--cl-warning)", text: "Fair" },
            { min: 3, width: "75%", color: "var(--cl-warning)", text: "Good" },
            { min: 4, width: "100%", color: "var(--cl-success)", text: "Strong" }
        ];

        passwordInput.addEventListener("input", function () {
            var value = passwordInput.value;
            var passedCount = 0;

            requirementItems.forEach(function (item) {
                var ruleName = item.getAttribute("data-rule");
                var ruleFn = rules[ruleName];
                var passed = ruleFn ? ruleFn(value) : false;

                item.classList.toggle("met", passed);
                if (passed) passedCount++;
            });

            var level = levels[0];
            for (var i = levels.length - 1; i >= 0; i--) {
                if (passedCount >= levels[i].min) {
                    level = levels[i];
                    break;
                }
            }

            fill.style.width = level.width;
            fill.style.backgroundColor = level.color;
            label.textContent = value.length === 0 ? "Password strength" : level.text;
        });
    }
})();

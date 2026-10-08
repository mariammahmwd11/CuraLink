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
        initPatientPasswordValidation();
    });
    function initPatientPasswordValidation() {
        var passwordInput = document.getElementById("registerPassword");
        var confirmInput = document.getElementById("registerConfirmPassword");

        if (!passwordInput || !confirmInput) return;

        var strengthFill = document.getElementById("passwordStrengthFill");
        var strengthLabel = document.getElementById("passwordStrengthLabel");
        var requirements = document.querySelectorAll("#passwordRequirements li");

        function updatePasswordStrength() {
            var password = passwordInput.value;

            var rules = {
                length: password.length >= 8,
                upperLower: /[a-z]/.test(password) && /[A-Z]/.test(password),
                number: /\d/.test(password),
                special: /[^A-Za-z0-9]/.test(password)
            };

            var passedCount = 0;

            requirements.forEach(function (item) {
                var ruleName = item.getAttribute("data-rule");
                var passed = rules[ruleName] === true;

                item.classList.toggle("met", passed);

                if (passed) {
                    passedCount++;
                }
            });

            if (!password) {
                strengthFill.style.width = "0%";
                strengthFill.style.backgroundColor = "var(--cl-border)";
                strengthLabel.textContent = "Password strength";
                return;
            }

            var levels = [
                {
                    width: "25%",
                    color: "var(--cl-danger)",
                    text: "Weak"
                },
                {
                    width: "50%",
                    color: "var(--cl-warning)",
                    text: "Fair"
                },
                {
                    width: "75%",
                    color: "var(--cl-warning)",
                    text: "Good"
                },
                {
                    width: "100%",
                    color: "var(--cl-success)",
                    text: "Strong"
                }
            ];

            var level = levels[passedCount - 1];

            strengthFill.style.width = level.width;
            strengthFill.style.backgroundColor = level.color;
            strengthLabel.textContent = level.text;
        }

        function validateConfirmPassword() {
            if (!confirmInput.value) {
                confirmInput.classList.remove("is-valid", "is-invalid");
                return;
            }

            if (passwordInput.value === confirmInput.value) {
                confirmInput.classList.add("is-valid");
                confirmInput.classList.remove("is-invalid");
            } else {
                confirmInput.classList.add("is-invalid");
                confirmInput.classList.remove("is-valid");
            }
        }

        passwordInput.addEventListener("input", function () {
            updatePasswordStrength();
            validateConfirmPassword();
        });

        confirmInput.addEventListener("input", function () {
            validateConfirmPassword();
        });

        updatePasswordStrength();
    }

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

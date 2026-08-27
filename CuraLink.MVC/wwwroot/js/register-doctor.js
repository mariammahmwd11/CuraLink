/**
 * CuraLink — Doctor Registration
 * Handles:
 * - Client-side validation
 * - File upload & preview
 * - Password strength
 * - API submission
 *
 * NOTE: password show/hide toggling is handled globally by auth.js
 * (it binds to any [data-toggle-target] button on every auth page).
 * This file intentionally does NOT bind its own toggle handler —
 * doing so on both files double-fires the click and cancels itself out.
 */

(function () {
    'use strict';

    // ============================================================
    // CONFIGURATION
    // ============================================================

    const CONFIG = {
        MAX_FILE_SIZE: 5 * 1024 * 1024, // 5 MB
        ALLOWED_FILE_TYPES: ['image/jpeg', 'image/png', 'application/pdf'],
        ALLOWED_FILE_EXTENSIONS: ['.jpg', '.jpeg', '.png', '.pdf']
    };

    // ============================================================
    // STATE
    // ============================================================

    const touchedFields = new Set();
    let formSubmitted = false;

    // ============================================================
    // DOM ELEMENTS
    // ============================================================

    const form = document.getElementById('doctorRegisterForm');

    if (!form) {
        console.error('[register-doctor.js] #doctorRegisterForm was not found — aborting setup.');
        return;
    }

    // Collect every element we depend on in one place, by id, so a
    // missing/renamed id is reported clearly instead of throwing a
    // bare "Cannot read properties of null" somewhere downstream.
    const ids = [
        'submitBtn', 'submitSpinner', 'submitBtnLabel',
        'registerAlertPlaceholder', 'registrationSuccess',
        'LicenseDocument', 'uploadDropzone', 'uploadPreview', 'uploadPrompt',
        'uploadImagePreview', 'uploadFileIcon', 'uploadFileName', 'uploadFileSize',
        'removeFileBtn', 'fileError',
        'FirstName', 'LastName', 'Email', 'PhoneNumber', 'Specialty', 'SyndicateId',
        'Password', 'ConfirmPassword', 'ConfirmAccuracy',
        'passwordStrengthFill', 'passwordStrengthLabel',
        'firstNameError', 'lastNameError', 'emailError', 'phoneError',
        'specialtyError', 'syndicateIdError', 'passwordError',
        'confirmPasswordError', 'termsError'
    ];

    const el = {};
    const missingIds = [];

    ids.forEach(function (id) {
        el[id] = document.getElementById(id);
        if (!el[id]) missingIds.push(id);
    });

    if (missingIds.length) {
        // This is the single most useful line for tracking this class of
        // bug down in the future: it tells you exactly which id in the
        // Razor view no longer matches what this script expects.
        console.error('[register-doctor.js] Missing element id(s): ' + missingIds.join(', ') +
            '. Check RegisterDoctor.cshtml for typos or removed elements.');
    }

    const submitBtn = el.submitBtn;
    const submitSpinner = el.submitSpinner;
    const submitBtnLabel = el.submitBtnLabel;
    const alertPlaceholder = el.registerAlertPlaceholder;
    const successState = el.registrationSuccess;

    const uploadInput = el.LicenseDocument;
    const uploadDropzone = el.uploadDropzone;
    const uploadPreview = el.uploadPreview;
    const uploadPrompt = el.uploadPrompt;
    const uploadImagePreview = el.uploadImagePreview;
    const uploadFileIcon = el.uploadFileIcon;
    const uploadFileName = el.uploadFileName;
    const uploadFileSize = el.uploadFileSize;
    const removeFileBtn = el.removeFileBtn;
    const fileError = el.fileError;

    // ============================================================
    // INITIALIZATION
    // ============================================================

    function init() {
        // The submit handler is bound on its own, first, and independently
        // of everything else below — so even if file-upload or realtime
        // validation wiring fails for some reason, Submit still works.
        form.addEventListener('submit', handleFormSubmit);

        try {
            setupFileUpload();
        } catch (err) {
            console.error('[register-doctor.js] File upload wiring failed:', err);
        }

        try {
            setupRealtimeValidation();
        } catch (err) {
            console.error('[register-doctor.js] Realtime validation wiring failed:', err);
        }

        form.setAttribute('novalidate', 'novalidate');
    }

    // ============================================================
    // FILE UPLOAD
    // ============================================================

    function setupFileUpload() {
        if (!uploadDropzone || !uploadInput) return;

        uploadDropzone.addEventListener('click', function (e) {
            if (e.target.closest('#removeFileBtn')) return;
            uploadInput.click();
        });

        uploadDropzone.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                uploadInput.click();
            }
        });

        uploadInput.addEventListener('change', handleFileSelect);

        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(function (eventName) {
            uploadDropzone.addEventListener(eventName, preventDefaults, false);
        });

        uploadDropzone.addEventListener('dragover', function () {
            uploadDropzone.classList.add('dragover');
        });

        uploadDropzone.addEventListener('dragleave', function () {
            uploadDropzone.classList.remove('dragover');
        });

        uploadDropzone.addEventListener('drop', handleFileDrop);

        if (removeFileBtn) {
            removeFileBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                clearFileInput();
            });
        }
    }

    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }

    function handleFileDrop(e) {
        uploadDropzone.classList.remove('dragover');

        const files = e.dataTransfer.files;
        if (!files || files.length === 0) return;

        try {
            const dataTransfer = new DataTransfer();
            dataTransfer.items.add(files[0]);
            uploadInput.files = dataTransfer.files;
        } catch (error) {
            console.error('Unable to set dropped file:', error);
        }

        handleFileSelect({ target: { files: files } });
    }

    function handleFileSelect(e) {
        const file = e.target.files && e.target.files[0];

        clearFilePreview();

        if (!file) return;

        const validation = validateFile(file);

        if (!validation.valid) {
            showFileError(validation.error);
            uploadInput.value = '';
            return;
        }

        clearFileError();
        touchedFields.add('LicenseDocument');
        displayFilePreview(file);
    }

    function validateFile(file) {
        if (file.size > CONFIG.MAX_FILE_SIZE) {
            return {
                valid: false,
                error: 'File is too large. Maximum size is 5 MB. Your file is ' +
                    (file.size / (1024 * 1024)).toFixed(2) + ' MB.'
            };
        }

        const extension = '.' + file.name.split('.').pop().toLowerCase();

        if (!CONFIG.ALLOWED_FILE_TYPES.includes(file.type) &&
            !CONFIG.ALLOWED_FILE_EXTENSIONS.includes(extension)) {
            return { valid: false, error: 'Invalid file type. Please upload JPG, JPEG, PNG, or PDF only.' };
        }

        return { valid: true };
    }

    function displayFilePreview(file) {
        if (!uploadFileName || !uploadFileSize || !uploadPreview || !uploadPrompt) return;

        uploadFileName.textContent = file.name;
        uploadFileSize.textContent = (file.size / (1024 * 1024)).toFixed(2) + ' MB';

        if (file.type === 'application/pdf') {
            if (uploadFileIcon) {
                uploadFileIcon.innerHTML = '<i class="bi bi-file-earmark-pdf-fill"></i>';
                uploadFileIcon.classList.remove('d-none');
            }
            if (uploadImagePreview) uploadImagePreview.classList.add('d-none');
        } else if (file.type.startsWith('image/') && uploadImagePreview) {
            if (uploadFileIcon) uploadFileIcon.classList.add('d-none');

            const reader = new FileReader();
            reader.onload = function (e) {
                uploadImagePreview.src = e.target.result;
                uploadImagePreview.alt = 'Selected license document preview';
                uploadImagePreview.classList.remove('d-none');
            };
            reader.readAsDataURL(file);
        }

        uploadPreview.classList.remove('d-none');
        uploadPrompt.classList.add('d-none');
    }

    function clearFilePreview() {
        if (uploadPreview) uploadPreview.classList.add('d-none');
        if (uploadPrompt) uploadPrompt.classList.remove('d-none');
        if (uploadImagePreview) {
            uploadImagePreview.src = '';
            uploadImagePreview.classList.add('d-none');
        }
        if (uploadFileIcon) uploadFileIcon.classList.add('d-none');
    }

    function clearFileInput() {
        if (uploadInput) uploadInput.value = '';
        clearFilePreview();
        clearFileError();
        touchedFields.delete('LicenseDocument');
    }

    function showFileError(message) {
        if (!fileError) return;
        fileError.textContent = message;
        fileError.removeAttribute('hidden');
        if (uploadDropzone) uploadDropzone.classList.add('error');
    }

    function clearFileError() {
        if (!fileError) return;
        fileError.textContent = '';
        fileError.setAttribute('hidden', '');
        if (uploadDropzone) uploadDropzone.classList.remove('error');
    }

    // ============================================================
    // REALTIME VALIDATION
    // ============================================================

    function setupRealtimeValidation() {
        setupTextField('FirstName');
        setupTextField('LastName');
        setupTextField('PhoneNumber');
        setupTextField('SyndicateId');

        if (el.Email) {
            el.Email.addEventListener('blur', function () {
                touchedFields.add('Email');
                validateEmail();
            });
            el.Email.addEventListener('input', function () {
                if (touchedFields.has('Email') || formSubmitted) validateEmail();
            });
        }

        if (el.Specialty) {
            el.Specialty.addEventListener('change', function () {
                touchedFields.add('Specialty');
                validateField(el.Specialty, 'Specialty');
            });
        }

        if (el.Password) {
            el.Password.addEventListener('blur', function () {
                touchedFields.add('Password');
                validatePassword();
            });
            el.Password.addEventListener('input', function () {
                touchedFields.add('Password');
                validatePassword();
                updatePasswordStrength();
                if (touchedFields.has('ConfirmPassword')) validateConfirmPassword();
            });
        }

        if (el.ConfirmPassword) {
            el.ConfirmPassword.addEventListener('blur', function () {
                touchedFields.add('ConfirmPassword');
                validateConfirmPassword();
            });
            el.ConfirmPassword.addEventListener('input', function () {
                if (touchedFields.has('ConfirmPassword') || touchedFields.has('Password') || formSubmitted) {
                    validateConfirmPassword();
                }
            });
        }

        if (el.ConfirmAccuracy) {
            el.ConfirmAccuracy.addEventListener('change', function () {
                touchedFields.add('ConfirmAccuracy');
                validateTerms();
            });
        }
    }

    function setupTextField(fieldId) {
        const field = el[fieldId];
        if (!field) return;

        field.addEventListener('blur', function () {
            touchedFields.add(fieldId);
            validateField(field, fieldId);
        });

        field.addEventListener('input', function () {
            if (touchedFields.has(fieldId) || formSubmitted) validateField(field, fieldId);
        });
    }

    // ============================================================
    // VALIDATION FUNCTIONS
    // ============================================================

    function validateField(field, fieldName) {
        const errorElement = getErrorElement(fieldName);

        if (!touchedFields.has(fieldName) && !formSubmitted) {
            clearFieldError(field, errorElement);
            return true;
        }

        const error = field.value.trim() ? '' : 'This field is required.';
        updateFieldError(field, errorElement, error);
        return error === '';
    }

    function validateEmail() {
        if (!el.Email) return true;
        const errorElement = getErrorElement('Email');

        if (!touchedFields.has('Email') && !formSubmitted) {
            clearFieldError(el.Email, errorElement);
            return true;
        }

        let error = '';
        const value = el.Email.value.trim();
        if (!value) {
            error = 'Email is required.';
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
            error = 'Please enter a valid email address.';
        }

        updateFieldError(el.Email, errorElement, error);
        return error === '';
    }

    function validatePassword() {
        if (!el.Password) return true;
        const errorElement = getErrorElement('Password');

        if (!touchedFields.has('Password') && !formSubmitted) {
            clearFieldError(el.Password, errorElement);
            return true;
        }

        let error = '';
        if (!el.Password.value) {
            error = 'Password is required.';
        } else if (el.Password.value.length < 8) {
            error = 'Password must be at least 8 characters long.';
        }

        updateFieldError(el.Password, errorElement, error);
        return error === '';
    }

    function validateConfirmPassword() {
        if (!el.ConfirmPassword || !el.Password) return true;
        const errorElement = getErrorElement('ConfirmPassword');

        if (!touchedFields.has('ConfirmPassword') && !formSubmitted) {
            clearFieldError(el.ConfirmPassword, errorElement);
            return true;
        }

        let error = '';
        if (!el.ConfirmPassword.value) {
            error = 'Please confirm your password.';
        } else if (el.Password.value !== el.ConfirmPassword.value) {
            error = 'Passwords do not match.';
        }

        updateFieldError(el.ConfirmPassword, errorElement, error);
        return error === '';
    }

    function validateTerms() {
        if (!el.ConfirmAccuracy) return true;
        const errorElement = getErrorElement('ConfirmAccuracy');

        if (!touchedFields.has('ConfirmAccuracy') && !formSubmitted) {
            clearFieldError(el.ConfirmAccuracy, errorElement);
            return true;
        }

        const error = el.ConfirmAccuracy.checked ? '' : 'You must confirm the information to proceed.';
        updateFieldError(el.ConfirmAccuracy, errorElement, error);
        return error === '';
    }

    function updateFieldError(field, errorElement, error) {
        if (!field || !errorElement) return;

        if (error) {
            field.classList.add('is-invalid');
            field.classList.remove('is-valid');
            errorElement.textContent = error;
            errorElement.removeAttribute('hidden');
        } else {
            field.classList.remove('is-invalid');
            const hasValue = field.type === 'checkbox' ? field.checked : !!field.value;
            field.classList.toggle('is-valid', hasValue);
            errorElement.textContent = '';
            errorElement.setAttribute('hidden', '');
        }
    }

    function clearFieldError(field, errorElement) {
        if (field) field.classList.remove('is-invalid', 'is-valid');
        if (errorElement) errorElement.setAttribute('hidden', '');
    }

    function getErrorElement(fieldName) {
        const mapping = {
            FirstName: el.firstNameError,
            LastName: el.lastNameError,
            Email: el.emailError,
            PhoneNumber: el.phoneError,
            Specialty: el.specialtyError,
            SyndicateId: el.syndicateIdError,
            Password: el.passwordError,
            ConfirmPassword: el.confirmPasswordError,
            ConfirmAccuracy: el.termsError
        };
        return mapping[fieldName];
    }

    // ============================================================
    // PASSWORD STRENGTH
    // ------------------------------------------------------------
    // Sets a CSS custom property instead of backgroundColor directly.
    // See doctor-register.css: `.password-strength-fill` reads its
    // color from `var(--strength-color, var(--cl-border))`. This way
    // the color is owned by ONE rule with a single source of truth,
    // instead of an inline style silently fighting a stylesheet
    // default — if this JS never ran for any reason, the bar would
    // simply stay neutral grey instead of stuck on red.
    // ============================================================

    function updatePasswordStrength() {
        if (!el.Password || !el.passwordStrengthFill || !el.passwordStrengthLabel) {
            return;
        }

        const password = el.Password.value;
        const fill = el.passwordStrengthFill;
        const label = el.passwordStrengthLabel;

        // Password requirements
        const requirements = document.querySelectorAll('#passwordRequirements li');

        const rules = {
            length: password.length >= 8,
            upperLower: /[a-z]/.test(password) && /[A-Z]/.test(password),
            number: /\d/.test(password),
            special: /[^A-Za-z0-9]/.test(password)
        };

        let passedCount = 0;

        requirements.forEach(function (item) {
            const ruleName = item.getAttribute('data-rule');
            const passed = rules[ruleName] === true;

            item.classList.toggle('met', passed);

            if (passed) {
                passedCount++;
            }
        });

        // Empty password
        if (!password) {
            fill.style.width = '0%';
            fill.style.backgroundColor = 'var(--cl-border)';
            label.textContent = 'Password strength';
            return;
        }

        // Strength levels
        const levels = [
            {
                width: '25%',
                color: 'var(--cl-danger)',
                text: 'Weak'
            },
            {
                width: '50%',
                color: 'var(--cl-warning)',
                text: 'Fair'
            },
            {
                width: '75%',
                color: 'var(--cl-warning)',
                text: 'Good'
            },
            {
                width: '100%',
                color: 'var(--cl-success)',
                text: 'Strong'
            }
        ];

        const level = levels[passedCount - 1];

        fill.style.width = level.width;
        fill.style.backgroundColor = level.color;
        label.textContent = level.text;
    }

    // ============================================================
    // FORM VALIDATION
    // ============================================================

    function validateForm() {
        formSubmitted = true;
        let isValid = true;

        if (el.FirstName && !validateField(el.FirstName, 'FirstName')) isValid = false;
        if (el.LastName && !validateField(el.LastName, 'LastName')) isValid = false;
        if (!validateEmail()) isValid = false;
        if (el.PhoneNumber && !validateField(el.PhoneNumber, 'PhoneNumber')) isValid = false;
        if (el.Specialty && !validateField(el.Specialty, 'Specialty')) isValid = false;
        if (el.SyndicateId && !validateField(el.SyndicateId, 'SyndicateId')) isValid = false;
        if (!validatePassword()) isValid = false;
        if (!validateConfirmPassword()) isValid = false;

        if (uploadInput) {
            if (!uploadInput.files || uploadInput.files.length === 0) {
                showFileError('Please upload your medical license or syndicate credential.');
                isValid = false;
            } else {
                const validation = validateFile(uploadInput.files[0]);
                if (!validation.valid) {
                    showFileError(validation.error);
                    isValid = false;
                } else {
                    clearFileError();
                }
            }
        }

        touchedFields.add('ConfirmAccuracy');
        if (!validateTerms()) isValid = false;

        return isValid;
    }

    // ============================================================
    // FORM SUBMISSION
    // ============================================================

    async function handleFormSubmit(e) {
        e.preventDefault();

        let isValid;
        try {
            clearAlert();
            isValid = validateForm();
        } catch (err) {
            // This is the fix for "button does nothing": a validation-step
            // error used to throw here silently, before ever reaching the
            // fetch call. Now it's caught and surfaced instead of swallowed.
            console.error('[register-doctor.js] validateForm() threw:', err);
            showAlert('Something went wrong while checking the form. Please refresh the page and try again.', 'danger');
            return;
        }

        if (!isValid) {
            const firstInvalid = form.querySelector('.is-invalid');
            if (firstInvalid) firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
            return;
        }

        setSubmitLoading(true);

        try {
            const formData = new FormData(form);
            formData.delete('ConfirmPassword');

            const response = await fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'Accept': 'application/json'
                }
            });

            if (response.status === 200 || response.status === 201 || response.status === 202) {
                showSuccess();
                return;
            }

            let message = response.status === 409
                ? 'An account with this email already exists.'
                : 'Registration failed. Please check your information and try again.';

            try {
                const data = await response.json();
                if (data.message) {
                    message = data.message;
                } else if (data.errors) {
                    message = Object.values(data.errors).flat().join(' ');
                }
            } catch {
                // Response wasn't JSON — keep the default message.
            }

            showAlert(message, 'danger');
            setSubmitLoading(false);
        } catch (error) {
            console.error('Doctor registration error:', error);
            showAlert('Network error. Please check your connection and try again.', 'danger');
            setSubmitLoading(false);
        }
    }

    // ============================================================
    // SUBMIT BUTTON
    // ============================================================

    function setSubmitLoading(isLoading) {
        if (!submitBtn) return;
        submitBtn.disabled = isLoading;
        if (submitSpinner) submitSpinner.classList.toggle('d-none', !isLoading);
        if (submitBtnLabel) submitBtnLabel.textContent = isLoading ? 'Creating account...' : 'Submit Registration';
    }

    // ============================================================
    // ALERT
    // ============================================================

    function showAlert(message, type) {
        if (!alertPlaceholder) return;
        alertPlaceholder.innerHTML =
            '<div class="alert alert-' + type + ' alert-dismissible fade show" role="alert">' +
            '<i class="bi bi-exclamation-circle me-2"></i>' +
            escapeHtml(message) +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
            '</div>';
    }

    function clearAlert() {
        if (alertPlaceholder) alertPlaceholder.innerHTML = '';
    }

    // ============================================================
    // SUCCESS
    // ============================================================

    function showSuccess() {
        form.classList.add('d-none');
        const switchLink = document.getElementById('loginSwitchLink');
        if (switchLink) switchLink.classList.add('d-none');
        if (successState) {
            successState.classList.remove('d-none');
            successState.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }

    // ============================================================
    // SECURITY HELPER
    // ============================================================

    function escapeHtml(value) {
        const div = document.createElement('div');
        div.textContent = value;
        return div.innerHTML;
    }

    // ============================================================
    // START
    // ============================================================

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();

/**
 * CuraLink — Patient: Upload Medical Documents
 * Scope: Views/Patient/Upload.cshtml only.
 *
 * Client-side validation mirrors PatientController.Upload()'s server-side
 * checks (10 MB max; PDF/JPG/JPEG/PNG only) so the user gets instant
 * feedback, but the server re-validates independently — this script is
 * a UX layer, not the source of truth.
 */

(function () {
    'use strict';

    const CONFIG = {
        MAX_FILE_SIZE: 10 * 1024 * 1024, // 10 MB
        ALLOWED_CONTENT_TYPES: ['application/pdf', 'image/jpeg', 'image/jpg', 'image/png'],
        ALLOWED_EXTENSIONS: ['.pdf', '.jpg', '.jpeg', '.png']
    };

    const form = document.getElementById('uploadDocumentForm');
    if (!form) {
        console.error('[patient-upload.js] #uploadDocumentForm was not found — aborting setup.');
        return;
    }

    const ids = [
        'fileInput', 'uploadDropzone', 'uploadPrompt', 'uploadPreview',
        'previewFileIcon', 'previewFileName', 'previewFileSize', 'removeFileBtn',
        'fileError', 'submitBtn', 'submitBtnLabel', 'submitSpinner',
        'uploadProgressWrap', 'uploadProgressBar', 'uploadAlertPlaceholder'
    ];

    const el = {};
    const missingIds = [];
    ids.forEach(function (id) {
        el[id] = document.getElementById(id);
        if (!el[id]) missingIds.push(id);
    });

    if (missingIds.length) {
        console.error('[patient-upload.js] Missing element id(s): ' + missingIds.join(', ') +
            '. Check Upload.cshtml for typos or removed elements.');
    }

    function init() {
        form.addEventListener('submit', handleSubmit);
        form.setAttribute('novalidate', 'novalidate');

        if (!el.uploadDropzone || !el.fileInput) return;

        el.uploadDropzone.addEventListener('click', function (e) {
            if (e.target.closest('#removeFileBtn')) return;
            el.fileInput.click();
        });

        el.uploadDropzone.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                el.fileInput.click();
            }
        });

        el.fileInput.addEventListener('change', function (e) {
            handleFileSelect(e.target.files && e.target.files[0]);
        });

        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(function (eventName) {
            el.uploadDropzone.addEventListener(eventName, function (e) {
                e.preventDefault();
                e.stopPropagation();
            });
        });

        el.uploadDropzone.addEventListener('dragover', function () {
            el.uploadDropzone.classList.add('dragover');
        });

        el.uploadDropzone.addEventListener('dragleave', function () {
            el.uploadDropzone.classList.remove('dragover');
        });

        el.uploadDropzone.addEventListener('drop', function (e) {
            el.uploadDropzone.classList.remove('dragover');
            const files = e.dataTransfer.files;
            if (!files || files.length === 0) return;

            try {
                const dataTransfer = new DataTransfer();
                dataTransfer.items.add(files[0]);
                el.fileInput.files = dataTransfer.files;
            } catch (error) {
                console.error('[patient-upload.js] Unable to set dropped file:', error);
            }

            handleFileSelect(files[0]);
        });

        if (el.removeFileBtn) {
            el.removeFileBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                clearSelection();
            });
        }
    }

    function handleFileSelect(file) {
        clearError();

        if (!file) {
            clearPreview();
            return;
        }

        const validation = validateFile(file);
        if (!validation.valid) {
            showError(validation.error);
            clearSelection();
            return;
        }

        showPreview(file);
    }

    function validateFile(file) {
        if (file.size > CONFIG.MAX_FILE_SIZE) {
            return { valid: false, error: 'File size must not exceed 10 MB.' };
        }

        const extension = '.' + file.name.split('.').pop().toLowerCase();
        const typeOk = CONFIG.ALLOWED_CONTENT_TYPES.includes(file.type);
        const extOk = CONFIG.ALLOWED_EXTENSIONS.includes(extension);

        if (!typeOk && !extOk) {
            return { valid: false, error: 'Only PDF, JPG, JPEG, and PNG files are allowed.' };
        }

        return { valid: true };
    }

    function showPreview(file) {
        if (!el.uploadPreview || !el.uploadPrompt) return;

        el.previewFileName.textContent = file.name;
        el.previewFileSize.textContent = formatSize(file.size);

        el.previewFileIcon.className = 'bi ' + iconForType(file.type);

        el.uploadPreview.classList.remove('d-none');
        el.uploadPrompt.classList.add('d-none');
    }

    function clearPreview() {
        if (el.uploadPreview) el.uploadPreview.classList.add('d-none');
        if (el.uploadPrompt) el.uploadPrompt.classList.remove('d-none');
    }

    function clearSelection() {
        if (el.fileInput) el.fileInput.value = '';
        clearPreview();
    }

    function iconForType(type) {
        if (type === 'application/pdf') return 'bi-file-earmark-pdf-fill';
        if (type.startsWith('image/')) return 'bi-file-earmark-image-fill';
        return 'bi-file-earmark-fill';
    }

    function formatSize(bytes) {
        return bytes >= 1024 * 1024
            ? (bytes / (1024 * 1024)).toFixed(2) + ' MB'
            : Math.max(bytes / 1024, 0.1).toFixed(1) + ' KB';
    }

    function showError(message) {
        if (!el.fileError) return;
        el.fileError.textContent = message;
        el.fileError.removeAttribute('hidden');
        if (el.uploadDropzone) el.uploadDropzone.classList.add('error');
    }

    function clearError() {
        if (!el.fileError) return;
        el.fileError.textContent = '';
        el.fileError.setAttribute('hidden', '');
        if (el.uploadDropzone) el.uploadDropzone.classList.remove('error');
    }

    function showAlert(message, type) {
        if (!el.uploadAlertPlaceholder) return;
        el.uploadAlertPlaceholder.innerHTML =
            '<div class="alert alert-' + type + ' alert-dismissible fade show" role="alert">' +
            '<i class="bi bi-exclamation-circle me-2"></i>' + escapeHtml(message) +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
            '</div>';
    }

    function clearAlert() {
        if (el.uploadAlertPlaceholder) el.uploadAlertPlaceholder.innerHTML = '';
    }

    function escapeHtml(value) {
        const div = document.createElement('div');
        div.textContent = value;
        return div.innerHTML;
    }

    // ============================================================
    // SUBMIT — XHR (not fetch) so we get real upload progress events.
    // On success, the server redirects to MedicalRecords; we just
    // navigate the browser there directly rather than swapping DOM.
    // ============================================================

    function handleSubmit(e) {
        e.preventDefault();
        clearAlert();

        const file = el.fileInput && el.fileInput.files && el.fileInput.files[0];
        if (!file) {
            showError('Please choose a file to upload.');
            return;
        }

        const validation = validateFile(file);
        if (!validation.valid) {
            showError(validation.error);
            return;
        }

        setLoading(true);

        const formData = new FormData(form);
        const xhr = new XMLHttpRequest();

        xhr.open('POST', form.action, true);
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');

        xhr.upload.addEventListener('progress', function (evt) {
            if (!evt.lengthComputable || !el.uploadProgressWrap || !el.uploadProgressBar) return;
            el.uploadProgressWrap.classList.remove('d-none');
            const pct = Math.round((evt.loaded / evt.total) * 100);
            el.uploadProgressBar.style.width = pct + '%';
            el.uploadProgressBar.setAttribute('aria-valuenow', pct);
        });

        xhr.addEventListener('load', function () {
            setLoading(false);

            if (xhr.status >= 200 && xhr.status < 400) {
                window.location.href = '/Patient/MedicalRecords';
                return;
            }

            showAlert('Upload failed. Please check your file and try again.', 'danger');
        });

        xhr.addEventListener('error', function () {
            setLoading(false);
            showAlert('Network error. Please check your connection and try again.', 'danger');
        });

        xhr.send(formData);
    }

    function setLoading(isLoading) {
        if (!el.submitBtn) return;
        el.submitBtn.disabled = isLoading;
        if (el.submitSpinner) el.submitSpinner.classList.toggle('d-none', !isLoading);
        if (el.submitBtnLabel) el.submitBtnLabel.textContent = isLoading ? 'Uploading...' : 'Upload';
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
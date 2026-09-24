(function () {
    'use strict';

    var input = document.getElementById('profilePhotoInput');
    var preview = document.getElementById('profilePhotoPreview');
    var initials = document.getElementById('profilePhotoInitials');
    var errorBox = document.getElementById('profilePhotoClientError');

    if (!input) {
        return;
    }

    var allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    var maxSizeBytes = 5 * 1024 * 1024;

    input.addEventListener('change', function () {
        errorBox.classList.add('d-none');
        errorBox.textContent = '';

        var file = input.files && input.files[0];
        if (!file) {
            return;
        }

        if (allowedTypes.indexOf(file.type) === -1) {
            errorBox.textContent = 'Only JPG, PNG, and WebP images are allowed.';
            errorBox.classList.remove('d-none');
            input.value = '';
            return;
        }

        if (file.size > maxSizeBytes) {
            errorBox.textContent = 'Profile photo must not exceed 5 MB.';
            errorBox.classList.remove('d-none');
            input.value = '';
            return;
        }

        var reader = new FileReader();
        reader.onload = function (e) {
            preview.src = e.target.result;
            preview.classList.remove('d-none');
            if (initials) {
                initials.classList.add('d-none');
            }
        };
        reader.readAsDataURL(file);
    });
})();
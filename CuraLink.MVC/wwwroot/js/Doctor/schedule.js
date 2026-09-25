(function () {
    'use strict';

    function toggleRow(checkbox) {
        var index = checkbox.getAttribute('data-day-index');
        var fields = document.getElementById('scheduleFields_' + index);
        if (!fields) {
            return;
        }
        fields.classList.toggle('enabled', checkbox.checked);
    }

    document.querySelectorAll('.schedule-day-checkbox').forEach(function (checkbox) {
        toggleRow(checkbox);
        checkbox.addEventListener('change', function () {
            toggleRow(checkbox);
        });
    });

    var form = document.getElementById('scheduleForm');
    var saveBtn = document.getElementById('saveScheduleBtn');
    var saveBtnText = document.getElementById('saveScheduleBtnText');
    var saveSpinner = document.getElementById('saveScheduleSpinner');

    if (form && saveBtn) {
        form.addEventListener('submit', function () {
            saveBtn.disabled = true;
            saveBtnText.classList.add('d-none');
            saveSpinner.classList.remove('d-none');
            // Full page postback (matches the rest of the app's PRG pattern),
            // so no need to re-enable the button here — the redirect/reload
            // after save (or the redisplayed view on validation error) resets it.
        });
    }
})();
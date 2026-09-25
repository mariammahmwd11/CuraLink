(function () {
    'use strict';

    var infoCard = document.getElementById('doctorInfoCard');
    if (!infoCard) {
        return;
    }

    var doctorId = infoCard.getAttribute('data-doctor-id');
    var doctorName = infoCard.getAttribute('data-doctor-name');

    var dateInput = document.getElementById('appointmentDate');
    var slotsContainer = document.getElementById('slotsContainer');
    var emptyState = document.getElementById('slotsEmptyState');
    var slotsErrorState = document.getElementById('slotsErrorState');
    var confirmSection = document.getElementById('confirmSection');
    var confirmSlotLabel = document.getElementById('confirmSlotLabel');
    var confirmDateLabel = document.getElementById('confirmDateLabel');
    var confirmBtn = document.getElementById('confirmBookingBtn');
    var confirmBtnText = document.getElementById('confirmBtnText');
    var confirmBtnSpinner = document.getElementById('confirmBtnSpinner');
    var cancelBtn = document.getElementById('cancelSelectionBtn');
    var bookingErrorState = document.getElementById('bookingErrorState');
    var confirmationCard = document.getElementById('confirmationCard');

    var selectedSlot = null; // { startTime, endTime }
    var isBooking = false;

    // Default to today, don't allow past dates.
    var today = new Date().toISOString().split('T')[0];
    dateInput.value = today;
    dateInput.min = today;

    dateInput.addEventListener('change', function () {
        clearSelection();
        loadSlots();
    });

    cancelBtn.addEventListener('click', clearSelection);
    confirmBtn.addEventListener('click', confirmBooking);

    loadSlots();

    function clearSelection() {
        selectedSlot = null;
        confirmSection.classList.add('d-none');
        bookingErrorState.classList.add('d-none');
        document.querySelectorAll('.slot-btn.selected').forEach(function (btn) {
            btn.classList.remove('selected');
        });
    }

    function loadSlots() {
        slotsContainer.innerHTML = '';
        emptyState.classList.add('d-none');
        slotsErrorState.classList.add('d-none');
        clearSelection();

        var date = dateInput.value;
        if (!date) {
            return;
        }

        fetch('/Patient/AvailableSlots?doctorId=' + encodeURIComponent(doctorId) + '&date=' + encodeURIComponent(date))
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (!data.success) {
                    slotsErrorState.textContent = data.error || 'We couldn\'t load available slots right now.';
                    slotsErrorState.classList.remove('d-none');
                    return;
                }

                if (!data.slots || data.slots.length === 0) {
                    emptyState.classList.remove('d-none');
                    return;
                }

                data.slots.forEach(function (slot) {
                    var btn = document.createElement('button');
                    btn.type = 'button';
                    btn.className = 'slot-btn';
                    btn.textContent = slot.startTime + ' - ' + slot.endTime;
                    btn.addEventListener('click', function () {
                        selectSlot(slot, btn);
                    });
                    slotsContainer.appendChild(btn);
                });
            })
            .catch(function () {
                slotsErrorState.textContent = 'We couldn\'t load available slots right now.';
                slotsErrorState.classList.remove('d-none');
            });
    }

    function selectSlot(slot, btnEl) {
        selectedSlot = slot;

        document.querySelectorAll('.slot-btn.selected').forEach(function (btn) {
            btn.classList.remove('selected');
        });
        btnEl.classList.add('selected');

        confirmSlotLabel.textContent = slot.startTime + ' - ' + slot.endTime;
        confirmDateLabel.textContent = dateInput.value;
        bookingErrorState.classList.add('d-none');
        confirmSection.classList.remove('d-none');
    }

    function confirmBooking() {
        if (isBooking || !selectedSlot) {
            return;
        }

        isBooking = true;
        confirmBtn.disabled = true;
        confirmBtnText.classList.add('d-none');
        confirmBtnSpinner.classList.remove('d-none');
        bookingErrorState.classList.add('d-none');

        fetch('/Patient/BookAppointment', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                doctorId: doctorId,
                date: dateInput.value,
                startTime: selectedSlot.startTime + ':00',
                endTime: selectedSlot.endTime + ':00'
            })
        })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (!data.success) {
                    bookingErrorState.textContent = data.error ||
                        'The selected slot is no longer available. Please choose another slot.';
                    bookingErrorState.classList.remove('d-none');

                    // Slot may have just been taken by someone else — refresh the list.
                    loadSlots();
                    return;
                }

                showConfirmation(data.appointmentId);
                confirmSection.classList.add('d-none');
                loadSlots(); // booked slot disappears from the list
            })
            .catch(function () {
                bookingErrorState.textContent = 'Something went wrong while booking your appointment.';
                bookingErrorState.classList.remove('d-none');
            })
            .finally(function () {
                isBooking = false;
                confirmBtn.disabled = false;
                confirmBtnText.classList.remove('d-none');
                confirmBtnSpinner.classList.add('d-none');
            });
    }

    function showConfirmation(appointmentId) {
        document.getElementById('confirmationDate').textContent = dateInput.value;
        document.getElementById('confirmationTime').textContent =
            selectedSlot.startTime + ' - ' + selectedSlot.endTime;
        document.getElementById('confirmationId').textContent = '#' + appointmentId;
        confirmationCard.classList.remove('d-none');
        confirmationCard.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
})();
(function () {
    'use strict';
    const UI = window.AssistantUI;
    const $ = (id) => document.getElementById(id);
    const root = $('baRoot');

    let patient = null;        // { id, name }
    let selectedSlot = null;   // { startTime, endTime }  (both "HH:mm")
    let timer = null;
    let seq = 0;               // patient search race guard
    let slotsSeq = 0;          // slots request race guard

    const today = new Date();
    const todayIso = new Date(today.getTime() - today.getTimezoneOffset() * 60000)
        .toISOString().slice(0, 10);
    $('baDate').min = todayIso;
    $('baDate').value = todayIso;

    const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    function prettyDate(iso) {
        if (!iso) return '\u2014';
        const p = iso.split('-');
        return p[2] + ' ' + MONTHS[parseInt(p[1], 10) - 1] + ' ' + p[0];
    }

    /* ---------------- patient ---------------- */
    function setPatient(p) {
        patient = p;
        $('baPatientChosen').hidden = !p;
        $('baPatientPicker').hidden = !!p;
        if (p) $('baPatientName').textContent = p.name;
        refresh();
    }

    $('baChange').addEventListener('click', () => { setPatient(null); $('baSearch').focus(); });

    async function search() {
        const term = $('baSearch').value.trim();
        const list = $('baResults');
        const state = $('baSearchState');
        list.innerHTML = '';
        if (!term) { seq++; state.textContent = ''; return; }

        const mine = ++seq;
        state.textContent = 'Searching...';
        const r = await UI.get(UI.urls.searchPatients + '?search=' + encodeURIComponent(term));
        if (mine !== seq) return;

        if (!r.ok) { state.textContent = r.message; return; }
        const rows = Array.isArray(r.data) ? r.data : [];
        state.textContent = rows.length ? '' : 'No patients found.';

        list.innerHTML = rows.map((p) => {
            const name = ((p.firstName || '') + ' ' + (p.lastName || '')).trim();
            return '<button type="button" class="list-group-item list-group-item-action" data-id="' + UI.esc(p.patientId) +
                '" data-name="' + UI.esc(name) + '"><div class="fw-semibold">' + UI.esc(name) + '</div>' +
                '<div class="small text-muted">' + UI.esc(p.phoneNumber) + ' &middot; ' + UI.esc(p.email) + '</div></button>';
        }).join('');
    }

    $('baSearch').addEventListener('input', () => { clearTimeout(timer); timer = setTimeout(search, 400); });
    $('baResults').addEventListener('click', (e) => {
        const b = e.target.closest('[data-id]');
        if (b) setPatient({ id: b.dataset.id, name: b.dataset.name });
    });

    /* ---------------- slots ---------------- */
    function showSlotsMessage(text) {
        $('baSlots').innerHTML = '<div class="text-muted small">' + UI.esc(text) + '</div>';
    }

    function clearSelection() {
        selectedSlot = null;
        refresh();
    }

    async function loadSlots() {
        const date = $('baDate').value;
        const grid = $('baSlots');

        $('baSlotsEmpty').hidden = true;
        $('baSlotsError').hidden = true;
        selectedSlot = null;
        refresh();

        if (!date) { showSlotsMessage('Select a date to see available times.'); return; }
        if (date < todayIso) {
            grid.innerHTML = '';
            $('baSlotsError').textContent = 'Past dates cannot be booked.';
            $('baSlotsError').hidden = false;
            return;
        }

        const mine = ++slotsSeq;
        showSlotsMessage('Loading available times...');

        const r = await UI.get(root.dataset.slotsUrl + '?date=' + encodeURIComponent(date));
        if (mine !== slotsSeq) return;           // a newer date was picked meanwhile

        grid.innerHTML = '';

        if (!r.ok) {
            $('baSlotsError').textContent = r.message || 'We could not load available times.';
            $('baSlotsError').hidden = false;
            UI.loginRedirectIfExpired(r);
            return;
        }


        const slotsData = r.data?.slots;
        const slots = Array.isArray(slotsData)
            ? slotsData
            : Array.isArray(slotsData?.slots)
                ? slotsData.slots
                : [];

        if (slots.length === 0) {
            $('baSlotsEmpty').hidden = false;
            return;
        }

        slots.forEach((slot) => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'slot-btn';
            btn.textContent = slot.startTime + ' - ' + slot.endTime;
            btn.addEventListener('click', () => selectSlot(slot, btn));
            grid.appendChild(btn);
        });
    }

    function selectSlot(slot, btn) {
        selectedSlot = { startTime: slot.startTime, endTime: slot.endTime };
        document.querySelectorAll('#baSlots .slot-btn.selected')
            .forEach((b) => b.classList.remove('selected'));
        btn.classList.add('selected');
        refresh();
    }

    $('baDate').addEventListener('change', loadSlots);

    /* ---------------- summary ---------------- */
    function refresh() {
        const date = $('baDate').value;
        $('sumPatient').textContent = patient ? patient.name : '\u2014';
        $('sumDate').textContent = prettyDate(date);
        $('sumTime').textContent = selectedSlot
            ? selectedSlot.startTime + ' \u2013 ' + selectedSlot.endTime
            : '\u2014';
        $('baConfirm').disabled = !(patient && date && date >= todayIso && selectedSlot);
    }

    /* ---------------- confirm ---------------- */
    function setBusy(busy) {
        $('baConfirm').disabled = busy || $('baConfirm').disabled;
        $('baSpinner').hidden = !busy;
        $('baLabel').textContent = busy ? 'Booking...' : 'Confirm Booking';
    }

    $('baConfirm').addEventListener('click', async function () {
        $('baError').hidden = true;
        $('baSuccess').hidden = true;
        if (!patient || !selectedSlot) return;

        setBusy(true);
        const r = await UI.post(UI.urls.bookApi, {
            patientId: patient.id,
            date: $('baDate').value,
            startTime: selectedSlot.startTime ,
            endTime: selectedSlot.endTime 
        });
        setBusy(false);

        if (!r.ok) {
            $('baError').textContent = r.message;
            $('baError').hidden = false;
            UI.loginRedirectIfExpired(r);
            // The slot may have just been taken by someone else: refresh the list.
            loadSlots();
            return;
        }

        UI.toast('Appointment booked successfully.', 'success');
        $('baSuccess').innerHTML = 'Appointment booked successfully. ' +
            '<a href="' + UI.esc(UI.urls.todayPage) + '" class="alert-link">View today\'s appointments</a>';
        $('baSuccess').hidden = false;

        loadSlots();   // the booked slot disappears, selection is cleared
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });

    /* ---------------- init ---------------- */
    const pid = root.dataset.patientId;
    if (pid) setPatient({ id: pid, name: root.dataset.patientName || 'Selected patient' });
    else setPatient(null);

    loadSlots();
})();
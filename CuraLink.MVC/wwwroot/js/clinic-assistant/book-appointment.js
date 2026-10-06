(function () {
    'use strict';
    const UI = window.AssistantUI;
    const $ = (id) => document.getElementById(id);
    const root = $('baRoot');

    let patient = null;   // { id, name }
    let timer = null;
    let seq = 0;

    const today = new Date();
    const todayIso = new Date(today.getTime() - today.getTimezoneOffset() * 60000).toISOString().slice(0, 10);
    $('baDate').min = todayIso;
    $('baDate').value = todayIso;

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

    /* ---------------- time ---------------- */
    function addMinutes(hhmm, mins) {
        const [h, m] = hhmm.split(':').map(Number);
        const total = h * 60 + m + mins;
        if (total >= 24 * 60) return '';
        return String(Math.floor(total / 60)).padStart(2, '0') + ':' + String(total % 60).padStart(2, '0');
    }

    $('baStart').addEventListener('change', () => {
        const s = $('baStart').value;
        if (s && (!$('baEnd').value || $('baEnd').value <= s)) $('baEnd').value = addMinutes(s, 30);
        refresh();
    });
    $('baEnd').addEventListener('change', refresh);
    $('baDate').addEventListener('change', refresh);

    function timeValid() {
        const s = $('baStart').value, e = $('baEnd').value;
        return !!s && !!e && e > s;
    }

    function refresh() {
        const date = $('baDate').value;
        $('sumPatient').textContent = patient ? patient.name : '\u2014';
        $('sumDate').textContent = date || '\u2014';
        $('sumTime').textContent = timeValid() ? $('baStart').value + ' \u2013 ' + $('baEnd').value : '\u2014';
        $('baConfirm').disabled = !(patient && date && date >= todayIso && timeValid());
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
        if (!patient || !timeValid()) return;

        setBusy(true);
        const r = await UI.post(UI.urls.bookApi, {
            patientId: patient.id,
            date: $('baDate').value,
            startTime: $('baStart').value + ':00',
            endTime: $('baEnd').value + ':00'
        });
        setBusy(false);

        if (!r.ok) {
            $('baError').textContent = r.message;
            $('baError').hidden = false;
            UI.loginRedirectIfExpired(r);
            refresh();
            return;
        }

        UI.toast('Appointment booked successfully.', 'success');
        $('baSuccess').innerHTML = 'Appointment booked successfully. ' +
            '<a href="' + UI.esc(UI.urls.todayPage) + '" class="alert-link">View today\'s appointments</a>';
        $('baSuccess').hidden = false;

        $('baStart').value = '';
        $('baEnd').value = '';
        refresh();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });

    /* ---------------- init ---------------- */
    const pid = root.dataset.patientId;
    if (pid) setPatient({ id: pid, name: root.dataset.patientName || 'Selected patient' });
    else setPatient(null);
})();
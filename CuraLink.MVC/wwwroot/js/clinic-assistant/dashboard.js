(function () {
    'use strict';
    const UI = window.AssistantUI;
    const $ = (id) => document.getElementById(id);

    function show(id, on) { $(id).hidden = !on; }

    function render(d) {
        $('dashSubtitle').innerHTML = d.clinicName
            ? 'Receptionist at <strong>' + UI.esc(d.clinicName) + '</strong>. Here is today\'s clinic activity.'
            : 'Here is today\'s clinic activity.';

        $('statTotal').textContent = d.totalToday ?? 0;
        $('statWaiting').textContent = d.waiting ?? 0;
        $('statCheckedIn').textContent = d.checkedIn ?? 0;
        $('statCompleted').textContent = d.completed ?? 0;

        const list = Array.isArray(d.appointmentDtos) ? d.appointmentDtos : [];
        show('dashEmpty', list.length === 0);
        show('dashTableWrap', list.length > 0);

        $('dashRows').innerHTML = list.map((a) => {
            const action = UI.isConfirmed(a.status)
                ? '<button type="button" class="btn btn-sm btn-primary" data-checkin="' + UI.esc(a.id) + '">Check in</button>'
                : '<span class="text-muted small">&mdash;</span>';
            return '<tr>' +
                '<td class="fw-semibold">' + UI.esc(UI.fmtTime(a.startTime)) + '</td>' +
                '<td>' + UI.esc(a.patientName) + '</td>' +
                '<td>' + UI.esc(a.doctorName) + '</td>' +
                '<td>' + UI.statusBadge(a.status) + '</td>' +
                '<td class="text-end">' + action + '</td></tr>';
        }).join('');
    }

    async function load(silent) {
        if (!silent) { show('dashLoading', true); show('dashContent', false); }
        show('dashError', false);

        const r = await UI.get(UI.urls.dashboardData);

        show('dashLoading', false);
        if (!r.ok || !r.data) {
            $('dashErrorText').textContent = r.message || 'Unable to load the dashboard.';
            show('dashError', true);
            UI.loginRedirectIfExpired(r);
            return;
        }
        render(r.data);
        show('dashContent', true);
    }

    $('dashRows').addEventListener('click', async function (e) {
        const btn = e.target.closest('[data-checkin]');
        if (!btn) return;
        const r = await UI.checkIn(btn.dataset.checkin, btn);
        if (r.ok) load(true); // refresh rows + statistics from the API
    });

    $('dashRetry').addEventListener('click', () => load(false));
    load(false);
})();
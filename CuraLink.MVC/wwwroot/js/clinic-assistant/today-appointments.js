(function () {
    'use strict';
    const UI = window.AssistantUI;
    const $ = (id) => document.getElementById(id);

    let items = [];
    let filter = 'all';

    function only(id) { ['taLoading', 'taError', 'taEmpty', 'taTableWrap'].forEach((p) => { $(p).hidden = p !== id; }); }

    function updateCounts() {
        const counts = { all: items.length, confirmed: 0, checkedin: 0, completed: 0, cancelled: 0 };
        items.forEach((a) => {
            const k = UI.normStatus(a.status) === 'canceled' ? 'cancelled' : UI.normStatus(a.status);
            if (k in counts) counts[k]++;
        });
        document.querySelectorAll('#taFilters [data-filter]').forEach((b) => {
            const base = b.dataset.base || (b.dataset.base = b.textContent.trim());
            b.textContent = base + ' (' + (counts[b.dataset.filter] ?? 0) + ')';
        });
    }

    function render() {
        updateCounts();
        const visible = items.filter((a) => {
            if (filter === 'all') return true;
            const k = UI.normStatus(a.status);
            return (filter === 'cancelled') ? (k === 'cancelled' || k === 'canceled') : k === filter;
        });

        if (visible.length === 0) {
            $('taEmptyTitle').textContent = items.length === 0 ? 'No appointments today' : 'No appointments in this filter';
            only('taEmpty');
            return;
        }

        $('taRows').innerHTML = visible.map((a) => {
            const time = UI.fmtTime(a.startTime) + (a.endTime ? ' \u2013 ' + UI.fmtTime(a.endTime) : '');
            let action = '<span class="text-muted small">&mdash;</span>';
            if (UI.isConfirmed(a.status)) {
                action = '<button type="button" class="btn btn-sm btn-primary" data-checkin="' + UI.esc(a.id) + '">Check in</button>';
            } else if (UI.normStatus(a.status) === 'checkedin') {
                action = '<button type="button" class="btn btn-sm btn-outline-secondary" disabled>Checked in</button>';
            }
            return '<tr><td class="fw-semibold">' + UI.esc(time) + '</td>' +
                '<td>' + UI.esc(a.patientName) + '</td><td>' + UI.esc(a.doctorName) + '</td>' +
                '<td>' + UI.statusBadge(a.status) + '</td><td class="text-end">' + action + '</td></tr>';
        }).join('');
        only('taTableWrap');
    }

    async function load() {
        only('taLoading');
        const r = await UI.get(UI.urls.todayData);
        if (!r.ok) {
            $('taError').textContent = r.message;
            only('taError');
            UI.loginRedirectIfExpired(r);
            return;
        }
        items = Array.isArray(r.data) ? r.data : [];
        render();
    }

    $('taRows').addEventListener('click', async function (e) {
        const btn = e.target.closest('[data-checkin]');
        if (!btn) return;
        const r = await UI.checkIn(btn.dataset.checkin, btn);
        if (r.ok) {
            const row = items.find((a) => String(a.id) === btn.dataset.checkin);
            if (row) row.status = 'CheckedIn'; // update in place, no reload needed
            render();
        }
    });

    $('taFilters').addEventListener('click', function (e) {
        const b = e.target.closest('[data-filter]');
        if (!b) return;
        filter = b.dataset.filter;
        document.querySelectorAll('#taFilters [data-filter]').forEach((x) => x.classList.toggle('active', x === b));
        if ($('taLoading').hidden && $('taError').hidden) render(); // only after data has loaded
    });

    $('taRefresh').addEventListener('click', load);
    load();
})();
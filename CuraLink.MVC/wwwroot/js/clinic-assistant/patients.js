(function () {
    'use strict';
    const UI = window.AssistantUI;
    const $ = (id) => document.getElementById(id);
    const input = $('patientSearch');
    const panels = ['psHint', 'psLoading', 'psError', 'psEmpty', 'psTableWrap'];

    let timer = null;
    let seq = 0;

    function only(id) { panels.forEach((p) => { $(p).hidden = p !== id; }); }

    async function search() {
        const term = input.value.trim();
        if (!term) { seq++; only('psHint'); return; }

        const mine = ++seq;
        only('psLoading');

        const r = await UI.get(UI.urls.searchPatients + '?search=' + encodeURIComponent(term));
        if (mine !== seq) return; // a newer search is running

        if (!r.ok) {
            $('psError').textContent = r.message;
            only('psError');
            UI.loginRedirectIfExpired(r);
            return;
        }

        const list = Array.isArray(r.data) ? r.data : [];
        if (list.length === 0) { only('psEmpty'); return; }

        $('psRows').innerHTML = list.map((p) => {
            const name = ((p.firstName || '') + ' ' + (p.lastName || '')).trim();
            const href = UI.urls.bookPage + '?patientId=' + encodeURIComponent(p.patientId) +
                '&patientName=' + encodeURIComponent(name);
            return '<tr>' +
                '<td class="fw-semibold">' + UI.esc(name) + '</td>' +
                '<td>' + UI.esc(p.email) + '</td>' +
                '<td>' + UI.esc(p.phoneNumber) + '</td>' +
                '<td class="text-end"><a class="btn btn-sm btn-primary" href="' + UI.esc(href) + '">' +
                '<i class="bi bi-calendar-plus me-1"></i>Book Appointment</a></td></tr>';
        }).join('');
        only('psTableWrap');
    }

    input.addEventListener('input', () => { clearTimeout(timer); timer = setTimeout(search, 400); });
    input.addEventListener('keydown', (e) => { if (e.key === 'Enter') { clearTimeout(timer); search(); } });
    $('patientSearchBtn').addEventListener('click', () => { clearTimeout(timer); search(); });
})();
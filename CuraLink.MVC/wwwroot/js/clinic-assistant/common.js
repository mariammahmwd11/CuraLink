// Shared helpers for the receptionist (clinic assistant) pages.
window.AssistantUI = (function () {
    'use strict';

    const urls = window.assistantUrls || {};
    const GENERIC = 'Something went wrong while contacting the server. Please try again.';
    const SESSION = 'Your session has expired or you do not have permission. Please log in again.';

    const csrf = () => document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

    function esc(value) {
        const d = document.createElement('div');
        d.textContent = value == null ? '' : String(value);
        return d.innerHTML;
    }

    function fmtTime(t) {
        if (!t) return '';
        const s = String(t);
        return /^\d{2}:\d{2}:\d{2}/.test(s) ? s.slice(0, 5) : s;
    }

    // Pulls a readable message out of {message}/{detail}/{error}/{title}/{errors:{...}}.
    function extractMessage(data) {
        if (!data || typeof data !== 'object') return null;
        if (data.errors && typeof data.errors === 'object') {
            const parts = [];
            Object.values(data.errors).forEach((v) => (Array.isArray(v) ? parts.push(...v) : parts.push(v)));
            if (parts.length) return parts.join(' ');
        }
        return data.message || data.detail || data.error || data.title || null;
    }

    function errorMessage(status, data) {
        if (status === 401 || status === 403) return SESSION;
        if (status === 0 || status >= 500) return GENERIC;
        return extractMessage(data) || 'The request could not be completed. Please check the information and try again.';
    }

    async function request(url, options) {
        let res;
        try {
            res = await fetch(url, Object.assign({ credentials: 'same-origin' }, options));
        } catch (e) {
            return { ok: false, status: 0, data: null, message: GENERIC };
        }
        // Cookie auth redirects to the login page when the session expired.
        if (res.redirected) return { ok: false, status: 401, data: null, message: SESSION };

        let data = null;
        const text = await res.text();
        if (text) { try { data = JSON.parse(text); } catch (e) { /* not JSON */ } }

        return {
            ok: res.ok, status: res.status, data: data,
            message: res.ok ? null : errorMessage(res.status, data)
        };
    }

    const get = (url) => request(url, { headers: { Accept: 'application/json' } });

    const post = (url, body) => request(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            // The server reads only the header name configured in AddAntiforgery; send the common ones.
            'RequestVerificationToken': csrf(),
            'X-CSRF-TOKEN': csrf(),
            'X-XSRF-TOKEN': csrf()
        },
        body: body === undefined ? undefined : JSON.stringify(body)
    });

    function toast(message, type) {
        let box = document.getElementById('assistantToasts');
        if (!box) {
            box = document.createElement('div');
            box.id = 'assistantToasts';
            box.style.cssText = 'position:fixed;top:1rem;right:1rem;z-index:2000;display:flex;flex-direction:column;gap:.5rem;max-width:360px;';
            document.body.appendChild(box);
        }
        const el = document.createElement('div');
        el.className = 'alert alert-' + (type || 'success') + ' shadow-sm mb-0';
        el.setAttribute('role', 'alert');
        el.textContent = message;
        box.appendChild(el);
        setTimeout(() => el.remove(), 4500);
    }

    const STATUS = {
        confirmed: ['Confirmed', 'bg-warning text-dark'],
        checkedin: ['Checked in', 'bg-primary'],
        completed: ['Completed', 'bg-success'],
        cancelled: ['Cancelled', 'bg-secondary'],
        canceled: ['Cancelled', 'bg-secondary']
    };

    function statusBadge(status) {
        const key = String(status || '').replace(/[\s_-]/g, '').toLowerCase();
        const m = STATUS[key] || [status || 'Unknown', 'bg-light text-dark border'];
        return '<span class="badge ' + m[1] + '">' + esc(m[0]) + '</span>';
    }

    const isConfirmed = (s) => String(s || '').toLowerCase() === 'confirmed';
    const normStatus = (s) => String(s || '').replace(/[\s_-]/g, '').toLowerCase();

    // Check-in action shared by the dashboard and today's appointments pages.
    async function checkIn(id, button) {
        const original = button ? button.innerHTML : '';
        if (button) {
            button.disabled = true;
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Checking in...';
        }
        const r = await post(urls.checkIn + '?id=' + encodeURIComponent(id));
        if (r.ok) {
            toast((r.data && r.data.message) || 'Patient checked in successfully.', 'success');
        } else {
            toast(r.message, 'danger');
            if (button) { button.disabled = false; button.innerHTML = original; }
        }
        return r;
    }

    function loginRedirectIfExpired(r) {
        if (r.status === 401 && urls.loginPage) {
            setTimeout(() => { window.location.href = urls.loginPage; }, 1800);
        }
    }

    return {
        urls, esc, fmtTime, get, post, toast, statusBadge, isConfirmed, normStatus,
        checkIn, errorMessage, loginRedirectIfExpired
    };
})();
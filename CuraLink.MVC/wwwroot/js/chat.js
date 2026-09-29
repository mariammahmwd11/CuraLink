/* CuraLink Chat UI
 * - Uses the global SignalR connection owned by realtime.js (no connection created here).
 * - Talks only to same-origin MVC actions; the JWT never reaches the browser.
 */
(function () {
    'use strict';

    var cfg = window.chatConfig;
    if (!cfg || !cfg.appointmentId) {
        console.error('[chat] chatConfig missing.');
        return;
    }

    var appointmentId = Number(cfg.appointmentId);
    var currentUserId = cfg.currentUserId ? String(cfg.currentUserId) : null;
    var MAX_LEN = cfg.maxLength || 2000;

    // Must match options.HeaderName in Program.cs (AddAntiforgery).
    var CSRF_HEADER = 'X-CSRF-TOKEN';

    var els = {};
    var isSending = false;
    var isMarkingRead = false;

    // ---------- Helpers ----------

    function $(id) { return document.getElementById(id); }

    function getAntiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function csrfHeaders(extra) {
        var headers = Object.assign({}, extra || {});
        headers[CSRF_HEADER] = getAntiForgeryToken();
        return headers;
    }

    function normalize(m) {
        if (!m) return null;
        var id = m.id !== undefined && m.id !== null ? m.id : m.messageId;
        if (id === undefined || id === null) return null;
        return {
            id: id,
            appointmentId: Number(m.appointmentId),
            senderId: m.senderId != null ? String(m.senderId) : '',
            receiverId: m.receiverId != null ? String(m.receiverId) : '',
            content: m.content || '',
            sentAt: m.sentAt,
            isRead: !!m.isRead,
            readAt: m.readAt || null
        };
    }

    function sameId(a, b) {
        return a != null && b != null && String(a).toLowerCase() === String(b).toLowerCase();
    }

    function isMine(m) { return sameId(m.senderId, currentUserId); }

    function messageAlreadyExists(messageId) {
        return !!document.getElementById('message-' + messageId);
    }

    function formatTime(iso) {
        if (!iso) return '';
        // Backend timestamps are UTC; ensure "Z" so the browser converts to local time.
        var s = /[zZ]|[+-]\d{2}:?\d{2}$/.test(iso) ? iso : iso + 'Z';
        var d = new Date(s);
        if (isNaN(d.getTime())) return '';
        return d.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' });
    }

    function toTimestamp(iso) {
        if (!iso) return Date.now();
        var s = /[zZ]|[+-]\d{2}:?\d{2}$/.test(iso) ? iso : iso + 'Z';
        var t = new Date(s).getTime();
        return isNaN(t) ? Date.now() : t;
    }

    function api(url, options) {
        options = options || {};
        options.credentials = 'same-origin';
        options.headers = Object.assign({ 'Accept': 'application/json' }, options.headers || {});
        return fetch(url, options).then(function (res) {
            if (!res.ok) {
                return res.text().then(function (body) {
                    console.error('[chat] API error', res.status, url, body || '(empty body)');
                    var err = new Error('Request failed with status ' + res.status);
                    err.status = res.status;
                    err.body = body;
                    throw err;
                });
            }
            return res.status === 204 ? null : res.json();
        });
    }

    // ---------- UI state ----------

    function show(el, visible) { if (el) el.hidden = !visible; }

    function showBanner(text) {
        els.bannerText.textContent = text;
        show(els.banner, true);
    }

    function hideBanner() { show(els.banner, false); }

    function updateEmptyState() {
        var hasMessages = !!els.messages.querySelector('.chat-msg');
        show(els.empty, !hasMessages && els.loading.hidden && els.error.hidden);
    }

    function scrollToBottom(smooth) {
        var el = els.messages;
        if (typeof el.scrollTo === 'function') {
            el.scrollTo({ top: el.scrollHeight, behavior: smooth ? 'smooth' : 'auto' });
        } else {
            el.scrollTop = el.scrollHeight;
        }
    }

    // ---------- Rendering ----------

    function statusHtml(m) {
        return m.isRead
            ? '<span class="chat-ticks chat-ticks--read" title="Read">\u2713\u2713 Read</span>'
            : '<span class="chat-ticks" title="Sent">\u2713 Sent</span>';
    }

    function buildMessageElement(m) {
        var mine = isMine(m);
        var row = document.createElement('div');
        row.id = 'message-' + m.id;
        row.className = 'chat-msg ' + (mine ? 'chat-msg--out' : 'chat-msg--in');
        row.dataset.sentAt = String(toTimestamp(m.sentAt));
        row.dataset.messageId = String(m.id);

        var bubble = document.createElement('div');
        bubble.className = 'chat-bubble';

        var text = document.createElement('div');
        text.className = 'chat-text';
        text.textContent = m.content; // textContent => no XSS

        var meta = document.createElement('div');
        meta.className = 'chat-meta';

        var time = document.createElement('span');
        time.className = 'chat-time';
        time.textContent = formatTime(m.sentAt);
        meta.appendChild(time);

        if (mine) {
            var status = document.createElement('span');
            status.className = 'chat-status-mark';
            status.innerHTML = statusHtml(m);
            meta.appendChild(status);
        }

        bubble.appendChild(text);
        bubble.appendChild(meta);
        row.appendChild(bubble);
        return row;
    }

    // Inserts keeping chronological order (handles SignalR message arriving before history).
    function insertMessage(m) {
        var el = buildMessageElement(m);
        var ts = Number(el.dataset.sentAt);
        var nodes = els.messages.querySelectorAll('.chat-msg');
        var before = null;
        for (var i = nodes.length - 1; i >= 0; i--) {
            if (Number(nodes[i].dataset.sentAt) <= ts) { break; }
            before = nodes[i];
        }
        if (before) {
            els.messages.insertBefore(el, before);
        } else {
            els.messages.appendChild(el);
        }
        show(els.empty, false);
    }

    function renderMessage(raw) {
        var m = normalize(raw);
        if (!m) return null;
        if (messageAlreadyExists(m.id)) return m;
        insertMessage(m);
        return m;
    }

    // ---------- History ----------

    function loadHistory() {
        show(els.error, false);
        show(els.empty, false);
        show(els.loading, true);
        hideBanner();

        var url = cfg.urls.messages + '?appointmentId=' + encodeURIComponent(appointmentId);

        return api(url)
            .then(function (data) {
                var list = Array.isArray(data) ? data : [];
                var needsRead = false;

                list.forEach(function (raw) {
                    var m = renderMessage(raw);
                    if (m && sameId(m.receiverId, currentUserId) && !m.isRead) needsRead = true;
                });

                show(els.loading, false);
                updateEmptyState();
                scrollToBottom(false);

                if (needsRead) markAsRead();
            })
            .catch(function (err) {
                console.error('[chat] Failed to load messages:', err);
                show(els.loading, false);
                show(els.error, true);
                showBanner('Unable to load the conversation. Please try again.');
            });
    }

    // ---------- Read receipts (outgoing action) ----------

    function markAsRead() {
        if (isMarkingRead) return;
        isMarkingRead = true;

        var url = cfg.urls.markRead + '?appointmentId=' + encodeURIComponent(appointmentId);
        api(url, {
            method: 'POST',
            headers: csrfHeaders()
        })
            .catch(function (err) {
                // Non-critical: log only, don't interrupt the conversation.
                console.error('[chat] Failed to mark messages as read:', err);
            })
            .then(function () { isMarkingRead = false; });
    }

    // ---------- Sending ----------

    function updateComposerState() {
        var len = els.input.value.length;
        els.counter.textContent = len + ' / ' + MAX_LEN;
        els.counter.classList.toggle('chat-counter--limit', len >= MAX_LEN);
        els.send.disabled = isSending || els.input.value.trim().length === 0;
    }

    function autoResize() {
        els.input.style.height = 'auto';
        els.input.style.height = Math.min(els.input.scrollHeight, 140) + 'px';
    }

    function sendMessage() {
        if (isSending) return;

        var content = els.input.value.trim();
        if (!content) return;
        if (content.length > MAX_LEN) content = content.substring(0, MAX_LEN);

        isSending = true;
        updateComposerState();
        hideBanner();

        var url = cfg.urls.send + '?appointmentId=' + encodeURIComponent(appointmentId);

        api(url, {
            method: 'POST',
            headers: csrfHeaders({ 'Content-Type': 'application/json' }),
            body: JSON.stringify({ content: content })
        })
            .then(function (data) {
                // Render from the API response; the SignalR echo (if any) is de-duplicated by ID.
                var raw = Object.assign({ appointmentId: appointmentId, senderId: currentUserId }, data || {});
                renderMessage(raw);

                els.input.value = '';
                autoResize();
                scrollToBottom(true);
            })
            .catch(function (err) {
                console.error('[chat] Failed to send message:', err);
                showBanner('Message could not be sent. Please try again.');
            })
            .then(function () {
                isSending = false;
                updateComposerState();
                els.input.focus();
            });
    }

    // ---------- Realtime hooks (called by realtime.js) ----------

    window.addMessageToChat = function (message) {
        var m = normalize(message);
        if (!m) return;

        // Ignore messages that belong to other appointments.
        if (m.appointmentId !== appointmentId) return;

        if (messageAlreadyExists(m.id)) return; // prevent duplicates (REST + SignalR)

        renderMessage(m);
        scrollToBottom(true);

        // If I'm the receiver and this page is open, mark as read.
        if (sameId(m.receiverId, currentUserId) && !document.hidden) {
            markAsRead();
        }
    };

    window.markMessagesAsReadInUI = function (receipt) {
        if (!receipt || Number(receipt.appointmentId) !== appointmentId) return;

        var ids = Array.isArray(receipt.messageIds) ? receipt.messageIds : [];
        var updated = false;

        ids.forEach(function (id) {
            var row = document.getElementById('message-' + id);
            if (!row || !row.classList.contains('chat-msg--out')) return;

            var mark = row.querySelector('.chat-status-mark');
            if (mark) {
                mark.innerHTML = statusHtml({ isRead: true });
                updated = true;
            }
        });

        if (updated) scrollToBottom(true);
    };

    // ---------- Connection status indicator (read-only, no new connection) ----------

    function refreshConnectionStatus() {
        var conn = window.realtimeConnection;
        var state = conn && conn.state ? String(conn.state) : '';
        var connected = state === 'Connected';
        var connecting = state === 'Connecting' || state === 'Reconnecting';

        els.statusDot.className = 'chat-status-dot' +
            (connected ? ' is-online' : connecting ? ' is-connecting' : '');
        els.statusText.textContent = connected ? 'Live' : connecting ? 'Connecting' : 'Offline';
    }

    // ---------- Init ----------

    function init() {
        els.messages = $('chatMessages');
        els.loading = $('chatLoading');
        els.error = $('chatError');
        els.empty = $('chatEmpty');
        els.retry = $('chatRetry');
        els.banner = $('chatBanner');
        els.bannerText = $('chatBannerText');
        els.bannerClose = $('chatBannerClose');
        els.input = $('chatInput');
        els.send = $('chatSend');
        els.counter = $('chatCounter');
        els.statusDot = $('chatStatusDot');
        els.statusText = $('chatStatusText');

        els.input.addEventListener('input', function () {
            if (els.input.value.length > MAX_LEN) {
                els.input.value = els.input.value.substring(0, MAX_LEN);
            }
            autoResize();
            updateComposerState();
        });

        els.input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey && !e.isComposing) {
                e.preventDefault();
                sendMessage();
            }
        });

        els.send.addEventListener('click', sendMessage);
        els.retry.addEventListener('click', loadHistory);
        els.bannerClose.addEventListener('click', hideBanner);

        // When the tab becomes visible again, acknowledge any incoming messages.
        document.addEventListener('visibilitychange', function () {
            if (document.hidden) return;
            var hasIncoming = els.messages.querySelector('.chat-msg--in');
            if (hasIncoming) markAsRead();
        });

        updateComposerState();
        refreshConnectionStatus();
        setInterval(refreshConnectionStatus, 3000);

        loadHistory();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
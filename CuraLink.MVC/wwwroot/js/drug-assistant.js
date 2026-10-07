(function () {
    'use strict';

    const cfg = window.drugAssistantConfig;
    if (!cfg) return;

    const $ = (id) => document.getElementById(id);

    const input = $('daInput');
    const sendBtn = $('daSend');
    const sendLabel = $('daSendLabel');
    const sendSpinner = $('daSendSpinner');

    const counter = $('daCounter');
    const banner = $('daBanner');
    const bannerText = $('daBannerText');

    const intro = $('daIntro');
    const loading = $('daLoading');
    const notice = $('daNotice');
    const result = $('daResult');

    let busy = false;

    const show = (n) => {
        n.hidden = false;
    };

    const hide = (n) => {
        n.hidden = true;
    };

    const hasText = (s) =>
        typeof s === 'string' && s.trim().length > 0;

    const token = () =>
        document.querySelector(
            'input[name="__RequestVerificationToken"]'
        )?.value || '';

    function updateComposer() {
        const len = input.value.length;

        counter.textContent =
            len + ' / ' + cfg.maxLength;

        counter.classList.toggle(
            'chat-counter--limit',
            len >= cfg.maxLength
        );

        sendBtn.disabled =
            busy || input.value.trim().length === 0;

        input.style.height = 'auto';
        input.style.height =
            Math.min(input.scrollHeight, 140) + 'px';
    }

    function setBusy(v) {
        busy = v;

        input.readOnly = v;

        sendSpinner.hidden = !v;

        sendLabel.textContent =
            v ? 'Analyzing...' : 'Ask Assistant';

        updateComposer();
    }

    function showBanner(text) {
        bannerText.textContent = text;
        banner.hidden = false;
    }

    $('daBannerClose').addEventListener('click', () => {
        banner.hidden = true;
    });

    function showNotice(text, warning) {
        notice.textContent = text;

        notice.className =
            'da-notice' +
            (warning ? ' da-notice--warning' : '');

        show(notice);
    }

    function resetOutput() {
        hide(intro);
        hide(result);
        hide(notice);

        banner.hidden = true;
    }

    function render(data, asked) {

        if (!data || !hasText(data.answer)) {
            showNotice(
                'The assistant could not generate an answer. Please try again.',
                true
            );

            return;
        }

        $('daAsked').textContent =
            'Your question: ' + asked;

        $('daAnswer').textContent =
            data.answer.trim();

        show(result);
    }

    async function ask() {

        if (busy) return;

        const query = input.value.trim();

        if (!query) return;

        resetOutput();

        show(loading);
        setBusy(true);

        try {

            const res = await fetch(
                cfg.urls.query,
                {
                    method: 'POST',
                    credentials: 'same-origin',

                    headers: {
                        'Content-Type': 'application/json',

                        'RequestVerificationToken': token(),
                        'X-CSRF-TOKEN': token(),
                        'X-XSRF-TOKEN': token(),
                        'X-Requested-With': 'XMLHttpRequest'
                    },

                    body: JSON.stringify({
                        query: query
                    })
                }
            );

            // Session expired / unauthorized
            if (
                res.redirected ||
                res.status === 401 ||
                res.status === 403
            ) {
                showBanner(
                    'Please log in to use the AI Drug Assistant.'
                );

                show(intro);

                return;
            }

            // Invalid request
            if (res.status === 400) {

                showBanner(
                    'Please enter a valid question about a medication or drug interaction.'
                );

                show(intro);

                return;
            }

            if (!res.ok) {
                throw new Error(
                    'Drug assistant request failed.'
                );
            }

            const data = await res.json();

            console.log('Drug Assistant Response:', data);

            render(data, query);

        } catch (error) {

            console.error(
                'Drug Assistant Error:',
                error
            );

            showBanner(
                'Something went wrong while contacting the drug assistant. Please try again.'
            );

            show(intro);

        } finally {

            hide(loading);

            setBusy(false);

            $('daBody').scrollTop = 0;
        }
    }

    input.addEventListener(
        'input',
        updateComposer
    );

    input.addEventListener(
        'keydown',
        (e) => {

            if (
                e.key === 'Enter' &&
                !e.shiftKey
            ) {
                e.preventDefault();

                ask();
            }
        }
    );

    sendBtn.addEventListener(
        'click',
        ask
    );

    document
        .querySelectorAll('.da-example')
        .forEach((b) => {

            b.addEventListener(
                'click',
                () => {

                    input.value =
                        b.textContent.trim();

                    updateComposer();

                    input.focus();
                }
            );

        });

    updateComposer();

})();
(function () {
    'use strict';

    const cfg = window.drugAssistantConfig;
    if (!cfg) return;

    const $ = (id) => document.getElementById(id);
    const input = $('daInput'), sendBtn = $('daSend'), sendLabel = $('daSendLabel'), sendSpinner = $('daSendSpinner');
    const counter = $('daCounter'), banner = $('daBanner'), bannerText = $('daBannerText');
    const intro = $('daIntro'), loading = $('daLoading'), notice = $('daNotice'), result = $('daResult');
    const DEFAULT_DISCLAIMER = 'This information is for educational purposes only and should not replace professional medical advice.';

    let busy = false;

    const show = (n) => { n.hidden = false; };
    const hide = (n) => { n.hidden = true; };
    const hasItems = (a) => Array.isArray(a) && a.length > 0;
    const hasText = (s) => typeof s === 'string' && s.trim().length > 0;
    const token = () => document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

    function updateComposer() {
        const len = input.value.length;
        counter.textContent = len + ' / ' + cfg.maxLength;
        counter.classList.toggle('chat-counter--limit', len >= cfg.maxLength);
        sendBtn.disabled = busy || input.value.trim().length === 0;
        input.style.height = 'auto';
        input.style.height = Math.min(input.scrollHeight, 140) + 'px';
    }

    function setBusy(v) {
        busy = v;
        input.readOnly = v;
        sendSpinner.hidden = !v;
        sendLabel.textContent = v ? 'Analyzing...' : 'Ask Assistant';
        updateComposer();
    }

    function showBanner(text) { bannerText.textContent = text; banner.hidden = false; }
    $('daBannerClose').addEventListener('click', () => { banner.hidden = true; });

    function showNotice(text, warning) {
        notice.textContent = text;
        notice.className = 'da-notice' + (warning ? ' da-notice--warning' : '');
        show(notice);
    }

    function resetOutput() {
        hide(intro); hide(result); hide(notice); banner.hidden = true;
    }

    function fillList(ul, items) {
        ul.replaceChildren();
        items.forEach((t) => { const li = document.createElement('li'); li.textContent = t; ul.appendChild(li); });
    }

    function toggle(block, on) { block.hidden = !on; }

    function render(data, asked) {
        const medication = hasText(data.medication) ? data.medication.trim() : '';
        const warnings = hasItems(data.warnings) ? data.warnings : [];

        const notFound = warnings.some((w) => /no fda drug label was found/i.test(w)) &&
            !hasText(data.dosageInformation) && !hasItems(data.activeIngredients) && !hasItems(data.interactionWarnings);

        if (!medication || notFound) {
            const base = warnings.length ? warnings.join(' ') : 'No medication could be identified from your question.';
            const hint = !medication
                ? ' Try naming a specific medication, for example "What are the side effects of ibuprofen?"'
                : ' Check the spelling or try the generic name.';
            showNotice(base + hint, !!medication);
            return;
        }

        $('daAsked').textContent = 'Your question: ' + asked;
        $('daMedication').textContent = medication;

        const chips = $('daIngredients'); chips.replaceChildren();
        if (hasItems(data.activeIngredients)) {
            data.activeIngredients.forEach((n) => {
                const s = document.createElement('span'); s.className = 'da-chip'; s.textContent = n; chips.appendChild(s);
            });
        }
        toggle($('daIngredientsBlock'), hasItems(data.activeIngredients));

        if (hasText(data.dosageInformation)) $('daDosage').textContent = data.dosageInformation;
        toggle($('daDosageBlock'), hasText(data.dosageInformation));

        if (hasItems(data.commonSideEffects)) fillList($('daSideEffects'), data.commonSideEffects);
        toggle($('daSideEffectsBlock'), hasItems(data.commonSideEffects));

        const wrap = $('daInteractions'); wrap.replaceChildren();
        if (hasItems(data.interactionWarnings)) {
            data.interactionWarnings.forEach((it) => {
                const card = document.createElement('div'); card.className = 'da-interaction';
                const l = document.createElement('div'); l.className = 'da-interaction-label'; l.textContent = 'Drug interaction';
                const d = document.createElement('div'); d.className = 'da-interaction-drug'; d.textContent = it.drug || '';
                const p = document.createElement('p'); p.textContent = it.description || '';
                card.append(l, d, p); wrap.appendChild(card);
            });
        }
        toggle($('daInteractionsBlock'), hasItems(data.interactionWarnings));

        if (warnings.length) fillList($('daWarnings'), warnings);
        toggle($('daWarningsBlock'), warnings.length > 0);

        $('daDisclaimer').textContent = hasText(data.disclaimer) ? data.disclaimer : DEFAULT_DISCLAIMER;
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
            const res = await fetch(cfg.urls.query, {
                method: 'POST',
                credentials: 'same-origin',
                headers: {
                    'Content-Type': 'application/json',
                    // The server only reads the header name configured in AddAntiforgery,
                    // so send the common names; the matching one is accepted.
                    'RequestVerificationToken': token(),
                    'X-CSRF-TOKEN': token(),
                    'X-XSRF-TOKEN': token(),
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({ query })
            });

            // Cookie auth redirects to the login page when the session expired.
            if (res.redirected || res.status === 401 || res.status === 403) {
                showBanner('Please log in to use the AI Drug Assistant.');
                show(intro);
                return;
            }
            if (res.status === 400) {
                showBanner('Please enter a valid question about a medication or drug interaction.');
                show(intro);
                return;
            }
            if (!res.ok) throw new Error();

            render(await res.json(), query);
        } catch {
            showBanner('Something went wrong while contacting the drug assistant. Please try again.');
            show(intro);
        } finally {
            hide(loading);
            setBusy(false);
            $('daBody').scrollTop = 0;
        }
    }

    input.addEventListener('input', updateComposer);
    input.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); ask(); }
    });
    sendBtn.addEventListener('click', ask);
    document.querySelectorAll('.da-example').forEach((b) => b.addEventListener('click', () => {
        input.value = b.textContent.trim(); updateComposer(); input.focus();
    }));

    updateComposer();
})();
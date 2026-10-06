(function () {
    'use strict';
    const UI = window.AssistantUI;
    const $ = (id) => document.getElementById(id);
    const form = $('cpForm');

    const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const EG_PHONE = /^01[0125][0-9]{8}$/;

    // Date of birth must be in the past.
    const yesterday = new Date(); yesterday.setDate(yesterday.getDate() - 1);
    $('cpDob').max = yesterday.toISOString().slice(0, 10);

    function setError(input, message) {
        input.classList.toggle('is-invalid', !!message);
        const fb = input.parentElement.querySelector('.invalid-feedback');
        if (fb) fb.textContent = message || '';
    }

    function validate() {
        let ok = true;
        const check = (id, message) => { setError($(id), message); if (message) ok = false; };

        check('cpFirst', $('cpFirst').value.trim() ? '' : 'First name is required.');
        check('cpLast', $('cpLast').value.trim() ? '' : 'Last name is required.');

        const email = $('cpEmail').value.trim();
        check('cpEmail', !email ? 'Email is required.' : !EMAIL.test(email) ? 'Enter a valid email address.' : '');

        const phone = $('cpPhone').value.trim();
        check('cpPhone', !phone ? 'Phone number is required.'
            : !EG_PHONE.test(phone) ? 'Enter a valid Egyptian phone number (e.g. 01012345678).' : '');

        const dob = $('cpDob').value;
        check('cpDob', !dob ? 'Date of birth is required.'
            : new Date(dob) >= new Date(new Date().toDateString()) ? 'Date of birth must be in the past.' : '');

        return ok;
    }

    function setBusy(busy) {
        $('cpSubmit').disabled = busy;
        $('cpSpinner').hidden = !busy;
        $('cpLabel').textContent = busy ? 'Creating...' : 'Create patient';
    }

    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        $('cpError').hidden = true;
        $('cpSuccess').hidden = true;
        if (!validate()) return;

        const first = $('cpFirst').value.trim();
        const last = $('cpLast').value.trim();

        setBusy(true);
        const r = await UI.post(UI.urls.createPatientApi, {
            firstName: first,
            lastName: last,
            email: $('cpEmail').value.trim(),
            phone: $('cpPhone').value.trim(),
            dateOfBirth: $('cpDob').value,
            bloodType: $('cpBlood').value || null,
            medicalHistoryNotes: $('cpNotes').value.trim() || null
        });
        setBusy(false);

        if (!r.ok) {
            $('cpError').textContent = r.message;
            $('cpError').hidden = false;
            UI.loginRedirectIfExpired(r);
            return;
        }

        const message = (r.data && r.data.message) || 'Patient created successfully.';
        UI.toast(message, 'success');
        $('cpSuccessText').textContent = message;

        const patientId = r.data && r.data.patientId;
        const link = $('cpBookLink');
        if (patientId) {
            link.href = UI.urls.bookPage + '?patientId=' + encodeURIComponent(patientId) +
                '&patientName=' + encodeURIComponent(first + ' ' + last);
            link.hidden = false;
        } else {
            link.hidden = true;
        }

        form.reset();
        form.querySelectorAll('.is-invalid').forEach((el) => el.classList.remove('is-invalid'));
        $('cpSuccess').hidden = false;
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });
})();
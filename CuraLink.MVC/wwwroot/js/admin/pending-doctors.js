(function () {
    "use strict";

    // ===============================================================
    // MVC endpoints
    // The browser talks to MVC.
    // MVC then calls the backend API using the admin JWT.
    // ===============================================================
    const ENDPOINTS = {
        list: "/Admin/GetPendingDoctorsData",

        documents: (doctorId) =>
            `/Admin/GetDoctorDocumentsData?doctorId=${encodeURIComponent(doctorId)}`,

        verify: (doctorId) =>
            `/Admin/VerifyDoctor?doctorId=${encodeURIComponent(doctorId)}`,

        download: (doctorId, documentId) =>
            `/Admin/DownloadDocument?doctorId=${encodeURIComponent(doctorId)}&documentId=${encodeURIComponent(documentId)}`
    };

    // ===============================================================
    // Doctor status
    // ===============================================================
    const DOCTOR_STATUS = {
        0: { label: "Unverified", css: "secondary" },
        1: { label: "Pending", css: "warning text-dark" },
        2: { label: "Approved", css: "success" },
        3: { label: "Rejected", css: "danger" }
    };

    let doctorsById = new Map();
    let activeDoctorId = null;

    let toastInstance = null;
    let reviewModalInstance = null;
    let approveModalInstance = null;

    // ===============================================================
    // Initialization
    // ===============================================================
    document.addEventListener("DOMContentLoaded", init);

    function init() {

        const reviewModal = document.getElementById("reviewDoctorModal");
        const approveModal = document.getElementById("approveConfirmModal");
        const toast = document.getElementById("actionToast");

        if (!reviewModal || !approveModal || !toast) {
            console.error("Required Bootstrap elements were not found.");
            return;
        }

        reviewModalInstance = new bootstrap.Modal(reviewModal);
        approveModalInstance = new bootstrap.Modal(approveModal);

        toastInstance = new bootstrap.Toast(toast, {
            delay: 4000
        });

        document
            .getElementById("btnRefresh")
            .addEventListener("click", loadPendingDoctors);

        document
            .getElementById("btnStartReject")
            .addEventListener("click", showRejectionSection);

        document
            .getElementById("btnCancelReject")
            .addEventListener("click", hideRejectionSection);

        document
            .getElementById("rejectionReasonInput")
            .addEventListener("input", validateRejectionReason);

        document
            .getElementById("btnConfirmReject")
            .addEventListener("click", submitRejection);

        document
            .getElementById("btnApprove")
            .addEventListener("click", openApproveConfirm);

        document
            .getElementById("btnConfirmApprove")
            .addEventListener("click", submitApproval);

        // Bootstrap doesn't correctly support one modal opening on top of
        // another out of the box — stacking reviewDoctorModal and
        // approveConfirmModal left the body stuck with a dark backdrop
        // and no scrolling once the top modal closed. openApproveConfirm()
        // now hides the review modal first and only opens the confirm
        // modal after that hide transition fully finishes. This listener
        // handles bringing the review modal back if the confirm modal is
        // closed without a successful approval (Cancel, or a failed request).
        approveModal.addEventListener("hidden.bs.modal", function () {
            if (activeDoctorId && doctorsById.has(String(activeDoctorId))) {
                reviewModalInstance.show();
            }
        });

        loadPendingDoctors();
    }

    // ===============================================================
    // Load Pending Doctors
    // ===============================================================
    async function loadPendingDoctors() {

        showLoading();

        try {

            console.log("GET:", ENDPOINTS.list);

            const response = await fetch(ENDPOINTS.list, {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                },
                credentials: "same-origin"
            });

            console.log("Pending doctors response:", response.status);

            if (!response.ok) {
                const message = await extractErrorMessage(
                    response,
                    "Failed to load pending doctors."
                );

                showPageError(message);
                return;
            }

            const doctors = await response.json();

            console.log("Pending doctors:", doctors);

            renderDoctors(
                Array.isArray(doctors) ? doctors : []
            );

        } catch (error) {

            console.error(
                "loadPendingDoctors failed:",
                error
            );

            showPageError(
                "Could not reach the server. Please try again."
            );
        }
    }

    // ===============================================================
    // Render Doctors
    // ===============================================================
    function renderDoctors(doctors) {

        doctorsById = new Map(
            doctors.map(d => [
                String(d.id),
                d
            ])
        );

        const tbody =
            document.getElementById("doctorsTableBody");

        tbody.innerHTML = "";

        hidePageError();

        if (doctors.length === 0) {
            toggleSections("empty");
            return;
        }

        tbody.innerHTML =
            doctors
                .map(renderDoctorRow)
                .join("");

        tbody
            .querySelectorAll("[data-review-id]")
            .forEach(button => {

                button.addEventListener(
                    "click",
                    function () {

                        const doctorId =
                            this.getAttribute("data-review-id");

                        openReviewModal(doctorId);
                    }
                );
            });

        toggleSections("table");
    }

    function renderDoctorRow(doctor) {

        const status =
            DOCTOR_STATUS[doctor.status] ??
            {
                label: "Unknown",
                css: "secondary"
            };

        const fullName =
            `${doctor.firstName ?? ""} ${doctor.lastName ?? ""}`
                .trim() || "-";

        return `
            <tr id="doctor-row-${escapeHtml(doctor.id)}">

                <td>
                    ${escapeHtml(fullName)}
                </td>

                <td>
                    ${escapeHtml(doctor.email ?? "-")}
                </td>

                <td>
                    ${escapeHtml(doctor.phoneNumber ?? "-")}
                </td>

                <td>
                    ${escapeHtml(doctor.specialty ?? "-")}
                </td>

                <td>
                    ${escapeHtml(doctor.syndicateId ?? "-")}
                </td>

                <td>
                    <span class="badge bg-${status.css}">
                        ${status.label}
                    </span>
                </td>

                <td class="text-end">

                    <button
                        type="button"
                        class="btn btn-sm btn-primary"
                        data-review-id="${escapeHtml(doctor.id)}">

                        <i class="bi bi-eye me-1"></i>
                        Review

                    </button>

                </td>

            </tr>
        `;
    }

    // ===============================================================
    // Open Review Modal
    // ===============================================================
    async function openReviewModal(doctorId) {

        const doctor =
            doctorsById.get(String(doctorId));

        if (!doctor) {
            console.error(
                "Doctor not found:",
                doctorId
            );
            return;
        }

        activeDoctorId = doctor.id;

        resetReviewModal();

        populateDoctorFields(doctor);

        reviewModalInstance.show();

        // ===========================================================
        // Get Documents
        // ===========================================================
        try {

            console.log(
                "GET documents:",
                ENDPOINTS.documents(doctor.id)
            );

            const response = await fetch(
                ENDPOINTS.documents(doctor.id),
                {
                    method: "GET",
                    headers: {
                        "Accept": "application/json"
                    },
                    credentials: "same-origin"
                }
            );

            console.log(
                "Documents response:",
                response.status
            );

            document
                .getElementById("rd-documentLoading")
                .classList
                .add("d-none");

            if (!response.ok) {

                const message =
                    await extractErrorMessage(
                        response,
                        "Failed to load verification documents."
                    );

                showReviewModalError(message);

                return;
            }

            const documents =
                await response.json();

            console.log(
                "Doctor documents:",
                documents
            );

            renderDocumentInfo(
                doctor.id,
                Array.isArray(documents)
                    ? documents
                    : []
            );

        } catch (error) {

            console.error(
                "Loading documents failed:",
                error
            );

            document
                .getElementById("rd-documentLoading")
                .classList
                .add("d-none");

            showReviewModalError(
                "Could not load the verification document."
            );
        }
    }

    // ===============================================================
    // Doctor Information
    // ===============================================================
    function populateDoctorFields(doctor) {

        const status =
            DOCTOR_STATUS[doctor.status] ??
            {
                label: "Unknown",
                css: "secondary"
            };

        const fullName =
            `${doctor.firstName ?? ""} ${doctor.lastName ?? ""}`
                .trim() || "-";

        setText(
            "rd-fullName",
            fullName
        );

        setText(
            "rd-email",
            doctor.email ?? "-"
        );

        setText(
            "rd-phone",
            doctor.phoneNumber ?? "-"
        );

        setText(
            "rd-specialty",
            doctor.specialty ?? "-"
        );

        setText(
            "rd-syndicateId",
            doctor.syndicateId ?? "-"
        );

        document
            .getElementById("rd-status")
            .innerHTML = `
                <span class="badge bg-${status.css}">
                    ${status.label}
                </span>
            `;

        document
            .getElementById("approveDoctorName")
            .textContent = fullName;
    }

    // ===============================================================
    // Documents
    // ===============================================================
    function renderDocumentInfo(
        doctorId,
        documents
    ) {

        if (documents.length === 0) {

            document
                .getElementById("rd-documentEmpty")
                .classList
                .remove("d-none");

            return;
        }

        // Get latest uploaded document
        const doc =
            [...documents]
                .sort(
                    (a, b) =>
                        new Date(b.uploadedAt) -
                        new Date(a.uploadedAt)
                )[0];

        console.log(
            "Selected document:",
            doc
        );

        setText(
            "rd-fileName",
            doc.fileName ?? "-"
        );

        setText(
            "rd-contentType",
            doc.contentType ?? "-"
        );

        setText(
            "rd-fileSize",
            formatFileSize(doc.fileSize)
        );

        setText(
            "rd-uploadedAt",
            formatDate(doc.uploadedAt)
        );

        const viewButton =
            document.getElementById(
                "rd-viewDocumentBtn"
            );

        /*
         * IMPORTANT:
         *
         * We DO NOT use doc.url.
         *
         * The request goes through MVC:
         *
         * MVC
         *   ↓
         * AdminApiClient
         *   ↓
         * API
         *
         * The MVC side attaches the Admin JWT.
         */

        viewButton.href =
            ENDPOINTS.download(
                doctorId,
                doc.id
            );

        viewButton.target = "_blank";

        viewButton.classList.remove("disabled");

        document
            .getElementById("rd-documentInfo")
            .classList
            .remove("d-none");
    }

    // ===============================================================
    // Reset Review Modal
    // ===============================================================
    function resetReviewModal() {

        hideRejectionSection();

        document
            .getElementById("reviewModalError")
            .classList
            .add("d-none");

        document
            .getElementById("rd-documentLoading")
            .classList
            .remove("d-none");

        document
            .getElementById("rd-documentEmpty")
            .classList
            .add("d-none");

        document
            .getElementById("rd-documentInfo")
            .classList
            .add("d-none");

        setText("rd-fileName", "-");
        setText("rd-contentType", "-");
        setText("rd-fileSize", "-");
        setText("rd-uploadedAt", "-");

        const viewButton =
            document.getElementById(
                "rd-viewDocumentBtn"
            );

        viewButton.href = "#";
    }

    // ===============================================================
    // APPROVE
    // ===============================================================
    function openApproveConfirm() {

        if (!activeDoctorId) {
            console.error(
                "No active doctor selected."
            );
            return;
        }

        // Never show a second Bootstrap modal while another one is still
        // open — stacking modals this way is what left the body stuck
        // with a dark backdrop and no scrolling once the top modal closed.
        // Hide the review modal first, and only open the confirm modal
        // once its hide transition has fully finished.
        reviewModalInstance.hide();

        document
            .getElementById("reviewDoctorModal")
            .addEventListener(
                "hidden.bs.modal",
                function onHidden() {
                    this.removeEventListener("hidden.bs.modal", onHidden);
                    approveModalInstance.show();
                }
            );
    }

    async function submitApproval() {

        if (!activeDoctorId) {
            return;
        }

        const button =
            document.getElementById(
                "btnConfirmApprove"
            );

        const spinner =
            document.getElementById(
                "approveSpinner"
            );

        button.disabled = true;

        spinner.classList.remove("d-none");

        try {

            console.log(
                "APPROVE doctor:",
                activeDoctorId
            );

            const response =
                await postVerifyDoctor(
                    activeDoctorId,
                    {
                        isApproved: true,
                        rejectionReason: null
                    }
                );

            console.log(
                "Approve response:",
                response.status
            );

            if (!response.ok) {

                const message =
                    await extractErrorMessage(
                        response,
                        "Failed to approve doctor."
                    );

                approveModalInstance.hide();

                showToast(
                    message,
                    false
                );

                return;
            }

            approveModalInstance.hide();

            reviewModalInstance.hide();

            removeDoctorFromList(
                activeDoctorId
            );

            showToast(
                "Doctor approved successfully.",
                true
            );

            activeDoctorId = null;

        } catch (error) {

            console.error(
                "submitApproval failed:",
                error
            );

            approveModalInstance.hide();

            showToast(
                "Could not reach the server.",
                false
            );

        } finally {

            button.disabled = false;

            spinner.classList.add("d-none");
        }
    }

    // ===============================================================
    // REJECT
    // ===============================================================
    function showRejectionSection() {

        document
            .getElementById("rejectionSection")
            .classList
            .remove("d-none");

        document
            .getElementById("reviewActionButtons")
            .classList
            .add("d-none");

        const buttons =
            document.getElementById(
                "rejectionActionButtons"
            );

        buttons.classList.remove("d-none");
        buttons.classList.add("d-flex");

        document
            .getElementById(
                "rejectionReasonInput"
            )
            .focus();
    }

    function hideRejectionSection() {

        document
            .getElementById("rejectionSection")
            .classList
            .add("d-none");

        document
            .getElementById("reviewActionButtons")
            .classList
            .remove("d-none");

        const buttons =
            document.getElementById(
                "rejectionActionButtons"
            );

        buttons.classList.add("d-none");
        buttons.classList.remove("d-flex");

        const textarea =
            document.getElementById(
                "rejectionReasonInput"
            );

        textarea.value = "";

        textarea.classList.remove(
            "is-invalid"
        );

        document
            .getElementById(
                "btnConfirmReject"
            )
            .disabled = true;
    }

    function validateRejectionReason() {

        const textarea =
            document.getElementById(
                "rejectionReasonInput"
            );

        const valid =
            textarea.value.trim().length > 0;

        textarea.classList.toggle(
            "is-invalid",
            !valid && textarea.value.length > 0
        );

        document
            .getElementById(
                "btnConfirmReject"
            )
            .disabled = !valid;

        return valid;
    }

    async function submitRejection() {

        if (!activeDoctorId) {
            return;
        }

        const textarea =
            document.getElementById(
                "rejectionReasonInput"
            );

        const reason =
            textarea.value.trim();

        if (!reason) {

            textarea.classList.add(
                "is-invalid"
            );

            return;
        }

        const button =
            document.getElementById(
                "btnConfirmReject"
            );

        const originalHtml =
            button.innerHTML;

        button.disabled = true;

        button.innerHTML = `
            <span class="spinner-border spinner-border-sm me-1"></span>
            Rejecting...
        `;

        try {

            console.log(
                "REJECT doctor:",
                activeDoctorId
            );

            const response =
                await postVerifyDoctor(
                    activeDoctorId,
                    {
                        isApproved: false,
                        rejectionReason: reason
                    }
                );

            console.log(
                "Reject response:",
                response.status
            );

            if (!response.ok) {

                const message =
                    await extractErrorMessage(
                        response,
                        "Failed to reject doctor."
                    );

                showToast(
                    message,
                    false
                );

                return;
            }

            reviewModalInstance.hide();

            removeDoctorFromList(
                activeDoctorId
            );

            showToast(
                "Doctor rejected successfully.",
                true
            );

            activeDoctorId = null;

        } catch (error) {

            console.error(
                "submitRejection failed:",
                error
            );

            showToast(
                "Could not reach the server.",
                false
            );

        } finally {

            button.disabled = false;

            button.innerHTML = originalHtml;
        }
    }

    // ===============================================================
    // VERIFY REQUEST
    // ===============================================================
    async function postVerifyDoctor(
        doctorId,
        payload
    ) {

        const token =
            document.querySelector(
                'input[name="__RequestVerificationToken"]'
            )?.value;

        const headers = {
            "Content-Type": "application/json",
            "Accept": "application/json"
        };

        /*
         * Your Program.cs currently uses:
         *
         * options.HeaderName = "X-CSRF-TOKEN";
         *
         * So we MUST use X-CSRF-TOKEN here.
         */
        if (token) {
            headers["X-CSRF-TOKEN"] = token;
        }

        console.log(
            "VERIFY URL:",
            ENDPOINTS.verify(doctorId)
        );

        console.log(
            "VERIFY PAYLOAD:",
            payload
        );

        return fetch(
            ENDPOINTS.verify(doctorId),
            {
                method: "POST",
                headers: headers,
                body: JSON.stringify(payload),
                credentials: "same-origin"
            }
        );
    }

    // ===============================================================
    // Remove doctor after approve/reject
    // ===============================================================
    function removeDoctorFromList(
        doctorId
    ) {

        doctorsById.delete(
            String(doctorId)
        );

        const row =
            document.getElementById(
                `doctor-row-${doctorId}`
            );

        if (row) {
            row.remove();
        }

        if (doctorsById.size === 0) {
            toggleSections("empty");
        }
    }

    // ===============================================================
    // UI
    // ===============================================================
    function showLoading() {

        hidePageError();

        toggleSections("loading");
    }

    function toggleSections(active) {

        const sections = {
            loading:
                document.getElementById(
                    "loadingState"
                ),

            empty:
                document.getElementById(
                    "emptyState"
                ),

            table:
                document.getElementById(
                    "tableWrapper"
                )
        };

        Object.entries(sections)
            .forEach(([key, element]) => {

                element.classList.toggle(
                    "d-none",
                    key !== active
                );
            });
    }

    function showPageError(message) {

        const alert =
            document.getElementById(
                "pageErrorAlert"
            );

        document.getElementById(
            "pageErrorAlertText"
        ).textContent = message;

        alert.classList.remove(
            "d-none"
        );

        toggleSections("empty");

        document
            .getElementById("emptyState")
            .classList
            .add("d-none");
    }

    function hidePageError() {

        document
            .getElementById("pageErrorAlert")
            .classList
            .add("d-none");
    }

    function showReviewModalError(
        message
    ) {

        const element =
            document.getElementById(
                "reviewModalError"
            );

        element.textContent = message;

        element.classList.remove(
            "d-none"
        );
    }

    function showToast(
        message,
        success
    ) {

        const toast =
            document.getElementById(
                "actionToast"
            );

        toast.classList.remove(
            "text-bg-success",
            "text-bg-danger"
        );

        toast.classList.add(
            success
                ? "text-bg-success"
                : "text-bg-danger"
        );

        document.getElementById(
            "actionToastBody"
        ).textContent = message;

        toastInstance.show();
    }

    // ===============================================================
    // Helpers
    // ===============================================================
    function setText(
        id,
        value
    ) {

        document.getElementById(
            id
        ).textContent = value;
    }

    async function extractErrorMessage(
        response,
        fallback
    ) {

        try {

            const body =
                await response.json();

            return body?.message ||
                body?.title ||
                fallback;

        } catch {

            return fallback;
        }
    }

    function formatFileSize(
        bytes
    ) {

        if (bytes == null) {
            return "-";
        }

        if (bytes < 1024) {
            return `${bytes} B`;
        }

        if (bytes < 1024 * 1024) {
            return `${(
                bytes / 1024
            ).toFixed(1)} KB`;
        }

        return `${(
            bytes /
            (1024 * 1024)
        ).toFixed(1)} MB`;
    }

    function formatDate(
        value
    ) {

        if (!value) {
            return "-";
        }

        const date =
            new Date(value);

        if (
            isNaN(
                date.getTime()
            )
        ) {
            return "-";
        }

        return date.toLocaleString(
            undefined,
            {
                year: "numeric",
                month: "short",
                day: "numeric",
                hour: "2-digit",
                minute: "2-digit"
            }
        );
    }

    function escapeHtml(
        value
    ) {

        return String(value)
            .replace(
                /&/g,
                "&amp;"
            )
            .replace(
                /</g,
                "&lt;"
            )
            .replace(
                />/g,
                "&gt;"
            )
            .replace(
                /"/g,
                "&quot;"
            )
            .replace(
                /'/g,
                "&#39;"
            );
    }

})();
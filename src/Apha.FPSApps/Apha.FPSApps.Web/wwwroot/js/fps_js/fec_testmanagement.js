// fec_testmanagement.js
// FEC Bulk Rates Update — Phase 4 MVC/UI layer.

/* jshint esversion: 6 */
/* global $, showLoader, hideLoader, showAlertMessage, showGovukConfirm, AlertType */

var BulkRates = (function () {
    'use strict';

    // ── Helpers ────────────────────────────────────────────────────────────

    function getAntiForgeryToken() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function showActionError(msg) {
        var banner = document.getElementById('actionErrorBanner');
        var text   = document.getElementById('actionErrorText');
        if (banner && text) {
            text.textContent = msg;
            banner.style.display = '';
            banner.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        } else {
            alert(msg);
        }
    }

    function hideActionError() {
        var banner = document.getElementById('actionErrorBanner');
        if (banner) { banner.style.display = 'none'; }
    }

    function ajaxPost(url, data, successCallback, errorCallback) {
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            headers: { 'RequestVerificationToken': getAntiForgeryToken() },
            success: function (result) {
                if (result && result.success) {
                    successCallback(result);
                } else {
                    var msg = (result && result.message) ? result.message : 'An unexpected error occurred.';
                    if (errorCallback) {
                        errorCallback(msg);
                    } else {
                        showActionError(msg);
                    }
                }
            },
            error: function (xhr) {
                var msg = 'An unexpected error occurred. Please try again.';
                if (xhr.responseJSON && xhr.responseJSON.message) { msg = xhr.responseJSON.message; }
                if (errorCallback) {
                    errorCallback(msg);
                } else {
                    showActionError(msg);
                }
            }
        });
    }

    // ── Create Request ──────────────────────────────────────────────────────

    function submitCreate() {
        var jobName = document.getElementById('jobName');
        var fpsYear = document.getElementById('fpsYear');
        var errorSummary = document.getElementById('createErrorSummary');
        var errorText    = document.getElementById('createErrorText');

        if (!jobName || !fpsYear) { return; }

        var yearVal = parseInt(fpsYear.value, 10);
        if (!yearVal || yearVal < 2000 || yearVal > 2100) {
            if (errorText)  { errorText.textContent  = 'Enter a valid FPS year (2000–2100).'; }
            if (errorSummary) { errorSummary.style.display = ''; }
            return;
        }
        if (errorSummary) { errorSummary.style.display = 'none'; }

        var btn = document.getElementById('btnCreateRequest');
        if (btn) { btn.disabled = true; }

        ajaxPost(
            '/FPS/BulkRates/Create',
            { jobName: jobName.value, fpsYear: yearVal },
            function (result) {
                window.fpsNavigateTo('/FPS/BulkRates/Detail/' + result.id);
            },
            function (msg) {
                if (errorText)    { errorText.textContent    = msg; }
                if (errorSummary) { errorSummary.style.display = ''; }
                if (btn) { btn.disabled = false; }
            }
        );
    }

    // ── Download Test Data (Excel) ───────────────────────────────────────────

    function downloadTestData() {
        var btn = document.getElementById('btnDownloadTestData');
        var jobName = btn ? btn.getAttribute('data-job-name') : null;
        var yearSelector = document.getElementById('yearSelector');
        var fpsYear = (yearSelector && yearSelector.value) ? parseInt(yearSelector.value, 10) : new Date().getFullYear();
        if (!fpsYear || fpsYear <= 0) {
            alert('Please select a year before downloading.');
            return;
        }
        var endpoint = jobName === 'BulkStaffRatesUpdate' ? '/FPS/BulkRates/DownloadStaffTestData'
            : jobName === 'BulkAnimalRatesUpdate' ? '/FPS/BulkRates/DownloadAnimalTestData'
            : '/FPS/BulkRates/DownloadTestData';
        window.location.href = endpoint + '?fpsYear=' + fpsYear;
    }

    // ── Upload Excel file ───────────────────────────────────────────────────
    function uploadFile(requestId) {
        var fileInput = document.getElementById('ratesFile');
        if (!fileInput || !fileInput.files || fileInput.files.length === 0) {
            showActionError('Please select a file before uploading.');
            return;
        }

        var file = fileInput.files[0];
        var formData = new FormData();
        formData.append('id', requestId);
        formData.append('file', file);

        var btn      = document.getElementById('btnUpload');
        var progress = document.getElementById('uploadProgress');
        if (btn)      { btn.disabled = true; }
        if (progress) { progress.style.display = ''; }

        $.ajax({
            url: '/FPS/BulkRates/Upload',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            headers: { 'RequestVerificationToken': getAntiForgeryToken() },
            success: function (result) {
                if (btn)      { btn.disabled = false; }
                if (progress) { progress.style.display = 'none'; }
                if (result && result.success) {
                    window.location.reload();
                } else {
                    showActionError((result && result.message) ? result.message : 'Upload failed.');
                }
            },
            error: function (xhr) {
                if (btn)      { btn.disabled = false; }
                if (progress) { progress.style.display = 'none'; }
                var msg = 'Upload failed. Please try again.';
                if (xhr.responseJSON && xhr.responseJSON.message) { msg = xhr.responseJSON.message; }
                showActionError(msg);
            }
        });
    }

    // ── FEC Data (Staging) — Download Staging Data ──────────────────────────

    function downloadStagingData(requestId) {
        window.location.href = '/FPS/BulkRates/DownloadStagingData/' + requestId;
    }

    // ── Bulk Rates queue grid ───────────────────────────────────────────────
    // getBulkRatesExtraFilters / viewBulkRatesRequest are looked up by _DataGrid.cshtml's
    // JS via window[functionName] (ExtraFilterMethod / ViewFunction) — they must be true
    // globals, not members of the BulkRates module object like the rest of this file.

    function getBulkRatesExtraFilters() {
        var jobNameEl = document.getElementById('jobNameFilter');
        var statusEl  = document.getElementById('statusFilter');
        var yearEl    = document.getElementById('yearSelector');
        return {
            jobName: jobNameEl ? jobNameEl.value : '',
            status:  statusEl  ? statusEl.value  : '',
            // Every grid reload (sort/filter/page/background poll) is a plain AJAX POST with
            // no query string, so FpsYearMiddleware.ResolveYear has nothing to go on but this
            // form field — without it, it silently falls back to whichever year is "Open",
            // which can differ from the year actually selected/displayed on the page.
            FPSYear: yearEl ? yearEl.value : ''
        };
    }
    window.getBulkRatesExtraFilters = getBulkRatesExtraFilters;

    function viewBulkRatesRequest(btn) {
        var id = $(btn).data('id');
        window.fpsNavigateTo('/FPS/BulkRates/Detail/' + id);
    }
    window.viewBulkRatesRequest = viewBulkRatesRequest;

    function filterGrid() {
        var gm = window['gridManager_bulkRatesGrid'];
        if (gm) { gm.reloadGrid({ page: 1 }); }
    }

    // ── Transient-status grid polling ────────────────────────────────────────
    // After every grid render — the initial server-rendered page, or any
    // reload (sort/filter/page/this poll's own reload) — check whether any
    // *visible* row is still in a transient state: "Submitted" (the display
    // label for the underlying Approved status — the worker hasn't picked it
    // up yet) or "Running". If so, reload again in POLL_INTERVAL_MS to pick up
    // the next status change; if not, do nothing. Entirely driven by what's
    // actually on the page rather than a separate server-computed flag, so it
    // starts and stops correctly regardless of how the grid was reached (the
    // Approve redirect, a manual refresh, revisiting later while still in
    // flight) and never runs at all when nothing on the page needs watching.
    //
    // Visible-rows-only is a real scope limit, not just an implementation detail:
    // the reload below always requests page 1 (matching every other reload in
    // this file), so a transient row sitting on a different page than the one
    // currently shown won't be seen and won't keep the poll going. Same applies
    // to an active status/job-type filter that hides the transient row entirely.
    // Fine for the common case (Approve redirects straight back to page 1,
    // unfiltered), but worth knowing if this ever needs to track a transient
    // row regardless of the viewer's current page/filter.
    //
    // Detection matches against the *display* label ("Submitted"/"Running"),
    // not the raw status ("Approved"/"Running") — the raw value never reaches
    // this grid's HTML (FpsViewModelMapper maps Status through
    // BulkRatesStatusDisplay.FriendlyLabel before the view model is built), so
    // matching display text is what's actually available here without adding
    // a new data attribute solely for this.
    var TRANSIENT_STATUS_LABELS = ['Submitted', 'Running'];
    var POLL_INTERVAL_MS = 10000;
    var _transientPollTimer = null;

    function hasTransientStatusRow() {
        var spans = document.querySelectorAll('#gridContainer_bulkRatesGrid [data-property="Status"] span');
        for (var i = 0; i < spans.length; i++) {
            if (TRANSIENT_STATUS_LABELS.indexOf(spans[i].textContent.trim()) !== -1) {
                return true;
            }
        }
        return false;
    }

    function checkAndScheduleTransientPoll() {
        if (_transientPollTimer) { return; } // one timer at a time — the pending poll will re-check when it fires
        if (!hasTransientStatusRow()) { return; }
        _transientPollTimer = setTimeout(function () {
            _transientPollTimer = null;
            var gm = window['gridManager_bulkRatesGrid'];
            if (gm) { gm.reloadGrid({ page: 1 }); }
        }, POLL_INTERVAL_MS);
    }

    document.addEventListener('gridReloaded', function (e) {
        if (!e.detail || e.detail.gridId !== 'bulkRatesGrid') { return; }
        checkAndScheduleTransientPoll();
    });

    // ── Release for Approval modal ──────────────────────────────────────────
    // Uses the shared govuk-modal-dialog.js showGovukConfirm() dialog directly
    // — no reason input needed, unlike Reject/Cancel below, which use their own
    // static partials (_RejectModal.cshtml/_CancelModal.cshtml) styled to look
    // identical (same isInfo "Please confirm" colour band) since they need a
    // reason textarea the shared dialog doesn't support.

    function showReleaseModal(requestId) {
        showGovukConfirm('Release this request for approval? This action cannot be undone.')
            .then(function (confirmed) {
                if (!confirmed) { return; }
                hideActionError();
                var btn = document.getElementById('btnRelease');
                if (btn) { btn.disabled = true; }
                ajaxPost(
                    '/FPS/BulkRates/Release',
                    { id: requestId },
                    function () { window.location.reload(); },
                    function (msg) {
                        showActionError(msg);
                        if (btn) { btn.disabled = false; }
                    }
                );
            });
    }

    // ── Approve modal ───────────────────────────────────────────────────────

    function showApproveModal(requestId) {
        showGovukConfirm('Approve this request? The changes will be processed and applied.')
            .then(function (confirmed) {
                if (!confirmed) { return; }
                hideActionError();
                var btn = document.getElementById('btnApprove');
                if (btn) { btn.disabled = true; }
                var returnUrl = btn ? (btn.getAttribute('data-return-url') || '/FPS/BulkRates') : '/FPS/BulkRates';
                ajaxPost(
                    '/FPS/BulkRates/Approve',
                    { id: requestId },
                    function () { window.fpsNavigateTo(returnUrl); },
                    function (msg) {
                        showActionError(msg);
                        if (btn) { btn.disabled = false; }
                    }
                );
            });
    }

    // ── Reject modal ─────────────────────────────────────────────────────────

    var _pendingRejectId = null;
    var _pendingRejectReturnUrl = '/FPS/BulkRates';

    function showRejectModal(requestId) {
        _pendingRejectId = requestId;
        var triggerBtn = document.getElementById('btnReject');
        _pendingRejectReturnUrl = triggerBtn ? (triggerBtn.getAttribute('data-return-url') || '/FPS/BulkRates') : '/FPS/BulkRates';
        var overlay = document.getElementById('rejectModalOverlay');
        var reason  = document.getElementById('rejectReason');
        var errEl   = document.getElementById('rejectReasonError');
        var group   = document.getElementById('rejectReasonGroup');
        if (!overlay) { return; }
        if (reason)  { reason.value = ''; }
        if (errEl)   { errEl.style.display = 'none'; }
        if (group)   { group.classList.remove('govuk-form-group--error'); }
        overlay.style.display = 'flex';
        if (reason)  { reason.focus(); }
    }

    function closeRejectModal() {
        var overlay = document.getElementById('rejectModalOverlay');
        if (overlay) { overlay.style.display = 'none'; }
        _pendingRejectId = null;
    }

    function confirmReject() {
        var reason  = document.getElementById('rejectReason');
        var errEl   = document.getElementById('rejectReasonError');
        var group   = document.getElementById('rejectReasonGroup');
        var reasonVal = reason ? reason.value.trim() : '';

        if (!reasonVal) {
            if (errEl)  { errEl.style.display = ''; }
            if (group)  { group.classList.add('govuk-form-group--error'); }
            if (reason) { reason.focus(); }
            return;
        }
        if (errEl)  { errEl.style.display = 'none'; }
        if (group)  { group.classList.remove('govuk-form-group--error'); }

        var btn = document.getElementById('btnConfirmReject');
        if (btn) { btn.disabled = true; }

        ajaxPost(
            '/FPS/BulkRates/Reject',
            { id: _pendingRejectId, reason: reasonVal },
            function () {
                var returnUrl = _pendingRejectReturnUrl;
                closeRejectModal();
                window.fpsNavigateTo(returnUrl);
            },
            function (msg) {
                if (btn) { btn.disabled = false; }
                alert(msg);
            }
        );
    }

    // ── Cancel modal ─────────────────────────────────────────────────────────

    var _pendingCancelId = null;
    var _pendingCancelReturnUrl = '/FPS/BulkRates';

    function showCancelModal(requestId) {
        _pendingCancelId = requestId;
        var triggerBtn = document.getElementById('btnCancel');
        _pendingCancelReturnUrl = triggerBtn ? (triggerBtn.getAttribute('data-return-url') || '/FPS/BulkRates') : '/FPS/BulkRates';
        var overlay = document.getElementById('cancelModalOverlay');
        var reason  = document.getElementById('cancelReason');
        if (!overlay) { return; }
        if (reason)  { reason.value = ''; }
        overlay.style.display = 'flex';
        if (reason)  { reason.focus(); }
    }

    function closeCancelModal() {
        var overlay = document.getElementById('cancelModalOverlay');
        if (overlay) { overlay.style.display = 'none'; }
        _pendingCancelId = null;
    }

    function confirmCancel() {
        var reason    = document.getElementById('cancelReason');
        var reasonVal = reason ? reason.value.trim() : '';

        var btn = document.getElementById('btnConfirmCancel');
        if (btn) { btn.disabled = true; }

        ajaxPost(
            '/FPS/BulkRates/Cancel',
            { id: _pendingCancelId, reason: reasonVal },
            function () {
                var returnUrl = _pendingCancelReturnUrl;
                closeCancelModal();
                window.fpsNavigateTo(returnUrl);
            },
            function (msg) {
                if (btn) { btn.disabled = false; }
                alert(msg);
            }
        );
    }

    // ── Keyboard: close modals on Escape ─────────────────────────────────────
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeRejectModal();
            closeCancelModal();
        }
    });

    // ── Button bindings ─────────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', function () {
        var btnDownload = document.getElementById('btnDownloadTestData');
        if (btnDownload) {
            btnDownload.addEventListener('click', downloadTestData);
        }
        // Bootstrap the transient-status poll for the initial server-rendered grid —
        // 'gridReloaded' only fires from reloadGrid()'s own AJAX callback, never for
        // the first page load. No-ops harmlessly on pages without the grid (e.g. Detail).
        checkAndScheduleTransientPoll();
    });

    // ── Public API ──────────────────────────────────────────────────────────
    return {
        submitCreate:           submitCreate,
        uploadFile:             uploadFile,
        showReleaseModal:       showReleaseModal,
        showApproveModal:       showApproveModal,
        showRejectModal:        showRejectModal,
        closeRejectModal:       closeRejectModal,
        confirmReject:          confirmReject,
        showCancelModal:        showCancelModal,
        closeCancelModal:       closeCancelModal,
        confirmCancel:          confirmCancel,
        filterGrid:             filterGrid,
        downloadTestData:       downloadTestData,
        downloadStagingData:    downloadStagingData
    };
}());


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

    // ── Active-request grid polling ─────────────────────────────────────────
    // Two different questions, answered independently on the same timer:
    //   - "What is the worker actually doing?" — the grid reload every tick,
    //     unchanged, giving the Status column its live Approved/Running/
    //     Completed/Failed updates. Never depended on a specific row.
    //   - "Can another request be created now?" — CanInitiateRequest, checked
    //     only after each reload's 'gridReloaded' event fires (so the page has
    //     already shown the latest status before polling can stop on it), used
    //     purely to decide when to swap the banner/button.
    // No hard cutoff: fast (3s) for the first ~40 ticks, then slow (20s)
    // indefinitely — a fixed cap would silently freeze the page on "Running"
    // for any request that legitimately runs long. The only two ways this
    // ends are CanInitiateRequest genuinely becoming true, or the tab closing.
    var _pollTimer = null;
    var _pollJobName = null;
    var _pollTickCount = 0;
    var POLL_FAST_INTERVAL_MS = 3000;
    var POLL_SLOW_INTERVAL_MS = 20000;
    var POLL_FAST_PHASE_TICKS = 40;

    function scheduleNextPoll() {
        var delay = _pollTickCount < POLL_FAST_PHASE_TICKS ? POLL_FAST_INTERVAL_MS : POLL_SLOW_INTERVAL_MS;
        _pollTimer = setTimeout(function () {
            _pollTickCount++;
            var gm = window['gridManager_bulkRatesGrid'];
            // silent: true — background poll, not a user action; skip the full-page loader
            // flash so only the Status badge visibly changes between ticks.
            if (gm) { gm.reloadGrid({ page: 1 }, { silent: true }); }
            else { scheduleNextPoll(); } // grid manager not ready yet — retry on schedule
        }, delay);
    }

    function onPollGridReloaded(e) {
        if (!_pollTimer || !_pollJobName) { return; } // polling already stopped — ignore a stray/late event
        if (!e.detail || e.detail.gridId !== 'bulkRatesGrid') { return; }

        // Capture before any stop() call, which clears the shared _pollJobName state as part of
        // fully tearing polling down — building the "New Request" link from it afterwards would
        // put a literal "null" in the URL.
        var pollingJobName = _pollJobName;

        $.ajax({
            url: '/FPS/BulkRates/CanInitiateRequest',
            type: 'GET',
            data: { jobName: pollingJobName },
            success: function (result) {
                if (!_pollTimer) { return; } // stopped while this request was in flight
                if (!result || !result.success) { scheduleNextPoll(); return; } // unknown state — retry, don't guess
                if (result.canInitiate) {
                    stopActiveRequestPolling();
                    $('#activeRequestBanner').remove();
                    var $btnArea = $('#newRequestButtonArea');
                    if ($btnArea.length) {
                        $btnArea.html('<a href="/FPS/BulkRates/Create?jobName=' + encodeURIComponent(pollingJobName) +
                            '" class="govuk-button govuk-button--secondary sup_margin_0">New Request</a>');
                    }
                    return;
                }
                scheduleNextPoll();
            },
            error: function () { scheduleNextPoll(); /* request failed — retry, don't guess */ }
        });
    }

    function stopActiveRequestPolling() {
        if (_pollTimer) { clearTimeout(_pollTimer); _pollTimer = null; }
        _pollJobName = null;
        document.removeEventListener('gridReloaded', onPollGridReloaded);
    }

    function startActiveRequestPolling(jobName) {
        if (_pollTimer) { return; }
        _pollJobName = jobName;
        _pollTickCount = 0;
        document.addEventListener('gridReloaded', onPollGridReloaded);
        scheduleNextPoll();
    }

    // ── Release for Approval modal ──────────────────────────────────────────
    // Uses the same custom-overlay pattern as the Cancel/Reject modals below,
    // rather than the generic showGovukConfirm() dialog, so the four action
    // modals on this page look and behave consistently with each other.

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
        startActiveRequestPolling: startActiveRequestPolling,
        downloadTestData:       downloadTestData,
        downloadStagingData:    downloadStagingData
    };
}());


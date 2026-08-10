/* year-end-cutover.js */

(function ($) {
    'use strict';

    var cfg = window.YearEndCutOverConfig || {};

    // ── Grid reload helpers ───────────────────────────────────────────────────

    function reloadHistoryGrid() {
        var gm = window['gridManager_yearEndCutOverHistoryGrid'];
        if (gm && typeof gm.reloadGrid === 'function') { gm.reloadGrid({ page: 1 }); }
    }

    // ── Generic helpers ───────────────────────────────────────────────────────

    function getAntiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function postJson(url, data, onSuccess, onError) {
        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: { 'RequestVerificationToken': getAntiForgeryToken() },
            success: function (result) {
                if (result.success) {
                    onSuccess(result);
                } else {
                    var msgs = (result.errors || []).map(function (e) { return e.message || e; });
                    onError(msgs.length ? msgs : ['Request failed.']);
                }
            },
            error: function () { onError(['A network error occurred. Please try again.']); }
        });
    }

    function showPageError(messages) {
        var summary = document.querySelector('.govuk-error-summary[aria-labelledby="yec-error-summary-title"]');
        var list    = document.getElementById('yec-error-list');
        if (summary && list) {
            list.innerHTML = '';
            (messages || ['An unexpected error occurred.']).forEach(function (m) {
                var li = document.createElement('li');
                li.textContent = m;
                list.appendChild(li);
            });
            summary.style.display = '';
        }
    }

    function hidePageError() {
        var summary = document.querySelector('.govuk-error-summary[aria-labelledby="yec-error-summary-title"]');
        if (summary) summary.style.display = 'none';
    }

    // ── Initiate CutOver Request button ───────────────────────────────────────
    $(function () {
        var btnInitiate = document.getElementById('btnInitiateCutOverRequest');
        if (btnInitiate) {
            btnInitiate.addEventListener('click', function () {
                hidePageError();

                var plannedYearVal = parseInt(document.getElementById('yearEndProcessYear').value, 10);

                if (!plannedYearVal || isNaN(plannedYearVal)) {
                    showPageError(['Please provide planned year.']);
                    return;
                }

                showGovukConfirm('Are you sure you want to initiate the CutOver Request for year ' + plannedYearVal + '?')
                    .then(function (confirmed) {
                        if (!confirmed) return;

                        btnInitiate.disabled = true;
                        postJson(cfg.triggerInitiateUrl + '?plannedYear=' + plannedYearVal, {},
                            function () {
                                showAlertMessage('Year End CutOver initiation request submitted successfully.', AlertType.SUCCESS);
                                reloadHistoryGrid();
                                var btnApprove = document.getElementById('btnApproveCutOverRequest');
                                if (btnApprove) { btnApprove.disabled = false; }
                            },
                            function (msgs) {
                                showPageError(msgs);
                                btnInitiate.disabled = false;
                            }
                        );
                    });
            });
        }

        // ── Approve CutOver Request button ────────────────────────────────────
        var btnApprove = document.getElementById('btnApproveCutOverRequest');
        if (btnApprove) {
            btnApprove.addEventListener('click', function () {
                hidePageError();

                var plannedYearVal = parseInt(document.getElementById('yearEndProcessYear').value, 10);

                if (!plannedYearVal || isNaN(plannedYearVal)) {
                    showPageError(['Please provide planned year.']);
                    return;
                }

                showGovukApproveReject('Are you sure you want to approve the CutOver Request for year ' + plannedYearVal + '?')
                    .then(function (confirmed) {
                        if (confirmed) {
                        btnApprove.disabled = true;
                        postJson(cfg.triggerApproveUrl + '?plannedYear=' + plannedYearVal, {},
                            function () {
                                showAlertMessage('Year End CutOver approval request submitted successfully.', AlertType.SUCCESS);
                                reloadHistoryGrid();
                            },
                            function (msgs) {
                                showPageError(msgs);
                                btnApprove.disabled = false;
                            }
                        );
                        } else {
                            btnApprove.disabled = true;
                            postJson(cfg.triggerRejectUrl + '?plannedYear=' + plannedYearVal, {},
                                function () {
                                    showAlertMessage('Year End CutOver request rejected successfully.', AlertType.SUCCESS);
                                    reloadHistoryGrid();
                                    btnInitiate.disabled = false;
                                },
                                function (msgs) {
                                    showPageError(msgs);
                                    btnApprove.disabled = false;
                                }
                            );
                        }

                    });
            });
        }
    });

}(jQuery));

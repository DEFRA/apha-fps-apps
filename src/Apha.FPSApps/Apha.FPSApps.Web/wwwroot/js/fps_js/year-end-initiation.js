/**
 * year-end-initiation.js
 *
 * Client-side logic for the Year End Initiation page.
 *
 * Edit pattern (matches PACT TestOrProduct):
 *   1. Edit button click  → $.get(url, params) → server returns partial HTML
 *   2. Partial HTML loads into #modaPopupBody  → #modalPopup shown
 *   3. Partial's Save button calls saveConfigValue() / saveMonthHour()
 *   4. Partial's Cancel/X button calls closeYeiModal()
 *   5. On success: close modal, reload grid via gridManager, show alert
 *
 * Confirm (delete) button:
 *   → showGovukConfirm() → on YES → POST save directly → reload grid
 *
 * URLs are set on window.YearEndInitiationConfig by Index.cshtml @section Scripts.
 */

(function ($) {
    'use strict';

    var cfg = window.YearEndInitiationConfig || {};

    // ── Modal open / close ────────────────────────────────────────────────────

    function openModalWithHtml(html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    }

    // Called by Cancel/X buttons inside the loaded partials
    window.closeYeiModal = function () {
        $('#modalPopup').removeClass('show');
        $('#modaPopupBody').html('');
    };

    // ── Grid reload helpers ───────────────────────────────────────────────────

    function reloadConfigGrid() {
        var gm = window['gridManager_yearEndConfigValuesGrid'];
        if (gm && typeof gm.reloadGrid === 'function') { gm.reloadGrid({ page: 1 }); }
    }

    function reloadMonthGrid() {
        var gm = window['gridManager_yearEndMonthHoursGrid'];
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

    function getCellValue(row, propertyName) {
        var cell = row.querySelector('td[data-property="' + propertyName + '"]');
        if (!cell) return '';
        var span = cell.querySelector('span[name="' + propertyName + '"]');
        return span ? span.textContent.trim() : cell.textContent.trim();
    }

    function showPageError(messages) {
        var summary = document.querySelector('.govuk-error-summary[aria-labelledby="yei-error-summary-title"]');
        var list    = document.getElementById('yei-error-list');
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
        var summary = document.querySelector('.govuk-error-summary[aria-labelledby="yei-error-summary-title"]');
        if (summary) summary.style.display = 'none';
    }

    function showModalError(messages) {
        var summary = document.querySelector('#modaPopupBody .govuk-error-summary');
        var list    = document.querySelector('#modaPopupBody .govuk-error-summary__list');
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

    // ── Config Value grid — Edit button ───────────────────────────────────────
    // Loads _EditConfigValue partial for the row's Id via $.get → shows in #modalPopup

    window.openConfigEditModal = function (btn) {
        hidePageError();

        var row         = $(btn).closest('tr')[0];
        var id          = getCellValue(row, 'Id');
        var fpsYearType = getCellValue(row, 'FpsYearType');

        // if (fpsYearType && fpsYearType !== 'Planned') {
        //     showAlertMessage('Only Planned year values can be edited.', AlertType.WARNING);
        //     return;
        // }

        $.get(cfg.editConfigValueUrl, { id: id })
            .done(function (html) {
                openModalWithHtml(html);
            })
            .fail(function () {
                showAlertMessage('Failed to load the edit form.', AlertType.ERROR);
            });
    };

    // ── Config Value grid — Delete/Confirm button ─────────────────────────────
    // Ask for confirmation then POST-save the current displayed row value directly

    window.confirmConfigValue = function (btn) {
        hidePageError();

        var row         = $(btn).closest('tr')[0];
        var id          = getCellValue(row, 'Id');
        var label       = getCellValue(row, 'Label');
        var value       = getCellValue(row, 'Value') || '';
        var fpsYearType = getCellValue(row, 'FpsYearType');

        // if (fpsYearType && fpsYearType !== 'Planned') {
        //     showAlertMessage('Only Planned year values can be confirmed.', AlertType.WARNING);
        //     return;
        // }

        showGovukConfirm('Are you sure you want to confirm the config value for "' + label + '"?')
            .then(function (confirmed) {
                if (!confirmed) return;
                postJson(cfg.saveSettingUrl, { id: id, setting: label, notes: value },
                    function () {
                        showAlertMessage('Config value "' + label + '" confirmed successfully.', AlertType.SUCCESS);
                        reloadConfigGrid();
                    },
                    function (msgs) { showPageError(msgs); }
                );
            });
    };

    // Called by the Save button inside _EditConfigValue.cshtml partial
    window.saveConfigValue = function () {
        var id    = $('#modaPopupBody #configModalId').val();
        var label = $('#modaPopupBody #configModalLabel').val();

        // Server renders either a <select id="configModalSelect"> or <input id="configModalInput">
        var $select = $('#modaPopupBody #configModalSelect');
        var $input  = $('#modaPopupBody #configModalInput');
        var value   = $select.length ? $select.val() : $input.val();

        postJson(cfg.saveSettingUrl, { id: id, setting: label, notes: value },
            function () {
                window.closeYeiModal();
                showAlertMessage('Config value "' + label + '" saved successfully.', AlertType.SUCCESS);
                reloadConfigGrid();
            },
            function (msgs) { showModalError(msgs); }
        );
    };

    // ── Month Working Hours grid — Edit button ────────────────────────────────
    // Loads _EditMonthHour partial for the row's Year+Month via $.get → shows in #modalPopup

    window.openMonthHourEditModal = function (btn) {
        hidePageError();

        var row         = $(btn).closest('tr')[0];
        var fpsYearType = getCellValue(row, 'FpsYearType');
        var year        = getCellValue(row, 'Year');
        var month       = getCellValue(row, 'Month');

        // if (fpsYearType && fpsYearType !== 'Planned') {
        //     showAlertMessage('Only Planned year month hours can be edited.', AlertType.WARNING);
        //     return;
        // }

        $.get(cfg.editMonthHourUrl, { year: year, month: month })
            .done(function (html) {
                openModalWithHtml(html);
            })
            .fail(function () {
                showAlertMessage('Failed to load the edit form.', AlertType.ERROR);
            });
    };

    // ── Month Working Hours grid — Delete/Confirm button ──────────────────────
    // Ask for confirmation then POST-save the current displayed row values directly

    window.confirmMonthHour = function (btn) {
        hidePageError();

        var row         = $(btn).closest('tr')[0];
        var fpsYearType = getCellValue(row, 'FpsYearType');
        var monthName   = getCellValue(row, 'MonthName');

        if (fpsYearType && fpsYearType !== 'Planned') {
            showAlertMessage('Only Planned year month hours can be confirmed.', AlertType.WARNING);
            return;
        }

        showGovukConfirm('Are you sure you want to confirm the working hours for ' + monthName + '?')
            .then(function (confirmed) {
                if (!confirmed) return;
                var dto = {
                    year:     parseInt(getCellValue(row, 'Year'), 10),
                    month:    parseInt(getCellValue(row, 'Month'), 10),
                    days:     parseFloat(getCellValue(row, 'Days'))    || null,
                    cvlHours: parseFloat(getCellValue(row, 'CvlHours')) || null,
                    vidHours: parseFloat(getCellValue(row, 'VidHours')) || null,
                    fmonth:   parseInt(getCellValue(row, 'Fmonth'), 10) || null,
                    fpsYear:  parseInt(getCellValue(row, 'FpsYear'), 10)
                };
                postJson(cfg.saveMonthHourUrl, dto,
                    function () {
                        showAlertMessage('Working hours for ' + monthName + ' confirmed successfully.', AlertType.SUCCESS);
                        reloadMonthGrid();
                    },
                    function (msgs) { showPageError(msgs); }
                );
            });
    };

    // Called by the Save button inside _EditMonthHour.cshtml partial
    window.saveMonthHour = function () {
        var dto = {
            year:     parseInt($('#modaPopupBody #monthModalYear').val(),    10),
            month:    parseInt($('#modaPopupBody #monthModalMonth').val(),   10),
            days:     parseFloat($('#modaPopupBody #monthModalDays').val())     || null,
            cvlHours: parseFloat($('#modaPopupBody #monthModalCvlHours').val()) || null,
            vidHours: parseFloat($('#modaPopupBody #monthModalVidHours').val()) || null,
            fmonth:   parseInt($('#modaPopupBody #monthModalFmonth').val(),  10) || null,
            fpsYear:  parseInt($('#modaPopupBody #monthModalFpsYear').val(), 10)
        };

        postJson(cfg.saveMonthHourUrl, dto,
            function () {
                window.closeYeiModal();
                showAlertMessage('Month working hours saved successfully.', AlertType.SUCCESS);
                reloadMonthGrid();
            },
            function (msgs) { showModalError(msgs); }
        );
    };

    // ── Initiate DataSetup Request button ─────────────────────────────────────

    $(function () {
        var btnInitiate = document.getElementById('btnInitiateDataSetupRequest');
        if (btnInitiate) {
            btnInitiate.addEventListener('click', function () {
                hidePageError();
                btnInitiate.disabled = true;
                postJson(cfg.triggerInitiateUrl, {},
                    function () {
                        showAlertMessage('Year End Initiation request submitted successfully.', AlertType.SUCCESS);
                    },
                    function (msgs) {
                        showPageError(msgs);
                        btnInitiate.disabled = false;
                    }
                );
            });
        }
    });

}(jQuery));

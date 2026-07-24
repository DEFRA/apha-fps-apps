/**
 * year-end-initiation.js
 *
 * Handles all client-side behaviour for the Year End Initiation page:
 *   - Config Value edit modal (open, save via AJAX)
 *   - Month Working Hours edit modal (open, save via AJAX)
 *   - Initiate DataSetup Request button (AJAX trigger)
 *   - Success / error banner display
 *
 * Configuration bridge (set inline by the Razor view before this script loads):
 *   window.YearEndInitiationConfig = { saveSettingUrl, saveMonthHourUrl, triggerInitiateUrl }
 */

(function () {
    'use strict';

    var cfg = window.YearEndInitiationConfig || {};

    // ── Banner helpers ────────────────────────────────────────────────────────

    function showSuccess(message) {
        var banner = document.getElementById('yei-success-banner');
        var msg    = document.getElementById('yei-success-banner-message');
        hideError();
        if (banner && msg) {
            msg.textContent = message;
            banner.style.display = '';
        }
    }

    function hideSuccess() {
        var banner = document.getElementById('yei-success-banner');
        if (banner) banner.style.display = 'none';
    }

    function showError(messages) {
        var summary = document.querySelector('.govuk-error-summary');
        var list    = document.getElementById('yei-error-list');
        hideSuccess();
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

    function hideError() {
        var summary = document.querySelector('.govuk-error-summary');
        if (summary) summary.style.display = 'none';
    }

    // ── Generic POST helper ───────────────────────────────────────────────────

    function postJson(url, data, onSuccess, onError) {
        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify(data)
        })
        .then(function (r) { return r.json(); })
        .then(function (result) {
            if (result.success) {
                onSuccess(result);
            } else {
                var msgs = (result.errors || []).map(function (e) { return e.message || e; });
                onError(msgs.length ? msgs : ['Request failed.']);
            }
        })
        .catch(function () { onError(['A network error occurred. Please try again.']); });
    }

    function getAntiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    // ── Config Value modal ────────────────────────────────────────────────────

    var _currentCfgBtn = null;

    function openConfigModal(btn) {
        _currentCfgBtn = btn;
        var id    = btn.getAttribute('data-config-id');
        var label = btn.getAttribute('data-config-label');
        var value = btn.getAttribute('data-config-value') || '';

        document.getElementById('configModalId').value = id;
        document.getElementById('configModalLabel').textContent = label;

        var isYesNo = label.toLowerCase().indexOf('approval') >= 0 ||
                      value === 'Yes' || value === 'No';

        var input  = document.getElementById('configModalInput');
        var select = document.getElementById('configModalSelect');
        if (isYesNo) {
            input.style.display  = 'none';
            select.style.display = '';
            select.value = value;
        } else {
            select.style.display = 'none';
            input.style.display  = '';
            input.value = value;
        }

        openModal('configEditModal');
    }

    function openModal(id) {
        var modal = document.getElementById(id);
        if (modal) {
            modal.setAttribute('aria-hidden', 'false');
            modal.classList.add('govuk-modal--open');
        }
    }

    function closeModal(id) {
        var modal = document.getElementById(id);
        if (modal) {
            modal.setAttribute('aria-hidden', 'true');
            modal.classList.remove('govuk-modal--open');
        }
    }

    document.addEventListener('DOMContentLoaded', function () {

        // Config edit buttons
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.cfg-edit-link');
            if (btn && !btn.disabled) {
                hideSuccess();
                hideError();
                openConfigModal(btn);
            }
        });

        // Config confirm (save-in-place) buttons
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.cfg-confirm-link');
            if (btn && !btn.disabled) {
                hideSuccess();
                hideError();
                var id    = btn.getAttribute('data-config-id');
                var label = btn.getAttribute('data-config-label');
                var value = btn.getAttribute('data-config-value') || '';
                saveConfigValue(id, label, value, btn.closest('tr'));
            }
        });

        // Config modal close
        document.getElementById('configModalClose').addEventListener('click', function () {
            closeModal('configEditModal');
        });
        document.getElementById('configModalCancel').addEventListener('click', function () {
            closeModal('configEditModal');
        });

        // Config modal save
        document.getElementById('configModalSave').addEventListener('click', function () {
            var id     = document.getElementById('configModalId').value;
            var label  = document.getElementById('configModalLabel').textContent;
            var input  = document.getElementById('configModalInput');
            var select = document.getElementById('configModalSelect');
            var value  = (select.style.display === 'none') ? input.value : select.value;

            var row = _currentCfgBtn ? _currentCfgBtn.closest('tr') : null;
            saveConfigValue(id, label, value, row, function () {
                closeModal('configEditModal');
            });
        });

        // Month Working Hours edit buttons
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.mh-edit-btn');
            if (btn && !btn.disabled) {
                hideSuccess();
                hideError();
                var row = btn.closest('tr');
                document.getElementById('monthModalYear').value    = row.dataset.year;
                document.getElementById('monthModalMonth').value   = row.dataset.month;
                document.getElementById('monthModalFmonth').value  = row.dataset.fmonth || '';
                document.getElementById('monthModalFpsYear').value = row.dataset.fpsYear;
                document.getElementById('monthModalDays').value    = row.dataset.days || '';
                document.getElementById('monthModalCvlHours').value = row.dataset.cvl || '';
                document.getElementById('monthModalVidHours').value = row.dataset.vid || '';
                openModal('monthRowEditModal');
                document.getElementById('monthRowEditModal')._row = row;
            }
        });

        // Month confirm (save-in-place) buttons
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.mh-confirm-btn');
            if (btn && !btn.disabled) {
                hideSuccess();
                hideError();
                var row = btn.closest('tr');
                saveMonthHour(row, row);
            }
        });

        // Month modal close
        document.getElementById('monthModalClose').addEventListener('click', function () {
            closeModal('monthRowEditModal');
        });
        document.getElementById('monthModalCancel').addEventListener('click', function () {
            closeModal('monthRowEditModal');
        });

        // Month modal save
        document.getElementById('monthModalSave').addEventListener('click', function () {
            var modal = document.getElementById('monthRowEditModal');
            var row   = modal._row;
            // Update dataset from inputs
            row.dataset.days  = document.getElementById('monthModalDays').value;
            row.dataset.cvl   = document.getElementById('monthModalCvlHours').value;
            row.dataset.vid   = document.getElementById('monthModalVidHours').value;
            saveMonthHour(row, row, function () {
                closeModal('monthRowEditModal');
            });
        });

        // Initiate DataSetup Request
        var btnInitiate = document.getElementById('btnInitiateDataSetupRequest');
        if (btnInitiate) {
            btnInitiate.addEventListener('click', function () {
                hideSuccess();
                hideError();
                btnInitiate.disabled = true;
                postJson(cfg.triggerInitiateUrl, {}, function () {
                    showSuccess('Year End Initiation request submitted successfully.');
                    btnInitiate.disabled = true;
                }, function (msgs) {
                    showError(msgs);
                    btnInitiate.disabled = false;
                });
            });
        }
    });

    // ── Save helpers ──────────────────────────────────────────────────────────

    function saveConfigValue(id, label, value, row, onDone) {
        var dto = { id: id, setting: label, notes: value };
        postJson(cfg.saveSettingUrl, dto, function () {
            if (row) {
                var valCell = row.querySelector('td:nth-child(2)');
                if (valCell) valCell.textContent = value;
                // Update data attributes on confirm buttons
                row.querySelectorAll('.cfg-confirm-link, .cfg-edit-link').forEach(function (b) {
                    b.setAttribute('data-config-value', value);
                });
            }
            showSuccess('Config value "' + label + '" saved successfully.');
            if (onDone) onDone();
        }, function (msgs) {
            showError(msgs);
            if (onDone) onDone();
        });
    }

    function saveMonthHour(row, displayRow, onDone) {
        var dto = {
            year:     parseInt(row.dataset.year, 10),
            month:    parseInt(row.dataset.month, 10),
            days:     parseFloat(row.dataset.days) || null,
            cvlHours: parseFloat(row.dataset.cvl) || null,
            vidHours: parseFloat(row.dataset.vid) || null,
            fmonth:   row.dataset.fmonth ? parseInt(row.dataset.fmonth, 10) : null,
            fpsYear:  parseInt(row.dataset.fpsYear, 10)
        };
        postJson(cfg.saveMonthHourUrl, dto, function () {
            if (displayRow) {
                displayRow.querySelector('.mh-days').textContent = dto.days != null ? dto.days : '';
                displayRow.querySelector('.mh-cvl').textContent  = dto.cvlHours != null ? dto.cvlHours : '';
                displayRow.querySelector('.mh-vid').textContent  = dto.vidHours != null ? dto.vidHours : '';
            }
            showSuccess('Month working hours saved successfully.');
            if (onDone) onDone();
        }, function (msgs) {
            showError(msgs);
            if (onDone) onDone();
        });
    }

})();

// Work Group Report page JavaScript
// getWorkGroupGridExtraFilters() is defined in the Razor @section Scripts block
// because it must be available before the _DataGrid grid manager initialises.

// ── Open edit modal via shared #modalPopup ────────────────────────────────────
function openWorkGroupEditModal(btnElement) {
    var $row = $(btnElement).closest('tr');
    var wgName = $row.find('td[data-property="WorkGroupName"] span').text().trim();
    var recipient = $row.find('td[data-property="EmailRecipient"] span').text().trim();

    // Read directly from the checkbox input's checked state in the SendEmailYes cell
    var flagged = $row.find('td[data-property="SendEmailYes"] input[type="checkbox"]').prop('checked') === true;

    $.ajax({
        url: '/PACT/WorkGroupReport/GetWorkGroupEdit',
        type: 'GET',
        data: {
            workGroupName: wgName,
            flaggedForEmail: flagged,
            emailRecipient: recipient
        },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            setTimeout(function () { $('#edit-email').focus(); }, 50);
        },
        error: function () {
            showAlertMessage('Error loading edit form.', AlertType.ERROR);
        }
    });
}

// ── Save handler (called from the partial's Save button) ──────────────────────
function saveWorkGroupEmail() {
    var form = $('#editEmailForm');
    var data = form.serializeObject ? form.serializeObject() : Object.fromEntries(new FormData(form[0]));

    if (data.sendEmail === undefined || data.sendEmail === '') {
        showAlertMessage('Please select Yes or No for SendEmail.', AlertType.ERROR);
        return;
    }

    $.ajax({
        url: '/PACT/WorkGroupReport/UpdateWorkGroupEmail',
        type: 'POST',
        data: data,
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').first().val() },
        success: function () {
            $('#modalPopup').removeClass('show');
            var gm = window['gridManager_workGroupGrid'];
            if (gm) { gm.reloadGrid({ page: 1 }); }
        },
        error: function () {
            showAlertMessage('Failed to save changes. Please try again.', AlertType.ERROR);
        }
    });
}

// ── Document ready ────────────────────────────────────────────────────────────
$(function () {

    // When Profit Centre dropdown changes: reload the grid AND refresh checkboxes
    $('#SelectedProfitCentre').on('change', function () {
        var pc = $(this).val();

        // Clear and hide previous send results when switching profit centre
        $('#sendResultsBody').empty();
        $('#sendResultsContainer').hide();

        // 1. Reload work-group grid
        var gm = window['gridManager_workGroupGrid'];
        if (gm) { gm.reloadGrid({ page: 1 }); }

        // 2. Fetch profit-centre settings and update the 4 checkboxes
        if (!pc) {
            applyProfitCentreSettings({ timesheet: false, outputsheet: false, timesheetLayout: 1 });
            return;
        }

        $.ajax({
            url: '/PACT/WorkGroupReport/GetProfitCentreSettings',
            type: 'GET',
            data: { profitCentre: pc },
            success: function (data) { applyProfitCentreSettings(data); },
            error: function () {
                applyProfitCentreSettings({ timesheet: false, outputsheet: false, timesheetLayout: 1 });
            }
        });
    });

    // Time sheet layout behaves like a radio group — checking one unchecks the other
    $('#layout-flat, #layout-crosstab').on('change', function () {
        if ($(this).is(':checked')) {
            var other = this.id === 'layout-flat' ? '#layout-crosstab' : '#layout-flat';
            $(other).prop('checked', false);
        }
        saveProfitCentreSettings();
    });

    // Time Sheets / Output Sheets checkboxes save independently
    $('#chk-timesheets, #chk-outputsheets').on('change', function () {
        saveProfitCentreSettings();
    });

    // ── Select PC's Work Groups ───────────────────────────────────────────────
    $('#btn-select-pc').on('click', function () {
        var pc = $('#SelectedProfitCentre').val();
        var hasTimeSheet = $('#chk-timesheets').is(':checked');
        var hasOutputSheet = $('#chk-outputsheets').is(':checked');

        if (!pc) { return; }

        if (!hasTimeSheet && !hasOutputSheet) {
            window.showAlertMessage('You must check Time Sheets and/or Output Sheets for the Profit Centre first.', AlertType.INFO);
            return;
        }

        $.ajax({
            url: '/PACT/WorkGroupReport/SelectPCWorkGroups',
            type: 'POST',
            data: {
                profitCentre: pc,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
            },
            complete: function () { reloadWorkGroupGrid(); }
        });
    });

    // ── Clear PC's Work Groups ────────────────────────────────────────────────
    $('#btn-clear-pc').on('click', function () {
        var pc = $('#SelectedProfitCentre').val();
        if (!pc) { return; }

        $.ajax({
            url: '/PACT/WorkGroupReport/ClearPCWorkGroups',
            type: 'POST',
            data: {
                profitCentre: pc,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
            },
            complete: function () { reloadWorkGroupGrid(); }
        });
    });

    // ── Clear All Work Groups ─────────────────────────────────────────────────
    $('#btn-clear-all-workgroups').on('click', function () {
        $.ajax({
            url: '/PACT/WorkGroupReport/ClearAllWorkGroups',
            type: 'POST',
            data: { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val() },
            complete: function () { reloadWorkGroupGrid(); }
        });
    });

    // ── Send Emails ───────────────────────────────────────────────────────────
    $('#btn-send-emails').on('click', function () {
        var pc = $('#SelectedProfitCentre').val();
        var period = $('#for-period-value').val();
        var hasTimeSheet = $('#chk-timesheets').is(':checked');
        var hasOutputSheet = $('#chk-outputsheets').is(':checked');

        if (!pc) {
            window.showAlertMessage('Please select a Profit Centre before sending.', AlertType.INFO);
            return;
        }
        if (!period) {
            window.showAlertMessage('Please select a Period before sending.', AlertType.INFO);
            return;
        }
        if (!hasTimeSheet && !hasOutputSheet) {
            window.showAlertMessage('You must check Time Sheets and/or Output Sheets before sending.', AlertType.INFO);
            return;
        }

        var $btn = $(this).prop('disabled', true).text('Sending…');

        $.ajax({
            url: '/PACT/WorkGroupReport/Send',
            type: 'POST',
            data: {
                profitCentre: pc,
                monthNumber: period,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
            },
            success: function (data) {
                renderSendResults(data.results);
                reloadWorkGroupGrid();
            },
            error: function (xhr) {
                var msg = (xhr.responseJSON && xhr.responseJSON.error)
                    ? xhr.responseJSON.error
                    : 'An error occurred while sending emails. Please try again.';
                window.showAlertMessage(msg, AlertType.ERROR);
            },
            complete: function () {
                $btn.prop('disabled', false).text('Send Emails');
            }
        });
    });
});

// ── Apply DB-driven settings to the four top checkboxes ──────────────────────
function applyProfitCentreSettings(data) {
    $('#chk-timesheets').prop('checked', data.timesheet === true);
    $('#chk-outputsheets').prop('checked', data.outputsheet === true);
    // timesheetLayout: 1 = Flat-file, 2 = Cross-tab (mirrors Access OptionValue)
    $('#layout-flat').prop('checked', data.timesheetLayout !== 2);
    $('#layout-crosstab').prop('checked', data.timesheetLayout === 2);
}

// ── PATCH the three profit-centre settings back to the database ───────────────
function saveProfitCentreSettings() {
    var pc = $('#SelectedProfitCentre').val();
    if (!pc) { return; }

    $.ajax({
        url: '/PACT/WorkGroupReport/PatchProfitCentreSettings',
        type: 'POST',
        data: {
            profitCentre: pc,
            sendTimeSheet: $('#chk-timesheets').is(':checked'),
            sendOutputSheet: $('#chk-outputsheets').is(':checked'),
            timesheetLayoutFlat: $('#layout-flat').is(':checked'),
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
        },
        error: function () {
            showAlertMessage('Failed to save settings. Please try again.', AlertType.ERROR);
        }
    });
}

// ── Reload just the work-group grid ──────────────────────────────────────────
function reloadWorkGroupGrid() {
    var gm = window['gridManager_workGroupGrid'];
    if (gm) { gm.reloadGrid({ page: 1 }); }
}

// ── Render email send results into the static results table ──────────────────
function renderSendResults(results) {
    var $body = $('#sendResultsBody').empty();

    if (!results || results.length === 0) {
        $body.append('<tr class="govuk-table__row"><td class="govuk-table__cell" colspan="4">No results returned.</td></tr>');
    } else {
        $.each(results, function (_, r) {
            $body.append(
                '<tr class="govuk-table__row">' +
                '<td class="govuk-table__cell">' + ($('<span>').text(r.workGroupName).html()) + '</td>' +
                '<td class="govuk-table__cell">' + ($('<span>').text(r.emailRecipient || '').html()) + '</td>' +
                '<td class="govuk-table__cell">' + ($('<span>').text(r.status || '').html()) + '</td>' +
                '<td class="govuk-table__cell">' + ($('<span>').text(r.reason || '').html()) + '</td>' +
                '</tr>'
            );
        });
    }

    $('#sendResultsContainer').show();
}

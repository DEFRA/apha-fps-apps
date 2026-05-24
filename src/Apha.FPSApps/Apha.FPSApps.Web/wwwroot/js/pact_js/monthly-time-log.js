// Monthly Time Log of Imports Page JavaScript

// ── Grid manager accessor ──────────────────────────────────────────
function getGridManager() {
    return window['gridManager_' + mtLogGridId];
}

// ── Validation summary helpers ─────────────────────────────────────
function showMtLogError(message) {
    $('#mtLogErrorList').empty().append('<li>' + message + '</li>');
    $('#mtLogErrorSummary').show().focus();
}

function clearMtLogError() {
    $('#mtLogErrorSummary').hide();
    $('#mtLogErrorList').empty();
}

// ── Criteria check ─────────────────────────────────────────────────
function hasMtLogCriteria() {
    return !!(
        $('#ddWorkGroup').val()    ||
        $('#ddProject').val()      ||
        $('#txtMonth').val()       ||
        $('#ddJobCode').val()      ||
        $('#ddTestCode').val()     ||
        $('#txtStaffId').val()     ||
        $('#dtDateImported').val() ||
        $('#txtUserId').val()      ||
        $('#ddAction').val()
    );
}

// ── Extra filters for _DataGrid gridManager ────────────────────────
// Called by _DataGrid.cshtml gridManager on every reload
// (pagination, sort, page-size). Returns ONLY search-panel values.
function getExtraFilters_mtLogGrid() {
    // JobCode and TestCode are OR alternatives — send whichever is set
    var jobCode  = $('#ddJobCode').val()  || null;
    var testCode = $('#ddTestCode').val() || null;
    // Use jobCode if set, otherwise testCode (mirrors frmMT_Log.frm logic)
    var timeCode = jobCode || testCode || null;

    return {
        workGroup:    $('#ddWorkGroup').val()    || null,
        timeCode:     timeCode,
        parentProject: $('#ddProject').val()    || null,
        pactStaffId:  $('#txtStaffId').val()    || null,
        dateImported: $('#dtDateImported').val() || null,
        month:        $('#txtMonth').val()       || null,
        userId:       $('#txtUserId').val()      || null,
        insertDelete: $('#ddAction').val()       || null
    };
}

// ── Button handlers ────────────────────────────────────────────────
$(function () {
    // Mutually exclusive: clearing JobCode clears TestCode and vice versa
    $('#ddJobCode').on('change', function () {
        if ($(this).val()) {
            $('#ddTestCode').val('');
        }
    });

    $('#ddTestCode').on('change', function () {
        if ($(this).val()) {
            $('#ddJobCode').val('');
        }
    });

    $('#btnSearch').on('click', function () {
        clearMtLogError();
        if (!hasMtLogCriteria()) {
            showMtLogError('Please enter some criteria before searching.');
            return;
        }
        var gm = getGridManager();
        if (gm) {
            gm.reloadGrid({ page: 1 });
        }
    });

    $('#btnClearAll').on('click', function () {
        clearMtLogError();
        $('#ddWorkGroup').val('');
        $('#ddProject').val('');
        $('#txtMonth').val('');
        $('#ddJobCode').val('');
        $('#ddTestCode').val('');
        $('#txtStaffId').val('');
        $('#dtDateImported').val('');
        $('#txtUserId').val('');
        $('#ddAction').val('');
    });
});

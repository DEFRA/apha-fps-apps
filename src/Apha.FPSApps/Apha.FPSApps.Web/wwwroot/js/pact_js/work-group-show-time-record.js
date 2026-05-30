/**
 * work-group-show-time-record.js
 * Client-side logic for the PACT Work Group Show Time Records page.
 */

var timeRecordsGridId = null;
var currentWorkGroup  = null;
var currentMonthNumber = null;

/**
 * Returns the current work group and month number as extra filter parameters
 * for every grid request (initial load, sort, pagination, and filter events).
 * Called by the _DataGrid partial via the ExtraFilterMethod hook.
 */
function getTimeRecordsExtraFilters() {
    return {
        workGroup: currentWorkGroup || null,
        monthNumber: currentMonthNumber || 1
    };
}

/**
 * Validates that a Work Group has been selected.
 * Shows a GDS error message and error styling on the input if blank.
 * Clears the error when a valid value is present.
 * @returns {boolean} true if valid, false if the field is empty.
 */
function validateWorkGroupSelected() {
    var $input = $('#workGroupSelect');
    var $error = $('#workGroupError');
    if (!currentWorkGroup) {
        $input.addClass('govuk-input--error');
        $error.show();
        return false;
    }
    $input.removeClass('govuk-input--error');
    $error.hide();
    return true;
}

/**
 * Reloads the time records grid resetting pagination, sort, and filter state.
 * Aborts and shows a validation error if no Work Group is selected.
 */
function reloadTimeRecordsGrid() {
    if (!validateWorkGroupSelected()) return;

    var gm = window['gridManager_' + timeRecordsGridId];
    if (!gm) return;

    gm.reloadGrid({
        filter: '{}',
        sortBy: '',
        descending: false,
        page: 1
    });
}

/**
 * Sums all Hours values in the current grid page and writes the result to #totalHours.
 * Reads from <td data-property="Hours"> cells rendered by the _DataGrid partial.
 */
function updateTotalHours() {
    var total = 0;
    $('#tbl_' + timeRecordsGridId + ' tbody td[data-property="Hours"] span').each(function () {
        var val = parseFloat($(this).text().trim());
        if (!isNaN(val)) total += val;
    });
    $('#totalHours').val(total.toFixed(2));
}

/**
 * Initialises the Work Group and Calendar Month searchable dropdowns.
 */
function initWorkGroupShowTimeRecordPage() {

    // ── Work Group searchable dropdown ────────────────────────────────────
    var $wgInput  = $('#workGroupSelect');
    var $wgPanel  = $('#workGroupDropdownPanel');
    var $wgSearch = $('#workGroupSearchBox');
    var $wgRows   = $('#workGroupDropdownBody tr');

    $wgInput.on('click', function (e) {
        e.stopPropagation();
        $wgPanel.toggle();
        if ($wgPanel.is(':visible')) {
            $wgSearch.val('').focus();
            $wgRows.show();
        }
    });

    $wgSearch.on('click', function (e) { e.stopPropagation(); });

    $wgSearch.on('input', function () {
        var term = $(this).val().toLowerCase();
        $wgRows.each(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(term) > -1);
        });
    });

    $(document).on('click', '#workGroupDropdownBody tr', function () {
        var value = $(this).data('value');
        var text  = $(this).find('td:first').text().trim();
        $wgInput.val(text);
        $wgPanel.hide();
        currentWorkGroup = value || null;
        validateWorkGroupSelected();
        reloadTimeRecordsGrid();
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#workGroupSelect, #workGroupDropdownPanel').length) {
            $wgPanel.hide();
        }
    });

    // ── Calendar Month searchable dropdown ────────────────────────────────
    var $cmInput  = $('#calenderMonthSelect');
    var $cmPanel  = $('#calenderMonthDropdownPanel');
    var $cmSearch = $('#calenderMonthSearchBox');
    var $cmRows   = $('#calenderMonthDropdownBody tr');

    $cmInput.on('click', function (e) {
        e.stopPropagation();
        $cmPanel.toggle();
        if ($cmPanel.is(':visible')) {
            $cmSearch.val('').focus();
            $cmRows.show();
        }
    });

    $cmSearch.on('click', function (e) { e.stopPropagation(); });

    $cmSearch.on('input', function () {
        var term = $(this).val().toLowerCase();
        $cmRows.each(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(term) > -1);
        });
    });

    $(document).on('click', '#calenderMonthDropdownBody tr', function () {
        var value = $(this).data('value');
        var text  = $(this).data('text');
        $cmInput.val(text);
        $cmPanel.hide();
        currentMonthNumber = value ? parseInt(value, 10) : 1;
        reloadTimeRecordsGrid();
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#calenderMonthSelect, #calenderMonthDropdownPanel').length) {
            $cmPanel.hide();
        }
    });

    // ── Pre-select work group passed from the previous page ───────────────
    var preselectedWorkGroup = $('#hdnSelectedWorkGroup').val();
    if (preselectedWorkGroup) {
        var $matchRow = $('#workGroupDropdownBody tr[data-value="' + preselectedWorkGroup + '"]');
        if ($matchRow.length) {
            $wgInput.val($matchRow.find('td:first').text().trim());
            currentWorkGroup = preselectedWorkGroup;
        }
    }

    // ── Pre-select calendar month passed from the previous page ───────────
    var preselectedMonthNumber = parseInt($('#hdnSelectedMonthNumber').val(), 10);
    if (!isNaN(preselectedMonthNumber) && preselectedMonthNumber > 0) {
        var $cmMatchRow = $('#calenderMonthDropdownBody tr[data-value="' + preselectedMonthNumber + '"]');
        if ($cmMatchRow.length) {
            $cmInput.val($cmMatchRow.data('text'));
            currentMonthNumber = preselectedMonthNumber;
        }
    }

    // ── Enter key support for grid column filters ─────────────────────────
    $('#gridContainer_' + timeRecordsGridId).on('keypress', '.grid-filter', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            $(this).trigger('change');
        }
    });

    // ── Update Total Hours whenever the grid reloads ───────────────────
    document.addEventListener('gridReloaded', function (e) {
        if (e.detail && e.detail.gridId === timeRecordsGridId) {
            updateTotalHours();
        }
    });

    // ── Update Total Hours for the initial server-rendered grid ───────────
    updateTotalHours();
}

$(document).ready(function () {
    initWorkGroupShowTimeRecordPage();
});

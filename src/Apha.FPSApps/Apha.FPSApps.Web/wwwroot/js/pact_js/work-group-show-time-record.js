/**
 * work-group-show-time-record.js
 * Client-side logic for the PACT Work Group Show Time Records page.
 */

var timeRecordsGridId = null;
var currentWorkGroup  = null;
var currentMonthNumber = null;

/**
 * Reloads the time records grid resetting pagination, sort, and filter state.
 */
function reloadTimeRecordsGrid() {
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
        if (currentWorkGroup) reloadTimeRecordsGrid();
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
        currentMonthNumber = value || null;
        if (currentMonthNumber) reloadTimeRecordsGrid();
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

    // ── Enter key support for grid column filters ─────────────────────────
    $('#gridContainer_' + timeRecordsGridId).on('keypress', '.grid-filter', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            $(this).trigger('change');
        }
    });

    // ── Initial grid load ─────────────────────────────────────────────────
    //reloadTimeRecordsGrid();

    // ── Update Total Hours whenever the grid reloads ───────────────────
    document.addEventListener('gridReloaded', function (e) {
        if (e.detail && e.detail.gridId === timeRecordsGridId) {
            updateTotalHours();
        }
    });
}

$(document).ready(function () {
    initWorkGroupShowTimeRecordPage();
});

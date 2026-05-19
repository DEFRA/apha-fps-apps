/**
 * work-group-valid-time-code.js
 * Client-side logic for the PACT Work Group Valid Time Codes page.
 */

var validTimeCodesGridId = null;
var currentWorkGroup     = null;

/**
 * Returns the current work group as an extra filter parameter for every
 * grid request (initial load, sort, pagination, and filter events).
 * Called by the _DataGrid partial via the ExtraFilterMethod hook.
 */
function getValidTimeCodesExtraFilters() {
    return {
        workGroup: currentWorkGroup || ''
    };
}

/**
 * Reloads the valid time codes grid, resetting pagination, sort, and filter state.
 */
function reloadValidTimeCodesGrid() {
    var gm = window['gridManager_' + validTimeCodesGridId];
    if (!gm) return;

    gm.reloadGrid({
        filter: '{}',
        sortBy: '',
        descending: false,
        page: 1
    });
}

/**
 * Initialises the Work Group searchable dropdown.
 */
function initWorkGroupValidTimeCodePage() {

    // ── Work Group searchable dropdown ────────────────────────────────────
    var $wgInput  = $('#workGroupSelect');
    var $wgPanel  = $('#workGroupDropdownPanel');
    var $wgSearch = $('#workGroupSearchBox');

    $wgInput.on('click', function (e) {
        e.stopPropagation();
        $wgPanel.toggle();
        if ($wgPanel.is(':visible')) {
            $wgSearch.val('').focus();
            $('#workGroupDropdownBody tr').show();
        }
    });

    $wgSearch.on('click', function (e) { e.stopPropagation(); });

    $wgSearch.on('input', function () {
        var term = $(this).val().toLowerCase();
        $('#workGroupDropdownBody tr').each(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(term) > -1);
        });
    });

    $(document).on('click', '#workGroupDropdownBody tr', function () {
        var value = $(this).data('value');
        $wgInput.val($(this).find('td:first').text().trim());
        $wgPanel.hide();
        currentWorkGroup = value || null;
        $('#hdnSelectedWorkGroup').val(currentWorkGroup || '');
        reloadValidTimeCodesGrid();
    });

    $(document).on('click', function (e) {
        if ($(e.target).closest('#workGroupSelect, #workGroupDropdownPanel').length === 0) {
            $wgPanel.hide();
        }
    });

    // ── Pre-select work group passed from the previous page ───────────────
    var preselected = $('#hdnSelectedWorkGroup').val();
    if (preselected) {
        var $matchRow = $('#workGroupDropdownBody tr[data-value="' + preselected + '"]');
        if ($matchRow.length) {
            $wgInput.val($matchRow.find('td:first').text().trim());
        }
        currentWorkGroup = preselected;
        reloadValidTimeCodesGrid();
    }

    // ── Enter key support for grid column filters ─────────────────────────
    $('#gridContainer_' + validTimeCodesGridId).on('keypress', '.grid-filter', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            $(this).trigger('change');
        }
    });
}

$(document).ready(function () {
    initWorkGroupValidTimeCodePage();
});

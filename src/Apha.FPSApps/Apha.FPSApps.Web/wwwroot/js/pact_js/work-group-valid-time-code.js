/**
 * work-group-valid-time-code.js
 * Client-side logic for the PACT Work Group Valid Time Codes page.
 */

var validTimeCodesGridId   = null;
var currentWorkGroup       = null;
var selectedParentProject  = null;

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
 * Also clears the "I have selected" input and disables the navigation button
 * since the previous row selection is no longer valid after a work group change.
 */
function reloadValidTimeCodesGrid() {
    selectedParentProject = null;
    $('#selectedProject').val('');
    $('#btnShowProjectAdministraction').prop('disabled', true);

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
 * Called when a row is selected in the valid time codes grid.
 * Displays the selected time code in the "I have selected" input box.
 * @param {HTMLElement} rowData - The selected grid row element.
 */
function onValidTimeCodeRowSelect(rowData) {
    var timeCode      = $(rowData).find('[data-property="TimeCode"]').text().trim();
    var parentProject = $(rowData).find('[data-property="ParentProject"]').text().trim();
    selectedParentProject = parentProject || null;
    $('#selectedProject').val(parentProject);
    $('#btnShowProjectAdministraction').prop('disabled', !selectedParentProject);
}

/**
 * Selects the first selectable row in the valid time codes grid and fires
 * its row-select callback, populating the "I have selected" panel automatically.
 */
function selectFirstValidTimeCodeRow() {
    var $firstRow = $('#tbl_' + validTimeCodesGridId + ' tbody tr.selectable-row:first');
    if ($firstRow.length) {
        $('#tbl_' + validTimeCodesGridId + ' tbody tr').removeClass('selected-row');
        $firstRow.addClass('selected-row');
        onValidTimeCodeRowSelect($firstRow[0]);
    }
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

    // ── Auto-select first row after grid reload ───────────────────────────
    var gridContainer = document.getElementById('gridContainer_' + validTimeCodesGridId);
    if (gridContainer) {
        new MutationObserver(function () {
            selectFirstValidTimeCodeRow();
        }).observe(gridContainer, { childList: true, subtree: true });
    }

    // ── Enter key support for grid column filters ─────────────────────────
    $('#gridContainer_' + validTimeCodesGridId).on('keypress', '.grid-filter', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            $(this).trigger('change');
        }
    });

    // ── Show Project Administration button ────────────────────────────────
    $('#btnShowProjectAdministraction').prop('disabled', true).on('click', function () {
        if (!selectedParentProject) return;
        window.fpsNavigateTo('/PACT/ProjectMaintenance/Details/' + encodeURIComponent(selectedParentProject));
    });
}

$(document).ready(function () {
    initWorkGroupValidTimeCodePage();
});

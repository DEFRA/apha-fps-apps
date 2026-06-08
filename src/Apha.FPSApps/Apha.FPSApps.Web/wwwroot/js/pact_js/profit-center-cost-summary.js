/**
 * profit-center-cost-summary.js
 * Client-side logic for the PACT PC SOCT Query page.
 * Handles the Period searchable dropdown and grid reload.
 */

var profitCenterCostGridId = null;
var currentMonthNumber = null;

/**
 * Returns the grid manager instance for the profit center cost grid.
 * @returns {object|undefined} The grid manager, or undefined if not yet initialised.
 */
function getProfitCenterCostGridManager() {
    return window['gridManager_' + profitCenterCostGridId];
}

/**
 * Returns extra filter parameters to be appended to each grid reload request.
 * This function is called by the grid manager when reloading data.
 * @returns {{ monthNumber: string|null }}
 */
function getProfitCenterCostGridExtraFilters() {
    return { monthNumber: currentMonthNumber || null };
}

/**
 * Called when a period is selected from the dropdown.
 * Updates the current month number and reloads the grid with the selected period filter.
 * @param {string|null} monthNumber - The selected month number, or null to clear.
 */
function onPeriodChange(monthNumber) {
    currentMonthNumber = monthNumber || null;

    if (!monthNumber) {
        reloadAllProfitCenterCostGrid();
        return;
    }

    reloadProfitCenterCostGrid(monthNumber);
}

/**
 * Reloads the profit center cost grid filtered by the specified month number,
 * resetting pagination, sort, and filter state.
 * @param {string} monthNumber - The month number to filter by.
 */
function reloadProfitCenterCostGrid(monthNumber) {
    var gm = getProfitCenterCostGridManager();
    if (!gm) {
        window.location.href = '/PACT/ProfitCenterCostSummary/Index?monthNumber=' + encodeURIComponent(monthNumber);
        return;
    }

    gm.reloadGrid({
        filter: '{}',
        sortBy: '',
        descending: false,
        page: 1,
        pageSize: 10
    }, { monthNumber: monthNumber });
}

/**
 * Reloads the profit center cost grid with no filter applied, showing all data.
 * Resets pagination, sort, and filter state.
 */
function reloadAllProfitCenterCostGrid() {
    var gm = getProfitCenterCostGridManager();
    if (!gm) {
        window.location.href = '/PACT/ProfitCenterCostSummary/Index';
        return;
    }

    gm.reloadGrid({
        filter: '{}',
        sortBy: '',
        descending: false,
        page: 1,
        pageSize: 10
    });
}

/**
 * Initialises the Period searchable dropdown.
 * Intended to be called on document ready.
 */
function initProfitCenterCostSummaryPage() {
    var $pInput  = $('#periodSelectDropdown');
    var $pPanel  = $('#periodDropdownPanel');
    var $pSearch = $('#periodSearchBox');
    var $pRows   = $('#PeriodDropdownBody tr');

    $pInput.on('click', function (e) {
        e.stopPropagation();
        $pPanel.toggle();
        if ($pPanel.is(':visible')) {
            $pSearch.val('').focus();
            $pRows.show();
        }
    });

    $pSearch.on('click', function (e) { e.stopPropagation(); });

    $pSearch.on('input', function () {
        var term = $(this).val().toLowerCase();
        $pRows.each(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(term) > -1);
        });
    });

    $(document).on('click', '#PeriodDropdownBody tr', function () {
        var monthNumber = $(this).data('value');
        var text = $(this).find('td:first').text().trim();
        $pInput.val(text);
        $pPanel.hide();
        onPeriodChange(monthNumber);
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#periodSelectDropdown, #periodDropdownPanel').length) {
            $pPanel.hide();
        }
    });

    if (profitCenterCostGridId) {
        $('#gridContainer_' + profitCenterCostGridId).on('keypress', '.grid-filter', function (e) {
            if (e.which === 13) {
                e.preventDefault();
                $(this).trigger('change');
            }
        });
    }
}

$(document).ready(function () {
    initProfitCenterCostSummaryPage();
});

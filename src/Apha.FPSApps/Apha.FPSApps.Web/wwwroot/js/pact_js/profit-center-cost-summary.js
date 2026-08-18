/**
 * profit-center-cost-summary.js
 * Client-side logic for the PACT PC SOCT Query page.
 * Handles the Period searchable dropdown and grid reload.
 */

var profitCenterCostGridId = null;
var currentMonthNumber = null;

/**
 * Returns the grid manager instance for the profit center cost grid.
 * @returns {object|undefined} The grid manager, or undefined if not yet initialised .
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
 * Uses a direct AJAX call with the anti-forgery token to avoid 403 errors
 * that occur when relying on the grid manager's internal $.post (which omits the token).
 * @param {string} monthNumber - The month number to filter by.
 */
function reloadProfitCenterCostGrid(monthNumber) {
    var antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]');
    var token = antiForgeryToken ? antiForgeryToken.value : '';

    $.ajax({
        url: '/PACT/ProfitCenterCostSummary/LoadProfitCenterCostGrid',
        type: 'POST',
        headers: { 'RequestVerificationToken': token },
        data: {
            page: 1,
            pageSize: 10,
            sortBy: '',
            descending: false,
            filter: '{}',
            monthNumber: monthNumber,
            __RequestVerificationToken: token
        },
        success: function (html) {
            $('#gridContainer_profitCenterCostGrid').html(html);
            profitCenterCostGridId = 'profitCenterCostGrid';
        },
        error:
            error: function()
        {
            $('#gridContainer_profitCenterCostGrid').html(
                '<div class="govuk-error-message">Failed to load data. Please try again.</div>'
            );
        }
        //     function (xhr, status, error) {
        //     if (xhr.status === 401) {
        //         // Session expired — reload the page to trigger the OIDC login flow
        //         window.location.reload();
        //     } else {
        //         console.error('Failed to load grid:', error);
        //         $('#gridContainer_profitCenterCostGrid').html(
        //             '<div class="govuk-error-message">Failed to load data. Please try again.</div>'
        //         );
        //     }
        // }
    });
}

/**
 * Reloads the profit center cost grid with no filter applied, showing all data.
 * Resets pagination, sort, and filter state.
 */
function reloadAllProfitCenterCostGrid() {
    var gm = getProfitCenterCostGridManager();
    if (!gm) {
        // No grid to reload - just clear the container
        $('#gridContainer_profitCenterCostGrid').html(
            '<div class="govuk-inset-text" style="margin-top: 20px;">' +
            '<p class="govuk-body">Please select a period from the dropdown above to view the Profit Center Cost Summary data.</p>' +
            '</div>'
        );
        return;
    }

    gm.reloadGrid({
        page: 1,
        pageSize: 10,
        sortBy: '',
        descending: false,
        filter: '{}'
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

    // Set the selected period if a month number is already selected
    if (currentMonthNumber) {
        var selectedRow = $('#PeriodDropdownBody tr[data-value="' + currentMonthNumber + '"]');
        if (selectedRow.length) {
            var selectedText = selectedRow.find('td:first').text().trim();
            $('#periodSelectDropdown').val(selectedText);
        }
    }
}

$(document).ready(function () {
    initProfitCenterCostSummaryPage();
});

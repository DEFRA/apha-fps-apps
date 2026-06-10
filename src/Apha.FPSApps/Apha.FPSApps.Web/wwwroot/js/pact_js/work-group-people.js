/**
 * work-group-people.js
 * Client-side logic for the PACT Work Group People page.
 * Handles the WorkGroup and Person searchable dropdowns,
 * grid reload, and grid row selection behaviour.
 */

var peopleGridId = null;
var preselectedWorkGroup = null;

/**
 * Returns the grid manager instance for the people grid.
 * @returns {object|undefined} The grid manager, or undefined if not yet initialised.
 */
function getPeopleGridManager() {
    return window['gridManager_' + peopleGridId];
}

/**
 * Returns extra filter parameters to be appended to each grid reload request.
 * @returns {{ workGroup: string }}
 */
function getPeopleGridExtraFilters() {
    return { workGroup: currentWorkGroup || '' };
}

var currentWorkGroup  = null;
var currentPersonName = null;

/**
 * Called when a work group is selected from the dropdown.
 * Clears the person selection, updates the information panel,
 * toggles the work group action buttons, and reloads the grid.
 * @param {string|null} workGroup - The selected work group name, or null to clear.
 */
function onWorkGroupPickChange(workGroup) {
    currentWorkGroup  = workGroup || null;
    currentPersonName = null;

    document.getElementById('personSelect').value = '';
    document.getElementById('selectedWorkgroup').value = workGroup || '';
    document.getElementById('selectedPerson').value = '';
    document.getElementById('btnShowTimeByJob').disabled = true;

    var hasWg = !!workGroup;
    ['btnShowSummary', 'btnShowTimeRecords', 'btnShowTimeCodes', 'btnShowTestOutputs']
        .forEach(function (id) {
            var btn = document.getElementById(id);
            if (btn) btn.disabled = !hasWg;
        });

    if (!workGroup) {
        reloadAllPeopleGrid();
        return;
    }

    reloadPeopleGrid(workGroup);
}

/**
 * Called when a person is selected from the dropdown.
 * Clears the work group selection, updates the information panel,
 * and reloads the grid filtered to the selected person.
 * @param {string|null} personName - The selected person name, or null to clear.
 * @param {string|null} personWorkGroup - The work group associated with the selected person.
 */
function onPersonPickChange(personName, personWorkGroup) {
    currentPersonName = personName || null;
    currentWorkGroup  = personWorkGroup || null;

    document.getElementById('workGroupSelect').value = '';
    document.getElementById('selectedWorkgroup').value = '';
    ['btnShowSummary', 'btnShowTimeRecords', 'btnShowTimeCodes', 'btnShowTestOutputs']
        .forEach(function (id) {
            var btn = document.getElementById(id);
            if (btn) btn.disabled = true;
        });

    document.getElementById('selectedPerson').value = personName || '';
    document.getElementById('btnShowTimeByJob').disabled = !personName;

    if (!personName) {
        reloadAllPeopleGrid();
        return;
    }

    reloadPeopleGridByPerson(personName, personWorkGroup);
}

/**
 * Reloads the people grid filtered by the specified work group,
 * resetting pagination, sort, and filter state.
 * @param {string} workGroup - The work group name to filter by.
 */
function reloadPeopleGrid(workGroup) {
    var gm = getPeopleGridManager();
    if (!gm) return;

    gm.reloadGrid({
        filter: '{}',
        sortBy: '',
        descending: false,
        page: 1,
        pageSize: 10
    }, { workGroup: workGroup });
}

/**
 * Reloads the people grid with no filter applied, showing all staff.
 * Resets pagination, sort, and filter state.
 */
function reloadAllPeopleGrid() {
    var gm = getPeopleGridManager();
    if (!gm) return;

    gm.reloadGrid({
        filter: '{}',
        sortBy: '',
        descending: false,
        page: 1,
        pageSize: 10
    });
}

/**
 * Reloads the people grid filtered by the specified person name via AJAX,
 * replacing the grid container HTML with the returned partial view.
 * @param {string} personName - The person name to filter by.
 * @param {string|null} personWorkGroup - The work group associated with the selected person.
 */
function reloadPeopleGridByPerson(personName, personWorkGroup) {
    $.ajax({
        url: '/PACT/WorkGroupPeople/LoadPeopleGrid',
        type: 'POST',
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        data: {
            Filter: '{}',
            SortBy: '',
            Descending: false,
            Page: 1,
            PageSize: 10,
            workGroup: personWorkGroup || null
        },
        success: function (html) {
            $('#gridContainer_peopleGrid').html(html);
            selectFirstPersonRow();
        },
        error: function () {
            console.error('Failed to load People grid by person.');
        }
    });
}

/**
 * Called when a row is selected in the people grid.
 * Updates the selected person information panel and enables the time-by-job button.
 * @param {HTMLElement} rowData - The selected grid row element.
 */
function onPersonRowSelect(rowData) {
    var name = $(rowData).find('[data-property="Name"]').text().trim();
    currentPersonName = name || null;
    document.getElementById('selectedPerson').value = name;
    document.getElementById('btnShowTimeByJob').disabled = !name;
}

/**
 * Selects the first selectable row in the people grid and fires its row-select
 * callback, populating the People Information panel automatically.
 * Only updates currentPersonName when a person filter is already active.
 */
function selectFirstPersonRow() {
    var $firstRow = $('#tbl_' + peopleGridId + ' tbody tr.selectable-row:first');
    if ($firstRow.length) {
        $('#tbl_' + peopleGridId + ' tbody tr').removeClass('selected-row');
        $firstRow.addClass('selected-row');
        onPersonRowSelect($firstRow[0]);
        // Only persist the selected person back to the filter state when
        // the grid is already in person-filter mode; never override it when
        // showing all-staff or work-group filtered results.
        if (currentPersonName) {
            var name = $firstRow.find('[data-property="Name"]').text().trim();
            currentPersonName = name || null;
        }
    }
}

/**
 * Navigates to the Test Capabilities for WorkGroup page with the currently
 * selected work group pre-populated.
 */
function navigateToTestCapabilities() {
    if (!currentWorkGroup) {
        console.warn('No work group selected.');
        return;
    }
    // Navigate to WorkGroupTestCapability with workgroup as query parameter
    window.fpsNavigateTo('/PACT/WorkGroupTestCapability?workgroup=' + encodeURIComponent(currentWorkGroup));
}

/**
 * Initialises the WorkGroup and Person searchable dropdowns,
 * and wires up Enter-key support for people grid filter inputs.
 * Intended to be called on document ready.
 */
function initWorkGroupPeoplePage() {
    // ── WorkGroup searchable dropdown ──────────────────────────────────────
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
        $('#workgroupValidationError').hide();
        $('#selectedWorkgroup').removeClass('govuk-input--error');
        onWorkGroupPickChange(value);
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#workGroupSelect, #workGroupDropdownPanel').length) {
            $wgPanel.hide();
        }
    });

    // ── Person searchable dropdown ─────────────────────────────────────────
    var $pInput  = $('#personSelect');
    var $pPanel  = $('#personDropdownPanel');
    var $pSearch = $('#personSearchBox');
    var $pRows   = $('#personDropdownBody tr');

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

    $(document).on('click', '#personDropdownBody tr', function () {
        var value     = $(this).attr('data-value');
        var workGroup = $(this).attr('data-workgroup') || null;
        var text      = $(this).find('td:first').text().trim();
        $pInput.val(text);
        $pPanel.hide();
        onPersonPickChange(value, workGroup);
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#personSelect, #personDropdownPanel').length) {
            $pPanel.hide();
        }
    });

    // ── Enter key support for grid filters ────────────────────────────────
    $('#gridContainer_' + peopleGridId).on('keypress', '.grid-filter', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            $(this).trigger('change');
        }
    });

    // ── Show Time by JobCode and Month button ──────────────────────────────
    $('#btnShowTimeByJob').on('click', function () {
        if (!currentWorkGroup) {
            showGovukAlert('Please select a person first.');
            return;
        }

        var url = '/PACT/WorkGroupSummarisedStaffTimeUsage?workGroup=' + encodeURIComponent(currentWorkGroup) + '&staffName=' + encodeURIComponent(currentPersonName || '');
        window.fpsNavigateTo(url);
    });

    // ── Show Time Records button ───────────────────────────────────────────
    $('#btnShowTimeRecords').on('click', function () {
        var $error = $('#workgroupValidationError');
        var $input = $('#selectedWorkgroup');
        if (!currentWorkGroup) {
            $input.addClass('govuk-input--error');
            $error.show();
            showGovukAlert('Please select a Work Group first.');
            return;
        }
        $input.removeClass('govuk-input--error');
        $error.hide();
        window.fpsNavigateTo('/PACT/WorkGroupShowTimeRecord?workGroup=' + encodeURIComponent(currentWorkGroup));
    });

    // ── Show Valid Time Codes button ───────────────────────────────────────
    $('#btnShowTimeCodes').on('click', function () {
        var $error = $('#workgroupValidationError');
        var $input = $('#selectedWorkgroup');
        if (!currentWorkGroup) {
            $input.addClass('govuk-input--error');
            $error.show();
            showGovukAlert('Please select a Work Group first.');
            return;
        }
        $input.removeClass('govuk-input--error');
        $error.hide();
        window.fpsNavigateTo('/PACT/WorkGroupValidTimeCode?workGroup=' + encodeURIComponent(currentWorkGroup));
    });

    // ── Show Summarised WorkGroup Time button ──────────────────────────────
    $('#btnShowSummary').on('click', function () {
        var $error = $('#workgroupValidationError');
        var $input = $('#selectedWorkgroup');
        if (!currentWorkGroup) {
            $input.addClass('govuk-input--error');
            $error.show();
            showGovukAlert('Please select a Work Group first.');
            return;
        }
        $input.removeClass('govuk-input--error');
        $error.hide();
        window.fpsNavigateTo('/PACT/WorkGroupSummarisedTimeUsage?workGroup=' + encodeURIComponent(currentWorkGroup));
    });
}

$(document).ready(function () {
    initWorkGroupPeoplePage();

    // Wire up the "Show me valid Test Outputs" button
    $('#btnShowTestOutputs').on('click', navigateToTestCapabilities);

    // ── Pre-select work group when returning from a child page ────────────
    if (preselectedWorkGroup) {
        var $matchRow = $('#workGroupDropdownBody tr[data-value="' + preselectedWorkGroup + '"]');
        if ($matchRow.length) {
            $('#workGroupSelect').val($matchRow.find('td:first').text().trim());
        }
        onWorkGroupPickChange(preselectedWorkGroup);
    }

    // Select first row on initial page load
    selectFirstPersonRow();

    // Re-select first row whenever the grid manager reloads the grid
    document.addEventListener('gridReloaded', function (e) {
        if (e.detail && e.detail.gridId === peopleGridId) {
            selectFirstPersonRow();
        }
    });
});

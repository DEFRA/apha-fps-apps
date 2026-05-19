// ══════════════════════════════════════════════════════════════════════════════
// WorkGroup Test Capability - Client-side functionality
// ══════════════════════════════════════════════════════════════════════════════

(function () {
    'use strict';

    // ── State Management ──────────────────────────────────────────────────
    var currentWorkGroup = null;
    var testCapabilityGridId = null;

    // ── Initialization ────────────────────────────────────────────────────
    function initialize(gridId) {
        testCapabilityGridId = gridId;

        // Check if workgroup is passed as query parameter
        var workgroupParam = getUrlParameter('workgroup');

        if (workgroupParam) {
            // Set the dropdown value
            $('#selectedWorkgroup').val(workgroupParam);
            currentWorkGroup = workgroupParam;
            // Load the grid with the workgroup
            reloadTestCapabilityGrid(workgroupParam);
        } else {
            // Clear grid on load if no workgroup specified
            clearTestCapabilityGrid();
        }
    }

    // ── Grid Manager Helper ───────────────────────────────────────────────
    function getCapabilityGridManager() {
        return window['gridManager_' + testCapabilityGridId];
    }

    // ── WorkGroup Selection ───────────────────────────────────────────────
    function onWorkGroupChange(value) {
        currentWorkGroup = value || null;
        if (currentWorkGroup) {
            reloadTestCapabilityGrid(currentWorkGroup);
        } else {
            clearTestCapabilityGrid();
        }
    }

    // ── Grid Reload ───────────────────────────────────────────────────────
    function reloadTestCapabilityGrid(workGroup) {
        $.ajax({
            url: '/PACT/WorkGroupTestCapability/LoadTestCapabilityGrid',
            type: 'POST',
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            data: {
                Page: 1,
                PageSize: 10,
                Filter: '{}',
                workGroup: workGroup || ''
            },
            success: function (html) {
                $('#gridContainer_testCapabilitiesWGGrid').html(html);
            },
            error: function () {
                console.error('Failed to load Test Capability grid.');
                showGovukAlert('Failed to load test capabilities. Please try again.');
            }
        });
    }

    function clearTestCapabilityGrid() {
        $('#tbl_' + testCapabilityGridId + ' tbody').html(
            '<tr><td colspan="100" class="govuk-table__cell" ' +
            'style="text-align:center;color:#505a5f;font-style:italic;padding:16px;">' +
            'Please select a WorkGroup to view test capabilities.</td></tr>'
        );
    }

    // ── Navigation ────────────────────────────────────────────────────────
    function navigateToTestCapability() {
        var workgroup = currentWorkGroup || $('#selectedWorkgroup').val();
        if (workgroup) {
            window.location.href = '/PACT/TestCapability?workgroup=' + encodeURIComponent(workgroup);
        } else {
            window.location.href = '/PACT/TestCapability';
        }
    }

    // ── Extra Filter Method (for pagination/sorting) ──────────────────────
    function getTestCapabilityExtraFilters() {
        return {
            workGroup: currentWorkGroup || ''
        };
    }

    // ── Row Selection Handler ─────────────────────────────────────────────
    function onTestCapabilityRowSelect(rowData) {
        // Extract portfolio value from the selected row
        var portfolio = $(rowData).closest('tr').find('[data-property="PlanPortfolio"]').text().trim();

        // Update the portfolio input field
        if (portfolio) {
            $('#selectedPortfolio').val(portfolio);
            // Enable the Project Administration button
            $('#btnShowProjectAdministration').prop('disabled', false);
        } else {
            // Disable the button if no portfolio selected
            $('#btnShowProjectAdministration').prop('disabled', true);
        }
    }

    // ── Navigate to Portfolio Maintenance ─────────────────────────────────
    function navigateToPortfolioMaintenance() {
        var portfolio = $('#selectedPortfolio').val();
        if (!portfolio) {
            showGovukAlert('Please select a test capability row first.');
            return;
        }
        // Navigate to Portfolio Maintenance with selected portfolio
        window.location.href = '/PACT/PortfolioMaintenance?portfolio=' + encodeURIComponent(portfolio);
    }

    // ── URL Parameter Helper ──────────────────────────────────────────────
    function getUrlParameter(name) {
        name = name.replace(/[\[]/, '\\[').replace(/[\]]/, '\\]');
        var regex = new RegExp('[\\?&]' + name + '=([^&#]*)');
        var results = regex.exec(location.search);
        return results === null ? '' : decodeURIComponent(results[1].replace(/\+/g, ' '));
    }

    // ── Public API ────────────────────────────────────────────────────────
    window.WorkGroupTestCapability = {
        initialize: initialize,
        onWorkGroupChange: onWorkGroupChange,
        reloadTestCapabilityGrid: reloadTestCapabilityGrid,
        clearTestCapabilityGrid: clearTestCapabilityGrid,
        navigateToTestCapability: navigateToTestCapability,
        navigateToPortfolioMaintenance: navigateToPortfolioMaintenance,
        getTestCapabilityExtraFilters: getTestCapabilityExtraFilters,
        onTestCapabilityRowSelect: onTestCapabilityRowSelect,
        getCapabilityGridManager: getCapabilityGridManager
    };

    // Expose individual functions to global scope for backward compatibility
    window.onWorkGroupChange = onWorkGroupChange;
    window.reloadTestCapabilityGrid = reloadTestCapabilityGrid;
    window.clearTestCapabilityGrid = clearTestCapabilityGrid;
    window.navigateToTestCapability = navigateToTestCapability;
    window.navigateToPortfolioMaintenance = navigateToPortfolioMaintenance;
    window.getTestCapabilityExtraFilters = getTestCapabilityExtraFilters;
    window.onTestCapabilityRowSelect = onTestCapabilityRowSelect;
    window.getCapabilityGridManager = getCapabilityGridManager;

})();

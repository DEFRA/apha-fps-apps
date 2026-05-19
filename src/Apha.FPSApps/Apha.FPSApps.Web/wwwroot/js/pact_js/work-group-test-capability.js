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
        window.location.href = '/PACT/TestCapability';
    }

    // ── Grid CRUD Functions ───────────────────────────────────────────────
    // These functions call the ORIGINAL TestCapability controller for CRUD operations
    // to avoid duplicating modal forms

    function addTestCapability() {
        $.get('/PACT/TestCapability/CreateTestCapability', function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        });
    }

    function editTestCapability(rowData) {
        var testCode = encodeURIComponent($(rowData).data('id') || '');
        var workGroup = encodeURIComponent(
            $(rowData).closest('tr').find('[data-property="WorkGroup"]').text().trim());
        $.get('/PACT/TestCapability/EditTestCapability?testCode=' + testCode + '&workGroup=' + workGroup,
            function (html) {
                $('#modaPopupBody').html(html);
                $('#modalPopup').addClass('show');
            });
    }

    function saveTestCapability() {
        clearValidationErrors('#modaPopupBody');
        var form = $('#testCapabilityForm');
        if (!isFormValid(form)) {
            displayClientValidationErrors(form, '#modaPopupBody');
            return;
        }
        var isEdit = form.find('[name="isEdit"]').val() === 'true';
        var data = {
            TestCode: form.find('[name="TestCode"]').val(),
            WorkGroup: form.find('[name="WorkGroup"]').val(),
            PlanPortfolio: form.find('[name="PlanPortfolio"]').val()
        };
        var url = isEdit
            ? '/PACT/TestCapability/EditTestCapability'
            : '/PACT/TestCapability/CreateTestCapability';
        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(data),
            success: function (result) {
                if (result.success) {
                    $('#modalPopup').removeClass('show');
                    showGovukAlert(isEdit ? 'Record updated successfully.' : 'Record saved successfully.');
                    reloadTestCapabilityGrid(currentWorkGroup);
                } else {
                    displayServerValidationErrors(result.errors, result.message, '#modaPopupBody');
                }
            },
            error: function () {
                showGovukAlert('Request failed. Please try again.');
            }
        });
    }

    function deleteTestCapability(rowData) {
        var testCode = encodeURIComponent($(rowData).data('id') || '');
        var workGroup = encodeURIComponent(
            $(rowData).closest('tr').find('[data-property="WorkGroup"]').text().trim());
        showGovukConfirm('Delete this Test Capability record?').then(function (confirmed) {
            if (!confirmed) return;
            $.ajax({
                url: '/PACT/TestCapability/DeleteTestCapability?testCode=' + testCode + '&workGroup=' + workGroup,
                type: 'DELETE',
                headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                success: function (result) {
                    if (result.success) {
                        reloadTestCapabilityGrid(currentWorkGroup);
                        showGovukAlert('Test Capability record deleted successfully.');
                    } else {
                        showGovukAlert(result.message ? result.message : 'Delete failed.');
                    }
                },
                error: function () {
                    showGovukAlert('An error occurred while deleting.');
                }
            });
        });
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
        }
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
        addTestCapability: addTestCapability,
        editTestCapability: editTestCapability,
        saveTestCapability: saveTestCapability,
        deleteTestCapability: deleteTestCapability,
        getTestCapabilityExtraFilters: getTestCapabilityExtraFilters,
        onTestCapabilityRowSelect: onTestCapabilityRowSelect,
        getCapabilityGridManager: getCapabilityGridManager
    };

    // Expose individual functions to global scope for backward compatibility
    window.onWorkGroupChange = onWorkGroupChange;
    window.reloadTestCapabilityGrid = reloadTestCapabilityGrid;
    window.clearTestCapabilityGrid = clearTestCapabilityGrid;
    window.navigateToTestCapability = navigateToTestCapability;
    window.addTestCapability = addTestCapability;
    window.editTestCapability = editTestCapability;
    window.saveTestCapability = saveTestCapability;
    window.deleteTestCapability = deleteTestCapability;
    window.getTestCapabilityExtraFilters = getTestCapabilityExtraFilters;
    window.onTestCapabilityRowSelect = onTestCapabilityRowSelect;
    window.getCapabilityGridManager = getCapabilityGridManager;

})();

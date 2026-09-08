// Invoice Recording Page JavaScript

// ── State ──────────────────────────────────────────────────────────
// Note: currentParentProject, currentMonth, and invoicesGridId 
// are initialized in the Razor view to avoid flicker
// DO NOT redeclare them here - they are set via inline script in Index.cshtml

function getInvoicesGridManager() {
    return window['gridManager_' + invoicesGridId];
}

let projectSelectDropdown = null;
// Guard to prevent the project dropdown callbacks from resetting the month
// selection while we are programmatically clearing it during a month change.
let isSyncingFilters = false;

$(document).ready(function () {
    initializeProjectMultiColumnDropdown();

    var preselected = (typeof preselectedProject !== 'undefined' && preselectedProject)
        ? String(preselectedProject)
        : '';
    if (preselected && projectSelectDropdown) {
        projectSelectDropdown.setValue(preselected);
    }
});

function initializeProjectMultiColumnDropdown() {
    projectSelectDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'projectSelectDropdown',
        containerSelector: '#projectMultiDropdown',
        placeholder: '-- All Projects --',
        showSerialNumber: false,
        searchPlaceholder: 'Search by project',
        labelText: '',
        columns: [
            { field: 'Text', header: 'Project', width: '300px' }
        ],
        data: (typeof projectOptionsListData !== 'undefined' ? projectOptionsListData : []),
        displayField: 'Text',
        valueField: 'Value',
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem, dropdown) {
                if (isSyncingFilters) return;
                $('#projectPick').val(selectedItem.Value);
                onProjectPickChange(selectedItem.Value);
            },
            onClear: function (dropdown) {
                if (isSyncingFilters) return;
                $('#projectPick').val('');
                onProjectPickChange('');
            }
        }
    });
}

// ── Project dropdown change ────────────────────────────────────────
function onProjectPickChange(value) {
    document.getElementById('monthPick').value = '';
    currentParentProject = value || null;
    currentMonth = null;
    reloadInvoicesGrid();
}

// ── Month dropdown change ──────────────────────────────────────────
function onMonthPickChange(value) {
    isSyncingFilters = true;
    $('#projectPick').val('');
    if (projectSelectDropdown && typeof projectSelectDropdown.clear === 'function') {
        projectSelectDropdown.clear();
    }
    isSyncingFilters = false;
    currentMonth = value || null;
    currentParentProject = null;
    reloadInvoicesGrid();
}

// ── Grid reload ────────────────────────────────────────────────────
function reloadInvoicesGrid() {
    $.ajax({
        url: '/PACT/Invoice/LoadInvoicesGrid',
        type: 'POST',
        data: {
            Page: 1,
            PageSize: 10,
            SortBy: 'Month',
            Descending: false,
            Filter: '{}',
            parentProject: currentParentProject || '',
            month: currentMonth || ''
        },
        success: function (html) {
            $('#gridContainer_invoicesGrid').html(html);
        },
        error: function () {
            console.error('Failed to load Invoices grid.');
        }
    });
}

// ── Extra filter method (passed to gridManager for pagination/sort) ─
function getInvoiceFilters() {
    return {
        parentProject: currentParentProject || '',
        month: currentMonth || ''
    };
}

// ── CRUD Functions ─────────────────────────────────────────────────
function addInvoice() {
    $.ajax({
        url: '/PACT/Invoice/GetInvoice',
        type: 'GET',
        data: { id: 0, parentProject: currentParentProject || '' },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            // Initialize form validation (unobtrusive + numeric)
            initializeFormValidation('#invoiceForm');
            initializeInvoiceProjectDropdown();
        },
        error: function () { showAlertMessage('An error occurred while loading the form.', AlertType.ERROR); }
    });
}

function editInvoice(btn) {
    var id = $(btn).data('id');
    $.ajax({
        url: '/PACT/Invoice/GetInvoice',
        type: 'GET',
        data: { id: id },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            // Initialize form validation (unobtrusive + numeric)
            initializeFormValidation('#invoiceForm');
            initializeInvoiceProjectDropdown();
        },
        error: function () { showAlertMessage('An error occurred while loading the form.', AlertType.ERROR); }
    });
}

function deleteInvoice(btn) {
    var id = $(btn).data('id');
    showGovukConfirm('Delete this invoice?').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/Invoice/DeleteInvoice',
            type: 'DELETE',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    reloadInvoicesGrid();
                    showAlertMessage('Invoice deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Error: ' + response.message, AlertType.ERROR);
                }
            },
            error: function () { showAlertMessage('An error occurred while deleting.', AlertType.ERROR); }
        });
    });
}

function saveInvoice() {
    clearValidationErrors('#modaPopupBody');
    var form = $('#invoiceForm');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }
    var data = form.serializeObject ? form.serializeObject() : Object.fromEntries(new FormData(form[0]));

    // Convert empty strings to null for numeric fields, but parse valid numbers
    ['Amount', 'CostOfWork', 'Wip', 'ProfitLoss'].forEach(function (f) {
        if (data[f] === '' || data[f] === undefined) {
            data[f] = null;
        } else if (typeof data[f] === 'string') {
            var parsed = parseFloat(data[f]);
            data[f] = isNaN(parsed) ? null : parsed;
        }
    });

    // Parse Month as integer
    if (data['Month'] === '' || data['Month'] === undefined) {
        data['Month'] = null;
    }

    $.ajax({
        url: '/PACT/Invoice/SaveInvoice',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                showAlertMessage(response.message || 'Invoice saved successfully.', AlertType.SUCCESS);
                reloadInvoicesGrid();
            } else {
                displayServerValidationErrors(response.errors, response.message, '#modaPopupBody');
                // Initialize form validation (unobtrusive + numeric)
                initializeFormValidation('#modaPopupBody');
            }
        },
        error: function () { 
            showAlertMessage('An error occurred while saving.', AlertType.ERROR); 
        }
    });
}

// ── Search (if needed) ─────────────────────────────────────────────
function filterInvoicesGrid(input) {
    var gm = getInvoicesGridManager();
    if (gm) gm.reloadGrid({ page: 1, search: input.value });
}

// ========================================
// Multi-Column Dropdown for Invoice Modal
// ========================================
function initializeInvoiceProjectDropdown() {
    var container = document.querySelector('#invoiceProjectMultiDropdown');
    if (!container || typeof MultiColumnDropdownComponent === 'undefined') {
        return;
    }

    var projectsData = [];
    var raw = container.getAttribute('data-projects');
    if (raw) {
        try { projectsData = JSON.parse(raw); } catch (e) { projectsData = []; }
    }
    var selectedProject = container.getAttribute('data-selected') || '';

    setTimeout(function () {
        var projectDropdown = new MultiColumnDropdownComponent({
            dropdownId: 'invoiceProjectDropdown',
            containerSelector: '#invoiceProjectMultiDropdown',
            placeholder: 'Select Project',
            showSerialNumber: false,
            searchPlaceholder: 'Search by project',
            labelText: '',
            required: true,
            columns: [
                { field: 'Text', header: 'Project', width: '300px' }
            ],
            data: projectsData || [],
            displayField: 'Text',
            valueField: 'Value',
            clearButtonClearsSelection: true,
            callbacks: {
                onSelect: function (selectedItem, dropdown) {
                    $('#ProjectParent').val(selectedItem.Value).trigger('change');
                    setTimeout(function () {
                        if (dropdown && typeof dropdown.closeDropdown === 'function') {
                            dropdown.closeDropdown();
                        }
                    }, 50);
                },
                onClear: function (dropdown) {
                    $('#ProjectParent').val('').trigger('change');
                }
            }
        });

        var initialProject = selectedProject || $('#ProjectParent').val();
        if (initialProject) {
            projectDropdown.setValue(initialProject);
        }
    }, 100);
}
window.initializeInvoiceProjectDropdown = initializeInvoiceProjectDropdown;

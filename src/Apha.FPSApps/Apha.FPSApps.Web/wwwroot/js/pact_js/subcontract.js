// SubContract Page JavaScript

// ── State ──────────────────────────────────────────────────────────
// Note: currentParentProject, currentMonth, and subContractsGridId 
// are initialized in the Razor view to avoid flicker
// DO NOT redeclare them here - they are set via inline script in Index.cshtml

function getSubContractsGridManager() {
    return window['gridManager_' + subContractsGridId];
}

// ── Project dropdown change ────────────────────────────────────────
function onProjectPickChange(value) {
    document.getElementById('monthPick').value = '';
    currentParentProject = value || null;
    currentMonth = null;
    reloadSubContractsGrid();
}

// ── Month dropdown change ──────────────────────────────────────────
function onMonthPickChange(value) {
    document.getElementById('projectPick').value = '';
    currentMonth = value ? parseInt(value) : null;
    currentParentProject = null;
    reloadSubContractsGrid();
}

// ── Grid reload ────────────────────────────────────────────────────
function reloadSubContractsGrid() {
    var postData = {
        Page: 1,
        PageSize: 50,
        SortBy: 'Month',
        Descending: false,
        Filter: '{}',
        parentProject: currentParentProject || ''
    };

    // Only add month if it has a value
    if (currentMonth) {
        postData.month = currentMonth;
    }

    $.ajax({
        url: '/PACT/SubContract/LoadSubContractsGrid',
        type: 'POST',
        data: postData,
        success: function (html) {
            $('#gridContainer_subContractsGrid').html(html);
        },
        error: function () {
            console.error('Failed to load SubContracts grid.');
        }
    });
}

// ── Extra filter method (passed to gridManager for pagination/sort) ─
function getSubContractFilters() {
    return {
        parentProject: currentParentProject || '',
        month: currentMonth || ''
    };
}

// ── CRUD Functions ─────────────────────────────────────────────────
function addSubContract() {
    $.get('/PACT/SubContract/GetSubContract',
        { id: 0, parentProject: currentParentProject || '' },
        function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        })
        .fail(function(xhr, status, error) {
            alert('Error loading form: ' + error);
        });
}

function editSubContract(btn) {
    var id = $(btn).data('id');
    $.get('/PACT/SubContract/GetSubContract', { id: id }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    })
    .fail(function(xhr, status, error) {
        alert('Error loading form: ' + error);
    });
}

function deleteSubContract(btn) {
    var id = $(btn).data('id');
    showGovukConfirm('Delete this subcontract?').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/SubContract/DeleteSubContract',
            type: 'DELETE',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    reloadSubContractsGrid();
                    showGovukAlert('SubContract deleted successfully.');
                } else {
                    showGovukAlert('Error: ' + response.message);
                }
            },
            error: function () { showGovukAlert('An error occurred while deleting.'); }
        });
    });
}

function saveSubContract() {
    clearValidationErrors('#modaPopupBody');
    var form = $('#subContractForm');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }
    var data = form.serializeObject ? form.serializeObject() : Object.fromEntries(new FormData(form[0]));

    ['Month', 'Amount', 'SupplierNumber'].forEach(function (f) {
        if (data[f] === '' || data[f] === undefined) data[f] = null;
    });

    $.ajax({
        url: '/PACT/SubContract/SaveSubContract',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                showGovukAlert(response.message || 'SubContract saved successfully.');
                reloadSubContractsGrid();
            } else {
                displayServerValidationErrors(response.errors, response.message, '#modaPopupBody');
            }
        },
        error: function () { 
            showGovukAlert('An error occurred while saving.'); 
        }
    });
}

// ── Search (if needed) ─────────────────────────────────────────────
function filterSubContractsGrid(input) {
    var gm = getSubContractsGridManager();
    if (gm) gm.reloadGrid({ page: 1, search: input.value });
}

// Invoice Recording Page JavaScript

// ── State ──────────────────────────────────────────────────────────
var currentParentProject = null;
var currentMonth = null;
var invoicesGridId = '';

function getInvoicesGridManager() {
    return window['gridManager_' + invoicesGridId];
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
    document.getElementById('projectPick').value = '';
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
            PageSize: 50,
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
    $.get('/PACT/Invoice/GetInvoice',
        { id: 0, parentProject: currentParentProject || '' },
        function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        })
        .fail(function(xhr, status, error) {
            alert('Error loading form: ' + error);
        });
}

function editInvoice(btn) {
    var id = $(btn).data('id');
    $.get('/PACT/Invoice/GetInvoice', { id: id }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    })
    .fail(function(xhr, status, error) {
        alert('Error loading form: ' + error);
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
                    showGovukAlert('Invoice deleted successfully.');
                } else {
                    showGovukAlert('Error: ' + response.message);
                }
            },
            error: function () { showGovukAlert('An error occurred while deleting.'); }
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

    ['Month', 'Amount', 'CostOfWork', 'Wip', 'ProfitLoss'].forEach(function (f) {
        if (data[f] === '' || data[f] === undefined) data[f] = null;
    });

    $.ajax({
        url: '/PACT/Invoice/SaveInvoice',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                showGovukAlert(response.message || 'Invoice saved successfully.');
                reloadInvoicesGrid();
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
function filterInvoicesGrid(input) {
    var gm = getInvoicesGridManager();
    if (gm) gm.reloadGrid({ page: 1, search: input.value });
}

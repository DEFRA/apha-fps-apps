// Invoice Recording Page JavaScript

var invoicesGridId;
var parentProject;
var currentMonth;

function initializeInvoiceRecording(gridId, project, month) {
    invoicesGridId = gridId;
    parentProject = project;
    currentMonth = month;
}

function applyFilters() {
    var project = document.getElementById('projectPick').value.trim();
    var month = document.getElementById('monthPick').value;

    var url = '/PACT/InvoiceRecording/Index';
    var params = [];
    if (project) params.push('parentProject=' + encodeURIComponent(project));
    if (month) params.push('month=' + month);
    if (params.length > 0) url += '?' + params.join('&');

    fpsNavigateTo(url);
}

function getInvoicesGridManager() {
    return window['gridManager_' + invoicesGridId];
}

function addInvoice() {
    $.get('/PACT/InvoiceRecording/GetInvoice',
        { id: 0, parentProject: decodeURIComponent(parentProject) },
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
    $.get('/PACT/InvoiceRecording/GetInvoice', { id: id }, function (html) {
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
            url: '/PACT/InvoiceRecording/DeleteInvoice',
            type: 'DELETE',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    getInvoicesGridManager()?.reloadGrid({ page: 1 });
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
    var form = $('#invoiceForm');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#invoiceForm');
        return;
    }
    var data = form.serializeObject ? form.serializeObject() : Object.fromEntries(new FormData(form[0]));

    ['Month', 'Amount', 'CostOfWork', 'Wip', 'ProfitLoss'].forEach(function (f) {
        if (data[f] === '' || data[f] === undefined) data[f] = null;
    });

    clearValidationErrors('#invoiceForm');
    $.ajax({
        url: '/PACT/InvoiceRecording/SaveInvoice',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                alert(response.message);
                $('#modalPopup').removeClass('show');
                getInvoicesGridManager()?.reloadGrid({ page: 1 });
            } else {
                displayServerValidationErrors(response.errors, response.message, '#invoiceForm');
            }
        },
        error: function () { alert('An error occurred while saving.'); }
    });
}

// ── Search ───────────────────────────────────────────────────────

function filterInvoicesGrid(input) {
    var gm = getInvoicesGridManager();
    if (gm) gm.reloadGrid({ page: 1, search: input.value });
}

// Initialize on document ready
$(document).ready(function () {
    if (typeof invoicesGridId !== 'undefined') {
        initializeInvoiceRecording(invoicesGridId, parentProject, currentMonth);
    }
});

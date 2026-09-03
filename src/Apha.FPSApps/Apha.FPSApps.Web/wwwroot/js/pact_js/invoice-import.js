// Invoice Import - Failed Records, Template, Import, Export, Delete All

function getFailedInvoicesGridManager() {
    return window['gridManager_' + failedInvoicesGridId];
}

function reloadFailedInvoicesGrid() {
    const gridManager = getFailedInvoicesGridManager();
    if (gridManager) {
        gridManager.reloadGrid({
            page: 1,
            sortBy: 'Id',
            descending: false
        });
    }
}

function downloadInvoiceImportTemplate() {
    const downloadUrl = '/PACT/InvoiceImport/DownloadTemplate';

    $.ajax({
        url: downloadUrl,
        type: 'GET',
        xhrFields: {
            responseType: 'blob'
        },
        success: function (blob, status, xhr) {
            const disposition = xhr.getResponseHeader('Content-Disposition') || '';
            const fileNameMatch = disposition.match(/filename\*=UTF-8''([^;]+)|filename="?([^";]+)"?/i);
            const fileName = decodeURIComponent(fileNameMatch?.[1] || fileNameMatch?.[2] || 'InvoiceImport-Template.xlsx');

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);

            showAlertMessage('Template downloaded successfully. Please use the template to import Invoice data.', AlertType.INFO);
        },
        error: function () {
            showAlertMessage('Template download failed. Please try again.', AlertType.ERROR);
        }
    });
}

function importInvoice(file) {
    if (!file) {
        showAlertMessage('Please select an Excel file to import.', AlertType.INFO);
        return;
    }

    const formData = new FormData();
    formData.append('file', file);

    $.ajax({
        url: '/PACT/InvoiceImport/Import',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                var msg = response.message || 'Import completed.';
                showAlertMessage(msg, AlertType.SUCCESS);
                reloadInvoicesGrid();
                reloadFailedInvoicesGrid();
            } else {
                showAlertMessage(response.message || 'Import failed.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred during import.', AlertType.ERROR);
        }
    });
}

function deleteAllFailedInvoiceImport() {
    showGovukConfirm('Delete all failed records?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/InvoiceImport/DeleteAllFailedInvoiceImport',
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    reloadFailedInvoicesGrid();
                    showAlertMessage('All failed records deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage(response.message || 'Failed to delete records.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting.', AlertType.ERROR);
            }
        });
    });
}

function editFailedInvoiceImport(btn) {
    const id = $(btn).data('id');

    $.ajax({
        url: '/PACT/InvoiceImport/GetFailedInvoiceImport',
        type: 'GET',
        data: { id: id },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            initializeFormValidation('#formEditFailedInvoice');
        },
        error: function () {
            showAlertMessage('Error loading form.', AlertType.ERROR);
        }
    });
}

function deleteFailedInvoiceImport(btn) {
    const id = $(btn).data('id');
    showGovukConfirm('Delete this failed record?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/InvoiceImport/DeleteFailedInvoiceImport',
            type: 'DELETE',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    reloadFailedInvoicesGrid();
                    showAlertMessage('Failed record deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Error: ' + response.message, AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting.', AlertType.ERROR);
            }
        });
    });
}

function saveFailedInvoice() {
    const form = $('#formEditFailedInvoice');

    initializeFormValidation('#formEditFailedInvoice');
    clearValidationErrors('#modaPopupBody');

    // Run unobtrusive validation rules first (regex/range/custom)
    if (typeof form.valid === 'function' && !form.valid()) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    // Fallback required-fields check
    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    const data = form.serializeObject ? form.serializeObject() : Object.fromEntries(new FormData(form[0]));

    $.ajax({
        url: '/PACT/InvoiceImport/SaveFailedInvoiceImport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                showAlertMessage(response.message || 'Record saved successfully.', AlertType.SUCCESS);
                reloadFailedInvoicesGrid();
                if (response.movedToInvoice) {
                    reloadInvoicesGrid();
                }
            } else {
                displayServerValidationErrors(response.errors, response.message, '#modaPopupBody');
                initializeFormValidation('#formEditFailedInvoice');
            }
        },
        error: function () {
            showAlertMessage('An error occurred while saving.', AlertType.ERROR);
        }
    });
}

function exportFailedInvoiceImport() {
    const downloadUrl = '/PACT/InvoiceImport/ExportFailedInvoiceImport';

    $.ajax({
        url: downloadUrl,
        type: 'GET',
        xhrFields: {
            responseType: 'blob'
        },
        success: function (blob, status, xhr) {
            const disposition = xhr.getResponseHeader('Content-Disposition') || '';
            const fileNameMatch = disposition.match(/filename\*=UTF-8''([^;]+)|filename="?([^";]+)"?/i);
            const fileName = decodeURIComponent(fileNameMatch?.[1] || fileNameMatch?.[2] || 'ExportedInvoiceImport.xlsx');

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);

            showAlertMessage('Failed records exported successfully.', AlertType.SUCCESS);
        },
        error: function () {
            showAlertMessage('Export failed. Please try again.', AlertType.ERROR);
        }
    });
}

// ── Event Handlers ─────────────────────────────────────────────────
$(document).ready(function () {
    $('#templateExcel').on('click', function () {
        downloadInvoiceImportTemplate();
    });

    $('#importBtn').on('click', function () {
        $('#csvInput').click();
    });

    $('#csvInput').on('change', function () {
        var file = this.files[0];
        if (file) {
            importInvoice(file);
            $(this).val('');
        }
    });

    $('#exportFailedBtn').on('click', function () {
        exportFailedInvoiceImport();
    });

    $('#deleteAllFailedBtn').on('click', function () {
        deleteAllFailedInvoiceImport();
    });
});

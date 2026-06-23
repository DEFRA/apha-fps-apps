let currentRmsMonth = initialRmsMonth ? parseInt(initialRmsMonth, 10) : null;

function getRmsSubContractsGridManager() {
    return window['gridManager_' + rmsSubContractsGridId];
}

function getRmsFailedSubContractsGridManager() {
    return window['gridManager_' + rmsFailedSubContractsGridId];
}

function getRmsSubContractFilters() {
    return {
        month: currentRmsMonth || ''
    };
}

function updateSelectedMonthText() {
    document.getElementById('txtSelectedMonth').value = document.getElementById('dpSelectmonth').value;
}

function reloadRmsGrid() {
    const gridManager = getRmsSubContractsGridManager();
    if (gridManager) {
        gridManager.reloadGrid({
            page: 1,
            sortBy: 'Project',
            descending: false
        });
    }
}

function reloadFailedGrid() {
    const gridManager = getRmsFailedSubContractsGridManager();
    if (gridManager) {
        gridManager.reloadGrid({
            page: 1,
            sortBy: 'Id',
            descending: false
        });
    }
}

function addSubContractRms() {
    if (!currentRmsMonth) {
        showGovukAlert('Please select a period first.');
        return;
    }

    $.get('/PACT/SubContractRms/GetSubContractRms', { id: 0, month: currentRmsMonth }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    }).fail(function () {
        showGovukAlert('Error loading form.');
    });
}

function editSubContractRms(btn) {
    const id = $(btn).data('id');
    $.get('/PACT/SubContractRms/GetSubContractRms', { id: id, month: currentRmsMonth }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    }).fail(function () {
        showGovukAlert('Error loading form.');
    });
}

function deleteSubContractRms(btn) {
    const id = $(btn).data('id');
    showGovukConfirm('Delete this subcontract?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/SubContractRms/DeleteSubContractRms',
            type: 'DELETE',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    reloadRmsGrid();
                    showGovukAlert('SubContract deleted successfully.');
                } else {
                    showGovukAlert('Error: ' + response.message);
                }
            },
            error: function () {
                showGovukAlert('An error occurred while deleting.');
            }
        });
    });
}

function saveProjectCost() {
    clearValidationErrors('#modaPopupBody');
    const form = $('#formAddProjectCost');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    const data = form.serializeObject ? form.serializeObject() : Object.fromEntries(new FormData(form[0]));

    ['Month', 'Amount', 'SupplierNumber', 'DailyRate', 'AnimalDays'].forEach(function (field) {
        if (data[field] === '' || data[field] === undefined) {
            data[field] = null;
        }
    });

    $.ajax({
        url: '/PACT/SubContractRms/SaveSubContractRms',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                showGovukAlert(response.message || 'SubContract saved successfully.');
                reloadRmsGrid();
            } else {
                displayServerValidationErrors(response.errors, response.message, '#modaPopupBody');
            }
        },
        error: function () {
            showGovukAlert('An error occurred while saving.');
        }
    });
}

function downloadSubContractRmsTemplate() {
    const downloadUrl = '/PACT/SubContractRms/DownloadTemplate';

    $.ajax({
        url: downloadUrl,
        type: 'GET',
        xhrFields: {
            responseType: 'blob'
        },
        success: function (blob, status, xhr) {
            const disposition = xhr.getResponseHeader('Content-Disposition') || '';
            const fileNameMatch = disposition.match(/filename\*=UTF-8''([^;]+)|filename="?([^";]+)"?/i);
            const fileName = decodeURIComponent(fileNameMatch?.[1] || fileNameMatch?.[2] || 'SubContractRMS-Template.xlsx');

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);

            showGovukAlert('Template downloaded successfully. Please use the template to import Sub-Contract RMS data.');
        },
        error: function () {
            showGovukAlert('Template download failed. Please try again.');
        }
    });
}

function importSubContractRms(file) {
    if (!file) {
        showGovukAlert('Please select an Excel file to import.');
        return;
    }

    const formData = new FormData();
    formData.append('file', file);

    $.ajax({
        url: '/PACT/SubContractRms/Import',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                const msg = response.message || ('Import completed successfully. ' + (response.passedCount || 0) + 'records successfully validated and is now live');
                showGovukAlert(msg);
                reloadRmsGrid();
                reloadFailedGrid();
            } else {
                showGovukAlert(response.message || 'Import failed.');
            }
        },
        error: function () {
            showGovukAlert('An error occurred while importing file.');
        }
    });
}

function deleteAllFailedSubContractRms() {
    showGovukConfirm('Delete all failed records?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/SubContractRms/DeleteAllFailedSubContractRms',
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    reloadFailedGrid();
                    showGovukAlert('Failed records deleted successfully.');
                } else {
                    showGovukAlert(response.message || 'Failed to delete failed records.');
                }
            },
            error: function () {
                showGovukAlert('An error occurred while deleting failed records.');
            }
        });
    });
}

function editFailedSubContractRms(btn) {
    const id = $(btn).data('id');
    $.get('/PACT/SubContractRms/GetFailedSubContractRms', { id: id }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    }).fail(function () {
        showGovukAlert('Error loading form.');
    });
}

function deleteFailedSubContractRms(btn) {
    const id = $(btn).data('id');
    showGovukConfirm('Delete this failed record?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/SubContractRms/DeleteFailedSubContractRms',
            type: 'DELETE',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    reloadFailedGrid();
                    showGovukAlert('Failed record deleted successfully.');
                } else {
                    showGovukAlert('Error: ' + response.message);
                }
            },
            error: function () {
                showGovukAlert('An error occurred while deleting.');
            }
        });
    });
}

function saveFailedSubContractRms() {
    clearValidationErrors('#modaPopupBody');
    const form = $('#formEditFailedSubContractRms');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    const data = form.serializeObject ? form.serializeObject() : Object.fromEntries(new FormData(form[0]));

    // Convert empty strings to null for optional numeric fields
    ['SupplierNumber', 'DailyRate', 'AnimalDays'].forEach(function (field) {
        if (data[field] === '' || data[field] === undefined) {
            data[field] = null;
        }
    });

    $.ajax({
        url: '/PACT/SubContractRms/SaveFailedSubContractRms',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                showGovukAlert(response.message || 'Failed record saved successfully.');
                reloadFailedGrid();
                if (response.movedToSubContract) {
                    reloadRmsGrid();
                }
            } else {
                displayServerValidationErrors(response.errors, response.message, '#modaPopupBody');
            }
        },
        error: function () {
            showGovukAlert('An error occurred while saving.');
        }
    });
}

function exportFailedSubContractRms() {
    const gridManager = getRmsFailedSubContractsGridManager();
    const exportUrl = '/PACT/SubContractRms/ExportFailedSubContractRms';

    if (!gridManager) {
        showGovukAlert('Failed to export: grid manager not found.');
        return;
    }

    var params = {
        filter: JSON.stringify(gridManager.getFilterModel())
    };

    var query = Object.keys(params)
        .map(function (k) { return encodeURIComponent(k) + '=' + encodeURIComponent(params[k]); })
        .join('&');

    window.location.href = exportUrl + '?' + query;
}

$(document).ready(function () {
    updateSelectedMonthText();

    $('#dpSelectmonth').on('change', function () {
        const value = this.value;
        currentRmsMonth = value ? parseInt(value, 10) : null;
        updateSelectedMonthText();
        reloadRmsGrid();
    });

    $('#templateExcel').on('click', function (e) {
        e.preventDefault();
        downloadSubContractRmsTemplate();
    });

    $('#csvInput').on('change', function () {
        const file = this.files && this.files[0];
        if (!file) return;

        importSubContractRms(file);
        $(this).val('');
    });

    $('#exportFailedBtn').on('click', function (e) {
        e.preventDefault();
        exportFailedSubContractRms();
    });

    $('#deleteAllFailedBtn').on('click', function (e) {
        e.preventDefault();
        deleteAllFailedSubContractRms();
    });
});

window.getRmsSubContractFilters = getRmsSubContractFilters;
window.addSubContractRms = addSubContractRms;
window.editSubContractRms = editSubContractRms;
window.deleteSubContractRms = deleteSubContractRms;
window.saveProjectCost = saveProjectCost;
window.downloadSubContractRmsTemplate = downloadSubContractRmsTemplate;
window.importSubContractRms = importSubContractRms;
window.deleteAllFailedSubContractRms = deleteAllFailedSubContractRms;
window.editFailedSubContractRms = editFailedSubContractRms;
window.deleteFailedSubContractRms = deleteFailedSubContractRms;
window.saveFailedSubContractRms = saveFailedSubContractRms;
window.exportFailedSubContractRms = exportFailedSubContractRms;

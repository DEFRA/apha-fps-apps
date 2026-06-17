let currentRmsMonth = initialRmsMonth ? parseInt(initialRmsMonth, 10) : null;

function getRmsSubContractsGridManager() {
    return window['gridManager_' + rmsSubContractsGridId];
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
                const msg = response.message || ('Import completed. Passed: ' + (response.passedCount || 0));
                showGovukAlert(msg);
                reloadRmsGrid();
            } else {
                showGovukAlert(response.message || 'Import failed.');
            }
        },
        error: function () {
            showGovukAlert('An error occurred while importing file.');
        }
    });
}

function openFailedSubContractRmsPopup() {
    $.get('/PACT/SubContractRms/ViewFailedSubContractRms', function (html) {
        $('#modaPopupBody').html(html);        
        $('#modalPopup').addClass('show');
    }).fail(function () {
        showGovukAlert('Failed to load failed records.');
    });
}

function deleteAllFailedSubContractRms() {
    showGovukConfirm('Delete all failed records for current user?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/SubContractRms/DeleteAllFailedSubContractRms',
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    var manager = window['gridManager_rmsFailedSubContractsGrid'];
                    if (manager) {
                        manager.reloadGrid({ page: 1, sortBy: 'Id', descending: false });
                    }
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

    $('#viewFailedBtn').on('click', function (e) {
        e.preventDefault();
        openFailedSubContractRmsPopup();
    });
});

window.getRmsSubContractFilters = getRmsSubContractFilters;
window.addSubContractRms = addSubContractRms;
window.editSubContractRms = editSubContractRms;
window.deleteSubContractRms = deleteSubContractRms;
window.saveProjectCost = saveProjectCost;
window.downloadSubContractRmsTemplate = downloadSubContractRmsTemplate;
window.importSubContractRms = importSubContractRms;
window.openFailedSubContractRmsPopup = openFailedSubContractRmsPopup;
window.deleteAllFailedSubContractRms = deleteAllFailedSubContractRms;

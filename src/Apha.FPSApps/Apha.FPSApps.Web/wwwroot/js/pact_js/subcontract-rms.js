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
    const monthSelect = document.getElementById('dpSelectmonth');
    const selectedText = monthSelect && monthSelect.selectedIndex > 0
        ? monthSelect.options[monthSelect.selectedIndex].text
        : '';

    document.getElementById('txtSelectedMonth').value = selectedText;
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

$(document).ready(function () {
    updateSelectedMonthText();

    $('#dpSelectmonth').on('change', function () {
        const value = this.value;
        currentRmsMonth = value ? parseInt(value, 10) : null;
        updateSelectedMonthText();
        reloadRmsGrid();
    });
});

window.getRmsSubContractFilters = getRmsSubContractFilters;
window.addSubContractRms = addSubContractRms;
window.editSubContractRms = editSubContractRms;
window.deleteSubContractRms = deleteSubContractRms;
window.saveProjectCost = saveProjectCost;

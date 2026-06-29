let currentFilterType = 'all';
let accountCategoryGrid = null;

// Wait for grid to be initialized
document.addEventListener('DOMContentLoaded', function() {
    accountCategoryGrid = window['gridManager_accountCategoryGrid'];
});

function filterAccountCategories(radio) {
    currentFilterType = radio.value;
    if (accountCategoryGrid) {
        accountCategoryGrid.reloadGrid({ page: 1 });
    }
}

function getAccountCategoryExtraFilters() {
    return {
        filterType: currentFilterType
    };
}

function addAccountCategory() {
    $.ajax({
        url: '/FPS/AccountCategoryMaintenance/Create',
        type: 'GET',
        success: function (data) {
            $('#accountCategoryModalContent').html(data);
            $('#accountCategoryModal').modal('show');
            // Enable unobtrusive validation on the new form
            var form = $('#addAccountCategoryForm');
            $.validator.unobtrusive.parse(form);
        },
        error: function () {
            showGovukAlert('Failed to load form');
        }
    });
}

function editAccountCategory(btn) {
    var accShortName = $(btn).data('id');
    $.ajax({
        url: '/FPS/AccountCategoryMaintenance/Edit',
        type: 'GET',
        data: { accShortName: accShortName },
        success: function (data) {
            $('#accountCategoryModalContent').html(data);
            $('#accountCategoryModal').modal('show');
            // Enable unobtrusive validation on the new form
            var form = $('#editAccountCategoryForm');
            $.validator.unobtrusive.parse(form);
        },
        error: function () {
            showGovukAlert('Failed to load form');
        }
    });
}

function deleteAccountCategory(btn) {
    var accShortName = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this account category?').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/FPS/AccountCategoryMaintenance/Delete',
            type: 'DELETE',
            data: { accShortName: accShortName },
            success: function (response) {
                if (response.success) {
                    if (accountCategoryGrid) {
                        accountCategoryGrid.reloadGrid({ page: 1 });
                    }
                    showGovukAlert(response.message || 'Account category deleted successfully.');
                } else {
                    showGovukAlert('Error: ' + (response.message || 'Failed to delete account category.'));
                }
            },
            error: function () {
                showGovukAlert('An error occurred while deleting the account category.');
            }
        });
    });
}

function saveAccountCategory(isEdit = false) {
    clearValidationErrors('#accountCategoryModalContent');
    var form = isEdit ? $('#editAccountCategoryForm') : $('#addAccountCategoryForm');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#accountCategoryModalContent');
        return;
    }

    // Validate AccountType - must be 'Pay' or 'NPRC'
    const accountType = $('#AccountType').val().trim();
    if (accountType !== 'Pay' && accountType !== 'NPRC') {
        const validationErrors = [{
            field: 'AccountType',
            message: 'AccountType must be either "Pay" or "NPRC".'
        }];
        displayServerValidationErrors(validationErrors, 'Please correct the following error:', '#accountCategoryModalContent');
        return;
    }

    const formData = {
        AccShortName: isEdit ? $('#originalAccShortName').val() : $('#AccShortName').val(),
        AccountDescription: $('#AccountDescription').val(),
        ConstituentAccountCodes: $('#ConstituentAccountCodes').val(),
        AccountType: accountType,
        ProjectSpecific: $('#ProjectSpecific').is(':checked') ? -1 : 0,
        RcSpecific: $('#RcSpecific').is(':checked') ? -1 : 0
    };

    let url = isEdit ? '/FPS/AccountCategoryMaintenance/Edit' : '/FPS/AccountCategoryMaintenance/Create';

    // For edit, append the original AccShortName as query parameter
    if (isEdit) {
        const originalAccShortName = $('#originalAccShortName').val();
        if (originalAccShortName) {
            url += '?originalAccShortName=' + encodeURIComponent(originalAccShortName);
        }
    }

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(formData),
        success: function (response) {
            if (response.success) {
                $('#accountCategoryModal').modal('hide');
                if (accountCategoryGrid) {
                    accountCategoryGrid.reloadGrid({ page: 1 });
                }
                showGovukAlert(response.message || (isEdit ? 'Account category updated successfully.' : 'Account category added successfully.'));
            } else {
                displayServerValidationErrors(response.errors, response.message, '#accountCategoryModalContent');
            }
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#accountCategoryModalContent');
            } else {
                showGovukAlert('An error occurred while saving the account category.');
            }
        }
    });
}

function closeAccountCategoryModal() {
    clearValidationErrors('#accountCategoryModalContent');
    $('#accountCategoryModal').modal('hide');
}

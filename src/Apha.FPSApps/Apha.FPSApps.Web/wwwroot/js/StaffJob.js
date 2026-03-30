// StaffJob.js - Shared staff job CRUD, charge rate calculation, and modal/validation helpers.
// Each page must configure StaffJobConfig before this script runs its event bindings.

var StaffJobConfig = {
    getJobCode: function () { return ''; },
    requireJobCodeForAdd: false,
    onSaved: function () { window.location.reload(); },
    onUpdated: function () { window.location.reload(); },
    onDeleted: function () { window.location.reload(); }
};

// ---- Staff Job CRUD ----

function addStaffJob(btn) {
    if (StaffJobConfig.requireJobCodeForAdd && !StaffJobConfig.getJobCode()) {
        alert('Please select a project first.');
        return;
    }
    $.ajax({
        url: '/FPS/StaffJob/Create',
        type: 'GET',
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message);
            } else {
                alert('An error occurred while opening the form.');
            }
        }
    });
}

function saveStaffJob() {
    var form = $('#formAddStaff');
    if (!isFormValid(form)) {
        displayClientValidationErrors(form);
        return;
    }
    var staffId = $('#StaffID').val();
    var staffName = $('#StaffDropdown option:selected').text();
    var data = {
        StaffID: staffId,
        JobCode: StaffJobConfig.getJobCode(),
        Name: staffName,
        ChargeRate: parseFloat($('#ChargeRate').val()) || 0,
        PlannedHours: parseFloat($('#PlannedHours').val()) || 0,
        Days: parseFloat($('#Days').val()) || 0,
        StaffCost: parseFloat($('#StaffCost').val()) || 0
    };
    $.ajax({
        url: '/FPS/StaffJob/Create',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                alert(result.message);
                closeModal();
                StaffJobConfig.onSaved();
            } else {
                displayServerValidationErrors(result.errors, result.message);
            }
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message);
            } else {
                alert('An error occurred while saving.');
            }
        }
    });
}

function editStaffJob(btn) {
    var staffJobId = $(btn).data('id');
    $.ajax({
        url: '/FPS/StaffJob/Edit',
        type: 'GET',
        data: { staffId: staffJobId, jobCode: StaffJobConfig.getJobCode() },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message);
            } else {
                alert('An error occurred while fetching the record.');
            }
        }
    });
}

function updateStaffJob() {
    var form = $('#formEditStaff');
    if (!isFormValid(form)) {
        displayClientValidationErrors(form);
        return;
    }
    var staffId = $('#StaffID').val();
    var jobCode = form.find('[name="JobCode"]').val();
    var staffName = $('#StaffDropdown option:selected').text();
    var data = {
        StaffID: staffId,
        JobCode: jobCode,
        Name: staffName,
        ChargeRate: parseFloat($('#ChargeRate').val()) || 0,
        PlannedHours: parseFloat($('#PlannedHours').val()) || 0,
        Days: parseFloat($('#Days').val()) || 0,
        StaffCost: parseFloat($('#StaffCost').val()) || 0
    };
    $.ajax({
        url: '/FPS/StaffJob/Edit?staffId=' + staffId,
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                alert(result.message);
                closeModal();
                StaffJobConfig.onUpdated();
            } else {
                displayServerValidationErrors(result.errors, result.message);
            }
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message);
            } else {
                alert('An error occurred while saving.');
            }
        }
    });
}

function deleteStaffJob(btn) {
    var staffJobId = $(btn).data('id');
    if (confirm('Are you sure you want to delete this record?')) {
        $.ajax({
            url: '/FPS/StaffJob/Delete',
            type: 'DELETE',
            data: { staffId: staffJobId, jobCode: StaffJobConfig.getJobCode() },
            success: function (response) {
                if (response.success) {
                    alert('Deleted successfully.');
                    StaffJobConfig.onDeleted();
                } else {
                    alert('Error: ' + response.message);
                }
            },
            error: function () {
                alert('An error occurred while deleting.');
            }
        });
    }
}

function getStaffJobExtraFilters() {
    return { jobCode: StaffJobConfig.getJobCode() };
}

// ---- Charge rate calculation ----

function onStaffSelected(selectElement) {
    var staffId = $(selectElement).val();
    var staffName = $(selectElement).find('option:selected').data('name');
    $('#StaffID').val(staffId);
    if ($('#Name').length) {
        $('#Name').val(staffName);
    }
    if (staffId) {
        fetchChargeRate(staffId);
    }
}

function fetchChargeRate(staffId) {
    var jobCode = StaffJobConfig.getJobCode();
    if (!staffId || !jobCode) { return; }
    var chargeRateField = $('#ChargeRate');
    chargeRateField.prop('disabled', true).val('');
    $.ajax({
        url: '/FPS/StaffJob/GetChargeRate',
        type: 'GET',
        data: { staffId: staffId, jobCode: jobCode },
        success: function (result) {
            chargeRateField.prop('disabled', false);
            chargeRateField.val(result.success ? result.chargeRate.toFixed(2) : '0.00');
            calculateStaffCost();
        },
        error: function () {
            chargeRateField.prop('disabled', false).val('0.00');
        }
    });
}

function calculateStaffCost() {
    var rate = parseFloat($('#ChargeRate').val()) || 0;
    var hours = parseFloat($('#PlannedHours').val()) || 0;
    $('#StaffCost').val((rate * hours).toFixed(2));
    $('#Days').val((hours / 8).toFixed(2));
}

$(document).on('change', '#PlannedHours, #ChargeRate', function () {
    calculateStaffCost();
});

// ---- Modal helpers ----

function closeModal() {
    clearValidationErrors();
    $('#modaPopupBody').html('');
    $('#modalPopup').removeClass('show');
}

function isFormValid(form) {
    var isValid = true;
    form.find('[required]').each(function () {
        if (!$(this).val() || $(this).val().trim() === '') {
            isValid = false;
            return false;
        }
    });
    return isValid;
}

function clearValidationErrors() {
    var errorSummary = $('.govuk-error-summary', '#modaPopupBody');
    errorSummary.hide().find('.govuk-error-summary__list').empty();
    $('[name]', '#modaPopupBody').each(function () {
        $(this).closest('.govuk-form-group').removeClass('govuk-form-group--error');
        $(this).removeClass('govuk-input--error');
        $(this).siblings('span[data-valmsg-for]').text('').hide();
    });
}

function displayClientValidationErrors(form) {
    var errorSummary = $('.govuk-error-summary', '#modaPopupBody');
    var errorList = errorSummary.find('.govuk-error-summary__list');
    errorList.empty();
    errorSummary.find('.govuk-error-summary__title').text('There is a problem');
    clearValidationErrors();
    var errors = [];
    form.find('[required]').each(function () {
        var $field = $(this);
        if (!$field.val() || $field.val().trim() === '') {
            var fieldName = $field.attr('name') || '';
            var label = $('label[for="' + fieldName + '"]', '#modaPopupBody').text().trim() || fieldName;
            errors.push({ field: fieldName, message: label + ' is required' });
        }
    });
    if (errors.length > 0) {
        errors.forEach(function (error) {
            errorList.append('<li><a href="#' + error.field + '">' + error.message + '</a></li>');
            var $field = $('[name="' + error.field + '"]', '#modaPopupBody');
            $field.closest('.govuk-form-group').addClass('govuk-form-group--error');
            $field.addClass('govuk-input--error');
            $field.siblings('span[data-valmsg-for]').text(error.message).show();
        });
        errorSummary.show().focus();
    }
}

function displayServerValidationErrors(errors, message) {
    var errorSummary = $('.govuk-error-summary', '#modaPopupBody');
    var errorList = errorSummary.find('.govuk-error-summary__list');
    errorList.empty();
    errorSummary.find('.govuk-error-summary__title').text('There is a problem');
    clearValidationErrors();
    if (errors && errors.length > 0) {
        errors.forEach(function (error) {
            var fieldName = error.field || '';
            var errorMessage = error.message || message || 'Validation error';
            errorList.append('<li><a href="#' + fieldName + '">' + errorMessage + '</a></li>');
            if (fieldName) {
                var $field = $('[name="' + fieldName + '"]', '#modaPopupBody');
                if ($field.length) {
                    $field.closest('.govuk-form-group').addClass('govuk-form-group--error');
                    $field.addClass('govuk-input--error');
                    $field.siblings('span[data-valmsg-for]').text(errorMessage).show();
                }
            }
        });
        errorSummary.show().focus();
    } else if (message) {
        errorList.append('<li>' + message + '</li>');
        errorSummary.show().focus();
    }
}

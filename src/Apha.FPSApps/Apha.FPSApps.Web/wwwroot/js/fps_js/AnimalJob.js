// AnimalJob.js - Shared animal plan CRUD, rate calculation, and modal/validation helpers.
// Each page must configure AnimalJobConfig before this script runs its event bindings.

var AnimalJobConfig = {
    getJobCode: function () { return ''; },
    requireJobCodeForAdd: false,
    onSaved: function () { window.location.reload(); },
    onUpdated: function () { window.location.reload(); },
    onDeleted: function () { window.location.reload(); }
};

// ---- Animal Plan CRUD ----

function addAnimalPlan(btn) {
    if (AnimalJobConfig.requireJobCodeForAdd && !AnimalJobConfig.getJobCode()) {
        alert('Please select a project first.');
        return;
    }
    $.ajax({
        url: '/FPS/AnimalJob/Create',
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

function saveAnimalPlan() {
    var form = $('#formAddAnimalPlan');
    if (!isFormValid(form)) {
        displayClientValidationErrors(form);
        return;
    }
    var data = {
        IndCounter: 0,
        JobCode: AnimalJobConfig.getJobCode(),
        AnimalType: $('#AnimalTypeDropdown').val(),
        NumberOfDays: parseFloat($('#NumberOfDays').val()) || 0,
        NumberOfAnimals: parseFloat($('#NumberOfAnimals').val()) || 0,
        DailyRate: parseFloat($('#DailyRate').val()) || 0,
        AnimalCost: parseFloat($('#AnimalCost').val()) || 0
    };
    $.ajax({
        url: '/FPS/AnimalJob/Create',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                alert(result.message);
                closeModal();
                AnimalJobConfig.onSaved();
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

function editAnimalPlan(btn) {
    var indCounter = $(btn).data('id');
    $.ajax({
        url: '/FPS/AnimalJob/Edit',
        type: 'GET',
        data: { indCounter: indCounter, jobCode: AnimalJobConfig.getJobCode() },
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

function updateAnimalPlan() {
    var form = $('#formEditAnimalPlan');
    if (!isFormValid(form)) {
        displayClientValidationErrors(form);
        return;
    }
    var indCounter = $('#IndCounter').val();
    var jobCode = form.find('[name="JobCode"]').val();
    var data = {
        IndCounter: parseInt(indCounter) || 0,
        JobCode: jobCode,
        AnimalType: $('#AnimalTypeDropdown').val(),
        NumberOfDays: parseFloat($('#NumberOfDays').val()) || 0,
        NumberOfAnimals: parseFloat($('#NumberOfAnimals').val()) || 0,
        DailyRate: parseFloat($('#DailyRate').val()) || 0,
        AnimalCost: parseFloat($('#AnimalCost').val()) || 0
    };
    $.ajax({
        url: '/FPS/AnimalJob/Edit?indCounter=' + indCounter,
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                alert(result.message);
                closeModal();
                AnimalJobConfig.onUpdated();
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

function deleteAnimalPlan(btn) {
    var indCounter = $(btn).data('id');
    if (confirm('Are you sure you want to delete this animal cost entry?')) {
        $.ajax({
            url: '/FPS/AnimalJob/Delete',
            type: 'DELETE',
            data: { indCounter: indCounter },
            success: function (response) {
                if (response.success) {
                    alert('Deleted successfully.');
                    AnimalJobConfig.onDeleted();
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

function getAnimalPlanExtraFilters() {
    return { jobCode: AnimalJobConfig.getJobCode() };
}

// ---- Rate calculation ----

function onAnimalTypeSelected(selectElement) {
    var animalType = $(selectElement).val();
    if (!animalType) {
        $('#DailyRate').val('');
        $('#AnimalCost').val('');
        return;
    }
    var rateField = $('#DailyRate');
    rateField.prop('disabled', true).val('');
    $.ajax({
        url: '/FPS/AnimalJob/GetAnimalRate',
        type: 'GET',
        data: { animalType: animalType },
        success: function (result) {
            rateField.prop('disabled', false);
            rateField.val(result.success ? result.dailyRate.toFixed(2) : '0.00');
            calculateAnimalCost();
        },
        error: function () {
            rateField.prop('disabled', false).val('0.00');
        }
    });
}

function calculateAnimalCost() {
    var days = parseFloat($('#NumberOfDays').val()) || 0;
    var animals = parseFloat($('#NumberOfAnimals').val()) || 0;
    var rate = parseFloat($('#DailyRate').val()) || 0;
    $('#AnimalCost').val(((days + animals)* rate).toFixed(2));
}

$(document).on('change', '#NumberOfDays, #NumberOfAnimals', function () {
    calculateAnimalCost();
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

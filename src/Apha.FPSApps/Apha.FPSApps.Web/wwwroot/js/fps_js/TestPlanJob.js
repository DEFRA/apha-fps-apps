// TestPlanJob.js - Test plan CRUD operations for the Programme Test Purchase Plan screen.
// Requires ajax-form-validation.js to be loaded before this script.
// Each page must configure TestPlanJobConfig before this script is used.

var TestPlanJobConfig = {
    getJobCode: function () { return ''; },
    requireJobCodeForAdd: false,
    onSaved: function () { window.location.reload(); },
    onUpdated: function () { window.location.reload(); },
    onDeleted: function () { window.location.reload(); }
};

// ---- Test Plan CRUD ----

function addTestPlan(btn) {
    if (TestPlanJobConfig.requireJobCodeForAdd && !TestPlanJobConfig.getJobCode()) {
        alert('Please select a project first.');
        return;
    }
    $.ajax({
        url: '/FPS/TestPlanJob/Create',
        type: 'GET',
        success: function (html) {
            $('#modaPopupBody').html(html);
            // Inject the current project as ProjectBuyerCode for the pricing lookup
            $('#modaPopupBody #ProjectBuyerCode').val(TestPlanJobConfig.getJobCode());
            $('#modalPopup').addClass('show');
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                alert('An error occurred while opening the form.');
            }
        }
    });
}

function saveTestPlan() {
    var form = $('#formAddTestPlan');
    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }
    var data = {
        IsEdit: false,
        TestCode: $('#TestCode').val(),
        Buyer: TestPlanJobConfig.getJobCode(),
        ProjectBuyerCode: TestPlanJobConfig.getJobCode(),
        NoRequired: parseFloat($('#NoRequired').val()) || 0,
        UnitPrice: parseFloat($('#UnitPrice').val()) || 0,
        Active: 1
    };
    $.ajax({
        url: '/FPS/TestPlanJob/Create',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                alert(result.message);
                closeModal();
                TestPlanJobConfig.onSaved();
            } else {
                displayServerValidationErrors(result.errors, result.message, '#modaPopupBody');
            }
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                alert('An error occurred while saving.');
            }
        }
    });
}

function editTestPlan(btn) {
    var testCode = $(btn).data('id');
    var buyer = TestPlanJobConfig.getJobCode();
    $.ajax({
        url: '/FPS/TestPlanJob/Edit',
        type: 'GET',
        data: { testCode: testCode, buyer: buyer },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                alert('An error occurred while fetching the record.');
            }
        }
    });
}

function updateTestPlan() {
    var form = $('#formEditTestPlan');
    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }
    var data = {
        IsEdit: true,
        TestCode: form.find('[name="TestCode"]').val(),
        Buyer: form.find('[name="Buyer"]').val(),
        ProjectBuyerCode: form.find('[name="ProjectBuyerCode"]').val(),
        NoRequired: parseFloat($('#NoRequired').val()) || 0,
        UnitPrice: parseFloat($('#UnitPrice').val()) || 0,
        Active: parseInt(form.find('[name="Active"]').val()) || 1
    };
    $.ajax({
        url: '/FPS/TestPlanJob/Edit',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                alert(result.message);
                closeModal();
                TestPlanJobConfig.onUpdated();
            } else {
                displayServerValidationErrors(result.errors, result.message, '#modaPopupBody');
            }
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                alert('An error occurred while saving.');
            }
        }
    });
}

function deleteTestPlan(btn) {
    var testCode = $(btn).data('id');
    var buyer = TestPlanJobConfig.getJobCode();
    showGovukConfirm('Are you sure you want to delete this test plan item?').then(function (confirmed) {
        if (!confirmed) { return; }
        $.ajax({
            url: '/FPS/TestPlanJob/Delete',
            type: 'DELETE',
            data: { testCode: testCode, buyer: buyer },
            success: function (response) {
                if (response.success) {
                    showGovukAlert('Deleted successfully.').then(function () {
                        TestPlanJobConfig.onDeleted();
                    });
                } else {
                    showGovukAlert(response.message);
                }
            },
            error: function () {
                showGovukAlert('An error occurred while deleting.');
            }
        });
    });
}

function getTestPlanExtraFilters() {
    return { jobCode: TestPlanJobConfig.getJobCode() };
}

// ---- Pricing and cost calculation ----

function onTestCodeSelected(select) {
    var description = $(select).find(':selected').data('description') || '';
    $('#ItemDescription').val(description);

    // Fetch recommended unit price from server
    var testCode = $(select).val();
    if (!testCode) { $('#RecUnitPrice').val('0.00'); return; }
    var projectBuyerCode = $('#ProjectBuyerCode').val() || '';
    $.get('/FPS/TestPlanJob/GetRecUnitPrice', { testCode: testCode, projectBuyerCode: projectBuyerCode }, function (result) {
        if (result.success) {
            var price = parseFloat(result.recUnitPrice || 0).toFixed(2);
            $('#RecUnitPrice').val(price);
            $('#UnitPrice').val(price);
            calculateTestCost();
        }
    });
}

function calculateTestCost() {
    var noRequired = parseFloat($('#NoRequired').val()) || 0;
    var unitPrice = parseFloat($('#UnitPrice').val()) || 0;
    $('#TotalCost').val((noRequired * unitPrice).toFixed(2));
}

$(document).on('change', '#NoRequired, #UnitPrice', function () {
    calculateTestCost();
});

// ---- Modal helpers ----

function closeModal() {
    clearValidationErrors('#modaPopupBody');
    $('#modaPopupBody').html('');
    $('#modalPopup').removeClass('show');
}

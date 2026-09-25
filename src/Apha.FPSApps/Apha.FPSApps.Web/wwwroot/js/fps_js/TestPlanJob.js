// TestPlanJob.js - Test plan CRUD operations for the Programme Test Purchase Plan screen.
// Requires ajax-form-validation.js to be loaded before this script.
// Each page must configure TestPlanJobConfig before this script is used.

var TestPlanJobConfig = {
    getJobCode: function () { return ''; },
    requireJobCodeForAdd: false,
    fromProjectPlanning: false,
    onSaved: function () { window.location.reload(); },
    onUpdated: function () { window.location.reload(); },
    onDeleted: function () { window.location.reload(); }
};

// ---- Test Plan CRUD ----

function addTestPlan(btn) {
    if (TestPlanJobConfig.requireJobCodeForAdd && !TestPlanJobConfig.getJobCode()) {
        showAlertMessage('Please select a project first.', AlertType.INFO);
        return;
    }
    showLoader();
    $.ajax({
        url: '/FPS/TestPlanJob/Create',
        type: 'GET',
        data: { fromProjectPlanning: TestPlanJobConfig.fromProjectPlanning === true },
        success: function (html) {
            $('#modaPopupBody').html(html);
            // Inject the current project as ProjectBuyerCode for the pricing lookup
            $('#modaPopupBody #ProjectBuyerCode').val(TestPlanJobConfig.getJobCode());
            $('#modalPopup').addClass('show');
            hideLoader();
        },
        error: function (xhr) {
            hideLoader();
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                showAlertMessage('An error occurred while opening the form.', AlertType.ERROR);
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
    showLoader();
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
            hideLoader();
            if (result.success) {
                closeModal();
                showAlertMessage(result.message, AlertType.SUCCESS).then(function () {                   
                    TestPlanJobConfig.onSaved();
                });
            } else {
                displayServerValidationErrors(result.errors, result.message, '#modaPopupBody');
            }
        },
        error: function (xhr) {
            hideLoader();
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                showAlertMessage('An error occurred while saving.', AlertType.ERROR);
            }
        }
    });
}

function editTestPlan(btn) {
    showLoader();
    var testCode = $(btn).data('id');
    var buyer = TestPlanJobConfig.getJobCode();
    $.ajax({
        url: '/FPS/TestPlanJob/Edit',
        type: 'GET',
        data: {
            testCode: testCode,
            buyer: buyer,
            fromProjectPlanning: TestPlanJobConfig.fromProjectPlanning === true
        },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            hideLoader();
        },
        error: function (xhr) {
            hideLoader();
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                showAlertMessage('An error occurred while fetching the record.', AlertType.ERROR);
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
    showLoader();
    var data = {
        IsEdit: true,
        TestCode: form.find('[name="TestCode"]').val(),
        Buyer: form.find('[name="Buyer"]').val(),
        ProjectBuyerCode: form.find('[name="ProjectBuyerCode"]').val(),
        NoRequired: parseFloat($('#NoRequired').val()) || 0,
        UnitPrice: $('#UnitPrice').val() || 0,
        Active: parseInt(form.find('[name="Active"]').val()) || 1
    };
    $.ajax({
        url: '/FPS/TestPlanJob/Edit',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            hideLoader();
            if (result.success) {
                closeModal();
                showAlertMessage(result.message, AlertType.SUCCESS).then(function () {                  
                    TestPlanJobConfig.onUpdated();
                });
            } else {
                displayServerValidationErrors(result.errors, result.message, '#modaPopupBody');
            }
        },
        error: function (xhr) {
            hideLoader();
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                showAlertMessage('An error occurred while saving.', AlertType.ERROR);
            }
        }
    });
}

function deleteTestPlan(btn) {
    var testCode = $(btn).data('id');
    var buyer = TestPlanJobConfig.getJobCode();
    showGovukConfirm('Are you sure you want to delete this test plan item?').then(function (confirmed) {
        if (!confirmed) { return; }
        showLoader();
        $.ajax({
            url: '/FPS/TestPlanJob/Delete',
            type: 'DELETE',
            data: { testCode: testCode, buyer: buyer },
            success: function (response) {
                hideLoader();
                if (response.success) {
                    showAlertMessage('Deleted successfully.', AlertType.SUCCESS).then(function () {
                        TestPlanJobConfig.onDeleted();
                    });
                } else {
                    showAlertMessage(response.message, AlertType.ERROR);
                }
            },
            error: function () {
                hideLoader();
                showAlertMessage('An error occurred while deleting.', AlertType.ERROR);
            }
        });
    });
}

function getTestPlanExtraFilters() {
    return { jobCode: TestPlanJobConfig.getJobCode() };
}

// ---- Test Code panel dropdown ----
// Lazy/chunked rendering, debounced search and delegated row selection are all
// provided by common/lazy-panel-dropdown.js.

var testCodeDropdown = createLazyPanelDropdown({
    panelId: 'TestCodeDropdownPanel',
    bodyId: 'TestCodeDropdownBody',
    searchBoxId: 'TestCodeSearchBox',
    displayId: 'TestCodeDisplay',
    getOptions: function () { return window.testCodePanelOptions; },
    getSearchKey: function (o) { return o.c + '\u0000' + o.d; },
    getRowAttributes: function (o) {
        return { value: o.v, code: o.c, description: o.d, unitprice: o.p };
    },
    getCells: function (o) { return [o.c, o.d, '\u00A3' + o.p]; },
    onSelect: function (row) {
        selectTestCode(row.value, row.code, row.description, row.unitprice);
    }
});

function toggleTestCodePanel() {
    testCodeDropdown.toggle();
}

function filterTestCodePanel(query) {
    testCodeDropdown.filter(query);
}

function selectTestCode(value, displayCode, description, unitPrice) {
    // Update visible display input
    var display = document.getElementById('TestCodeDisplay');
    if (display) display.value = displayCode;

    // Update hidden select and fire onTestCodeSelected
    var select = document.getElementById('TestCode');
    if (select) {
        select.value = value;
        // Ensure the selected option has the description data attribute set
        var opt = select.querySelector('option[value="' + value + '"]');
        if (opt) opt.setAttribute('data-description', description);
        $(select).trigger('change');
    }

    testCodeDropdown.close();
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
            var price = result.recUnitPrice || 0;
            $('#RecUnitPrice').val(price);
            $('#UnitPrice').val(price);
            calculateTestCost();
        }
    });
}

function calculateTestCost() {
    var noRequired = parseFloat($('#NoRequired').val()) || 0;
    var unitPrice = parseFloat($('#UnitPrice').val()) || 0;
    $('#TotalCost').val((noRequired * unitPrice).toFixed(4));
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

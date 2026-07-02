// PlanStaffZTCode.js - ZT Code plan CRUD and time summary refresh.
// Requires ajax-form-validation.js to be loaded before this script.
// The page must set ZtPlanConfig before this script runs.

var ZtPlanConfig = {
    staffId: '',
    summaryUrl: '',
    createUrl: '',
    editUrl: '',
    deleteUrl: '',
    gridId: 'ztCodesGrid'
};

// ---- ZT Plan CRUD ----

function addZtPlan(btn) {
    $.ajax({
        url: ZtPlanConfig.createUrl,
        type: 'GET',
        data: { staffId: ZtPlanConfig.staffId },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        },
        error: function () {
            showAlertMessage('An error occurred while loading the form.', AlertType.ERROR);
        }
    });
}

function saveZtPlan() {
    clearValidationErrors('#modaPopupBody');
    var form = $('#addZtPlanForm');

    if (!ztIsFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    var data = {
        StaffID: ZtPlanConfig.staffId,
        JobCode: form.find('[name="JobCode"]').val(),
        ZtDescription: form.find('[name="ZtDescription"]').val(),
        PlannedHours: ztParseFloatOrZero(form.find('[name="PlannedHours"]').val())
    };

    $.ajax({
        url: ZtPlanConfig.createUrl,
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                closeZtModal();
                showAlertMessage(result.message || 'ZT plan entry created successfully.', AlertType.SUCCESS);
                window['gridManager_' + ZtPlanConfig.gridId].reloadGrid({ page: 1 });
                refreshZtTimeSummary();
            } else {
                displayServerValidationErrors(result.errors, result.message, '#modaPopupBody');
            }
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                showAlertMessage('An error occurred while saving.', AlertType.ERROR);
            }
        }
    });
}

function editZtPlan(btn) {
    var jobCode = $(btn).data('id');
    $.ajax({
        url: ZtPlanConfig.editUrl,
        type: 'GET',
        data: { staffId: ZtPlanConfig.staffId, jobCode: jobCode },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        },
        error: function () {
            showAlertMessage('An error occurred while loading the form.', AlertType.ERROR);
        }
    });
}

function updateZtPlan() {
    clearValidationErrors('#modaPopupBody');
    var form = $('#editZtPlanForm');

    if (!ztIsFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    var staffId = form.find('[name="StaffID"]').val();
    var data = {
        StaffID: staffId,
        JobCode: form.find('[name="JobCode"]').val(),
        OriginalJobCode: form.find('[name="OriginalJobCode"]').val(),
        ZtDescription: form.find('[name="ZtDescription"]').val(),
        PlannedHours: ztParseFloatOrZero(form.find('[name="PlannedHours"]').val())
    };

    $.ajax({
        url: ZtPlanConfig.editUrl,
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                closeZtModal();
                showAlertMessage(result.message || 'ZT plan entry updated successfully.', AlertType.SUCCESS);
                window['gridManager_' + ZtPlanConfig.gridId].reloadGrid({ page: 1 });
                refreshZtTimeSummary();
            } else {
                displayServerValidationErrors(result.errors, result.message, '#modaPopupBody');
            }
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                showAlertMessage('An error occurred while saving.', AlertType.ERROR);
            }
        }
    });
}

function deleteZtPlan(btn) {
    var jobCode = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this record?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: ZtPlanConfig.deleteUrl,
            type: 'DELETE',
            data: { staffId: ZtPlanConfig.staffId, jobCode: jobCode },
            success: function (response) {
                if (response.success) {
                    showAlertMessage('Deleted successfully', AlertType.SUCCESS).then(function () {
                        window['gridManager_' + ZtPlanConfig.gridId].reloadGrid({ page: 1 });
                        refreshZtTimeSummary();
                    });
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

// ---- Modal helpers ----

function closeZtModal() {
    clearValidationErrors('#modaPopupBody');
    $('#modaPopupBody').html('');
    $('#modalPopup').removeClass('show');
}

// ---- Time summary refresh ----

function refreshZtTimeSummary() {
    if (!ZtPlanConfig.staffId || !ZtPlanConfig.summaryUrl) { return; }
    $.get(ZtPlanConfig.summaryUrl, { staffId: ZtPlanConfig.staffId }, function (json) {
        if (json.success) {
            ztSetEl('ts-hrspaid', json.hrsPaid);
            ztSetEl('ts-leave', json.leave);
            ztSetEl('ts-sickspecial', json.sickSpecial);
            ztSetEl('ts-hrsavail', json.hrsAvail);
            ztSetEl('ts-planned-zt', json.plannedAdminZT);
            ztSetEl('ts-free', json.freeForChargeableWork);
            ztSetEl('staff-name', json.name);
            ztSetEl('staff-grade', json.workGroupGrade);
        }
    });
}

// ---- Utility ----

function ztSetEl(id, value) {
    var el = document.getElementById(id);
    if (el) { el.textContent = (value !== undefined && value !== null) ? value : ''; }
}

function ztIsFormValid(form) {
    var isValid = true;
    form.find('[required]').each(function () {
        if (!$(this).val() || $(this).val().trim() === '') {
            isValid = false;
            return false;
        }
    });
    return isValid;
}

function ztParseFloatOrZero(val) {
    var cleaned = (val || '').replace(/[£,]/g, '').trim();
    return cleaned !== '' ? parseFloat(cleaned) : 0;
}

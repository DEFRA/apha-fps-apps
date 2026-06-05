/**
 * fps_setupresource_ZT_Codes.js (production)
 * Wires the ZT planning grid and modal to the PlanStaffZTCodeController endpoints.
 * Follows the same pattern as AnimalMaintenance/Index.cshtml.
 * Requires ztStaffId, ztStaffSummaryUrl, ztCreateUrl, ztEditUrl, ztDeleteUrl to be
 * defined in the Razor view before this script is loaded.
 */

function addZtPlan() {
    $.ajax({
        url: ztCreateUrl,
        type: 'GET',
        data: { staffId: ztStaffId },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        },
        error: function () {
            showGovukAlert('An error occurred while loading the form.');
        }
    });
}

function saveZtPlan() {
    clearValidationErrors('#modaPopupBody');
    var form = $('#addZtPlanForm');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    var data = {
        StaffID:      ztStaffId,
        JobCode:      form.find('[name="JobCode"]').val(),
        ZtDescription: form.find('[name="ZtDescription"]').val(),
        PlannedHours: parseFloatOrZero(form.find('[name="PlannedHours"]').val())
    };

    $.ajax({
        url: ztCreateUrl,
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                closeZtModal();
                showGovukAlert(result.message || 'ZT plan entry created successfully.');
                window['gridManager_ztCodesGrid'].reloadGrid({ page: 1 });
                refreshZtTimeSummary();
            } else {
                displayServerValidationErrors(result.errors, result.message, '#modaPopupBody');
            }
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                showGovukAlert('An error occurred while saving.');
            }
        }
    });
}

function editZtPlan(btn) {
    var jobCode = $(btn).data('id');
    $.ajax({
        url: ztEditUrl,
        type: 'GET',
        data: { staffId: ztStaffId, jobCode: jobCode },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        },
        error: function () {
            showGovukAlert('An error occurred while loading the form.');
        }
    });
}

function updateZtPlan() {
    clearValidationErrors('#modaPopupBody');
    var form = $('#editZtPlanForm');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    var staffId = form.find('[name="StaffID"]').val();
    var data = {
        StaffID:      staffId,
        JobCode:      form.find('[name="JobCode"]').val(),
        ZtDescription: form.find('[name="ZtDescription"]').val(),
        PlannedHours: parseFloatOrZero(form.find('[name="PlannedHours"]').val())
    };

    $.ajax({
        url: ztEditUrl + '?staffId=' + encodeURIComponent(staffId),
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                closeZtModal();
                showGovukAlert(result.message || 'ZT plan entry updated successfully.');
                window['gridManager_ztCodesGrid'].reloadGrid({ page: 1 });
                refreshZtTimeSummary();
            } else {
                displayServerValidationErrors(result.errors, result.message, '#modaPopupBody');
            }
        },
        error: function (xhr) {
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                showGovukAlert('An error occurred while saving.');
            }
        }
    });
}

function deleteZtPlan(btn) {
    var jobCode = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this record?').then(function (confirmed) {
        if (!confirmed) { return; }
        $.ajax({
            url: ztDeleteUrl,
            type: 'DELETE',
            data: { staffId: ztStaffId, jobCode: jobCode },
            success: function (response) {
                if (response.success) {
                    showGovukAlert('Deleted successfully.').then(function () {
                        window['gridManager_ztCodesGrid'].reloadGrid({ page: 1 });
                        refreshZtTimeSummary();
                    });
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

function closeZtModal() {
    clearValidationErrors('#modaPopupBody');
    $('#modaPopupBody').html('');
    $('#modalPopup').removeClass('show');
}

function refreshZtTimeSummary() {
    if (!ztStaffId || !ztStaffSummaryUrl) { return; }
    $.get(ztStaffSummaryUrl, { staffId: ztStaffId }, function (json) {
        if (json.success) {
            setEl('ts-hrspaid',    json.hrsPaid);
            setEl('ts-leave',      json.leave);
            setEl('ts-sickspecial', json.sickSpecial);
            setEl('ts-hrsavail',   json.hrsAvail);
            setEl('ts-planned-zt', json.plannedAdminZT);
            setEl('ts-free',       json.freeForChargeableWork);
            setEl('staff-name',    json.name);
            setEl('staff-grade',   json.workGroupGrade);
        }
    });
}

function setEl(id, value) {
    var el = document.getElementById(id);
    if (el) { el.textContent = (value !== undefined && value !== null) ? value : ''; }
}

function parseFloatOrZero(val) {
    var cleaned = (val || '').replace(/[£,]/g, '').trim();
    return cleaned !== '' ? parseFloat(cleaned) : 0;
}

function getGridManager() {
    return window['gridManager_' + WorkGroupStaffConfig.gridId];
}

function getMaintWGStaffExtraFilters() {
    return {};
}

function addMaintWGStaff() {
    $.get(WorkGroupStaffConfig.createUrl, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function editMaintWGStaff(btn) {
    var pactId = $(btn).data('id');
    $.get(WorkGroupStaffConfig.editUrl, { pactId: pactId }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function deleteMaintWGStaff(btn) {
    var pactId = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this WG Staff record?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: WorkGroupStaffConfig.deleteUrl,
            type: 'DELETE',
            data: { pactId: pactId },
            success: function (response) {
                if (response.success) {
                    showAlertMessage('WG Staff record deleted successfully', AlertType.SUCCESS).then(function () {
                        getGridManager().reloadGrid({ page: 1 });
                    });
                } else {
                    showAlertMessage('Error: ' + (response.message || 'Delete failed.'), AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting.', AlertType.ERROR);
            }
        });
    });
}

function saveMaintWGStaff() {
    clearValidationErrors('#modaPopupBody');
    var form = $('#formMaintWGStaff');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    var isEdit = $('#hdnIsEdit').val() === 'true';

    var hrsPaid = parseFloat($('#HrsPaid').val()) || 0;
    var leave = parseFloat($('#Leave').val()) || 0;
    var sickSpecial = parseFloat($('#SickSpecial').val()) || 0;

    var dto = {
        PactId: $('#PactId').val(),
        SpNumber: $('#SpNumber').val() || '',
        Name: $('#Name').val(),
        WorkGroupGrade: $('#WorkGroupGrade').val(),
        PersonStatus: $('#PersonStatus').val(),
        PersonClass: $('#PersonClass').val() || null,
        HrsPaid: hrsPaid,
        Leave: leave,
        SickSpecial: sickSpecial,
        HrsAvail: parseFloat($('#HrsAvail').val()) || 0,
        MakeAvailable: $('#MakeAvailable').is(':checked') ? 1 : 0,
        TimeRecorder: $('#TimeRecorder').is(':checked') ? 1 : 0,
        StartDate: $('#StartDate').val() || null,
        EndDate: $('#EndDate').val() || null,
        HoursPerWeek: $('#HoursPerWeek').val() !== '' ? parseFloat($('#HoursPerWeek').val()) : null
    };

    var url = isEdit
        ? WorkGroupStaffConfig.updateUrl
        : WorkGroupStaffConfig.createUrl;

    $.ajax({
        url: url,
        type: 'POST',
        data: JSON.stringify(dto),
        contentType: 'application/json',
        success: function (response) {
            if (response.success) {
                closeModal();
                showAlertMessage(response.message, AlertType.SUCCESS);
                getGridManager().reloadGrid({ page: 1 });
            } else {
                displayServerValidationErrors(response.errors, response.message, '#modaPopupBody');
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

function closeModal() {
    clearValidationErrors();
    $('#modaPopupBody').html('');
    $('#modalPopup').removeClass('show');
}
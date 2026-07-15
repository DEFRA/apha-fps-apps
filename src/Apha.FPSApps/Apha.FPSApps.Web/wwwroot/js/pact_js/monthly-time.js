function getLiveGridManager() {
    return window['gridManager_' + monthlyTimeLiveGridId];
}

function getStagingGridManager() {
    return window['gridManager_' + monthlyTimeStagingGridId];
}

function getMonthlyTimeLiveFilters() {
    return {
        workGroup: $('#ddWorkGroup').val() || null,
        timeCode: $('#ddTimeCode').val() || null,
        pactStaffId: $('#ddStaff').val() || null,
        parentProject: $('#ddParentProject').val() || null,
        month: $('#ddMonth').val() || null
    };
}

function getMonthlyTimeStagingFilters() {
    return {
        passed: window.monthlyTimePassedFilter ?? null
    };
}

function reloadLiveGrid() {
    const gm = getLiveGridManager();
    if (gm) {
        gm.reloadGrid({ page: 1, sortBy: 'WorkGroup', descending: false });
    }
}

function reloadStagingGrid() {
    const gm = getStagingGridManager();
    if (gm) {
        gm.reloadGrid({ page: 1, sortBy: 'Id', descending: false });
    }
}

function clearLiveSearch() {
    $('#ddWorkGroup').val('');
    $('#ddStaff').val('');
    $('#ddTimeCode').val('');
    $('#ddParentProject').val('');
    $('#ddMonth').val('');

    if (window.monthlyTimeStaffDropdown) {
        window.monthlyTimeStaffDropdown.clear();
    }

    resetTimeCodeOptions();
    resetParentProjectOptions();
}

function resetTimeCodeOptions() {
    const $timeCode = $('#ddTimeCode');
    $timeCode.empty().append('<option value="">--select--</option>');
}

function resetParentProjectOptions() {
    const $parentProject = $('#ddParentProject');
    $parentProject.empty().append('<option value="">--select--</option>');
}

function initStaffDropdown() {
    window.monthlyTimeStaffDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'monthlyTimeStaff',
        containerSelector: '#staffSelectDropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: 'Staff',
        columns: [
            { field: 'name', header: 'Name', width: '180px' },
            { field: 'pactId', header: 'PACTid', width: '90px' },
            { field: 'workGroupGrade', header: 'WG_Grade', width: '100px' }
        ],
        data: [],
        displayField: function (row) { return row.name || ''; },
        valueField: function (row) { return row.pactId || ''; },
        enableSearch: true,
        showSerialNumber: false,
        callbacks: {
            onSelect: function (selectedItem) {
                $('#ddStaff').val(selectedItem?.pactId || '');
            },
            onClear: function () {
                $('#ddStaff').val('');
            }
        }
    });
}

function loadStaffByWorkGroup(workGroup) {
    if (!window.monthlyTimeStaffDropdown) return;

    window.monthlyTimeStaffDropdown.clear();

    if (!workGroup) {
        window.monthlyTimeStaffDropdown.updateData([]);
        return;
    }

    $.get('/PACT/MonthlyTime/GetStaffByWorkGroup', { workGroup: workGroup })
        .done(function (data) {
            window.monthlyTimeStaffDropdown.updateData(Array.isArray(data) ? data : []);
        })
        .fail(function () {
            window.monthlyTimeStaffDropdown.updateData([]);
            showAlertMessage('Failed to load staff options.', AlertType.ERROR);
        });
}

function loadTimeCodesByWorkGroup(workGroup) {
    resetTimeCodeOptions();
    resetParentProjectOptions();

    if (!workGroup) {
        return;
    }

    $.get('/PACT/MonthlyTime/GetTimeCodesByWorkGroup', { workGroup: workGroup })
        .done(function (data) {
            const items = Array.isArray(data) ? data : [];
            const $timeCode = $('#ddTimeCode');
            items.forEach(function (item) {
                $timeCode.append($('<option>', { value: item.value, text: item.text }));
            });
        })
        .fail(function () {
            showAlertMessage('Failed to load timecode options.', AlertType.ERROR);
        });
}

function loadProjectsByWorkGroupAndTimeCode(workGroup, timeCode) {
    resetParentProjectOptions();

    if (!workGroup || !timeCode) {
        return;
    }

    $.get('/PACT/MonthlyTime/GetProjectsByWorkGroupAndTimeCode', { workGroup: workGroup, timeCode: timeCode })
        .done(function (data) {
            const items = Array.isArray(data) ? data : [];
            const $parentProject = $('#ddParentProject');
            items.forEach(function (item) {
                $parentProject.append($('<option>', { value: item.value, text: item.text }));
            });
        })
        .fail(function () {
            showAlertMessage('Failed to load parent project options.', AlertType.ERROR);
        });
}

function parseCompositeKey(key) {
    const parts = (key || '').split('|');
    return {
        pactStaffId: parts[0] || '',
        timeCode: parts[1] || '',
        month: parts[2] || '',
        parentProject: parts[3] || ''
    };
}

function editMonthlyTimeLive(btn) {
    const key = $(btn).data('id');
    const parsed = parseCompositeKey(key);
    $.get('/PACT/MonthlyTime/GetLiveRecord', parsed)
        .done(function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        })
        .fail(function () {
            showAlertMessage('Failed to load monthly time record.', AlertType.ERROR);
        });
}

function saveMonthlyTimeLive() {
    clearValidationErrors('#modaPopupBody');
    const form = $('#monthlyTimeLiveForm');
    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    const data = form.serializeObject();
    $.ajax({
        url: '/PACT/MonthlyTime/SaveLiveRecord',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                reloadLiveGrid();
                showAlertMessage(response.message, AlertType.SUCCESS);
            } else {
                showAlertMessage(response.message || 'Update failed.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred while saving.', AlertType.ERROR);
        }
    });
}

function addStagingMonthlyTime() {
    $.get('/PACT/MonthlyTime/AddStagingRecord')
        .done(function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        })
        .fail(function () {
            showAlertMessage('Failed to load add form.', AlertType.ERROR);
        });
}

function editStagingMonthlyTime(btn) {
    const id = $(btn).data('id');
    $.get('/PACT/MonthlyTime/GetStagingRecord', { id: id })
        .done(function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        })
        .fail(function () {
            showAlertMessage('Failed to load staging record.', AlertType.ERROR);
        });
}

function saveStagingMonthlyTime() {
    clearValidationErrors('#modaPopupBody');
    const form = $('#stagingMonthlyTimeForm');
    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    const data = form.serializeObject();
    $.ajax({
        url: '/PACT/MonthlyTime/SaveStagingRecord',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                reloadStagingGrid();
                showAlertMessage(response.message, AlertType.SUCCESS);
            } else {
                showAlertMessage(response.message || 'Save failed.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred while saving.', AlertType.ERROR);
        }
    });
}

function deleteStagingMonthlyTime(btn) {
    const id = $(btn).data('id');
    showGovukConfirm('Delete this imported record?').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/MonthlyTime/DeleteStagingRecord',
            type: 'DELETE',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    reloadStagingGrid();
                    showAlertMessage('Imported record deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Failed to delete imported record.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting.', AlertType.ERROR);
            }
        });
    });
}

function openImportTypeModal() {
    $('input[name="importType"]').prop('checked', false);
    $('#importTypeModal').addClass('show').css('display', 'flex');
}

function closeImportTypeModal() {
    $('#importTypeModal').removeClass('show').hide();
}

function confirmImportType() {
    const selected = $('input[name="importType"]:checked').val();
    if (!selected) {
        showAlertMessage('Please select an import type.', AlertType.INFO);
        return;
    }
    window.monthlyTimeImportType = selected;
    closeImportTypeModal();
    $('#csvInput').click();
}

function importMonthlyTime(file) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('importType', window.monthlyTimeImportType || '2');

    $.ajax({
        url: '/PACT/MonthlyTime/Import',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                reloadStagingGrid();
                showAlertMessage(response.message || 'Import completed.', AlertType.SUCCESS);
            } else {
                showAlertMessage(response.message || 'Import failed.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred while importing.', AlertType.ERROR);
        }
    });
}

function validateMonthlyTime() {
    $.post('/PACT/MonthlyTime/Validate')
        .done(function (response) {
            if (response.success) {
                reloadStagingGrid();
                showAlertMessage(response.message, AlertType.SUCCESS);
            } else {
                showAlertMessage(response.message || 'Validation failed.', AlertType.ERROR);
            }
        })
        .fail(function () {
            showAlertMessage('An error occurred during validation.', AlertType.ERROR);
        });
}

function makeLiveMonthlyTime() {
    $.post('/PACT/MonthlyTime/MakeLive')
        .done(function (response) {
            if (response.success) {
                reloadLiveGrid();
                reloadStagingGrid();
                showAlertMessage(response.message, AlertType.SUCCESS);
            } else {
                showAlertMessage(response.message || 'Make live failed.', AlertType.ERROR);
            }
        })
        .fail(function (xhr) {
            showAlertMessage(xhr.responseJSON?.message || 'An error occurred during make live.', AlertType.ERROR);
        });
}

function deleteAllMonthlyTime() {
    showGovukConfirm('Delete all imported records for the current user?').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/MonthlyTime/DeleteAllStagingRecords',
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    reloadStagingGrid();
                    showAlertMessage('Imported records deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Delete all failed.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting imported records.', AlertType.ERROR);
            }
        });
    });
}

function exportMonthlyTime() {
    const passed = window.monthlyTimePassedFilter;
    const url = passed === null || passed === undefined
        ? '/PACT/MonthlyTime/ExportStaging'
        : '/PACT/MonthlyTime/ExportStaging?passed=' + passed;
    window.location = url;
}

$(function () {
    window.monthlyTimePassedFilter = null;

    initStaffDropdown();

    $('#ddWorkGroup').on('change', function () {
        const workGroup = $(this).val();
        loadStaffByWorkGroup(workGroup);
        loadTimeCodesByWorkGroup(workGroup);
    });

    $('#ddTimeCode').on('change', function () {
        const workGroup = $('#ddWorkGroup').val();
        const timeCode = $(this).val();
        loadProjectsByWorkGroupAndTimeCode(workGroup, timeCode);
    });

    $('#btnSearchLive').on('click', reloadLiveGrid);
    $('#btnClearLiveSearch').on('click', function () {
        clearLiveSearch();
        reloadLiveGrid();
    });
    $('#importTypeBtn').on('click', openImportTypeModal);
    $('#csvInput').on('change', function () {
        const file = this.files && this.files[0];
        if (file) {
            importMonthlyTime(file);
        }
        this.value = '';
    });
    $('#validateBtn').on('click', validateMonthlyTime);
    $('#passedBtn').on('click', function () { window.monthlyTimePassedFilter = true; reloadStagingGrid(); });
    $('#failedBtn').on('click', function () { window.monthlyTimePassedFilter = false; reloadStagingGrid(); });
    $('#allBtn').on('click', function () { window.monthlyTimePassedFilter = null; reloadStagingGrid(); });
    $('#moveBtn').on('click', makeLiveMonthlyTime);
    $('#deleteAllWGBtn').on('click', deleteAllMonthlyTime);
    $('#exportExcel').on('click', exportMonthlyTime);
});

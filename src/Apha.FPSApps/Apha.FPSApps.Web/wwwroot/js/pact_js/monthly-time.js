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

    $.ajax({
        url: '/PACT/MonthlyTime/GetStaffByWorkGroup',
        type: 'GET',
        data: { workGroup: workGroup },
        success: function (data) {
            window.monthlyTimeStaffDropdown.updateData(Array.isArray(data) ? data : []);
        },
        error: function () {
            window.monthlyTimeStaffDropdown.updateData([]);
            showAlertMessage('Failed to load staff options.', AlertType.ERROR);
        }
    });
}

function loadTimeCodesByWorkGroup(workGroup) {
    resetTimeCodeOptions();
    resetParentProjectOptions();

    if (!workGroup) {
        return;
    }

    $.ajax({
        url: '/PACT/MonthlyTime/GetTimeCodesByWorkGroup',
        type: 'GET',
        data: { workGroup: workGroup },
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            const $timeCode = $('#ddTimeCode');
            items.forEach(function (item) {
                $timeCode.append($('<option>', { value: item.value, text: item.text }));
            });
        },
        error: function () {
            showAlertMessage('Failed to load timecode options.', AlertType.ERROR);
        }
    });
}

function loadProjectsByWorkGroupAndTimeCode(workGroup, timeCode) {
    resetParentProjectOptions();

    if (!workGroup || !timeCode) {
        return;
    }

    $.ajax({
        url: '/PACT/MonthlyTime/GetProjectsByWorkGroupAndTimeCode',
        type: 'GET',
        data: { workGroup: workGroup, timeCode: timeCode },
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            const $parentProject = $('#ddParentProject');
            items.forEach(function (item) {
                $parentProject.append($('<option>', { value: item.value, text: item.text }));
            });
        },
        error: function () {
            showAlertMessage('Failed to load parent project options.', AlertType.ERROR);
        }
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
    $.ajax({
        url: '/PACT/MonthlyTime/GetLiveRecord',
        type: 'GET',
        data: parsed,
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            const workGroup      = $('#LiveWorkGroup').val();
            const existingName   = $('#LiveName').val();
            const existingPactId = $('#LivePactStaffId').val();
            initLiveModalDropdowns(workGroup, existingName, existingPactId);
        },
        error: function () {
            showAlertMessage('Failed to load monthly time record.', AlertType.ERROR);
        }
    });
}

function initLiveModalDropdowns(existingWorkGroup, existingName, existingPactId) {
    window.liveWorkGroupDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'liveWorkGroup',
        containerSelector: '#live-modal-workgroup-dropdown-container',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: 'Work Group <span class="app-required" aria-hidden="true">*</span>',
        columns: [
            { field: 'text', header: 'Work Group', width: '200px' }
        ],
        data: $('#ddWorkGroup option').filter(function () { return $(this).val() !== ''; }).map(function () {
            return { value: $(this).val(), text: $(this).text() };
        }).get(),
        displayField: function (row) { return row.text || ''; },
        valueField: function (row) { return row.value || ''; },
        enableSearch: true,
        showSerialNumber: false,
        callbacks: {
            onSelect: function (selectedItem) {
                $('#LiveWorkGroup').val(selectedItem?.value || '');
                loadLiveModalStaffByWorkGroup('');
            },
            onClear: function () {
                $('#LiveWorkGroup').val('');
                if (window.liveNameDropdown) window.liveNameDropdown.updateData([]);
                $('#LivePactStaffId').val('');
                $('#LiveName').val('');
            }
        }
    });

    window.liveNameDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'liveName',
        containerSelector: '#live-modal-name-dropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: '',
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
                $('#LiveName').val(selectedItem?.name || '');
                $('#LivePactStaffId').val(selectedItem?.pactId || '');
            },
            onClear: function () {
                $('#LiveName').val('');
                $('#LivePactStaffId').val('');
            }
        }
    });

    if (existingWorkGroup) {
        window.liveWorkGroupDropdown.setValue(existingWorkGroup);
        loadLiveModalStaffByWorkGroup(existingWorkGroup, existingName, existingPactId);
    }
}

function loadLiveModalStaffByWorkGroup(workGroup, restoreName, restorePactId) {
    if (!window.liveNameDropdown) return;

    window.liveNameDropdown.clear();
    $('#LiveName').val('');
    $('#LivePactStaffId').val('');

    $.ajax({
        url: '/PACT/MonthlyTime/GetStaffByWorkGroup',
        type: 'GET',
        data: { workGroup: workGroup },
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            window.liveNameDropdown.updateData(items);

            if (restoreName || restorePactId) {
                const match = items.find(function (x) {
                    return (restorePactId && x.pactId === restorePactId) || (restoreName && x.name === restoreName);
                });
                if (match) {
                    window.liveNameDropdown.setValue(match.pactId);
                    // Set hidden inputs explicitly in case setValue does not fire onSelect
                    $('#LiveName').val(match.name);
                    $('#LivePactStaffId').val(match.pactId);
                } else {
                    // Staff not in list (e.g. inactive) — show stored values directly
                    $('#LiveName').val(restoreName || '');
                    $('#LivePactStaffId').val(restorePactId || '');
                }
            }
        },
        error: function () {
            window.liveNameDropdown.updateData([]);
            showAlertMessage('Failed to load staff options.', AlertType.ERROR);
        }
    });
}

function saveMonthlyTimeLive() {
    const form = $('#monthlyTimeLiveForm');
    clearValidationErrors(form);

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, form);
        return;
    }

    const data = {
        CompositeKey: $('#CompositeKey').val(),
        WorkGroup: $('#LiveWorkGroup').val(),
        PactStaffId: $('#LivePactStaffId').val(),
        Name: $('#LiveName').val(),
        TimeCode: $('#TimeCode').val(),
        ParentProject: $('#ParentProject').val(),
        Month: $('#LiveMonth').val(),
        Hours: $('#LiveHours').val()
    };

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
    $.ajax({
        url: '/PACT/MonthlyTime/AddStagingRecord',
        type: 'GET',
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            initStagingModalDropdowns(null);
        },
        error: function () {
            showAlertMessage('Failed to load add form.', AlertType.ERROR);
        }
    });
}

function editStagingMonthlyTime(btn) {
    const id = $(btn).data('id');
    $.ajax({
        url: '/PACT/MonthlyTime/GetStagingRecord',
        type: 'GET',
        data: { id: id },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            const workGroup  = $('#StagingWorkGroup').val();
            const existingName      = $('#StagingName').val();
            const existingPactId    = $('#StagingPactStaffId').val();
            const existingTimeCode  = $('#StagingTimeCode').val();
            initStagingModalDropdowns(workGroup, existingName, existingPactId, existingTimeCode);
        },
        error: function () {
            showAlertMessage('Failed to load staging record.', AlertType.ERROR);
        }
    });
}

function initStagingModalDropdowns(existingWorkGroup, existingName, existingPactId, existingTimeCode) {
    // Read work-group list from the JSON block embedded by the partial
    var wgData = [];
    var $wgJson = $('#staging-modal-workgroups-data');
    if ($wgJson.length) {
        try { wgData = JSON.parse($wgJson.text()); } catch (e) { wgData = []; }
    }

    window.stagingWorkGroupDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'stagingWorkGroup',
        containerSelector: '#staging-modal-workgroup-dropdown-container',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: 'Work Group <span class="app-required" aria-hidden="true">*</span>',
        columns: [
            { field: 'text', header: 'Work Group', width: '200px' }
        ],
        data: wgData,
        displayField: function (row) { return row.text || ''; },
        valueField: function (row) { return row.value || ''; },
        enableSearch: true,
        showSerialNumber: false,
        callbacks: {
            onSelect: function (selectedItem) {
                $('#StagingWorkGroup').val(selectedItem?.value || '');
                //loadStagingModalStaffByWorkGroup(selectedItem?.value || '');
                //loadStagingModalTimeCodesByWorkGroup(selectedItem?.value || '');
            },
            onClear: function () {
                $('#StagingWorkGroup').val('');
                // Reload all-data lists so the user can still pick freely without a WG
                loadAllStagingModalStaff();
                loadAllStagingModalTimeCodes();
                loadAllStagingModalProjects();
            }
        }
    });

    window.stagingNameDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'stagingName',
        containerSelector: '#staging-modal-name-dropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: '',
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
                $('#StagingName').val(selectedItem?.name || '');
                $('#StagingPactStaffId').val(selectedItem?.pactId || '');
            },
            onClear: function () {
                $('#StagingName').val('');
                $('#StagingPactStaffId').val('');
            }
        }
    });

    if (existingWorkGroup) {
        // EDIT mode: restore selections using dependent per-WG data
        window.stagingWorkGroupDropdown.setValue(existingWorkGroup);

        // Load staff so Name dropdown is populated, then restore the selected name display
        loadStagingModalStaffByWorkGroup(existingWorkGroup, existingName, existingPactId);

        // Only reload time codes when NOT already pre-populated by the server
        if (!existingTimeCode) {
            loadStagingModalTimeCodesByWorkGroup(existingWorkGroup);
        }
    } else {
        // ADD mode: load all available data so user can pick without selecting WG first
        loadAllStagingModalStaff();
        loadAllStagingModalTimeCodes();
        loadAllStagingModalProjects();
    }

    $('#StagingTimeCode').on('change', function () {
        const workGroup = $('#StagingWorkGroup').val();
        const timeCode  = $(this).val();
        if (workGroup) {
            // WG already chosen — filter projects by WG + TC
            loadStagingModalParentProjectsByWorkGroupAndTimeCode(workGroup, timeCode);
        }
        // No WG yet in ADD mode: projects list stays as all-data
    });
}

function loadStagingModalStaffByWorkGroup(workGroup, restoreName, restorePactId) {
    if (!window.stagingNameDropdown) return;

    window.stagingNameDropdown.clear();
    $('#StagingName').val('');
    $('#StagingPactStaffId').val('');

    if (!workGroup) {
        window.stagingNameDropdown.updateData([]);
        return;
    }

    $.ajax({
        url: '/PACT/MonthlyTime/GetStaffByWorkGroup',
        type: 'GET',
        data: { workGroup: workGroup },
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            window.stagingNameDropdown.updateData(items);

            if (restoreName || restorePactId) {
                const match = items.find(function (x) {
                    return (restorePactId && x.pactId === restorePactId) || (restoreName && x.name === restoreName);
                });
                if (match) {
                    window.stagingNameDropdown.setValue(match.pactId);
                    // Set hidden inputs explicitly in case setValue does not fire onSelect
                    $('#StagingName').val(match.name);
                    $('#StagingPactStaffId').val(match.pactId);
                } else {
                    // Employee not in list (e.g. inactive) — show stored text directly
                    $('#StagingName').val(restoreName || '');
                    $('#StagingPactStaffId').val(restorePactId || '');
                }
            }
        },
        error: function () {
            window.stagingNameDropdown.updateData([]);
            showAlertMessage('Failed to load staff options.', AlertType.ERROR);
        }
    });
}

function loadStagingModalTimeCodesByWorkGroup(workGroup) {
    resetStagingModalTimeCodeOptions();
    resetStagingModalParentProjectOptions();

    if (!workGroup) return;

    $.ajax({
        url: '/PACT/MonthlyTime/GetTimeCodesByWorkGroup',
        type: 'GET',
        data: { workGroup: workGroup },
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            const $timeCode = $('#StagingTimeCode');
            items.forEach(function (item) {
                $timeCode.append($('<option>', { value: item.value, text: item.text }));
            });
        },
        error: function () {
            showAlertMessage('Failed to load timecode options.', AlertType.ERROR);
        }
    });
}

function loadStagingModalParentProjectsByWorkGroupAndTimeCode(workGroup, timeCode) {
    resetStagingModalParentProjectOptions();

    if (!workGroup || !timeCode) return;

    $.ajax({
        url: '/PACT/MonthlyTime/GetProjectsByWorkGroupAndTimeCode',
        type: 'GET',
        data: { workGroup: workGroup, timeCode: timeCode },
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            const $parentProject = $('#StagingParentProject');
            items.forEach(function (item) {
                $parentProject.append($('<option>', { value: item.value, text: item.text }));
            });
        },
        error: function () {
            showAlertMessage('Failed to load parent project options.', AlertType.ERROR);
        }
    });
}

function resetStagingModalTimeCodeOptions() {
    $('#StagingTimeCode').empty().append('<option value="">--select--</option>');
}

function resetStagingModalParentProjectOptions() {
    $('#StagingParentProject').empty().append('<option value="">--select--</option>');
}

function loadAllStagingModalStaff() {
    if (!window.stagingNameDropdown) return;
    $.ajax({
        url: '/PACT/MonthlyTime/GetStaffByWorkGroup',
        type: 'GET',
        success: function (data) {
            window.stagingNameDropdown.updateData(Array.isArray(data) ? data : []);
        },
        error: function () {
            showAlertMessage('Failed to load staff options.', AlertType.ERROR);
        }
    });
}

function loadAllStagingModalTimeCodes() {
    resetStagingModalTimeCodeOptions();
    $.ajax({
        url: '/PACT/MonthlyTime/GetAllTimeCodes',
        type: 'GET',
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            const $timeCode = $('#StagingTimeCode');
            items.forEach(function (item) {
                $timeCode.append($('<option>', { value: item.value, text: item.text }));
            });
        },
        error: function () {
            showAlertMessage('Failed to load timecode options.', AlertType.ERROR);
        }
    });
}

function loadAllStagingModalProjects() {
    resetStagingModalParentProjectOptions();
    $.ajax({
        url: '/PACT/MonthlyTime/GetAllProjects',
        type: 'GET',
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            const $parentProject = $('#StagingParentProject');
            items.forEach(function (item) {
                $parentProject.append($('<option>', { value: item.value, text: item.text }));
            });
        },
        error: function () {
            showAlertMessage('Failed to load parent project options.', AlertType.ERROR);
        }
    });
}

function saveStagingMonthlyTime() {
    const form = $('#stagingMonthlyTimeForm');
    clearValidationErrors(form);

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, form);
        return;
    }

    const data = {
        Id: $('#Id').val(),
        WorkGroup: $('#StagingWorkGroup').val(),
        PactStaffId: $('#StagingPactStaffId').val(),
        Name: $('#StagingName').val(),
        TimeCode: $('#StagingTimeCode').val(),
        ParentProject: $('#StagingParentProject').val(),
        Month: $('#StagingMonth').val(),
        Hours: $('#StagingHours').val()
    };

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
    showLoader();

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
        },
        complete: function () {
            hideLoader();
        }
    });
}

function validateMonthlyTime() {
    showLoader();
    $.ajax({
        url: '/PACT/MonthlyTime/Validate',
        type: 'POST',
        success: function (response) {
            if (response.success) {
                reloadStagingGrid();
                showAlertMessage(response.message, AlertType.SUCCESS);
            } else {
                showAlertMessage(response.message || 'Validation failed.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred during validation.', AlertType.ERROR);
        },
        complete: function () {
            hideLoader();
        }
    });
}

function makeLiveMonthlyTime() {
    showLoader();
    $.ajax({
        url: '/PACT/MonthlyTime/MakeLive',
        type: 'POST',
        success: function (response) {
            if (response.success) {
                reloadLiveGrid();
                reloadStagingGrid();
                showAlertMessage(response.message, AlertType.SUCCESS);
            } else {
                showAlertMessage(response.message || 'Make live failed.', AlertType.ERROR);
            }
        },
        error: function (xhr) {
            showAlertMessage(xhr.responseJSON?.message || 'An error occurred during make live.', AlertType.ERROR);
        },
        complete: function () {
            hideLoader();
        }
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

function deleteFailedMonthlyTime() {
    showGovukConfirm('Delete failed imported records for the current user?').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/MonthlyTime/DeleteFailedStagingRecords',
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    reloadStagingGrid();
                    showAlertMessage('Failed imported records deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage(response.message || 'Delete failed records failed.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting failed imported records.', AlertType.ERROR);
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
    $('#deleteFailedWGBtn').on('click', deleteFailedMonthlyTime);
    $('#exportExcel').on('click', exportMonthlyTime);
});

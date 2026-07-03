var selectedUserId = 0;
var baselinePermissions = null;

function snapshotPermissions() {
    return {
        profitCentres: getCheckedValues('profitCentres'),
        programs: getCheckedValues('programs'),
        categories: getCheckedValues('categories'),
        testOwners: getCheckedValues('testOwners'),
        projectGroups: getCheckedValues('projectGroups')
    };
}

function hasPermissionsChanged() {
    if (!baselinePermissions) return false;
    var current = snapshotPermissions();
    var keys = ['profitCentres', 'programs', 'categories', 'testOwners', 'projectGroups'];
    for (var i = 0; i < keys.length; i++) {
        var key = keys[i];
        if (current[key].length !== baselinePermissions[key].length) return true;
        for (var j = 0; j < current[key].length; j++) {
            if (current[key][j] !== baselinePermissions[key][j]) return true;
        }
    }
    return false;
}

function updateSaveButtonState() {
    $('#mupSaveBtn').prop('disabled', !hasPermissionsChanged());
}

// --- Row selection handler (called by DataGrid component on row click) ---
window.onUserRowSelect = function (rowElement) {
    var $row = $(rowElement);
    var userId = $row.data('id');
    if (!userId) return;

    selectedUserId = parseInt(userId);
    var username = $row.find('td[data-property="Comments"] span').text().trim()
                || $row.find('td[data-property="Username"] span').text().trim();
    $('#mupSelectedUser').val(username || 'User ' + userId);
    loadPermissions(selectedUserId);
};

function loadPermissions(userId) {
    $.ajax({
        url: '/FPS/UserPermission/GetPermissions',
        type: 'GET',
        data: { userId: userId },
        success: function (result) {
            if (result.success) {
                applyPermissions(result.data);
                $('#permissionPanelsContainer').show();
                $('#saveToolbar').show();
            } else {
                showAlertMessage(result.message || 'Failed to load permissions.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred while loading permissions.', AlertType.ERROR);
        }
    });
}

function applyPermissions(data) {
    // Uncheck all first
    $('.permission-item').prop('checked', false);
    // Clear any free-text entries
    $('#testOwners_newEntries').empty();
    $('#testOwners_newEntry').val('');
    // For testOwners: clear existing checkboxes and rebuild with only assigned values
    $('#panel_testOwners .fps-mup-checkbox-label').remove();
    var assignedTestOwners = data.testOwners || [];
    assignedTestOwners.forEach(function (val) {
        var cbId = 'cb_testOwners_' + val.replace(/ /g, '_');
        var label = '<label class="fps-mup-checkbox-label" for="' + cbId + '">' +
            '<input type="checkbox" id="' + cbId + '" class="fps-mup-checkbox permission-item" value="' + val + '" checked />' +
            '<span class="fps-mup-checkbox-text">' + val + '</span>' +
            '</label>';
        // Insert before the free-text container
        var $freeTextContainer = $('#testOwners_freeTextContainer');
        if ($freeTextContainer.length) {
            $freeTextContainer.before(label);
        } else {
            $('#panel_testOwners').append(label);
        }
    });
    // Check assigned ones for other panels
    checkItems('profitCentres', data.profitCentres || []);
    checkItems('programs', data.programs || []);
    checkItems('categories', data.categories || []);
    checkItems('projectGroups', data.projectGroups || []);
    // Update all counts
    updateAllCounts();
    // Snapshot baseline and disable Save
    baselinePermissions = snapshotPermissions();
    $('#mupSaveBtn').prop('disabled', true);
}

function checkItems(panelId, values) {
    values.forEach(function (val) {
        $('#panel_' + panelId + ' input[type="checkbox"][value="' + val + '"]').prop('checked', true);
    });
}

function getCheckedValues(panelId) {
    var values = [];
    $('#panel_' + panelId + ' input[type="checkbox"].permission-item:checked').each(function () {
        values.push($(this).val());
    });
    // Include free-text new entries for testOwners
    if (panelId === 'testOwners') {
        $('#testOwners_newEntries .fps-mup-freetext-entry-input').each(function () {
            var val = $(this).val().trim().toUpperCase();
            if (val && values.indexOf(val) === -1) {
                values.push(val);
            }
        });
        // Also include the value in the active text box even if "+" was not clicked
        var pendingVal = $('#testOwners_newEntry').val().trim().toUpperCase();
        if (pendingVal && pendingVal.length <= 2 && values.indexOf(pendingVal) === -1) {
            values.push(pendingVal);
        }
    }
    return values;
}

function savePermissions() {
    if (selectedUserId <= 0) {
        showAlertMessage('Please select a user first.', AlertType.INFO);
        return;
    }

    var data = {
        UserId: selectedUserId,
        ProfitCentres: getCheckedValues('profitCentres'),
        Programs: getCheckedValues('programs'),
        Categories: getCheckedValues('categories'),
        TestOwners: getCheckedValues('testOwners'),
        ProjectGroups: getCheckedValues('projectGroups')
    };

    $.ajax({
        url: '/FPS/UserPermission/SavePermissions',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                showAlertMessage(result.message || 'Permissions saved successfully.', AlertType.SUCCESS).then(function () {
                    loadPermissions(selectedUserId);
                });
            } else {
                showAlertMessage(result.message || 'Failed to save permissions.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred while saving permissions.', AlertType.ERROR);
        }
    });
}

// --- User CRUD ---
function addUser(btn) {
    $.ajax({
        url: '/FPS/UserPermission/Create',
        type: 'GET',
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass("show");
        },
        error: function () {
            showAlertMessage('An error occurred while loading the form.', AlertType.ERROR);
        }
    });
}

function saveUser() {
    clearValidationErrors('#modaPopupBody');
    var form = $('#addUserForm');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    var email = form.find('[name="UserEmail"]').val();
    if (email && !isValidEmail(email)) {
        displayServerValidationErrors([{ field: 'UserEmail', message: 'Please enter a valid email address' }], 'Please correct the errors below.', '#modaPopupBody');
        return;
    }

    var data = {
        UserId: 0,
        Username: form.find('[name="Username"]').val(),
        Comments: form.find('[name="Comments"]').val(),
        UserEmail: form.find('[name="UserEmail"]').val(),
        Dt2Username: form.find('[name="Dt2Username"]').val()
    };

    $.ajax({
        url: '/FPS/UserPermission/Create',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                closeModal();
                showAlertMessage(result.message || 'User created successfully.', AlertType.SUCCESS);
                window['gridManager_userPermissionGrid'].reloadGrid({ page: 1 });
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

function editUser(btn) {
    var userId = $(btn).data('id');
    $.ajax({
        url: '/FPS/UserPermission/Edit',
        type: 'GET',
        data: { userId: userId },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass("show");
        },
        error: function () {
            showAlertMessage('An error occurred while loading the form.', AlertType.ERROR);
        }
    });
}

function updateUser() {
    clearValidationErrors('#modaPopupBody');
    var form = $('#editUserForm');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    var email = form.find('[name="UserEmail"]').val();
    if (email && !isValidEmail(email)) {
        displayServerValidationErrors([{ field: 'UserEmail', message: 'Please enter a valid email address' }], 'Please correct the errors below.', '#modaPopupBody');
        return;
    }

    var data = {
        UserId: parseInt(form.find('[name="UserId"]').val()),
        Username: form.find('[name="Username"]').val(),
        Comments: form.find('[name="Comments"]').val(),
        UserEmail: form.find('[name="UserEmail"]').val(),
        Dt2Username: form.find('[name="Dt2Username"]').val()
    };

    $.ajax({
        url: '/FPS/UserPermission/Edit',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                closeModal();
                showAlertMessage(result.message || 'User updated successfully.', AlertType.SUCCESS);
                window['gridManager_userPermissionGrid'].reloadGrid({ page: 1 });
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

function deleteUser(btn) {
    var userId = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this user?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/FPS/UserPermission/Delete',
            type: 'DELETE',
            data: { userId: userId },
            success: function (response) {
                if (response.success) {
                    showAlertMessage('User deleted successfully.', AlertType.SUCCESS).then(function () {
                        window['gridManager_userPermissionGrid'].reloadGrid({ page: 1 });
                        selectedUserId = 0;
                        $('#mupSelectedUser').val('No user selected');
                        $('#permissionPanelsContainer').hide();
                        $('#saveToolbar').hide();
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

function closeModal() {
    clearValidationErrors('#modaPopupBody');
    $('#modaPopupBody').html("");
    $('#modalPopup').removeClass("show");
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

function isValidEmail(email) {
    var pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return pattern.test(email);
}

function updateCount(panelId) {
    var count = $('#panel_' + panelId + ' input.permission-item:checked').length;
    // Include free-text entries in count for testOwners
    if (panelId === 'testOwners') {
        count += $('#testOwners_newEntries .fps-mup-freetext-entry-input').filter(function () {
            return $(this).val().trim() !== '';
        }).length;
    }
    $('#' + panelId + 'Count').text(count + ' selected');
}

function updateAllCounts() {
    updateCount('profitCentres');
    updateCount('programs');
    updateCount('categories');
    updateCount('testOwners');
    updateCount('projectGroups');
}

// --- Free-text entry for Test Owner Permissions ---
function addFreeTextEntry(panelId) {
    var $input = $('#' + panelId + '_newEntry');
    var value = $input.val().trim().toUpperCase();

    if (!value) {
        return;
    }

    if (value.length > 2) {
        showAlertMessage('Test owner code must be a maximum of 2 characters.', AlertType.INFO);
        return;
    }

    // Check for duplicates in existing checkboxes
    var existsInCheckboxes = false;
    $('#panel_' + panelId + ' input[type="checkbox"].permission-item').each(function () {
        if ($(this).val().toUpperCase() === value) {
            existsInCheckboxes = true;
            return false;
        }
    });
    if (existsInCheckboxes) {
        showAlertMessage('This test owner code already exists in the list.', AlertType.INFO);
        return;
    }

    // Check for duplicates in already added free-text entries
    var existsInNewEntries = false;
    $('#' + panelId + '_newEntries .fps-mup-freetext-entry-input').each(function () {
        if ($(this).val().trim().toUpperCase() === value) {
            existsInNewEntries = true;
            return false;
        }
    });
    if (existsInNewEntries) {
        showAlertMessage('This test owner code has already been added.', AlertType.INFO);
        return;
    }

    // Add new entry row
    var entryHtml = '<div class="fps-mup-freetext-entry-row">' +
        '<input type="text" class="govuk-input fps-mup-freetext-entry-input" value="' + value + '" maxlength="2" readonly />' +
        '<button type="button" class="fps-mup-freetext-remove-btn" onclick="removeFreeTextEntry(this, \'' + panelId + '\')" title="Remove" aria-label="Remove entry">&times;</button>' +
        '</div>';
    $('#' + panelId + '_newEntries').append(entryHtml);

    // Clear input and add a new empty text box
    $input.val('');
    $input.focus();

    // Update count and save button state
    updateCount(panelId);
    updateSaveButtonState();
}

function removeFreeTextEntry(btn, panelId) {
    $(btn).closest('.fps-mup-freetext-entry-row').remove();
    updateCount(panelId);
    updateSaveButtonState();
}

// Update counts and Save button state when checkboxes change
$(document).on('change', '.permission-item', function () {
    var panelId = $(this).closest('[id^="panel_"]').attr('id').replace('panel_', '');
    updateCount(panelId);
    updateSaveButtonState();
});

// Allow Enter key to add free-text entry
$(document).on('keypress', '.fps-mup-freetext-input', function (e) {
    if (e.which === 13) {
        e.preventDefault();
        var panelId = $(this).attr('id').replace('_newEntry', '');
        addFreeTextEntry(panelId);
    }
});

// Enable Save button when user types in the free-text input
$(document).on('input', '.fps-mup-freetext-input', function () {
    var val = $(this).val().trim();
    if (val) {
        $('#mupSaveBtn').prop('disabled', false);
    } else {
        updateSaveButtonState();
    }
});

// --- Auto-select first row on grid load/reload ---
function selectFirstRow() {
    var $firstRow = $('#gridContainer_' + window.userPermissionGridId + ' table tbody tr:first');
    if ($firstRow.length && $firstRow.data('id')) {
        $firstRow.closest('table').find('tbody tr').removeClass('selected-row');
        $firstRow.addClass('selected-row');
        window.onUserRowSelect($firstRow[0]);
    }
}

// After grid reloads (pagination, filter, sort, CRUD), auto-select first row
document.addEventListener('gridReloaded', function (e) {
    if (e.detail && e.detail.gridId === window.userPermissionGridId) {
        selectFirstRow();
    }
});

// Auto-select first row on initial page load
$(document).ready(function () {
    selectFirstRow();
});

// Portfolio Time Codes - JavaScript Module
// Manages job code and time code grids with CRUD operations

let currentParentProject = '';
let currentJobCodeId = '';
let currentTestCode = '';
let jobCodeGridId = '';
let timeCodeGridId = '';
let portfolioDropdown = null;
let isInitializingPortfolioDropdown = false;

// Initialize the module with grid IDs and selected portfolio
function initPortfolioTimeCodes(selectedPortfolio, jobCodeGrid, timeCodeGrid) {
    currentParentProject = selectedPortfolio;
    jobCodeGridId = jobCodeGrid;
    timeCodeGridId = timeCodeGrid;
}

// Initialize Portfolio Multi-Column Dropdown
function initializePortfolioMultiDropdown() {
    // Wait for DOM and ensure data is available
    if (typeof portfolioOptionsListData === 'undefined') {
        return;
    }

    isInitializingPortfolioDropdown = true;

    // Parse the portfolio options to extract code and title
    const portfolioData = portfolioOptionsListData.map(function(option) {
        // The Text format is "CODE - Title", split it
        const parts = option.Text.split(' - ');
        return {
            Value: option.Value,
            Code: parts[0] || option.Value,
            Title: parts[1] || ''
        };
    });

    portfolioDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'portfolioDropdown',
        containerSelector: '#portfolioMultiDropdown',
        placeholder: 'Select Portfolio',
        showSerialNumber: false,
        searchPlaceholder: 'Search by code or title',
        labelText: '',
        columns: [
            { field: 'Code', header: 'Portfolio Code', width: '120px' },
            { field: 'Title', header: 'Project Title', width: '300px' }
        ],
        data: portfolioData,
        displayField: 'Code',
        valueField: 'Value',
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem, dropdown) {
                // Only navigate if not initializing
                if (!isInitializingPortfolioDropdown) {
                    onPortfolioChange(selectedItem.Value);
                }
            },
            onClear: function (dropdown) {
                // Only navigate if not initializing
                if (!isInitializingPortfolioDropdown) {
                    onPortfolioChange('');
                }
            }
        }
    });

    // Set initial value if a portfolio is already selected
    if (selectedPortfolioValue && selectedPortfolioValue !== '') {
        portfolioDropdown.setValue(selectedPortfolioValue);
    }

    // Allow navigation after a short delay to ensure setValue completes
    setTimeout(function() {
        isInitializingPortfolioDropdown = false;
    }, 500);
}

// Initialize dropdown on DOM ready
document.addEventListener('DOMContentLoaded', function () {
    if (typeof portfolioOptionsListData !== 'undefined') {
        initializePortfolioMultiDropdown();
    }

    // Make the Active checkboxes clickable on the time code grid
    enableTimeCodeActiveCheckboxes();

    // Re-enable the checkboxes whenever the time code grid reloads
    document.addEventListener('gridReloaded', function (e) {
        if (e.detail && e.detail.gridId === (timeCodeGridId || 'timeCodeGrid')) {
            enableTimeCodeActiveCheckboxes();
        }
    });

    // Active/Inactive toggle on the time code grid
    $(document).on('change', 'td.checkbox-cell[data-property="Active"] input[type="checkbox"]', function () {
        if ($(this).closest('table').attr('id') === 'tbl_' + (timeCodeGridId || 'timeCodeGrid')) {
            toggleTimeCodeActive(this);
        }
    });
});

function toggleSidebar() {
    document.querySelector('.sidenav').classList.toggle('collapsed');
}

function getJobCodeGridManager() {
    return window['gridManager_' + jobCodeGridId];
}

function getTimeCodeGridManager() {
    return window['gridManager_' + timeCodeGridId];
}

// Portfolio Change Handler
function onPortfolioChange(parentProject) {
    if (parentProject) {
        window.fpsNavigateTo('/PACT/PortfolioTimeCodes/Index?parentProject=' + encodeURIComponent(parentProject));

    } else {
        window.fpsNavigateTo('/PACT/PortfolioTimeCodes/Index');
    }
}

// ========================================
// Job Code Functions
// ========================================

function addJobCode() {
    if (!currentParentProject) {
        showAlertMessage('Please select a portfolio first.', AlertType.INFO);
        return;
    }

    $.ajax({
        url: '/PACT/PortfolioTimeCodes/CreateJobCode',
        type: 'GET',
        data: { parentProject: currentParentProject },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        },
        error: function () {
            showAlertMessage('Failed to load add job code form.', AlertType.ERROR);
        }
    });
}

function editJobCode(btn) {
    var jobCodeId = $(btn).data('id');
    $.ajax({
        url: '/PACT/PortfolioTimeCodes/EditJobCode',
        type: 'GET',
        data: { jobCodeId: jobCodeId },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        },
        error: function () {
            showAlertMessage('Failed to load edit job code form.', AlertType.ERROR);
        }
    });
}

function deleteJobCode(btn) {
    var jobCodeId = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this job code?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/PortfolioTimeCodes/DeleteJobCode',
            type: 'DELETE',
            data: { jobCodeId: jobCodeId, parentProject: currentParentProject },
            success: function (response) {
                if (response.success) {
                    refreshJobCodeGrid();
                    showAlertMessage(response.message || 'Job code deleted successfully.', AlertType.SUCCESS);

                    // If the deleted job code was selected, clear the time code grid
                    if (currentJobCodeId === jobCodeId) {
                        currentJobCodeId = '';
                        const gridManager = getTimeCodeGridManager();
                        if (gridManager) {
                            gridManager.clearGrid();
                        } else {
                            $('#gridContainer_' + timeCodeGridId).html('<p class="sup_p_8">Select a job code to view time codes.</p>');
                        }
                    }
                } else {
                    showAlertMessage('Error: ' + (response.message || 'Failed to delete job code'), AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting job code.', AlertType.ERROR);
            }
        });
    });
}

function saveJobCode() {
    const form = $('#jobCodeForm');
    const isEdit = form.find('input[name="isEdit"]').val() === 'true';
    const url = isEdit ? '/PACT/PortfolioTimeCodes/EditJobCode' : '/PACT/PortfolioTimeCodes/CreateJobCode';

    const data = {
        JobCodeId: form.find('[name="JobCodeId"]').val(),
        ParentProject: form.find('[name="ParentProject"]').val(),
        JobCodeName: form.find('[name="JobCodeName"]').val(),
        Type: form.find('[name="Type"]').val(),
        JobCodeWorkGroup: form.find('[name="JobCodeWorkGroup"]').val()
    };

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                refreshJobCodeGrid();
                if (isEdit) {
                    showAlertMessage(response.message || 'Job code updated successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage(response.message || 'Job code saved successfully.', AlertType.SUCCESS);
                }
            } else {
                displayServerValidationErrors(response.errors, response.message, '#jobCodeForm');
            }
        },
        error: function () {
            showAlertMessage('Failed to save job code.', AlertType.ERROR);
        }
    });
}

function selectJobCode(row) {
    var jobCodeId = $(row).data('id');

    if (!jobCodeId) {
        showAlertMessage('Error: Could not get Job Code ID from selected row', AlertType.ERROR);
        return;
    }

    currentJobCodeId = jobCodeId;
    currentTestCode = ''; // Reset test code when job code changes
    refreshTimeCodeGrid();
}

function selectTimeCode(row) {
    var testCode = $(row).data('testcode');
    if (testCode) {
        currentTestCode = testCode;
        // Optionally refresh or do something with the selected test code
    }
}

// ========================================
// Time Code Functions
// ========================================

function addTimeCode() {
    if (!currentParentProject) {
        showAlertMessage('Please select a portfolio first.', AlertType.INFO);
        return;
    }

    $.ajax({
        url: '/PACT/PortfolioTimeCodes/CreateTimeCode',
        type: 'GET',
        data: {
            parentProject: currentParentProject,
            jobCodeId: currentJobCodeId
        },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        },
        error: function () {
            showAlertMessage('Failed to load add time code form.', AlertType.ERROR);
        }
    });
}

function editTimeCode(btn) {
    var timeCode = $(btn).data('id');
    var $row = $(btn).closest('tr');
    var workGroup = $row.find('[data-property="WorkGroup"]').text().trim();

    if (!timeCode) {
        showAlertMessage('Error: Time Code is not set', AlertType.ERROR);
        return;
    }

    if (!workGroup) {
        showAlertMessage('Error: Work Group is not set.', AlertType.ERROR);
        return;
    }

    if (!currentParentProject) {
        showAlertMessage('Error: Parent project is not set.', AlertType.ERROR);
        return;
    }

    var requestUrl = '/PACT/PortfolioTimeCodes/EditTimeCode';
    var requestData = {
        workGroup: workGroup,
        timeCode: timeCode,
        jobCodeId: currentJobCodeId || '',
        parentProject: currentParentProject
    };

    $.ajax({
        url: requestUrl,
        type: 'GET',
        data: requestData,
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        },
        error: function (xhr, status, error) {
            var errorMessage = 'Failed to load edit time code form.';
            if (xhr.status === 404) {
                errorMessage = 'Time code not found. It may have been deleted.';
            } else if (xhr.status === 400) {
                errorMessage = 'Bad request: ' + (xhr.responseText || 'Invalid parameters');
            } else if (xhr.status === 500) {
                errorMessage = 'Server error: ' + (xhr.responseText || 'Please check the server logs');
            } else if (xhr.responseText) {
                errorMessage = 'Error: ' + xhr.responseText;
            }

            showAlertMessage(errorMessage, AlertType.ERROR);
        }
    });
}

function deleteTimeCode(btn) {
    var timeCode = $(btn).data('id');
    var workGroup = $(btn).closest('tr').find('[data-property="WorkGroup"] span').text().trim();
    showGovukConfirm('Are you sure you want to delete this time code?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/PortfolioTimeCodes/DeleteTimeCode',
            type: 'DELETE',
            data: {
                workGroup: workGroup,
                timeCode: timeCode,
                parentProject: currentParentProject
            },
            success: function (response) {
                if (response.success) {
                    refreshTimeCodeGrid();
                    showAlertMessage(response.message || 'Time code deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Error: ' + (response.message || 'Failed to delete time code'), AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting time code.', AlertType.ERROR);
            }
        });
    });
}

function saveTimeCode() {
    const form = $('#timeCodeForm');
    const isEdit = form.find('input[name="isEdit"]').val() === 'true';
    const url = isEdit ? '/PACT/PortfolioTimeCodes/EditTimeCode' : '/PACT/PortfolioTimeCodes/CreateTimeCode';

    // Helper function to get trimmed value or null
    function getValueOrNull(selector) {
        const value = form.find(selector).val();
        return value && value.trim() !== '' ? value.trim() : null;
    }

    // Business Rule: JobCode is mutually exclusive with Portfolio/TestCode
    // Only send non-disabled fields
    const jobCode = form.find('[name="JobCode"]').prop('disabled') ? null : getValueOrNull('[name="JobCode"]');
    const testCode = form.find('[name="TestCode"]').prop('disabled') ? null : getValueOrNull('[name="TestCode"]');
    const portfolio = form.find('[name="Portfolio"]').prop('disabled') ? null : getValueOrNull('[name="Portfolio"]');

    const data = {
        WorkGroup: form.find('[name="WorkGroup"]').val(),
        TimeCode: form.find('[name="TimeCode"]').val(),
        ParentProject: form.find('[name="ParentProject"]').val(),
        JobCode: jobCode,
        Active: form.find('[name="Active"]').is(':checked'),
        Project: form.find('[name="Project"]').val(),
        TestCode: testCode,
        Portfolio: portfolio,
        OriginalWorkGroup: form.find('[name="OriginalWorkGroup"]').val()
    };

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                refreshTimeCodeGrid();
                if (isEdit) {
                    showAlertMessage(response.message || 'Time code updated successfully.', AlertType.SUCCESS);
                } else { 
                    showAlertMessage(response.message || 'Time code saved successfully.', AlertType.SUCCESS);
                }
            } else {
                displayServerValidationErrors(response.errors, response.message, '#timeCodeForm');
            }
        },
        error: function () {
            showAlertMessage('Failed to save time code.', AlertType.ERROR);
        }
    });
}

function getTimeCodeExtraFilters() {
    return {
        parentProject: currentParentProject,
        jobCodeId: currentJobCodeId || null,
        testCode: currentTestCode || null
    };
}

// ========================================
// Active/Inactive Toggle (time code grid)
// ========================================

// Make the Active column checkboxes clickable in the time code grid.
// Editable only for an open (editable) year; for a closed / read-only year the
// checkbox stays display-only, matching how the Edit/Delete action buttons are
// disabled server-side by FPSReadOnlyTagHelper.
function enableTimeCodeActiveCheckboxes() {
    var gridId = timeCodeGridId || 'timeCodeGrid';
    var $cells = $('#gridContainer_' + gridId + ' td.checkbox-cell[data-property="Active"]');

    if (typeof isFPSYearClosed !== 'undefined' && isFPSYearClosed) {
        $cells.find('.govuk-checkboxes__item').css('pointer-events', 'none');
        $cells.find('input[type="checkbox"]').prop('disabled', true);
        return;
    }

    $cells.find('.govuk-checkboxes__item').css('pointer-events', 'auto');
    $cells.find('input[type="checkbox"]').prop('disabled', false);
}

// Toggle active/inactive: read existing values, then post the update
function toggleTimeCodeActive(checkboxEl) {
    var $chk = $(checkboxEl);
    var $row = $chk.closest('tr');
    var timeCode = $row.find('[data-property="TimeCode"]').text().trim();
    var workGroup = $row.find('[data-property="WorkGroup"]').text().trim();
    var newActive = $chk.is(':checked');

    if (!timeCode || !workGroup || !currentParentProject) {
        $chk.prop('checked', !newActive);
        showAlertMessage('Unable to determine the selected time code.', AlertType.ERROR);
        return;
    }

    // Read remaining fields from the grid row (not returned by GetTimeCodeValid)
    var rowProject = $row.find('[data-property="Project"]').text().trim();
    var rowJobCode = $row.find('[data-property="JobCode"]').text().trim();
    var rowTestCode = $row.find('[data-property="TestCode"]').text().trim();

    // 1) Read all existing values for the selected time code
    $.ajax({
        url: '/PACT/PortfolioMaintenance/GetTimeCodeValid',
        type: 'GET',
        data: { workGroup: workGroup, timeCode: timeCode, parentProject: currentParentProject },
        success: function (res) {
            if (!res || !res.success || !res.data) {
                $chk.prop('checked', !newActive);
                showAlertMessage((res && res.message) || 'Failed to load time code details.', AlertType.ERROR);
                return;
            }

            var existingData = res.data;

            // 2) Build the update payload from existing values, changing only the Active flag
            var payload = {
                workGroup: existingData.workGroup,
                originalWorkGroup: existingData.workGroup,
                timeCode: existingData.timeCode,
                project: rowProject || existingData.parentProject,
                jobCode: rowJobCode || null,
                testCode: rowTestCode || null,
                portfolio: existingData.portfolio,
                parentProject: existingData.parentProject,
                active: newActive
            };

            // 3) Persist the change
            $.ajax({
                url: '/PACT/PortfolioTimeCodes/EditTimeCode',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload),
                success: function (updateRes) {
                    if (updateRes.success) {
                        showAlertMessage(updateRes.message || 'Time code updated successfully.', AlertType.SUCCESS);
                        refreshTimeCodeGrid();
                    } else {
                        $chk.prop('checked', !newActive);
                        showAlertMessage(updateRes.message || 'Failed to update time code.', AlertType.ERROR);
                    }
                },
                error: function () {
                    $chk.prop('checked', !newActive);
                    showAlertMessage('An error occurred while updating.', AlertType.ERROR);
                }
            });
        },
        error: function () {
            $chk.prop('checked', !newActive);
            showAlertMessage('An error occurred while loading time code details.', AlertType.ERROR);
        }
    });
}

// ========================================
// Grid Refresh Functions
// ========================================

function refreshJobCodeGrid() {
    if (!currentParentProject) return;

    const gridManager = getJobCodeGridManager();
    if (gridManager) {
        gridManager.reloadGrid({ page: 1 });
    } else {
        // Fallback to manual reload
        const request = {
            page: 1,
            pageSize: 10,
            sortBy: '',
            descending: false,
            filter: '{}'
        };

        $.ajax({
            url: '/PACT/PortfolioTimeCodes/LoadJobCodeGrid',
            type: 'POST',
            data: { ...request, parentProject: currentParentProject },
            success: function (html) {
                $('#gridContainer_' + jobCodeGridId).html(html);
            },
            error: function () {
                showAlertMessage('Failed to refresh job code grid.', AlertType.ERROR);
            }
        });
    }
}

function refreshTimeCodeGrid() {
    if (!currentParentProject) {
        showAlertMessage('Cannot refresh: Missing parent project', AlertType.INFO);
        return;
    }

    const gridManager = getTimeCodeGridManager();

    if (gridManager) {
        gridManager.reloadGrid({ page: 1 });
    } else {
        // Fallback to manual reload
        const request = {
            page: 1,
            pageSize: 10,
            sortBy: '',
            descending: false,
            filter: '{}'
        };

        const postData = {
            ...request,
            parentProject: currentParentProject,
            jobCodeId: currentJobCodeId || null,
            testCode: currentTestCode || null
        };

        $.ajax({
            url: '/PACT/PortfolioTimeCodes/LoadTimeCodeGrid',
            type: 'POST',
            data: postData,
            success: function (html) {
                $('#gridContainer_' + timeCodeGridId).html(html);
            },
            error: function () {
                showAlertMessage('Failed to refresh time code grid.', AlertType.ERROR);
            }
        });
    }
}

// ========================================
// Multi-Column Dropdown for Job Code Modal
// ========================================

function initializeJobCodeWorkGroupDropdown(config) {
    setTimeout(function () {
        var isClearing = false;
        var workGroupDropdown = new MultiColumnDropdownComponent({
            dropdownId: 'workGroupDropdown',
            containerSelector: '#workGroupMultiDropdown',
            placeholder: 'Select Work Group',
            showSerialNumber: false,
            searchPlaceholder: 'Search work group',
            labelText: 'Work Group',
            columns: [
                { field: 'Value', header: 'Work Group', width: '150px' },
                { field: 'Text', header: 'Description', width: '250px' }
            ],
            data: config.workGroupData || [],
            displayField: 'Text',
            valueField: 'Value',
            clearButtonClearsSelection: true,
            callbacks: {
                onSelect: function (selectedItem, dropdown) {
                    if (!isClearing) {
                        $('#JobCodeWorkGroup').val(selectedItem.Value).trigger('change');
                    }
                },
                onClear: function (dropdown) {
                    if (!isClearing) {
                        isClearing = true;
                        $('#JobCodeWorkGroup').val('').trigger('change');
                        setTimeout(function () {
                            isClearing = false;
                        }, 50);
                    }
                }
            }
        });

        // Set initial value if provided
        if (config.selectedWorkGroup && config.selectedWorkGroup !== '') {
            workGroupDropdown.setValue(config.selectedWorkGroup);
        }
    }, 100);
}

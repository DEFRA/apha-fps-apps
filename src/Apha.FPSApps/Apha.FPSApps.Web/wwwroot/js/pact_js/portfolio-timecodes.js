// Portfolio Time Codes - JavaScript Module
// Manages job code and time code grids with CRUD operations

let currentParentProject = '';
let currentJobCodeId = '';
let currentTestCode = '';
let jobCodeGridId = '';
let timeCodeGridId = '';

// Initialize the module with grid IDs and selected portfolio
function initPortfolioTimeCodes(selectedPortfolio, jobCodeGrid, timeCodeGrid) {
    currentParentProject = selectedPortfolio;
    jobCodeGridId = jobCodeGrid;
    timeCodeGridId = timeCodeGrid;
}

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
        window.location.href = '/PACT/PortfolioTimeCodes/Index?parentProject=' + encodeURIComponent(parentProject);
    } else {
        window.location.href = '/PACT/PortfolioTimeCodes/Index';
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

    $.get('/PACT/PortfolioTimeCodes/CreateJobCode', { parentProject: currentParentProject })
        .done(function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        })
        .fail(function (xhr, status, error) {
            showAlertMessage('Failed to load add job code form.', AlertType.ERROR);
        });
}

function editJobCode(btn) {
    var jobCodeId = $(btn).data('id');
    $.get('/PACT/PortfolioTimeCodes/EditJobCode', { jobCodeId: jobCodeId })
        .done(function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        })
        .fail(function () {
            showAlertMessage('Failed to load edit job code form.', AlertType.ERROR);
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

    $.get('/PACT/PortfolioTimeCodes/CreateTimeCode', {
        parentProject: currentParentProject,
        jobCodeId: currentJobCodeId
    })
        .done(function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        })
        .fail(function () {
            showAlertMessage('Failed to load add time code form.', AlertType.ERROR);
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

    $.get(requestUrl, requestData)
        .done(function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        })
        .fail(function (xhr, status, error) {
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

        $.post('/PACT/PortfolioTimeCodes/LoadJobCodeGrid',
            { ...request, parentProject: currentParentProject })
            .done(function (html) {
                $('#gridContainer_' + jobCodeGridId).html(html);
            })
            .fail(function () {
                showAlertMessage('Failed to refresh job code grid.', AlertType.ERROR);
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

        $.post('/PACT/PortfolioTimeCodes/LoadTimeCodeGrid', postData)
            .done(function (html) {
                $('#gridContainer_' + timeCodeGridId).html(html);
            })
            .fail(function (xhr, status, error) {
                showAlertMessage('Failed to refresh time code grid.', AlertType.ERROR);
            });
    }
}

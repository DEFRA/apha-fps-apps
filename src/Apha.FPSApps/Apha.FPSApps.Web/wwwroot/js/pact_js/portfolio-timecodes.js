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
        showGovukAlert('Please select a portfolio first.');
        return;
    }

    $.get('/PACT/PortfolioTimeCodes/CreateJobCode', { parentProject: currentParentProject })
        .done(function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        })
        .fail(function (xhr, status, error) {
            showGovukAlert('Failed to load add job code form.');
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
            showGovukAlert('Failed to load edit job code form.');
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
                    showGovukAlert(response.message || 'Job code deleted successfully.');
                    refreshJobCodeGrid();

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
                    showGovukAlert('Error: ' + (response.message || 'Failed to delete job code'));
                }
            },
            error: function () {
                showGovukAlert('An error occurred while deleting job code.');
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
                if (isEdit) {
                    showGovukAlert(response.message || 'Job code updated successfully.');
                } else {
                    showGovukAlert(response.message || 'Job code saved successfully.');
                }
                refreshJobCodeGrid();
            } else {
                displayServerValidationErrors(response.errors, response.message, '#jobCodeForm');
            }
        },
        error: function () {
            showGovukAlert('Failed to save job code.');
        }
    });
}

function selectJobCode(row) {
    var jobCodeId = $(row).data('id');

    if (!jobCodeId) {
        showGovukAlert('Error: Could not get Job Code ID from selected row');
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
        showGovukAlert('Please select a portfolio first.');
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
            showGovukAlert('Failed to load add time code form.');
        });
}

function editTimeCode(btn) {
    var timeCode = $(btn).data('id');
    var $row = $(btn).closest('tr');
    var workGroup = $row.find('[data-property="WorkGroup"]').text().trim();

    if (!timeCode) {
        showGovukAlert('Error: Could not get Time Code from button');
        return;
    }

    if (!workGroup) {
        showGovukAlert('Error: Could not get Work Group from row. Please ensure the grid has loaded correctly.');
        return;
    }

    if (!currentParentProject) {
        showGovukAlert('Error: Parent project is not set.');
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

            showGovukAlert(errorMessage);
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
                    showGovukAlert(response.message || 'Time code deleted successfully.');
                    refreshTimeCodeGrid();
                } else {
                    showGovukAlert('Error: ' + (response.message || 'Failed to delete time code'));
                }
            },
            error: function () {
                showGovukAlert('An error occurred while deleting time code.');
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
                if (isEdit) {
                    showGovukAlert(response.message || 'Time code updated successfully.');
                } else { 
                    showGovukAlert(response.message || 'Time code saved successfully.');
                }
                refreshTimeCodeGrid();
            } else {
                displayServerValidationErrors(response.errors, response.message, '#timeCodeForm');
            }
        },
        error: function () {
            showGovukAlert('Failed to save time code.');
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
                alert('Failed to refresh job code grid.');
            });
    }
}

function refreshTimeCodeGrid() {
    if (!currentParentProject) {
        alert('Cannot refresh: Missing parent project');
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
                alert('Failed to refresh time code grid.');
            });
    }
}
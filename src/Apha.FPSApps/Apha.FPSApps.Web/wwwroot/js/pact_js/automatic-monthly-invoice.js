// Automatic Monthly Invoice Creation Page JavaScript

// ── State ──────────────────────────────────────────────────────────
// Note: currentMonth and automaticInvoiceGridId are initialized in the Razor view
// DO NOT redeclare them here - they are set via inline script in Index.cshtml

function getAutomaticInvoiceGridManager() {
    return window['gridManager_' + automaticInvoiceGridId];
}

// ── Month dropdown change ──────────────────────────────────────────
function onMonthPickChange(value) {
    currentMonth = value || null;
    reloadAutomaticInvoicesGrid();
}

// ── Grid reload ────────────────────────────────────────────────────
function reloadAutomaticInvoicesGrid() {
    $.ajax({
        url: '/PACT/AutomaticMonthlyInvoice/LoadInvoicesGrid',
        type: 'POST',
        data: {
            Page: 1,
            PageSize: 50,
            SortBy: 'ProjectParent',
            Descending: false,
            Filter: '{}',
            month: currentMonth || ''
        },
        success: function (html) {
            $('#gridContainer_automaticInvoiceGrid').html(html);
        },
        error: function () {
            console.error('Failed to load Automatic Invoices grid.');
        }
    });
}

// ── Extra filter method (passed to gridManager for pagination/sort) ─
function getAutomaticInvoiceFilters() {
    return {
        month: currentMonth || ''
    };
}

// ── CRUD Functions ─────────────────────────────────────────────────
function addAutomaticInvoice() {
    $.ajax({
        url: '/PACT/AutomaticMonthlyInvoice/GetInvoice',
        type: 'GET',
        data: { invoiceId: 0, selectedMonth: currentMonth || '' },
        success: function (response) {
            // Check if response is JSON (error) or HTML (success)
            if (typeof response === 'object' && response.success === false) {
                // Handle JSON error response
                showGovukAlert(response.message || 'Failed to load form');
            } else {
                // Handle HTML response (partial view)
                $('#modaPopupBody').html(response);
                $('#modalPopup').addClass('show');
            }
        },
        error: function(xhr, status, error) {
            var errorMessage = 'Error loading form: ' + error;
            try {
                var response = JSON.parse(xhr.responseText);
                if (response.message) {
                    errorMessage = response.message;
                }
            } catch (e) {
                // Not JSON, use default error message
            }
            showGovukAlert(errorMessage);
        }
    });
}

function editAutomaticInvoice(btn) {
    var id = $(btn).data('id');
    $.ajax({
        url: '/PACT/AutomaticMonthlyInvoice/GetInvoice',
        type: 'GET',
        data: { invoiceId: id },
        success: function (response) {
            // Check if response is JSON (error) or HTML (success)
            if (typeof response === 'object' && response.success === false) {
                // Handle JSON error response
                showGovukAlert(response.message || 'Failed to load invoice');
            } else {
                // Handle HTML response (partial view)
                $('#modaPopupBody').html(response);
                $('#modalPopup').addClass('show');
            }
        },
        error: function(xhr, status, error) {
            var errorMessage = 'Error loading form: ' + error;
            try {
                var response = JSON.parse(xhr.responseText);
                if (response.message) {
                    errorMessage = response.message;
                }
            } catch (e) {
                // Not JSON, use default error message
            }
            showGovukAlert(errorMessage);
        }
    });
}

function deleteAutomaticInvoice(btn) {
    var id = $(btn).data('id');
    showGovukConfirm('Delete this invoice?').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/AutomaticMonthlyInvoice/DeleteInvoice',
            type: 'DELETE',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    reloadAutomaticInvoicesGrid();
                    showGovukAlert('Invoice deleted successfully.');
                } else {
                    showGovukAlert('Error: ' + response.message);
                }
            },
            error: function () { showGovukAlert('An error occurred while deleting.'); }
        });
    });
}

function saveAutomaticInvoice() {
    var form = $('#automaticInvoiceForm');
    if (!form.length) {
        console.error('Form not found');
        return;
    }

    // Clear previous errors
    $('.govuk-error-summary').hide();
    $('.govuk-error-message').hide();
    $('.govuk-form-group').removeClass('govuk-form-group--error');

    // Collect form data
    var formData = {};
    form.find('input, select, textarea').each(function() {
        var $field = $(this);
        var name = $field.attr('name');
        if (name && name !== 'isEdit') {
            var val = $field.val();

            // Handle InvoiceCounter (hidden field)
            if (name === 'InvoiceCounter') {
                formData[name] = val ? parseInt(val, 10) : 0;
            }
            // Handle Month (select dropdown) - must be valid integer or null
            else if (name === 'Month') {
                if (val && val !== '') {
                    var monthInt = parseInt(val, 10);
                    formData[name] = isNaN(monthInt) ? null : monthInt;
                } else {
                    formData[name] = null;
                }
            }
            // Handle Amount (number input)
            else if ($field.attr('type') === 'number') {
                formData[name] = val ? parseFloat(val) : null;
            }
            // Handle other fields (strings)
            else {
                formData[name] = val || null;
            }
        }
    });

    console.log('Sending form data:', formData);

    $.ajax({
        url: '/PACT/AutomaticMonthlyInvoice/SaveInvoice',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(formData),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                reloadAutomaticInvoicesGrid();
                showGovukAlert(response.message || 'Invoice saved successfully.');
            } else {
                displayServerValidationErrors(response.errors, response.message, '#modaPopupBody');
            }
    },
        error: function () {
            showGovukAlert('An error occurred while saving.');
        }
    });
}

// ── Copy/Paste Functions ───────────────────────────────────────────

function openCopyInvoiceModal() {
    if (!currentMonth) {
        showGovukAlert('Please select a source month first.');
        return;
    }

    // Check if "Select All" checkbox is checked
    var isSelectAllChecked = $('#automaticInvoiceGrid_selectAll').is(':checked');

    var copyMode;
    var copyCount;

    if (isSelectAllChecked) {
        // Select All is checked → Bulk mode (copy all invoices from source month)
        copyMode = 'bulk';
        copyCount = ' (All invoices)';
    } else {
        // Check individual selections
        var selectedRecords = getSelectedInvoiceRecords();
        if (selectedRecords.length > 0) {
            copyMode = 'selective';
            copyCount = ` (${selectedRecords.length} invoice${selectedRecords.length > 1 ? 's' : ''})`;
        } else {
            // No records selected and Select All is not checked
            showGovukAlert('Please select at least one invoice or check "Select All" to copy all invoices.');
            return;
        }
    }

    // Set the source month in the modal
    var selectedMonthText = $('#monthPick option:selected').text();
    $('#txtSelectedMonth').val(selectedMonthText);

    // Update the modal title based on copy mode
    $('#copyInvoiceModalLabel').text('Copy Invoice' + copyCount + ' to target month');

    // Clear target month selection
    $('#targetMonthSelect').val('');

    // Clear all error messages and styles
    $('#formCopyInvoice-db-error').css('display', 'none');
    $('#modal-targetmonth-error').css('display', 'none');
    $('#fg-targetmonth').removeClass('govuk-form-group--error');
    $('#targetMonthSelect').removeClass('govuk-select--error');

    // Show the modal using the standard .show class
    $('#copyInvoiceModal').addClass('show');
}

function closeCopyInvoiceModal() {
    // Clear all error messages and styles when closing
    $('#formCopyInvoice-db-error').css('display', 'none');
    $('#modal-targetmonth-error').css('display', 'none');
    $('#fg-targetmonth').removeClass('govuk-form-group--error');
    $('#targetMonthSelect').removeClass('govuk-select--error');
    $('#targetMonthSelect').val('');

    // Reset modal title
    $('#copyInvoiceModalLabel').text('Copy Invoice to target month');

    // Close the modal
    $('#copyInvoiceModal').removeClass('show');
}

function getSelectedInvoiceRecords() {
    // Check if "Select All" is checked - if so, return empty array for bulk mode
    var isSelectAllChecked = $('#automaticInvoiceGrid_selectAll').is(':checked');
    if (isSelectAllChecked) {
        return [];
    }

    var selectedRecords = [];

    // Use the correct table ID with 'tbl_' prefix
    var $checkboxes = $('#tbl_automaticInvoiceGrid .row-checkbox:checked');

    if ($checkboxes.length === 0) {
        $checkboxes = $('input[type="checkbox"].row-checkbox:checked');
    }

    $checkboxes.each(function() {
        var $row = $(this).closest('tr');
        var invoiceCounter = parseInt($row.attr('data-id'), 10);

        if (isNaN(invoiceCounter)) {
            return;
        }

        var record = { InvoiceCounter: invoiceCounter };

        $row.find('td[data-property]').each(function() {
            var prop = $(this).attr('data-property');
            var type = $(this).attr('data-type');
            var text = $(this).find('span').first().text().trim();

            if (type === 'GbpValue' || type === 'Decimal' || type === 'Number') {
                record[prop] = text ? parseFloat(text.replace(/[£,\s]/g, '')) : null;
            } else if (type === 'Integer') {
                record[prop] = text ? parseInt(text, 10) : null;
            } else {
                record[prop] = text || null;
            }
        });

        selectedRecords.push(record);
    });

    return selectedRecords;
}

function saveCopiedInvoice() {
    try {
        // Clear all previous errors first - use simple CSS display property
        $('#formCopyInvoice-db-error').css('display', 'none');
        $('#modal-targetmonth-error').css('display', 'none');
        $('#fg-targetmonth').removeClass('govuk-form-group--error');
        $('#targetMonthSelect').removeClass('govuk-select--error');

        var targetMonth = $('#targetMonthSelect').val();

        // Validation: Check if target month is selected
        if (!targetMonth) {
            $('#modal-targetmonth-error-msg').text('Please select a target month.');
            $('#modal-targetmonth-error').css('display', 'block');
            $('#fg-targetmonth').addClass('govuk-form-group--error');
            $('#targetMonthSelect').addClass('govuk-select--error');
            return;
        }

        // Validation: Check if source month is selected
        if (!currentMonth) {
            $('#formCopyInvoice-db-error-msg').text('Please select a source month first by using the "Select Month" dropdown on the main page.');
            $('#formCopyInvoice-db-error').css('display', 'block');
            $('.modal-body').scrollTop(0);
            return;
        }

        // Validation: Check if source and target are different
        if (currentMonth === targetMonth) {
            $('#formCopyInvoice-db-error-msg').text('Source and target months must be different. Please select a different target month.');
            $('#formCopyInvoice-db-error').css('display', 'block');
            $('.modal-body').scrollTop(0);
            return;
        }

        // Check if "Select All" checkbox is checked
        var isSelectAllChecked = $('#automaticInvoiceGrid_selectAll').is(':checked');

        var selectedRecords = [];

        if (!isSelectAllChecked) {
            // Get selected invoice records only if Select All is not checked
            selectedRecords = getSelectedInvoiceRecords();
        }

        // Call the copy API
        $.ajax({
            url: '/PACT/AutomaticMonthlyInvoice/CopyInvoices',
            type: 'POST',
            contentType: 'application/json',
            dataType: 'json',
            data: JSON.stringify({
                sourceMonth: parseInt(currentMonth, 10),
                targetMonth: parseInt(targetMonth, 10),
                invoiceRecords: selectedRecords.length > 0 ? selectedRecords : null
            }),
        success: function (response) {
            if (response.success) {
                closeCopyInvoiceModal();
                showGovukAlert(response.message);

                // Switch to the target month to show the copied invoices
                currentMonth = targetMonth;
                $('#monthPick').val(targetMonth);
                reloadAutomaticInvoicesGrid();
            } else {
                // Display error message in the copy invoice modal
                $('#formCopyInvoice-db-error-msg').text(response.message || 'Failed to copy invoices.');
                $('#formCopyInvoice-db-error').css('display', 'block');

                // Scroll to the top of the modal to show the error
                $('.modal-body').scrollTop(0);
            }
        },
        error: function (xhr, status, error) {
            var errorMessage = 'An error occurred while copying invoices.';

            if (xhr.status === 0) {
                errorMessage = 'Network error: Unable to connect to server. Please check your connection and try again.';
            } else if (xhr.status === 400) {
                errorMessage = 'Bad request: ' + (xhr.responseText || 'Invalid data sent to server');
            } else if (xhr.responseText) {
                try {
                    var errorResponse = JSON.parse(xhr.responseText);
                    if (errorResponse.message) {
                        errorMessage = errorResponse.message;
                    } else if (errorResponse.errors && errorResponse.errors.length > 0) {
                        errorMessage = errorResponse.errors.join(', ');
                    } else if (errorResponse.title) {
                        errorMessage = errorResponse.title;
                    }
                } catch (e) {
                    // If parsing fails, show the raw text (truncated if too long)
                    if (xhr.responseText.length > 200) {
                        errorMessage = 'Error ' + xhr.status + ': ' + xhr.statusText;
                    } else {
                        errorMessage += ' ' + xhr.responseText;
                    }
                }
            } else {
                errorMessage = 'Error ' + xhr.status + ': ' + (xhr.statusText || error);
            }

            $('#formCopyInvoice-db-error-msg').text(errorMessage);
            $('#formCopyInvoice-db-error').css('display', 'block');

            // Scroll to the top of the modal to show the error
            $('.modal-body').scrollTop(0);
        }
    });
    } catch (ex) {
        alert('An error occurred: ' + ex.message);
    }
}

function copyToPasteBuffer() {
    if (!currentMonth) {
        showGovukAlert('Please select a month first.');
        return;
    }
    previousMonthForCopy = currentMonth;
    showGovukAlert('Invoices from month ' + currentMonth + ' copied to buffer. Now select the target month and click "Paste from Previous Month".');
}

function pasteFromPreviousMonth() {
    if (!previousMonthForCopy) {
        showGovukAlert('Please copy invoices from a source month first using "Copy to Clipboard" button.');
        return;
    }

    if (!currentMonth) {
        showGovukAlert('Please select a target month.');
        return;
    }

    if (previousMonthForCopy === currentMonth) {
        showGovukAlert('Source and target months must be different.');
        return;
    }

    showGovukConfirm('Copy all invoices from month ' + previousMonthForCopy + ' to month ' + currentMonth + '?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/AutomaticMonthlyInvoice/CopyInvoices',
            type: 'POST',
            data: {
                sourceMonth: previousMonthForCopy,
                targetMonth: currentMonth
            },
            success: function (response) {
                if (response.success) {
                    reloadAutomaticInvoicesGrid();
                    showGovukAlert(response.message);
                    previousMonthForCopy = null; // Clear buffer after successful paste
                } else {
                    showGovukAlert('Error: ' + response.message);
                }
            },
            error: function (xhr, status, error) {
                showGovukAlert('An error occurred while copying invoices: ' + error);
            }
        });
    });
}

// ── Helper Functions ───────────────────────────────────────────────

// Override grid action functions for this page
window.addGridItem = function() {
    addAutomaticInvoice();
};

window.editGridItem = function(btn) {
    editAutomaticInvoice(btn);
};

window.deleteGridItem = function(btn) {
    deleteAutomaticInvoice(btn);
};

window.saveModalForm = function() {
    saveAutomaticInvoice();
};

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
    $.get('/PACT/AutomaticMonthlyInvoice/GetInvoice',
        { id: 0, selectedMonth: currentMonth || '' },
        function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        })
        .fail(function(xhr, status, error) {
            alert('Error loading form: ' + error);
        });
}

function editAutomaticInvoice(btn) {
    var id = $(btn).data('id');
    $.get('/PACT/AutomaticMonthlyInvoice/GetInvoice', { id: id }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    })
    .fail(function(xhr, status, error) {
        alert('Error loading form: ' + error);
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

    // Set the source month in the modal
    var selectedMonthText = $('#monthPick option:selected').text();
    $('#txtSelectedMonth').val(selectedMonthText);

    // Clear target month selection
    $('#targetMonthSelect').val('');

    // Clear all error messages and styles
    $('#formCopyInvoice-db-error').attr('hidden', true);
    $('#modal-targetmonth-error').attr('hidden', true);
    $('#fg-targetmonth').removeClass('govuk-form-group--error');
    $('#targetMonthSelect').removeClass('govuk-select--error');

    // Show the modal using the standard .show class
    $('#copyInvoiceModal').addClass('show');
}

function closeCopyInvoiceModal() {
    // Clear all error messages and styles when closing
    $('#formCopyInvoice-db-error').attr('hidden', true);
    $('#modal-targetmonth-error').attr('hidden', true);
    $('#fg-targetmonth').removeClass('govuk-form-group--error');
    $('#targetMonthSelect').removeClass('govuk-select--error');
    $('#targetMonthSelect').val('');

    // Close the modal
    $('#copyInvoiceModal').removeClass('show');
}

function saveCopiedInvoice() {
    console.log('saveCopiedInvoice called');

    // Clear all previous errors first
    $('#formCopyInvoice-db-error').attr('hidden', true).hide();
    $('#modal-targetmonth-error').attr('hidden', true).hide();
    $('#fg-targetmonth').removeClass('govuk-form-group--error');
    $('#targetMonthSelect').removeClass('govuk-select--error');

    var targetMonth = $('#targetMonthSelect').val();

    // Validation: Check if target month is selected
    if (!targetMonth) {
        $('#modal-targetmonth-error-msg').text('Please select a target month.');
        $('#modal-targetmonth-error').removeAttr('hidden').show().css('display', 'block');
        $('#fg-targetmonth').addClass('govuk-form-group--error');
        $('#targetMonthSelect').addClass('govuk-select--error');
        return;
    }

    // Validation: Check if source month is selected
    if (!currentMonth) {
        $('#formCopyInvoice-db-error-msg').text('Please select a source month first by using the "Select Month" dropdown on the main page.');
        $('#formCopyInvoice-db-error').removeAttr('hidden').show();
        return;
    }

    // Validation: Check if source and target are different
    if (currentMonth === targetMonth) {
        $('#formCopyInvoice-db-error-msg').text('Source and target months must be different. Please select a different target month.');
        $('#formCopyInvoice-db-error').removeAttr('hidden').show();
        return;
    }

    // Call the copy API
    $.ajax({
        url: '/PACT/AutomaticMonthlyInvoice/CopyInvoices',
        type: 'POST',
        data: {
            sourceMonth: currentMonth,
            targetMonth: targetMonth
        },
        success: function (response) {
            if (response.success) {
                closeCopyInvoiceModal();
                showGovukAlert(response.message);

                // Switch to the target month to show the copied invoices
                currentMonth = targetMonth;
                $('#monthPick').val(targetMonth);
                reloadAutomaticInvoicesGrid();
            } else {
                displayServerValidationErrors(response.errors, response.message, '#modaPopupBody');
            }
        },
        error: function () {
            showGovukAlert('An error occurred while saving.');
        }
    });
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

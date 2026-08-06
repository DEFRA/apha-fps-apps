// Number Validation - Shared JavaScript Module
// Handles numeric input validation for decimal/currency fields across PACT module
// Usage: Add 'decfmt-input' class to any input field that needs numeric validation

// Helper function to get clean label text from an input field
// Uses the same approach as ajax-form-validation.js
function getFieldLabel($input) {
    var labelText = '';
    var inputName = $input.attr('name') || '';
    var inputId = $input.attr('id') || '';

    // Try to find label by 'for' attribute matching the input's name
    if (inputName) {
        var $label = $('label[for="' + inputName + '"]');
        if ($label.length > 0) {
            // Clone label, remove child elements (like asterisk spans), get text, remove trailing colons
            labelText = $label.clone().children().remove().end().text().trim().replace(/:\s*$/, '');
        }
    }

    // If not found by name, try by id
    if (!labelText && inputId) {
        var $label = $('label[for="' + inputId + '"]');
        if ($label.length > 0) {
            labelText = $label.clone().children().remove().end().text().trim().replace(/:\s*$/, '');
        }
    }

    // If no label found, try to get label from parent form-group
    if (!labelText) {
        var $formGroup = $input.closest('.govuk-form-group');
        var $label = $formGroup.find('label').first();
        if ($label.length > 0) {
            labelText = $label.clone().children().remove().end().text().trim().replace(/:\s*$/, '');
        }
    }

    // If still no label, use field name or id or a generic term
    if (!labelText) {
        labelText = inputName || inputId || 'This field';
    }

    return labelText;
}

// Numeric input validation - allows positive/negative numbers with decimal point
function validateNumericInput(event) {
    var input = event.target;
    var value = input.value;
    var key = event.key;
    var cursorPosition = input.selectionStart;

    // Allow control keys
    if (['Backspace', 'Delete', 'Tab', 'Escape', 'Enter', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'].includes(key)) {
        return true;
    }

    // Allow Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X
    if (event.ctrlKey || event.metaKey) {
        return true;
    }

    // Allow digits
    if (/^\d$/.test(key)) {
        return true;
    }

    // Allow minus sign only at the beginning and only if there isn't one already
    if (key === '-') {
        if (cursorPosition === 0 && !value.includes('-')) {
            return true;
        }
        event.preventDefault();
        return false;
    }

    // Allow decimal point only if there isn't one already
    if (key === '.' || key === ',') {
        if (!value.includes('.') && !value.includes(',')) {
            return true;
        }
        event.preventDefault();
        return false;
    }

    // Block all other keys
    event.preventDefault();
    return false;
}

// Format and validate numeric input on paste
function handleNumericPaste(event) {
    // Get the original event if this is a jQuery event
    var originalEvent = event.originalEvent || event;
    originalEvent.preventDefault();

    var pastedData = (originalEvent.clipboardData || window.clipboardData).getData('text');

    // Check if pasted data contains any alphabetic characters
    if (/[a-zA-Z]/.test(pastedData)) {
        showAlertMessage('You may have enter text in a numeric field or a number that is larger than the FieldSize Permits.', AlertType.ERROR);
        return;
    }

    // Remove any non-numeric characters except minus and decimal point
    var cleaned = pastedData.replace(/[^\d.-]/g, '');

    // If after cleaning, nothing remains, show error
    if (!cleaned) {
        showAlertMessage('You may have enter text in a numeric field or a number that is larger than the FieldSize Permits.', AlertType.ERROR);
        return;
    }

    // Ensure only one minus sign at the beginning
    if (cleaned.indexOf('-') > 0) {
        cleaned = cleaned.replace(/-/g, '');
    } else if ((cleaned.match(/-/g) || []).length > 1) {
        cleaned = '-' + cleaned.replace(/-/g, '');
    }

    // Ensure only one decimal point
    var parts = cleaned.split('.');
    if (parts.length > 2) {
        cleaned = parts[0] + '.' + parts.slice(1).join('');
    }

    // Validate range: -999999999999999.9999 to 999999999999999.9999
    var parsedValue = parseFloat(cleaned);
    var min = -999999999999999.9999;
    var max = 999999999999999.9999;

    if (!isNaN(parsedValue) && (parsedValue < min || parsedValue > max)) {
        // Get the input element to find its label
        var input = event.target || event.currentTarget;
        var $input = $(input);
        var fieldName = getFieldLabel($input);

        showAlertMessage(fieldName + ' must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999', AlertType.ERROR);
        return;
    }

    // Get the input element (handle both jQuery events and native events)
    var input = event.target || event.currentTarget;
    var start = input.selectionStart;
    var end = input.selectionEnd;
    var currentValue = input.value;

    // Replace the selected portion (or insert at cursor if nothing selected)
    input.value = currentValue.substring(0, start) + cleaned + currentValue.substring(end);

    // Set cursor position after the pasted content
    var newCursorPos = start + cleaned.length;
    input.selectionStart = input.selectionEnd = newCursorPos;

    // Trigger input event for any validation listeners
    $(input).trigger('input');
}

// Validate numeric input range and provide visual feedback
function validateRangeOnInput(input) {
    var $input = $(input); // Use jQuery for consistency
    var value = $input.val().trim();
    var fieldName = $input.attr('name') || $input.attr('id');

    // Get the label text for better error messages using the helper function
    var labelText = getFieldLabel($input);

    // Sanitize the input value to fix invalid formats like "999-87.0000"
    // Only allow minus at the beginning, remove any other minus signs
    if (value.length > 0) {
        var sanitized = value;
        var firstChar = value.charAt(0);
        var isNegative = firstChar === '-';

        if (isNegative) {
            // Keep first minus, remove all others
            sanitized = '-' + value.substring(1).replace(/-/g, '');
        } else {
            // Remove all minus signs if not at the beginning
            sanitized = value.replace(/-/g, '');
        }

        // Ensure only one decimal point
        var parts = sanitized.split('.');
        if (parts.length > 2) {
            sanitized = parts[0] + '.' + parts.slice(1).join('');
        }

        // Update the field value if it was sanitized
        if (sanitized !== value) {
            var cursorPos = input.selectionStart;
            $input.val(sanitized);
            // Restore cursor position
            input.selectionStart = input.selectionEnd = Math.min(cursorPos, sanitized.length);
            value = sanitized;
        }
    }

    // Find the parent form-group and validation message span
    var $formGroup = $input.closest('.govuk-form-group');
    var $validationSpan = $formGroup.find('span[data-valmsg-for="' + fieldName + '"], span[asp-validation-for="' + fieldName + '"]');

    // If validation span not found by name, try finding by class
    if ($validationSpan.length === 0) {
        $validationSpan = $formGroup.find('.govuk-error-message, .field-validation-error');
    }

    // Skip validation if field is empty
    if (value === '' || value === '-') {
        $input.removeClass('govuk-input--error');
        $formGroup.removeClass('govuk-form-group--error');
        $input.removeAttr('title');
        if ($validationSpan.length > 0) {
            $validationSpan.text('').hide();
        }
        return;
    }

    var parsedValue = parseFloat(value);
    var min = -999999999999999.9999;
    var max = 999999999999999.9999;

    // Check if value is a valid number
    if (isNaN(parsedValue)) {
        $input.addClass('govuk-input--error');
        $formGroup.addClass('govuk-form-group--error');
        $input.attr('title', 'Please enter a valid number');
        if ($validationSpan.length > 0) {
            $validationSpan.text('Please enter a valid number').show();
        }
        return;
    }

    // Check if value is within range
    if (parsedValue < min || parsedValue > max) {
        var errorMessage = labelText + ' must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999';
        $input.addClass('govuk-input--error');
        $formGroup.addClass('govuk-form-group--error');
        $input.attr('title', errorMessage);
        if ($validationSpan.length > 0) {
            $validationSpan.text(errorMessage).show();
        }
    } else {
        $input.removeClass('govuk-input--error');
        $formGroup.removeClass('govuk-form-group--error');
        $input.removeAttr('title');
        if ($validationSpan.length > 0) {
            $validationSpan.text('').hide();
        }
    }
}

// Initialize numeric input validation for all fields with 'decfmt-input' class
function initializeNumericInputValidation() {
    var $decimalFields = $('.decfmt-input'); // Use jQuery selector

    $decimalFields.each(function() {
        var $field = $(this);
        var fieldId = $field.attr('id') || $field.attr('name') || 'unknown';


        // Set maxlength="20" for all decfmt-input fields if not already set
        if (!$field.attr('maxlength')) {
            $field.attr('maxlength', '20');
        }

        // Remove existing handlers first to prevent duplicates
        $field.off('keydown.numericValidation');
        $field.off('paste.numericValidation');
        $field.off('input.numericValidation');
        $field.off('blur.numericValidation');

        // Add event listeners using jQuery with namespaced events
        $field.on('keydown.numericValidation', function(e) {
            return validateNumericInput(e);
        });

        $field.on('paste.numericValidation', function(e) {
            handleNumericPaste(e);
        });

        $field.on('input.numericValidation', function() {
            validateRangeOnInput(this);
        });

        $field.on('blur.numericValidation', function() {
            validateRangeOnInput(this);
        });
    });
}

// Attach numeric validation to dynamically loaded elements
function attachNumericValidation() {
    initializeNumericInputValidation();
}

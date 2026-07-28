// Number Validation - Shared JavaScript Module
// Handles numeric input validation for decimal/currency fields across PACT module
// Usage: Add 'decfmt-input' class to any input field that needs numeric validation

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
    event.preventDefault();
    var pastedData = (event.clipboardData || window.clipboardData).getData('text');

    // Check if pasted data contains any alphabetic characters
    if (/[a-zA-Z]/.test(pastedData)) {
        showAlertMessage('Alphanumeric paste not allowed in number fields', AlertType.ERROR);
        return;
    }

    // Remove any non-numeric characters except minus and decimal point
    var cleaned = pastedData.replace(/[^\d.-]/g, '');

    // If after cleaning, nothing remains, show error
    if (!cleaned) {
        showAlertMessage('Alphanumeric paste not allowed in number fields', AlertType.ERROR);
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
        showAlertMessage('Value must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999', AlertType.ERROR);
        return;
    }

    // Insert cleaned text at cursor position
    var input = event.target;
    var start = input.selectionStart;
    var end = input.selectionEnd;
    var currentValue = input.value;

    input.value = currentValue.substring(0, start) + cleaned + currentValue.substring(end);
    input.selectionStart = input.selectionEnd = start + cleaned.length;

    // Trigger input event for any validation listeners
    input.dispatchEvent(new Event('input', { bubbles: true }));

    // Validate range after paste for visual feedback
    validateRangeOnInput(input);
}

// Initialize numeric input validation for all fields with 'decfmt-input' class
function initializeNumericInputValidation() {
    var decimalFields = document.querySelectorAll('.decfmt-input');

    decimalFields.forEach(function(element) {
        if (element) {
            element.addEventListener('keydown', validateNumericInput);
            element.addEventListener('paste', handleNumericPaste);
        }
    });
}

// Attach numeric validation to dynamically loaded elements
function attachNumericValidation() {
    initializeNumericInputValidation();
}

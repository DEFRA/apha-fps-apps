// ── Numeric / decimal input constraint ────────────────────────────────
// Apply the CSS class "js-numeric-decimal" to any <input> to restrict its
// value to numbers only. A single decimal point is allowed, and a maximum
// of 4 digits are permitted after the decimal point.
//
// Usage:
//   <input type="text" class="js-numeric-decimal" />
//
// The behaviour is wired automatically on document ready and also works for
// inputs added to the DOM later (via delegated events).

(function () {
    'use strict';

    var NUMERIC_DECIMAL_SELECTOR = '.js-numeric-decimal';
    var MAX_DECIMAL_PLACES = 4;

    // Matches an optional integer part, an optional single decimal point and
    // up to MAX_DECIMAL_PLACES fractional digits. Empty string is allowed so
    // the user can clear the field.
    var VALID_PATTERN = new RegExp('^\\d*(?:\\.\\d{0,' + MAX_DECIMAL_PLACES + '})?$');

    function isValidNumericValue(value) {
        return VALID_PATTERN.test(value);
    }

    // Blocks characters that could never form a valid value.
    function handleKeyPress(e) {
        // Allow control keys (backspace, tab, arrows, etc.).
        if (e.ctrlKey || e.metaKey || e.key === undefined || e.key.length > 1) {
            return;
        }

        var input = e.target;
        var char = e.key;

        // Only digits and a single decimal point are permitted.
        if (!/[0-9.]/.test(char)) {
            e.preventDefault();
            return;
        }

        // Build the value that would result if the key were accepted.
        var start = input.selectionStart;
        var end = input.selectionEnd;
        var proposed = input.value.slice(0, start) + char + input.value.slice(end);

        if (!isValidNumericValue(proposed)) {
            e.preventDefault();
        }
    }

    // Sanitises pasted / auto-filled / IME content that bypasses keypress.
    function handleInput(e) {
        var input = e.target;
        if (isValidNumericValue(input.value)) {
            input.dataset.lastValidValue = input.value;
            return;
        }

        // Revert to the last known valid value.
        input.value = input.dataset.lastValidValue || '';
    }

    function bindNumericDecimalInputs() {
        // Delegated listeners handle both existing and dynamically added inputs.
        document.addEventListener('keypress', function (e) {
            if (e.target && e.target.matches && e.target.matches(NUMERIC_DECIMAL_SELECTOR)) {
                handleKeyPress(e);
            }
        });

        document.addEventListener('input', function (e) {
            if (e.target && e.target.matches && e.target.matches(NUMERIC_DECIMAL_SELECTOR)) {
                handleInput(e);
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', bindNumericDecimalInputs);
    } else {
        bindNumericDecimalInputs();
    }

    // Expose the validator for optional reuse (e.g. form submit validation).
    window.isValidNumericDecimal = isValidNumericValue;
})();

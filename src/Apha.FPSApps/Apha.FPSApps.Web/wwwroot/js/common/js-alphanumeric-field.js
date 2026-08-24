(function () {
    'use strict';

    var ALPHANUMERIC_SELECTOR = '.js-alphanumeric';

    // Matches a string consisting solely of letters (A-Z, a-z) and digits (0-9).
    // Empty string is allowed so the user can type or clear the field.
    var VALID_PATTERN = /^[A-Za-z0-9]*$/;

    function isValidAlphanumericValue(value) {
        return VALID_PATTERN.test(value);
    }

    // Blocks characters that could never form a valid value.
    function handleKeyPress(e) {
        // Allow control keys (backspace, tab, arrows, etc.).
        if (e.ctrlKey || e.metaKey || e.key === undefined || e.key.length > 1) {
            return;
        }

        var char = e.key;

        // Only letters and digits are permitted.
        if (!/^[A-Za-z0-9]$/.test(char)) {
            e.preventDefault();
        }
    }

    // Sanitises pasted / auto-filled / IME content that bypasses keypress.
    function handleInput(e) {
        var input = e.target;
        if (isValidAlphanumericValue(input.value)) {
            input.dataset.lastValidValue = input.value;
            return;
        }

        // Strip any invalid characters and restore caret position.
        var start = input.selectionStart;
        var cleaned = input.value.replace(/[^A-Za-z0-9]/g, '');
        var removed = input.value.length - cleaned.length;
        input.value = cleaned;
        input.dataset.lastValidValue = cleaned;
        if (typeof start === 'number') {
            var caret = Math.max(0, start - removed);
            input.setSelectionRange(caret, caret);
        }
    }

    function bindAlphanumericInputs() {
        // Delegated listeners handle both existing and dynamically added inputs.
        document.addEventListener('keypress', function (e) {
            if (e.target && e.target.matches && e.target.matches(ALPHANUMERIC_SELECTOR)) {
                handleKeyPress(e);
            }
        });

        document.addEventListener('input', function (e) {
            if (e.target && e.target.matches && e.target.matches(ALPHANUMERIC_SELECTOR)) {
                handleInput(e);
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', bindAlphanumericInputs);
    } else {
        bindAlphanumericInputs();
    }

    // Expose the validator for optional reuse (e.g. form submit validation).
    window.isValidAlphanumeric = isValidAlphanumericValue;
})();

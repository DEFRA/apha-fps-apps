// Release Summary Page JavaScript

// ── Selectors ─────────────────────────────────────────────────────────────────
// Note: setFinalSummaryRunUrl is initialised in the Razor view.
// DO NOT redeclare it here - it is set via inline script in Index.cshtml

var GRID_SELECTOR = '#gridContainer_releaseSummariesGrid';
var ROW_SELECTOR = GRID_SELECTOR + ' tbody tr';
var CHECKBOX_CLASS = '.grid-checkbox';

// ── Helpers ───────────────────────────────────────────────────────────────────

function getGridRows() {
    return $(ROW_SELECTOR);
}

function getRowCheckbox($row) {
    return $row.find(CHECKBOX_CLASS);
}

/**
 * Returns the index of the last checked row, or -1 if none are checked.
 */
function getLastCheckedIndex() {
    var lastCheckedIndex = -1;
    getGridRows().each(function (index) {
        if (getRowCheckbox($(this)).is(':checked')) {
            lastCheckedIndex = index;
        }
    });
    return lastCheckedIndex;
}

/**
 * Returns the index of the last checked row, excluding the given row index.
 */
function getLastCheckedIndexExcluding(excludeIndex) {
    var lastCheckedIndex = -1;
    getGridRows().each(function (index) {
        if (index !== excludeIndex && getRowCheckbox($(this)).is(':checked')) {
            lastCheckedIndex = index;
        }
    });
    return lastCheckedIndex;
}

/**
 * Disables checkboxes that appear before the last checked row.
 * The last checked row stays enabled so it can be unchecked.
 */
function updateCheckboxStates() {
    var lastCheckedIndex = getLastCheckedIndex();
    getGridRows().each(function (index) {
        var $checkbox = getRowCheckbox($(this));
        $checkbox.prop('disabled', lastCheckedIndex !== -1 && index < lastCheckedIndex);
    });
}

/**
 * Validates that the row being checked is the next sequential row.
 * Returns true if the check is allowed, false otherwise.
 */
function isSequentialCheckAllowed(currentIndex) {
    var lastCheckedIndex = getLastCheckedIndexExcluding(currentIndex);
    var expectedNextIndex = lastCheckedIndex + 1;
    return currentIndex === expectedNextIndex;
}

/**
 * Sends the updated finalSummariesRun value to the server.
 */
function saveFinalSummaryRun(periodName, finalSummariesRun, $row, $checkbox, isChecked) {
    $.ajax({
        url: setFinalSummaryRunUrl,
        type: 'POST',
        data: { periodName: periodName, finalSummariesRun: finalSummariesRun, sendEmail: '' },
        success: function () {
            $row.css('background-color', '#ffffcc');
            updateCheckboxStates();
        },
        error: function () {
            showGovukAlert('Failed to update Final Summaries Run for period: ' + periodName);
            $checkbox.prop('checked', !isChecked);
            updateCheckboxStates();
        }
    });
}

/**
 * Sends the updated sendEmail setting to the server when cbSendEmail is toggled.
 */
function onSendEmailChange() {
    var sendEmail = $(this).is(':checked') ? '1' : '0';
    $.ajax({
        url: setFinalSummaryRunUrl,
        type: 'POST',
        data: { periodName: '', finalSummariesRun: 0, sendEmail: sendEmail },
        error: function () {
            showGovukAlert('Failed to update Send Report Emails setting.');
            $('#cbSendEmail').prop('checked', !$('#cbSendEmail').is(':checked'));
        }
    });
}

// ── Event handler ─────────────────────────────────────────────────────────────

function onCheckboxChange() {
    var $checkbox = $(this);
    var $row = $checkbox.closest('tr');
    var periodName = $row.data('id');
    var isChecked = $checkbox.is(':checked');
    var currentIndex = getGridRows().index($row);

    if (isChecked && !isSequentialCheckAllowed(currentIndex)) {
        showGovukAlert('Please check checkboxes sequentially. You cannot skip rows.');
        $checkbox.prop('checked', false);
        updateCheckboxStates();
        return;
    }

    saveFinalSummaryRun(periodName, isChecked ? 1 : 0, $row, $checkbox, isChecked);
}

// ── Initialise ────────────────────────────────────────────────────────────────

$(function () {
    updateCheckboxStates();
    $(GRID_SELECTOR).on('change', CHECKBOX_CLASS, onCheckboxChange);
    $('#cbSendEmail').on('change', onSendEmailChange);
});

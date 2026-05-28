function getSummarisedWgTimeExtraFilters() {
    return {
        yrPlanAmount: parseFloat($('#hdnYrPlanAmount').val()) || 0
    };
}

function selectGridRow($row) {
    if (!$row || $row.length === 0) return;

    const project = $row.find('td[data-property="ParentProject"] span').text().trim();

    $row.addClass('selected-row').siblings().removeClass('selected-row');
    $('#txtSelectedProject').val(project);
    $('#txtProjectDescription').val((project && projectTitleLookup && projectTitleLookup[project]) ? projectTitleLookup[project] : '');
}

$(document).ready(function () {
    const $firstRow = $('table[id^="tbl_summarised"]:not(.totals-table) tbody tr:first');
    selectGridRow($firstRow);
});

document.addEventListener('gridReloaded', function (e) {
    if (e.detail && e.detail.gridId === 'summarisedWorkgroupTimeGrid') {
        const $firstRow = $('table[id^="tbl_summarised"]:not(.totals-table) tbody tr:first');
        selectGridRow($firstRow);
    }
});

$(document).on('click', 'table[id^="tbl_summarised"]:not(.totals-table) tbody tr', function () {
    selectGridRow($(this));
});

function resetCalculationGrid() {
    $('#hdnYrPlanAmount').val('0');
    window['gridManager_summarisedWorkgroupTimeGrid'].reloadGrid({ page: 1 });
}

function openTimeRecordModal() {
    $('#timeRecordModal').addClass('open');
    $('#modal-amount').val('');
    $('#formTimeRecord-db-error').attr('hidden', true);
    $('#modal-amount-error').attr('hidden', true);
}

function closeTimeRecordModal() {
    $('#timeRecordModal').removeClass('open');
    $('#formTimeRecord')[0].reset();
}

function calculateSpent() {
    const amount = $('#modal-amount').val();

    if (!amount || parseFloat(amount) <= 0) {
        $('#modal-amount-error-msg').text('Please enter a valid amount');
        $('#modal-amount-error').attr('hidden', false);
        return;
    }

    $('#modal-amount-error').attr('hidden', true);
    $('#formTimeRecord-db-error').attr('hidden', true);
    $('#hdnYrPlanAmount').val(parseFloat(amount));

    closeTimeRecordModal();
    window['gridManager_summarisedWorkgroupTimeGrid'].reloadGrid({ page: 1 });
}

$(window).on('click', function(event) {
    if ($(event.target).is('#timeRecordModal')) {
        closeTimeRecordModal();
    }
});

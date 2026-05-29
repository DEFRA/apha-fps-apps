function getSummarisedWgTimeExtraFilters() {
    return {
        yrPlanAmount: parseFloat($('#hdnYrPlanAmount').val()) || 0
    };
}

// summary start
function syncTotalsWidths() {
    const $table = $('#gridContainer_summarisedWorkgroupTimeGrid').find('table.editable-grid-table').first();
    if (!$table.length) return;

    // Read every visible <th> in the header row (skip filter row)
    const $headers = $table.find('thead tr.govuk-table__row th');
    if (!$headers.length) return;

    const $container = $('#columnTotalsContainer');
    const $cells = $container.children('[data-col-index]');
    if (!$cells.length) return;

    // Use getBoundingClientRect for sub-pixel accurate width.
    // Cells use border-box, so the assigned width equals the th BCR exactly –
    // no padding/border offset accumulates across columns.
    $headers.each(function (i) {
        const w = this.getBoundingClientRect().width;
        $cells.filter('[data-col-index="' + i + '"]').css({
            width:    w + 'px',
            minWidth: w + 'px',
            maxWidth: w + 'px'
        });
    });

    // Sync cell height from the first tbody row so the totals row is the
    // same height as the data rows regardless of font/padding changes.
    const $firstDataRow = $table.find('tbody tr:first');
    if ($firstDataRow.length) {
        const rowH = Math.round($firstDataRow[0].getBoundingClientRect().height);
        $cells.css('height', rowH + 'px');
    }

    // Match container width to table exactly
    const tableWidth = $table[0].getBoundingClientRect().width;
    $container.css('width', tableWidth + 'px');
}

function moveTotalsIntoScrollContainer() {
    const $gridScroll = $('#gridContainer_summarisedWorkgroupTimeGrid').find('.grid-scroll-container').first();
    const $totals     = $('#columnTotalsContainer');

    if (!$gridScroll.length || !$totals.length) return;

    // Already inside – nothing to do
    if ($.contains($gridScroll[0], $totals[0])) return;

    // Move totals div inside the scroll container, after the table
    $gridScroll.css('overflow-x', 'auto');
    $gridScroll.append($totals);
}

// summary End
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
    syncTotalsWidths();

    // Re-sync while a column is being dragged (live update)
    document.addEventListener('mousemove', function () {
        if (document.body.style.cursor === 'col-resize') {
            syncTotalsWidths();
        }
    });

    // Re-sync once drag is released
    window.addEventListener('mouseup', function () {
        setTimeout(syncTotalsWidths, 50);
    });

    // Re-sync on window resize
    $(window).on('resize.totalsSync', function () { syncTotalsWidths(); });
});

document.addEventListener('gridReloaded', function (e) {
    if (e.detail && e.detail.gridId === 'summarisedWorkgroupTimeGrid') {
        const $firstRow = $('table[id^="tbl_summarised"]:not(.totals-table) tbody tr:first');
        selectGridRow($firstRow);
        syncTotalsWidths();
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

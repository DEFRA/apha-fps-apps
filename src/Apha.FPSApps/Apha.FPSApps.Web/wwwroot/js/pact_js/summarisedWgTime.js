function getSummarisedWgTimeExtraFilters() {
    return {
        yrPlanAmount: parseFloat($('#hdnYrPlanAmount').val()) || 0
    };
}

/**
 * Aligns the footer totals row with their corresponding table column headers
 * using getBoundingClientRect for sub-pixel accuracy.
 * Mirrors the approach used by work-group-summarised-staffTimeUsage.js.
 */
function alignColumnTotals() {
    const table = document.querySelector('#gridContainer_summarisedWorkgroupTimeGrid table');
    if (!table) return;

    const tableRect = table.getBoundingClientRect();
    const headers   = table.querySelectorAll('thead tr.govuk-table__row th');
    if (headers.length === 0) return;

    const container = document.getElementById('columnTotalsContainer');
    if (!container) return;

    const label   = container.querySelector('.column-total-label');
    const columns = container.querySelectorAll('.column-total');
    if (!label || columns.length === 0) return;

    // Column order matches SummarisedWgTimePivotRow GridColumn Order:
    // 0=ParentProject, 1=April … 12=March, 13=SumOfTime, 14=CostDisplay,
    // 15=Budget(YrPlan), 16=SpentDisplay
    const columnMap = {
        'ParentProject': 0,
        'April':         1,
        'May':           2,
        'June':          3,
        'July':          4,
        'August':        5,
        'September':     6,
        'October':       7,
        'November':      8,
        'December':      9,
        'January':       10,
        'February':      11,
        'March':         12,
        'SumOfTime':     13,
        'CostDisplay':   14,
        'Budget':        15,
        'SpentDisplay':  16
    };

    const dataColumns = [
        'April', 'May', 'June', 'July', 'August', 'September',
        'October', 'November', 'December', 'January', 'February', 'March',
        'SumOfTime', 'CostDisplay', 'Budget', 'SpentDisplay'
    ];

    // Position label under the Project header
    const projectHeader = headers[columnMap['ParentProject']];
    if (projectHeader) {
        const rect = projectHeader.getBoundingClientRect();
        label.style.position = 'absolute';
        label.style.left     = (rect.left - tableRect.left) + 'px';
        label.style.width    = rect.width + 'px';
    }

    // Position each numeric cell under its matching column header
    columns.forEach(function (col, index) {
        if (index >= dataColumns.length) return;
        const header = headers[columnMap[dataColumns[index]]];
        if (!header) return;
        const headerRect = header.getBoundingClientRect();
        col.style.position = 'absolute';
        col.style.left     = (headerRect.left - tableRect.left) + 'px';
        col.style.width    = headerRect.width + 'px';
    });

    // Sync container dimensions to the table
    container.style.width    = table.getBoundingClientRect().width + 'px';
    container.style.position = 'relative';

    const firstDataRow = table.querySelector('tbody tr');
    if (firstDataRow) {
        const rowH = Math.round(firstDataRow.getBoundingClientRect().height);
        container.style.height = rowH + 'px';
    }
}

function selectGridRow($row) {
    if (!$row || $row.length === 0) return;

    const project = $row.find('td[data-property="ParentProject"] span').text().trim();

    $row.addClass('selected-row').siblings().removeClass('selected-row');
    $('#txtSelectedProject').val(project);
    $('#txtProjectDescription').val((project && projectTitleLookup && projectTitleLookup[project]) ? projectTitleLookup[project] : '');
}

// ── Initialisation ────────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', function () {
    const table = document.querySelector('#gridContainer_summarisedWorkgroupTimeGrid table');
    if (!table) return;

    // Align on load and window resize
    window.addEventListener('load',   function () { requestAnimationFrame(alignColumnTotals); });
    window.addEventListener('resize', function () { requestAnimationFrame(alignColumnTotals); });

    // Re-align whenever the table is resized (browser zoom, column drag)
    const resizeObserver = new ResizeObserver(function () { requestAnimationFrame(alignColumnTotals); });
    resizeObserver.observe(table);

    // Re-align after the user releases a column-resizer handle
    let isResizing = false;
    document.addEventListener('mousedown', function (e) {
        if (e.target.classList.contains('column-resizer')) {
            isResizing = true;
        }
    });
    document.addEventListener('mouseup', function () {
        if (isResizing) {
            isResizing = false;
            requestAnimationFrame(alignColumnTotals);
        }
    });

    // Re-align when any <th> style attribute changes (inline column-width updates)
    const mutationObserver = new MutationObserver(function (mutations) {
        const needsRealign = mutations.some(function (m) {
            return m.type === 'attributes' && m.attributeName === 'style';
        });
        if (needsRealign) {
            requestAnimationFrame(alignColumnTotals);
        }
    });
    table.querySelectorAll('th').forEach(function (th) {
        mutationObserver.observe(th, { attributes: true, attributeFilter: ['style'] });
    });

    // Initial alignment – two passes for slow grid initialisation
    setTimeout(function () { alignColumnTotals(); }, 100);
    setTimeout(function () { alignColumnTotals(); }, 500);

    // Auto-select first row
    const $firstRow = $('table[id^="tbl_summarised"]:not(.totals-table) tbody tr:first');
    selectGridRow($firstRow);
});

document.addEventListener('gridReloaded', function (e) {
    if (e.detail && e.detail.gridId === 'summarisedWorkgroupTimeGrid') {
        const $firstRow = $('table[id^="tbl_summarised"]:not(.totals-table) tbody tr:first');
        selectGridRow($firstRow);
        requestAnimationFrame(alignColumnTotals);
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

/**
 * work-group-time-by-job-code.js
 * Client-side logic for the PACT Work Group Time By Job Code page.
 */

var currentWorkGroup  = currentWorkGroup  || null;
var currentPersonName = currentPersonName || null;
var jobTitleLookup    = jobTitleLookup    || {};

/**
 * Retrieves the job title for a given job code from the lookup dictionary.
 * @param {string} jobCode - The job code to look up.
 * @returns {string} The corresponding job title, or an empty string if not found.
 */
function getJobTitleByJobCode(jobCode) {
    if (!jobCode || !jobTitleLookup) {
        return '';
    }
    return jobTitleLookup[jobCode] || '';
}

/**
 * Returns the current work group and person name as extra filter parameters
 * for every grid request (initial load, sort, pagination, and filter events).
 * Called by the _DataGrid partial via the ExtraFilterMethod hook.
 */
function getWorkGroupTimeByJobCodeExtraFilters() {
    return { workGroup: currentWorkGroup, personName: currentPersonName };
}

/**
 * Called by the _DataGrid row-selection handler when a grid row is clicked.
 * Reads the JobCode and JobTitle cell values from the clicked <tr> and writes
 * them into the corresponding display inputs below the grid.
 * @param {HTMLTableRowElement} row - The clicked <tr> element.
 */
function onTimeByJobCodeRowSelected(row) {
    var jobCode  = $(row).find('td[data-property="JobCode"] span').text().trim();
    var jobTitle = getJobTitleByJobCode(jobCode);
    $('#txtJobCode').val(jobCode);
    $('#txtJobTitle').val(jobTitle);
}

/**
 * Aligns the three footer summary rows (totals, standard hours, % allocated)
 * with their corresponding table column headers.
 */
function alignColumnTotals() {
    const table = document.querySelector('#gridContainer_timeUsageGrid table');
    if (!table) return;

    const tableRect = table.getBoundingClientRect();
    const headers   = table.querySelectorAll('thead th');

    if (headers.length === 0) return;

    // Map of column field names to their index in the table
    const columnMap = {
        'ParentProject': 0,
        'JobCode':       1,
        'April':         2,
        'May':           3,
        'June':          4,
        'July':          5,
        'August':        6,
        'September':     7,
        'October':       8,
        'November':      9,
        'December':      10,
        'January':       11,
        'February':      12,
        'March':         13,
        'TotalTime':     14,
        'TotalCost':     15
    };

    // Aligns a single summary-row container by its id
    const alignContainer = (containerId) => {
        const container = document.getElementById(containerId);
        if (!container) return;

        const label   = container.querySelector('.column-total-label');
        const columns = container.querySelectorAll('.column-total');

        if (!label || columns.length === 0) return;

        // Position label to span the Project + JobCode columns
        const projectHeader = headers[columnMap['ParentProject']];
        const jobCodeHeader  = headers[columnMap['JobCode']];

        if (projectHeader && jobCodeHeader) {
            const projectRect = projectHeader.getBoundingClientRect();
            const jobCodeRect = jobCodeHeader.getBoundingClientRect();

            label.style.position = 'absolute';
            label.style.left     = (projectRect.left - tableRect.left) + 'px';
            label.style.width    = (jobCodeRect.right - projectRect.left) + 'px';
        }

        // Position each numeric cell under its matching month / total column
        const monthColumns = [
            'April', 'May', 'June', 'July', 'August', 'September',
            'October', 'November', 'December', 'January', 'February', 'March',
            'TotalTime', 'TotalCost'
        ];

        columns.forEach((col, index) => {
            if (index >= monthColumns.length) return;

            const header = headers[columnMap[monthColumns[index]]];
            if (!header) return;

            const headerRect = header.getBoundingClientRect();

            col.style.position = 'absolute';
            col.style.left     = (headerRect.left - tableRect.left) + 'px';
            col.style.width    = headerRect.width + 'px';
        });
    };

    alignContainer('columnTotalsContainer');
    alignContainer('columnStandardHoursContainer');
    alignContainer('columnStndHrsAllocatedContainer');
}

// ── Initialisation ────────────────────────────────────────────────────────────

/**
 * Selects the first row in the time-usage grid and populates the JobCode /
 * JobTitle display inputs.  Safe to call when the grid is empty.
 */
function selectFirstRow() {
    var $firstRow = $('#tbl_timeUsageGrid tbody tr.selectable-row:first');
    if ($firstRow.length) {
        $('#tbl_timeUsageGrid tbody tr').removeClass('selected-row');
        $firstRow.addClass('selected-row');
        onTimeByJobCodeRowSelected($firstRow[0]);
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const table = document.querySelector('#gridContainer_timeUsageGrid table');
    if (!table) return;

    // Align on load and on window resize
    window.addEventListener('load',   () => requestAnimationFrame(alignColumnTotals));
    window.addEventListener('resize', () => requestAnimationFrame(alignColumnTotals));

    // Re-align whenever the table itself is resized (e.g. browser zoom)
    const resizeObserver = new ResizeObserver(() => requestAnimationFrame(alignColumnTotals));
    resizeObserver.observe(table);

    // Re-align after the user finishes dragging a column resizer handle
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

    // Re-align when any <th> style attribute changes (column resizing updates widths inline)
    const mutationObserver = new MutationObserver(function (mutations) {
        const needsRealign = mutations.some(
            m => m.type === 'attributes' && m.attributeName === 'style'
        );
        if (needsRealign) {
            requestAnimationFrame(alignColumnTotals);
        }
    });

    table.querySelectorAll('th').forEach(function (th) {
        mutationObserver.observe(th, { attributes: true, attributeFilter: ['style'] });
    });

    // Initial alignment — two passes to handle slow grid initialisation
    setTimeout(() => alignColumnTotals(), 100);
    setTimeout(() => alignColumnTotals(), 500);

    // Auto-select the first row on initial page load
    selectFirstRow();

    // Re-select the first row of each new page after pagination / sort reloads.
    // jobTitleLookup is fully populated on page load (PageSize=1000), so no
    // further round-trips are needed to resolve the job title on any page.
    document.addEventListener('gridReloaded', function (e) {
        if (e.detail && e.detail.gridId === 'timeUsageGrid') {
            selectFirstRow();
        }
    });
});

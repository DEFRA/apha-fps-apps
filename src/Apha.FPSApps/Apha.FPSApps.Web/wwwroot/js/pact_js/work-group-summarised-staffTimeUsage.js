/**
 * work-group-time-by-job-code.js
 * Client-side logic for the PACT Work Group Time By Job Code page.
 */

var currentWorkGroup  = currentWorkGroup  || null;
var currentStaffName  = currentStaffName  || null;
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
    return {staffName: currentStaffName };
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

// ── UK decimal formatter ──────────────────────────────────────────────────────

/**
 * Formats a numeric text value using standard half-up rounding:
 *   - decimal fraction >= 0.5 : round to whole number  e.g. 58.93 → 59
 *   - decimal fraction <  0.5 : keep 1 decimal place   e.g. 12.38 → 12.4
 *   - no decimal               : leave unchanged        e.g. 100   → 100
 *  - Values ending with '%' are left untouched.
 *  - Values prefixed with '£' have the symbol preserved.
 * @param {string} text - The raw cell text to inspect and optionally reformat.
 * @returns {string} The original text or a reformatted string.
 */
function formatIfExceedsTwoDecimals(text) {
    if (!text || !text.trim()) return text;

    var trimmed = text.trim();

    // Percentage values are already formatted server-side — leave them alone
    if (trimmed.endsWith('%')) return text;

    var hasPound = trimmed.startsWith('£');
    var rawNum = (hasPound ? trimmed.slice(1) : trimmed).replace(/,/g, '');
    var num = parseFloat(rawNum);

    if (isNaN(num)) return text;

    // No decimal part — leave as-is
    if (rawNum.indexOf('.') === -1) return text;

    // Compare the full decimal fraction to 0.5 (standard half-up rounding)
    var formatted;

    formatted = num.toLocaleString('en-GB', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    // var fracPart = num - Math.floor(num);
    // var formatted;

    // if (fracPart >= 0.5) {
    //     // Fraction >= 0.5 : round up to whole number   e.g. 58.93 → 59
    //     formatted = Math.round(num).toLocaleString('en-GB');
    // } else if (fracPart === 0) {
    //     // No meaningful fraction : show as whole number e.g. 12.00 → 12
    //     formatted = Math.floor(num).toLocaleString('en-GB');
    // } else {
    //     // Fraction < 0.5 : keep 1 decimal place        e.g. 12.38 → 12.4
    //     formatted = num.toLocaleString('en-GB', {
    //         minimumFractionDigits: 2,
    //         maximumFractionDigits: 2
    //     });
    // }

    return hasPound ? '\u00a3' + formatted : formatted;
}

/**
 * Scans all footer summary cells and grid data cells.
 * Any numeric value with more than 2 decimal places is reformatted to
 * 2 decimal places using the en-GB (UK) locale.
 * The original full-precision value is stored in data-tooltip (styled CSS
 * tooltip) and title (native browser fallback) so it appears on mouse-over.
 */
function formatAllNumericValues() {
    // ── Footer summary rows ───────────────────────────────────────────────
    ['columnTotalsContainer', 'columnStandardHoursContainer', 'columnStndHrsAllocatedContainer']
        .forEach(function (id) {
            var container = document.getElementById(id);
            if (!container) return;

            container.querySelectorAll('.column-total').forEach(function (cell) {
                var original = cell.textContent;
                var raw = original.trim();
                if (!raw) return;
                var reformatted = formatIfExceedsTwoDecimals(original);
                // To display the original value as a tooltip on mouse hover, simply uncomment the commented line of code below
                //cell.setAttribute('data-tooltip', raw);
                //cell.title = raw;
                cell.textContent = reformatted;
            });
        });

    // ── Grid data cells (month / numeric columns) ─────────────────────────
    document.querySelectorAll('#gridContainer_timeUsageGrid table tbody td[data-property] span')
        .forEach(function (span) {
            var original = span.textContent;
            var raw = original.trim();
            if (!raw) return;
            var reformatted = formatIfExceedsTwoDecimals(original);
            // To display the original value as a tooltip on mouse hover, simply uncomment the commented line of code below
            //span.setAttribute('data-tooltip', raw);
            //span.title = raw;
            span.textContent = reformatted;
        });
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

    // Format numeric values: round to 1 d.p., then to whole number if decimal digit >= 5
    // e.g. 12.61 → 13  |  12.38 → 12.4
    setTimeout(() => formatAllNumericValues(), 150);

    // Auto-select the first row on initial page load
    selectFirstRow();

    // Re-select the first row of each new page after pagination / sort reloads.
    // jobTitleLookup is fully populated on page load (PageSize=1000), so no
    // further round-trips are needed to resolve the job title on any page.
    document.addEventListener('gridReloaded', function (e) {
        if (e.detail && e.detail.gridId === 'timeUsageGrid') {
            selectFirstRow();
            requestAnimationFrame(formatAllNumericValues);
        }
    });
});

// Calculate column totals after grid is loaded
$(document).ready(function () {
    // Wait for the grid to be rendered in the DOM
    setTimeout(function() {
        syncColumnWidths();
        calculateColumnTotals();
    }, 500);
});

// Recalculate totals when grid is reloaded
$(document).on('gridReloaded', function () {
    syncColumnWidths();
    calculateColumnTotals();
});

// Sync the totals table column widths with the grid table column widths
function syncColumnWidths() {
    const gridTable = $('table[id^="tbl_summarised"]').not('.totals-table');
    const totalsTable = $('#tbl_totals_summarisedWorkgroupTimeGrid');

    if (gridTable.length === 0 || totalsTable.length === 0) {
        console.log('Tables not found for sync');
        return;
    }

    // Get all header cells from the grid
    const headers = gridTable.find('thead th');
    // Get all totals cells
    const totalsCells = totalsTable.find('.totals-cell');

    console.log('Headers found:', headers.length);
    console.log('Totals cells found:', totalsCells.length);

    // Match each total cell to its corresponding header
    headers.each(function(index) {
        if (index < totalsCells.length) {
            const width = $(this).outerWidth();
            $(totalsCells[index]).css('width', width + 'px');
            console.log(`Setting totals cell ${index} width to ${width}px`);
        }
    });

    // Set the totals table width to match the grid table
    const tableWidth = gridTable.outerWidth();
    totalsTable.css('width', tableWidth + 'px');
    console.log('Totals table width set to:', tableWidth);
}

function calculateColumnTotals() {
    console.log('calculateColumnTotals called');

    // Find the grid table (exclude the totals table)
    const gridTable = $('table[id^="tbl_summarised"]').not('.totals-table');
    console.log('Grid table found:', gridTable.length, gridTable.attr('id'));

    // Check rows
    const rows = gridTable.find('tbody tr');
    console.log('Rows found:', rows.length);

    // Initialize totals for all columns
    let totals = {
        april: 0, may: 0, june: 0, july: 0, august: 0, september: 0,
        october: 0, november: 0, december: 0, january: 0, february: 0, march: 0,
        time: 0, cost: 0, yrPlan: 0
    };

    // Sum up each column from the grid rows using data-property attributes
    rows.each(function(index) {
        const $row = $(this);

        // Month columns (M1-M12)
        totals.april += parseFloatSafe($row.find('td[data-property="M1"] span').text());
        totals.may += parseFloatSafe($row.find('td[data-property="M2"] span').text());
        totals.june += parseFloatSafe($row.find('td[data-property="M3"] span').text());
        totals.july += parseFloatSafe($row.find('td[data-property="M4"] span').text());
        totals.august += parseFloatSafe($row.find('td[data-property="M5"] span').text());
        totals.september += parseFloatSafe($row.find('td[data-property="M6"] span').text());
        totals.october += parseFloatSafe($row.find('td[data-property="M7"] span').text());
        totals.november += parseFloatSafe($row.find('td[data-property="M8"] span').text());
        totals.december += parseFloatSafe($row.find('td[data-property="M9"] span').text());
        totals.january += parseFloatSafe($row.find('td[data-property="M10"] span').text());
        totals.february += parseFloatSafe($row.find('td[data-property="M11"] span').text());
        totals.march += parseFloatSafe($row.find('td[data-property="M12"] span').text());

        // Sum of Time
        totals.time += parseFloatSafe($row.find('td[data-property="SumOfTime"] span').text());

        // Sum of Cost
        totals.cost += parseFloatSafe($row.find('td[data-property="SumOfCost"] span').text());

        // Budget (Year Plan)
        totals.yrPlan += parseFloatSafe($row.find('td[data-property="Budget"] span').text());
    });

    console.log('Totals calculated:', totals);

    // Calculate percentage spent
    const percentSpent = totals.yrPlan > 0 ? ((totals.cost / totals.yrPlan) * 100).toFixed(2) : '0.00';

    // Update the totals display with formatting
    $('#total_april').text(formatNumber(totals.april));
    $('#total_may').text(formatNumber(totals.may));
    $('#total_june').text(formatNumber(totals.june));
    $('#total_july').text(formatNumber(totals.july));
    $('#total_august').text(formatNumber(totals.august));
    $('#total_september').text(formatNumber(totals.september));
    $('#total_october').text(formatNumber(totals.october));
    $('#total_november').text(formatNumber(totals.november));
    $('#total_december').text(formatNumber(totals.december));
    $('#total_january').text(formatNumber(totals.january));
    $('#total_february').text(formatNumber(totals.february));
    $('#total_march').text(formatNumber(totals.march));
    $('#total_time').text(formatNumber(totals.time));
    $('#total_cost').text('£' + formatNumber(totals.cost));
    $('#total_yrPlan').text('£' + formatNumber(totals.yrPlan));
    $('#total_spent').text(percentSpent + '%');

    console.log('Totals updated in DOM');
}

// Helper function to safely parse float values (removes £, commas, %)
function parseFloatSafe(text) {
    if (!text) return 0;
    const cleaned = text.replace(/[£,%]/g, '').replace(/,/g, '').trim();
    const value = parseFloat(cleaned);
    return isNaN(value) ? 0 : value;
}

// Helper function to format numbers with commas and 2 decimal places
function formatNumber(num) {
    return num.toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

// Handle row selection to populate project details
$(document).on('click', 'table[id^="tbl_summarised"]:not(.totals-table) tbody tr', function () {
    const $row = $(this);
    const project = $row.find('td[data-property="ParentProject"] span').text().trim();

    // Set the selected project immediately
    $('#txtSelectedProject').val(project);

    // Clear the description while loading
    $('#txtProjectDescription').val('Loading...');

    if (!project) {
        $('#txtProjectDescription').val('');
        return;
    }

    // Try to get ProjectTitle from data attribute first, then from hidden column
    let title = $row.data('projecttitle') || '';

    // If not found in data attribute, try to find it from a ProjectTitle cell (if column exists)
    if (!title) {
        const titleCell = $row.find('td[data-property="ProjectTitle"] span');
        if (titleCell.length > 0) {
            title = titleCell.text().trim();
        }
    }

    // If we found the title in the row data, use it
    if (title) {
        $('#txtProjectDescription').val(title);
        console.log('Selected project:', project, 'Title from row:', title);
    } else {
        // Otherwise, fetch from the API
        fetchProjectDescription(project);
    }
});

// Fetch project description from the API
function fetchProjectDescription(projectId) {
    console.log('Fetching project description for:', projectId);

    $.ajax({
        url: '/PACT/SummarisedWgTime/GetProjectDescription',
        type: 'GET',
        data: { projectId: projectId },
        success: function(response) {
            if (response.success) {
                $('#txtProjectDescription').val(response.projectTitle);
                console.log('Project description fetched:', response.projectTitle);
            } else {
                $('#txtProjectDescription').val('Not found');
                console.log('Project not found:', response.message);
            }
        },
        error: function(xhr, status, error) {
            console.error('Error fetching project description:', error);
            $('#txtProjectDescription').val('Error loading description');
        }
    });
}

// ============================================================
// RESET CALCULATION FUNCTION
// ============================================================
function resetCalculationGrid() {
    console.log('Reset calculation requested');

    // Find the grid table and reset the Budget and Spent columns for all rows
    const gridTable = $('table[id^="tbl_summarised"]').not('.totals-table');
    const rows = gridTable.find('tbody tr');

    let resetCount = 0;
    rows.each(function() {
        const $row = $(this);

        // Reset the Budget (YrPlan) cell to empty or 0
        $row.find('td[data-property="Budget"] span').text('£0.00');

        // Reset the PercentSpent cell to 0%
        $row.find('td[data-property="PercentSpent"] span').text('0.00%');

        resetCount++;
    });

    console.log('Reset completed. Total rows reset:', resetCount);

    // Clear selected project
    $('#txtSelectedProject').val('');
    $('#txtProjectDescription').val('');

    // Recalculate totals to reflect reset values
    calculateColumnTotals();
}

// ============================================================
// CALCULATE YEAR PLAN MODAL FUNCTIONS
// ============================================================
function openTimeRecordModal() {
    $('#timeRecordModal').css('display', 'flex');
    $('#modal-amount').val('');
    $('#formTimeRecord-db-error').attr('hidden', true);
    $('#modal-amount-error').attr('hidden', true);
}

function closeTimeRecordModal() {
    $('#timeRecordModal').css('display', 'none');
    $('#formTimeRecord')[0].reset();
}

function resetCalculation() {
    // Clear the amount field
    $('#modal-amount').val('');
    // Hide any error messages
    $('#modal-amount-error').attr('hidden', true);
    $('#formTimeRecord-db-error').attr('hidden', true);
    // Focus back on the input
    $('#modal-amount').focus();
}

function calculateSpent() {
    const amount = $('#modal-amount').val();

    // Validate input
    if (!amount || parseFloat(amount) <= 0) {
        $('#modal-amount-error-msg').text('Please enter a valid amount');
        $('#modal-amount-error').attr('hidden', false);
        return;
    }

    // Hide any errors
    $('#modal-amount-error').attr('hidden', true);
    $('#formTimeRecord-db-error').attr('hidden', true);

    const yrPlanAmount = parseFloat(amount);

    console.log('Calculate year plan for all projects with amount:', yrPlanAmount);

    // Find the grid table and update the Budget column for all rows
    const gridTable = $('table[id^="tbl_summarised"]').not('.totals-table');
    const rows = gridTable.find('tbody tr');

    let updatedCount = 0;
    rows.each(function() {
        const $row = $(this);
        const project = $row.find('td[data-property="ParentProject"] span').text().trim();

        // Get the Cost value for this row
        const costText = $row.find('td[data-property="SumOfCost"] span').text();
        const cost = parseFloatSafe(costText);

        // Calculate Spent percentage: (Cost / YrPlan) * 100
        const spentPercentage = yrPlanAmount > 0 ? ((cost / yrPlanAmount) * 100).toFixed(2) : '0.00';

        // Update the Budget (YrPlan) cell
        $row.find('td[data-property="Budget"] span').text('£' + formatNumber(yrPlanAmount));

        // Update the PercentSpent cell
        $row.find('td[data-property="PercentSpent"] span').text(spentPercentage + '%');

        console.log('Updated project:', project, 'YrPlan:', yrPlanAmount, 'Cost:', cost, 'Spent:', spentPercentage + '%');
        updatedCount++;
    });

    console.log('Total rows updated:', updatedCount);

    // Recalculate and update totals
    calculateColumnTotals();

    // Close modal
    closeTimeRecordModal();

    // Show success message
    console.log('Year Plan calculated successfully for all projects. Updated', updatedCount, 'rows.');
}

// Close modal when clicking outside
$(window).on('click', function(event) {
    if ($(event.target).hasClass('govuk-edit-modal')) {
        if (event.target.id === 'timeRecordModal') {
            closeTimeRecordModal();
        }
    }
});

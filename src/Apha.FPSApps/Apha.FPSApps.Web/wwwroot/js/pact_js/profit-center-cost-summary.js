// Profit Center Cost Summary JavaScript

(function () {
    'use strict';

    let profitCenterCostData = [];

    // Initialize on page load
    document.addEventListener('DOMContentLoaded', function () {
        initializePage();
    });

    /**
     * Initialize the page - set up event listeners and fetch data
     */
    function initializePage() {
        setupPeriodSelectorListener();
        loadProfitCenterCostData();
    }

    /**
     * Set up event listener for period selector dropdown
     */
    function setupPeriodSelectorListener() {
        const periodSelect = document.getElementById('periodSelect');
        if (!periodSelect) return;

        // Add event listener for period change
        periodSelect.addEventListener('change', function () {
            loadProfitCenterCostData();
        });
    }

    /**
     * Fetch profit center cost data from API
     */
    function loadProfitCenterCostData() {
        const tableBody = document.getElementById('profitCenterCostTableBody');
        if (!tableBody) return;

        // Show loading state
        showLoadingState(tableBody);

        // Get selected period value
        const periodSelect = document.getElementById('periodSelect');
        const monthNumber = periodSelect ? periodSelect.value : '';

        // Build URL with optional month filter
        let url = '/PACT/ProfitCenterCostSummary/GetProfitCenterCostData';
        if (monthNumber) {
            url += `?monthNumber=${encodeURIComponent(monthNumber)}`;
        }

        // Fetch data from API
        fetch(url)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to fetch data');
                }
                return response.json();
            })
            .then(data => {
                profitCenterCostData = data || [];
                renderProfitCenterCostTable(profitCenterCostData);
            })
            .catch(error => {
                console.error('Error loading profit center cost data:', error);
                showErrorState(tableBody);
            });
    }

    /**
     * Show loading state in table body
     * @param {HTMLElement} tableBody - The table body element
     */
    function showLoadingState(tableBody) {
        tableBody.innerHTML = '';
        const row = createTableRow();
        const cell = createTableCell('', 2, 'center');
        cell.innerHTML = '<div class="govuk-!-margin-bottom-2">Loading profit center cost data...</div>';
        row.appendChild(cell);
        tableBody.appendChild(row);
    }

    /**
     * Show error state in table body
     * @param {HTMLElement} tableBody - The table body element
     */
    function showErrorState(tableBody) {
        tableBody.innerHTML = '';
        const row = createTableRow();
        const cell = createTableCell('Error loading data. Please try again later.', 2, 'center');
        cell.style.color = '#d4351c';
        row.appendChild(cell);
        tableBody.appendChild(row);
    }

    /**
     * Show no data state in table body
     * @param {HTMLElement} tableBody - The table body element
     */
    function showNoDataState(tableBody) {
        tableBody.innerHTML = '';
        const row = createTableRow();
        const cell = createTableCell('No profit center cost data available.', 2, 'center');
        row.appendChild(cell);
        tableBody.appendChild(row);
    }

    /**
     * Create a table row element
     * @returns {HTMLTableRowElement}
     */
    function createTableRow() {
        const row = document.createElement('tr');
        row.className = 'govuk-table__row';
        return row;
    }

    /**
     * Create a table cell element
     * @param {string} content - Cell content
     * @param {number} colspan - Column span (optional)
     * @param {string} align - Text alignment (optional)
     * @returns {HTMLTableCellElement}
     */
    function createTableCell(content, colspan = 1, align = 'left') {
        const cell = document.createElement('td');
        cell.className = 'govuk-table__cell';
        if (colspan > 1) {
            cell.colSpan = colspan;
        }
        if (align === 'center') {
            cell.style.textAlign = 'center';
            cell.style.padding = '20px';
        } else if (align === 'right') {
            cell.classList.add('govuk-table__cell--numeric');
        }
        cell.textContent = content;
        return cell;
    }

    /**
     * Render the profit center cost table
     * @param {Array} data - Array of profit center cost objects
     */
    function renderProfitCenterCostTable(data) {
        const tableBody = document.getElementById('profitCenterCostTableBody');
        const totalCostCell = document.getElementById('totalCost');

        if (!tableBody) return;

        // Clear existing rows
        tableBody.innerHTML = '';

        if (!data || data.length === 0) {
            showNoDataState(tableBody);
            if (totalCostCell) {
                totalCostCell.textContent = '£0.00';
            }
            return;
        }

        // Sort data by profit center name
        data.sort((a, b) => (a.profitCentre || '').localeCompare(b.profitCentre || ''));

        let totalCost = 0;

        // Create table rows
        data.forEach(item => {
            const row = createTableRow();

            const profitCentreCell = createTableCell(item.profitCentre || 'N/A');

            const cost = item.cost || 0;
            const costCell = createTableCell(formatCurrency(cost), 1, 'right');

            totalCost += cost;

            row.appendChild(profitCentreCell);
            row.appendChild(costCell);
            tableBody.appendChild(row);
        });

        // Update total
        if (totalCostCell) {
            totalCostCell.textContent = formatCurrency(totalCost);
        }
    }

    /**
     * Format number as currency (GBP)
     * @param {number} value - The numeric value to format
     * @returns {string} Formatted currency string
     */
    function formatCurrency(value) {
        return new Intl.NumberFormat('en-GB', {
            style: 'currency',
            currency: 'GBP',
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(value);
    }

})();

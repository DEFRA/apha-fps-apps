// ============================================
// Toggle Sidebar Function
// ============================================
function toggleSidebar() {
    const sidebar = document.querySelector('.sidenav');
    if (sidebar) {
        sidebar.classList.toggle('collapsed');
    }
}

// ============================================
// GOV.UK Tabs Navigation
// ============================================
document.addEventListener("DOMContentLoaded", function () {
    // Let GOV.UK Frontend handle tabs if available
    if (!window.GOVUKFrontend) {
        // Fallback: manual tab handling if GOV.UK Frontend is not available
        const tabs = document.querySelectorAll(".govuk-tabs__tab");
        const panels = document.querySelectorAll(".govuk-tabs__panel");

        if (tabs.length > 0 && panels.length > 0) {
            tabs.forEach((tab) => {
                tab.addEventListener("click", function (e) {
                    e.preventDefault();
                    const targetId = this.getAttribute("href").substring(1);

                    panels.forEach((panel) => {
                        panel.classList.add("govuk-tabs__panel--hidden");
                    });

                    document.querySelectorAll(".govuk-tabs__list-item")
                        .forEach((li) => li.classList.remove("govuk-tabs__list-item--selected"));

                    const targetPanel = document.getElementById(targetId);
                    if (targetPanel) {
                        targetPanel.classList.remove("govuk-tabs__panel--hidden");
                    }

                    const parentLi = this.parentElement;
                    if (parentLi) {
                        parentLi.classList.add("govuk-tabs__list-item--selected");
                    }
                });
            });
        }
    }
});

// ============================================
// Table Sorting Functionality
// ============================================
function initializeTableFeatures() {
    const tables = document.querySelectorAll('.govuk-table.fps-sr-data-table, .user-table.project-year-table');

    tables.forEach(table => {
        // Add sorting to headers
        const headers = table.querySelectorAll('thead th.govuk-table__header, thead th');
        headers.forEach((header, index) => {
            // Skip action columns
            if (header.textContent.trim().toLowerCase() === 'actions') {
                return;
            }

            // Make header sortable
            header.style.cursor = 'pointer';
            header.style.position = 'relative';
            header.style.userSelect = 'none';

            // Add sort indicator
            const sortIndicator = document.createElement('span');
            sortIndicator.className = 'sort-indicator';
            sortIndicator.innerHTML = ' ⇅';
            sortIndicator.style.opacity = '0.3';
            sortIndicator.style.marginLeft = '5px';
            header.appendChild(sortIndicator);

            // Add click event for sorting
            header.addEventListener('click', function() {
                sortTable(table, index, this);
            });
        });

        // Add column resizing
        addColumnResizing(table);
    });
}

function sortTable(table, columnIndex, headerElement) {
    const tbody = table.querySelector('tbody');
    if (!tbody) return;

    const rows = Array.from(tbody.querySelectorAll('tr'));
    const currentDirection = headerElement.dataset.sortDirection || 'asc';
    const newDirection = currentDirection === 'asc' ? 'desc' : 'asc';

    // Clear all sort indicators
    table.querySelectorAll('thead th .sort-indicator').forEach(indicator => {
        indicator.innerHTML = ' ⇅';
        indicator.style.opacity = '0.3';
    });

    // Update current sort indicator
    const sortIndicator = headerElement.querySelector('.sort-indicator');
    sortIndicator.innerHTML = newDirection === 'asc' ? ' ▲' : ' ▼';
    sortIndicator.style.opacity = '1';
    headerElement.dataset.sortDirection = newDirection;

    // Sort rows
    rows.sort((a, b) => {
        const aCell = a.cells[columnIndex];
        const bCell = b.cells[columnIndex];

        if (!aCell || !bCell) return 0;

        let aValue = aCell.textContent.trim();
        let bValue = bCell.textContent.trim();

        // Try to parse as number (remove currency symbols and commas)
        const aNum = parseFloat(aValue.replace(/[£,]/g, ''));
        const bNum = parseFloat(bValue.replace(/[£,]/g, ''));

        if (!isNaN(aNum) && !isNaN(bNum)) {
            return newDirection === 'asc' ? aNum - bNum : bNum - aNum;
        }

        // String comparison
        return newDirection === 'asc' 
            ? aValue.localeCompare(bValue)
            : bValue.localeCompare(aValue);
    });

    // Reorder rows in DOM
    rows.forEach(row => tbody.appendChild(row));
}

// ============================================
// Column Resizing Functionality
// ============================================
function addColumnResizing(table) {
    const headers = table.querySelectorAll('thead th');

    headers.forEach((header, index) => {
        header.style.position = 'relative';

        // Create resize handle
        const resizeHandle = document.createElement('div');
        resizeHandle.className = 'resize-handle';
        resizeHandle.style.position = 'absolute';
        resizeHandle.style.top = '0';
        resizeHandle.style.right = '0';
        resizeHandle.style.width = '5px';
        resizeHandle.style.height = '100%';
        resizeHandle.style.cursor = 'col-resize';
        resizeHandle.style.userSelect = 'none';
        resizeHandle.style.zIndex = '10';

        // Visual indicator on hover
        resizeHandle.addEventListener('mouseenter', function() {
            this.style.backgroundColor = 'rgba(0, 0, 0, 0.2)';
        });

        resizeHandle.addEventListener('mouseleave', function() {
            if (!this.classList.contains('resizing')) {
                this.style.backgroundColor = 'transparent';
            }
        });

        header.appendChild(resizeHandle);

        // Resize logic
        let startX, startWidth;

        resizeHandle.addEventListener('mousedown', function(e) {
            e.preventDefault();
            startX = e.pageX;
            startWidth = header.offsetWidth;

            resizeHandle.classList.add('resizing');
            resizeHandle.style.backgroundColor = 'rgba(0, 0, 0, 0.3)';

            document.addEventListener('mousemove', onMouseMove);
            document.addEventListener('mouseup', onMouseUp);
        });

        function onMouseMove(e) {
            const width = startWidth + (e.pageX - startX);
            if (width > 50) { // Minimum width
                header.style.width = width + 'px';
                header.style.minWidth = width + 'px';
            }
        }

        function onMouseUp() {
            resizeHandle.classList.remove('resizing');
            resizeHandle.style.backgroundColor = 'transparent';

            document.removeEventListener('mousemove', onMouseMove);
            document.removeEventListener('mouseup', onMouseUp);
        }
    });
}
// Initialize table sorting and resizing for this page
document.addEventListener('DOMContentLoaded', function () {
    // Wait for GOV.UK Frontend to initialize
    setTimeout(function () {
        if (typeof initializeTableFeatures === 'function') {
            initializeTableFeatures();
        }
    }, 200);
});


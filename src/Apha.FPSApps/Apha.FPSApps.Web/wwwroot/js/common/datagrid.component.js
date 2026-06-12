/**
 * Reusable DataGrid Component
 * Version: 1.0
 * 
 * Usage:
 * var grid = new DataGridComponent({
 *     gridId: 'myGrid',
 *     containerSelector: '#gridContainer',
 *     columns: [...],
 *     data: [...],
 *     pageSize: 10,
 *     enableSort: true,
 *     enableFilter: true,
 *     enableResize: true,
 *     showAddButton: true,
 *     addButtonText: 'Add New Record',
 *     showSecondButton: true,
 *     secondButtonText: 'Custom Action',
 *     callbacks: { ... }
 * });
 */

(function(window) {
    'use strict';

    /**
     * DataGridComponent Constructor
     * @param {Object} config - Configuration object
     */
    function DataGridComponent(config) {
        // Default configuration
        this.config = Object.assign({
            gridId: 'dataGrid',
            containerSelector: '#gridContainer',
            columns: [],
            data: [],
            pageSize: 10,
            currentPage: 1,
            enableSort: true,
            enableFilter: true,
            enableResize: true,
            enableSelection: false,
            enablePagination: true,
            title: 'Data Grid',
            showAddButton: false,
            addButtonText: 'Add',
            showSecondButton: false,
            secondButtonText: 'Action',
            iconButtons: false,
            iconBasePath: '../images/',
            containerMinHeight: null, // e.g., '400px', '50vh'
            scrollContainerMaxHeight: null, // e.g., '400px', '50vh', 'auto'
            scrollContainerHeight:null,
            pageSizeOptions: [5, 10, 20, 50, 100],
            callbacks: {
                onAdd: null,
                onSecondButton: null,
                onEdit: null,
                onDelete: null,
                onCopy: null,
                onRowSelect: null,
                onBulkCopy: null,
                onBulkDelete: null
            }
        }, config);

        this.originalData = [...this.config.data];
        this.filteredData = [...this.config.data];
        this.sortConfig = { column: null, descending: false };
        this.filterModel = {};
        this.selectedRows = new Set();

        this.init();
    }

    /**
     * Initialize the component
     */
    DataGridComponent.prototype.init = function() {
        this.render();
        this.attachEventHandlers();
        this.applyStoredColumnWidths();
    };

    /**
     * Render the entire grid
     */
    DataGridComponent.prototype.render = function() {
        var container = document.querySelector(this.config.containerSelector);
        if (!container) {
            console.error('Container not found:', this.config.containerSelector);
            return;
        }

        container.innerHTML = this.getGridHTML();
        this.renderTableBody();
        this.renderPagination();
    };

    /**
     * Generate the main grid HTML structure
     */
    DataGridComponent.prototype.getGridHTML = function() {
        var config = this.config;
        var gridId = config.gridId;
        var tableCaption = this.escapeHtml(config.tableCaption || config.title || config.gridId || 'Data table');

        var html = `
            <div class="editable-grid-container sup_border"${config.containerMinHeight ? ` style="min-height: ${config.containerMinHeight};"` : ''}>
                <div class="sup_border_bottom_0">
                ${(config.title || (config.showAddButton && config.addButtonText) || (config.showSecondButton && config.secondButtonText)) ? `
                    <h2 class="sup_border-bottom sup_text_color sup_heading_bg_color govuk-heading-s sup_margin_0 sup_align_heading">
                        <div class="sup_flex_end_center_100">
                           <div> ${config.title}</div>
                            <div>
                                ${config.showSecondButton ? `
                                <button type="button" class="govuk-button sup_margin_0 sup_p_8" id="${gridId}_secondBtn">
                                    ${config.secondButtonText}
                                </button>
                                ` : ''}
                                ${config.showAddButton ? `
                                <button type="button" class="govuk-button sup_margin_0" style="padding: 6px 10px; font-size: 13px;" id="${gridId}_addRowBtn">
                                    <img src="../images/circle-plus-solid-full-white.svg" alt="" aria-hidden="true" width="14" style="margin-right: 4px;"> ${config.addButtonText}
                                </button>
                                ` : ''}
                            </div>
                        </div>
                    </h2>
                    `:''}
                    <div class="grid-toolbar">
                        <div class="sup_flex_center_gap_0">
                            <!-- Filters will be rendered here if needed -->
                        </div>
                    </div>
                </div>

                <div>
                    <div class="grid-scroll-container"${config.scrollContainerHeight ? ` style="height: ${config.scrollContainerHeight}; max-height: ${config.scrollContainerMaxHeight || config.scrollContainerHeight}; overflow-y: auto;"` : ''}>
                        <table class="editable-grid-table govuk-table custom-table" id="tbl_${gridId}" style="margin-bottom: 0px;">
                            <caption class="govuk-visually-hidden">${tableCaption}</caption>
                            <thead class="govuk-table__head">
                                <tr class="govuk-table__row">
                                    ${this.getTableHeaderHTML()}
                                </tr>
                            </thead>
                            <tbody class="govuk-table__body" id="${gridId}_tableBody">
                                <!-- Table rows will be rendered here -->
                            </tbody>
                        </table>
                    </div>
                    ${config.enablePagination ? this.getPaginationHTML() : ''}
                </div>
            </div>
        `;

        return html;
    };

    /**
     * Generate table header HTML
     */
    DataGridComponent.prototype.getTableHeaderHTML = function() {
        var html = '';
        var config = this.config;

        if (config.enableSelection) {
            html += `
                <th class="govuk-table__header" style="width: 50px;">
                    <div class="govuk-checkboxes govuk-checkboxes--small"
                        data-module="govuk-checkboxes"
                        style=" display: flex; align-items: center;  justify-content: flex-start;">
                        <div class="govuk-checkboxes__item">
                            <input type='checkbox' class="govuk-checkboxes__input select-all-checkbox"
                                id="${config.gridId}_selectAll" name="${config.gridId}_selectAll" aria-label="Select all rows" />
                            <label class="govuk-label govuk-checkboxes__label"
                                for="${config.gridId}_selectAll"><span class="govuk-visually-hidden">Select all rows</span></label>
                        </div>
                   </div>
                </th>
            `;
        }

        config.columns.forEach(function(column) {
            var sortClass = config.enableSort && column.sortable !== false ? 'sortable-header' : '';
            var dataColumn = column.field ? `data-column="${column.field}"` : '';
            
            html += `
                <th ${dataColumn} scope="col" class="govuk-table__header ${sortClass}" 
                    ${column.width ? `style="width: ${column.width}px;"` : ''}>
                    ${column.header || column.field}
                    ${config.enableResize ? '<div class="column-resizer">&nbsp;</div>' : ''}
                </th>
            `;
        });

        if (config.callbacks.onEdit || config.callbacks.onDelete || config.callbacks.onCopy) {
            html += '<th class="govuk-table__header"' + (config.iconButtons ? ' style="width:70px;min-width:70px;"' : '') + '>Actions</th>';
        }

        return html;
    };

    /**
     * Get pagination HTML structure
     */
    DataGridComponent.prototype.getPaginationHTML = function() {
        var gridId = this.config.gridId;
        var pageSizeOptions = this.config.pageSizeOptions.map(function(size) {
            return `<option value="${size}" ${size === this.config.pageSize ? 'selected="selected"' : ''}>${size}</option>`;
        }, this).join('');

        return `
            <div class="sup_pagination_footer sup_p_0">
                <div class="sup_pagination_wrapper">
                    <div class="sup_margin_top_bottom_5_10 sup_flex_center">
                        <label for="${gridId}_pageSize" class="sup_margin_right_5">Records per page &nbsp;</label>
                        <select id="${gridId}_pageSize" class="govuk-select govuk-select--width-2 sup_width_4em govuk-!-font-size-16">
                            ${pageSizeOptions}
                        </select>
                    </div>
                    <nav class="govuk-pagination sup_margin_top_bottom_5_10" aria-label="Pagination">
                        <ul class="govuk-pagination__list" id="${gridId}_pagination">
                            <!-- Pagination will be rendered here -->
                        </ul>
                    </nav>
                </div>
            </div>
        `;
    };

    /**
     * Render table body with data
     */
    DataGridComponent.prototype.renderTableBody = function() {
        var tbody = document.getElementById(this.config.gridId + '_tableBody');
        if (!tbody) return;

        var paginatedData = this.getPaginatedData();
        var html = '';

        if (paginatedData.length === 0) {
            var colspan = this.config.columns.length + (this.config.enableSelection ? 1 : 0) + 
                         (this.config.callbacks.onEdit || this.config.callbacks.onDelete ? 1 : 0);
            html = `<tr><td colspan="${colspan}" style="text-align: center;">No data available</td></tr>`;
        } else {
            paginatedData.forEach(function(row, index) {
                html += this.getTableRowHTML(row, index);
            }, this);
        }

        tbody.innerHTML = html;
    };

    /**
     * Generate table row HTML
     */
    DataGridComponent.prototype.getTableRowHTML = function(row, rowIndex) {
        var config = this.config;
        var rowId = row.id || rowIndex;
        var html = `<tr data-id="${rowId}" data-row-index="${rowIndex}">`;

        // Selection checkbox
        if (config.enableSelection) {
            html += `
                <td class="govuk-table__cell"> 
                <div class="govuk-checkboxes govuk-checkboxes--small" data-module="govuk-checkboxes">
                    <div class="govuk-checkboxes__item" style="text-align:'center'">
                            <input type='checkbox' onclick="event.stopPropagation()" class="govuk-checkboxes__input row-checkbox" data-row-id="${rowId}" id="${config.gridId}_checkbox${rowId}" name="${config.gridId}_checkbox${rowId}" aria-label="Select row ${rowId}" />
                            <label class="govuk-label govuk-checkboxes__label sup_label_auto_width" for="${config.gridId}_checkbox${rowId}" style="padding: 0;"><span class="govuk-visually-hidden">Select row ${rowId}</span></label>
                    </div>
                </div>

                </td>
            `;
        }

        // Data columns
        config.columns.forEach(function(column) {
            var value = this.getCellValue(row, column);
            var cellHtml = column.render ? column.render(value, row, rowIndex) : this.escapeHtml(value);
            
            html += `<td class="govuk-table__cell" data-property="${column.field}">${cellHtml}</td>`;
        }, this);

        // Action buttons
        if (config.callbacks.onEdit || config.callbacks.onDelete || config.callbacks.onCopy) {
            html += `<td class="govuk-table__cell">`;
            
            if (config.callbacks.onEdit) {
                html += `<button type="button" class="govuk-button govuk-button--secondary sup_margin_0 sup_p_8 edit-row-btn" data-row-id="${rowId}" title="Edit"><img src="../images/pen-to-square-regular-full.svg" alt="Edit" width="16"></button> `;
            }
            if (config.callbacks.onCopy) {
                html += `<button type="button" class="govuk-button sup_margin_0 sup_p_8 copy-row-btn" data-row-id="${rowId}" title="Copy"><img src="../images/copy-regular-full.svg" alt="Copy" width="16"></button> `;
            }
            if (config.callbacks.onDelete) {
                html += `<button type="button" class="govuk-button govuk-button--warning sup_margin_0 sup_p_8 delete-row-btn" data-row-id="${rowId}" title="Delete"><img src="../images/trash-can-regular-full.svg" alt="Delete" width="16"></button>`;
            }
            
            html += `</td>`;
        }

        html += '</tr>';
        return html;
    };

    /**
     * Get cell value from row data
     */
    DataGridComponent.prototype.getCellValue = function(row, column) {
        if (typeof column.field === 'function') {
            return column.field(row);
        }
        return row[column.field] !== undefined ? row[column.field] : '';
    };

    /**
     * Escape HTML to prevent XSS
     */
    DataGridComponent.prototype.escapeHtml = function(text) {
        if (text === null || text === undefined) return '';
        var div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    };

    /**
     * Get paginated data
     */
    DataGridComponent.prototype.getPaginatedData = function() {
        if (!this.config.enablePagination) {
            return this.filteredData;
        }

        var startIndex = (this.config.currentPage - 1) * this.config.pageSize;
        var endIndex = startIndex + this.config.pageSize;
        return this.filteredData.slice(startIndex, endIndex);
    };

    /**
     * Render pagination controls
     */
    DataGridComponent.prototype.renderPagination = function() {
        if (!this.config.enablePagination) return;

        var paginationContainer = document.getElementById(this.config.gridId + '_pagination');
        if (!paginationContainer) return;

        var totalPages = Math.ceil(this.filteredData.length / this.config.pageSize);
        var currentPage = this.config.currentPage;
        var html = '';

        paginationContainer.innerHTML = '';

        // Previous button - always visible but disabled on first page
        var prevLi = document.createElement('li');
        prevLi.className = 'govuk-pagination__prev';
        if (currentPage > 1) {
            prevLi.innerHTML = `<a class="govuk-link govuk-pagination__link" href="#" data-pageno="${currentPage - 1}" aria-label="Previous page" rel="prev">
                <svg class="govuk-pagination__icon govuk-pagination__icon--prev" xmlns="http://www.w3.org/2000/svg" height="13" width="15" aria-hidden="true" focusable="false" viewBox="0 0 15 13">
                    <path d="m6.5938-0.0078125-6.7266 6.7266 6.7441 6.4062 1.377-1.449-4.1856-3.9768h12.896v-2h-12.984l4.2931-4.293-1.414-1.414z"></path>
                </svg>
                <span class="govuk-pagination__link-title">Previous<span class="govuk-visually-hidden"> page</span></span>
            </a>`;
        } else {
            prevLi.innerHTML = `<a class="govuk-link govuk-pagination__link govuk-pagination__link--disabled" href="#" aria-label="Previous page" aria-disabled="true">
                <svg class="govuk-pagination__icon govuk-pagination__icon--prev" xmlns="http://www.w3.org/2000/svg" height="13" width="15" aria-hidden="true" focusable="false" viewBox="0 0 15 13">
                    <path d="m6.5938-0.0078125-6.7266 6.7266 6.7441 6.4062 1.377-1.449-4.1856-3.9768h12.896v-2h-12.984l4.2931-4.293-1.414-1.414z"></path>
                </svg>
                <span class="govuk-pagination__link-title">Previous<span class="govuk-visually-hidden"> page</span></span>
            </a>`;
        }
        paginationContainer.appendChild(prevLi);

        // Page numbers
        var startPage = Math.max(1, currentPage - 2);
        var endPage = Math.min(totalPages, currentPage + 2);

        for (var i = startPage; i <= endPage; i++) {
            var pageLi = document.createElement('li');
            if (i === currentPage) {
                pageLi.className = 'govuk-pagination__item govuk-pagination__item--current';
                pageLi.innerHTML = `<a class="govuk-link govuk-pagination__link" href="#" aria-label="Page ${i}" aria-current="page">${i}</a>`;
            } else {
                pageLi.className = 'govuk-pagination__item';
                pageLi.innerHTML = `<a class="govuk-link govuk-pagination__link" href="#" data-pageno="${i}" aria-label="Page ${i}">${i}</a>`;
            }
            paginationContainer.appendChild(pageLi);
        }

        // Next button - always visible but disabled on last page
        var nextLi = document.createElement('li');
        nextLi.className = 'govuk-pagination__next';
        if (currentPage < totalPages) {
            nextLi.innerHTML = `<a class="govuk-link govuk-pagination__link" href="#" data-pageno="${currentPage + 1}" aria-label="Next page" rel="next">
                <span class="govuk-pagination__link-title">Next<span class="govuk-visually-hidden"> page</span></span>
                <svg class="govuk-pagination__icon govuk-pagination__icon--next" xmlns="http://www.w3.org/2000/svg" height="13" width="15" aria-hidden="true" focusable="false" viewBox="0 0 15 13">
                    <path d="m8.107-0.0078125-1.4136 1.414 4.2926 4.293h-12.986v2h12.896l-4.1855 3.9766 1.377 1.4492 6.7441-6.4062-6.7246-6.7266z"></path>
                </svg>
            </a>`;
        } else {
            nextLi.innerHTML = `<a class="govuk-link govuk-pagination__link govuk-pagination__link--disabled" href="#" aria-label="Next page" aria-disabled="true">
                <span class="govuk-pagination__link-title">Next<span class="govuk-visually-hidden"> page</span></span>
                <svg class="govuk-pagination__icon govuk-pagination__icon--next" xmlns="http://www.w3.org/2000/svg" height="13" width="15" aria-hidden="true" focusable="false" viewBox="0 0 15 13">
                    <path d="m8.107-0.0078125-1.4136 1.414 4.2926 4.293h-12.986v2h12.896l-4.1855 3.9766 1.377 1.4492 6.7441-6.4062-6.7246-6.7266z"></path>
                </svg>
            </a>`;
        }
        paginationContainer.appendChild(nextLi);
    };

    /**
     * Attach event handlers
     */
    DataGridComponent.prototype.attachEventHandlers = function() {
        var self = this;
        var container = document.querySelector(this.config.containerSelector);
        if (!container) return;

        // Page size change
        var pageSizeSelect = document.getElementById(this.config.gridId + '_pageSize');
        if (pageSizeSelect) {
            pageSizeSelect.addEventListener('change', function() {
                self.config.pageSize = parseInt(this.value);
                self.config.currentPage = 1;
                self.renderTableBody();
                self.renderPagination();
            });
        }

        // Pagination clicks
        container.addEventListener('click', function(e) {
            if (e.target.hasAttribute('data-pageno')) {
                e.preventDefault();
                var pageNo = parseInt(e.target.getAttribute('data-pageno'));
                if (pageNo > 0) {
                    self.config.currentPage = pageNo;
                    self.renderTableBody();
                    self.renderPagination();
                }
            }
        });

        // Sort header clicks
        if (this.config.enableSort) {
            container.addEventListener('click', function(e) {
                var th = e.target.closest('.sortable-header');
                if (th) {
                    var column = th.getAttribute('data-column');
                    if (column) {
                        self.sortByColumn(column);
                    }
                }
            });
        }

        // Add button
        var addBtn = document.getElementById(this.config.gridId + '_addRowBtn');
        if (addBtn && this.config.callbacks.onAdd) {
            addBtn.addEventListener('click', function() {
                self.config.callbacks.onAdd(self);
            });
        }

        // Second button
        var secondBtn = document.getElementById(this.config.gridId + '_secondBtn');
        if (secondBtn && this.config.callbacks.onSecondButton) {
            secondBtn.addEventListener('click', function() {
                self.config.callbacks.onSecondButton(self);
            });
        }

        // Edit, Delete, Copy buttons
        container.addEventListener('click', function(e) {
            var target = e.target.closest('.edit-row-btn, .delete-row-btn, .copy-row-btn') || e.target;

            if (target.classList.contains('edit-row-btn') && self.config.callbacks.onEdit) {
                var rowId = target.getAttribute('data-row-id');
                var rowData = self.getRowDataById(rowId);
                self.config.callbacks.onEdit(rowData, rowId, self);
            }

            if (target.classList.contains('delete-row-btn') && self.config.callbacks.onDelete) {
                var rowId = target.getAttribute('data-row-id');
                var rowData = self.getRowDataById(rowId);
                self.config.callbacks.onDelete(rowData, rowId, self);
            }

            if (target.classList.contains('copy-row-btn') && self.config.callbacks.onCopy) {
                var rowId = target.getAttribute('data-row-id');
                var rowData = self.getRowDataById(rowId);
                self.config.callbacks.onCopy(rowData, rowId, self);
            }
        });

        // Select all checkbox
        if (this.config.enableSelection) {
            var selectAllCheckbox = document.getElementById(this.config.gridId + '_selectAll');
            if (selectAllCheckbox) {
                selectAllCheckbox.addEventListener('change', function() {
                    var checkboxes = container.querySelectorAll('.row-checkbox');
                    checkboxes.forEach(function(checkbox) {
                        checkbox.checked = selectAllCheckbox.checked;
                    });
                });
            }

            // Individual checkboxes
            container.addEventListener('change', function(e) {
                if (e.target.classList.contains('row-checkbox')) {
                    var allCheckboxes = container.querySelectorAll('.row-checkbox');
                    var checkedCheckboxes = container.querySelectorAll('.row-checkbox:checked');
                    var selectAll = document.getElementById(self.config.gridId + '_selectAll');
                    if (selectAll) {
                        selectAll.checked = allCheckboxes.length === checkedCheckboxes.length;
                    }
                }
            });
        }

        // Column resizing
        if (this.config.enableResize) {
            this.initColumnResize();
        }
    };

    /**
     * Sort data by column
     */
    DataGridComponent.prototype.sortByColumn = function(column) {
        var self = this;
        
        // Toggle sort direction if same column, otherwise ascending
        if (this.sortConfig.column === column) {
            this.sortConfig.descending = !this.sortConfig.descending;
        } else {
            this.sortConfig.column = column;
            this.sortConfig.descending = false;
        }

        this.filteredData.sort(function(a, b) {
            var aVal = a[column];
            var bVal = b[column];
            
            // Handle null/undefined
            if (aVal === null || aVal === undefined) return 1;
            if (bVal === null || bVal === undefined) return -1;
            
            // Numeric comparison
            if (typeof aVal === 'number' && typeof bVal === 'number') {
                return self.sortConfig.descending ? bVal - aVal : aVal - bVal;
            }
            
            // String comparison
            aVal = String(aVal).toLowerCase();
            bVal = String(bVal).toLowerCase();
            
            if (self.sortConfig.descending) {
                return bVal.localeCompare(aVal);
            } else {
                return aVal.localeCompare(bVal);
            }
        });

        this.config.currentPage = 1;
        this.renderTableBody();
        this.renderPagination();
        this.updateSortIndicators();
    };

    /**
     * Update sort indicators in table headers
     */
    DataGridComponent.prototype.updateSortIndicators = function() {
        var table = document.getElementById('tbl_' + this.config.gridId);
        if (!table) return;

        var headers = table.querySelectorAll('th[data-column]');
        
        headers.forEach(function(header) {
            var column = header.getAttribute('data-column');
            
            // Remove existing classes and icons
            header.classList.remove('sorted-asc', 'sorted-desc');
            var existingIcon = header.querySelector('.sort-icon');
            if (existingIcon) {
                existingIcon.remove();
            }
            
            // Add current sort indicator
            if (column === this.sortConfig.column) {
                header.classList.add(this.sortConfig.descending ? 'sorted-desc' : 'sorted-asc');
                var icon = document.createElement('span');
                icon.className = 'sort-icon';
                icon.textContent = this.sortConfig.descending ? ' ▼' : ' ▲';
                header.appendChild(icon);
            }
        }, this);
    };

    /**
     * Filter data
     */
    DataGridComponent.prototype.filterData = function(filterModel) {
        var self = this;
        this.filterModel = filterModel;
        
        this.filteredData = this.originalData.filter(function(row) {
            for (var key in filterModel) {
                if (filterModel.hasOwnProperty(key)) {
                    var filterValue = String(filterModel[key]).toLowerCase();
                    var rowValue = String(row[key] || '').toLowerCase();
                    
                    if (rowValue.indexOf(filterValue) === -1) {
                        return false;
                    }
                }
            }
            return true;
        });

        this.config.currentPage = 1;
        this.renderTableBody();
        this.renderPagination();
    };

    /**
     * Update grid data
     */
    DataGridComponent.prototype.updateData = function(newData) {
        this.originalData = [...newData];
        this.filteredData = [...newData];
        this.config.currentPage = 1;
        this.renderTableBody();
        this.renderPagination();
    };

    /**
     * Get row data by ID
     */
    DataGridComponent.prototype.getRowDataById = function(rowId) {
        return this.filteredData.find(function(row) {
            return String(row.id) === String(rowId);
        });
    };

    /**
     * Get all selected row IDs
     */
    DataGridComponent.prototype.getSelectedRowIds = function() {
        var container = document.querySelector(this.config.containerSelector);
        var checkboxes = container.querySelectorAll('.row-checkbox:checked');
        var ids = [];
        
        checkboxes.forEach(function(checkbox) {
            ids.push(checkbox.getAttribute('data-row-id'));
        });
        
        return ids;
    };

    /**
     * Initialize column resize functionality
     */
    DataGridComponent.prototype.initColumnResize = function() {
        var self = this;
        var table = document.getElementById('tbl_' + this.config.gridId);
        if (!table) return;

        var pressed = false;
        var startX, startWidth, th;
        var resizing = false;

        var resizers = table.querySelectorAll('.column-resizer');
        resizers.forEach(function(resizer) {
            resizer.addEventListener('mousedown', function(e) {
                pressed = true;
                resizing = false;
                th = resizer.parentElement;
                startX = e.pageX;
                startWidth = th.offsetWidth;
                document.body.style.cursor = 'col-resize';
                e.preventDefault();
            });
        });

        document.addEventListener('mousemove', function(e) {
            if (!pressed) return;
            var diff = e.pageX - startX;
            if (Math.abs(diff) > 2) resizing = true;
            th.style.width = (startWidth + diff) + 'px';
        });

        window.addEventListener('mouseup', function(e) {
            if (pressed) {
                pressed = false;
                setTimeout(function() { resizing = false; }, 100);
                document.body.style.cursor = '';
                self.saveColumnWidths();
            }
        });
    };

    /**
     * Save column widths to localStorage
     */
    DataGridComponent.prototype.saveColumnWidths = function() {
        var widths = {};
        var table = document.getElementById('tbl_' + this.config.gridId);
        if (!table) return;

        var headers = table.querySelectorAll('th[data-column]');
        headers.forEach(function(th) {
            var column = th.getAttribute('data-column');
            if (column) {
                widths[column] = th.offsetWidth;
            }
        });

        localStorage.setItem(this.config.gridId + '_colWidths', JSON.stringify(widths));
    };

    /**
     * Apply stored column widths from localStorage
     */
    DataGridComponent.prototype.applyStoredColumnWidths = function() {
        try {
            var widths = JSON.parse(localStorage.getItem(this.config.gridId + '_colWidths') || '{}');
            var table = document.getElementById('tbl_' + this.config.gridId);
            if (!table) return;

            var headers = table.querySelectorAll('th[data-column]');
            headers.forEach(function(th) {
                var column = th.getAttribute('data-column');
                if (column && widths[column]) {
                    th.style.width = widths[column] + 'px';
                }
            });
        } catch (e) {
            console.error('Error applying column widths:', e);
        }
    };

    /**
     * Refresh/reload the grid
     */
    DataGridComponent.prototype.refresh = function() {
        this.renderTableBody();
        this.renderPagination();
    };

    // Expose to global scope
    window.DataGridComponent = DataGridComponent;

})(window);
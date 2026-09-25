/**
 * Reusable MultiColumnDropdown Component
 * Version: 1.0
 * 
 * Usage:
 * var dropdown = new MultiColumnDropdownComponent({
 *     dropdownId: 'myDropdown',
 *     containerSelector: '#dropdownContainer',
 *     placeholder: 'Select an option',
 *     searchPlaceholder: 'Type to search',
 *     columns: [
 *         { field: 'id', header: 'ID', width: '80px' },
 *         { field: 'name', header: 'Name', width: '200px' }
 *     ],
 *     data: [...],
 *     displayField: 'name',
 *     valueField: 'id',
 *     enableSearch: true,
 *     clearButtonClearsSelection: false, // false: clear search only (default), true: clear both search and selection
 *     callbacks: {
 *         onSelect: function(selectedItem) { ... },
 *         onClear: function(dropdown) { ... }
 *     }
 * });
 */

(function (window) {
    'use strict';

    // Global z-index counter for managing dropdown stacking
    var globalZIndex = 10000;
    var dropdownInstances = [];

    // Number of rows materialised in the DOM per chunk. Remaining rows are
    // appended lazily while the user scrolls, so opening a dropdown backed by a
    // large lookup no longer blocks on building thousands of table rows.
    var ROW_CHUNK_SIZE = 200;

    // Delay applied to the search box so a burst of keystrokes results in a
    // single filter + render pass instead of one per character.
    var SEARCH_DEBOUNCE_MS = 150;

    function closeOtherDropdowns(currentDropdown) {
        dropdownInstances.forEach(function (dropdownInstance) {
            if (dropdownInstance && dropdownInstance !== currentDropdown && dropdownInstance.isOpen) {
                dropdownInstance.closeDropdown();
            }
        });
    }

    /**
     * MultiColumnDropdownComponent Constructor
     * @param {Object} config - Configuration object
     */
    function MultiColumnDropdownComponent(config) {
        // Default configuration
        this.config = Object.assign({
            dropdownId: 'multiColumnDropdown',
            containerSelector: '#dropdownContainer',
            placeholder: 'Select an option',
            searchPlaceholder: 'Type to search',
            searchLabelText: '',
            ariaLabel: '',
            ariaLabelledBy: '',
            columns: [],
            data: [],
            displayField: 'name',
            valueField: 'id',
            enableSearch: true,
            showSerialNumber: true,
            labelText: '',
            required: false,
            disabled: false,
            clearButtonClearsSelection: false, // false: clear search only, true: clear both search and selection
            showClearButton: true, // false: hide the clear (X) button in the search box
            callbacks: {
                onSelect: null,
                onChange: null,
                onClear: null
            }
        }, config);

        this.originalData = [...this.config.data];
        this.filteredData = [...this.config.data];
        this.selectedValue = null;
        this.selectedItem = null;
        this.isOpen = false;
        this.focusedRowIndex = -1; // Track keyboard navigation

        // Rendering state: the table body is built lazily (on open) and in chunks.
        this.bodyDirty = true;
        this.renderedRowCount = 0;

        // Cache of lowercased, concatenated column values keyed by row object so
        // filtering does not re-stringify every cell on every keystroke.
        this.searchKeyCache = new WeakMap();

        // Superseded by the shared document-level handler; retained so existing
        // callers that inspect it keep working.
        this.documentClickHandler = null;

        dropdownInstances.push(this);

        this.init();
    }

    /**
     * Initialize the component
     */
    MultiColumnDropdownComponent.prototype.init = function () {
        this.render();
        this.attachEventHandlers();
    };

    /**
     * Render the entire dropdown
     */
    MultiColumnDropdownComponent.prototype.render = function () {
        var container = document.querySelector(this.config.containerSelector);
        if (!container) {
            console.error('Container not found:', this.config.containerSelector);
            return;
        }

        container.innerHTML = this.getDropdownHTML();

        // The panel is hidden until the user opens it, so defer building the
        // rows. This keeps modal/page load cost independent of the data volume.
        this.bodyDirty = true;
        this.renderedRowCount = 0;
    };

    /**
     * Generate the main dropdown HTML structure
     */
    MultiColumnDropdownComponent.prototype.getDropdownHTML = function () {
        var config = this.config;
        var dropdownId = config.dropdownId;
        var requiredMark = config.required ? '<span class="sup_color_red">*</span>' : '';
        var inputLabelText = this.escapeHtml(config.labelText || this.getReferencedLabelText(config.ariaLabelledBy) || config.ariaLabel || config.placeholder || 'Select an option');
        var inputAriaAttributes = config.labelText
            ? ''
            : config.ariaLabelledBy
                ? `aria-labelledby="${config.ariaLabelledBy}"`
                : `aria-label="${this.escapeHtml(config.ariaLabel || config.placeholder || 'Select an option')}"`;

        var html = `
            <div class="tableselectdropdown input-group searchfiels" data-dropdown-id="${dropdownId}">
                ${config.labelText ? `
                    <label for="${dropdownId}_input" class="govuk-label govuk-!-font-weight-bold">
                        ${config.labelText} ${requiredMark}
                    </label>
                ` : `
                    <label for="${dropdownId}_input" class="govuk-label govuk-visually-hidden">
                        ${inputLabelText} ${requiredMark}
                    </label>
                `}
                <input 
                    type="text" 
                    id="${dropdownId}_input" 
                    name="${dropdownId}_input" 
                    placeholder="${config.placeholder}" 
                    class="dropdown-input down-arrow-img govuk-input govuk-!-font-size-16" 
                    ${config.disabled ? 'disabled' : ''}
                    ${config.required ? 'required' : ''}
                    ${inputAriaAttributes}
                    readonly
                />
                <input type="hidden" id="${dropdownId}_value" />
                
                <div class="multicolumn-dropdown-panel" id="${dropdownId}_panel">
                    ${config.enableSearch ? `
                        <div class="search-box-wrapper">
                            ${config.searchLabelText ? `
                                <label for="${dropdownId}_search" class="govuk-label govuk-visually-hidden">
                                    ${this.escapeHtml(config.searchLabelText)}
                                </label>
                            ` : ''}
                            <input 
                                type="text" 
                                class="select-search-box" 
                                id="${dropdownId}_search"
                                placeholder="${config.searchPlaceholder}" 
                                aria-label="Search by code or name"
                            />
                            ${config.showClearButton ? `
                                <button 
                                    type="button" 
                                    class="govuk-button govuk-button--secondary clear-search-btn" 
                                    id="${dropdownId}_clearSearch"
                                    aria-label="Clear search"
                                >
                                    <span class="sup_error_text_color govuk-!-font-size-19">&times;</span>
                                </button>
                            ` : ''}
                        </div>
                    ` : ''}
                    
                    <div class="dropdown-table-wrapper">
                        <table>
                            <thead>
                                <tr>
                                    ${this.getTableHeaderHTML()}
                                </tr>
                            </thead>
                            <tbody id="${dropdownId}_tbody">
                                <!-- Table rows will be rendered here -->
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        `;

        return html;
    };

    MultiColumnDropdownComponent.prototype.getReferencedLabelText = function (labelId) {
        if (!labelId) {
            return '';
        }

        var labelElement = document.getElementById(labelId);
        return labelElement ? (labelElement.textContent || '').trim() : '';
    };

    /**
     * Generate table header HTML
     */
    MultiColumnDropdownComponent.prototype.getTableHeaderHTML = function () {
        var html = '';
        var config = this.config;

        // Serial number column
        if (config.showSerialNumber) {
            html += '<th style="width: 60px;">Sr. No</th>';
        }

        // Data columns
        config.columns.forEach(function (column) {
            var style = column.width ? `style="width: ${column.width};"` : '';
            html += `<th ${style}>${column.header || column.field}</th>`;
        });

        return html;
    };

    /**
     * Render table body with data
     */
    MultiColumnDropdownComponent.prototype.renderTableBody = function () {
        // While the panel is closed there is nothing to see; flag the body as
        // stale and rebuild it the next time the dropdown is opened.
        if (!this.isOpen) {
            this.bodyDirty = true;
            this.renderedRowCount = 0;
            return;
        }

        this.renderTableBodyNow();
    };

    /**
     * Build the first chunk of rows immediately, regardless of open state.
     */
    MultiColumnDropdownComponent.prototype.renderTableBodyNow = function () {
        var tbody = document.getElementById(this.config.dropdownId + '_tbody');
        if (!tbody) return;

        this.renderedRowCount = 0;

        if (this.filteredData.length === 0) {
            var colspan = this.config.columns.length + (this.config.showSerialNumber ? 1 : 0);
            tbody.innerHTML = `<tr><td colspan="${colspan}" style="text-align: center; padding: 10px;">No data available</td></tr>`;
            this.bodyDirty = false;
            return;
        }

        tbody.innerHTML = '';
        this.bodyDirty = false;
        this.renderMoreRows(ROW_CHUNK_SIZE);
    };

    /**
     * Remove every rendered row and mark the body for a rebuild on next open.
     */
    MultiColumnDropdownComponent.prototype.clearRenderedRows = function () {
        var tbody = document.getElementById(this.config.dropdownId + '_tbody');
        if (tbody) {
            tbody.innerHTML = '';
        }

        this.renderedRowCount = 0;
        this.bodyDirty = true;
    };

    /**
     * Ensure the table body reflects the current filtered data.
     */
    MultiColumnDropdownComponent.prototype.ensureBodyRendered = function () {
        if (this.bodyDirty) {
            this.renderTableBodyNow();
        }
    };

    /**
     * Append the next batch of rows to the table body.
     */
    MultiColumnDropdownComponent.prototype.renderMoreRows = function (count) {
        var tbody = document.getElementById(this.config.dropdownId + '_tbody');
        if (!tbody) return;

        var start = this.renderedRowCount;
        var end = Math.min(start + (count || ROW_CHUNK_SIZE), this.filteredData.length);
        if (end <= start) return;

        var html = '';
        for (var i = start; i < end; i++) {
            html += this.getTableRowHTML(this.filteredData[i], i);
        }

        tbody.insertAdjacentHTML('beforeend', html);
        this.renderedRowCount = end;
    };

    /**
     * Materialise rows up to (and including) the supplied index.
     */
    MultiColumnDropdownComponent.prototype.ensureRowRendered = function (rowIndex) {
        this.ensureBodyRendered();

        if (rowIndex >= this.renderedRowCount) {
            this.renderMoreRows(rowIndex - this.renderedRowCount + 1);
        }
    };

    /**
     * Generate table row HTML
     */
    MultiColumnDropdownComponent.prototype.getTableRowHTML = function (row, rowIndex) {
        var config = this.config;
        var rowValue = this.getFieldValue(row, config.valueField);
        var isSelected = this.selectedValue === rowValue;
        var isFocused = this.focusedRowIndex === rowIndex;

        var html = `<tr class="dropdown-row ${isSelected ? 'selected' : ''} ${isFocused ? 'focused' : ''}" 
                        data-value="${this.escapeHtml(rowValue)}" 
                        data-row-index="${rowIndex}"
                        tabindex="0">`;

        // Serial number
        if (config.showSerialNumber) {
            html += `<td>${rowIndex + 1}</td>`;
        }

        // Data columns
        config.columns.forEach(function (column) {
            var value = this.getFieldValue(row, column.field);
            var cellHtml = column.render ? column.render(value, row, rowIndex) : this.escapeHtml(value);
            html += `<td>${cellHtml}</td>`;
        }, this);

        html += '</tr>';
        return html;
    };

    /**
     * Get field value from row data
     */
    MultiColumnDropdownComponent.prototype.getFieldValue = function (row, field) {
        if (typeof field === 'function') {
            return field(row);
        }

        // Support nested properties like 'address.city'
        if (typeof field === 'string' && field.indexOf('.') > -1) {
            var parts = field.split('.');
            var value = row;
            for (var i = 0; i < parts.length; i++) {
                if (value === null || value === undefined) return '';
                value = value[parts[i]];
            }
            return value !== undefined ? value : '';
        }

        return row[field] !== undefined ? row[field] : '';
    };

    /**
     * Resolve the data item represented by a rendered row element.
     * The row's data-value is used first, because the currently filtered data
     * can be reset (e.g. when the dropdown is closed) before every handler for
     * the originating event has run, which would make the row index stale.
     */
    MultiColumnDropdownComponent.prototype.getItemForRow = function (row) {
        if (!row) return null;

        var rowValue = row.getAttribute('data-value');
        if (rowValue !== null) {
            var match = this.originalData.find(function (item) {
                return String(this.getFieldValue(item, this.config.valueField)) === String(rowValue);
            }, this);

            if (match) return match;
        }

        var rowIndex = parseInt(row.getAttribute('data-row-index'), 10);
        return isNaN(rowIndex) ? null : this.filteredData[rowIndex];
    };

    /**
     * Escape HTML to prevent XSS
     */
    MultiColumnDropdownComponent.prototype.escapeHtml = function (text) {
        if (text === null || text === undefined) return '';
        var div = document.createElement('div');
        div.textContent = String(text);
        return div.innerHTML;
    };

    /**
     * A single document-level click handler serves every live dropdown. One
     * listener per instance used to accumulate whenever a host partial was
     * re-injected (the modal case), so every click ended up running a handler
     * for each dropdown ever created.
     */
    var sharedDocumentClickHandler = null;

    function ensureSharedDocumentClickHandler() {
        if (sharedDocumentClickHandler) return;

        sharedDocumentClickHandler = function (e) {
            dropdownInstances.forEach(function (instance) {
                // Closed dropdowns have nothing to close, so they cost nothing.
                if (!instance.isOpen) return;

                var container = document.querySelector('[data-dropdown-id="' + instance.config.dropdownId + '"]');
                if (container && !container.contains(e.target)) {
                    instance.closeDropdown();
                }
            });
        };

        document.addEventListener('click', sharedDocumentClickHandler);
    }

    function releaseSharedDocumentClickHandler() {
        if (!sharedDocumentClickHandler || dropdownInstances.length > 0) return;

        document.removeEventListener('click', sharedDocumentClickHandler);
        sharedDocumentClickHandler = null;
    }

    /**
     * Attach event handlers
     */
    MultiColumnDropdownComponent.prototype.attachEventHandlers = function () {
        var self = this;
        var dropdownId = this.config.dropdownId;
        var input = document.getElementById(dropdownId + '_input');
        var panel = document.getElementById(dropdownId + '_panel');
        var searchBox = document.getElementById(dropdownId + '_search');
        var clearSearchBtn = document.getElementById(dropdownId + '_clearSearch');
        var tbody = document.getElementById(dropdownId + '_tbody');

        if (!input || !panel) return;

        // Toggle dropdown on input click
        input.addEventListener('click', function (e) {
            e.stopPropagation();
            if (!self.config.disabled) {
                self.toggleDropdown();
            }
        });

        // Keyboard support: open dropdown from the (readonly) input via keyboard
        input.addEventListener('keydown', function (e) {
            if (self.config.disabled) return;

            if (e.key === 'Enter' || e.key === ' ' || e.key === 'Spacebar') {
                e.preventDefault();
                self.toggleDropdown();
            } else if (e.key === 'ArrowDown') {
                e.preventDefault();
                if (!self.isOpen) {
                    self.openDropdown();
                } else if (self.config.enableSearch) {
                    var sb = document.getElementById(self.config.dropdownId + '_search');
                    if (sb) sb.focus();
                } else if (self.filteredData.length > 0) {
                    self.focusRow(0);
                }
            } else if (e.key === 'Escape' && self.isOpen) {
                e.preventDefault();
                self.closeDropdown();
            }
        });

        // Search functionality
        if (searchBox && this.config.enableSearch) {
            searchBox.addEventListener('input', function () {
                var value = this.value;
                if (self.searchDebounceTimer) {
                    clearTimeout(self.searchDebounceTimer);
                }
                self.searchDebounceTimer = setTimeout(function () {
                    self.searchDebounceTimer = null;
                    self.filterData(value);
                    self.focusedRowIndex = -1; // Reset focus when filtering
                }, SEARCH_DEBOUNCE_MS);
            });

            searchBox.addEventListener('click', function (e) {
                e.stopPropagation();
            });

            // Keyboard navigation from search box
            searchBox.addEventListener('keydown', function (e) {
                if (e.key === 'Tab' && !e.shiftKey) {
                    // Tab: Move focus to first row
                    if (self.filteredData.length > 0) {
                        e.preventDefault();
                        self.focusRow(0);
                    }
                } else if (e.key === 'ArrowDown') {
                    // Arrow Down: Move to first row
                    e.preventDefault();
                    if (self.filteredData.length > 0) {
                        self.focusRow(0);
                    }
                } else if (e.key === 'Escape') {
                    e.preventDefault();
                    self.closeDropdown();
                } else if (e.key === 'Enter') {
                    e.preventDefault();
                    // Apply any pending debounced search before acting on the results
                    self.flushPendingSearch();
                    // If there's exactly one filtered result, select it
                    if (self.filteredData.length === 1) {
                        self.selectItem(self.filteredData[0]);
                        self.closeDropdown();
                    }
                }
            });
        }

        // Clear search button
        if (clearSearchBtn && this.config.enableSearch) {
            clearSearchBtn.addEventListener('click', function (e) {
                e.stopPropagation();

                // Case 1: Clear only search box (default behavior)
                // Case 2: Clear both search box and dropdown selection (when clearButtonClearsSelection is true)
                if (self.config.clearButtonClearsSelection) {
                    // Clear the main dropdown selection
                    self.clear();
                }

                // Clear the search box
                if (searchBox) {
                    searchBox.value = '';
                    self.cancelPendingSearch();
                    self.filterData('');
                    searchBox.focus();
                }
            });
        }

        // Row selection
        if (tbody) {
            tbody.addEventListener('click', function (e) {
                var row = e.target.closest('.dropdown-row');
                if (row) {
                    var selectedData = self.getItemForRow(row);
                    self.selectItem(selectedData);
                    self.closeDropdown();
                }
            });

            // Keyboard navigation for rows
            tbody.addEventListener('keydown', function (e) {
                var row = e.target.closest('.dropdown-row');
                if (!row) return;

                var rowIndex = parseInt(row.getAttribute('data-row-index'));

                if (e.key === 'ArrowDown') {
                    e.preventDefault();
                    self.focusRow(rowIndex + 1);
                } else if (e.key === 'ArrowUp') {
                    e.preventDefault();
                    if (rowIndex > 0) {
                        self.focusRow(rowIndex - 1);
                    } else {
                        // Go back to search box
                        var searchBox = document.getElementById(self.config.dropdownId + '_search');
                        if (searchBox) {
                            searchBox.focus();
                            self.focusedRowIndex = -1;
                        }
                    }
                } else if (e.key === 'Enter') {
                    e.preventDefault();
                    e.stopPropagation();
                    self.selectItem(self.getItemForRow(row));
                    self.closeDropdown();
                } else if (e.key === 'Escape') {
                    e.preventDefault();
                    self.closeDropdown();
                } else if (e.key === 'Tab') {
                    e.preventDefault();
                    if (e.shiftKey) {
                        // Shift+Tab: Go back to search or previous row
                        if (rowIndex > 0) {
                            self.focusRow(rowIndex - 1);
                        } else {
                            var searchBox = document.getElementById(self.config.dropdownId + '_search');
                            if (searchBox) {
                                searchBox.focus();
                                self.focusedRowIndex = -1;
                            }
                        }
                    } else {
                        // Tab: Move to next row
                        self.focusRow(rowIndex + 1);
                    }
                }
            });
        }

        // Close dropdown when clicking outside
        ensureSharedDocumentClickHandler();

        // Append the next batch of rows as the user scrolls the panel
        var tableWrapper = panel.querySelector('.dropdown-table-wrapper');
        if (tableWrapper) {
            tableWrapper.addEventListener('scroll', function () {
                if (self.renderedRowCount >= self.filteredData.length) return;

                if (this.scrollTop + this.clientHeight >= this.scrollHeight - 100) {
                    self.renderMoreRows(ROW_CHUNK_SIZE);
                }
            });
        }

        // Prevent panel clicks from closing dropdown
        panel.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    };

    /**
     * Run any pending debounced search immediately.
     */
    MultiColumnDropdownComponent.prototype.flushPendingSearch = function () {
        if (!this.searchDebounceTimer) return;

        clearTimeout(this.searchDebounceTimer);
        this.searchDebounceTimer = null;

        var searchBox = document.getElementById(this.config.dropdownId + '_search');
        this.filterData(searchBox ? searchBox.value : '');
        this.focusedRowIndex = -1;
    };

    /**
     * Discard any pending debounced search.
     */
    MultiColumnDropdownComponent.prototype.cancelPendingSearch = function () {
        if (this.searchDebounceTimer) {
            clearTimeout(this.searchDebounceTimer);
            this.searchDebounceTimer = null;
        }
    };

    /**
     * Toggle dropdown open/close
     */
    MultiColumnDropdownComponent.prototype.toggleDropdown = function () {
        if (this.isOpen) {
            this.closeDropdown();
        } else {
            this.openDropdown();
        }
    };

    /**
     * Open dropdown
     */
    MultiColumnDropdownComponent.prototype.openDropdown = function () {
        var panel = document.getElementById(this.config.dropdownId + '_panel');
        var input = document.getElementById(this.config.dropdownId + '_input');
        var container = document.querySelector(this.config.containerSelector);

        if (panel && input) {
            closeOtherDropdowns(this);

            // Increase z-index to ensure this dropdown appears above others
            globalZIndex++;
            panel.style.zIndex = globalZIndex;
            if (container) {
                container.style.position = 'relative';
                container.style.zIndex = globalZIndex;
            }

            panel.style.display = 'block';
            this.isOpen = true;
            input.classList.add('dropdown-open');

            // Build the rows only now that the panel is actually visible
            this.ensureBodyRendered();

            // Focus search box if enabled
            if (this.config.enableSearch) {
                var searchBox = document.getElementById(this.config.dropdownId + '_search');
                if (searchBox) {
                    setTimeout(function () {
                        searchBox.focus();
                    }, 100);
                }
            }
        }
    };

    /**
     * Close dropdown
     */
    MultiColumnDropdownComponent.prototype.closeDropdown = function () {
        var panel = document.getElementById(this.config.dropdownId + '_panel');
        var input = document.getElementById(this.config.dropdownId + '_input');
        var searchBox = document.getElementById(this.config.dropdownId + '_search');
        var container = document.querySelector(this.config.containerSelector);

        if (panel && input) {
            panel.style.display = 'none';
            this.isOpen = false;
            input.classList.remove('dropdown-open');
            this.focusedRowIndex = -1; // Reset focus

            // Drop the rows materialised by lazy loading. Without this the DOM
            // keeps every row the user scrolled into view - for several
            // dropdowns at once - which slows down every later query, hit test
            // and style recalculation on the page.
            this.clearRenderedRows();

            // Reset z-index
            if (container) {
                container.style.zIndex = '';
            }

            // Clear search
            if (searchBox && this.config.enableSearch) {
                this.cancelPendingSearch();
                searchBox.value = '';
                this.filterData('');
            }
        }
    };

    /**
     * Focus a specific row for keyboard navigation
     */
    MultiColumnDropdownComponent.prototype.focusRow = function (rowIndex) {
        if (rowIndex < 0 || rowIndex >= this.filteredData.length) {
            return;
        }

        this.focusedRowIndex = rowIndex;
        var tbody = document.getElementById(this.config.dropdownId + '_tbody');
        if (!tbody) return;

        // The target row may not have been materialised yet when the list is
        // being rendered in chunks.
        this.ensureRowRendered(rowIndex);

        var row = tbody.querySelector('.dropdown-row[data-row-index="' + rowIndex + '"]');
        if (row) {
            row.focus();
            // Scroll into view if needed
            row.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
        }
    };

    /**
     * Filter data based on search term
     */
    MultiColumnDropdownComponent.prototype.filterData = function (searchTerm) {
        var self = this;
        searchTerm = String(searchTerm).toLowerCase().trim();

        if (!searchTerm) {
            this.filteredData = [...this.originalData];
        } else {
            this.filteredData = this.originalData.filter(function (row) {
                // Search across all columns using a cached, lowercased key
                return self.getSearchKey(row).indexOf(searchTerm) > -1;
            });
        }

        this.renderTableBody();
    };

    /**
     * Build (once per row) the lowercased concatenation of all searchable
     * column values. Cached so repeated filtering does not re-stringify cells.
     */
    MultiColumnDropdownComponent.prototype.getSearchKey = function (row) {
        if (row === null || typeof row !== 'object') {
            return String(this.getFieldValue(row, this.config.displayField)).toLowerCase();
        }

        var key = this.searchKeyCache.get(row);
        if (key !== undefined) return key;

        var parts = [];
        for (var i = 0; i < this.config.columns.length; i++) {
            parts.push(String(this.getFieldValue(row, this.config.columns[i].field)).toLowerCase());
        }

        key = parts.join('\u0000');
        this.searchKeyCache.set(row, key);
        return key;
    };

    /**
     * Select an item
     */
    MultiColumnDropdownComponent.prototype.selectItem = function (item) {
        var input = document.getElementById(this.config.dropdownId + '_input');
        var hiddenInput = document.getElementById(this.config.dropdownId + '_value');

        if (input && hiddenInput && item) {
            var displayValue = this.getFieldValue(item, this.config.displayField);
            var value = this.getFieldValue(item, this.config.valueField);

            input.value = displayValue;
            hiddenInput.value = value;

            this.selectedValue = value;
            this.selectedItem = item;

            // Update row selection styling
            this.updateRowSelection();

            // Trigger callback
            if (this.config.callbacks.onSelect) {
                this.config.callbacks.onSelect(item, this);
            }

            if (this.config.callbacks.onChange) {
                this.config.callbacks.onChange(item, this);
            }
        }
    };

    /**
     * Update row selection styling
     */
    MultiColumnDropdownComponent.prototype.updateRowSelection = function () {
        var tbody = document.getElementById(this.config.dropdownId + '_tbody');
        if (!tbody) return;

        var rows = tbody.querySelectorAll('.dropdown-row');
        rows.forEach(function (row) {
            var rowValue = row.getAttribute('data-value');
            if (String(rowValue) === String(this.selectedValue)) {
                row.classList.add('selected');
            } else {
                row.classList.remove('selected');
            }
        }, this);
    };

    /**
     * Set selected value programmatically
     */
    MultiColumnDropdownComponent.prototype.setValue = function (value) {
        var item = this.originalData.find(function (row) {
            return String(this.getFieldValue(row, this.config.valueField)) === String(value);
        }, this);

        if (item) {
            this.selectItem(item);
        }
    };

    /**
     * Get selected value
     */
    MultiColumnDropdownComponent.prototype.getValue = function () {
        return this.selectedValue;
    };

    /**
     * Get selected item
     */
    MultiColumnDropdownComponent.prototype.getSelectedItem = function () {
        return this.selectedItem;
    };

    /**
     * Clear selection
     */
    MultiColumnDropdownComponent.prototype.clear = function () {
        var input = document.getElementById(this.config.dropdownId + '_input');
        var hiddenInput = document.getElementById(this.config.dropdownId + '_value');

        if (input) input.value = '';
        if (hiddenInput) hiddenInput.value = '';

        this.selectedValue = null;
        this.selectedItem = null;

        this.updateRowSelection();

        if (this.config.callbacks.onClear) {
            this.config.callbacks.onClear(this);
        }
    };

    /**
     * Update dropdown data
     */
    MultiColumnDropdownComponent.prototype.updateData = function (newData) {
        this.originalData = [...newData];
        this.filteredData = [...newData];
        this.searchKeyCache = new WeakMap();
        this.renderTableBody();
    };

    /**
     * Enable dropdown
     */
    MultiColumnDropdownComponent.prototype.enable = function () {
        var input = document.getElementById(this.config.dropdownId + '_input');
        if (input) {
            input.disabled = false;
            this.config.disabled = false;
        }
    };

    /**
     * Disable dropdown
     */
    MultiColumnDropdownComponent.prototype.disable = function () {
        var input = document.getElementById(this.config.dropdownId + '_input');
        if (input) {
            input.disabled = true;
            this.config.disabled = true;
        }
        this.closeDropdown();
    };

    /**
     * Refresh/reload the dropdown
     */
    MultiColumnDropdownComponent.prototype.refresh = function () {
        this.bodyDirty = true;
        this.renderTableBody();
    };

    /**
     * Destroy the dropdown and clean up
     */
    MultiColumnDropdownComponent.prototype.destroy = function () {
        var self = this;

        this.cancelPendingSearch();
        this.clearRenderedRows();

        var container = document.querySelector(this.config.containerSelector);
        if (container) {
            container.innerHTML = '';
        }

        dropdownInstances = dropdownInstances.filter(function (dropdownInstance) {
            return dropdownInstance !== self;
        });

        releaseSharedDocumentClickHandler();

        // Clear references
        this.originalData = [];
        this.filteredData = [];
        this.searchKeyCache = new WeakMap();
        this.renderedRowCount = 0;
        this.bodyDirty = true;
        this.selectedValue = null;
        this.selectedItem = null;
    };

    // Expose to global scope
    window.MultiColumnDropdownComponent = MultiColumnDropdownComponent;

})(window);

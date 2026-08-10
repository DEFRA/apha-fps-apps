// DepartmentIncome Page JavaScript

// ── Month Dropdown State ───────────────────────────────────────────
// Note: departmentIncomePageMonths is initialized in the Razor view
// as a serialized JSON array of MonthItem (MonthNumber, MonthName).
// Source: tblkpMonth — 12 fiscal months, 1=April … 12=March.
// DO NOT redeclare it here — it is set via inline script in Index.cshtml.

// ========================================
// Multi-Column Dropdowns for Period From / Period To filters
// Follows the same pattern as initializeSubContractProjectDropdown in subcontract.js
// Access source: SELECT DISTINCTROW MonthNumber, MonthName FROM tblkpMonth ORDER BY MonthNumber
// 2 columns only; MonthNumber shown in input on selection
// ========================================

function initializeDepartmentIncomePeriodDropdowns(config) {

    var monthsData = config.monthsData || [];

    setTimeout(function () {

        // ── Period From ──────────────────────────────────────────────────
        var periodFromDropdown = new MultiColumnDropdownComponent({
            dropdownId:                'periodFrom',
            containerSelector:         '#periodFromDropdown',
            placeholder:               '--select--',
            searchPlaceholder:         'Type to search',
            showSerialNumber:          false,
            enableSearch:              false,
            clearButtonClearsSelection: false,
            columns: [
                { field: 'MonthNumber', header: 'Month No', width: '90px'  },
                { field: 'MonthName',   header: 'Month',    width: '130px' }
            ],
            data:         monthsData,
            displayField: 'MonthNumber',
            valueField:   'MonthNumber',
            callbacks: {
                onSelect: function (selectedItem) {
                    $('#monthFromSelect').val(String(selectedItem.MonthNumber)).trigger('change');
                },
                onClear: function () {
                    $('#monthFromSelect').val('').trigger('change');
                }
            }
        });

        var initialFrom = $('#monthFromSelect').val();
        if (initialFrom) {
            periodFromDropdown.setValue(initialFrom);
        }

        // ── Period To ────────────────────────────────────────────────────
        var periodToDropdown = new MultiColumnDropdownComponent({
            dropdownId:                'periodTo',
            containerSelector:         '#periodToDropdown',
            placeholder:               '--select--',
            searchPlaceholder:         'Type to search',
            showSerialNumber:          false,
            enableSearch:              false,
            clearButtonClearsSelection: false,
            columns: [
                { field: 'MonthNumber', header: 'Month No', width: '90px'  },
                { field: 'MonthName',   header: 'Month',    width: '130px' }
            ],
            data:         monthsData,
            displayField: 'MonthNumber',
            valueField:   'MonthNumber',
            callbacks: {
                onSelect: function (selectedItem) {
                    $('#monthToSelect').val(String(selectedItem.MonthNumber)).trigger('change');
                },
                onClear: function () {
                    $('#monthToSelect').val('').trigger('change');
                }
            }
        });

        var initialTo = $('#monthToSelect').val();
        if (initialTo) {
            periodToDropdown.setValue(initialTo);
        }

    }, 100);
}

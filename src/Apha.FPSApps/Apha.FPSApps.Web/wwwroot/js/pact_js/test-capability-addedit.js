(function (window, $) {
    'use strict';

    function initSingleColumnDropdown(config) {
        var hiddenField = $('#' + config.fieldId);
        if (!hiddenField.length || !$(config.containerSelector).length) {
            return null;
        }

        var dropdown = new MultiColumnDropdownComponent({
            dropdownId: config.dropdownId,
            containerSelector: config.containerSelector,
            placeholder: config.placeholder,
            searchPlaceholder: config.searchPlaceholder,
            searchLabelText: config.searchPlaceholder,
            columns: [
                { field: 'Text', header: config.headerText, width: '100%' }
            ],
            data: config.data || [],
            displayField: 'Text',
            valueField: 'Value',
            showSerialNumber: false,
            clearButtonClearsSelection: true,
            showClearButton: true,
            callbacks: {
                onSelect: function (selectedItem) {
                    hiddenField.val(selectedItem.Value).trigger('change');
                },
                onClear: function () {
                    hiddenField.val('').trigger('change');
                }
            }
        });

        var existingValue = hiddenField.val();
        if (existingValue) {
            dropdown.setValue(existingValue);
        }

        return dropdown;
    }

    // The modal partial is re-injected on every Add/Edit, so the components it
    // created previously must be disposed of, otherwise their data and DOM are
    // kept alive for the lifetime of the page.
    var activeDropdowns = [];

    function destroyActiveDropdowns() {
        activeDropdowns.forEach(function (dropdown) {
            if (dropdown && typeof dropdown.destroy === 'function') {
                dropdown.destroy();
            }
        });

        activeDropdowns = [];
    }

    window.initializeTestCapabilityAddEditDropdowns = function (options) {
        if (typeof MultiColumnDropdownComponent === 'undefined') {
            return;
        }

        destroyActiveDropdowns();

        options = options || {};

        if (!options.isEditMode) {
            activeDropdowns.push(initSingleColumnDropdown({
                fieldId: 'TestCode',
                dropdownId: 'testCapabilityTestCodeDropdown',
                containerSelector: '#testCodeMultiDropdown',
                placeholder: 'Select Test Code',
                searchPlaceholder: 'Search by Test Code',
                headerText: 'Test Code',
                data: options.testCodeData
            }));

            activeDropdowns.push(initSingleColumnDropdown({
                fieldId: 'WorkGroup',
                dropdownId: 'testCapabilityWorkGroupDropdown',
                containerSelector: '#workGroupMultiDropdown',
                placeholder: 'Select Work Group',
                searchPlaceholder: 'Search by Work Group',
                headerText: 'Work Group',
                data: options.workGroupData
            }));
        }

        activeDropdowns.push(initSingleColumnDropdown({
            fieldId: 'PlanPortfolio',
            dropdownId: 'testCapabilityPlanPortfolioDropdown',
            containerSelector: '#planPortfolioMultiDropdown',
            placeholder: 'Select Plan Portfolio',
            searchPlaceholder: 'Search by Plan Portfolio',
            headerText: 'Plan Portfolio',
            data: options.planPortfolioData
        }));

        activeDropdowns = activeDropdowns.filter(Boolean);
    };

    window.destroyTestCapabilityAddEditDropdowns = destroyActiveDropdowns;
}(window, jQuery));

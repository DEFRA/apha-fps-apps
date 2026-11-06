
var selectedProjectValue = null;
var selectedProfitCentreValue = null;
var selectedWorkGroupValue = null;
var selectedSellingWorkGroupValue = null;
var selectedBuyingProjectValue = null;

function downloadExcelReport(url, data) {
    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        xhrFields: { responseType: 'blob' },
        success: function (blob, status, xhr) {
            var contentDisposition = xhr.getResponseHeader('Content-Disposition');
            var fileName = 'export.xlsx';
            if (contentDisposition) {
                var match = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                if (match && match[1]) {
                    fileName = match[1].replace(/['"]/g, '');
                }
            }
            var downloadUrl = window.URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.href = downloadUrl;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            $(a).remove();
            window.URL.revokeObjectURL(downloadUrl);
        },
        error: function () {
            showAlertMessage('An error occurred while exporting.', AlertType.ERROR);
        }
    });
}

$(document).ready(function () {
    var selectProjectDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'selectProjectDropdown',
        containerSelector: '#selectProjectMultiDropdown',
        placeholder: 'Select Project',
        showSerialNumber: false,
        searchPlaceholder: 'Type to search project',
        labelText: '',
        columns: [
            { field: 'Value', header: 'Parent Project', width: '150px' },
            { field: 'Text', header: 'Manager', width: '150px' }
        ],
        data: projectOptionsListData,
        displayField: 'Value',
        valueField: 'Value',
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem, dropdown) {
                selectedProjectValue = selectedItem.Value;
            },
            onClear: function (dropdown) {
                selectedProjectValue = null;
            }
        }
    });

    var isClearingTimeSale = false;

    var selectProfitCentreDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'selectProfitCentreDropdown',
        containerSelector: '#selectProfitCentreMultiDropdown',
        placeholder: 'Select Profit Centre',
        showSerialNumber: false,
        searchPlaceholder: 'Type to search profit centre',
        labelText: '',
        columns: [
            { field: 'Value', header: 'Profit Centre', width: '150px' },
            { field: 'Text', header: 'Division', width: '150px' }
        ],
        data: profitCentreOptionsListData,
        displayField: 'Value',
        valueField: 'Value',
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem, dropdown) {
                selectedProfitCentreValue = selectedItem.Value;
                if (!isClearingTimeSale) {
                    isClearingTimeSale = true;
                    selectWorkGroupDropdown.clear();
                    isClearingTimeSale = false;
                }
            },
            onClear: function (dropdown) {
                selectedProfitCentreValue = null;
                if (!isClearingTimeSale) {
                    isClearingTimeSale = true;
                    selectWorkGroupDropdown.clear();
                    isClearingTimeSale = false;
                }
            }
        }
    });

    var selectWorkGroupDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'selectWorkGroupDropdown',
        containerSelector: '#selectWorkGroupMultiDropdown',
        placeholder: 'Select WorkGroup',
        showSerialNumber: false,
        searchPlaceholder: 'Type to search work group',
        labelText: '',
        columns: [
            { field: 'Value', header: 'Work Group', width: '150px' },
            { field: 'Text', header: 'Profit Centre', width: '150px' }
        ],
        data: workGroupOptionsListData,
        displayField: 'Value',
        valueField: 'Value',
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem, dropdown) {
                selectedWorkGroupValue = selectedItem.Value;
                if (!isClearingTimeSale) {
                    isClearingTimeSale = true;
                    selectProfitCentreDropdown.clear();
                    isClearingTimeSale = false;
                }
            },
            onClear: function (dropdown) {
                selectedWorkGroupValue = null;
                if (!isClearingTimeSale) {
                    isClearingTimeSale = true;
                    selectProfitCentreDropdown.clear();
                    isClearingTimeSale = false;
                }
            }
        }
    });

    var isClearing = false;

    var selectSellingWorkGroupDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'selectSellingWorkGroupDropdown',
        containerSelector: '#selectSellingWorkGroupMultiDropdown',
        placeholder: 'Select Selling Workgroup',
        showSerialNumber: false,
        searchPlaceholder: 'Type to search selling workgroup',
        labelText: '',
        columns: [
            { field: 'Value', header: 'Work Group', width: '150px' },
            { field: 'Text', header: 'Profit Centre', width: '150px' }
        ],
        data: workGroupOptionsListData,
        displayField: 'Value',
        valueField: 'Value',
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem, dropdown) {
                selectedSellingWorkGroupValue = selectedItem.Value;
                if (!isClearing) {
                    isClearing = true;
                    selectBuyingProjectDropdown.clear();
                    isClearing = false;
                }
            },
            onClear: function (dropdown) {
                selectedSellingWorkGroupValue = null;
                if (!isClearing) {
                    isClearing = true;
                    selectBuyingProjectDropdown.clear();
                    isClearing = false;
                }
            }
        }
    });

    var selectBuyingProjectDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'selectBuyingProjectDropdown',
        containerSelector: '#selectBuyingProjectMultiDropdown',
        placeholder: 'Select Buying Project',
        showSerialNumber: false,
        searchPlaceholder: 'Type to search buying project',
        labelText: '',
        columns: [
            { field: 'Value', header: 'Parent Project', width: '150px' },
            { field: 'Text', header: 'Manager', width: '150px' }
        ],
        data: projectOptionsListData,
        displayField: 'Value',
        valueField: 'Value',
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem, dropdown) {
                selectedBuyingProjectValue = selectedItem.Value;
                if (!isClearing) {
                    isClearing = true;
                    selectSellingWorkGroupDropdown.clear();
                    isClearing = false;
                }
            },
            onClear: function (dropdown) {
                selectedBuyingProjectValue = null;
                if (!isClearing) {
                    isClearing = true;
                    selectSellingWorkGroupDropdown.clear();
                    isClearing = false;
                }
            }
        }
    });

    $('#export-project-link').on('click', function (e) {
        e.preventDefault();
        if (!selectedProjectValue) {
            showAlertMessage('Please select a project first.', AlertType.INFO);
            return;
        }
        downloadExcelReport(
            exportUrls.timePurchaseProject,
            { project: selectedProjectValue }
        );
    });

    $('#export-profit-centre-link').on('click', function (e) {
        e.preventDefault();
        if (!selectedProfitCentreValue) {
            showAlertMessage('Please select a profit centre first.', AlertType.INFO);   
            return;
        }
        downloadExcelReport(
            exportUrls.timeSaleProfitCentre,
            { profitCentre: selectedProfitCentreValue }
        );
    });

    $('#export-workgroup-link').on('click', function (e) {
        e.preventDefault();
        if (!selectedWorkGroupValue) {
            showAlertMessage('Please select a work group first.', AlertType.INFO);
            return;
        }
        downloadExcelReport(
            exportUrls.timeSaleWorkgroup,
            { workGroup: selectedWorkGroupValue }
        );
    });

    $('#test-sale-capabilities-button').on('click', function () {
        if (!selectedSellingWorkGroupValue) {
            showAlertMessage('Please select a Selling Workgroup first.', AlertType.INFO);
            return;
        }
        window.fpsNavigateTo('/PACT/BosworthInterface/ListTestCapability?workGroup=' + encodeURIComponent(selectedSellingWorkGroupValue));
    });

    $('#export-selling-workgroup-link').on('click', function (e) {
        e.preventDefault();
        if (selectedSellingWorkGroupValue) {
            downloadExcelReport(
                exportUrls.testSaleSellingWg,
                { workGroup: selectedSellingWorkGroupValue }
            );
        } else {
            showAlertMessage('Please select a Selling Workgroup first.', AlertType.INFO);
        }
    });

    $('#export-buying-project-link').on('click', function (e) {
        e.preventDefault();
        if (selectedBuyingProjectValue) {
            downloadExcelReport(
                exportUrls.testSaleBuyingProj,
                { parentProject: selectedBuyingProjectValue }
            );
        } else {
            showAlertMessage('Please select a Buying Project first.', AlertType.INFO);
        }
    });
});

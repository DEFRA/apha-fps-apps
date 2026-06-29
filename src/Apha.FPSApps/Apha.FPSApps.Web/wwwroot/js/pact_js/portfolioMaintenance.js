// Portfolio Maintenance page script
// Last updated: 2025-01-XX (Portfolio Time Codes button navigation)

var currentParentProject = '';
var currentTestCode = '';
var currentWorkGroup = '';
var currentPortfolio = '';
let portfolioSelectDropdown = null;
let selectedPortfolio = null;
function toggleSidebar() {
    document.querySelector('#shortnav').classList.toggle('collapsed');
}

// ── Portfolio dropdown init ───────────────────────────────────────────────
$(document).ready(function () {
    initializePortfolioMultiColumnDropdown();
    var $panel = $('#portfolioDropdownPanel');
    var $input = $('#dpselectportfolio');
    var $rows  = $('#portfolioDropdownBody tr');

    // Toggle open/close on input click
    $input.on('click', function (e) {
        e.stopPropagation();
        $panel.toggleClass('open');
    });

    // Row selection
    $rows.on('click', function () {
        var value = $(this).data('value');
        var parts = [];
        $(this).find('td').each(function () { parts.push($(this).text().trim()); });
        $input.val(parts.join(' - '));
        $panel.removeClass('open');
        $('#portfolioDropdownBody tr').show();
        $('#portfolioDropdownPanel .select-search-box').val('');
        loadPortfolioData(value);
    });

    // Search filter
    $panel.find('.select-search-box').on('input', function () {
        var term = $(this).val().toLowerCase();
        $rows.each(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(term) !== -1);
        });
    });

    // Clear search
    $panel.find('.clear-search-btn').on('click', function (e) {
        e.stopPropagation();
        $panel.find('.select-search-box').val('');
        $rows.show();
    });

    // Close on outside click
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.portfolio-picker-wrapper').length) {
            $panel.removeClass('open');
        }
    });

    // ── Check for portfolio parameter and auto-select ─────────────────────
    var urlParams = new URLSearchParams(window.location.search);
    var portfolioParam = urlParams.get('portfolio');

    if (portfolioParam) {
        // Use a slight delay to ensure all event handlers are fully bound
        setTimeout(function() {
            // Find the matching row in the dropdown
            var $matchingRow = $rows.filter(function() {
                return $(this).data('value') === portfolioParam;
            });

            if ($matchingRow.length > 0) {
                // Manually simulate the row click behavior
                var value = $matchingRow.data('value');
                var label = $matchingRow.data('label');

                // Update the input display value
                $input.val(label || value);

                // Close the panel
                $panel.removeClass('open');

                // Clear search
                $('#portfolioDropdownPanel .select-search-box').val('');
                $rows.show();

                // Load the portfolio data
                loadPortfolioData(value);
            }
        }, 100);
    }

    // ── Save portfolio form ───────────────────────────────────────────────────
    $('#btnSavePortfolio').on('click', function () {
        clearValidationErrors('#portfolioDetailForm');
        var payload = {
            parentProject: $('#hdnParentProject').val(),
            projectTitle: $('#txtProjectTitle').val(),
            finished: $('#chkFinished').is(':checked'),
            program: $('#dpProgramme').val(),
            projectManager: $('#dpManager').val(),
            budgetCvl: $('#txtBudgetCvl').val() || null,
            transferIncome: $('#txtTransferIncome').val() || null,
            comments: $('#txtComments').val()
        };

        $.ajax({
            url: '/PACT/PortfolioMaintenance/Edit',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload)
        }).done(function (res) {
            if (res.success) {
                clearValidationErrors('#portfolioDetailForm');
                alert(res.message || 'Saved successfully.');
            } else {
                displayServerValidationErrors(res.errors, res.message, '#portfolioDetailForm');
            }
        }).fail(function () { alert('An error occurred while saving.'); });
    });

    // ── Portfolio Time Codes button ───────────────────────────────────────────
    $('#btnPortfolioTimeCodes').on('click', function (e) {
        e.preventDefault(); // Prevent any default behavior
        e.stopPropagation(); // Stop event bubbling

        if (!currentParentProject) {
            alert('Please select a portfolio first.');
            return;
        }

        // Navigate to Portfolio Time Codes page with selected portfolio
        var url = '/PACT/PortfolioTimeCodes/Index?parentProject=' + encodeURIComponent(currentParentProject);
        window.location.href = url;
    });

    // ── Modal submit ──────────────────────────────────────────────────────────
    $(document).on('click', '#modalSubmitBtn', function () {
        var fn = $('#modaPopupBody').data('submitFn');
        if (fn && window[fn]) window[fn]();
    });

    // ── Re-select first row after constituent tests grid reloads (filter/sort) ─
    document.addEventListener('gridReloaded', function (e) {
        if (e.detail.gridId !== 'constituentTestGrid') return;

        var $firstRow = $('#tbl_constituentTestGrid tbody tr').first();
        var firstTestCode = ($firstRow.length && $firstRow.data('id')) ? String($firstRow.data('id')) : '';

        if (firstTestCode) {
            var previousTestCode = currentTestCode;
            currentTestCode = firstTestCode;
            currentPortfolio = currentParentProject;
            $firstRow.addClass('selected-row');
            $('#txtSelectedPortfolioTest').val(currentParentProject + ' - ' + currentTestCode);
            // Only reload the work groups grid when the selected test has changed
            if (currentTestCode !== previousTestCode) {
                loadTimeCodeGrid(currentParentProject, currentTestCode);
            }
        } else {
            currentTestCode = '';
            $('#txtSelectedPortfolioTest').val('');
            loadTimeCodeGrid(currentParentProject, '');
        }
    });
});

// ── Wrap a plain message string into the errors-array format ─────────────
function errMsg(msg) { return [{ field: '', message: msg }]; }

// ── Update a nav link href, preserving existing query params ─────────────
function updateNavHref(id, parentProject) {
    var current = $(id).attr('href') || '';
    var parts = current.split('?');
    var params = new URLSearchParams(parts[1] || '');
    params.set('parentProject', parentProject);
    $(id).attr('href', parts[0] + '?' + params.toString());
}

// ── Load all data for a portfolio ────────────────────────────────────────
function loadPortfolioData(parentProject) {
    currentParentProject = parentProject;
    currentTestCode = '';
    currentWorkGroup = '';
    currentPortfolio = '';
    $('#txtSelectedPortfolioTest').val('');
    clearValidationErrors('#portfolioDetailForm');
    resetFormButtons(false);

    $.get('/PACT/PortfolioMaintenance/GetPortfolio', { parentProject: parentProject })
        .done(function (res) {
            if (res.success && res.data) {
                var d = res.data;
                $('#hdnParentProject').val(d.parentProject || '');
                $('#txtParentProject').val(d.parentProject || '');
                $('#txtProjectTitle').val(d.projectTitle || '');
                $('#chkFinished').prop('checked', d.finished === -1 || d.finished === true);
                $('#dpProgramme').val(d.program || '');
                $('#dpManager').val(d.manager || '');
                $('#txtBudgetCvl').val(d.budgetCvl || '');
                $('#txtTransferIncome').val(d.transferIncome || '');
                $('#txtComments').val(d.comments || '');

                // Update sidebar nav links — preserves existing query params (e.g. year)
                updateNavHref('#sideNavTestPurchase', parentProject);
                updateNavHref('#sideNavTimeCodes', parentProject);
                updateNavHref('#sideNavInvoices', parentProject);

                resetFormButtons(true);
                loadConstituentTestGrid(parentProject);
            } else {
                showGovukAlert(res.message || 'Portfolio not found.');
            }
        })
        .fail(function () { alert('An error occurred while loading portfolio data.'); });
}

// ── Enable/disable buttons ───────────────────────────────────────────────
function resetFormButtons(enabled) {
    $('#btnSavePortfolio, #btnPortfolioTimeCodes')
        .prop('disabled', !enabled);
}

// ── Constituent Tests grid ───────────────────────────────────────────────
function loadConstituentTestGrid(parentProject, page, pageSize, sortBy, desc) {
    var payload = {
        pageNumber: page || 1,
        pageSize: pageSize || 10,
        sortBy: sortBy || '',
        descending: desc || false,
        filter: '{}'
    };

    $.ajax({
        url: '/PACT/PortfolioMaintenance/LoadConstituentTestGrid?parentProject=' + encodeURIComponent(parentProject),
        method: 'POST',
        data: payload
    }).done(function (html) {
        $('#gridContainer_constituentTestGrid').html(html);
        var $firstRow = $('#tbl_constituentTestGrid tbody tr').first();
        var firstTestCode = ($firstRow.length && $firstRow.data('id')) ? String($firstRow.data('id')) : '';
        if (firstTestCode) {
            currentTestCode = firstTestCode;
            currentPortfolio = currentParentProject;
            $firstRow.addClass('selected-row');
            $('#txtSelectedPortfolioTest').val(parentProject + ' - ' + currentTestCode);
            loadTimeCodeGrid(parentProject, currentTestCode);
        } else {
            currentTestCode = '';
            $('#txtSelectedPortfolioTest').val('');
            loadTimeCodeGrid(parentProject, '');
        }
    }).fail(function () { alert('An error occurred while loading constituent tests.'); });
}

function addConstituentTest() {
    if (!currentParentProject) return;
    $.get('/PACT/PortfolioMaintenance/CreateConstituentTest',
        { parentProject: currentParentProject },
        function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            $('#modaPopupBody').data('submitFn', 'saveConstituentTest');
        });
}

function saveConstituentTest() {
    clearValidationErrors('#formAddTest');
    var form = $('#formAddTest');
    var payload = {
        testCode: form.find('#txtmodal-testcode').val(),
        workGroup: form.find('#txtmodal-workgroup').val(),
        planPortfolio: currentParentProject
    };

    $.ajax({
        url: '/PACT/PortfolioMaintenance/CreateConstituentTest',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload)
    }).done(function (res) {
        if (res.success) {
            $('#modalPopup').removeClass('show');
            loadConstituentTestGrid(currentParentProject);
            alert(res.message || 'Test added.');
        } else {
            displayServerValidationErrors(res.errors, res.message, '#formAddTest');
        }
    }).fail(function () { alert('An error occurred while saving.'); });
}

function deleteConstituentTest(btn) {
    var testCode = $(btn).data('id');
    var workGroup = $(btn).closest('tr').find('[data-property="WorkGroup"]').text().trim() || currentWorkGroup;
    showGovukConfirm('Delete this constituent test?').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/PortfolioMaintenance/DeleteConstituentTest',
            method: 'DELETE',
            data: { testCode: testCode, workGroup: workGroup }
        }).done(function (res) {
            if (res.success) {
                loadConstituentTestGrid(currentParentProject);
                if (currentTestCode === testCode) {
                    currentTestCode = '';
                    $('#txtSelectedPortfolioTest').val('');
                    loadTimeCodeGrid(currentParentProject, '');
                }
                showGovukAlert(res.message || 'Deleted.');
            } else {
                showGovukAlert('Error: ' + (res.message || 'Delete failed.'));
            }
        }).fail(function () { showGovukAlert('An error occurred while deleting.'); });
    });
}

function selectConstituentTest(row) {
    var testCode = String($(row).data('id') || '');
    if (!testCode) return;
    currentTestCode = testCode;
    currentPortfolio = currentParentProject;
    $('#txtSelectedPortfolioTest').val(currentParentProject + ' - ' + testCode);
    loadTimeCodeGrid(currentParentProject, testCode);
}

// ── Extra filters supplied to the Work Groups grid on every reload ──────
function getTimeCodeGridExtraFilters() {
    return {
        parentProject: currentParentProject,
        testCode: currentTestCode
    };
}

// ── Time Codes / Work Groups grid ────────────────────────────────────────
function loadTimeCodeGrid(parentProject, testCode, page, pageSize) {
    var payload = {
        pageNumber: page || 1,
        pageSize: pageSize || 10,
        sortBy: '',
        descending: false,
        filter: '{}'
    };
    var url = '/PACT/PortfolioMaintenance/LoadTimeCodeGrid';
    if (parentProject) {
        url += '?parentProject=' + encodeURIComponent(parentProject);
        if (testCode) url += '&testCode=' + encodeURIComponent(testCode);
    }

    $.ajax({
        url: url,
        method: 'POST',
        data: payload
    }).done(function (html) {
        $('#gridContainer_portfolioTimeCodeGrid').html(html);
    }).fail(function () { alert('An error occurred while loading work groups.'); });
}

function addPortfolioTimeCode() {
    if (!currentParentProject) return;
    $.get('/PACT/PortfolioMaintenance/CreatePortfolioTimeCode',
        { parentProject: currentParentProject, selectedTestCode: currentTestCode, selectedPortfolio: currentPortfolio },
        function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            $('#modaPopupBody').data('submitFn', 'savePortfolioTimeCode');
        });
}

function savePortfolioTimeCode() {
    clearValidationErrors('#timeCodeForm');
    var form = $('#timeCodeForm');
    var payload = {
        workGroup: form.find('[name=WorkGroup]').val(),
        timeCode: form.find('[name=TimeCode]').val(),
        parentProject: form.find('[name=ParentProject]').val(),
        portfolio: form.find('[name=Portfolio]').val(),
        testCode: currentTestCode,
        active: form.find('[name=Active]').is(':checked')
    };

    $.ajax({
        url: '/PACT/PortfolioMaintenance/CreatePortfolioTimeCode',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload)
    }).done(function (res) {
        if (res.success) {
            $('#modalPopup').removeClass('show');
            loadTimeCodeGrid(currentParentProject, currentTestCode);
            alert(res.message || 'Work group added.');
        } else {
            displayServerValidationErrors(res.errors, res.message, '#timeCodeForm');
        }
    }).fail(function () { alert('An error occurred while saving.'); });
}

function editPortfolioTimeCode(btn) {
    var timeCode = $(btn).data('id');
    var workGroup = $(btn).closest('tr').find('[data-property="WorkGroup"]').text().trim() || '';
    $.get('/PACT/PortfolioMaintenance/EditPortfolioTimeCode',
        { workGroup: workGroup, timeCode: timeCode, parentProject: currentParentProject, selectedTestCode: currentTestCode },
        function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            $('#modaPopupBody').data('submitFn', 'updatePortfolioTimeCode');
        });
}

function updatePortfolioTimeCode() {
    clearValidationErrors('#timeCodeForm');
    var form = $('#timeCodeForm');
    var workGroup = form.data('workgroup');
    var payload = {
        workGroup: workGroup,
        timeCode: form.find('[name=TimeCode]').val(),
        parentProject: currentParentProject,
        testCode: form.find('[name=TestCode]').val(),
        portfolio: form.find('[name=Portfolio]').val(),
        active: form.find('[name=Active]').is(':checked')
    };

    $.ajax({
        url: '/PACT/PortfolioMaintenance/EditPortfolioTimeCode',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload)
    }).done(function (res) {
        if (res.success) {
            $('#modalPopup').removeClass('show');
            loadTimeCodeGrid(currentParentProject, currentTestCode);
            alert(res.message || 'Work group updated.');
        } else {
            displayServerValidationErrors(res.errors, res.message, '#timeCodeForm');
        }
    }).fail(function () { alert('An error occurred while saving.'); });
}

function deletePortfolioTimeCode(btn) {
    var timeCode = $(btn).data('id');
    var workGroup = $(btn).closest('tr').find('[data-property="WorkGroup"]').text().trim() || '';
    showGovukConfirm('Delete this work group entry?').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/PortfolioMaintenance/DeletePortfolioTimeCode',
            method: 'DELETE',
            data: { workGroup: workGroup, timeCode: timeCode, parentProject: currentParentProject }
        }).done(function (res) {
            if (res.success) {
                loadTimeCodeGrid(currentParentProject, currentTestCode);
                showGovukAlert(res.message || 'Deleted.');
            } else {
                showGovukAlert('Error: ' + (res.message || 'Delete failed.'));
            }
        }).fail(function () { showGovukAlert('An error occurred while deleting.'); });
    });
}

function initializePortfolioMultiColumnDropdown() {
    /*Multicolumn dropdown functionality for program selection*/
    portfolioSelectDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'portfolioSelectDropdown',
        containerSelector: '#portfolioSelectMultiDropdown',//name as per cshtml div id
        placeholder: 'Select a Portfolio',
        showSerialNumber: false,
        searchPlaceholder: 'Search by code or title',
        labelText: '',//this label will come at the top of dropdown, can be set as per requirement
        columns: [
            { field: 'Value', header: 'Code', width: '80px' },
            { field: 'Text', header: 'Portfolio Title', width: '150px' },
        ],
        data: portfolioOptionsListData,
        displayField: 'Text',
        valueField: 'Value',
        clearButtonClearsSelection: false,//this will only clear searchbox and not selected value
        callbacks: {
            onSelect: function (selectedItem, dropdown) {
                selectedPortfolio = selectedItem.Value;
                loadPortfolioData(selectedPortfolio); 
            },
             

        }
    });
   
}
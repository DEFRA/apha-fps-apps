// DepartmentIncome Page JavaScript
// departmentIncomePageMonths, departmentIncomePagePeriods, and deptIncomeLoadGridUrl
// are declared as inline data variables in Index.cshtml before this file is loaded.

// ── Period From / To MultiColumnDropdownComponent initialisation ──────────

function initializeDepartmentIncomePeriodDropdowns(config) {
    var monthsData = config.monthsData || [];

    setTimeout(function () {
        var periodFromDropdown = new MultiColumnDropdownComponent({
            dropdownId:                 'periodFrom',
            containerSelector:          '#periodFromDropdown',
            placeholder:                '--select--',
            searchPlaceholder:          'Type to search',
            showSerialNumber:           false,
            enableSearch:               false,
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
        if (initialFrom) { periodFromDropdown.setValue(initialFrom); }

        var periodToDropdown = new MultiColumnDropdownComponent({
            dropdownId:                 'periodTo',
            containerSelector:          '#periodToDropdown',
            placeholder:                '--select--',
            searchPlaceholder:          'Type to search',
            showSerialNumber:           false,
            enableSearch:               false,
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
        if (initialTo) { periodToDropdown.setValue(initialTo); }

    }, 100);
}

// ── Snapshot query map ────────────────────────────────────────────────────

var deptIncomeSnapshotQueryMap = {
    'time':        'qryDeptIncomeTime',
    'tests':       'qryDeptIncomeTest',
    'animals':     'qryDeptIncomeAnimal',
    'exceptional': 'qryDeptIncomeExceptional',
    'totals':      'qryDeptIncomeTotals'
};

// ── Snapshot Period modal ─────────────────────────────────────────────────

function editDeptIncomeSnapshotPeriod(btn) {
    var periodName = $(btn).data('id');
    $.get('/FPS/DepartmentIncome/EditSnapshotPeriod', { periodName: periodName }, function (html) {
        $('#modaPopupBodyDeptIncome').html(html);
        $('#modalPopupDeptIncome').addClass('show');
    });
}

function saveDeptIncomeSnapshotPeriod() {
    var periodName   = $('#hdnSnapshotPeriodName').val();
    var periodLocked = $('#chkPeriodLocked').is(':checked');

    $.ajax({
        url:         '/FPS/DepartmentIncome/UpdateSnapshotPeriod',
        type:        'POST',
        data:        JSON.stringify({ periodName: periodName, periodLocked: periodLocked }),
        contentType: 'application/json',
        success: function (response) {
            if (response.success) {
                closeDeptIncomeSnapshotModal();
                showAlertMessage(response.message || 'Period locked updated successfully.', AlertType.SUCCESS);
                var gm = window['gridManager_departmentIncomeSnapshotGrid'];
                if (gm) { gm.reloadGrid({ page: 1 }); }
            } else {
                showAlertMessage(response.message || 'Update failed.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred while saving.', AlertType.ERROR);
        }
    });
}

function closeDeptIncomeSnapshotModal() {
    $('#modaPopupBodyDeptIncome').html('');
    $('#modalPopupDeptIncome').removeClass('show');
}

// ── Snapshot grid helpers ─────────────────────────────────────────────────

function getDepartmentIncomeSnapshotExtraFilters() {
    return {
        project:   document.getElementById('projectSelect')?.value || '',
        monthFrom: document.getElementById('monthFromSelect')?.value || '',
        monthTo:   document.getElementById('monthToSelect')?.value || ''
    };
}

function reloadDepartmentIncomeSnapshotGrid() {
    var gm = window['gridManager_departmentIncomeSnapshotGrid'];
    if (gm) { gm.reloadGrid({ page: 1 }); }
}

function showSnapshotGrid() {
    var snapshotGridArea        = document.getElementById('snapshotGridArea');
    var snapshotQueryResultsArea = document.getElementById('snapshotQueryResultsArea');
    if (snapshotGridArea)        { snapshotGridArea.style.display = ''; }
    if (snapshotQueryResultsArea) { snapshotQueryResultsArea.style.display = 'none'; }
}

// ── Extra-filter callbacks ────────────────────────────────────────────────

function getDeptIncomeSnapshotQueryExtraFilters() {
    var select   = document.getElementById('snapshotQuerySelect');
    var shortKey = (select && select.selectedOptions.length > 0)
        ? select.selectedOptions[0].value
        : 'time';
    return {
        source:    'snapshot',
        queryType: deptIncomeSnapshotQueryMap[shortKey] || 'qryDeptIncomeTime',
        project:   getDeptIncomeProject(),
        monthFrom: getDeptIncomeMonthFromValue(),
        monthTo:   getDeptIncomeMonthToValue()
    };
}

function getDeptIncomeCurrentExtraFilters() {
    var select    = document.getElementById('querySelect');
    var queryType = (select && select.selectedIndex >= 0)
        ? select.options[select.selectedIndex].value
        : 'qryDeptIncomeTime';
    return {
        queryType: queryType,
        project:   getDeptIncomeProject(),
        monthFrom: getDeptIncomeMonthFromValue(),
        monthTo:   getDeptIncomeMonthToValue()
    };
}

// ── Utility: read filter values ───────────────────────────────────────────

function getDeptIncomeProject() {
    var el = document.getElementById('projectSelect');
    return el ? (el.value || '') : '';
}

function getDeptIncomeMonthFromValue() {
    var el = document.getElementById('monthFromSelect');
    if (!el || !el.value) { return null; }
    var n = parseInt(el.value, 10);
    return Number.isNaN(n) ? null : n;
}

function getDeptIncomeMonthToValue() {
    var el = document.getElementById('monthToSelect');
    if (!el || !el.value) { return null; }
    var n = parseInt(el.value, 10);
    return Number.isNaN(n) ? null : n;
}

// ── Grid loaders ──────────────────────────────────────────────────────────

function loadCurrentGrid(queryType) {
    var container   = document.getElementById('gridContainer_departmentIncomeCurrentGrid');
    var resultsArea = document.getElementById('currentQueryResultsArea');
    if (container)   { container.innerHTML = '<p class="govuk-body-s">Loading...</p>'; }
    if (resultsArea) { resultsArea.style.display = ''; }

    var params = {
        page:      1,
        pageSize:  20,
        queryType: queryType,
        source:    'current',
        project:   getDeptIncomeProject(),
        monthFrom: getDeptIncomeMonthFromValue(),
        monthTo:   getDeptIncomeMonthToValue()
    };

    $.post(deptIncomeLoadGridUrl, params)
        .done(function (html) {
            if (container) { $(container).html(html); }
        })
        .fail(function () {
            showDeptIncomePopupMessage('Error', 'An error occurred while loading query data.');
            if (container)   { $(container).html(''); }
            if (resultsArea) { resultsArea.style.display = 'none'; }
        });
}

function loadSnapshotQueryGrid(queryType) {
    var container            = document.getElementById('gridContainer_departmentIncomeSnapshotQueryGrid');
    var snapshotGridArea     = document.getElementById('snapshotGridArea');
    var snapshotQueryResultsArea = document.getElementById('snapshotQueryResultsArea');

    if (container)            { container.innerHTML = '<p class="govuk-body-s">Loading...</p>'; }
    if (snapshotGridArea)     { snapshotGridArea.style.display = 'none'; }
    if (snapshotQueryResultsArea) { snapshotQueryResultsArea.style.display = ''; }

    var params = {
        page:      1,
        pageSize:  20,
        queryType: queryType,
        source:    'snapshot',
        project:   getDeptIncomeProject(),
        monthFrom: getDeptIncomeMonthFromValue(),
        monthTo:   getDeptIncomeMonthToValue()
    };

    $.post(deptIncomeLoadGridUrl, params)
        .done(function (html) {
            if (container) { $(container).html(html); }
        })
        .fail(function () {
            showDeptIncomePopupMessage('Error', 'An error occurred while loading query data.');
            if (container) { $(container).html(''); }
            showSnapshotGrid();
        });
}

// ── Run Query button handlers ─────────────────────────────────────────────

function openSnapshotQueryModal() {
    var monthFrom = document.getElementById('monthFromSelect')?.value;
    var monthTo   = document.getElementById('monthToSelect')?.value;
    if (!monthFrom || !monthTo) {
        showAlertMessage('Please select Period From and Period To, and run the query again.', AlertType.WARNING);
        return;
    }
    var select   = document.getElementById('snapshotQuerySelect');
    var shortKey = (select && select.selectedOptions.length > 0)
        ? select.selectedOptions[0].value
        : 'time';
    loadSnapshotQueryGrid(deptIncomeSnapshotQueryMap[shortKey] || 'qryDeptIncomeTime');
}

function openCurrentQueryModal() {
    var monthFrom = document.getElementById('monthFromSelect')?.value;
    var monthTo   = document.getElementById('monthToSelect')?.value;
    if (!monthFrom || !monthTo) {
        showAlertMessage('Please select Period From and Period To, and run the query again.', AlertType.WARNING);
        return;
    }
    var select    = document.getElementById('querySelect');
    var queryType = (select && select.selectedIndex >= 0)
        ? select.options[select.selectedIndex].value
        : 'qryDeptIncomeTime';
    loadCurrentGrid(queryType);
}

// ── Popup message helpers ─────────────────────────────────────────────────

function closeDeptIncomePopupMessage() {
    var overlay = document.getElementById('govuk-popup-overlay');
    var popup   = document.getElementById('govuk-popup');
    if (overlay) { overlay.classList.remove('active'); }
    if (popup)   { popup.classList.remove('active'); popup.innerHTML = ''; }
}

function showDeptIncomePopupMessage(title, message) {
    var overlay = document.getElementById('govuk-popup-overlay');
    var popup   = document.getElementById('govuk-popup');
    if (!overlay || !popup) { window.alert(message); return; }

    popup.innerHTML =
        '<div class="govuk-notification-banner" role="alert">' +
        '  <div class="govuk-notification-banner__header">' +
        '    <h2 class="govuk-notification-banner__title">' + title + '</h2>' +
        '  </div>' +
        '  <div class="govuk-notification-banner__content">' +
        '    <h3 class="govuk-notification-banner__heading">' + message + '</h3>' +
        '    <div class="govuk-button-group">' +
        '      <button type="button" class="govuk-button govuk-button--secondary" id="departmentIncomePopupCloseBtn">Close</button>' +
        '    </div>' +
        '  </div>' +
        '</div>';

    overlay.classList.add('active');
    popup.classList.add('active');

    var closeBtn = document.getElementById('departmentIncomePopupCloseBtn');
    if (closeBtn) { closeBtn.addEventListener('click', closeDeptIncomePopupMessage); }
}

// ── Filter Enter-key support ──────────────────────────────────────────────
// Attach a delegated keydown handler to a static grid container so that
// pressing Enter in any .grid-filter text box triggers the same reload as
// blur/change — without modifying the shared _DataGrid.cshtml partial.

function attachDeptIncomeFilterEnterKey(containerId, gridManagerKey) {
    $(document).off('keydown.deptIncomeFilter_' + containerId)
               .on('keydown.deptIncomeFilter_' + containerId,
                   '#' + containerId + ' .grid-filter',
                   function (e) {
                       if (e.key === 'Enter') {
                           e.preventDefault();
                           var gm = window[gridManagerKey];
                           if (gm) { gm.reloadGrid({ page: 1 }); }
                       }
                   });
}

// ── DOMContentLoaded: wire events and initialise period dropdowns ─────────

function deptIncomeInit() {

    initializeDepartmentIncomePeriodDropdowns({ monthsData: departmentIncomePageMonths });

    // Attach Enter-key filter handler for each Department Income grid
    attachDeptIncomeFilterEnterKey('gridContainer_departmentIncomeSnapshotGrid',      'gridManager_departmentIncomeSnapshotGrid');
    attachDeptIncomeFilterEnterKey('gridContainer_departmentIncomeSnapshotQueryGrid', 'gridManager_departmentIncomeSnapshotQueryGrid');
    attachDeptIncomeFilterEnterKey('gridContainer_departmentIncomeCurrentGrid',       'gridManager_departmentIncomeCurrentGrid');

    reloadDepartmentIncomeSnapshotGrid();

    var projectSelect   = document.getElementById('projectSelect');
    var monthFromSelect = document.getElementById('monthFromSelect');
    var monthToSelect   = document.getElementById('monthToSelect');

    if (projectSelect)   { projectSelect.addEventListener('change',  reloadDepartmentIncomeSnapshotGrid); }
    if (monthFromSelect) { monthFromSelect.addEventListener('change', reloadDepartmentIncomeSnapshotGrid); }
    if (monthToSelect)   { monthToSelect.addEventListener('change',   reloadDepartmentIncomeSnapshotGrid); }

    var runSnapshot     = document.getElementById('runQueryBtnSnapshot');
    var runCurrent      = document.getElementById('runQueryBtnCurrent');
    var backToSnapshot  = document.getElementById('backToSnapshotBtn');
    var closeCurrentBtn = document.getElementById('closeCurrentGridBtn');

    if (closeCurrentBtn) {
        closeCurrentBtn.addEventListener('click', function () {
            var resultsArea = document.getElementById('currentQueryResultsArea');
            var container   = document.getElementById('gridContainer_departmentIncomeCurrentGrid');
            if (container)   { container.innerHTML = ''; }
            if (resultsArea) { resultsArea.style.display = 'none'; }
            var snapshotTab = document.querySelector('#department-income-tabs .govuk-tabs__tab[href="#department-income-snapshot"]');
            if (snapshotTab) { snapshotTab.click(); }
        });
    }

    if (backToSnapshot) {
        backToSnapshot.addEventListener('click', function () { showSnapshotGrid(); });
    }

    if (runSnapshot) {
        runSnapshot.addEventListener('click', function () { openSnapshotQueryModal(); });
    }

    if (runCurrent) {
        runCurrent.addEventListener('click', function () { openCurrentQueryModal(); });
    }

    var popupOverlay = document.getElementById('govuk-popup-overlay');
    if (popupOverlay) {
        popupOverlay.addEventListener('click', function (evt) {
            if (evt.target && evt.target.id === 'govuk-popup-overlay') {
                closeDeptIncomePopupMessage();
            }
        });
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', deptIncomeInit);
} else {
    deptIncomeInit();
}

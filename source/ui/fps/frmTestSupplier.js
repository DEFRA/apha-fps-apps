'use strict';

/* ─────────────────────────────────────────────────────────────────────────────
   frm_test_supplier.js
   Form-specific logic for fps/frm_test_supplier.html
   (Test Suppliers View — Who is Buying TestX?)
   ─────────────────────────────────────────────────────────────────────────────
   Reusable helpers used from js/common.js:
     escapeHtml, getPerPage, renderPagination, openModal, closeModal
   ───────────────────────────────────────────────────────────────────────────── */

/* ── Sample data — rows from the Access form screenshot ── */
var allRecords = [
    { id: 1, project: 'CSUT1306',  projectManager: 'Plumbley, Glendon', noTests: 0, testPrice: 776.00, testCost: 0.00, projectStatus: 'Approved' },
    { id: 2, project: 'QAPTPORT1', projectManager: 'Plumbley, Glendon', noTests: 0, testPrice: 776.00, testCost: 0.00, projectStatus: 'Approved' }
];
var filteredRecords = allRecords.slice();
var currentPage     = 1;

/* ── Editing state ── */
var editingTblTestUsageId = null;

/* ── Named page-navigation callback (no arguments.callee) ── */
function onPageClick(page) {
    currentPage = page;
    renderTable();
    renderPagination(filteredRecords, currentPage, getPerPage(), 'pagination', onPageClick);
}

/* ─────────────────────────────────────────────────────────────────────────────
   renderTable — builds tbody rows for the Test Usage grid.
   ───────────────────────────────────────────────────────────────────────────── */
function renderTable() {
    var tbody   = document.getElementById('tblTestUsageBody');
    var perPage = getPerPage();
    var start   = (currentPage - 1) * perPage;
    var end     = start + perPage;
    var pageRecords = filteredRecords.slice(start, end);

    if (pageRecords.length === 0) {
        tbody.innerHTML =
            '<tr class="govuk-table__row">' +
            '<td class="govuk-table__cell" colspan="7" style="text-align:center;">No records found.</td>' +
            '</tr>';
        updateTotals([]);
        return;
    }

    var rows = '';
    for (var i = 0; i < pageRecords.length; i++) {
        var item = pageRecords[i];
        rows +=
            '<tr class="govuk-table__row">' +
            '<td class="govuk-table__cell">' + escapeHtml(item.project) + '</td>' +
            '<td class="govuk-table__cell">' + escapeHtml(item.projectManager) + '</td>' +
            '<td class="govuk-table__cell" style="text-align:right;">' + escapeHtml(String(item.noTests)) + '</td>' +
            '<td class="govuk-table__cell" style="text-align:right;">&pound;' + item.testPrice.toFixed(2) + '</td>' +
            '<td class="govuk-table__cell" style="text-align:right;">&pound;' + item.testCost.toFixed(2) + '</td>' +
            '<td class="govuk-table__cell">' + escapeHtml(item.projectStatus) + '</td>' +
            '<td class="govuk-table__cell" style="text-align:center;">' +
                '<button onclick=\'openTblTestUsageEditModal(' + JSON.stringify(item) + ')\'' +
                    ' aria-label="Edit test usage for project ' + escapeHtml(item.project) + '"' +
                    ' style="min-width:24px;min-height:24px;display:inline-flex;align-items:center;justify-content:center;">' +
                    '<img src="../images/pen-to-square-regular-full.svg"' +
                         ' alt="Edit icon for project ' + escapeHtml(item.project) + '" width="20">' +
                '</button>' +
                ' ' +
                '<button onclick="handleTblTestUsageDelete(' + item.id + ')"' +
                    ' aria-label="Delete test usage for project ' + escapeHtml(item.project) + '"' +
                    ' style="min-width:24px;min-height:24px;display:inline-flex;align-items:center;justify-content:center;">' +
                    '<img src="../images/trash-can-regular-full.svg"' +
                         ' alt="Delete icon for project ' + escapeHtml(item.project) + '" width="20">' +
                '</button>' +
            '</td>' +
            '</tr>';
    }
    tbody.innerHTML = rows;

    updateTotals(filteredRecords);
}

/* ─────────────────────────────────────────────────────────────────────────────
   updateTotals — recalculates and displays the grid totals row.
   ───────────────────────────────────────────────────────────────────────────── */
function updateTotals(records) {
    var totalNoTests  = 0;
    var totalTestCost = 0;
    for (var i = 0; i < records.length; i++) {
        totalNoTests  += (records[i].noTests  || 0);
        totalTestCost += (records[i].testCost || 0);
    }
    var elNoTests  = document.getElementById('totalNoTests');
    var elTestCost = document.getElementById('totalTestCost');
    if (elNoTests)  { elNoTests.value  = String(totalNoTests); }
    if (elTestCost) { elTestCost.value = '\u00a3' + totalTestCost.toFixed(2); }
}

/* ─────────────────────────────────────────────────────────────────────────────
   applyFilters — filters allRecords by the checkbox state.
   (Test selection filter: in a real system this would load different data;
    for the prototype it is a no-op on the static dataset.)
   ───────────────────────────────────────────────────────────────────────────── */
function applyFilters() {
    var showRejected = document.getElementById('chkShowRejected').checked;
    filteredRecords = allRecords.filter(function (r) {
        return showRejected || r.projectStatus !== 'Rejected';
    });
    currentPage = 1;
    renderTable();
    renderPagination(filteredRecords, currentPage, getPerPage(), 'pagination', onPageClick);
}

/* ─────────────────────────────────────────────────────────────────────────────
   Modal functions — open, close, delete, save/update.
   ───────────────────────────────────────────────────────────────────────────── */
function openTblTestUsageAddModal() {
    editingTblTestUsageId = null;
    document.getElementById('tblTestUsageModalLabel').textContent = 'Add Test Usage';
    document.getElementById('formTblTestUsage').reset();
    document.getElementById('tblTestUsageSaveBtn').style.display   = '';
    document.getElementById('tblTestUsageUpdateBtn').style.display = 'none';
    openModal('tblTestUsageModal');
}

function openTblTestUsageEditModal(item) {
    editingTblTestUsageId = item.id;
    document.getElementById('tblTestUsageModalLabel').textContent = 'Edit Test Usage';
    document.getElementById('modal-tu-project').value        = item.project        || '';
    document.getElementById('modal-tu-projectManager').value = item.projectManager || '';
    document.getElementById('modal-tu-noTests').value        = item.noTests !== undefined  ? String(item.noTests)         : '';
    document.getElementById('modal-tu-testPrice').value      = item.testPrice !== undefined ? item.testPrice.toFixed(2)   : '';
    document.getElementById('modal-tu-testCost').value       = item.testCost  !== undefined ? item.testCost.toFixed(2)    : '';
    document.getElementById('modal-tu-projectStatus').value  = item.projectStatus || '';
    document.getElementById('tblTestUsageSaveBtn').style.display   = 'none';
    document.getElementById('tblTestUsageUpdateBtn').style.display = '';
    openModal('tblTestUsageModal');
}

function closeTblTestUsageModal() {
    closeModal('tblTestUsageModal');
}

function handleTblTestUsageDelete(id) {
    var kept = [];
    for (var i = 0; i < allRecords.length; i++) {
        if (allRecords[i].id !== id) { kept.push(allRecords[i]); }
    }
    allRecords      = kept;
    filteredRecords = allRecords.slice();
    renderTable();
    renderPagination(filteredRecords, currentPage, getPerPage(), 'pagination', onPageClick);
}

function saveTblTestUsage() {
    var projectVal        = document.getElementById('modal-tu-project').value.trim();
    var projectManagerVal = document.getElementById('modal-tu-projectManager').value.trim();
    var noTestsVal        = parseInt(document.getElementById('modal-tu-noTests').value, 10) || 0;
    var testPriceVal      = parseFloat(document.getElementById('modal-tu-testPrice').value) || 0;
    var testCostVal       = parseFloat(document.getElementById('modal-tu-testCost').value)  || 0;
    var projectStatusVal  = document.getElementById('modal-tu-projectStatus').value;

    if (editingTblTestUsageId !== null) {
        /* Edit branch */
        for (var i = 0; i < allRecords.length; i++) {
            if (allRecords[i].id === editingTblTestUsageId) {
                allRecords[i].project        = projectVal;
                allRecords[i].projectManager = projectManagerVal;
                allRecords[i].noTests        = noTestsVal;
                allRecords[i].testPrice      = testPriceVal;
                allRecords[i].testCost       = testCostVal;
                allRecords[i].projectStatus  = projectStatusVal;
                break;
            }
        }
    } else {
        /* Add branch */
        var newId = allRecords.length > 0
            ? allRecords[allRecords.length - 1].id + 1
            : 1;
        allRecords.push({
            id:             newId,
            project:        projectVal,
            projectManager: projectManagerVal,
            noTests:        noTestsVal,
            testPrice:      testPriceVal,
            testCost:       testCostVal,
            projectStatus:  projectStatusVal
        });
    }

    filteredRecords = allRecords.slice();
    renderTable();
    renderPagination(filteredRecords, currentPage, getPerPage(), 'pagination', onPageClick);
    closeTblTestUsageModal();
}

/* ─────────────────────────────────────────────────────────────────────────────
   initTable — initial render with full dataset.
   ───────────────────────────────────────────────────────────────────────────── */
function initTable(records) {
    allRecords      = records;
    filteredRecords = allRecords.slice();
    currentPage     = 1;
    renderTable();
    renderPagination(filteredRecords, currentPage, getPerPage(), 'pagination', onPageClick);
}

/* ─────────────────────────────────────────────────────────────────────────────
   DOMContentLoaded — wire events then initialise table.
   ───────────────────────────────────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('btnViewTest').addEventListener('click', applyFilters);

    document.getElementById('chkShowRejected').addEventListener('change', applyFilters);

    document.getElementById('recordsPerPage').addEventListener('change', function () {
        currentPage = 1;
        renderTable();
        renderPagination(filteredRecords, currentPage, getPerPage(), 'pagination', onPageClick);
    });

    document.getElementById('btnTblTestUsageAdd').addEventListener('click', openTblTestUsageAddModal);

    initTable(allRecords);
});

// Yearly Details Page JavaScript
// Depends on globals declared in Index.cshtml:
//   projectId, selectedYear, programme, isDefra, yearlyDetailsUrls

// ── Tab grid endpoint / id maps ────────────────────────────────────
var tabGridEndpoints = {
    'Staff-tab': yearlyDetailsUrls.loadStaffGrid,
    'Tests-tab': yearlyDetailsUrls.loadTestGrid,
    'Animals-tab': yearlyDetailsUrls.loadAnimalGrid,
    'AdditionalCosts-tab': yearlyDetailsUrls.loadAdditionalCostGrid,
    'MarkupAndProfit-tab': yearlyDetailsUrls.loadMarkupAndProfitGrid
};

var tabGridIds = {
    'Staff-tab': 'staffGrid',
    'Tests-tab': 'testGrid',
    'Animals-tab': 'animalGrid',
    'AdditionalCosts-tab': 'additionalCostGrid',
    'MarkupAndProfit-tab': 'markupAndProfitGrid'
};

// ── Initialize on page load ────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        if (typeof initializeTableFeatures === 'function') {
            initializeTableFeatures();
            document.querySelectorAll('.sort-indicator').forEach(function (el) { el.remove(); });
        }
    }, 200);

    removeFilterRows(document);

    document.querySelectorAll('.sup_pagination_footer.sup_p_0').forEach(function (el) {
        el.hidden = true;
    });

    attachButtonEventListeners();
});

// ── Utility ────────────────────────────────────────────────────────
function removeFilterRows(container) {
    container.querySelectorAll('tr.filter-row').forEach(function (row) { row.remove(); });
}

function getAntiForgeryToken() {
    var el = document.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : '';
}

function attachButtonEventListeners() {
    var addYearBtn = document.getElementById('add_year');
    if (addYearBtn) addYearBtn.addEventListener('click', function () { addProjectYear(projectId); });

    var openCopyDataBtn = document.getElementById('openCopyDataModal');
    if (openCopyDataBtn) openCopyDataBtn.addEventListener('click', function () { openCopyYearDataModal(); });

    var addStaffBtn = document.getElementById('addstaffbookedBtn');
    if (addStaffBtn) addStaffBtn.addEventListener('click', function () { openAddStaffModal(projectId, selectedYear); });

    var addTestBtn = document.getElementById('addtestbookedBtn');
    if (addTestBtn) addTestBtn.addEventListener('click', function () { openAddTestModal(projectId, selectedYear); });

    var addAnimalBtn = document.getElementById('addanimalbookedBtn');
    if (addAnimalBtn) addAnimalBtn.addEventListener('click', function () { openAddAnimalModal(projectId, selectedYear); });

    var addAdditionalBtn = document.getElementById('addadditionalBtn');
    if (addAdditionalBtn) addAdditionalBtn.addEventListener('click', function () { openAddAdditionalCostModal(projectId, selectedYear); });
}

// ── Modal helpers ──────────────────────────────────────────────────
var _lastFocusedElementBeforeModal = null;

function getModalFocusableElements(modalElement) {
    if (!modalElement) return [];

    return Array.from(modalElement.querySelectorAll(
        'a[href], area[href], input:not([disabled]):not([type="hidden"]), select:not([disabled]), ' +
        'textarea:not([disabled]), button:not([disabled]), iframe, object, embed, [contenteditable], ' +
        '[tabindex]:not([tabindex="-1"])'))
        .filter(function (element) {
            return element.offsetParent !== null || getComputedStyle(element).position === 'fixed';
        });
}

function trapModalFocus(event) {
    var modal = document.getElementById('project1ModalContainer');
    if (!modal || modal.classList.contains('project-modal-hidden')) return;

    if (event.key === 'Escape') {
        event.preventDefault();
        closeModal();
        return;
    }

    if (event.key !== 'Tab') return;

    var focusableElements = getModalFocusableElements(modal);
    if (focusableElements.length === 0) {
        event.preventDefault();
        modal.focus();
        return;
    }

    var firstElement = focusableElements[0];
    var lastElement = focusableElements[focusableElements.length - 1];
    var activeElement = document.activeElement;

    if (event.shiftKey) {
        if (activeElement === modal || activeElement === firstElement || !modal.contains(activeElement)) {
            event.preventDefault();
            lastElement.focus();
        }
    } else {
        if (activeElement === modal || !modal.contains(activeElement)) {
            event.preventDefault();
            firstElement.focus();
        } else if (activeElement === lastElement) {
            event.preventDefault();
            firstElement.focus();
        }
    }
}

function openModal() {
    var el = document.getElementById('project1ModalContainer');
    _lastFocusedElementBeforeModal = document.activeElement;
    el.classList.remove('project-modal-hidden');
    el.classList.add('show');
    el.style.display = 'block';
    el.setAttribute('aria-hidden', 'false');
    document.body.classList.add('modal-open');

    document.addEventListener('keydown', trapModalFocus);

    setTimeout(function () {
        el.focus();
    }, 0);
}
function closeModal() {
    var el = document.getElementById('project1ModalContainer');
    el.classList.remove('show');
    el.style.display = '';
    el.classList.add('project-modal-hidden');
    el.setAttribute('aria-hidden', 'true');
    document.body.classList.remove('modal-open');
    document.removeEventListener('keydown', trapModalFocus);

    if (_lastFocusedElementBeforeModal && typeof _lastFocusedElementBeforeModal.focus === 'function') {
        _lastFocusedElementBeforeModal.focus();
    }
}

// ── Year navigation ────────────────────────────────────────────────
function selectYear(pid, year) {
    window.location.href = yearlyDetailsUrls.index + '?projectId=' + encodeURIComponent(pid) + '&selectedYear=' + year;
}

function deletetblProjectYear(btn) {
    var year = parseInt(btn.getAttribute('data-year'), 10);
    showGovukConfirm('Delete year ' + year + ' and all its data?').then(function (result) {
        if (!result) return;
        fetch(yearlyDetailsUrls.deleteProjectYear + '?projectId=' + encodeURIComponent(projectId) + '&year=' + year, {
            method: 'DELETE',
            headers: { 'RequestVerificationToken': getAntiForgeryToken() }
        })
            .then(function (r) { return r.json(); })
            .then(function (d) {
                if (d.success) {
                    var row = btn.closest('.project-year-row');
                    if (row) row.remove();
                    if (year === selectedYear) {
                        var firstRow = document.querySelector('.project-year-row');
                        if (firstRow) {
                            selectYear(projectId, parseInt(firstRow.getAttribute('data-year'), 10));
                        } else {
                            window.location.href = yearlyDetailsUrls.index + '?projectId=' + encodeURIComponent(projectId);
                        }
                    }
                } else {
                    showAlertMessage(d.message || 'Failed to delete project year.', AlertType.ERROR);
                }
            })
            .catch(function (err) { console.error('Delete year error:', err); showAlertMessage('Failed to delete project year.', AlertType.ERROR); });
    });
}

function addProjectYear(pid) {
    var years = Array.from(document.querySelectorAll('.project-year-row')).map(function (r) { return parseInt(r.getAttribute('data-year'), 10); });
    var nextYear = years.length > 0 ? Math.max.apply(null, years) + 1 : new Date().getFullYear();
    fetch(yearlyDetailsUrls.addProjectYear + '?projectId=' + encodeURIComponent(pid) + '&year=' + nextYear + '&programme=' + encodeURIComponent(programme))
        .then(function (r) { return r.text(); })
        .then(function (html) {
            document.getElementById('project1ModalContent').innerHTML = html;
            openModal();
            bindAddYearForm(pid, nextYear);
        });
}

function bindAddYearForm(pid, year) {
    var form = document.getElementById('addNewProjectYearForm');
    if (!form) return;
    form.addEventListener('submit', function (e) {
        e.preventDefault();

        var $form = $('#addNewProjectYearForm');
        var $modal = $('#project1ModalContent');
        clearValidationErrors($modal);
        if (!isFormValid($form)) { displayClientValidationErrors($form, $modal); return; }

        fetch(yearlyDetailsUrls.addProjectYear, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': getAntiForgeryToken() },
            body: new URLSearchParams(new FormData(form)).toString() + '&projectId=' + encodeURIComponent(pid) + '&year=' + year + '&programme=' + encodeURIComponent(programme)
        })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (data.success) { closeModal(); selectYear(pid, data.year); }
                else if (data.errors) { _showModalErrors(data.errors, $modal); }
                else { showAlertMessage(data.message || 'Failed to add project year.', AlertType.ERROR); }
            })
            .catch(function (err) { console.error('Add project year error:', err); showAlertMessage('Failed to add project year.', AlertType.ERROR); });
    });
}
function openCopyYearDataModal() {
    var html = document.getElementById('copyYearModalContentHidden').innerHTML;
    document.getElementById('project1ModalContent').innerHTML = html;
    openModal();
    bindCopyYearDataForm();
}

function bindCopyYearDataForm() {
    var modalContent = document.getElementById('project1ModalContent');
    if (!modalContent) return;

    var selectedYearFromGlobal = parseInt(selectedYear, 10);
    var selectedYearFromRow = NaN;
    var selectedYearRow = document.querySelector('.project-year-row.selected-table-rowbg');
    if (selectedYearRow) {
        selectedYearFromRow = parseInt(selectedYearRow.getAttribute('data-year'), 10);
    }

    var selectedYearFromHeading = NaN;
    var selectedYearHeading = document.querySelector('.project-year-costs-header h3');
    if (selectedYearHeading && selectedYearHeading.textContent) {
        var headingMatch = selectedYearHeading.textContent.match(/\d{4}/);
        if (headingMatch && headingMatch[0]) {
            selectedYearFromHeading = parseInt(headingMatch[0], 10);
        }
    }

    var currentSelectedYear = !isNaN(selectedYearFromRow)
        ? selectedYearFromRow
        : (!isNaN(selectedYearFromHeading) ? selectedYearFromHeading : selectedYearFromGlobal);

    var yearSelect = modalContent.querySelector('#copyFromYearSelect');
    var doCopyBtn = modalContent.querySelector('#btnDoCopy');
    if (!doCopyBtn) return;

    if (yearSelect) {
        yearSelect.innerHTML = '';

        var years = (Array.isArray(projectYears) ? projectYears : [])
            .map(function (y) { return parseInt(y, 10); })
            .filter(function (y) { return !isNaN(y) && !isNaN(currentSelectedYear) && y < currentSelectedYear; })
            .sort(function (a, b) { return b - a; });

        years.forEach(function (y) {
            var option = document.createElement('option');
            option.value = y;
            option.textContent = y;
            yearSelect.appendChild(option);
        });

        var hasOptions = yearSelect.options.length > 0;
        yearSelect.disabled = !hasOptions;
        doCopyBtn.disabled = !hasOptions;
    }


    doCopyBtn.addEventListener('click', function () {
        var form = modalContent.querySelector('#copyDataForm');
        var $form = $(form);
        var $modal = $('#project1ModalContent');
        clearValidationErrors($modal);
        if (!isFormValid($form)) { displayClientValidationErrors($form, $modal); return; }

        var sourceYear = yearSelect ? parseInt(yearSelect.value, 10) : NaN;
        if (isNaN(sourceYear)) {
            showAlertMessage('Please select a year to copy from.', AlertType.ERROR);
            return;
        }

        showGovukConfirm('Copying will first delete all existing data for the selected year, then copy data from the selected source year. Do you want to continue?')
            .then(function (result) {
                if (!result) return;

                fetch(yearlyDetailsUrls.copyYearData, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': getAntiForgeryToken() },
                    body: 'projectId=' + encodeURIComponent(projectId)
                        + '&sourceYear=' + encodeURIComponent(sourceYear)
                        + '&targetYear=' + encodeURIComponent(currentSelectedYear)
                })
                    .then(function (r) { return r.json(); })
                    .then(function (data) {
                        if (data.success) {
                            closeModal();
                            selectYear(projectId, currentSelectedYear);
                        } else if (data.errors) {
                            _showModalErrors(data.errors, $modal);
                        } else {
                            showAlertMessage(data.message || 'Failed to copy year data.', AlertType.ERROR);
                        }
                    })
                    .catch(function (err) {
                        console.error('Copy year data error:', err);
                        showAlertMessage('Failed to copy year data.', AlertType.ERROR);
                    });
            });
    });
}

// ── DataGrid bridge functions ──────────────────────────────────────
function gridAddStaff() { openAddStaffModal(projectId, selectedYear); }
function gridEditStaff(btn) { openEditStaffModal(projectId, selectedYear, btn.getAttribute('data-id')); }
function gridDeleteStaff(btn) { deleteStaff(projectId, selectedYear, btn.getAttribute('data-id')); }

function gridAddTest() { openAddTestModal(projectId, selectedYear); }
function gridEditTest(btn) { openEditTestModal(projectId, selectedYear, btn.getAttribute('data-id')); }
function gridDeleteTest(btn) { deleteTest(projectId, selectedYear, btn.getAttribute('data-id')); }

function gridAddAnimal() { openAddAnimalModal(projectId, selectedYear); }
function gridEditAnimal(btn) { openEditAnimalModal(projectId, selectedYear, btn.getAttribute('data-id')); }
function gridDeleteAnimal(btn) { deleteAnimal(projectId, selectedYear, btn.getAttribute('data-id')); }

function gridAddAdditionalCost() { openAddAdditionalCostModal(projectId, selectedYear); }
function gridEditAdditionalCost(btn) { openEditAdditionalCostModal(projectId, selectedYear, btn.getAttribute('data-id')); }
function gridDeleteAdditionalCost(btn) { deleteAdditionalCost(projectId, selectedYear, btn.getAttribute('data-id')); }

function gridEditMarkupAndProfit(btn) { openEditMarkupAndProfitModal(projectId, btn.getAttribute('data-id')); }

// ── Legacy edit/delete handlers (kept for compatibility) ───────────
function handleEdit(id) { openEditStaffModal(projectId, selectedYear, id); }
function handleDelete(id) { deleteStaff(projectId, selectedYear, id); }
function handleAnimalEdit(id) { openEditAnimalModal(projectId, selectedYear, id); }
function handleAnimalDelete(id) { deleteAnimal(projectId, selectedYear, id); }
function handleAdditionalEdit(id) { openEditAdditionalCostModal(projectId, selectedYear, id); }
function handleAdditionalDelete(id) { deleteAdditionalCost(projectId, selectedYear, id); }

// ── Staff ──────────────────────────────────────────────────────────
var _staffIsAddingNew = true;
var _staffCurrentIdentity = null;

function openAddStaffModal(pid, year) {
    fetch(yearlyDetailsUrls.createStaff + '?projectId=' + encodeURIComponent(pid) + '&year=' + year + '&isDefra=' + isDefra)
        .then(function (r) { return r.text(); })
        .then(function (html) {
            document.getElementById('project1ModalContent').innerHTML = html;
            _staffIsAddingNew = true;
            _staffCurrentIdentity = null;
            openModal();
            fetchHoursPerDay();
            initWgGradeDropdown();
        });
}
function openEditStaffModal(pid, year, srIdentity) {
    fetch(yearlyDetailsUrls.editStaff + '?projectId=' + encodeURIComponent(pid) + '&year=' + year + '&srIdentity=' + srIdentity + '&isDefra=' + isDefra)
        .then(function (r) { return r.text(); })
        .then(function (html) {
            document.getElementById('project1ModalContent').innerHTML = html;
            _staffIsAddingNew = false;
            _staffCurrentIdentity = srIdentity;
            openModal();
            fetchHoursPerDay();
            initWgGradeDropdown();
            var hiddenGrade = document.getElementById('WgGrade');
            var displayInput = document.getElementById('wgGradeSelect');
            if (hiddenGrade && displayInput && hiddenGrade.value) {
                // Resolve the display text from the cached option list rather than
                // forcing every row into the DOM. Rendering the full list here
                // defeated the lazy loading and made opening the panel in edit
                // mode hang while the browser laid out thousands of rows.
                var selected = getWgGradeOptions().filter(function (o) {
                    return o.v === hiddenGrade.value;
                })[0];
                displayInput.value = selected ? selected.v : hiddenGrade.value;
            }
        });
}
function saveStaff() {
    var $form = $('#staffRequirementForm');
    var $modal = $('#project1ModalContent');
    clearValidationErrors($modal);
    if (!isFormValid($form)) { displayClientValidationErrors($form, $modal); return; }
    var form = $form[0];
    var token = form.querySelector('input[name="__RequestVerificationToken"]').value;
    var url = _staffIsAddingNew
        ? yearlyDetailsUrls.createStaff + '?projectId=' + encodeURIComponent(projectId) + '&year=' + selectedYear
        : yearlyDetailsUrls.editStaff + '?projectId=' + encodeURIComponent(projectId) + '&year=' + selectedYear + '&srIdentity=' + _staffCurrentIdentity;

    showLoader();
    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token },
        body: new URLSearchParams(new FormData(form)).toString()
    })
        .then(function (r) { hideLoader(); return r.json(); })
        .then(function (d) {
            if (d.success) { closeModal(); showAlertMessage(d.message, AlertType.SUCCESS).then(function () { loadStaffGrid(); }); }
            else if (d.errors) { _showModalErrors(d.errors, $modal); }
            else { showAlertMessage(d.message || 'Failed to save staff requirement.', AlertType.ERROR); }
        })
        .catch(function (err) { hideLoader(); console.error('Staff save error:', err); showAlertMessage('Failed to save staff requirement.', AlertType.ERROR); });
}
function deleteStaff(pid, year, srIdentity) {
    showGovukConfirm('Delete this staff entry?').then(function (result) {
        if (!result) return;

        showLoader();
        fetch(yearlyDetailsUrls.deleteStaff + '?projectId=' + encodeURIComponent(pid) + '&year=' + year + '&srIdentity=' + srIdentity, {
            method: 'DELETE',
            headers: { 'RequestVerificationToken': getAntiForgeryToken() }
        })
            .then(function (r) { hideLoader(); return r.json(); })
            .then(function (d) {
                if (d.success) { showAlertMessage(d.message, AlertType.SUCCESS).then(function () { loadStaffGrid(); }); }
                else { showAlertMessage(d.message || 'Failed to delete Staff entry.', AlertType.ERROR); }
            })
            .catch(function (err) { hideLoader(); console.error('Staff delete error:', err); showAlertMessage('Failed to delete Staff entry.', AlertType.ERROR); });
    });
}

// ── Tests ──────────────────────────────────────────────────────────
var _testIsAddingNew = true;
var _testCurrentCode = null;

function openAddTestModal(pid, year) {
    fetch(yearlyDetailsUrls.createTest + '?projectId=' + encodeURIComponent(pid) + '&year=' + year + '&isDefra=' + isDefra)
        .then(function (r) { return r.text(); })
        .then(function (html) {
            document.getElementById('project1ModalContent').innerHTML = html;
            _testIsAddingNew = true;
            _testCurrentCode = null;
            openModal();
            initTestCodeDropdown();
        });
}
function openEditTestModal(pid, year, testCode) {
    fetch(yearlyDetailsUrls.editTest + '?projectId=' + encodeURIComponent(pid) + '&year=' + year + '&testCode=' + encodeURIComponent(testCode) + '&isDefra=' + isDefra)
        .then(function (r) { return r.text(); })
        .then(function (html) {
            document.getElementById('project1ModalContent').innerHTML = html;
            _testIsAddingNew = false;
            _testCurrentCode = testCode;
            openModal();
            initTestCodeDropdown();
            var hiddenCode = document.getElementById('TestCode');
            var displayInput = document.getElementById('testCodeSelect');
            if (hiddenCode && displayInput && hiddenCode.value) {
                _testCodeRows.ensureAllRendered();
                var matchRow = document.querySelector('#testCodeDropdownBody tr[data-value="' + hiddenCode.value + '"]');
                displayInput.value = matchRow ? matchRow.querySelector('td').textContent.trim() : hiddenCode.value;
            }
        });
}
function saveTest() {
    var $form = $('#testRequirementForm');
    var $modal = $('#project1ModalContent');
    clearValidationErrors($modal);
    if (!isFormValid($form)) { displayClientValidationErrors($form, $modal); return; }
    var form = $form[0];
    var token = form.querySelector('input[name="__RequestVerificationToken"]').value;
    var url = _testIsAddingNew
        ? yearlyDetailsUrls.createTest + '?projectId=' + encodeURIComponent(projectId) + '&year=' + selectedYear
        : yearlyDetailsUrls.editTest + '?projectId=' + encodeURIComponent(projectId) + '&year=' + selectedYear + '&testCode=' + encodeURIComponent(_testCurrentCode);

    showLoader();
    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token },
        body: new URLSearchParams(new FormData(form)).toString()
    })
        .then(function (r) { hideLoader(); return r.json(); })
        .then(function (d) {
            if (d.success) { closeModal(); showAlertMessage(d.message, AlertType.SUCCESS).then(function () { loadTestGrid(); }); }
            else if (d.errors) { _showModalErrors(d.errors, $modal); }
            else { showAlertMessage(d.message || 'Failed to save test requirement.', AlertType.ERROR); }
        })
        .catch(function (err) { hideLoader(); console.error('Test save error:', err); showAlertMessage('Failed to save test requirement.', AlertType.ERROR); });
}
function deleteTest(pid, year, testCode) {
    showGovukConfirm('Delete this test entry?').then(function (result) {
        if (!result) return;

        showLoader();
        fetch(yearlyDetailsUrls.deleteTest + '?projectId=' + encodeURIComponent(pid) + '&year=' + year + '&testCode=' + encodeURIComponent(testCode), {
            method: 'DELETE',
            headers: { 'RequestVerificationToken': getAntiForgeryToken() }
        })
            .then(function (r) { hideLoader(); return r.json(); })
            .then(function (d) {
                if (d.success) { showAlertMessage(d.message, AlertType.SUCCESS).then(function () { loadTestGrid(); }); }
                else { showAlertMessage(d.message || 'Failed to delete Test entry.', AlertType.ERROR); }
            })
            .catch(function (err) { hideLoader(); console.error('Test delete error:', err); showAlertMessage('Failed to delete Test entry.', AlertType.ERROR); });
    })
}

// ── Animals ────────────────────────────────────────────────────────
var _animalIsAddingNew = true;
var _animalCurrentIdentity = null;

function openAddAnimalModal(pid, year) {
    fetch(yearlyDetailsUrls.createAnimal + '?projectId=' + encodeURIComponent(pid) + '&year=' + year + '&isDefra=' + isDefra)
        .then(function (r) { return r.text(); })
        .then(function (html) {
            document.getElementById('project1ModalContent').innerHTML = html;
            _animalIsAddingNew = true;
            _animalCurrentIdentity = null;
            openModal();
            initAnimalTypeDropdown();
        });
}
function openEditAnimalModal(pid, year, arIdentity) {
    fetch(yearlyDetailsUrls.editAnimal + '?projectId=' + encodeURIComponent(pid) + '&year=' + year + '&arIdentity=' + arIdentity + '&isDefra=' + isDefra)
        .then(function (r) { return r.text(); })
        .then(function (html) {
            document.getElementById('project1ModalContent').innerHTML = html;
            _animalIsAddingNew = false;
            _animalCurrentIdentity = arIdentity;
            openModal();
            initAnimalTypeDropdown();
            var hiddenType = document.getElementById('AnimalType');
            var displayInput = document.getElementById('animalTypeSelect');
            if (hiddenType && displayInput && hiddenType.value) {
                _animalTypeRows.ensureAllRendered();
                var matchRow = document.querySelector('#animalTypeDropdownBody tr[data-value="' + hiddenType.value + '"]');
                displayInput.value = matchRow ? matchRow.querySelector('td').textContent.trim() : hiddenType.value;
            }
        });
}
function saveAnimal() {
    var $form = $('#animalRequirementForm');
    var $modal = $('#project1ModalContent');
    clearValidationErrors($modal);
    if (!isFormValid($form)) { displayClientValidationErrors($form, $modal); return; }
    var form = $form[0];
    var token = form.querySelector('input[name="__RequestVerificationToken"]').value;
    var url = _animalIsAddingNew
        ? yearlyDetailsUrls.createAnimal + '?projectId=' + encodeURIComponent(projectId) + '&year=' + selectedYear
        : yearlyDetailsUrls.editAnimal + '?projectId=' + encodeURIComponent(projectId) + '&year=' + selectedYear + '&arIdentity=' + _animalCurrentIdentity;

    showLoader();
    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token },
        body: new URLSearchParams(new FormData(form)).toString()
    })
        .then(function (r) { hideLoader(); return r.json(); })
        .then(function (d) {
            if (d.success) {
                closeModal(); showAlertMessage(d.message, AlertType.SUCCESS).then(function () { loadAnimalGrid(); });
            }
            else if (d.errors) { _showModalErrors(d.errors, $modal); }
            else { showAlertMessage(d.message || 'Failed to save animal requirement.', AlertType.ERROR); }
        })
        .catch(function (err) { hideLoader(); console.error('Animal save error:', err); showAlertMessage('Failed to save animal requirement.', AlertType.ERROR); });
}
function deleteAnimal(pid, year, arIdentity) {
    showGovukConfirm('Delete this animal entry?').then(function (result) {
        if (!result) return;

        showLoader();
        fetch(yearlyDetailsUrls.deleteAnimal + '?projectId=' + encodeURIComponent(pid) + '&year=' + year + '&arIdentity=' + arIdentity, {
            method: 'DELETE',
            headers: { 'RequestVerificationToken': getAntiForgeryToken() }
        })
            .then(function (r) { hideLoader(); return r.json(); })
            .then(function (d) {
                if (d.success) { showAlertMessage(d.message, AlertType.SUCCESS).then(function () { loadAnimalGrid(); }); }
                else { showAlertMessage(d.message || 'Failed to delete animal entry.', AlertType.ERROR); }
            })
            .catch(function (err) { hideLoader(); console.error('Animal delete error:', err); showAlertMessage('Failed to delete animal entry.', AlertType.ERROR); });
    });
}

// ── Additional Costs ───────────────────────────────────────────────
var _additionalCostIsAddingNew = true;
var _additionalCostCurrentIdentity = null;

function openAddAdditionalCostModal(pid, year) {
    fetch(yearlyDetailsUrls.createAdditionalCost + '?projectId=' + encodeURIComponent(pid) + '&year=' + year)
        .then(function (r) { return r.text(); })
        .then(function (html) {
            document.getElementById('project1ModalContent').innerHTML = html;
            _additionalCostIsAddingNew = true;
            _additionalCostCurrentIdentity = null;

            fetchadditionalcostinflamation(pid, year)
                .finally(function () {
                    openModal();
                    initAccountCatDropdown();
                });
        });
}
function openEditAdditionalCostModal(pid, year, acIdentity) {
    fetch(yearlyDetailsUrls.editAdditionalCost + '?projectId=' + encodeURIComponent(pid) + '&year=' + year + '&acIdentity=' + acIdentity)
        .then(function (r) { return r.text(); })
        .then(function (html) {
            document.getElementById('project1ModalContent').innerHTML = html;
            _additionalCostIsAddingNew = false;
            _additionalCostCurrentIdentity = acIdentity;

            fetchadditionalcostinflamation(pid, year)
                .finally(function () {
                    openModal();
                    initAccountCatDropdown();
                    var hiddenCat = document.getElementById('AccountCat');
                    var displayInput = document.getElementById('accountCatSelect');
                    if (hiddenCat && displayInput && hiddenCat.value) {
                        _accountCatRows.ensureAllRendered();
                        var matchRow = document.querySelector('#accountCatDropdownBody tr[data-value="' + hiddenCat.value + '"]');
                        displayInput.value = matchRow ? matchRow.querySelector('td').textContent.trim() : hiddenCat.value;
                    }
                });
        });
}
function saveAdditionalCost() {
    var $form = $('#additionalCostForm');
    var $modal = $('#project1ModalContent');
    clearValidationErrors($modal);
    if (!isFormValid($form)) { displayClientValidationErrors($form, $modal); return; }
    var form = $form[0];
    var token = form.querySelector('input[name="__RequestVerificationToken"]').value;
    var url = _additionalCostIsAddingNew
        ? yearlyDetailsUrls.createAdditionalCost + '?projectId=' + encodeURIComponent(projectId) + '&year=' + selectedYear
        : yearlyDetailsUrls.editAdditionalCost + '?projectId=' + encodeURIComponent(projectId) + '&year=' + selectedYear + '&acIdentity=' + _additionalCostCurrentIdentity;

    showLoader();
    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token },
        body: new URLSearchParams(new FormData(form)).toString()
    })
        .then(function (r) { hideLoader(); return r.json(); })
        .then(function (d) {
            if (d.success) { closeModal(); showAlertMessage(d.message, AlertType.SUCCESS).then(function () { loadAdditionalCostGrid(); }); }
            else if (d.errors) { _showModalErrors(d.errors, $modal); }
            else { showAlertMessage(d.message || 'Failed to save additional cost.', AlertType.ERROR); }
        })
        .catch(function (err) { hideLoader(); console.error('Additional cost save error:', err); showAlertMessage('Failed to save additional cost.', AlertType.ERROR); });
}
function deleteAdditionalCost(pid, year, acIdentity) {
    showGovukConfirm('Delete this additional cost entry?').then(function (result) {
        if (!result) return;

        showLoader();
        fetch(yearlyDetailsUrls.deleteAdditionalCost + '?projectId=' + encodeURIComponent(pid) + '&year=' + year + '&acIdentity=' + acIdentity, {
            method: 'DELETE',
            headers: { 'RequestVerificationToken': getAntiForgeryToken() }
        })
            .then(function (r) { hideLoader(); return r.json(); })
            .then(function (d) {
                if (d.success) { showAlertMessage(d.message, AlertType.SUCCESS).then(function () { loadAdditionalCostGrid(); }); }
                else { showAlertMessage(d.message || 'Failed to delete Additional cost entry.', AlertType.ERROR); }
            })
            .catch(function (err) { hideLoader(); console.error('Additional cost delete error:', err); showAlertMessage('Failed to delete Additional cost entry.', AlertType.ERROR); });
    });
}

// ── Markup/Profit ──────────────────────────────────────────────────
function saveYearRate(pid, year, row) {
    var data = {
        Project: pid, YearValue: year,
        MarkupTime: row.querySelector('[name="MarkupTime"]').value,
        MarkupTests: row.querySelector('[name="MarkupTests"]').value,
        MarkupAnimals: row.querySelector('[name="MarkupAnimals"]').value,
        MarkupAdditional: row.querySelector('[name="MarkupAdditional"]').value,
        ProfitTime: row.querySelector('[name="ProfitTime"]').value,
        ProfitTests: row.querySelector('[name="ProfitTests"]').value,
        ProfitAnimals: row.querySelector('[name="ProfitAnimals"]').value,
        ProfitAdditional: row.querySelector('[name="ProfitAdditional"]').value
    };
    fetch(yearlyDetailsUrls.updateProjectYearRate + '?projectId=' + encodeURIComponent(pid) + '&year=' + year, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiForgeryToken() },
        body: JSON.stringify(data)
    }).then(function (r) { return r.json(); }).then(function (d) { if (!d.success) showAlertMessage('Failed to save rates.', AlertType.ERROR); });
}
function openEditMarkupAndProfitModal(pid, yearVal) {
    fetch(yearlyDetailsUrls.editMarkupAndProfit + '?projectId=' + encodeURIComponent(pid) + '&year=' + yearVal + '&programme=' + encodeURIComponent(programme))
        .then(function (r) { return r.text(); })
        .then(function (html) {
            document.getElementById('project1ModalContent').innerHTML = html;
            openModal();
            bindMarkupAndProfitForm(pid, yearVal);
        });
}
function bindMarkupAndProfitForm(pid, yearVal) {
    var form = document.getElementById('addNewProjectYearForm');
    if (!form) return;
    form.addEventListener('submit', function (e) {
        e.preventDefault();

        var $form = $('#addNewProjectYearForm');
        var $modal = $('#project1ModalContent');
        clearValidationErrors($modal);
        if (!isFormValid($form)) { displayClientValidationErrors($form, $modal); return; }

        fetch(yearlyDetailsUrls.updateProjectYearRate + '?projectId=' + encodeURIComponent(pid) + '&year=' + yearVal, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': getAntiForgeryToken() },
            body: new URLSearchParams(new FormData(form)).toString()
        })
            .then(function (r) { return r.json(); })
            .then(function (d) {
                if (d.success) { closeModal(); loadMarkupAndProfitGrid(); }
                else if (d.errors) { _showModalErrors(d.errors, $modal); }
                else { showAlertMessage(d.message || 'Failed to save markup and profit rates.', AlertType.ERROR); }
            })
            .catch(function (err) { console.error('Update markup and profit error:', err); showAlertMessage('Failed to save markup and profit rates.', AlertType.ERROR); });
    });
}

// ── Tab grid loaders ───────────────────────────────────────────────
function loadTabGrid(tabId, page, pageSize) {
    var url = tabGridEndpoints[tabId];
    if (!url) return;
    var panel = document.getElementById(tabId);
    if (!panel) return;

    var params = new URLSearchParams();
    params.append('projectId', projectId);
    params.append('year', selectedYear);
    params.append('Page', (page || 1).toString());
    params.append('PageSize', (pageSize || 10).toString());

    var token = document.querySelector('input[name="__RequestVerificationToken"]');
    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token ? token.value : '' },
        body: params.toString()
    })
        .then(function (response) { return response.text(); })
        .then(function (html) {
            var gridId = tabGridIds[tabId];
            var gridContainer = document.getElementById('gridContainer_' + gridId);
            if (!gridContainer) return;
            gridContainer.innerHTML = html;
            gridContainer.querySelectorAll('script').forEach(function (oldScript) {
                var newScript = document.createElement('script');
                if (oldScript.src) { newScript.src = oldScript.src; } else { newScript.textContent = oldScript.textContent; }
                oldScript.parentNode.replaceChild(newScript, oldScript);
            });
            removeFilterRows(gridContainer);
            gridContainer.querySelectorAll('.sup_pagination_footer.sup_p_0').forEach(function (el) { el.hidden = true; });
        })
        .catch(function (err) { console.error('Failed to load grid for ' + tabId, err); });
}

function loadStaffGrid(page, pageSize) { loadTabGrid('Staff-tab', page, pageSize); loadYearTotals(); }
function loadTestGrid(page, pageSize) { loadTabGrid('Tests-tab', page, pageSize); loadYearTotals(); }
function loadAnimalGrid(page, pageSize) { loadTabGrid('Animals-tab', page, pageSize); loadYearTotals(); }
function loadAdditionalCostGrid(page, pageSize) { loadTabGrid('AdditionalCosts-tab', page, pageSize); loadYearTotals(); }
function loadMarkupAndProfitGrid(page, pageSize) { loadTabGrid('MarkupAndProfit-tab', page, pageSize); }

function loadActiveTabGrid() {
    var activeLink = document.querySelector('.govuk-tabs__list-item--selected .govuk-tabs__tab');
    if (!activeLink) return;
    var tabId = activeLink.getAttribute('href').replace('#', '');
    if (tabGridEndpoints[tabId]) loadTabGrid(tabId);
}

// ── Year totals ────────────────────────────────────────────────────
function loadYearTotals() {
    var token = document.querySelector('input[name="__RequestVerificationToken"]');
    var params = new URLSearchParams();
    params.append('projectId', projectId);
    params.append('year', selectedYear);
    fetch(yearlyDetailsUrls.getYearTotals, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token ? token.value : '' },
        body: params.toString()
    })
        .then(function (r) { return r.json(); })
        .then(function (d) {
            document.getElementById('year-total-staff').value = formatGbp(d.staffCostTotal);
            document.getElementById('staffTotalAmount').value = formatGbp(d.staffCostTotal);
            document.getElementById('year-total-test').value = formatGbp(d.testCostTotal);
            document.getElementById('testTotalAmount').value = formatGbp(d.testCostTotal);
            document.getElementById('year-total-animal').value = formatGbp(d.animalCostTotal);
            document.getElementById('animalTotalAmount').value = formatGbp(d.animalCostTotal);
            document.getElementById('year-total-additional').value = formatGbp(d.additionalCostTotal);
            document.getElementById('additionalTotalAmount').value = formatGbp(d.additionalCostTotal);
            document.getElementById('year-total-grand').value = formatGbp(d.grandTotal);
        })
        .catch(function (err) { console.error('Failed to load year totals', err); });
}

function formatGbp(value) {
    return '\u00a3' + (value || 0).toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

// ── Year row click / tab click handlers ───────────────────────────
function onYearRowClick(row) {
    var year = parseInt(row.getAttribute('data-year'), 10);
    if (isNaN(year) || year === selectedYear) return;
    selectedYear = year;

    var heading = document.querySelector('.project-year-costs-header h3');
    if (heading) heading.textContent = year + ' Year Costs';

    document.querySelectorAll('.project-year-row').forEach(function (r) { r.classList.remove('selected-table-rowbg'); });
    row.classList.add('selected-table-rowbg');

    loadActiveTabGrid();
    loadYearTotals();

    var totalsTitle = document.querySelector('.project-year-totals-title');
    if (totalsTitle) totalsTitle.textContent = year + ' Year Totals:';
}
function clearNonActiveGridContainers(activeTabId) {
    var activeGridId = tabGridIds[activeTabId];
    if (!activeGridId) return;

    ['staffGrid', 'testGrid', 'animalGrid', 'additionalCostGrid', 'markupAndProfitGrid'].forEach(function(gridId) {
        if (gridId === activeGridId) return;
        var gridContainer = document.getElementById('gridContainer_' + gridId);
        if (gridContainer) gridContainer.innerHTML = '';
    });
}
(function initTabClickHandlers() {
    document.addEventListener('DOMContentLoaded', function () {
        var tabLinks = document.querySelectorAll('.govuk-tabs__tab');
        tabLinks.forEach(function (link) {
            link.addEventListener('click', function (e) {
                var href = link.getAttribute('href');
                if (!href) return;
                var tabId = href.replace('#', '');
                clearNonActiveGridContainers(tabId);
                if (tabGridEndpoints[tabId]) loadTabGrid(tabId);
                loadYearTotals();
                document.querySelectorAll('.govuk-tabs__list-item').forEach(function (li) { li.classList.remove('govuk-tabs__list-item--selected'); });
                link.closest('.govuk-tabs__list-item').classList.add('govuk-tabs__list-item--selected');
                document.querySelectorAll('.govuk-tabs__panel').forEach(function (p) {
                    if (p.id === tabId) { p.classList.remove('govuk-tabs__panel--hidden'); p.style.display = 'block'; }
                    else { p.classList.add('govuk-tabs__panel--hidden'); p.style.display = 'none'; }
                });
                e.preventDefault();
            });
        });

        if (selectedYear > 0) {
            document.querySelectorAll('.govuk-tabs__panel').forEach(function (p) { p.classList.add('govuk-tabs__panel--hidden'); p.style.display = 'none'; });
            var staffPanel = document.getElementById('Staff-tab');
            if (staffPanel) { staffPanel.classList.remove('govuk-tabs__panel--hidden'); staffPanel.style.display = 'block'; }
            loadStaffGrid();
            loadYearTotals();
        }

        document.querySelectorAll('.project-year-row').forEach(function (row) {
            row.addEventListener('click', function () { onYearRowClick(row); });
        });
    });
})();

function allGridExtraFilters() {
    return { year: selectedYear };
}

function initSearchableDropdown(options) {
    var input = document.getElementById(options.inputId);
    var panel = document.getElementById(options.panelId);
    var searchBox = document.getElementById(options.searchBoxId);
    var rows = document.querySelectorAll(options.rowsSelector);
    if (!input || !panel) return;

    if (options.preventTyping) {
        input.addEventListener('input', function (e) {
            e.preventDefault();
            this.value = this.getAttribute('data-current-value') || '';
        });
        input.addEventListener('beforeinput', function (e) {
            if (e.inputType !== 'insertReplacementText') {
                e.preventDefault();
            }
        });

        if (input.value) {
            input.setAttribute('data-current-value', input.value);
        }
    }

    function openDropdown() {
        panel.style.display = 'block';
        input.setAttribute('aria-expanded', 'true');
        if (searchBox) {
            searchBox.value = '';
            options.filterRows('');
            searchBox.focus();
        }
    }

    function closeDropdown() {
        panel.style.display = 'none';
        input.setAttribute('aria-expanded', 'false');
    }

    input.addEventListener('click', function (e) {
        e.stopPropagation();
        if (panel.style.display === 'block') {
            closeDropdown();
        } else {
            openDropdown();
        }
    });

    input.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' || e.key === ' ' || e.key === 'ArrowDown') {
            e.preventDefault();
            openDropdown();
        }
    });

    if (searchBox) {
        searchBox.addEventListener('click', function (e) { e.stopPropagation(); });
        searchBox.addEventListener('input', function () { options.filterRows(this.value.toLowerCase()); });
        searchBox.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                closeDropdown();
                input.focus();
            } else if (e.key === 'ArrowDown') {
                e.preventDefault();
                var firstVisible = document.querySelector(options.rowsSelector + ':not([style*="display: none"])');
                if (firstVisible) firstVisible.focus();
            }
        });
    }

    rows.forEach(function (row) {
        bindDropdownRow(row);
    });

    // Dropdowns that render their rows lazily re-bind the behaviour above each
    // time a new chunk of rows is added to the DOM.
    if (options.registerRowBinder) {
        options.registerRowBinder(function (newRows) {
            newRows.forEach(function (row) { bindDropdownRow(row); });
        });
    }

    function bindDropdownRow(row) {
        row.tabIndex = 0;

        row.addEventListener('click', function () {
            options.onRowSelect(this, input);
            closeDropdown();
            input.focus();
        });

        row.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            } else if (e.key === 'Escape') {
                closeDropdown();
                input.focus();
            } else if (e.key === 'ArrowDown') {
                e.preventDefault();
                var next = this.nextElementSibling;
                while (next && next.style.display === 'none') next = next.nextElementSibling;
                if (next) next.focus();
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                var prev = this.previousElementSibling;
                while (prev && prev.style.display === 'none') prev = prev.previousElementSibling;
                if (prev) prev.focus();
                else if (searchBox) searchBox.focus();
            }
        });

        row.addEventListener('mouseenter', function () { this.style.backgroundColor = '#f3f2f1'; });
        row.addEventListener('mouseleave', function () { this.style.backgroundColor = ''; });
    }

    document.addEventListener('click', function (e) {
        if (input && panel && !input.contains(e.target) && !panel.contains(e.target)) closeDropdown();
    });
}

// ── WG Grade custom dropdown ───────────────────────────────────────
// Rows are no longer emitted server-side. They are rendered lazily, in chunks,
// from the #wgGradePanelData JSON block so the modal opens quickly. The selection
// logic below is unchanged: rows still expose the same data-* attributes and
// still update the hidden WgGrade / Chargerate fields.

var WG_GRADE_CHUNK_SIZE = 50;
var _wgGradeVisible = [];
var _wgGradeRendered = 0;
var _wgGradeBindRows = null;
var _wgGradeOptions = null;

function getWgGradeOptions() {
    // The modal markup is injected with innerHTML, which does not execute
    // <script> tags, so the payload is parsed from an inert JSON block instead.
    if (_wgGradeOptions === null) {
        var dataEl = document.getElementById('wgGradePanelData');
        try {
            _wgGradeOptions = dataEl ? (JSON.parse(dataEl.textContent) || []) : [];
        } catch (e) {
            console.warn('Failed to parse WG Grade options.', e);
            _wgGradeOptions = [];
        }
        _wgGradeOptions.forEach(function (o) {
            o.key = ((o.v || '') + '\u0000' + (o.crd || '')).toLowerCase();
        });
    }
    return _wgGradeOptions;
}

function escapeWgGradeHtml(text) {
    return String(text === null || text === undefined ? '' : text)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}

function renderMoreWgGradeRows(count) {
    var body = document.getElementById('wgGradeDropdownBody');
    if (!body) return;

    var start = _wgGradeRendered;
    var end = Math.min(start + (count || WG_GRADE_CHUNK_SIZE), _wgGradeVisible.length);
    if (end <= start) return;

    var cell = ' style="padding:6px 8px; border-bottom:1px solid #f3f2f1;"';
    var html = '';
    for (var i = start; i < end; i++) {
        var o = _wgGradeVisible[i];
        html += '<tr data-value="' + escapeWgGradeHtml(o.v) + '"' +
                    ' data-chargerate="' + escapeWgGradeHtml(o.cr) + '"' +
                    ' data-chargeratewithinflamation="' + escapeWgGradeHtml(o.cri) + '"' +
                    ' tabindex="0" role="option" style="cursor:pointer;">' +
                    '<td' + cell + '>' + escapeWgGradeHtml(o.v) + '</td>' +
                    '<td' + cell + '>' + escapeWgGradeHtml(o.crd) + '</td>' +
                '</tr>';
    }

    body.insertAdjacentHTML('beforeend', html);

    if (_wgGradeBindRows) {
        _wgGradeBindRows(Array.prototype.slice.call(body.children, start, end));
    }

    _wgGradeRendered = end;
}

function renderWgGradeRows() {
    var body = document.getElementById('wgGradeDropdownBody');
    if (!body) return;

    body.innerHTML = '';
    _wgGradeRendered = 0;
    renderMoreWgGradeRows(WG_GRADE_CHUNK_SIZE);
}

// Ensures every matching row exists in the DOM. Only use when code genuinely
// needs the whole list materialised -- it renders every row synchronously and
// so undoes the benefit of chunked rendering.
function ensureAllWgGradeRowsRendered() {
    if (_wgGradeRendered === 0) {
        _wgGradeVisible = getWgGradeOptions();
        renderWgGradeRows();
    }
    while (_wgGradeRendered < _wgGradeVisible.length) {
        renderMoreWgGradeRows(WG_GRADE_CHUNK_SIZE);
    }
}

function initWgGradeDropdown() {
    // Reset lazy state: this runs fresh each time the modal is opened, and the
    // modal markup (including the JSON payload) is replaced each time.
    _wgGradeVisible = [];
    _wgGradeRendered = 0;
    _wgGradeBindRows = null;
    _wgGradeOptions = null;

    initSearchableDropdown({
        inputId: 'wgGradeSelect',
        panelId: 'wgGradeDropdownPanel',
        searchBoxId: 'wgGradeSearchBox',
        rowsSelector: '#wgGradeDropdownBody tr',
        preventTyping: true,
        filterRows: filterWgGradeRows,
        registerRowBinder: function (binder) { _wgGradeBindRows = binder; },
        onRowSelect: function (row, input) {
            var grade = row.getAttribute('data-value');
            var chargeRate = row.getAttribute('data-chargeratewithinflamation') || row.getAttribute('data-chargerate');
            input.value = grade;
            input.setAttribute('data-current-value', grade);
            document.getElementById('WgGrade').value = grade;
            document.getElementById('Chargerate').value = chargeRate;
            // document.getElementById('Payrate').value = row.getAttribute('data-payrate');
            // document.getElementById('Npr').value = row.getAttribute('data-npr');
            // document.getElementById('Ohr').value = row.getAttribute('data-ohr');
            input.dispatchEvent(new Event('input', { bubbles: true }));
            input.dispatchEvent(new Event('change', { bubbles: true }));
            calcStaffCost();
        }
    });

    // Render the first chunk immediately so the list is populated even if the
    // panel is opened without the search box firing an input event.
    _wgGradeVisible = getWgGradeOptions();
    renderWgGradeRows();

    var wgGradePanel = document.getElementById('wgGradeDropdownPanel');
    if (wgGradePanel && !wgGradePanel.dataset.lazyScrollBound) {
        // 'scroll' does not bubble, so this is bound directly to the panel.
        wgGradePanel.dataset.lazyScrollBound = '1';
        wgGradePanel.addEventListener('scroll', function () {
            if (_wgGradeRendered >= _wgGradeVisible.length) return;
            if (this.scrollTop + this.clientHeight >= this.scrollHeight - 100) {
                renderMoreWgGradeRows(WG_GRADE_CHUNK_SIZE);
            }
        });    }

    var hoursInput = document.getElementById('Nohours');
    if (hoursInput) {
        ['input', 'change', 'keyup', 'keydown', 'paste'].forEach(function (evt) {
            hoursInput.addEventListener(evt, function () { setTimeout(calcStaffCost, 0); });
        });
    }
    var daysInput = document.getElementById('Nodays');
    if (daysInput) {
        ['input', 'change', 'keyup', 'keydown', 'paste'].forEach(function (evt) {
            daysInput.addEventListener(evt, function () { setTimeout(calcStaffCostFromDays, 0); });
        });
    }
    document.addEventListener('click', function (e) {
        if (input && panel && !input.contains(e.target) && !panel.contains(e.target)) closeDropdown();
    });
}
function filterWgGradeRows(term) {
    // Filtering now rebuilds the rendered chunk from the cached option list
    // instead of toggling display on every server-rendered row.
    var options = getWgGradeOptions();
    var q = String(term || '').toLowerCase().trim();
    _wgGradeVisible = q
        ? options.filter(function (o) { return o.key.indexOf(q) !== -1; })
        : options;
    renderWgGradeRows();
}

// ── Generic lazy dropdown rows ───────────────────────────────────────────────
// Same approach as the WG Grade dropdown above, factored out so the Test Code,
// Animal Type and Account Category panels can reuse it. Rows are built from an
// inert JSON payload in chunks; the selection logic in each dropdown is
// unchanged because the generated rows expose the same data-* attributes.
var LAZY_DROPDOWN_CHUNK_SIZE = 50;

function createLazyDropdownRows(cfg) {
    var state = {
        options: null,
        visible: [],
        rendered: 0,
        bindRows: null
    };

    function getOptions() {
        // The modal markup is injected with innerHTML, which does not execute
        // <script> tags, so the payload is parsed from an inert JSON block.
        if (state.options === null) {
            var dataEl = document.getElementById(cfg.dataId);
            try {
                state.options = dataEl ? (JSON.parse(dataEl.textContent) || []) : [];
            } catch (e) {
                console.warn('Failed to parse options for ' + cfg.dataId + '.', e);
                state.options = [];
            }
            state.options.forEach(function (o) {
                o.key = cfg.searchKey(o).toLowerCase();
            });
        }
        return state.options;
    }

    function renderMore(count) {
        var body = document.getElementById(cfg.bodyId);
        if (!body) return;

        var start = state.rendered;
        var end = Math.min(start + (count || LAZY_DROPDOWN_CHUNK_SIZE), state.visible.length);
        if (end <= start) return;

        var html = '';
        for (var i = start; i < end; i++) {
            html += cfg.buildRow(state.visible[i]);
        }
        body.insertAdjacentHTML('beforeend', html);

        if (state.bindRows) {
            state.bindRows(Array.prototype.slice.call(body.children, start, end));
        }
        state.rendered = end;
    }

    function render() {
        var body = document.getElementById(cfg.bodyId);
        if (!body) return;
        body.innerHTML = '';
        state.rendered = 0;
        renderMore(LAZY_DROPDOWN_CHUNK_SIZE);
    }

    return {
        reset: function () {
            state.options = null;
            state.visible = [];
            state.rendered = 0;
            state.bindRows = null;
        },
        registerRowBinder: function (binder) { state.bindRows = binder; },
        // Renders the first chunk so the list is populated as soon as the panel opens.
        start: function () {
            state.visible = getOptions();
            render();

            var panel = document.getElementById(cfg.panelId);
            if (panel && !panel.dataset.lazyScrollBound) {
                // 'scroll' does not bubble, so this is bound directly to the panel.
                panel.dataset.lazyScrollBound = '1';
                panel.addEventListener('scroll', function () {
                    if (state.rendered >= state.visible.length) return;
                    if (this.scrollTop + this.clientHeight >= this.scrollHeight - 100) {
                        renderMore(LAZY_DROPDOWN_CHUNK_SIZE);
                    }
                });
            }
        },
        filter: function (term) {
            var options = getOptions();
            var q = String(term || '').toLowerCase().trim();
            state.visible = q
                ? options.filter(function (o) { return o.key.indexOf(q) !== -1; })
                : options;
            render();
        },
        // Ensures every matching row exists in the DOM. Used when code needs to
        // look a row up by value (e.g. prepopulating the input in edit mode).
        ensureAllRendered: function () {
            if (state.rendered === 0) {
                state.visible = getOptions();
                render();
            }
            while (state.rendered < state.visible.length) {
                renderMore(LAZY_DROPDOWN_CHUNK_SIZE);
            }
        },
        findOption: function (value) {
            var match = null;
            getOptions().some(function (o) {
                if (o.v === value) { match = o; return true; }
                return false;
            });
            return match;
        }
    };
}

var _hoursPerDay = 7.2;
var _additioncostinflamation = 1.00;

function fetchHoursPerDay() {
    fetch(yearlyDetailsUrls.getHoursInDay)
        .then(function (r) { return r.json(); })
        .then(function (result) { if (result.success && result.hoursPerDay) _hoursPerDay = result.hoursPerDay; })
        .catch(function () { /* fallback: keep default 7.2 */ });
}

function fetchadditionalcostinflamation(pid, year) {
    var url = yearlyDetailsUrls.getAdditionalCostinflamation
        + '?projectId=' + encodeURIComponent(pid || projectId)
        + '&year=' + (year || selectedYear);

    return fetch(url)
        .then(function (r) { return r.json(); })
        .then(function (result) {
            if (result.success && result.additionalCostInflamation) {
                _additioncostinflamation = result.additionalCostInflamation;
            }
            syncItemCost();
        })
        .catch(function (err) {
            console.warn('Failed to fetch additional cost inflamation, using previous/default value.', err);
            syncItemCost();
        });
}

var _calcStaffGuard = false;
function calcStaffCost() {
    if (_calcStaffGuard) return;
    var hoursEl = document.getElementById('Nohours');
    var daysEl = document.getElementById('Nodays');
    var rateEl = document.getElementById('Chargerate');
    var costEl = document.getElementById('StaffCost');
    if (!hoursEl || !rateEl || !costEl) return;
    var hoursStr = hoursEl.value.trim();
    var hours = hoursStr !== '' ? parseFloat(hoursStr) : 0;
    var rate = parseFloat(rateEl.value);
    if (daysEl && document.activeElement !== daysEl) {
        daysEl.value = (_hoursPerDay > 0) ? (hours / _hoursPerDay) : '0.00';
    }
    costEl.value = !isNaN(rate) ? (hours * rate) : '';
}
function calcStaffCostFromDays() {
    if (_calcStaffGuard) return;
    var hoursEl = document.getElementById('Nohours');
    var daysEl = document.getElementById('Nodays');
    var rateEl = document.getElementById('Chargerate');
    var costEl = document.getElementById('StaffCost');
    if (!hoursEl || !daysEl || !rateEl || !costEl) return;
    var daysStr = daysEl.value.trim();
    var days = daysStr !== '' ? parseFloat(daysStr) : 0;
    _calcStaffGuard = true;
    if (document.activeElement !== hoursEl) {
        hoursEl.value = (_hoursPerDay > 0) ? (days * _hoursPerDay) : '0.00';
    }
    var hours = parseFloat(hoursEl.value);
    var rate = parseFloat(rateEl.value);
    costEl.value = !isNaN(rate) ? (hours * rate) : '';
    _calcStaffGuard = false;
}

// ── Test Code custom dropdown ──────────────────────────────────────
var _testCodeRows = createLazyDropdownRows({
    dataId: 'testCodePanelData',
    bodyId: 'testCodeDropdownBody',
    panelId: 'testCodeDropdownPanel',
    searchKey: function (o) { return (o.v || '') + '\u0000' + (o.d || '') + '\u0000' + (o.upd || ''); },
    buildRow: function (o) {
        var cell = ' style="padding:6px 8px; border-bottom:1px solid #f3f2f1;"';
        return '<tr data-value="' + escapeWgGradeHtml(o.v) + '"' +
                   ' data-description="' + escapeWgGradeHtml(o.d) + '"' +
                   ' data-unitprice="' + escapeWgGradeHtml(o.up) + '"' +
                   ' data-unitpricewithinflamation="' + escapeWgGradeHtml(o.upi) + '"' +
                   ' style="cursor:pointer;">' +
                   '<td' + cell + '>' + escapeWgGradeHtml(o.v) + '</td>' +
                   '<td' + cell + '>' + escapeWgGradeHtml(o.d) + '</td>' +
                   '<td' + cell + '>' + escapeWgGradeHtml(o.upd) + '</td>' +
               '</tr>';
    }
});

function initTestCodeDropdown() {
    _testCodeRows.reset();

    initSearchableDropdown({
        inputId: 'testCodeSelect',
        panelId: 'testCodeDropdownPanel',
        searchBoxId: 'testCodeSearchBox',
        rowsSelector: '#testCodeDropdownBody tr',
        filterRows: filterTestCodeRows,
        registerRowBinder: _testCodeRows.registerRowBinder,
        onRowSelect: function (row, input) {
            var code = row.getAttribute('data-value');
            var unitPrice = row.getAttribute('data-unitpricewithinflamation') || row.getAttribute('data-unitprice');
            input.value = code;
            document.getElementById('TestCode').value = code;
            document.getElementById('UnitPrice').value = unitPrice;
            input.dispatchEvent(new Event('input', { bubbles: true }));
            input.dispatchEvent(new Event('change', { bubbles: true }));
            calcTestCost();
        }
    });

    _testCodeRows.start();

    var noInput = document.getElementById('NumberOfTests');
    if (noInput) {
        ['input', 'change', 'keyup', 'keydown', 'paste'].forEach(function (evt) {
            noInput.addEventListener(evt, function () { setTimeout(calcTestCost, 0); });
        });
    }
}
function filterTestCodeRows(term) {
    _testCodeRows.filter(term);
}
function calcTestCost() {
    var noEl = document.getElementById('NumberOfTests');
    var unitPriceEl = document.getElementById('UnitPrice');
    var costEl = document.getElementById('TestCost');
    if (!noEl || !unitPriceEl || !costEl) return;
    var noStr = noEl.value.trim();
    var no = parseFloat(noStr);
    var price = parseFloat(unitPriceEl.value);
    costEl.value = (noStr !== '' && !isNaN(no) && !isNaN(price)) ? (no * price) : '';
}

// ── Animal Type custom dropdown ────────────────────────────────────
var _animalTypeRows = createLazyDropdownRows({
    dataId: 'animalTypePanelData',
    bodyId: 'animalTypeDropdownBody',
    panelId: 'animalTypeDropdownPanel',
    searchKey: function (o) { return (o.v || '') + '\u0000' + (o.drd || ''); },
    buildRow: function (o) {
        var cell = ' style="padding:6px 8px; border-bottom:1px solid #f3f2f1;"';
        return '<tr data-value="' + escapeWgGradeHtml(o.v) + '"' +
                   ' data-dailyrate="' + escapeWgGradeHtml(o.dr) + '"' +
                   ' data-dailyratewithinflamation="' + escapeWgGradeHtml(o.dri) + '"' +
                   ' style="cursor:pointer;">' +
                   '<td' + cell + '>' + escapeWgGradeHtml(o.v) + '</td>' +
                   '<td' + cell + '>' + escapeWgGradeHtml(o.drd) + '</td>' +
               '</tr>';
    }
});

function initAnimalTypeDropdown() {
    _animalTypeRows.reset();

    initSearchableDropdown({
        inputId: 'animalTypeSelect',
        panelId: 'animalTypeDropdownPanel',
        searchBoxId: 'animalTypeSearchBox',
        rowsSelector: '#animalTypeDropdownBody tr',
        filterRows: filterAnimalTypeRows,
        registerRowBinder: _animalTypeRows.registerRowBinder,
        onRowSelect: function (row, input) {
            var animalType = row.getAttribute('data-value');
            var dailyRate = row.getAttribute('data-dailyratewithinflamation') || row.getAttribute('data-dailyrate');
            input.value = animalType;
            document.getElementById('AnimalType').value = animalType;
            document.getElementById('DailyRate').value = dailyRate;
            input.dispatchEvent(new Event('input', { bubbles: true }));
            input.dispatchEvent(new Event('change', { bubbles: true }));
            calcAnimalCost();
        }
    });

    _animalTypeRows.start();

    ['NumberOfAnimals', 'NumberOfDays'].forEach(function (id) {
        var el = document.getElementById(id);
        if (el) {
            ['input', 'change', 'keyup', 'keydown', 'paste'].forEach(function (evt) {
                el.addEventListener(evt, function () { setTimeout(calcAnimalCost, 0); });
            });
        }
    });
}
function filterAnimalTypeRows(term) {
    _animalTypeRows.filter(term);
}
function calcAnimalCost() {
    var noAnimalsEl = document.getElementById('NumberOfAnimals');
    var noDaysEl = document.getElementById('NumberOfDays');
    var rateEl = document.getElementById('DailyRate');
    var costEl = document.getElementById('AnimalCost');
    if (!noAnimalsEl || !noDaysEl || !rateEl || !costEl) return;
    var noStr = noAnimalsEl.value.trim();
    var daysStr = noDaysEl.value.trim();
    var no = parseFloat(noStr);
    var days = parseFloat(daysStr);
    var rate = parseFloat(rateEl.value);
    costEl.value = (noStr !== '' && daysStr !== '' && !isNaN(no) && !isNaN(days) && !isNaN(rate))
        ? (no * days * rate) : '';
}

// ── Account Category custom dropdown ──────────────────────────────
var _accountCatRows = createLazyDropdownRows({
    dataId: 'accountCatPanelData',
    bodyId: 'accountCatDropdownBody',
    panelId: 'accountCatDropdownPanel',
    searchKey: function (o) { return (o.v || '') + '\u0000' + (o.uid || ''); },
    buildRow: function (o) {
        var cell = ' style="padding:6px 8px; border-bottom:1px solid #f3f2f1;"';
        return '<tr data-value="' + escapeWgGradeHtml(o.v) + '"' +
                   ' data-useinflation="' + escapeWgGradeHtml(o.ui) + '"' +
                   ' style="cursor:pointer;">' +
                   '<td' + cell + '>' + escapeWgGradeHtml(o.v) + '</td>' +
                   '<td' + cell + '>' + escapeWgGradeHtml(o.uid) + '</td>' +
               '</tr>';
    }
});

function initAccountCatDropdown() {
    _accountCatRows.reset();

    initSearchableDropdown({
        inputId: 'accountCatSelect',
        panelId: 'accountCatDropdownPanel',
        searchBoxId: 'accountCatSearchBox',
        rowsSelector: '#accountCatDropdownBody tr',
        filterRows: filterAccountCatRows,
        registerRowBinder: _accountCatRows.registerRowBinder,
        onRowSelect: function (row, input) {
            var cat = row.getAttribute('data-value');
            var useInflation = row.getAttribute('data-useinflation') === 'true';
            input.value = cat;
            input.setAttribute('data-current-useinflation', useInflation ? 'true' : 'false');
            document.getElementById('AccountCat').value = cat;
            input.dispatchEvent(new Event('input', { bubbles: true }));
            input.dispatchEvent(new Event('change', { bubbles: true }));
            setTimeout(syncItemCost, 0);
        }
    });

    _accountCatRows.start();

    var costInput = document.getElementById('CostEntered');
    if (costInput) {
        ['input', 'change', 'keyup', 'keydown', 'paste'].forEach(function (evt) {
            costInput.addEventListener(evt, function () { setTimeout(syncItemCost, 0); });
        });
    }

    setTimeout(syncItemCost, 0);
}
function filterAccountCatRows(term) {
    _accountCatRows.filter(term);
}
function syncItemCost() {
    var costEl = document.getElementById('CostEntered');
    var itemCostEl = document.getElementById('ItemCost');
    var catEl = document.getElementById('AccountCat');
    var catSelectEl = document.getElementById('accountCatSelect');
    if (!costEl || !itemCostEl) return;

    var costStr = costEl.value.trim();
    var cost = parseFloat(costStr);
    if (costStr === '' || isNaN(cost)) {
        itemCostEl.value = '';
        return;
    }

    var useInflation = false;
    if (catSelectEl) {
        useInflation = catSelectEl.getAttribute('data-current-useinflation') === 'true';
    }

    if (!useInflation) {
        var cat = catEl ? catEl.value : '';
        if (cat) {
            // Rows are rendered lazily, so the option list is the source of truth here.
            var option = _accountCatRows.findOption(cat);
            useInflation = !!option && option.ui === 'true';
            if (catSelectEl) {
                catSelectEl.setAttribute('data-current-useinflation', useInflation ? 'true' : 'false');
            }
        }
    }

    var itemCost = cost;
    if (useInflation) {
        var inflation = parseFloat(_additioncostinflamation);
        if (!isNaN(inflation)) {
            itemCost = cost * inflation;
        }
    }

    itemCostEl.value = itemCost;
}

// ── Private helper ─────────────────────────────────────────────────
function _showModalErrors(errors, $modal) {
    displayServerValidationErrors(errors, 'Please correct the errors below.', $modal);
    var $summary = $modal.find('.govuk-error-summary');
    var $list = $summary.find('.govuk-error-summary__list');
    $summary.find('.govuk-error-summary__title').text('There is a problem');
    errors.forEach(function (e) {
        if ($list.find('a[href="#' + e.field + '"]').length === 0) {
            $list.append('<li><a href="#' + e.field + '">' + e.message + '</a></li>');
        }
    });
    $summary.show().focus();
}

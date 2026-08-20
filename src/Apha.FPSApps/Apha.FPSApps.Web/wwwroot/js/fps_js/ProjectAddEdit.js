// Scripts for the Project Add/Edit view (ProjectAddEdit.cshtml).
// Server-generated URLs are provided via window.projectAddEditConfig (set inline in the view).
(function () {
    'use strict';

    function getConfig() {
        return window.projectAddEditConfig || {};
    }

    // Restrict Project Code fields to alphanumeric characters only (A-Z, a-z, 0-9).
    // Blocks typing of special characters and strips them from pasted/other input.
    function restrictToAlphanumeric(input) {
        if (!input) return;
        input.addEventListener('keypress', function (e) {
            // Allow control keys (backspace, tab, enter, etc.)
            if (e.ctrlKey || e.metaKey || e.key.length > 1) return;
            if (!/^[A-Za-z0-9]$/.test(e.key)) {
                e.preventDefault();
            }
        });
        input.addEventListener('input', function () {
            var cleaned = input.value.replace(/[^A-Za-z0-9]/g, '');
            if (cleaned !== input.value) {
                input.value = cleaned;
            }
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        restrictToAlphanumeric(document.querySelector('input[name="ParentProject"]:not([readonly])'));
        restrictToAlphanumeric(document.getElementById('NewProjectCode'));
    });

    function scrollToFirstError() {
        // Wait one tick for the DOM to render error messages
        setTimeout(function () {
            var firstError = document.querySelector('.govuk-error-message:not([style*="display: none"]):not([style*="display:none"])');
            if (!firstError) return;
            firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
            // Try to focus the associated input
            var formGroup = firstError.closest('.govuk-form-group');
            if (formGroup) {
                var input = formGroup.querySelector('input:not([type="hidden"]):not([readonly]), select, textarea');
                if (input) input.focus({ preventScroll: true });
            }
        }, 50);
    }

    function stripCurrencyFormatting() {
        // No-op: numeric fields are already plain numbers (enforced by js-numeric-decimal).
    }

    // ── Button handlers (top-level so onclick="..." always resolves them) ──

    window.submitProject = function () {
        try {
            var form = document.querySelector('form[data-parent-project]');
            var formId = form.id;
            var isEdit = form.dataset.isEdit === 'true';
            var parentProject = form.dataset.parentProject;
            if (typeof clearValidationErrors === 'function') clearValidationErrors('#main-content');
            stripCurrencyFormatting();
            var $form = $('#' + formId);
            if ($form.data('validator') && !$form.valid()) {
                if (typeof displayClientValidationErrors === 'function') displayClientValidationErrors($form, '#main-content');
                scrollToFirstError();
                return;
            }
            submitForm(formId, isEdit, parentProject);
        } catch (e) {
            console.error('submitProject error:', e);
            showAlertMessage('An error occurred: ' + e.message, AlertType.ERROR);
        }
    };

    window.handleDeleteProject = function () {
        var form = document.querySelector('form[data-parent-project]');
        var parentProject = form.dataset.parentProject;
        deleteProject(parentProject);
    };

    window.planProject = function () {
        var form = document.querySelector('form[data-parent-project]');
        var projectCode = form.dataset.parentProject;
        fpsNavigateTo(getConfig().planningUrl + '?projectCode=' + encodeURIComponent(projectCode));
    };

    window.deleteProject = function (parentProject) {
        showGovukConfirm('Are you sure you want to delete this project and all associated data?').then(function (confirmed) {
            if (!confirmed) return;
            var token = document.querySelector('input[name="__RequestVerificationToken"]');
            fetch(getConfig().deleteUrl + '?parentProject=' + encodeURIComponent(parentProject), {
                method: 'POST',
                headers: { 'RequestVerificationToken': token ? token.value : '' }
            })
            .then(function (r) { return r.json(); })
            .then(function (result) {
                if (result.success) {
                    showAlertMessage(result.message || 'Project deleted successfully.', AlertType.SUCCESS).then(function () {
                        window.location.href = result.redirectUrl;
                    });
                } else {
                    displayServerValidationErrors(result.errors, result.message, '#main-content');
                }
            });
        });
    };

    window.changeProjectCode = function (oldCode, newCode) {
        if (!oldCode || !newCode) {
            showAlertMessage('Both old and new project codes are required.', AlertType.INFO);
            return;
        }
        showGovukConfirm('Are you sure you want to change project code from "' + oldCode + '" to "' + newCode + '"? This will update all related records.').then(function (confirmed) {
            if (!confirmed) return;
            var token = document.querySelector('input[name="__RequestVerificationToken"]');
            fetch(getConfig().changeCodeUrl + '?oldCode=' + encodeURIComponent(oldCode) + '&newCode=' + encodeURIComponent(newCode), {
                method: 'POST',
                headers: { 'RequestVerificationToken': token ? token.value : '' }
            })
            .then(function (r) { return r.json(); })
            .then(function (result) {
                if (result.success) {
                    showAlertMessage(result.message || 'Project code changed successfully.', AlertType.SUCCESS).then(function () {
                        window.location.href = result.redirectUrl;
                    });
                } else {
                    displayServerValidationErrors(result.errors, result.message, '#main-content');
                }
            })
            .catch(function (err) {
                console.error('Error:', err);
                showAlertMessage('An error occurred while changing project code.', AlertType.ERROR);
            });
        });
    };

    function submitForm(formId, isEdit, parentProject) {
        showLoader();
        clearValidationErrors('#main-content');
        var form = document.getElementById(formId);
        var formData = new FormData(form);
        var data = {};
        formData.forEach(function (value, key) { data[key] = value; });

        var numericFields = ['CustIncome', 'TransferIncome', 'Profit', 'BudgetCvl', 'PvsIncome',
                             'PlanCaseWorkDebit', 'CarryOver', 'CarryOverSeed', 'CostCentre', 'IsDefraProject'];
        numericFields.forEach(function (field) {
            var raw = (data[field] || '').toString().trim();
            if (raw === '') {
                data[field] = null;
            } else {
                data[field] = isNaN(raw) ? null : raw;
            }
        });
        ['TransferIncome', 'CustIncome', 'IsDefraProject'].forEach(function (field) {
            if (data[field] === null || data[field] === undefined) data[field] = 0;
        });
        if (data['ProjectGroup'] === '' || data['ProjectGroup'] === undefined) data['ProjectGroup'] = null;

        var url = isEdit
            ? getConfig().editUrl + '?parentProject=' + encodeURIComponent(parentProject)
            : getConfig().addUrl;

        var token = document.querySelector('input[name="__RequestVerificationToken"]');
        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token ? token.value : ''
            },
            body: JSON.stringify(data)
        })
        .then(function (r) {
            hideLoader();
            if (!r.ok) {
                return r.text().then(function (t) { throw new Error('Server returned ' + r.status + ': ' + t); });
            }
            return r.json();
        })
        .then(function (result) {
            hideLoader();
            if (result.success) {
                if (!isEdit && result.data && result.data.parentProject) {
                    showAlertMessage(result.message || 'Project created successfully.', AlertType.SUCCESS).then(function () {
                        window.location.href = getConfig().editUrl + '?parentProject=' + encodeURIComponent(result.data.parentProject);
                    });
                } else {
                    showAlertMessage(result.message || 'Project updated successfully.', AlertType.SUCCESS);
                }
            } else {
                displayServerValidationErrors(result.errors, result.message, '#main-content');
                scrollToFirstError();
            }
        })
        .catch(function (err) {
            hideLoader();
            console.error('Error:', err);
            showAlertMessage('An error occurred while submitting the form.', AlertType.ERROR);
        });
    }

    // ── DOM-ready: validator, dropdowns, currency field events ────────────

    $(document).ready(function () {
        if (typeof $.validator !== 'undefined' && typeof $.validator.unobtrusive !== 'undefined') {
            $.validator.setDefaults({ ignore: ':hidden' });
            $.validator.unobtrusive.parse(document);
        }

        // Resource Centre auto-fill from Cost Centre
        function syncResourceCentreFromCostCentre() {
            var sel = document.getElementById('CostCentre');
            if (!sel) return;
            var parts = sel.options[sel.selectedIndex].text.split('|');
            var owningRc = document.getElementById('OwningRc');
            if (owningRc) owningRc.value = parts.length >= 2 ? parts[1].trim() : '';
        }
        var _ccEl = document.getElementById('CostCentre');
        if (_ccEl) _ccEl.addEventListener('change', syncResourceCentreFromCostCentre);

        // Programme multi-column dropdown
        var programDisplay = document.getElementById('ProgramDisplay');
        var programSelect  = document.getElementById('Program');
        var programPanel   = document.getElementById('ProgramDropdownPanel');
        var programSearch  = document.getElementById('ProgramSearchBox');
        var programBody    = document.getElementById('ProgramDropdownBody');

        if (programDisplay && programSelect && programPanel && programSearch && programBody) {
            if (programSelect.value) {
                var matchRow = programBody.querySelector('tr[data-value="' + programSelect.value + '"]');
                programDisplay.value = matchRow ? matchRow.getAttribute('data-display') : programSelect.value;
            }
            programDisplay.addEventListener('click', function (e) {
                e.stopPropagation();
                var isOpen = programPanel.style.display === 'block';
                programPanel.style.display = isOpen ? 'none' : 'block';
                programDisplay.setAttribute('aria-expanded', isOpen ? 'false' : 'true');
                if (!isOpen) { programSearch.value = ''; filterProgramRows(); programSearch.focus(); }
            });
            programSearch.addEventListener('input', filterProgramRows);
            function filterProgramRows() {
                var q = programSearch.value.toLowerCase();
                programBody.querySelectorAll('tr').forEach(function (row) {
                    row.style.display = row.textContent.toLowerCase().indexOf(q) !== -1 ? '' : 'none';
                });
            }
            programBody.querySelectorAll('tr').forEach(function (row) {
                row.addEventListener('click', function () {
                    programDisplay.value = this.getAttribute('data-display');
                    var val = this.getAttribute('data-value');
                    Array.from(programSelect.options).forEach(function (o) { o.selected = o.value === val; });
                    programSelect.dispatchEvent(new Event('change'));
                    programPanel.style.display = 'none';
                    programDisplay.setAttribute('aria-expanded', 'false');
                });
                row.addEventListener('mouseenter', function () { this.style.backgroundColor = '#f3f2f1'; });
                row.addEventListener('mouseleave', function () { this.style.backgroundColor = ''; });
            });
            document.addEventListener('click', function (e) {
                if (!programDisplay.contains(e.target) && !programPanel.contains(e.target)) {
                    programPanel.style.display = 'none';
                    programDisplay.setAttribute('aria-expanded', 'false');
                }
            });
        }

        // Manager multi-column dropdown is initialised by the shared _ManagerPicker partial.

        // Cost Centre multi-column dropdown
        var costCentreDisplay = document.getElementById('CostCentreDisplay');
        var costCentreSelect  = document.getElementById('CostCentre');
        var costCentrePanel   = document.getElementById('CostCentreDropdownPanel');
        var costCentreSearch  = document.getElementById('CostCentreSearchBox');
        var costCentreBody    = document.getElementById('CostCentreDropdownBody');

        if (costCentreDisplay && costCentreSelect && costCentrePanel && costCentreSearch && costCentreBody) {
            if (costCentreSelect.value) {
                var matchCcRow = costCentreBody.querySelector('tr[data-value="' + costCentreSelect.value + '"]');
                costCentreDisplay.value = matchCcRow ? matchCcRow.getAttribute('data-display') : costCentreSelect.value;
            }
            costCentreDisplay.addEventListener('click', function (e) {
                e.stopPropagation();
                var isOpen = costCentrePanel.style.display === 'block';
                costCentrePanel.style.display = isOpen ? 'none' : 'block';
                costCentreDisplay.setAttribute('aria-expanded', isOpen ? 'false' : 'true');
                if (!isOpen) { costCentreSearch.value = ''; filterCostCentreRows(); costCentreSearch.focus(); }
            });
            costCentreSearch.addEventListener('input', filterCostCentreRows);
            function filterCostCentreRows() {
                var q = costCentreSearch.value.toLowerCase();
                costCentreBody.querySelectorAll('tr').forEach(function (row) {
                    row.style.display = row.textContent.toLowerCase().indexOf(q) !== -1 ? '' : 'none';
                });
            }
            costCentreBody.querySelectorAll('tr').forEach(function (row) {
                row.addEventListener('click', function () {
                    costCentreDisplay.value = this.getAttribute('data-display');
                    var val = this.getAttribute('data-value');
                    Array.from(costCentreSelect.options).forEach(function (o) { o.selected = o.value === val; });
                    costCentreSelect.dispatchEvent(new Event('change'));
                    costCentrePanel.style.display = 'none';
                    costCentreDisplay.setAttribute('aria-expanded', 'false');
                });
                row.addEventListener('mouseenter', function () { this.style.backgroundColor = '#f3f2f1'; });
                row.addEventListener('mouseleave', function () { this.style.backgroundColor = ''; });
            });
            document.addEventListener('click', function (e) {
                if (!costCentreDisplay.contains(e.target) && !costCentrePanel.contains(e.target)) {
                    costCentrePanel.style.display = 'none';
                    costCentreDisplay.setAttribute('aria-expanded', 'false');
                }
            });
        }

        // Trigger Resource Centre sync on page load for edit mode
        syncResourceCentreFromCostCentre();
    });
})();

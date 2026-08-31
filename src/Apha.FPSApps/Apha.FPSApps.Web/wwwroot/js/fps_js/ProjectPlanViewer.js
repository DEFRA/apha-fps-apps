// Extra-filter callbacks for Project Plan Viewer sub-grids.
// Each function returns the current projectCode so the Load action can scope
// the data to the selected project.
function getCurrentProjectCode() {
    return document.getElementById('hdnProjectCode')?.value ?? '';
}

window.getPlanSummaryStaffExtraFilters = function () { return { projectCode: getCurrentProjectCode() }; };
window.getPlanSummaryTestExtraFilters  = function () { return { projectCode: getCurrentProjectCode() }; };
window.getPlanSummaryAnimalExtraFilters = function () { return { projectCode: getCurrentProjectCode() }; };
window.getPlanSummaryAdditionalExtraFilters = function () { return { projectCode: getCurrentProjectCode() }; };
window.getStaffPlanExtraFilters   = function () { return { projectCode: getCurrentProjectCode() }; };
window.getStaffActualExtraFilters = function () { return { projectCode: getCurrentProjectCode() }; };
window.getTestPlanExtraFilters    = function () { return { projectCode: getCurrentProjectCode() }; };
window.getTestActualExtraFilters  = function () { return { projectCode: getCurrentProjectCode() }; };
window.getAnimalPlanExtraFilters  = function () { return { projectCode: getCurrentProjectCode() }; };
window.getAnimalActualExtraFilters = function () { return { projectCode: getCurrentProjectCode() }; };
window.getAdditionalPlanExtraFilters   = function () { return { projectCode: getCurrentProjectCode() }; };
window.getAdditionalActualExtraFilters = function () { return { projectCode: getCurrentProjectCode() }; };

(function () {
    'use strict';

    var hdnProjectCode = document.getElementById('hdnProjectCode');
    var projectSelect = document.getElementById('projectSelect');
    var projectDetailsGridId = 'isProjectDetailsGrid';

    var dependentGridIds = [
        'planSummaryStaffGrid',
        'planSummaryTestGrid',
        'planSummaryAnimalGrid',
        'planSummaryAdditionalGrid',
        'staffPlanGrid',
        'staffActualGrid',
        'testPlanGrid',
        'testActualGrid',
        'animalPlanGrid',
        'actualAnimalCostGrid',
        'additionalCostPlanGrid',
        'actualAdditionalCostGrid'
    ];

    function getProjectCode() {
        return hdnProjectCode ? hdnProjectCode.value : '';
    }

    // Extra filter functions for dependent grids - pass parentProject to the controller
    window.getplanSummaryTestGridExtraFilters = function () {
        return { parentProject: getProjectCode() };
    };
    window.gettestPlanGridExtraFilters = function () {
        return { parentProject: getProjectCode() };
    };
    window.getTestActualGridExtraFilters = function () {
        return { parentProject: getProjectCode() };
    };
    window.getplanSummaryStaffGridExtraFilters = function () {
        return { parentProject: getProjectCode() };
    };
    window.getstaffPlanGridExtraFilters = function () {
        return { parentProject: getProjectCode() };
    };
    window.getstaffActualGridExtraFilters = function () {
        return { parentProject: getProjectCode() };
    };
    window.getStaffActualGridExtraFilters = function () {
        return { parentProject: getProjectCode() };
    };
    window.getplanSummaryAnimalGridExtraFilters = function () {
        return { parentProject: getProjectCode() };
    };
    window.getanimalPlanGridExtraFilters = function () {
        return { parentProject: getProjectCode() };
    };
    window.getactualAnimalCostGridExtraFilters = function () {
        return { parentProject: getProjectCode(), animalOnly: true };
    };
    window.getplanSummaryAdditionalGridExtraFilters = function () {
        return { parentProject: getProjectCode() };
    };
    window.getadditionalCostPlanGridExtraFilters = function () {
        return { parentProject: getProjectCode() };
    };
    window.getactualAdditionalCostGridExtraFilters = function () {
        return { parentProject: getProjectCode(), animalOnly: false };
    };

    function setVal(id, value) {
        var el = document.getElementById(id);
        if (el) el.value = value || '';
    }

    function formatCurrency(value) {
        if (value == null || isNaN(value)) return '0.00';
        return Number(value).toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function reloadGrid(gridId) {
        if (!getProjectCode()) return;
        var mgr = window['gridManager_' + gridId];
        if (mgr && typeof mgr.reloadGrid === 'function') {
            mgr.reloadGrid({ page: 1 });
        }
    }

    function refreshDependentGrids() {
        dependentGridIds.forEach(function (id) {
            reloadGrid(id);
        });
    }

    function clearTotals() {
        setVal('planSummaryStaffTotal', '');
        setVal('planSummaryTestTotal', '');
        setVal('planSummaryAnimalTotal', '');
        setVal('planSummaryAdditionalTotal', '');
        setVal('staff-planned-total-cost', '');
        setVal('staff-total-hrs', '');
        setVal('staff-total-cost', '');
        setVal('staff-percent-plan', '');
        setVal('test-planned-total-cost', '');
        setVal('test-total-cost', '');
        setVal('test-percent-plan', '');
        setVal('animal-planned-total-cost', '');
        setVal('animal-total-cost', '');
        setVal('animal-percent-plan', '');
        setVal('additional-planned-total-cost', '');
        setVal('additional-total-cost', '');
        setVal('additional-percent-plan', '');
    }

    function clearFullProjectDetails() {
        var fields = [
            'project-code', 'fpd-description', 'fpd-shortTitle', 'fpd-customer',
            'fpd-programme', 'fpd-manager', 'fpd-disease', 'fpd-custIncome',
            'fpd-transferIncome', 'fpd-targetProfit', 'fpd-status', 'fpd-costBookNo',
            'fpd-contract', 'fpd-defraProject', 'fpd-costCentre', 'fpd-resourceCentre',
            'fpd-projectGroup', 'fpd-incomeAccount', 'fpd-objectiveCode', 'fpd-budget',
            'fpd-pvsIncome', 'fpd-planCWDebit', 'fpd-carryOver', 'fpd-carryOverSeed'
        ];
        fields.forEach(function (id) { setVal(id, ''); });
        var comments = document.getElementById('fpd-comments');
        if (comments) comments.textContent = '';
    }

    // Expose clearProjectState globally so Index.cshtml dropdown handlers can call it
    window.clearProjectState = function () {
        if (hdnProjectCode) hdnProjectCode.value = '';
        setVal('selectedProjectDetails', '');
        setVal('selectedProgramDetails', '');
        clearTotals();
        clearFullProjectDetails();
    };

    function updateFullProjectDetails(data) {
        setVal('project-code', data.projectCode);
        setVal('fpd-description', data.projectTitle);
        setVal('fpd-shortTitle', data.shortTitle);
        setVal('fpd-customer', data.customer);
        setVal('fpd-programme', data.program);
        setVal('fpd-manager', data.manager);
        setVal('fpd-disease', data.disease);
        setVal('fpd-custIncome', formatCurrency(data.custIncome));
        setVal('fpd-transferIncome', formatCurrency(data.transferIncome));
        setVal('fpd-targetProfit', formatCurrency(data.targetProfit));
        setVal('fpd-status', data.projectStatus);
        setVal('fpd-costBookNo', data.costBookNo);
        setVal('fpd-contract', data.contract);
        setVal('fpd-defraProject', data.isDefraProject === 1 ? 'Yes' : 'No');
        setVal('fpd-costCentre', data.costCentre);
        setVal('fpd-resourceCentre', data.owningRc);
        setVal('fpd-projectGroup', data.projectGroup);
        setVal('fpd-incomeAccount', data.incomeAccountCode);
        setVal('fpd-objectiveCode', data.subAccountCode);
        setVal('fpd-budget', formatCurrency(data.budgetCvl));
        setVal('fpd-pvsIncome', formatCurrency(data.pvsIncome));
        setVal('fpd-planCWDebit', formatCurrency(data.planCaseWorkDebit));
        setVal('fpd-carryOver', formatCurrency(data.carryOver));
        setVal('fpd-carryOverSeed', formatCurrency(data.carryOverSeed));
        var comments = document.getElementById('fpd-comments');
        if (comments) comments.textContent = data.comments || '';
    }

    // Load project details and refresh all dependent sections
    async function loadProjectByCode(projectCode) {
        if (!projectCode) {
            window.clearProjectState();
            refreshDependentGrids();
            return;
        }

        // Fetch project details
        try {
            const detailResponse = await fetch('/FPS/ProjectPlanViewer/GetProjectDetails?projectCode=' + encodeURIComponent(projectCode));
            const detailData = await detailResponse.json();
            if (detailData.success) {
                setVal('selectedProgramDetails', detailData.program || '');
                updateFullProjectDetails(detailData);
            }
        } catch (e) {
            console.error('Failed to load project details:', e);
        }

        // Fetch Plan Summary totals
        try {
            const summaryResponse = await fetch('/FPS/ProjectPlanViewer/GetPlanSummaryTotals?projectCode=' + encodeURIComponent(projectCode));
            const summaryData = await summaryResponse.json();
            if (summaryData.success) {
                setVal('planSummaryStaffTotal', formatCurrency(summaryData.totalStaffCost));
                setVal('planSummaryTestTotal', formatCurrency(summaryData.totalTestCost));
                setVal('planSummaryAnimalTotal', formatCurrency(summaryData.totalAnimalCost));
                setVal('planSummaryAdditionalTotal', formatCurrency(summaryData.totalAdditionalCost));
            }
        } catch (e) {
            console.error('Failed to load plan summary totals:', e);
        }

        // Fetch Staff Plan vs Actuals totals
        try {
            const staffResponse = await fetch('/FPS/ProjectPlanViewer/GetStaffPlanVsActualTotals?projectCode=' + encodeURIComponent(projectCode));
            const staffData = await staffResponse.json();
            if (staffData.success) {
                setVal('staff-planned-total-cost', formatCurrency(staffData.totalPlannedCost));
                setVal('staff-total-hrs', Number(staffData.totalActualHrs || 0).toFixed(2));
                setVal('staff-total-cost', formatCurrency(staffData.totalActualCost));
                setVal('staff-percent-plan', Number(staffData.percentOfPlan || 0).toFixed(2) + '%');
            }
        } catch (e) {
            console.error('Failed to load staff plan vs actual totals:', e);
        }

        // Fetch Test Plan vs Actuals totals
        try {
            const testResponse = await fetch('/FPS/ProjectPlanViewer/GetTestPlanVsActualTotals?projectCode=' + encodeURIComponent(projectCode));
            const testData = await testResponse.json();
            if (testData.success) {
                setVal('test-planned-total-cost', formatCurrency(testData.totalPlannedCost));
                setVal('test-total-cost', formatCurrency(testData.totalActualCost));
                setVal('test-percent-plan', Number(testData.percentOfPlan || 0).toFixed(2) + '%');
            }
        } catch (e) {
            console.error('Failed to load test plan vs actual totals:', e);
        }

        // Fetch Animal Plan vs Actuals totals
        try {
            const animalResponse = await fetch('/FPS/ProjectPlanViewer/GetAnimalPlanVsActualTotals?projectCode=' + encodeURIComponent(projectCode));
            const animalData = await animalResponse.json();
            if (animalData.success) {
                setVal('animal-planned-total-cost', formatCurrency(animalData.totalPlannedCost));
                setVal('animal-total-cost', formatCurrency(animalData.totalActualCost));
                setVal('animal-percent-plan', Number(animalData.percentOfPlan || 0).toFixed(2) + '%');
            }
        } catch (e) {
            console.error('Failed to load animal plan vs actual totals:', e);
        }

        // Fetch Additional Plan vs Actuals totals
        try {
            const additionalResponse = await fetch('/FPS/ProjectPlanViewer/GetAdditionalPlanVsActualTotals?projectCode=' + encodeURIComponent(projectCode));
            const additionalData = await additionalResponse.json();
            if (additionalData.success) {
                setVal('additional-planned-total-cost', formatCurrency(additionalData.totalPlannedCost));
                setVal('additional-total-cost', formatCurrency(additionalData.totalActualCost));
                setVal('additional-percent-plan', Number(additionalData.percentOfPlan || 0).toFixed(2) + '%');
            }
        } catch (e) {
            console.error('Failed to load additional plan vs actual totals:', e);
        }

        // Refresh all dependent grids
        refreshDependentGrids();
    }

    // Auto-select first row in a grid
    function selectFirstRow(gridId, onRowSelected) {
        var $firstRow = $('#gridContainer_' + gridId + ' table tbody tr:first');
        if ($firstRow.length && $firstRow.data('id')) {
            $firstRow.closest('table').find('tbody tr').removeClass('selected-row');
            $firstRow.addClass('selected-row');
            if (onRowSelected) onRowSelected($firstRow);
        }
    }

    // Row selection handler for the project details grid
    window.onProjectDetailsRowSelect = function (rowElement) {
        var $row = $(rowElement);
        var projectCode = $row.data('id');
        if (!projectCode) return;

        projectCode = String(projectCode);
        if (hdnProjectCode) hdnProjectCode.value = projectCode;

        // Update header
        setVal('selectedProjectDetails', projectCode);

        // Trigger full project load
        loadProjectByCode(projectCode);
    };

    // After the project details grid reloads, auto-select the first row to cascade
    document.addEventListener('gridReloaded', function (e) {
        if (e.detail && e.detail.gridId === projectDetailsGridId) {
            selectFirstRow(projectDetailsGridId, function ($row) {
                var projectCode = String($row.data('id') || '');
                if (projectCode) {
                    if (hdnProjectCode) hdnProjectCode.value = projectCode;
                    // Only update project dropdown if it was the trigger
                    if (window.gridLoadTrigger === 'project' && projectSelect) {
                        projectSelect.value = projectCode;
                    }
                    setVal('selectedProjectDetails', projectCode);
                    loadProjectByCode(projectCode);
                } else {
                    window.clearProjectState();
                    refreshDependentGrids();
                }
            });

                    // If grid is empty after reload, clear dependent state
                        var $rows = $('#gridContainer_' + projectDetailsGridId + ' table tbody tr');
                        if (!$rows.length || !$rows.first().data('id')) {
                            window.clearProjectState();
                            refreshDependentGrids();
                        }
                    }
                });
            })();

            (function () {
                'use strict';

                const projectSelect = document.getElementById('projectSelect');
                const programSelect = document.getElementById('programSelect');
                const projectGroupSelect = document.getElementById('projectGroupSelect');

                // Tracks which dropdown triggered the last grid reload
                window.gridLoadTrigger = '';

                // Grid ID for the project details master grid
                const projectDetailsGridId = 'isProjectDetailsGrid';

                function getSelectedProgram() {
                    return programSelect ? programSelect.value : '';
                }

                function getSelectedProjectGroup() {
                    return projectGroupSelect ? projectGroupSelect.value : '';
                }

                // Refresh the project details grid via the DataGrid mechanism
                function refreshProjectDetailsGrid() {
                    var mgr = window['gridManager_' + projectDetailsGridId];
                    if (mgr && typeof mgr.reloadGrid === 'function') {
                        mgr.reloadGrid({ page: 1 });
                    }
                }

                // Programme change
                if (programSelect) {
                    programSelect.addEventListener('change', function () {
                        if (projectGroupSelect) projectGroupSelect.value = '';
                        if (projectSelect) projectSelect.value = '';
                        if (window.clearProjectState) window.clearProjectState();

                        if (programSelect.value) {
                            window.gridLoadTrigger = 'program';
                            refreshProjectDetailsGrid();
                        }
                    });
                }

                // Project group change
                if (projectGroupSelect) {
                    projectGroupSelect.addEventListener('change', function () {
                        if (programSelect) programSelect.value = '';
                        if (projectSelect) projectSelect.value = '';
                        if (window.clearProjectState) window.clearProjectState();

                        if (projectGroupSelect.value) {
                            window.gridLoadTrigger = 'group';
                            refreshProjectDetailsGrid();
                        }
                    });
                }

                // Project selection change
                if (projectSelect) {
                    projectSelect.addEventListener('change', function () {
                        if (programSelect) programSelect.value = '';
                        if (projectGroupSelect) projectGroupSelect.value = '';
                        if (window.clearProjectState) window.clearProjectState();

                        if (projectSelect.value) {
                            window.gridLoadTrigger = 'project';
                            refreshProjectDetailsGrid();
                        }
                    });
                }

                // DataGrid extra filters - project details grid passes program/group/project filters
                window.getisProjectDetailsGridExtraFilters = function () {
                    return {
                        program: getSelectedProgram(),
                        projectGroup: getSelectedProjectGroup(),
                        parentProject: projectSelect ? projectSelect.value : ''
                    };
                };
            })();

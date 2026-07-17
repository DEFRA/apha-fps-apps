(function (cfg) {
    'use strict';

    /* ── Module-level state ─────────────────────────────────────────── */
    let currentCentre = cfg.currentCentre || '';
    let currentWorkGroup = cfg.currentWorkGroup || '';
    let currentGrade = cfg.currentGrade || '';

    /* ── Utility helpers ────────────────────────────────────────────── */
    function el(id) { return document.getElementById(id); }

    function setVal(id, value) {
        const input = el(id);
        if (input) input.value = value;
    }

    /* ── Resource-centre cascade ────────────────────────────────────── */
    function LoadGroupsByResourceCentre() {
        const sel = el('resourceCentreSelect');
        const nameEl = el('ssrSelectedCentreName');
        currentCentre = sel ? sel.value : '';

        if (!currentCentre) {
            if (nameEl) nameEl.textContent = '';
            currentGrade = '';
            const list = el('ssrGradeList');
            if (list) list.innerHTML = '';
            ssrClearAll();
            return;
        }

        if (nameEl) nameEl.textContent = currentCentre;
        currentWorkGroup = '';
        currentGrade = '';
        bindGroupDropdownList([]);
        showLoader();
        $.get('/FPS/SetUpStaffResources/GetGroupsByResourceCentre',
            { resourceCentre: currentCentre },
            function (response) {
                hideLoader();
                if (response && response.success) {
                    bindGroupDropdownList(response.data);
                } else {
                    showAlertMessage('Could not load work groups for \'' + currentCentre + '\': ' + (response && response.message || 'Unknown error.'), AlertType.ERROR);
                }
            }
        ).fail(function () {
            hideLoader();
            showAlertMessage('Could not load work groups for \'' + currentCentre + '\'. Please try selecting the Resource Centre again.', AlertType.ERROR);
        });
    }

    function bindGroupDropdownList(workgroups) {
        const select = el('workGroupSelect');
        if (!select) return;

        select.innerHTML = '<option value="">-- Select a Work Group --</option>';
        (workgroups || []).forEach(function (wg) {
            const opt = document.createElement('option');
            opt.value = wg;
            opt.textContent = wg;
            select.appendChild(opt);
        });
    }

    function LoadGradeByGroup() {
        const sel = el('workGroupSelect');
        const selectedGroup = sel ? sel.value : '';
        currentWorkGroup = selectedGroup;

        if (!selectedGroup) {
            bindGradeList([]);
            return;
        }
        showLoader();
        $.get('/FPS/SetUpStaffResources/GetGradesByGroups',
            { workGroup: selectedGroup },
            function (response) {
                hideLoader();
                if (response && response.success) {
                    bindGradeDropdownList(response.data);
                } else {
                    showAlertMessage('Could not load grades for work group \'' + selectedGroup + '\': ' + (response && response.message || 'Unknown error.'), AlertType.ERROR);
                }
            }
        ).fail(function () {
            hideLoader();
            showAlertMessage('Could not load grades for work group \'' + selectedGroup + '\'. Please try selecting the work group again.', AlertType.ERROR);
        });
    }

    function bindGradeDropdownList(workgroups) {
        const select = el('workGroupGradeSelect');
        if (!select) return;

        select.innerHTML = '<option value="">-- Select a WorkGroup Grade--</option>';
        (workgroups || []).forEach(function (wg) {
            const opt = document.createElement('option');
            opt.value = wg.wgGrade;
            opt.textContent = wg.wgGrade;
            select.appendChild(opt);
        });
    }
    window.LoadGroupsByResourceCentre = LoadGroupsByResourceCentre;
    window.LoadGradeByGroup = LoadGradeByGroup;

}(window.ssrConfig || {}));


/* ── Resource Allocation (Stage 2) module ───────────────────────────────── */
(function (cfg) {
    'use strict';

    var gradesUrl = cfg.gradesUrl || '';
    var staffGridUrl = cfg.staffGridUrl || '';
    var jobsGridUrl = cfg.jobsGridUrl || '';
    var totalsUrl = cfg.totalsUrl || '';

    var currentGrade = '';
    var currentStaffId = '';

    /* ── Resource Centre change ─────────────────────────────────────────── */
    async function OnResourceCenterChange() {
        const centre = document.getElementById('resourceCentreSelect').value;
        const gradeSelect = document.getElementById('workGroupGradeSelect');

        gradeSelect.disabled = true;
        gradeSelect.innerHTML = '<option value="">-- Select a Workgroup Grade --</option>';
        clearGrids();

        if (!centre) return;

        try {
            const resp = await fetch(`${gradesUrl}?resourceCentre=${encodeURIComponent(centre)}`);
            const json = await resp.json();
            if (json.success && json.data) {
                json.data.forEach(g => {
                    const opt = document.createElement('option');
                    opt.value = g.value;
                    opt.textContent = g.text;
                    gradeSelect.appendChild(opt);
                });
                gradeSelect.disabled = false;
            }
        } catch (e) {
            showAlertMessage("Error, In OnResourceCenterChange", AlertType.ERROR);
        }
    }

    /* ── Grade change → load staff allocation grid ──────────────────────── */
    async function WorkgroupGradeChange() {
        const grade = document.getElementById('workGroupGradeSelect').value;
        document.getElementById('stage2SelectedWorkGroupGrade').textContent = grade;
        const group = document.getElementById('workGroupSelect').value;
        document.getElementById('stage2SelectedWorkGroup').textContent = group;
        clearJobsGrid();

        if (!grade) { clearStaffGrid(); return; }

        currentGrade = grade;
        currentStaffId = '';

        try {
            showLoader();
            $.post(staffGridUrl, { workGroupGrade: grade, page: 1, pageSize: 10 }, function (html) {
                document.getElementById('gridContainer_StaffAllocationGrid').innerHTML = html;
                SelectFirstStaffRow();
                loadStaffAllocationTotals(grade);

            }).fail(function () {
                hideLoader();
                showAlertMessage('Error loading staff allocation grid.', AlertType.ERROR);
            });
        } catch (e) {
            hideLoader();
            showAlertMessage("Error, In WorkgroupGradeChange", AlertType.ERROR);
        }
    }

    /* ── Load grade-level column totals into the summary panel ─────────── */
    function loadStaffAllocationTotals(grade) {
        if (!totalsUrl || !grade) { clearSummaryPanel(); return; }
        $.get(totalsUrl, { workGroupGrade: grade }, function (data) {
            if (data && data.success) {
                document.getElementById('stage2HoursAvailInput').value = data.hrsAvail;
                document.getElementById('stage2PlannedHrsInput').value = data.plannedHrs;
                document.getElementById('stage2AllocationPctInput').value = data.allocationPct;
                document.getElementById('stage2AssuredChargeInput').value = data.assuredChargeHrs;
                document.getElementById('stage2AssuredUtilInput').value = data.assuredUtilPct;
                document.getElementById('stage2TotalChargeInput').value = data.totalChargeHrs;
                document.getElementById('stage2TotalUtilInput').value = data.totalUtilPct;
                hideLoader();
            } else {
                clearSummaryPanel();
                hideLoader();
            }
        }).fail(function () {
            hideLoader();
            clearSummaryPanel();
        });
    }

    function clearSummaryPanel() {
        ['stage2HoursAvailInput', 'stage2PlannedHrsInput', 'stage2AllocationPctInput',
            'stage2AssuredChargeInput', 'stage2AssuredUtilInput',
            'stage2TotalChargeInput', 'stage2TotalUtilInput'].forEach(function (id) {
                var inp = document.getElementById(id);
                if (inp) inp.value = '';
            });
    }

    /* ── Staff row selection → load jobs grid ───────────────────────────── */
    // rowData is the <tr> DOM element passed by the DataGrid row-click handler.
    async function OnStaffRowSelect(rowData) {
        if (!rowData) return;

        const $row = $(rowData);
        const staffId = $row.data('id');
        const staffName = $row.find('td[data-property="Name"] span, td[data-property="Name"]').first().text().trim();
        const hrsAvail = $row.find('td[data-property="HoursAvailable"] span, td[data-property="HoursAvailable"]').first().text().trim();
        const planHrs = $row.find('td[data-property="PlannedHours"] span, td[data-property="PlannedHours"]').first().text().trim();
        const alocPct = $row.find('td[data-property="AllocationPct"] span, td[data-property="AllocationPct"]').first().text().trim();
        const appCh = $row.find('td[data-property="AssuredChargeHours"] span, td[data-property="AssuredChargeHours"]').first().text().trim();
        const appUti = $row.find('td[data-property="AssuredUtilisationPct"] span, td[data-property="AssuredUtilisationPct"]').first().text().trim();
        const crgHr = $row.find('td[data-property="ChargeHours"] span, td[data-property="ChargeHours"]').first().text().trim();
        const utiPct = $row.find('td[data-property="UtilisationPct"] span, td[data-property="UtilisationPct"]').first().text().trim();

        document.getElementById('stage2SelectedStaffName').textContent = staffName;
        document.getElementById('stage2PersonSelectedInput').value = staffName;
        document.getElementById('stage2SelectedStaffHoursInput').value = planHrs;

        if (!staffId) return;

        currentStaffId = staffId;

        try {
            showLoader();
            $.post(jobsGridUrl, { staffId: staffId, page: 1, pageSize: 10 }, function (html) {
                document.getElementById('gridContainer_StaffJobsGrid').innerHTML = html;
                hideLoader();
            }).fail(function () {
                hideLoader();
                showAlertMessage('Error loading staff jobs grid.', AlertType.ERROR);
            });
        } catch (e) {
            hideLoader();
            showAlertMessage("Error, In OnStaffRowSelect", AlertType.ERROR);
        }
    }

    /* ── Auto-select first staff row after grid reload ──────────────────── */
    function SelectFirstStaffRow() {
        var $firstRow = $('#gridContainer_StaffAllocationGrid table tbody tr.selectable-row:first');
        if ($firstRow.length && $firstRow.data('id')) {
            $('#gridContainer_StaffAllocationGrid table tbody tr').removeClass('selected-row');
            $firstRow.addClass('selected-row');
            OnStaffRowSelect($firstRow[0]);
        }
    }

    /* ── Helpers ────────────────────────────────────────────────────────── */
    function clearStaffGrid() {
        $.post(staffGridUrl, { workGroupGrade: '' }, function (html) {
            document.getElementById('gridContainer_StaffAllocationGrid').innerHTML = html;
        });
        clearSummaryPanel();
    }

    function clearJobsGrid() {
        $.post(jobsGridUrl, { staffId: '' }, function (html) {
            document.getElementById('gridContainer_StaffJobsGrid').innerHTML = html;
        });
        document.getElementById('stage2PersonSelectedInput').value = '';
        document.getElementById('stage2SelectedStaffName').textContent = '';
        document.getElementById('stage2SelectedStaffHoursInput').value = '';
    }

    function clearGrids() {
        clearStaffGrid();
        clearJobsGrid();
    }

    /* ── ExtraFilterMethod callbacks (used by DataGrid reloadGrid) ─────── */
    function GetStaffAllocationExtraFilters() {
        return { workGroupGrade: currentGrade };
    }

    function GetStaffJobsExtraFilters() {
        return { staffId: currentStaffId };
    }

    /* ── Expose functions called from HTML attributes and DataGrid ───────── */
    window.OnResourceCenterChange = OnResourceCenterChange;
    window.WorkgroupGradeChange = WorkgroupGradeChange;
    window.GetStaffAllocationExtraFilters = GetStaffAllocationExtraFilters;
    window.GetStaffJobsExtraFilters = GetStaffJobsExtraFilters;
    window.OnStaffRowSelect = OnStaffRowSelect;
    window.SelectFirstStaffRow = SelectFirstStaffRow;
    window.clearJobsGrid = clearJobsGrid;
    window.clearStaffGrid = clearStaffGrid;
    window.clearGrids = clearGrids;
}(window.raConfig || {}));
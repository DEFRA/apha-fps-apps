/**
 * SetUpStaffResources.js
 * Client-side logic for the Set Up Staff Resources page.
 *
 * Requires window.ssrConfig to be populated by the Razor view before this script runs:
 *   window.ssrConfig = {
 *     currentCentre : '<razor-encoded value>',
 *     currentGrade  : '<razor-encoded value>',
 *     ztCodeUrl     : '<razor Url.Action result>'
 *   };
 */
(function (cfg) {
    'use strict';

    /* ── Module-level state ─────────────────────────────────────────── */
    let currentCentre = cfg.currentCentre || '';
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
        currentGrade = '';
        bindGroupDropdownList([]);

        $.get('/FPS/SetUpStaffResources/GetGroupsByResourceCentre',
            { resourceCentre: currentCentre },
            function (response) {
                if (response && response.success) {
                    bindGroupDropdownList(response.data);
                } else {
                    console.warn('GetGroupsByResourceCentre:', response && response.message);
                }
            }
        ).fail(function () {
            console.error('GetGroupsByResourceCentre failed for:', currentCentre);
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

    /* ── WorkGroup → Grade cascade ──────────────────────────────────── */
    function LoadGradeByGroup() {
        const sel = el('workGroupSelect');
        const selectedGroup = sel ? sel.value : '';

        if (!selectedGroup) {
            bindGradeList([]);
            return;
        }

        $.get('/FPS/SetUpStaffResources/GetGradesByGroups',
            { workGroup: selectedGroup },
            function (response) {
                if (response && response.success) {
                    bindGradeList(response.data);
                } else {
                    console.warn('GetGradesByGroups:', response && response.message);
                }
            }
        ).fail(function () {
            console.error('GetGradesByGroups failed for:', selectedGroup);
        });
    }

    /**
     * Populate the grade listbox.
     * @param {Array<{wgGrade:string, gradeCode:string}>} grades
     */
    function bindGradeList(grades) {
        const list = el('ssrGradeList');
        if (!list) return;

        list.innerHTML = '';

        (grades || []).forEach(function (item) {
            const wg = (typeof item === 'object') ? (item.wgGrade || '') : item;
            const gradeCode = (typeof item === 'object') ? (item.gradeCode || '') : '';

            const li = document.createElement('li');
            li.className = 'ssr-grade-item';
            li.textContent = wg;
            li.setAttribute('role', 'option');
            li.setAttribute('aria-selected', 'false');
            li.setAttribute('data-grade-code', gradeCode);
            li.addEventListener('click', function () { ssrSelectWorkGroup(wg); });
            list.appendChild(li);
        });

        if (grades && grades.length > 0) {
            const first = (typeof grades[0] === 'object') ? grades[0].wgGrade : grades[0];
            ssrSelectWorkGroup(first);
        } else {
            currentGrade = '';
            ssrClearAll();
            reloadStaffGrid();
        }
    }

    /* ── Grade selection ────────────────────────────────────────────── */
    function ssrSelectWorkGroup(wg) {
        currentGrade = wg;

        // Highlight active grade item
        document.querySelectorAll('#ssrGradeList .ssr-grade-item').forEach(function (li) {
            const active = li.textContent.trim() === wg;
            li.classList.toggle('ssr-grade-item--active', active);
            li.setAttribute('aria-selected', active ? 'true' : 'false');
        });

        // Clear person selection; keep grade/workhrs until the AJAX result arrives
        ssrClearPersonSelection();

        // Fetch GradeCode + total AtWork for the selected grade
        if (wg) {
            refreshGradeStats(wg);
        } else {
            setVal('ssrSummaryGrade', '');
            setVal('ssrWorkHrs', '0');
        }

        reloadStaffGrid();
    }

    /**
     * Fetch GradeCode and total AtWork hours for a WgGrade and populate the summary inputs.
     * Also called after a successful save to keep the total in sync.
     */
    function refreshGradeStats(wg) {
        if (!wg) return;

        $.get('/FPS/SetUpStaffResources/GetGradeStats', { wgGrade: wg }, function (data) {
            if (data && data.success) {
                setVal('ssrSummaryGrade', wg || '');
                setVal('ssrWorkHrs', data.totalAtWork != null ? data.totalAtWork : '0');
            }
        }).fail(function () {
            console.warn('GetGradeStats failed for grade:', wg);
        });
    }

    /* ── Staff grid ─────────────────────────────────────────────────── */
    function reloadStaffGrid() {
        const gm = window['gridManager_ssrStaffGrid'];
        if (gm) gm.reloadGrid({ page: 1 });
    }

    /** Called by the DataGrid component as an extra-filter source. */
    function ssrGetStaffExtraFilters() {
        return { wgGrade: currentGrade || '' };
    }

    /* ── Staff row selection ────────────────────────────────────────── */
    /**
     * Called when a staff grid row is clicked.
     * Updates the Person Selected box only — does NOT override the Summary Grade,
     * which is driven solely by grade-list selection.
     */
    function ssrOnStaffRowSelect(row) {
        const id = $(row).data('id') || '';
        const name = $(row).find('td[data-property="Name"] span').text().trim();

        setVal('ssrPersonSelected', name);
        setVal('ssrSelectedPersonId', id);
    }

    function ssrSelectFirstStaffRow() {
        const $first = $('#gridContainer_ssrStaffGrid table tbody tr.selectable-row:first');
        if ($first.length && $first.data('id')) {
            $('#gridContainer_ssrStaffGrid table tbody tr').removeClass('selected-row');
            $first.addClass('selected-row');
            ssrOnStaffRowSelect($first[0]);
        }
    }

    /* ── Edit modal ─────────────────────────────────────────────────── */
    function editSsrStaff(btn) {
        const id = $(btn).data('id');
        $.get('/FPS/SetUpStaffResources/Edit', { pactId: id }, function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        }).fail(function () {
            alert('Failed to load edit form. Please try again.');
        });
    }

    function saveSetUpStaffResources() {
        const pactId = el('hdnPactId')?.value || '';
        const name = el('ssrEditName')?.value || '';
        const hrsPaid = parseFloat(el('ssrEditHrsPaid')?.value) || 0;
        const leave = parseFloat(el('ssrEditLeave')?.value) || 0;
        const sickSp = parseFloat(el('ssrEditSickSp')?.value) || 0;
        const planable = el('ssrEditPlanable')?.checked ? 1 : 0;

        if (!pactId) {
            alert('Cannot save: staff record ID is missing.');
            return;
        }

        const aftInput = document.querySelector('#ssrEditForm input[name="__RequestVerificationToken"]');
        const aft = aftInput ? aftInput.value : '';

        const dto = {
            PactId: pactId,
            Name: name,
            HrsPaid: hrsPaid,
            Leave: leave,
            SickSpecial: sickSp,
            HrsAvail: hrsPaid - leave - sickSp,
            MakeAvailable: planable
        };

        $.ajax({
            url: '/FPS/SetUpStaffResources/Edit',
            type: 'POST',
            data: JSON.stringify(dto),
            contentType: 'application/json; charset=utf-8',
            headers: { 'RequestVerificationToken': aft },
            success: function (data) {
                if (data.success) {
                    closeModal();
                    reloadStaffGrid();
                    // Refresh summary totals because AtWork may have changed
                    refreshGradeStats(currentGrade);
                } else {
                    alert('Save failed: ' + (data.message || 'Unknown error.'));
                }
            },
            error: function () {
                alert('An error occurred while saving. Please try again.');
            }
        });
    }

    function closeModal() {
        $('#modaPopupBody').html('');
        $('#modalPopup').removeClass('show');
    }

    /* ── Navigate to ZT codes ───────────────────────────────────────── */
    function ssrPlanPersonOntoZT() {
        const person = el('ssrPersonSelected');
        const idEl = el('ssrSelectedPersonId');

        if (!person || !person.value) {
            alert('Please select a person first.');
            return;
        }

        let url = cfg.ztCodeUrl;
        const id = idEl ? idEl.value : '';
        if (id) url += '?staffId=' + encodeURIComponent(id);
        window.location.href = url;
    }

    /* ── Clear helpers ──────────────────────────────────────────────── */
    function ssrClearPersonSelection() {
        setVal('ssrPersonSelected', '');
        setVal('ssrSelectedPersonId', '');
    }

    function ssrClearAll() {
        setVal('ssrSummaryGrade', '');
        setVal('ssrWorkHrs', '0');
        ssrClearPersonSelection();
    }

    /* ── Grid-reloaded event ────────────────────────────────────────── */
    document.addEventListener('gridReloaded', function (e) {
        if (e.detail && e.detail.gridId === 'ssrStaffGrid') {
            ssrSelectFirstStaffRow();
        }
    });

    /* ── Expose functions required by Razor-rendered HTML handlers ─── */
    window.LoadGroupsByResourceCentre = LoadGroupsByResourceCentre;
    window.LoadGradeByGroup = LoadGradeByGroup;
    window.ssrSelectWorkGroup = ssrSelectWorkGroup;
    window.ssrGetStaffExtraFilters = ssrGetStaffExtraFilters;
    window.ssrOnStaffRowSelect = ssrOnStaffRowSelect;
    window.editSsrStaff = editSsrStaff;
    window.saveSetUpStaffResources = saveSetUpStaffResources;
    window.closeModal = closeModal;
    window.ssrPlanPersonOntoZT = ssrPlanPersonOntoZT;

}(window.ssrConfig || {}));

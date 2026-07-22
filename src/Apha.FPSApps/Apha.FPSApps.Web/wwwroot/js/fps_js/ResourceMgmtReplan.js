(function () {
    'use strict';

    /* ── State ─────────────────────────────────────────────────────────── */
    let _currentResourceCentre = '';
    let _currentWgGrade        = '';
    let _currentJobCode        = '';
    let _stagedRows            = [];
    const _emptyStaffGridHtml  = (document.getElementById('gridContainer_RePlanGrid') || {}).innerHTML || '';

    /* ── DOM helpers ────────────────────────────────────────────────────── */
    const el      = id => document.getElementById(id);
    const setVal  = (id, v) => { const e = el(id); if (e) e.value = v ?? ''; };
    const setTxt  = (id, v) => { const e = el(id); if (e) e.textContent = v ?? ''; };

    /* ── Antiforgery token ──────────────────────────────────────────────── */
    function getAntiforgeryHeader() {
        const tmp = document.createElement('div');
        tmp.innerHTML = RRA_ANTIFORGERY_TOKEN;
        const input = tmp.querySelector('input[name="__RequestVerificationToken"]');
        return input ? { 'RequestVerificationToken': input.value } : {};
    }

    /* ── 1. Resource Centre change ──────────────────────────────────────── */
    function rraUpdateResourceCentre() {
        const centre = el('resourceCentreSelect')?.value || '';
        _currentResourceCentre = centre;
        _currentWgGrade = '';
        _currentJobCode = '';
        _stagedRows = [];

        setTxt('ssrMainCentreName', centre);

        const workGroupList = el('ssrWorkGroupList');
        if (workGroupList) workGroupList.innerHTML = '';

        rraClearGrids();
        rraResetAllTimeFields();
        rraResetStagedPanel();

        if (!centre) return;

        showLoader();
        $.get(RRA_WORKGROUPS_URL, { resourceCentre: centre })
            .done(function (r) {
                hideLoader();
                if (r?.success && r.data?.length) {
                    rraPopulateGroupList(r.data);
                } else {
                    rraPopulateGroupList([]);
                }
            })
            .fail(function () {
                hideLoader();
                showAlertMessage('Failed to load workgroups. Please try again.', AlertType.ERROR);
            });
    }

    /* ── Populate the left-hand workgroup grade list ────────────────────── */
    function rraPopulateGroupList(items) {
        const list = el('ssrWorkGroupList');
        if (!list) return;

        list.innerHTML = '';

        if (!items || !items.length) {
            rraClearStaffGrid();
            return;
        }

        const frag = document.createDocumentFragment();
        items.forEach(function (item) {
            const li = document.createElement('li');
            li.className = 'ssr-grade-item';
            li.role = 'option';
            li.textContent = item;
            li.dataset.wgGrade = item;
            li.addEventListener('click', function () {
                rraOnGroupSelect(li);
            });
            frag.appendChild(li);
        });
        list.appendChild(frag);

        // Auto-select the first workgroup so the grid loads immediately.
        const firstItem = list.querySelector('.ssr-grade-item');
        if (firstItem) rraOnGroupSelect(firstItem);
    }

    /* ── 2. Workgroup grade selected from list ──────────────────────────── */
    function rraOnGroupSelect(liEl) {
        document.querySelectorAll('#ssrWorkGroupList .ssr-grade-item')
            .forEach(function (i) { i.classList.remove('ssr-grade-item--active'); });
        liEl.classList.add('ssr-grade-item--active');

        _currentWgGrade = liEl.dataset.wgGrade || '';
        _currentJobCode = '';
        _stagedRows     = [];

        rraResetAllTimeFields();
        rraResetStagedPanel();

        if (!_currentWgGrade) return;

        rraLoadRePlanGrid();
    }

    /* ── Load Section 2: re-plan grid ───────────────────────────────────── */
    function rraLoadRePlanGrid() {
        if (!_currentWgGrade) {
            rraClearStaffGrid();
            return;
        }

        showLoader();
        $.ajax({
            url:  RRA_LOAD_GRID_URL,
            type: 'POST',
            data: { workGroup: _currentWgGrade },
            headers: getAntiforgeryHeader()
        })
        .done(function (html) {
            hideLoader();
            const container = el('gridContainer_RePlanGrid');
            if (container) $(container).html(html);
        })
        .fail(function () {
            hideLoader();
            showAlertMessage('Failed to load the re-plan grid. Please try again.', AlertType.ERROR);
        });
    }

    /* ── ExtraFilterMethod hook — passes current workgroup to every grid request ── */
    window.rraGetRePlanExtraFilters = function () {
        return { workGroup: _currentWgGrade || '' };
    };

    /* ── ExtraFilterMethod hook — passes jobCode + wgGrade to AllTime grid reloads ── */
    window.rraGetAllTimeExtraFilters = function () {
        return { jobCode: _currentJobCode || '', wgGrade: _currentWgGrade || '' };
    };

    /* ── Staff row selected in Section 2 grid ───────────────────────────── */
    function rraOnStaffRowSelect(trEl) {
        if (!trEl) return;

        // _DataGrid sets data-id on the <tr> from KeyProperty ("StaffRowKey").
        // StaffRowKey is rendered as "{ParentProject}|{WgGrade}" by the server.
        var rowKey = trEl.getAttribute('data-id') || '';
        var parts  = rowKey.split('|');
        if (parts.length < 2 || !parts[0] || !parts[1]) return;

        _currentJobCode = parts[0];   // ParentProject = sj.JobCode
        _currentWgGrade = parts[1];   // WgGrade = wgg.WgGrade (e.g. "WG1-AGRADE")

        if (!_currentJobCode) return;

        rraLoadAllTimeGrid();
        rraResetStagedPanel();
    }

    /* ── Load Section 3: all-time grid ─────────────────────────────────── */
    function rraLoadAllTimeGrid() {
        if (!_currentJobCode || !_currentWgGrade) {
            rraClearAllTimeGrid();
            return;
        }

        showLoader();
        $.ajax({
            url:  RRA_LOAD_ALLTIME_URL,
            type: 'POST',
            data: { jobCode: _currentJobCode, wgGrade: _currentWgGrade },
            headers: getAntiforgeryHeader()
        })
        .done(function (html) {
            hideLoader();
            const container = el('gridContainer_AllTimeGrid');
            if (container) $(container).html(html);
            rraRecalcAllTimeTotal();
        })
        .fail(function () {
            hideLoader();
            showAlertMessage('Failed to load the all-time grid. Please try again.', AlertType.ERROR);
        });

        setVal('ssrAllTimeProject', _currentJobCode);
        setVal('ssrAllTimeWgGrade', _currentWgGrade);
    }

    /* ── Recalculate all-time total from rendered grid rows ─────────────── */
    function rraRecalcAllTimeTotal() {
        let total = 0;
        // _DataGrid renders cells as: <td data-property="PlannedHours"><span>value</span></td>
        document.querySelectorAll('#gridContainer_AllTimeGrid td[data-property="PlannedHours"] span').forEach(function (cell) {
            total += parseFloat(cell.textContent) || 0;
        });
        setVal('ssrAllTimeTotal', total.toFixed(2));
    }

    /* ── Staged rows panel ──────────────────────────────────────────────── */
    function rraLoadStagedRows() {
        if (!_currentJobCode || !_currentWgGrade) return;

        showLoader();
        $.get(RRA_LOAD_STAGED_URL, { jobCode: _currentJobCode, wgGrade: _currentWgGrade })
            .done(function (r) {
                hideLoader();
                if (r?.success) {
                    _stagedRows = r.data || [];
                    rraRenderStagedGrid();
                    rraEnableStagedButtons(true);
                } else {
                    showAlertMessage(r?.message || 'Failed to load staged rows.', AlertType.ERROR);
                }
            })
            .fail(function () {
                hideLoader();
                showAlertMessage('Failed to load staged rows. Please try again.', AlertType.ERROR);
            });
    }

    /* ── Render the staged rows table ───────────────────────────────────── */
    function rraRenderStagedGrid() {
        const container = el('ssrStagedGrid');
        if (!container) return;

        if (!_stagedRows.length) {
            container.innerHTML = '<p class="govuk-body govuk-!-margin-top-2">No staged rows.</p>';
            setVal('ssrNewPlanTotal', '0.00');
            return;
        }

        let total = 0;
        const rows = _stagedRows.map(function (r) {
            total += r.plannedHours || 0;
            return `<tr>
                <td class="govuk-table__cell">${escHtml(r.staffId || '')}</td>
                <td class="govuk-table__cell">${escHtml(r.jobCode || '')}</td>
                <td class="govuk-table__cell text-AlignRight">${(r.plannedHours || 0).toFixed(2)}</td>
                <td class="govuk-table__cell">
                    <button type="button" class="govuk-button govuk-button--warning govuk-!-margin-bottom-0"
                            onclick="rraRemoveStagedRow('${escHtml(r.staffId || '')}')">Remove</button>
                </td>
            </tr>`;
        }).join('');

        container.innerHTML = `
            <table class="govuk-table govuk-!-margin-bottom-0">
                <thead class="govuk-table__head">
                    <tr class="govuk-table__row">
                        <th class="govuk-table__header">Staff ID</th>
                        <th class="govuk-table__header">Job Code</th>
                        <th class="govuk-table__header govuk-table__header--numeric">Planned Hours</th>
                        <th class="govuk-table__header"></th>
                    </tr>
                </thead>
                <tbody class="govuk-table__body">${rows}</tbody>
            </table>`;

        setVal('ssrNewPlanTotal', total.toFixed(2));
    }

    /* ── Remove a staged row by staffId ─────────────────────────────────── */
    function rraRemoveStagedRow(staffId) {
        _stagedRows = _stagedRows.filter(function (r) { return r.staffId !== staffId; });
        rraRenderStagedGrid();
        if (!_stagedRows.length) rraEnableStagedButtons(false);
    }

    /* ── Enable / disable OK + Cancel buttons ───────────────────────────── */
    function rraEnableStagedButtons(enabled) {
        const ok     = el('ssrReplanOkBtn');
        const cancel = el('ssrReplanCancelBtn');
        if (ok)     ok.disabled     = !enabled;
        if (cancel) cancel.disabled = !enabled;
    }

    /* ── Commit re-plan (OK button) ─────────────────────────────────────── */
    function rraConfirmRePlan() {
        if (!_stagedRows.length) {
            showAlertMessage('There are no staged rows to commit.', AlertType.WARNING);
            return;
        }

        showLoader();
        $.ajax({
            url:         RRA_COMMIT_URL,
            type:        'POST',
            contentType: 'application/json',
            data:        JSON.stringify({
                jobCode:    _currentJobCode,
                wgGrade:    _currentWgGrade,
                stagedRows: _stagedRows
            }),
            headers: getAntiforgeryHeader()
        })
        .done(function (r) {
            hideLoader();
            if (r?.success) {
                showAlertMessage('Re-plan committed successfully.', AlertType.SUCCESS);
                rraResetAll();
            } else {
                showAlertMessage(r?.message || 'Commit failed. Please check your data and try again.', AlertType.ERROR);
            }
        })
        .fail(function (xhr) {
            hideLoader();
            const msg = xhr.responseJSON?.message || 'An error occurred while committing the re-plan.';
            showAlertMessage(msg, AlertType.ERROR);
        });
    }

    /* ── Cancel staged re-plan ──────────────────────────────────────────── */
    function rraCancelRePlan() {
        _stagedRows = [];
        rraResetStagedPanel();
    }

    /* ── Reset helpers ──────────────────────────────────────────────────── */
    function rraResetAll() {
        _currentWgGrade = '';
        _currentJobCode = '';
        _stagedRows     = [];

        document.querySelectorAll('#ssrWorkGroupList .ssr-workgroup-item')
            .forEach(function (i) { i.classList.remove('ssr-workgroup-item--selected'); });

        rraClearGrids();
        rraResetAllTimeFields();
        rraResetStagedPanel();
    }

    function rraClearGrids() {
        rraClearStaffGrid();
        rraClearAllTimeGrid();
    }

    function rraClearStaffGrid() {
        const c = el('gridContainer_RePlanGrid');
        if (c) c.innerHTML = _emptyStaffGridHtml;
    }

    function rraClearAllTimeGrid() {
        const c = el('gridContainer_AllTimeGrid');
        if (c) c.innerHTML = '';
    }

    function rraResetAllTimeFields() {
        setVal('ssrAllTimeProject', '');
        setVal('ssrAllTimeWgGrade', '');
        setVal('ssrAllTimeTotal',   '0.00');
    }

    function rraResetStagedPanel() {
        _stagedRows = [];
        const c = el('ssrStagedGrid');
        if (c) c.innerHTML = '';
        setVal('ssrNewPlanTotal', '0.00');
        rraEnableStagedButtons(false);
    }

    /* ── HTML escape utility ────────────────────────────────────────────── */
    function escHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    /* ── Expose public API (called by Razor inline handlers) ────────────── */
    window.rraUpdateResourceCentre  = rraUpdateResourceCentre;
    window.rraOnStaffRowSelect      = rraOnStaffRowSelect;
    window.rraRePlanGeneralGrades   = rraLoadStagedRows;
    window.rraConfirmRePlan         = rraConfirmRePlan;
    window.rraCancelRePlan          = rraCancelRePlan;
    window.rraRemoveStagedRow       = rraRemoveStagedRow;

}());

// COS90 page JavaScript
// getCos90GridExtraFilters() is defined in the Razor @section Scripts block
// because it must be available before the _DataGrid grid manager initialises.

$(function () {
    var editContext = { workGroupName: '', profitCentre: '', currentFlag: 0 };

    // ── For Period custom dropdown ───────────────────────────────────────
    (function initializePeriodDropdown() {
        var periods = window.__cos90PeriodData || [];
        var dropdown = document.getElementById('for-period-dropdown');
        var input = document.getElementById('txtstaffsearchBox');
        var hidden = document.getElementById('for-period-value');
        var panel = document.getElementById('for-period-panel');
        var search = document.getElementById('for-period-search');
        var tbody = document.getElementById('for-period-body');

        if (!dropdown || !input || !hidden || !panel || !search || !tbody) return;

        function renderRows(filter) {
            var term = (filter || '').toLowerCase();
            tbody.innerHTML = '';

            if (!term) {
                var clearTr = document.createElement('tr');
                clearTr.onclick = function () {
                    input.value = '--select--';
                    hidden.value = '';
                    panel.style.display = 'none';
                };
                tbody.appendChild(clearTr);
            }

            periods
                .filter(function (p) {
                    return String(p.period || '').toLowerCase().includes(term) ||
                        String(p.monthName || '').toLowerCase().includes(term);
                })
                .forEach(function (p) {
                    var tr = document.createElement('tr');
                    tr.innerHTML =
                        '<td class="sup_text_center">' + (p.period ?? '') + '</td>' +
                        '<td class="sup_text_center">' + (p.monthName ?? '') + '</td>' +
                        '<td class="sup_text_align_right">' + (p.monthNumber ?? '') + '</td>';

                    tr.onclick = function () {
                        input.value = String(p.period ?? '');
                        hidden.value = p.monthNumber ?? '';
                        panel.style.display = 'none';
                    };

                    tbody.appendChild(tr);
                });
        }

        var preselected = hidden.value;
        if (preselected) {
            var match = periods.find(function (p) { return String(p.monthNumber) === String(preselected); });
            if (match) {
                input.value = String(match.period ?? '');
            }
        }

        input.addEventListener('click', function (e) {
            e.stopPropagation();
            panel.style.display = 'block';
            panel.style.width = '100%';
            search.value = '';
            renderRows('');
            search.focus();
        });

        search.addEventListener('input', function (e) {
            renderRows((e.target.value || '').toLowerCase());
        });

        document.addEventListener('click', function (e) {
            if (!dropdown.contains(e.target)) {
                panel.style.display = 'none';
            }
        });
    }());

    // ── When Profit Centre dropdown changes: reload the grid ──────────────
    $('#SelectedProfitCentre').on('change', function () {
        var gm = window['gridManager_cos90WorkGroupGrid'];
        if (gm) {
            gm.reloadGrid({ page: 1 });
        }
    });

    // ── Select PC's Work Groups ──────────────────────────────────────────
    $('#selectPCWorkgroupsBtn').on('click', function () {
        var pc = $('#SelectedProfitCentre').val();
        if (!pc) {
            alert('Please select a Profit Centre first.');
            return;
        }

        var token = $('input[name="__RequestVerificationToken"]').first().val();

        $.ajax({
            url: '/PACT/WorkGroupCos90s/SelectPCWorkGroups',
            type: 'POST',
            data: { profitCentre: pc },
            headers: { 'RequestVerificationToken': token },
            success: function (res) {
                if (res.success) {
                    var gm = window['gridManager_cos90WorkGroupGrid'];
                    if (gm) { gm.reloadGrid({ page: 1 }); }
                }
            },
            error: function () {
                alert('Failed to select work groups. Please try again.');
            }
        });
    });

    // ── Clear PC's Work Groups ───────────────────────────────────────────
    $('#clearPCWorkgroupsBtn').on('click', function () {
        var pc = $('#SelectedProfitCentre').val();
        if (!pc) {
            alert('Please select a Profit Centre first.');
            return;
        }

        var token = $('input[name="__RequestVerificationToken"]').first().val();

        $.ajax({
            url: '/PACT/WorkGroupCos90s/ClearPCWorkGroups',
            type: 'POST',
            data: { profitCentre: pc },
            headers: { 'RequestVerificationToken': token },
            success: function (res) {
                if (res.success) {
                    var gm = window['gridManager_cos90WorkGroupGrid'];
                    if (gm) { gm.reloadGrid({ page: 1 }); }
                }
            },
            error: function () {
                alert('Failed to clear work groups. Please try again.');
            }
        });
    });

    // ── Clear All Work Groups ────────────────────────────────────────────
    $('#clearAllWorkgroupsBtn').on('click', function () {
        if (!confirm('This will clear the COS90 flag for ALL work groups across all profit centres. Continue?')) {
            return;
        }

        var token = $('input[name="__RequestVerificationToken"]').first().val();

        $.ajax({
            url: '/PACT/WorkGroupCos90s/ClearAllWorkGroups',
            type: 'POST',
            headers: { 'RequestVerificationToken': token },
            success: function (res) {
                if (res.success) {
                    var gm = window['gridManager_cos90WorkGroupGrid'];
                    if (gm) { gm.reloadGrid({ page: 1 }); }
                }
            },
            error: function () {
                alert('Failed to clear all work groups. Please try again.');
            }
        });
    });

    // ── Edit Print COS90 modal (row action) ─────────────────────────────
    window.editCos90WorkGroup = function (btn) {
        var row = $(btn).closest('tr');
        var workGroupName = row.find('span[name="WorkGroupName"]').text().trim();
        var profitCentre = row.find('span[name="ProfitCentre"]').text().trim() || $('#SelectedProfitCentre').val();
        var yesChecked = row.find('input[name="Cos90Yes"]').is(':checked');

        editContext = {
            workGroupName: workGroupName,
            profitCentre: profitCentre,
            currentFlag: yesChecked ? 1 : 0
        };

        $('#txtmodal-workgroup').val(workGroupName);
        if (yesChecked) {
            $('#printCOS90-yes').prop('checked', true);
        } else {
            $('#printCOS90-no').prop('checked', true);
        }

        $('#editPrintCOS90Modal').addClass('show');
    };

    $('#closePrintModalBtn, #cancelPrintModalBtn').on('click', function () {
        $('#editPrintCOS90Modal').removeClass('show');
    });

    $('#savePrintBtn').on('click', function () {
        var flag = $('input[name="printCOS90"]:checked').val();
        if (typeof flag === 'undefined') {
            alert('Please select Yes or No.');
            return;
        }

        var token = $('input[name="__RequestVerificationToken"]').first().val();

        $.ajax({
            url: '/PACT/WorkGroupCos90s/UpdateWorkGroupCos90',
            type: 'POST',
            data: {
                workGroupName: editContext.workGroupName,
                profitCentre: editContext.profitCentre,
                flag: flag
            },
            headers: { 'RequestVerificationToken': token },
            success: function (res) {
                if (res.success) {
                    $('#editPrintCOS90Modal').removeClass('show');
                    var gm = window['gridManager_cos90WorkGroupGrid'];
                    if (gm) { gm.reloadGrid({ page: 1 }); }
                }
            },
            error: function () {
                alert('Failed to update COS90 setting. Please try again.');
            }
        });
    });

    // ── Print COS90s button is disabled per requirements ─────────────────
    // No click handler – button stays disabled until Print functionality is implemented.

    // ── Maintain Working Hours & Days modal (read-only) ──────────────────
    $('#maintainWorkHoursDaysBtn').on('click', function () {
        $.ajax({
            url: '/PACT/WorkGroupCos90s/GetMonthHourGrid',
            type: 'GET',
            success: function (html) {
                $('#monthHourGridContainer').html(html);
                $('#workingHoursModal').addClass('show');
            },
            error: function () {
                alert('Failed to load working hours data.');
            }
        });
    });

    $('#closeWorkingHoursModalBtn').on('click', function () {
        $('#workingHoursModal').removeClass('show');
    });

    // ── Excel COS90s ────────────────────────────────────────────────────
    $('#btn-excel-cos90').on('click', function () {
        // Clear any previous validation errors
        clearValidationErrors();

        var payload = {
            selectedProfitCentre: $('#SelectedProfitCentre').val() || '',
            selectedMonthNumber: $('#for-period-value').val() || '',
            selectedYear: $('#dpInYear').val() || '',
            pactId: $('#dpWgMemberList').val() || ''
        };

        // Client-side validation - show as popup alerts
        if (!payload.selectedProfitCentre) {
            window.showGovukAlert('Please select a Profit Centre.');
            return;
        }

        if (!payload.selectedMonthNumber) {
            window.showGovukAlert('Please select a Period.');
            return;
        }

        if (!payload.selectedYear) {
            window.showGovukAlert('Please select a Year.');
            return;
        }

        // Skip backend call if no person is selected
        if (!payload.pactId) {
            window.showGovukAlert('Please select a Person or leave blank to use Work Groups above.');
            return;
        }

        var token = $('input[name="__RequestVerificationToken"]').first().val();

        $.ajax({
            url: '/PACT/WorkGroupCos90s/ExportCos90s',
            type: 'POST',
            data: payload,
            headers: { 'RequestVerificationToken': token },
            xhrFields: { responseType: 'blob' },
            success: function (blob, _status, xhr) {
                var disposition = xhr.getResponseHeader('Content-Disposition') || '';
                var fileNameMatch = /filename\*=UTF-8''([^;]+)|filename="?([^";]+)"?/i.exec(disposition);
                var fileName = fileNameMatch
                    ? decodeURIComponent(fileNameMatch[1] || fileNameMatch[2])
                    : 'COS90.xlsx';

                var url = window.URL.createObjectURL(blob);
                var link = document.createElement('a');
                link.href = url;
                link.download = fileName;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                window.URL.revokeObjectURL(url);
            },
            error: function (xhr) {
                var contentType = xhr.getResponseHeader('Content-Type') || '';

                // Server-side validation errors - show using displayServerValidationErrors
                if (contentType.indexOf('application/json') >= 0 && xhr.response) {
                    var reader = new FileReader();
                    reader.onload = function () {
                        try {
                            var result = JSON.parse(reader.result);
                            var errors = result && result.errors ? result.errors : null;
                            var message = result && result.message ? result.message : 'There is a problem';

                            if (errors) {
                                displayServerValidationErrors(errors, message);
                                return;
                            }

                            // If no errors array, show generic message as alert
                            window.showGovukAlert(message);
                        } catch (_e) {
                            window.showGovukAlert('Failed to generate COS90 Excel.');
                        }
                    };
                    reader.readAsText(xhr.response);
                    return;
                }

                window.showGovukAlert('Failed to generate COS90 Excel.');
            }
        });
    });
});

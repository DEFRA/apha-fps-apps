var selectedProjectCode = '';

$(document).ready(function () {
    // ── Program Dropdown ─────────────────────────────────────────
    var $input = $('#programSelect');
    var $panel = $('#programDropdownPanel');
    var $search = $('#programSearchBox');
    var $rows = $('#programDropdownBody tr');

    $input.on('click', function (e) {
        e.stopPropagation();
        $panel.toggle();
        if ($panel.is(':visible')) {
            $search.val('').focus();
            $rows.show();
        }
    });

    $search.on('click', function (e) { e.stopPropagation(); });

    $search.on('input', function () {
        var term = $(this).val().toLowerCase();
        $rows.each(function () {
            var text = $(this).text().toLowerCase();
            $(this).toggle(text.indexOf(term) > -1);
        });
    });

    $(document).on('click', '#programDropdownBody tr', function () {
        var code = $(this).data('value');
        var text = $(this).find('td:first').text() + ' - ' + $(this).find('td:last').text();
        $input.val(text);
        $panel.hide();
        $('#selectedProgramNo').val(code);
        loadProgram(code);
        loadProjectsGrid(code);
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#programSelect, #programDropdownPanel').length) {
            $panel.hide();
        }
    });

    // ── Initial Load ─────────────────────────────────────────────
    var programNo = $('#selectedProgramNo').val();
    if (programNo) {
        // Set the input display text from the matching row
        var $match = $('#programDropdownBody tr[data-value="' + programNo + '"]');
        if ($match.length) {
            $input.val($match.find('td:first').text() + ' - ' + $match.find('td:last').text());
        }
        loadProgram(programNo);
        loadProjectsGrid(programNo);
    }

    // Save button
    $('#btnSaveProgram').on('click', function () {
        saveProgram();
    });

    // Project Maintenance button
    $('#projectMaintenanceBtn').on('click', function () {
        if (!selectedProjectCode) {
            alert('Please select a project first.');
            return;
        }
        window.location.href = programMaintenanceConfig.projectMaintenanceUrl +
            '/' + encodeURIComponent(selectedProjectCode);
    });
});

function loadProgram(programNo) {
    $.ajax({
        url: programMaintenanceConfig.getProgramUrl,
        type: 'GET',
        data: { programNo: programNo },
        success: function (result) {
            if (result.success) {
                var d = result.data;
                $('#Program_ProgramNo').val(d.programNo);
                $('#Program_ProgramName').val(d.programName);
                $('#Program_SectorName').val(d.sectorName);
                $('#Program_Customer').val(d.customer);
                $('#Program_Manager').val(d.manager);
                $('#Program_Minim').val(d.minim);
                $('#Program_Directorate').val(d.directorate);
                clearValidationErrors('#programDetailForm');
            }
        },
        error: function () {
            alert('An error occurred while loading the program.');
        }
    });
}

function saveProgram() {
    var data = {
        ProgramNo: $('#Program_ProgramNo').val(),
        ProgramName: $('#Program_ProgramName').val(),
        SectorName: $('#Program_SectorName').val(),
        Customer: $('#Program_Customer').val(),
        Manager: $('#Program_Manager').val(),
        Minim: $('#Program_Minim').val(),
        Directorate: $('#Program_Directorate').val()
    };

    clearValidationErrors('#programDetailForm');
    $.ajax({
        url: programMaintenanceConfig.saveProgramUrl,
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.success) {
                alert(result.message);
            } else {
                // Server returns field names without the "Program." prefix (e.g. "ProgramNo"),
                // but asp-for generates name="Program.ProgramNo", so we remap here so that
                // displayServerValidationErrors can match fields and show errors inline.
                var remappedErrors = (result.errors || []).map(function (e) {
                    return { field: e.field ? 'Program.' + e.field : e.field, message: e.message };
                });
                displayServerValidationErrors(remappedErrors, result.message, '#programDetailForm');
            }
        },
        error: function () {
            alert('An error occurred while saving.');
        }
    });
}

function loadProjectsGrid(programNo) {
    if (window['gridManager_projectsGrid']) {
        window['gridManager_projectsGrid'].reloadGrid({ page: 1 });
    }
}

function getProjectsGridExtraFilters() {
    return { programNo: $('#selectedProgramNo').val() };
}

function selectProject(btn) {
    var $row = $(btn).is('tr') ? $(btn) : $(btn).closest('tr');
    selectedProjectCode = $row.data('id');
    $('#selectedProject').val(selectedProjectCode);

    // Highlight the selected row
    $('#tbl_projectsGrid tbody tr').removeClass('selected-row');
    $row.addClass('selected-row');
}

// Auto-select the first project row after the projects grid reloads
document.addEventListener('gridReloaded', function (e) {
    if (e.detail && e.detail.gridId === 'projectsGrid') {
        var $firstRow = $('#tbl_projectsGrid tbody tr[data-id]:first');
        if ($firstRow.length) {
            selectProject($firstRow[0]);
        } else {
            selectedProjectCode = '';
            $('#selectedProject').val('');
        }
    }
});

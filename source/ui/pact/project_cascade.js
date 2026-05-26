// Project Cascade - JavaScript Handler
// This script manages project cascade display with three data grids

// Global variables
let projectDropdown = null;
let workgroupDropdown = null;
let workgroupTimeCodeDropdown = null;
let workgroupTimeRecordDropdown = null;
let jobCodeDropdown = null;
let pactStaffDropdown = null;
let projectList = [];
let timecodeList = [];
let workgroupsData = [];
let jobcodesData = [];
let jobCodesDropdownData = [];
let pactStaffData = [];
let timecodesData = [];
let timeEntriesData = [];
let jobcodesGrid = null;
let timecodesGrid = null;
let timeEntriesGrid = null;
let selectedProject = null;

/**
 * Load project list for dropdown
 */
async function loadProjectList() {
    try {
        const response = await fetch('../js/pact_js/data/portfolio-selector-grid.json');
        if (!response.ok) throw new Error('Failed to load project list');
        projectList = await response.json();
        return true;
    } catch (error) {
        console.error('Error loading project list:', error);
        projectList = [];
        return false;
    }
}

async function loadTimecodeList() {
    try {
        const response = await fetch('../js/pact_js/data/timecodes_dropdown.json');
        if (!response.ok) throw new Error('Failed to load timecode list');
        timecodeList = await response.json();
        populateSelect(document.getElementById('modal-timeCodeEntry'), timecodeList, 'code', 'code');
        return true;
    } catch (error) {
        console.error('Error loading timecode list:', error);
        timecodeList = [];
        return false;
    }
}

/**
 * Load job codes data
 */
async function loadJobcodesData() {
    try {
        const response = await fetch('../js/pact_js/data/project_cascade_jobcodes.json');
        if (!response.ok) throw new Error('Failed to load job codes data');
        jobcodesData = await response.json();
        console.log('Job codes data loaded:', jobcodesData.length, 'records');
        return true;
    } catch (error) {
        console.error('Error loading job codes data:', error);
        jobcodesData = [];
        return false;
    }
}

/**
 * Load time codes data
 */
async function loadTimecodesData() {
    try {
        const response = await fetch('../js/pact_js/data/project_cascade_timecodes.json');
        if (!response.ok) throw new Error('Failed to load time codes data');
        timecodesData = await response.json();
        console.log('Time codes data loaded:', timecodesData.length, 'records');
        return true;
    } catch (error) {
        console.error('Error loading time codes data:', error);
        timecodesData = [];
        return false;
    }
}

/**
 * Load time entries data
 */
async function loadTimeEntriesData() {
    try {
        const response = await fetch('../js/pact_js/data/pact_time_entries.json');
        if (!response.ok) throw new Error('Failed to load time entries data');
        timeEntriesData = await response.json();
        console.log('Time entries data loaded:', timeEntriesData.length, 'records');
        return true;
    } catch (error) {
        console.error('Error loading time entries data:', error);
        timeEntriesData = [];
        return false;
    }
}

/**
 * Load workgroups data for dropdown
 */
async function loadWorkgroupsData() {
    try {
        const response = await fetch('../js/pact_js/data/workgroup_person_selection.json');
        if (!response.ok) throw new Error('Failed to load workgroups data');
        const data = await response.json();
        workgroupsData = data.workgroups || [];
        console.log('Workgroups data loaded:', workgroupsData.length, 'records');
        return true;
    } catch (error) {
        console.error('Error loading workgroups data:', error);
        workgroupsData = [];
        return false;
    }
}

/**
 * Load job codes dropdown data
 */
async function loadJobCodesDropdownData() {
    try {
        const response = await fetch('../js/pact_js/data/jobcodes_dropdown.json');
        if (!response.ok) throw new Error('Failed to load job codes dropdown data');
        jobCodesDropdownData = await response.json();
        console.log('Job codes dropdown data loaded:', jobCodesDropdownData.length, 'records');
        return true;
    } catch (error) {
        console.error('Error loading job codes dropdown data:', error);
        jobCodesDropdownData = [];
        return false;
    }
}

/**
 * Load PACT staff list data
 */
async function loadPactStaffData() {
    try {
        const response = await fetch('../js/pact_js/data/pact_staff_list.json');
        if (!response.ok) throw new Error('Failed to load PACT staff data');
        pactStaffData = await response.json();
        console.log('PACT staff data loaded:', pactStaffData.length, 'records');
        return true;
    } catch (error) {
        console.error('Error loading PACT staff data:', error);
        pactStaffData = [];
        return false;
    }
}

/**
 * Initialize first data grid - Job Codes Belongs to Project
 */
function initializeJobcodesGrid() {
    const columns = [
        { field: 'jobCode', header: 'Job Code', width: 100, sortable: true },
        { field: 'name', header: 'Name', width: 300, sortable: true },
        { field: 'project', header: 'Project', width: 100, sortable: true },
        { field: 'wkfG', header: 'Work Group', width: 100, sortable: true },
        { field: 'newProgram', header: 'New Program', width: 100, sortable: true },
        {
            field: 'actions',
            header: 'Actions',
            sortable: false,
            width: 100,
            render: function(value, row) {
                return (
                    '<div class="sup_text_center">' +
                    '<button type="button" onclick="editJobcode(' + row.id + ')"' +
                    ' style="background:none;border:none;cursor:pointer;padding:0px 4px;"' +
                    ' aria-label="Edit job code ' + row.jobCode + '">' +
                    '<img src="../images/pen-to-square-regular-full.svg" alt="Edit" width="20"></button>' +
                    '<button type="button" onclick="deleteJobcode(' + row.id + ', \'' + row.jobCode + '\')"' +
                    ' style="background:none;border:none;cursor:pointer;padding:0px 4px;"' +
                    ' aria-label="Delete job code ' + row.jobCode + '">' +
                    '<img src="../images/trash-can-regular-full.svg" alt="Delete" width="20"></button>' +
                    '</div>'
                );
            }
        }
    ];

    jobcodesGrid = new DataGridComponent({
        gridId: 'JobcodeBelongsToProjectGrid',
        containerSelector: '#gridContainer_JobcodeBelongsToProjectGrid',
        title: 'Job Codes Belonging to Project',
        columns: columns,
        data: [], // Start with empty data - will populate on project selection
        pageSize: 5,
        enableSort: true,
        enableResize: true,
        enableSelection: false,
        enablePagination: true,
        showAddButton: true,
        containerMinHeight: '200px',     // Full CSS value
        scrollContainerHeight: '200px',  
        pageSizeOptions: [5, 10, 15, 20],
        callbacks: {
            onAdd: openAddJobcodeModal
        }
    });

    console.log('Job codes grid initialized (empty - awaiting project selection)');
}

/**
 * Initialize second data grid - Time Code Valid Options
 */
function initializeTimecodesGrid() {
    const columns = [
        { field: 'timeCode', header: 'Time Code', width: 100, sortable: true },
        { field: 'wrkGrp', header: 'Work Group', width: 100, sortable: true },
        { field: 'project', header: 'Project', width: 100, sortable: true },
        { field: 'jobCode', header: 'Job Code', width: 100, sortable: true },
        { 
            field: 'active', 
            header: 'Active', 
            width: 80, 
            sortable: true,
            render: function(value, row, index) {
                return `<div class="govuk-checkboxes govuk-checkboxes--small" data-module="govuk-checkboxes">
                    <div class="govuk-checkboxes__item">
                        <input class="govuk-checkboxes__input" id="selectRow${index}" type="checkbox" ${value ? "checked" : ""} disabled/>
                        <label class="govuk-label govuk-checkboxes__label sup_label_auto_width" for="selectRow${index}" style="padding: 0;">  </label>   
                    </div> 
                </div>`;
            }
        },
        {
            field: 'actions',
            header: 'Actions',
            sortable: false,
            width: 100,
            render: function(value, row) {
                return (
                    '<div class="sup_text_center">' +
                    '<button type="button" onclick="editTimecode(' + row.id + ')"' +
                    ' style="background:none;border:none;cursor:pointer;padding:0 4px;"' +
                    ' aria-label="Edit time code ' + row.timeCode + '">' +
                    '<img src="../images/pen-to-square-regular-full.svg" alt="Edit" width="20"></button>' +
                    '<button type="button" onclick="deleteTimecode(' + row.id + ', \'' + row.timeCode + '\')"' +
                    ' style="background:none;border:none;cursor:pointer;padding:0 4px;"' +
                    ' aria-label="Delete time code ' + row.timeCode + '">' +
                    '<img src="../images/trash-can-regular-full.svg" alt="Delete" width="20"></button>' +
                    '</div>'
                );
            }
        }
    ];

    timecodesGrid = new DataGridComponent({
        gridId: 'TimeCodeValidOptionGrid',
        containerSelector: '#gridContainer_TimeCodeValidOptionGrid',
        title: 'Time Code Valid Options',
        columns: columns,
        data: [], // Start with empty data - will populate on job code selection
        pageSize: 5,
        enableSort: true,
        enableResize: true,
        enableSelection: false,
        enablePagination: true,
        showAddButton: true,
        containerMinHeight: '200px',     // Full CSS value
        scrollContainerHeight: '200px',  
        pageSizeOptions: [5, 10, 15, 20],
        callbacks: {
            onAdd: openAddTimecodeModal
        }
    });

    console.log('Time codes grid initialized (empty - awaiting job code selection)');
}

/**
 * Initialize third data grid - Time Records
 */
function initializeTimeEntriesGrid() {
    const columns = [
        { 
            field: 'pactStaffId', 
            header: 'PACT Staff ID', 
            width: 120, 
            sortable: true,
            render: function(value) {
                return '<div style="text-align: left;">' + (value || '') + '</div>';
            }
        },
        { field: 'timeCode', header: 'Time Code', width: 100, sortable: true },
        { 
            field: 'month', 
            header: 'Month', 
            width: 80, 
            sortable: true,
            render: function(value) {
                return '<div style="text-align: right;">' + (value || '') + '</div>';
            }
        },
        { field: 'parentProject', header: 'Parent Project', width: 120, sortable: true },
        { field: 'workGroup', header: 'Work Group', width: 100, sortable: true },
        { 
            field: 'hours', 
            header: 'Hours', 
            width: 80, 
            sortable: true,
            render: function(value) {
                return '<div style="text-align: right;">' + (value || '') + '</div>';
            }
        },
        {
            field: 'actions',
            header: 'Actions',
            sortable: false,
            width: 100,
            render: function(value, row) {
                return (
                    '<div class="sup_text_center">' +
                    '<button type="button" onclick="editTimeEntry(' + row.id + ')"' +
                    ' style="background:none;border:none;cursor:pointer;padding:0 4px;"' +
                    ' aria-label="Edit time entry ' + row.id + '">' +
                    '<img src="../images/pen-to-square-regular-full.svg" alt="Edit" width="20"></button>' +
                    '<button type="button" onclick="deleteTimeEntry(' + row.id + ', ' + row.pactStaffId + ')"' +
                    ' style="background:none;border:none;cursor:pointer;padding:0 4px;"' +
                    ' aria-label="Delete time entry ' + row.id + '">' +
                    '<img src="../images/trash-can-regular-full.svg" alt="Delete" width="20"></button>' +
                    '</div>'
                );
            }
        }
    ];

    timeEntriesGrid = new DataGridComponent({
        gridId: 'TimeRecordsGrid',
        containerSelector: '#gridContainer_TimeRecordsGrid',
        title: 'Time Records',
        columns: columns,
        data: [], // Start with empty data - will populate on time code selection
        pageSize: 5,
        enableSort: true,
        enableResize: true,
        enableSelection: false,
        enablePagination: true,
        showAddButton: true,
        containerMinHeight: '200px',     // Full CSS value
        scrollContainerHeight: '200px',  
        pageSizeOptions: [5, 10, 15, 20],
        callbacks: {
            onAdd: openAddTimeentryModal
        }
    });

    console.log('Time entries grid initialized (empty - awaiting time code selection)');
}

/**
 * Setup row click handler for Job Codes grid
 * When a row is clicked, check if jobCode exists in timecodes data and filter the Time Codes grid
 */
function setupJobcodesGridRowClickHandler() {
    const tableBody = document.getElementById('JobcodeBelongsToProjectGrid_tableBody');
    
    if (!tableBody) {
        // If the table body doesn't exist yet, try again after a short delay
        setTimeout(setupJobcodesGridRowClickHandler, 200);
        return;
    }

    // Remove any existing listeners to prevent duplicates
    const existingHandler = tableBody.getAttribute('data-handler-attached');
    if (existingHandler === 'true') {
        console.log('Job codes grid row click handler already attached');
        return;
    }

    tableBody.addEventListener('click', function(e) {
        const row = e.target.closest('tr');
        if (!row || row.parentElement.id !== 'JobcodeBelongsToProjectGrid_tableBody') return;
        
        // Get all cells in the row
        const cells = row.querySelectorAll('td');
        if (cells.length >= 4) {
            // The jobCode is in the first column (index 0)
            // The wkfG (Work Group) is in the fourth column (index 3)
            const jobCodeValue = cells[0].textContent.trim();
            const workGroupValue = cells[3].textContent.trim();
            
            console.log('Job code row clicked - JobCode:', jobCodeValue, 'Work Group:', workGroupValue);
            
            // Update the selected jobcode textbox
            const txtSelectedJobcode = document.getElementById('txtSelectedJobcode');
            if (txtSelectedJobcode) {
                txtSelectedJobcode.value = jobCodeValue;
            }

            const txtSelectedTimeCode = document.getElementById('txtSelectedTimeCode');
            if (txtSelectedTimeCode) {
                txtSelectedTimeCode.value = jobCodeValue;
            }
            
            // Set the Work Group value in the dropdown
            if (workgroupDropdown && workGroupValue) {
                console.log('Attempting to set workgroup dropdown to:', workGroupValue);
                console.log('Dropdown instance:', workgroupDropdown);
                console.log('Dropdown data:', workgroupDropdown.originalData);
                
                // Try to set the value
                try {
                    workgroupDropdown.setValue(workGroupValue);
                    console.log('Successfully set workgroup dropdown to:', workGroupValue);
                    
                    // Verify it was set
                    const currentValue = workgroupDropdown.getValue();
                    console.log('Current dropdown value after setting:', currentValue);
                } catch (error) {
                    console.error('Error setting workgroup dropdown:', error);
                }
            } else {
                if (!workgroupDropdown) {
                    console.warn('Workgroup dropdown not initialized yet');
                }
                if (!workGroupValue) {
                    console.warn('Work Group value is empty');
                }
            }
            
            // Check if this jobCode exists in project_cascade_timecodes.json data
            const filteredTimecodes = timecodesData.filter(item => item.jobCode === jobCodeValue);
            
            if (filteredTimecodes.length > 0) {
                // JobCode exists in timecodes data - display matching records
                timecodesGrid.updateData(filteredTimecodes);
                console.log(`JobCode "${jobCodeValue}" found in timecodes data - displaying ${filteredTimecodes.length} record(s)`);
                
                // Auto-select first row of time codes grid
                autoSelectFirstRow('TimeCodeValidOptionGrid');
            } else {
                // JobCode does not exist in timecodes data - show empty grid
                timecodesGrid.updateData([]);
                console.log(`JobCode "${jobCodeValue}" not found in timecodes data - no records to display`);
            }
            
            // Clear third grid when first grid is clicked
            timeEntriesGrid.updateData([]);
            
            // Highlight selected row
            const parentBody = row.parentElement;
            parentBody.querySelectorAll('tr').forEach(tr => tr.classList.remove('selected-row'));
            row.classList.add('selected-row');
        }
    });
    
    tableBody.setAttribute('data-handler-attached', 'true');
    console.log('Job codes grid row click handler attached');
}

/**
 * Setup row click handler for Time Codes grid
 * When a row is clicked, check if timeCode and workGroup exist in time entries data and filter the Time Entries grid
 */
function setupTimecodesGridRowClickHandler() {
    const tableBody = document.getElementById('TimeCodeValidOptionGrid_tableBody');
    
    if (!tableBody) {
        // If the table body doesn't exist yet, try again after a short delay
        setTimeout(setupTimecodesGridRowClickHandler, 200);
        return;
    }

    // Remove any existing listeners to prevent duplicates
    const existingHandler = tableBody.getAttribute('data-handler-attached');
    if (existingHandler === 'true') {
        console.log('Time codes grid row click handler already attached');
        return;
    }

    tableBody.addEventListener('click', function(e) {
        const row = e.target.closest('tr');
        if (!row || row.parentElement.id !== 'TimeCodeValidOptionGrid_tableBody') return;
        
        // Get all cells in the row
        const cells = row.querySelectorAll('td');
        if (cells.length >= 2) {
            // The timeCode is in the first column (index 0)
            // The wrkGrp is in the second column (index 1)
            // The project is in the third column (index 2)
            const timeCodeValue = cells[0].textContent.trim();
            const wrkGrpValue = cells[1].textContent.trim();
            const projectValue = cells.length >= 3 ? cells[2].textContent.trim() : '';
            
            console.log('Time code row clicked - TimeCode:', timeCodeValue, 'WrkGrp:', wrkGrpValue);
            
            // Update the selected textboxes
            const txtSelectedWorkGroup = document.getElementById('txtSelectedWorkGroup');
            const txtSelectedTimeCode = document.getElementById('txtSelectedTimeCode');
            const txtSelectedProjectcodeTwo = document.getElementById('txtSelectedProjectcodeTwo');
            
            if (txtSelectedWorkGroup) {
                txtSelectedWorkGroup.value = wrkGrpValue;
            }
            if (txtSelectedTimeCode) {
                txtSelectedTimeCode.value = timeCodeValue;
            }
            if (txtSelectedProjectcodeTwo) {
                txtSelectedProjectcodeTwo.value = projectValue;
            }
            
            // Check if this timeCode AND workGroup combination exists in pact_time_entries.json data
            const filteredEntries = timeEntriesData.filter(item => 
                item.timeCode === timeCodeValue && item.workGroup === wrkGrpValue
            );
            
            if (filteredEntries.length > 0) {
                
                // Auto-select first row of time entries grid (just highlight, no further action)
                setTimeout(() => {
                    const timeEntriesTableBody = document.getElementById('TimeRecordsGrid_tableBody');
                    if (timeEntriesTableBody) {
                        const firstRow = timeEntriesTableBody.querySelector('tr');
                        if (firstRow) {
                            firstRow.classList.add('selected-row');
                        }
                    }
                }, 150);
                // TimeCode and WorkGroup combination exists - display matching records
                timeEntriesGrid.updateData(filteredEntries);
                console.log(`TimeCode "${timeCodeValue}" with WorkGroup "${wrkGrpValue}" found in time entries data - displaying ${filteredEntries.length} record(s)`);
            } else {
                // Combination does not exist in time entries data - show empty grid
                timeEntriesGrid.updateData([]);
                console.log(`TimeCode "${timeCodeValue}" with WorkGroup "${wrkGrpValue}" not found in time entries data - no records to display`);
            }
            
            // Highlight selected row
            const parentBody = row.parentElement;
            parentBody.querySelectorAll('tr').forEach(tr => tr.classList.remove('selected-row'));
            row.classList.add('selected-row');
        }
    });
    
    tableBody.setAttribute('data-handler-attached', 'true');
    console.log('Time codes grid row click handler attached');
}

/**
 * Initialize project dropdown
 */
async function initializeProjectDropdown() {
    const loaded = await loadProjectList(); 
    if (loaded) {
        projectDropdown = new MultiColumnDropdownComponent({
            dropdownId: 'projectDropdown',
            containerSelector: '#projectSelectDropdown',
            placeholder: 'Select Project',
            showSerialNumber: false,
            searchPlaceholder: 'Search by project name',
            labelText: '',
            columns: [
                { field: 'code', header: 'Project Code', width: '80px' },
                { field: 'title', header: 'Project Name', width: '120px' }
            ],
            data: projectList?.portfolios || [],
            displayField: 'code',
            valueField: 'code',
            callbacks: {
                onSelect: function(selectedItem, dropdown) {
                    selectedProject = selectedItem.code;
                    console.log(`Selected project: ${selectedItem.title} (ID: ${selectedItem.code})`);
                    
                    // Update the selected project textboxes
                    const txtSelectedProject = document.getElementById('txtSelectedProjectcode');
                    const txtSelectedProjectcodeTwo = document.getElementById('txtSelectedProjectcodeTwo');
                    const txtSelectedProjectTitle = document.getElementById('txtSelectedProjectTitle');
                    if (txtSelectedProject) {
                        txtSelectedProject.value = selectedItem.code;
                        if (txtSelectedProjectcodeTwo) {
                            txtSelectedProjectcodeTwo.value = selectedItem.code; // Update second project code textbox
                        }
                    }
                    if (txtSelectedProjectTitle) {
                        txtSelectedProjectTitle.value = selectedItem.title;
                    }
                    
                    // Filter grids based on selected project
                    filterGridsByProject(selectedItem.code);
                }
            }
        });
    }
}

/**
 * Auto-select first row of a grid
 */
function autoSelectFirstRow(gridId, clickHandler) {
    setTimeout(() => {
        const tableBody = document.getElementById(gridId + '_tableBody');
        if (tableBody) {
            const firstRow = tableBody.querySelector('tr');
            if (firstRow) {
                firstRow.click();
                console.log(`Auto-selected first row of ${gridId}`);
            }
        }
    }, 150);
}

/**
 * Filter all grids based on selected project
 */
function filterGridsByProject(projectCode) {
    // Filter job codes grid (first grid)
    if (jobcodesGrid) {
        const filteredJobcodes = jobcodesData.filter(item => item.project === projectCode);
        jobcodesGrid.updateData(filteredJobcodes);
        console.log('Filtered job codes:', filteredJobcodes.length, 'records');
        
        // Auto-select first row of job codes grid
        if (filteredJobcodes.length > 0) {
            autoSelectFirstRow('JobcodeBelongsToProjectGrid');
        }
    }
    
    // Clear second and third grids since no job code is selected yet
    if (timecodesGrid) {
        timecodesGrid.updateData([]);
        console.log('Time codes grid cleared - awaiting job code selection');
    }
    
    if (timeEntriesGrid) {
        timeEntriesGrid.updateData([]);
        console.log('Time entries grid cleared - awaiting time code selection');
    }
}

/**
 * Action Handlers for Job Codes Grid
 */
let editingJobcodeId = null;

function editJobcode(id) {
    console.log('Edit job code with ID:', id);
    const jobcode = jobcodesData.find(item => item.id === id);
    if (jobcode) {
        editingJobcodeId = id;
        document.getElementById('jobcodeModalLabel').textContent = 'Edit Job Code';
        
        // Clear error messages
        const dbError = document.getElementById('formJobcode-db-error');
        if (dbError) {
            dbError.hidden = true;
            const errorMsg = document.getElementById('formJobcode-db-error-msg');
            if (errorMsg) errorMsg.textContent = '';
        }
        
        // Clear individual field errors
        ['jobCode', 'project'].forEach(field => {
            const fieldError = document.getElementById('modal-' + field + '-error');
            const fieldErrorMsg = document.getElementById('modal-' + field + '-error-msg');
            const fieldFormGroup = document.getElementById('fg-' + field);
            if (fieldError) fieldError.hidden = true;
            if (fieldErrorMsg) fieldErrorMsg.textContent = '';
            if (fieldFormGroup) fieldFormGroup.classList.remove('govuk-form-group--error');
        });
        
        document.getElementById('modal-jobCode').value = jobcode.jobCode || '';
        document.getElementById('modal-jobCode').disabled = true;
        document.getElementById('modal-name').value = jobcode.name || '';
        document.getElementById('modal-project').value = jobcode.project || '';
        document.getElementById('modal-wkfG').value = jobcode.wkfG || '';
        document.getElementById('modal-newProgram').value = jobcode.newProgram || '';
        
        // Set workgroup dropdown value
        const workgroupDropdownInput = document.getElementById('workgroupDropdown_input');
        if (workgroupDropdownInput && jobcode.wkfG) {
            workgroupDropdownInput.value = jobcode.wkfG;
        }
        
        document.getElementById('jobcodeSaveBtn').style.display = 'none';
        document.getElementById('jobcodeUpdateBtn').style.display = '';
        
        document.getElementById('jobcodeModal').style.display = 'flex';
    }
}

function openAddJobcodeModal() {
    editingJobcodeId = null;
    document.getElementById('jobcodeModalLabel').textContent = 'Add Job Code';
    document.getElementById('formJobcode').reset();
    
    // Enable job code field for adding new records
    document.getElementById('modal-jobCode').disabled = false;
    
    // Clear error messages
    const dbError = document.getElementById('formJobcode-db-error');
    if (dbError) {
        dbError.hidden = true;
        const errorMsg = document.getElementById('formJobcode-db-error-msg');
        if (errorMsg) errorMsg.textContent = '';
    }
    
    // Clear individual field errors
    ['jobCode', 'name', 'project', 'wkfG', 'newProgram'].forEach(field => {
        const fieldError = document.getElementById('modal-' + field + '-error');
        const fieldErrorMsg = document.getElementById('modal-' + field + '-error-msg');
        const fieldFormGroup = document.getElementById('fg-' + field);
        if (fieldError) fieldError.hidden = true;
        if (fieldErrorMsg) fieldErrorMsg.textContent = '';
        if (fieldFormGroup) fieldFormGroup.classList.remove('govuk-form-group--error');
    });
    
    // Pre-fill project with selected project
    const txtSelectedProjectcode = document.getElementById('txtSelectedProjectcode');
    if (txtSelectedProjectcode && txtSelectedProjectcode.value) {
        document.getElementById('modal-project').value = txtSelectedProjectcode.value;
    }
    
    // Clear workgroup dropdown
    const workgroupDropdownInput = document.getElementById('workgroupDropdown_input');
    if (workgroupDropdownInput) {
        workgroupDropdownInput.value = '';
    }
    document.getElementById('modal-wkfG').value = '';
    
    document.getElementById('jobcodeSaveBtn').style.display = '';
    document.getElementById('jobcodeUpdateBtn').style.display = 'none';
    document.getElementById('jobcodeModal').style.display = 'flex';
}

function closeJobcodeModal() {
    // Clear error messages
    const dbError = document.getElementById('formJobcode-db-error');
    if (dbError) {
        dbError.hidden = true;
        const errorMsg = document.getElementById('formJobcode-db-error-msg');
        if (errorMsg) errorMsg.textContent = '';
    }
    
    // Clear individual field errors
    ['jobCode', 'project'].forEach(field => {
        const fieldError = document.getElementById('modal-' + field + '-error');
        const fieldErrorMsg = document.getElementById('modal-' + field + '-error-msg');
        const fieldFormGroup = document.getElementById('fg-' + field);
        if (fieldError) fieldError.hidden = true;
        if (fieldErrorMsg) fieldErrorMsg.textContent = '';
        if (fieldFormGroup) fieldFormGroup.classList.remove('govuk-form-group--error');
    });
    
    document.getElementById('jobcodeModal').style.display = 'none';
    editingJobcodeId = null;
}

function saveJobcode() {
    // Clear any previous error messages
    const dbError = document.getElementById('formJobcode-db-error');
    const dbErrorMsg = document.getElementById('formJobcode-db-error-msg');
    if (dbError) dbError.hidden = true;
    if (dbErrorMsg) dbErrorMsg.textContent = '';
    
    // Clear individual field errors
    ['jobCode', 'project'].forEach(field => {
        const fieldError = document.getElementById('modal-' + field + '-error');
        const fieldErrorMsg = document.getElementById('modal-' + field + '-error-msg');
        const fieldFormGroup = document.getElementById('fg-' + field);
        if (fieldError) fieldError.hidden = true;
        if (fieldErrorMsg) fieldErrorMsg.textContent = '';
        if (fieldFormGroup) fieldFormGroup.classList.remove('govuk-form-group--error');
    });
    
    const jobCode = document.getElementById('modal-jobCode').value.trim();
    const name = document.getElementById('modal-name').value.trim();
    const project = document.getElementById('modal-project').value.trim();
    const wkfG = document.getElementById('modal-wkfG').value.trim();
    const newProgram = document.getElementById('modal-newProgram').value.trim();
    
    // Validate required fields
    let hasError = false;
    const errors = [];
    
    if (!jobCode) {
        const fieldError = document.getElementById('modal-jobCode-error');
        const fieldErrorMsg = document.getElementById('modal-jobCode-error-msg');
        const fieldFormGroup = document.getElementById('fg-jobCode');
        if (fieldError && fieldErrorMsg && fieldFormGroup) {
            fieldErrorMsg.textContent = 'Job Code is required';
            fieldError.hidden = false;
            fieldFormGroup.classList.add('govuk-form-group--error');
            errors.push('Job Code is required');
            hasError = true;
        }
    }
    
    if (!project) {
        const fieldError = document.getElementById('modal-project-error');
        const fieldErrorMsg = document.getElementById('modal-project-error-msg');
        const fieldFormGroup = document.getElementById('fg-project');
        if (fieldError && fieldErrorMsg && fieldFormGroup) {
            fieldErrorMsg.textContent = 'Project is required';
            fieldError.hidden = false;
            fieldFormGroup.classList.add('govuk-form-group--error');
            errors.push('Project is required');
            hasError = true;
        }
    }
    
    if (hasError) {
        if (dbError && dbErrorMsg) {
            dbErrorMsg.textContent = 'Please fill in all required fields';
            dbError.hidden = false;
        }
        return;
    }
    
    if (editingJobcodeId) {
        // Update existing
        const index = jobcodesData.findIndex(item => item.id === editingJobcodeId);
        if (index !== -1) {
            jobcodesData[index] = {
                ...jobcodesData[index],
                jobCode: jobCode,
                name: name,
                project: project,
                wkfG: wkfG,
                newProgram: newProgram
            };
            console.log('Job code updated:', jobcodesData[index]);
        }
    } else {
        // Add new
        const newJobcode = {
            id: jobcodesData.length > 0 ? Math.max(...jobcodesData.map(j => j.id)) + 1 : 1,
            jobCode: jobCode,
            name: name,
            project: project,
            wkfG: wkfG,
            newProgram: newProgram
        };
        jobcodesData.push(newJobcode);
        console.log('Job code added:', newJobcode);
    }
    
    // Refresh grid if current project matches
    if (selectedProject) {
        filterGridsByProject(selectedProject);
    } else {
        jobcodesGrid.updateData(jobcodesData);
    }
    
    closeJobcodeModal();
}

let pendingDeleteData = null;

function deleteJobcode(id, jobCodeValue) {
    console.log('Delete job code with ID:', id, 'JobCode:', jobCodeValue);
    
    // Check if this job code has related records in time codes grid
    const relatedTimeCodes = timecodesData.filter(item => item.jobCode === jobCodeValue);
    
    if (relatedTimeCodes.length > 0) {
        alert('This Jobcode has related records in TimeCodeValid, delete aborted');
        console.log('Delete aborted: Found', relatedTimeCodes.length, 'related time code(s) for job code', jobCodeValue);
        return;
    }
    
    pendingDeleteData = { id: id, value: jobCodeValue, type: 'jobcode' };
    document.getElementById('deleteMessage').textContent = 'Are you sure you want to delete job code "' + jobCodeValue + '"?';
    document.getElementById('deleteModal').style.display = 'flex';
}

/**
 * Action Handlers for Time Codes Grid
 */
let editingTimecodeId = null;

function editTimecode(id) {
    console.log('Edit time code with ID:', id);
    const timecode = timecodesData.find(item => item.id === id);
    if (timecode) {
        editingTimecodeId = id;
        document.getElementById('timecodeModalLabel').textContent = 'Edit Time Code';
        
        // Clear error messages
        const dbError = document.getElementById('formTimecode-db-error');
        if (dbError) {
            dbError.hidden = true;
            const errorMsg = document.getElementById('formTimecode-db-error-msg');
            if (errorMsg) errorMsg.textContent = '';
        }
        
        // Clear individual field errors
        ['timeCode', 'wrkGrp', 'projectTimecode', 'jobCodeTimecode'].forEach(field => {
            const fieldError = document.getElementById('modal-' + field + '-error');
            const fieldErrorMsg = document.getElementById('modal-' + field + '-error-msg');
            let fieldFormGroup;
            if (field === 'projectTimecode') {
                fieldFormGroup = document.getElementById('fg-projectTimecode');
            } else if (field === 'jobCodeTimecode') {
                fieldFormGroup = document.getElementById('fg-jobCodeTimecode');
            } else {
                fieldFormGroup = document.getElementById('fg-' + field);
            }
            if (fieldError) fieldError.hidden = true;
            if (fieldErrorMsg) fieldErrorMsg.textContent = '';
            if (fieldFormGroup) fieldFormGroup.classList.remove('govuk-form-group--error');
        });
        
        document.getElementById('modal-timeCode').disabled = true;
        document.getElementById('modal-wrkGrp').disabled = true;
        document.getElementById('modal-projectTimecode').disabled = true;

        document.getElementById('modal-timeCode').value = timecode.timeCode || '';
        document.getElementById('modal-wrkGrp').value = timecode.wrkGrp || '';
        document.getElementById('modal-projectTimecode').value = timecode.project || '';
        document.getElementById('modal-jobCodeTimecode').value = timecode.jobCode || '';
        document.getElementById('modal-active').checked = timecode.active || false;
        
        // Set workgroup dropdown value
        const workgroupTimeCodeDropdownInput = document.getElementById('workgroupTimeCodeDropdown_input');
        if (workgroupTimeCodeDropdownInput && timecode.wrkGrp) {
            workgroupTimeCodeDropdownInput.value = timecode.wrkGrp;
        }
        
        // Disable workgroup dropdown when editing
        if (workgroupTimeCodeDropdown) {
            workgroupTimeCodeDropdown.disable();
        }
        
        // Set job code dropdown value
        const jobCodeDropdownInput = document.getElementById('jobCodeDropdown_input');
        if (jobCodeDropdownInput && timecode.jobCode) {
            jobCodeDropdownInput.value = timecode.jobCode;
        }
        
        document.getElementById('timecodeSaveBtn').style.display = 'none';
        document.getElementById('timecodeUpdateBtn').style.display = '';
        
        document.getElementById('timecodeModal').style.display = 'flex';
    }
}

function openAddTimecodeModal() {
    editingTimecodeId = null;
    document.getElementById('timecodeModalLabel').textContent = 'Add Time Code';
    document.getElementById('formTimecode').reset();
    
    // Enable fields for adding new records
    document.getElementById('modal-timeCode').disabled = false;
    document.getElementById('modal-projectTimecode').disabled = false;
    const workgroupTimeCodeDropdownInput = document.getElementById('workgroupTimeCodeDropdown_input');
    if (workgroupTimeCodeDropdownInput) {
        workgroupTimeCodeDropdownInput.disabled = false;
    }
    
    // Enable workgroup dropdown when adding
    if (workgroupTimeCodeDropdown) {
        workgroupTimeCodeDropdown.enable();
    }
    
    // Clear error messages
    const dbError = document.getElementById('formTimecode-db-error');
    if (dbError) {
        dbError.hidden = true;
        const errorMsg = document.getElementById('formTimecode-db-error-msg');
        if (errorMsg) errorMsg.textContent = '';
    }
    
    // Clear individual field errors
    ['timeCode', 'wrkGrp', 'projectTimecode', 'jobCodeTimecode'].forEach(field => {
        const fieldError = document.getElementById('modal-' + field + '-error');
        const fieldErrorMsg = document.getElementById('modal-' + field + '-error-msg');
        let fieldFormGroup;
        if (field === 'projectTimecode') {
            fieldFormGroup = document.getElementById('fg-projectTimecode');
        } else if (field === 'jobCodeTimecode') {
            fieldFormGroup = document.getElementById('fg-jobCodeTimecode');
        } else {
            fieldFormGroup = document.getElementById('fg-' + field);
        }
        if (fieldError) fieldError.hidden = true;
        if (fieldErrorMsg) fieldErrorMsg.textContent = '';
        if (fieldFormGroup) fieldFormGroup.classList.remove('govuk-form-group--error');
    });
    
    // Pre-fill project and jobCode with selected values
    const txtSelectedProjectcode = document.getElementById('txtSelectedProjectcode');
    const txtSelectedJobcode = document.getElementById('txtSelectedJobcode');
    
    if (txtSelectedProjectcode && txtSelectedProjectcode.value) {
        document.getElementById('modal-projectTimecode').value = txtSelectedProjectcode.value;
    }
    if (txtSelectedJobcode && txtSelectedJobcode.value) {
        const jobCodeValue = txtSelectedJobcode.value;
        document.getElementById('modal-jobCodeTimecode').value = jobCodeValue;
        // Set timeCode to same value as jobCode
       // document.getElementById('modal-timeCode').value = jobCodeValue;
    }
    
    // Clear workgroup dropdown
    //const workgroupTimeCodeDropdownInput = document.getElementById('workgroupTimeCodeDropdown_input');
    //if (workgroupTimeCodeDropdownInput) {
        workgroupTimeCodeDropdownInput.value = '';
   // }
    document.getElementById('modal-wrkGrp').value = '';
    
    document.getElementById('timecodeSaveBtn').style.display = '';
    document.getElementById('timecodeUpdateBtn').style.display = 'none';
    document.getElementById('timecodeModal').style.display = 'flex';
}

function closeTimecodeModal() {
    // Clear error messages
    const dbError = document.getElementById('formTimecode-db-error');
    if (dbError) {
        dbError.hidden = true;
        const errorMsg = document.getElementById('formTimecode-db-error-msg');
        if (errorMsg) errorMsg.textContent = '';
    }
    
    // Clear individual field errors
    ['timeCode', 'wrkGrp', 'projectTimecode', 'jobCodeTimecode'].forEach(field => {
        const fieldError = document.getElementById('modal-' + field + '-error');
        const fieldErrorMsg = document.getElementById('modal-' + field + '-error-msg');
        let fieldFormGroup;
        if (field === 'projectTimecode') {
            fieldFormGroup = document.getElementById('fg-projectTimecode');
        } else if (field === 'jobCodeTimecode') {
            fieldFormGroup = document.getElementById('fg-jobCodeTimecode');
        } else {
            fieldFormGroup = document.getElementById('fg-' + field);
        }
        if (fieldError) fieldError.hidden = true;
        if (fieldErrorMsg) fieldErrorMsg.textContent = '';
        if (fieldFormGroup) fieldFormGroup.classList.remove('govuk-form-group--error');
    });
    
    document.getElementById('timecodeModal').style.display = 'none';
    editingTimecodeId = null;
}

function saveTimecode() {
    // Clear any previous error messages
    const dbError = document.getElementById('formTimecode-db-error');
    const dbErrorMsg = document.getElementById('formTimecode-db-error-msg');
    if (dbError) dbError.hidden = true;
    if (dbErrorMsg) dbErrorMsg.textContent = '';
    
    // Clear individual field errors
    ['timeCode', 'wrkGrp', 'projectTimecode', 'jobCodeTimecode'].forEach(field => {
        const fieldError = document.getElementById('modal-' + field + '-error');
        const fieldErrorMsg = document.getElementById('modal-' + field + '-error-msg');
        let fieldFormGroup;
        if (field === 'projectTimecode') {
            fieldFormGroup = document.getElementById('fg-projectTimecode');
        } else if (field === 'jobCodeTimecode') {
            fieldFormGroup = document.getElementById('fg-jobCodeTimecode');
        } else {
            fieldFormGroup = document.getElementById('fg-' + field);
        }
        if (fieldError) fieldError.hidden = true;
        if (fieldErrorMsg) fieldErrorMsg.textContent = '';
        if (fieldFormGroup) fieldFormGroup.classList.remove('govuk-form-group--error');
    });
    
    const timeCode = document.getElementById('modal-timeCode').value.trim();
    const wrkGrp = document.getElementById('modal-wrkGrp').value.trim();
    const project = document.getElementById('modal-projectTimecode').value.trim();
    const jobCode = document.getElementById('modal-jobCodeTimecode').value.trim();
    const active = document.getElementById('modal-active').checked;
    
    // Validate required fields
    let hasError = false;
    const errors = [];
    
    if (!timeCode) {
        const fieldError = document.getElementById('modal-timeCode-error');
        const fieldErrorMsg = document.getElementById('modal-timeCode-error-msg');
        const fieldFormGroup = document.getElementById('fg-timeCode');
        if (fieldError && fieldErrorMsg && fieldFormGroup) {
            fieldErrorMsg.textContent = 'Time Code is required';
            fieldError.hidden = false;
            fieldFormGroup.classList.add('govuk-form-group--error');
            errors.push('Time Code is required');
            hasError = true;
        }
    }
    
    if (!wrkGrp) {
        const fieldError = document.getElementById('modal-wrkGrp-error');
        const fieldErrorMsg = document.getElementById('modal-wrkGrp-error-msg');
        const fieldFormGroup = document.getElementById('fg-wrkGrp');
        if (fieldError && fieldErrorMsg && fieldFormGroup) {
            fieldErrorMsg.textContent = 'Work Group is required';
            fieldError.hidden = false;
            fieldFormGroup.classList.add('govuk-form-group--error');
            errors.push('Work Group is required');
            hasError = true;
        }
    }
    
    if (!project) {
        const fieldError = document.getElementById('modal-projectTimecode-error');
        const fieldErrorMsg = document.getElementById('modal-projectTimecode-error-msg');
        const fieldFormGroup = document.getElementById('fg-projectTimecode');
        if (fieldError && fieldErrorMsg && fieldFormGroup) {
            fieldErrorMsg.textContent = 'Project is required';
            fieldError.hidden = false;
            fieldFormGroup.classList.add('govuk-form-group--error');
            errors.push('Project is required');
            hasError = true;
        }
    }
    
    if (!jobCode) {
        const fieldError = document.getElementById('modal-jobCodeTimecode-error');
        const fieldErrorMsg = document.getElementById('modal-jobCodeTimecode-error-msg');
        const fieldFormGroup = document.getElementById('fg-jobCodeTimecode');
        if (fieldError && fieldErrorMsg && fieldFormGroup) {
            fieldErrorMsg.textContent = 'Job Code is required';
            fieldError.hidden = false;
            fieldFormGroup.classList.add('govuk-form-group--error');
            errors.push('Job Code is required');
            hasError = true;
        }
    }
    
    if (hasError) {
        if (dbError && dbErrorMsg) {
            dbErrorMsg.textContent = 'Please fill in all required fields';
            dbError.hidden = false;
        }
        return;
    }
    
    if (editingTimecodeId) {
        // Update existing
        const index = timecodesData.findIndex(item => item.id === editingTimecodeId);
        if (index !== -1) {
            timecodesData[index] = {
                ...timecodesData[index],
                timeCode: timeCode,
                wrkGrp: wrkGrp,
                project: project,
                jobCode: jobCode,
                active: active
            };
            console.log('Time code updated:', timecodesData[index]);
        }
    } else {
        // Add new
        const newTimecode = {
            id: timecodesData.length > 0 ? Math.max(...timecodesData.map(t => t.id)) + 1 : 1,
            timeCode: timeCode,
            wrkGrp: wrkGrp,
            project: project,
            jobCode: jobCode,
            active: active
        };
        timecodesData.push(newTimecode);
        console.log('Time code added:', newTimecode);
    }
    
    // Refresh the time codes grid with current filter
    const txtSelectedJobcode = document.getElementById('txtSelectedJobcode');
    if (txtSelectedJobcode && txtSelectedJobcode.value) {
        const filteredTimecodes = timecodesData.filter(item => item.jobCode === txtSelectedJobcode.value);
        timecodesGrid.updateData(filteredTimecodes);
    }
    
    closeTimecodeModal();
}

function deleteTimecode(id, timeCodeValue) {
    console.log('Delete time code with ID:', id, 'TimeCode:', timeCodeValue);
    pendingDeleteData = { id: id, value: timeCodeValue, type: 'timecode' };
    document.getElementById('deleteMessage').textContent = 'Are you sure you want to delete time code "' + timeCodeValue + '"?';
    document.getElementById('deleteModal').style.display = 'flex';
}

/**
 * Action Handlers for Time Entries Grid
 */
let editingTimeentryId = null;

function editTimeEntry(id) {
    console.log('Edit time entry with ID:', id);
    const timeEntry = timeEntriesData.find(item => item.id === id);
    if (timeEntry) {
        editingTimeentryId = id;
        document.getElementById('timeentryModalLabel').textContent = 'Edit Time Entry';
        
        // Clear error messages
        const dbError = document.getElementById('formTimeentry-db-error');
        if (dbError) {
            dbError.hidden = true;
            const errorMsg = document.getElementById('formTimeentry-db-error-msg');
            if (errorMsg) errorMsg.textContent = '';
        }
        
        // Clear individual field errors
        ['pactStaffId', 'timeCodeEntry', 'month', 'parentProject', 'workGroup', 'hours'].forEach(field => {
            const fieldError = document.getElementById('modal-' + field + '-error');
            const fieldErrorMsg = document.getElementById('modal-' + field + '-error-msg');
            const fieldFormGroup = document.getElementById('fg-' + field);
            if (fieldError) fieldError.hidden = true;
            if (fieldErrorMsg) fieldErrorMsg.textContent = '';
            if (fieldFormGroup) fieldFormGroup.classList.remove('govuk-form-group--error');
        });
        
        document.getElementById('modal-pactStaffId').value = timeEntry.pactStaffId || '';
        document.getElementById('modal-timeCodeEntry').value = timeEntry.timeCode || '';
        document.getElementById('modal-month').value = timeEntry.month || '';
        document.getElementById('modal-parentProject').value = timeEntry.parentProject || '';
        document.getElementById('modal-workGroup').value = timeEntry.workGroup || '';
        document.getElementById('modal-hours').value = timeEntry.hours || ''; 

        document.getElementById('modal-pactStaffId').disabled = true
        document.getElementById('modal-timeCodeEntry').disabled = true;
        document.getElementById('modal-month').disabled = true;

        // Set PACT staff dropdown value
        const pactStaffDropdownInput = document.getElementById('pactStaffIDSelectDropdown_input');
        if (pactStaffDropdownInput && timeEntry.pactStaffId) {
            pactStaffDropdownInput.value = timeEntry.pactStaffId;
        }

        // Set workgroup dropdown value
        const workgroupTimeRecordDropdownInput = document.getElementById('workgroupTimeRecordDropdown_input');
        if (workgroupTimeRecordDropdownInput && timeEntry.workGroup) {
            workgroupTimeRecordDropdownInput.value = timeEntry.workGroup; 
        }
        
        document.getElementById('timeentrySaveBtn').style.display = 'none';
        document.getElementById('timeentryUpdateBtn').style.display = '';
        
        document.getElementById('timeentryModal').style.display = 'flex';
    }
}

function openAddTimeentryModal() {
    editingTimeentryId = null;
    document.getElementById('timeentryModalLabel').textContent = 'Add Time Entry';
    document.getElementById('formTimeentry').reset();
    document.getElementById('modal-month').disabled = false;
    document.getElementById('modal-timeCodeEntry').disabled = false;
    // Clear error messages
    const dbError = document.getElementById('formTimeentry-db-error');
    if (dbError) {
        dbError.hidden = true;
        const errorMsg = document.getElementById('formTimeentry-db-error-msg');
        if (errorMsg) errorMsg.textContent = '';
    }
    
    // Clear individual field errors
    ['pactStaffId', 'timeCodeEntry', 'month', 'parentProject', 'workGroup', 'hours'].forEach(field => {
        const fieldError = document.getElementById('modal-' + field + '-error');
        const fieldErrorMsg = document.getElementById('modal-' + field + '-error-msg');
        const fieldFormGroup = document.getElementById('fg-' + field);
        if (fieldError) fieldError.hidden = true;
        if (fieldErrorMsg) fieldErrorMsg.textContent = '';
        if (fieldFormGroup) fieldFormGroup.classList.remove('govuk-form-group--error');
    });
    
    // Pre-fill fields with selected values
    const txtSelectedProjectcodeTwo = document.getElementById('txtSelectedProjectcodeTwo');
    const txtSelectedTimeCode = document.getElementById('txtSelectedTimeCode');
    const txtSelectedWorkGroup = document.getElementById('txtSelectedWorkGroup');
    
    if (txtSelectedProjectcodeTwo && txtSelectedProjectcodeTwo.value) {
        document.getElementById('modal-parentProject').value = txtSelectedProjectcodeTwo.value;
    }
    if (txtSelectedTimeCode && txtSelectedTimeCode.value) {
        document.getElementById('modal-timeCodeEntry').value = txtSelectedTimeCode.value;
    }
    if (txtSelectedWorkGroup && txtSelectedWorkGroup.value) {
        document.getElementById('modal-workGroup').value = txtSelectedWorkGroup.value;
        // Also set the dropdown input value
        const workgroupTimeRecordDropdownInput = document.getElementById('workgroupTimeRecordDropdown_input');
        if (workgroupTimeRecordDropdownInput) {
            workgroupTimeRecordDropdownInput.value = txtSelectedWorkGroup.value;
        }
    } else {
        // Clear workgroup dropdown if no value
        const workgroupTimeRecordDropdownInput = document.getElementById('workgroupTimeRecordDropdown_input');
        if (workgroupTimeRecordDropdownInput) {
            workgroupTimeRecordDropdownInput.value = '';
        }
        document.getElementById('modal-workGroup').value = '';
    }
    
    document.getElementById('timeentrySaveBtn').style.display = '';
    document.getElementById('timeentryUpdateBtn').style.display = 'none';
    document.getElementById('timeentryModal').style.display = 'flex';
}

function closeTimeentryModal() {
    // Clear error messages
    const dbError = document.getElementById('formTimeentry-db-error');
    if (dbError) {
        dbError.hidden = true;
        const errorMsg = document.getElementById('formTimeentry-db-error-msg');
        if (errorMsg) errorMsg.textContent = '';
    }
    
    // Clear individual field errors
    ['pactStaffId', 'timeCodeEntry', 'month', 'parentProject', 'workGroup', 'hours'].forEach(field => {
        const fieldError = document.getElementById('modal-' + field + '-error');
        const fieldErrorMsg = document.getElementById('modal-' + field + '-error-msg');
        const fieldFormGroup = document.getElementById('fg-' + field);
        if (fieldError) fieldError.hidden = true;
        if (fieldErrorMsg) fieldErrorMsg.textContent = '';
        if (fieldFormGroup) fieldFormGroup.classList.remove('govuk-form-group--error');
    });
    
    document.getElementById('timeentryModal').style.display = 'none';
    editingTimeentryId = null;
}

function saveTimeentry() {
    // Clear any previous error messages
    const dbError = document.getElementById('formTimeentry-db-error');
    const dbErrorMsg = document.getElementById('formTimeentry-db-error-msg');
    if (dbError) dbError.hidden = true;
    if (dbErrorMsg) dbErrorMsg.textContent = '';
    
    // Clear individual field errors
    ['pactStaffId', 'timeCodeEntry', 'month', 'parentProject', 'workGroup', 'hours'].forEach(field => {
        const fieldError = document.getElementById('modal-' + field + '-error');
        const fieldErrorMsg = document.getElementById('modal-' + field + '-error-msg');
        const fieldFormGroup = document.getElementById('fg-' + field);
        if (fieldError) fieldError.hidden = true;
        if (fieldErrorMsg) fieldErrorMsg.textContent = '';
        if (fieldFormGroup) fieldFormGroup.classList.remove('govuk-form-group--error');
    });
    
    const pactStaffId = document.getElementById('modal-pactStaffId').value.trim();
    const timeCode = document.getElementById('modal-timeCodeEntry').value.trim();
    const month = document.getElementById('modal-month').value.trim();
    const parentProject = document.getElementById('modal-parentProject').value.trim();
    const workGroup = document.getElementById('modal-workGroup').value.trim();
    const hours = document.getElementById('modal-hours').value.trim();
    
    // Validate required fields
    let hasError = false;
    const errors = [];
    
    if (!pactStaffId) {
        const fieldError = document.getElementById('modal-pactStaffId-error');
        const fieldErrorMsg = document.getElementById('modal-pactStaffId-error-msg');
        const fieldFormGroup = document.getElementById('fg-pactStaffId');
        if (fieldError && fieldErrorMsg && fieldFormGroup) {
            fieldErrorMsg.textContent = 'PACT Staff ID is required';
            fieldError.hidden = false;
            fieldFormGroup.classList.add('govuk-form-group--error');
            errors.push('PACT Staff ID is required');
            hasError = true;
        }
    }
    
    if (!timeCode) {
        const fieldError = document.getElementById('modal-timeCodeEntry-error');
        const fieldErrorMsg = document.getElementById('modal-timeCodeEntry-error-msg');
        const fieldFormGroup = document.getElementById('fg-timeCodeEntry');
        if (fieldError && fieldErrorMsg && fieldFormGroup) {
            fieldErrorMsg.textContent = 'Time Code is required';
            fieldError.hidden = false;
            fieldFormGroup.classList.add('govuk-form-group--error');
            errors.push('Time Code is required');
            hasError = true;
        }
    }
    
    if (!month) {
        const fieldError = document.getElementById('modal-month-error');
        const fieldErrorMsg = document.getElementById('modal-month-error-msg');
        const fieldFormGroup = document.getElementById('fg-month');
        if (fieldError && fieldErrorMsg && fieldFormGroup) {
            fieldErrorMsg.textContent = 'Month is required';
            fieldError.hidden = false;
            fieldFormGroup.classList.add('govuk-form-group--error');
            errors.push('Month is required');
            hasError = true;
        }
    } else {
        const monthValue = parseInt(month);
        if (isNaN(monthValue) || monthValue < 1 || monthValue > 12) {
            const fieldError = document.getElementById('modal-month-error');
            const fieldErrorMsg = document.getElementById('modal-month-error-msg');
            const fieldFormGroup = document.getElementById('fg-month');
            if (fieldError && fieldErrorMsg && fieldFormGroup) {
                fieldErrorMsg.textContent = 'Month must be between 1 and 12';
                fieldError.hidden = false;
                fieldFormGroup.classList.add('govuk-form-group--error');
                errors.push('Month must be between 1 and 12');
                hasError = true;
            }
        }
    }
    
    if (!parentProject) {
        const fieldError = document.getElementById('modal-parentProject-error');
        const fieldErrorMsg = document.getElementById('modal-parentProject-error-msg');
        const fieldFormGroup = document.getElementById('fg-parentProject');
        if (fieldError && fieldErrorMsg && fieldFormGroup) {
            fieldErrorMsg.textContent = 'Parent Project is required';
            fieldError.hidden = false;
            fieldFormGroup.classList.add('govuk-form-group--error');
            errors.push('Parent Project is required');
            hasError = true;
        }
    }
    
    if (!workGroup) {
        const fieldError = document.getElementById('modal-workGroup-error');
        const fieldErrorMsg = document.getElementById('modal-workGroup-error-msg');
        const fieldFormGroup = document.getElementById('fg-workGroup');
        if (fieldError && fieldErrorMsg && fieldFormGroup) {
            fieldErrorMsg.textContent = 'Work Group is required';
            fieldError.hidden = false;
            fieldFormGroup.classList.add('govuk-form-group--error');
            errors.push('Work Group is required');
            hasError = true;
        }
    }
    
    if (!hours) {
        const fieldError = document.getElementById('modal-hours-error');
        const fieldErrorMsg = document.getElementById('modal-hours-error-msg');
        const fieldFormGroup = document.getElementById('fg-hours');
        if (fieldError && fieldErrorMsg && fieldFormGroup) {
            fieldErrorMsg.textContent = 'Hours is required';
            fieldError.hidden = false;
            fieldFormGroup.classList.add('govuk-form-group--error');
            errors.push('Hours is required');
            hasError = true;
        }
    }
    
    if (hasError) {
        if (dbError && dbErrorMsg) {
            dbErrorMsg.textContent = 'Please fill in all required fields';
            dbError.hidden = false;
        }
        return;
    }
    
    if (editingTimeentryId) {
        // Update existing
        const index = timeEntriesData.findIndex(item => item.id === editingTimeentryId);
        if (index !== -1) {
            timeEntriesData[index] = {
                ...timeEntriesData[index],
                pactStaffId: parseInt(pactStaffId),
                timeCode: timeCode,
                month: parseInt(month),
                parentProject: parentProject,
                workGroup: workGroup,
                hours: parseFloat(hours)
            };
            console.log('Time entry updated:', timeEntriesData[index]);
        }
    } else {
        // Add new
        const newTimeentry = {
            id: timeEntriesData.length > 0 ? Math.max(...timeEntriesData.map(t => t.id)) + 1 : 1,
            pactStaffId: parseInt(pactStaffId),
            timeCode: timeCode,
            month: parseInt(month),
            parentProject: parentProject,
            workGroup: workGroup,
            hours: parseFloat(hours)
        };
        timeEntriesData.push(newTimeentry);
        console.log('Time entry added:', newTimeentry);
    }
    
    // Refresh the time entries grid with current filter
    const txtSelectedTimeCode = document.getElementById('txtSelectedTimeCode');
    const txtSelectedWorkGroup = document.getElementById('txtSelectedWorkGroup');
    if (txtSelectedTimeCode && txtSelectedWorkGroup && txtSelectedTimeCode.value && txtSelectedWorkGroup.value) {
        const filteredEntries = timeEntriesData.filter(item => 
            item.timeCode === txtSelectedTimeCode.value && item.workGroup === txtSelectedWorkGroup.value
        );
        timeEntriesGrid.updateData(filteredEntries);
    }
    
    closeTimeentryModal();
}

function deleteTimeEntry(id, staffId) {
    console.log('Delete time entry with ID:', id, 'Staff ID:', staffId);
    pendingDeleteData = { id: id, value: staffId, type: 'timeentry' };
    document.getElementById('deleteMessage').textContent = 'Are you sure you want to delete this time entry for Staff ID "' + staffId + '"?';
    document.getElementById('deleteModal').style.display = 'flex';
}

/**
 * Delete Modal Handlers
 */
function closeDeleteModal() {
    document.getElementById('deleteModal').style.display = 'none';
    pendingDeleteData = null;
}

function confirmDelete() {
    if (!pendingDeleteData) return;
    
    const { id, type } = pendingDeleteData;
    
    if (type === 'jobcode') {
        const index = jobcodesData.findIndex(item => item.id === id);
        if (index !== -1) {
            jobcodesData.splice(index, 1);
            console.log('Job code deleted');
            if (selectedProject) {
                filterGridsByProject(selectedProject);
            } else {
                jobcodesGrid.updateData(jobcodesData);
            }
        }
    } else if (type === 'timecode') {
        const index = timecodesData.findIndex(item => item.id === id);
        if (index !== -1) {
            timecodesData.splice(index, 1);
            console.log('Time code deleted');
            const txtSelectedJobcode = document.getElementById('txtSelectedJobcode');
            if (txtSelectedJobcode && txtSelectedJobcode.value) {
                const filteredTimecodes = timecodesData.filter(item => item.jobCode === txtSelectedJobcode.value);
                timecodesGrid.updateData(filteredTimecodes);
            }
        }
    } else if (type === 'timeentry') {
        const index = timeEntriesData.findIndex(item => item.id === id);
        if (index !== -1) {
            timeEntriesData.splice(index, 1);
            console.log('Time entry deleted');
            const txtSelectedTimeCode = document.getElementById('txtSelectedTimeCode');
            const txtSelectedWorkGroup = document.getElementById('txtSelectedWorkGroup');
            if (txtSelectedTimeCode && txtSelectedWorkGroup && txtSelectedTimeCode.value && txtSelectedWorkGroup.value) {
                const filteredEntries = timeEntriesData.filter(item => 
                    item.timeCode === txtSelectedTimeCode.value && item.workGroup === txtSelectedWorkGroup.value
                );
                timeEntriesGrid.updateData(filteredEntries);
            }
        }
    }
    
    closeDeleteModal();
}

/**
 * Initialize page
 */
document.addEventListener('DOMContentLoaded', async function() {
    console.log('Initializing Project Cascade page...');
    
    // Load all data
    await Promise.all([
        loadJobcodesData(),
        loadTimecodeList(),
        loadTimecodesData(),
        loadTimeEntriesData(),
        loadWorkgroupsData(),
        loadJobCodesDropdownData(),
        loadPactStaffData()
    ]);
    
    // Initialize all grids
    initializeJobcodesGrid();
    initializeTimecodesGrid();
    initializeTimeEntriesGrid();
    
    // Setup row click handlers for cascading filters with a small delay to ensure DOM is ready
    setTimeout(() => {
        setupJobcodesGridRowClickHandler();
        setupTimecodesGridRowClickHandler();
    }, 100);
    
    // Initialize project dropdown
    await initializeProjectDropdown();
    
    // Initialize workgroup dropdowns
    initializeWorkgroupDropdown();
    initializeWorkgroupTimeCodeDropdown();
    initializeWorkgroupTimeRecordDropdown();
    
    // Initialize job code dropdown
    initializeJobCodeDropdown();
    
    // Initialize PACT staff dropdown
    initializePactStaffDropdown();
    
    // Add validation for month input field
    const monthInput = document.getElementById('modal-month');
    if (monthInput) {
        monthInput.addEventListener('input', function() {
            const value = parseInt(this.value);
            if (value > 12) {
                this.value = 12;
            } else if (value < 1 && this.value !== '') {
                this.value = 1;
            }
        });
    }
    
    console.log('Project Cascade page initialized successfully');
});

/**
 * Initialize workgroup dropdown for modal
 */
function initializeWorkgroupDropdown() {
    if (workgroupsData.length === 0) {
        console.warn('Workgroups data not loaded');
        return;
    }
    
    workgroupDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'workgroupDropdown',
        containerSelector: '#workgroupSelectDropdown',
        placeholder: 'Select Work Group',
        showSerialNumber: false,
        searchPlaceholder: 'Search by code or description',
        labelText: '',
        columns: [
            { field: 'code', header: 'Code', width: '80px' },
            { field: 'description', header: 'Description', width: '150px' }
        ],
        data: workgroupsData,
        displayField: 'code',
        valueField: 'code',
        callbacks: {
            onSelect: function(selectedItem, dropdown) {
                // Update the hidden input field
                const modalWkfG = document.getElementById('modal-wkfG');
                if (modalWkfG) {
                    modalWkfG.value = selectedItem.code;
                }
                console.log('Selected workgroup:', selectedItem.code);
            }
        }
    });
    
    console.log('Workgroup dropdown initialized');
}

/**
 * Initialize workgroup dropdown for Time Code modal
 */
function initializeWorkgroupTimeCodeDropdown() {
    if (workgroupsData.length === 0) {
        console.warn('Workgroups data not loaded for Time Code dropdown');
        return;
    }
    
    workgroupTimeCodeDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'workgroupTimeCodeDropdown',
        containerSelector: '#workgroupTimeCodeDropdown',
        placeholder: 'Select Work Group',
        showSerialNumber: false,
        searchPlaceholder: 'Search by code or description',
        labelText: '',
        columns: [
            { field: 'code', header: 'Code', width: '80px' },
            { field: 'description', header: 'Description', width: '150px' }
        ],
        data: workgroupsData,
        displayField: 'code',
        valueField: 'code',
        callbacks: {
            onSelect: function(selectedItem, dropdown) {
                // Update the hidden input field
                const modalWrkGrp = document.getElementById('modal-wrkGrp');
                if (modalWrkGrp) {
                    modalWrkGrp.value = selectedItem.code;
                }
                console.log('Selected workgroup for time code:', selectedItem.code);
            }
        }
    });
    
    console.log('Workgroup Time Code dropdown initialized');
}

/**
 * Initialize workgroup dropdown for Time Entry modal
 */
function initializeWorkgroupTimeRecordDropdown() {
    if (workgroupsData.length === 0) {
        console.warn('Workgroups data not loaded for Time Record dropdown');
        return;
    }
    
    workgroupTimeRecordDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'workgroupTimeRecordDropdown',
        containerSelector: '#workgroupTimeRecordDropdown',
        placeholder: 'Select Work Group',
        showSerialNumber: false,
        searchPlaceholder: 'Search by code or description',
        labelText: '',
        disabled: true, // Initially disabled until a time code is selected
        columns: [
            { field: 'code', header: 'Code', width: '80px' },
            { field: 'description', header: 'Description', width: '150px' }
        ],
        data: workgroupsData,
        displayField: 'code',
        valueField: 'code',
        callbacks: {
            onSelect: function(selectedItem, dropdown) {
                // Update the hidden input field
                const modalWorkGroup = document.getElementById('modal-workGroup');
                if (modalWorkGroup) {
                    modalWorkGroup.value = selectedItem.code;
                }
                console.log('Selected workgroup for time entry:', selectedItem.code);
            }
        }
    });
    
    console.log('Workgroup Time Record dropdown initialized');
}

/**
 * Initialize job code dropdown for Time Code modal
 */
function initializeJobCodeDropdown() {
    if (jobCodesDropdownData.length === 0) {
        console.warn('Job codes dropdown data not loaded');
        return;
    }
    
    jobCodeDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'jobCodeDropdown',
        containerSelector: '#jobCodeDropdown',
        placeholder: 'Select Job Code',
        showSerialNumber: false,
        searchPlaceholder: 'Search by code or description',
        labelText: '',
        columns: [
            { field: 'code', header: 'Job Code', width: '100px' },
            { field: 'description', header: 'Description', width: '250px' }
        ],
        data: jobCodesDropdownData,
        displayField: 'code',
        valueField: 'code',
        callbacks: {
            onSelect: function(selectedItem, dropdown) {
                // Update the hidden input field
                const modalJobCodeTimecode = document.getElementById('modal-jobCodeTimecode');
                if (modalJobCodeTimecode) {
                    modalJobCodeTimecode.value = selectedItem.code;
                }
                console.log('Selected job code:', selectedItem.code);
            }
        }
    });
    
    console.log('Job Code dropdown initialized');
}

/**
 * Initialize PACT staff dropdown for Time Entry modal
 */
function initializePactStaffDropdown() {
    if (pactStaffData.length === 0) {
        console.warn('PACT staff data not loaded');
        return;
    }
    
    pactStaffDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'pactStaffIDSelectDropdown',
        containerSelector: '#pactStaffIDSelectDropdown',
        placeholder: 'Select PACT Staff',
        showSerialNumber: false,
        searchPlaceholder: 'Search by ID or name',
        labelText: '',
        columns: [
            { field: 'pactId', header: 'PACT ID', width: '80px' },
            { field: 'name', header: 'Name', width: '200px' }
        ],
        data: pactStaffData,
        displayField: 'pactId',
        valueField: 'pactId',
        callbacks: {
            onSelect: function(selectedItem, dropdown) {
                // Update the hidden input field
                const modalPactStaffId = document.getElementById('modal-pactStaffId');
                if (modalPactStaffId) {
                    modalPactStaffId.value = selectedItem.pactId;
                }
                console.log('Selected PACT staff:', selectedItem.pactId, '-', selectedItem.name);
            }
        }
    });
    
    console.log('PACT Staff dropdown initialized');
}

// Populate select dropdown
function populateSelect(selectElement, data, valueKey, textKey) {
    if (!selectElement) return;
    
    // Build options HTML with selected attribute on default option
    let optionsHTML = '<option value="" selected>-- Select Project --</option>';
    
    data.forEach(item => {
        optionsHTML += `<option value="${item[valueKey]}">${item[textKey]}</option>`;
    });
    
    selectElement.innerHTML = optionsHTML;
    
    // Force selection after DOM update
    setTimeout(() => {
        selectElement.selectedIndex = 0;
        selectElement.value = '';
    }, 0);
}
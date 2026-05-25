// Monthly TIME Log of Imports - JavaScript Handler
// This script manages the filtering and display of time log import records

// Global data
let workGroupsData = [];
let projectListData = [];
let jobCodeListData = [];
let staffListData = [];
let projectsData = [];
let jobCodesData = [];
let testCodesData = [];
let staffData = [];
let timeLogRecordsData = [];
let filteredRecords = [];
let timeLogGrid = null; // DataGridComponent instance
let projectDropdown = null; // MultiColumnDropdownComponent instance
let jobCodeDropdown = null; // MultiColumnDropdownComponent instance
let staffIDDropdown = null; // MultiColumnDropdownComponent instance
let selectedProjectCode = ''; // Store selected project code
let selectedJobCode = ''; // Store selected job code
let selectedStaffId = ''; // Store selected staff ID

/**
 * Load all required data from JSON files
 */
async function loadData() {
    try {
        const response = await fetch('../js/pact_js/data/time_log_imports.json');
        if (!response.ok) throw new Error('Failed to load time log import records data');
        const data = await response.json();
        
        // Extract data from JSON
        workGroupsData = data.workGroups || [];
        projectsData = data.projects || [];
        jobCodesData = data.jobCodes || [];
        testCodesData = data.testCodes || [];
        staffData = data.staff || [];
        timeLogRecordsData = data.records || [];
        
        return true;
    } catch (error) {
        console.error('Error loading data:', error);
        // Fallback data if JSON fails to load
        workGroupsData = [
            { id: 1, name: "BAC3" },
            { id: 2, name: "BAC2" },
            { id: 3, name: "BAC1" }
        ];
        projectsData = [
            { id: 1, code: "VM0533A" },
            { id: 2, code: "VM0533B" },
            { id: 3, code: "RDDR1140" }
        ];
        jobCodesData = [
            { id: 1, code: "JR" },
            { id: 2, code: "OR" },
            { id: 3, code: "SR" }
        ];
        testCodesData = [
            { id: 1, code: "PT0000" },
            { id: 2, code: "PT0001" },
            { id: 3, code: "PT0002" },
            { id: 4, code: "PT0003" },
            { id: 5, code: "PT0004" },
            { id: 6, code: "PT0005" },
            { id: 7, code: "PT0006" },
            { id: 8, code: "PT0007" },
            { id: 9, code: "PT0008" },
            { id: 10, code: "PT0009" },
            { id: 11, code: "PT0010" },
            { id: 12, code: "PT0011" },
            { id: 13, code: "PT0012" },
            { id: 14, code: "PT0013" },
            { id: 15, code: "PT0014" },
            { id: 16, code: "PT0015" }
        ];
       
      //  timeLogRecordsData = generateSampleData();
        return false;
    }
}

/**
 * Load project list data from JSON file
 */
async function loadProjectListData() {
    try {
        const response = await fetch('../js/pact_js/data/project-list.json');
        if (!response.ok) throw new Error('Failed to load project list data');
        projectListData = await response.json();
        return true;
    } catch (error) {
        console.error('Error loading project list data:', error);
        projectListData = [];
        return false;
    }
}

/**
 * Load job code list data from JSON file
 */
async function loadJobCodeListData() {
    try {
        const response = await fetch('../js/pact_js/data/jobcode-list.json');
        if (!response.ok) throw new Error('Failed to load job code list data');
        jobCodeListData = await response.json();
        return true;
    } catch (error) {
        console.error('Error loading job code list data:', error);
        jobCodeListData = [];
        return false;
    }
}

async function loadStaffListData() {
    try {
        const response = await fetch('../js/pact_js/data/staff-list.json');
        if (!response.ok) throw new Error('Failed to load staff list data');
        staffListData = await response.json();
        return true;
    } catch (error) {
        console.error('Error loading staff list data:', error);
        staffListData = [];
        return false;
    }
}

/**
 * Generate sample data for demonstration
 */
function generateSampleData() {
    const sampleRecords = [
        { id: 1214573, timeCode: "VM0533A", project: "VM0533A", month: 1, staffId: "18727", wg: "BAC3", hours: 24, dateImported: "02/05/2025 10:36:07", mabUserSPNo: "DEMETER\\m304206", action: "II" },
        { id: 1214574, timeCode: "VM0533A", project: "VM0533A", month: 1, staffId: "14534", wg: "BAC3", hours: 16, dateImported: "02/05/2025 10:36:08", mabUserSPNo: "DEMETER\\m304206", action: "II" },
        { id: 1214574, timeCode: "VM0533B", project: "VM0533B", month: 1, staffId: "14534", wg: "BAC3", hours: 10, dateImported: "02/05/2025 10:36:08", mabUserSPNo: "DEMETER\\m304206", action: "II" },
        { id: 1214575, timeCode: "RDDR1140", project: "RDDR1140", month: 1, staffId: "11944", wg: "BAC3", hours: 7, dateImported: "02/05/2025 10:36:07", mabUserSPNo: "DEMETER\\m304206", action: "II" },
        { id: 1214576, timeCode: "VM0533B", project: "VM0533B", month: 1, staffId: "11944", wg: "BAC3", hours: 73, dateImported: "02/05/2025 10:36:07", mabUserSPNo: "DEMETER\\m304206", action: "II" },
        { id: 1214577, timeCode: "FDSE2226", project: "FDSE2226", month: 1, staffId: "13658", wg: "BAC3", hours: 51, dateImported: "02/05/2025 10:36:07", mabUserSPNo: "DEMETER\\m304206", action: "II" },
        { id: 1214579, timeCode: "RDDR1106", project: "RDDR1106", month: 1, staffId: "14475", wg: "BAC3", hours: 74, dateImported: "02/05/2025 10:36:07", mabUserSPNo: "DEMETER\\m304206", action: "II" },
        { id: 1214580, timeCode: "VM0533B", project: "VM0533B", month: 1, staffId: "14475", wg: "BAC3", hours: 29, dateImported: "02/05/2025 10:36:08", mabUserSPNo: "DEMETER\\m304206", action: "II" },
        { id: 1214581, timeCode: "RDCR2008", project: "RDCR2008", month: 1, staffId: "14545", wg: "BAC3", hours: 132, dateImported: "02/05/2025 10:36:08", mabUserSPNo: "DEMETER\\m304206", action: "II" },
        { id: 1214582, timeCode: "RDCR2008", project: "RDCR2008", month: 1, staffId: "14541", wg: "BAC3", hours: 113, dateImported: "02/05/2025 10:36:08", mabUserSPNo: "DEMETER\\m304206", action: "II" },
        { id: 1214583, timeCode: "RDSE2226", project: "RDSE2226", month: 1, staffId: "14757", wg: "BAC3", hours: 43, dateImported: "02/05/2025 10:36:08", mabUserSPNo: "DEMETER\\m304206", action: "II" },
        { id: 1214584, timeCode: "EXEU1681", project: "EXEU1681", month: 1, staffId: "14836", wg: "BAC3", hours: 17, dateImported: "02/05/2025 10:36:03", mabUserSPNo: "DEMETER\\m304206", action: "II" }
    ];
    return sampleRecords;
}

/**
 * Populate dropdown with data
 */
function populateDropdown(selectId, data, valueKey, textKey, placeholder) {
    const selectElement = document.getElementById(selectId);
    if (!selectElement) return;
    
    let optionsHTML = `<option value="">${placeholder}</option>`;
    
    data.forEach(item => {
        optionsHTML += `<option value="${item[valueKey]}">${item[textKey]}</option>`;
    });
    
    selectElement.innerHTML = optionsHTML;
}

/**
 * Initialize all dropdowns
 */
function initializeDropdowns() {
    populateDropdown('workGroupSelect', workGroupsData, 'name', 'name', '-- Select WorkGroup --');
   // populateDropdown('projectSelect', projectsData, 'code', 'code', '-- Select Project --');
   // populateDropdown('jobCodeSelect', jobCodesData, 'code', 'code', '-- Select Jobcode --');
    populateDropdown('testCodeSelect', testCodesData, 'code', 'code', '-- Select Testcode --');
 //   populateDropdown('staffIdSelect', staffData, 'staffId', 'staffId', '-- Select Staff ID --');
}

/**
 * Initialize the DataGrid
 */
function initializeGrid() {
    const gridContainer = document.getElementById('gridContainer_timeLogGrid');
    if (!gridContainer) {
        console.error('Grid container not found');
        return;
    }

    // Define grid columns based on the screenshot
    const columns = [
        { 
            field: 'id', 
            header: 'ID', 
            width: 70,
            sortable: true
        },
        { 
            field: 'timeCode', 
            header: 'Time Code', 
            width: 100,
            sortable: true
        },
        { 
            field: 'project', 
            header: 'Project', 
            width: 100,
            sortable: true
        },
        { 
            field: 'month', 
            header: 'Month', 
            width: 70,
            sortable: true,
            render: function(value) {
                return '<div style="text-align: center;">' + value + '</div>';
            }
        },
        { 
            field: 'staffId', 
            header: 'Staff ID', 
            width: 80,
            sortable: true
        },
        { 
            field: 'wg', 
            header: 'WG', 
            width: 70,
            sortable: true
        },
        { 
            field: 'hours', 
            header: 'Hours', 
            width: 70,
            sortable: true,
            render: function(value) {
                return '<div style="text-align: right;">' + value + '</div>';
            }
        },
        { 
            field: 'dateImported', 
            header: 'Date Imported', 
            width: 150,
            sortable: true
        },
        { 
            field: 'mabUserSPNo', 
            header: 'MAB User SP No.', 
            width: 150,
            sortable: true
        },
        { 
            field: 'action', 
            header: 'Action', 
            width: 70,
            sortable: true
        }
    ];

    // Initialize the DataGridComponent with empty data initially
    timeLogGrid = new DataGridComponent({
        gridId: 'timeLogGrid',
        containerSelector: '#gridContainer_timeLogGrid',
        title: '',
        columns: columns,
        data: [],
        pageSize: 15,
        enableSort: true,
        enableResize: true,
        enableSelection: false,
        enablePagination: true,
        showAddButton: false,
        pageSizeOptions: [10, 15, 20, 25, 50]
    });
    
    // Set initial filtered records to empty array
    filteredRecords = [];
}

/**
 * Convert date from YYYY-MM-DD format to DD/MM/YYYY format
 */
function convertDateFormat(dateString) {
    if (!dateString) return '';
    
    // Split the date string (YYYY-MM-DD)
    const parts = dateString.split('-');
    if (parts.length !== 3) return dateString;
    
    const year = parts[0];
    const month = parts[1];
    const day = parts[2];
    
    // Return in DD/MM/YYYY format
    return `${day}/${month}/${year}`;
}

/**
 * Extract date portion from datetime string (DD/MM/YYYY HH:MM:SS)
 */
function extractDatePortion(dateTimeString) {
    if (!dateTimeString) return '';
    
    // Split by space to separate date and time
    const parts = dateTimeString.split(' ');
    return parts[0]; // Return only the date part (DD/MM/YYYY)
}

/**
 * Filter records based on search criteria
 */
function filterRecords() {
    const workGroup = document.getElementById('workGroupSelect').value;
    const project = selectedProjectCode;
    const jobCode = selectedJobCode;
    const testCode = document.getElementById('testCodeSelect').value;
    const staffId = selectedStaffId;
    const month = document.getElementById('monthInput').value;
    const dateImported = document.getElementById('dateImportedInput').value;
    const mabUserSP = document.getElementById('mabUserSPInput').value.toLowerCase();
    const action = document.getElementById('actionSelect').value;
    
    // Convert date from YYYY-MM-DD to DD/MM/YYYY for comparison
    const formattedDate = convertDateFormat(dateImported);

    filteredRecords = timeLogRecordsData.filter(record => {
        let matches = true;

        if (workGroup && record.wg !== workGroup) matches = false;
        if (project && record.project !== project) matches = false;
        if (jobCode && record.timeCode !== jobCode) matches = false;
        if (testCode && record.timeCode !== testCode) matches = false;
        if (staffId && record.staffId !== staffId) matches = false;
        if (month && record.month.toString() !== month) matches = false;
        
        // Match only the date portion, ignoring time
        if (formattedDate) {
            const recordDateOnly = extractDatePortion(record.dateImported);
            if (recordDateOnly !== formattedDate) matches = false;
        }
        
        if (mabUserSP && !record.mabUserSPNo.toLowerCase().includes(mabUserSP)) matches = false;
        if (action && record.action !== action) matches = false;

        return matches;
    });

    return filteredRecords;
}

/**
 * Handle search button click
 */
function handleSearch() {
    // Get all filter values
    const workGroup = document.getElementById('workGroupSelect').value;
    const project = selectedProjectCode;
    const jobCode = selectedJobCode;
    const testCode = document.getElementById('testCodeSelect').value;
    const staffId = selectedStaffId;
    const month = document.getElementById('monthInput').value;
    const dateImported = document.getElementById('dateImportedInput').value;
    const mabUserSP = document.getElementById('mabUserSPInput').value;
    const action = document.getElementById('actionSelect').value;
    
    // Check if at least one criteria is provided
    if (!workGroup && !project && !jobCode && !testCode && !staffId && !month && !dateImported && !mabUserSP && !action) {
        alert('Please enter some criteria');
        return;
    }
    
    const results = filterRecords();
    
    if (timeLogGrid) {
        timeLogGrid.updateData(results);
    }
    
    console.log(`Search completed: ${results.length} records found`);
}

/**
 * Clear all filters
 */
function handleClearAll() {
    // Reset all filter controls
    document.getElementById('workGroupSelect').selectedIndex = 0;
    selectedProjectCode = '';
    const projectInput = document.getElementById('projectDropdown_input');
    if (projectInput) projectInput.value = '';
    selectedJobCode = '';
    const jobCodeInput = document.getElementById('jobCodeDropdown_input');
    if (jobCodeInput) jobCodeInput.value = '';
    selectedStaffId = '';
    const staffIDInput = document.getElementById('staffIDDropdown_input');
    if (staffIDInput) staffIDInput.value = '';
    // document.getElementById('jobCodeSelect').selectedIndex = 0;
    document.getElementById('testCodeSelect').selectedIndex = 0;
    // document.getElementById('staffIdSelect').selectedIndex = 0;
    document.getElementById('monthInput').value = '';
    document.getElementById('dateImportedInput').value = '';
    document.getElementById('mabUserSPInput').value = '';
    document.getElementById('actionSelect').selectedIndex = 0;
    
    // Clear the grid
    if (timeLogGrid) {
        timeLogGrid.updateData([]);
    }
    
    filteredRecords = [];
    console.log('All filters cleared');
}

/**
 * Setup event listeners
 */
function setupEventListeners() {
    const searchBtn = document.getElementById('searchBtn');
    const clearAllBtn = document.getElementById('clearAllBtn');
    
    if (searchBtn) {
        searchBtn.addEventListener('click', handleSearch);
    }
    
    if (clearAllBtn) {
        clearAllBtn.addEventListener('click', handleClearAll);
    }
    
    // Add Enter key support for input fields
    const inputFields = ['monthInput', 'dateImportedInput', 'mabUserSPInput'];
    inputFields.forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field) {
            field.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    handleSearch();
                }
            });
        }
    });
}

/**
 * Initialize the page
 */
async function initializePage() {
    console.log('Initializing Monthly TIME Log of Imports page...');
    
    // Load data
    await loadData();
    await loadProjectListData();
    await loadJobCodeListData();
    await loadStaffListData();
    // Initialize UI components
    initializeDropdowns();
    initializeGrid();
    setupEventListeners();
    
    console.log('Page initialization complete');
}

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', async () => { 
        await initializePage();
     /*Multicolumn dropdown functionality for project selection*/
    projectDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'projectDropdown',
        containerSelector: '#projectSelectDropdown',
        placeholder: 'Select Project',
        showSerialNumber:false,
        searchPlaceholder: 'Search by code or description',
        labelText: 'Project',//this label will come at the top of dropdown, can be set as per requirement
        columns: [
            { field: 'code', header: 'Code', width: '80px' },
            { field: 'description', header: 'Description', width: '150px' }, 
        ],
        data: projectListData,
        displayField: 'code',
        valueField: 'code',
        callbacks: {
            onSelect: function(selectedItem, dropdown) {
                selectedProjectCode = selectedItem.code;
                console.log('Selected project:', selectedProjectCode);
            }
        }
    });
    /*Multicolumn dropdown end here*/

     /*Multicolumn dropdown functionality for job code selection*/
    jobCodeDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'jobCodeDropdown',
        containerSelector: '#jobCodeSelectDropdown',
        placeholder: 'Select Job Code',
        showSerialNumber:false,
        searchPlaceholder: 'Search by code or description',
        labelText: 'Job Code',//this label will come at the top of dropdown, can be set as per requirement
        columns: [
            { field: 'code', header: 'Code', width: '80px' },
            { field: 'description', header: 'Description', width: '150px' }, 
        ],
        data: jobCodeListData,
        displayField: 'code',
        valueField: 'code',
        callbacks: {
            onSelect: function(selectedItem, dropdown) {
                selectedJobCode = selectedItem.code;
                console.log('Selected job code:', selectedJobCode);
            }
        }
    });
    /*Multicolumn dropdown end here*/

     /*Multicolumn dropdown functionality for staff ID selection*/
    staffIDDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'staffIDDropdown',
        containerSelector: '#staffIDSelectDropdown',
        placeholder: 'Select Staff ID',
        showSerialNumber:false,
        searchPlaceholder: 'Search by PACTID or StaffID or Name',
        labelText: 'Staff ID',//this label will come at the top of dropdown, can be set as per requirement
        columns: [
            { field: 'pactId', header: 'PACT ID', width: '80px' },
            { field: 'staffId', header: 'Staff ID', width: '80px' },
            { field: 'name', header: 'Name', width: '150px' }, 
        ],
        data: staffListData,
        displayField: 'pactId',
        valueField: 'pactId',
        callbacks: {
            onSelect: function(selectedItem, dropdown) {
                selectedStaffId = selectedItem.pactId;
                console.log('Selected staff ID:', selectedStaffId);
            }
        }
    });
    /*Multicolumn dropdown end here*/

    });
} else {
    initializePage();
}

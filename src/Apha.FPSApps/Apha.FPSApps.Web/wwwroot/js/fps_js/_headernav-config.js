/**
 * headernav-config.js  —  FPS header and navigation data
 *
 * Edit this file to update the app header or add/remove/rename menu items.
 *
 * Header config shape:
 *   appName       : string   — displayed in the header pill
 *   showYearSelect: boolean  — show the year selector
 *   yearSelectId  : string   — id attribute for the <select> element
 *
 * Menu item shapes:
 *   Direct link:          { label, href }
 *   Dropdown (Level 1):   { label, id, items: [ ...level-2 ] }
 *   Sub-dropdown (L2→L3): { label, id, items: [ ...level-3 ] }
 *   Plain link (L2 or L3):{ label, href }
 *   Multi-column:         add columns, colWidths[] to a Level-1 item;
 *                         add column (1-based) to a Level-2 item to pin it
 */

const headerConfig = {
    appName: 'FPS',
    showYearSelect: true,
    yearSelectId: 'dpSelectyear'
};

const menuConfig = [
    {
        label: "Programme Management",
        id: "programme-management",
        items: [
            { label: "Select Programme", href: "program_management.html" },
            {
                label: "Plan Projects",
                id: "plan-projects",
                items: [
                    { label: "Set Up A New Project",                              href: "setup_newproject.html" },
                    { label: "Plan Project Individually",                         href: "planproject_individually.html" },
                    { label: "Plan Staff for Entire Programme",                   href: "planstaff_entireprogram.html" },
                    { label: "Plan Animals for Entire Programme",                 href: "#" },
                    { label: "Plan Test Purchases for Entire Programme",          href: "#" },
                    { label: "Plan Additional Cost for Entire Programme",         href: "#" }
                ]
            },
            {
                label: "Review Plans",
                id: "pm-review-plans",
                items: [
                    { label: "Project Profitability",  href: "project_profitability.html" },
                    { label: "Staff Plan Pivot",     href: "#" }
                ]
            },
            {
                label: "Summary Printouts",
                id: "summary-printouts",
                items: [
                    { label: "Programme Profitability Report", href: "#" },
                    { label: "Summarized by Disease",          href: "#" },
                    { label: "Summarized by Customer",         href: "#" },
                    { label: "Project Specifics",              href: "#" }
                ]
            }
        ]
    },
    {
        label: "Resource Management",
        id: "resource-management",
        items: [
            { label: "Create Resource Centre", href: "createresourcecenter.html" },
            {
                label: "Setup Resource",
                id: "setup-resource",
                items: [
                    { label: "Enter Staff Resources (by Resource Centre)", href: "setupresource.html" },
                    { label: "WorkGroup Resources",                         href: "workgroupresources.html" },
                    { label: "Resource Set-Up Report",                      href: "#" },
                    { label: "High Level Summary",                          href: "#" }
                ]
            },
            {
                label: "Review Plans",
                id: "rm-review-plans",
                items: [
                    { label: "Recalculate Rates",       href: "reviewplans.html" },
                    { label: "Contribution Summary",    href: "#" },
                    { label: "Staff Plan Pivot",        href: "#" },
                    { label: "Resource Utilization View", href: "#" },
                    { label: "View/Replan Staff Hours", href: "#" }
                ]
            },
            { label: "Reports", href: "reports.html" }
        ]
    },

     {
        label: "MAB",
        id: "mab",
        columns: 3,
        colWidths: ['200px', '310px', '240px'],
        items: [
            { label: "Maintenance Menu",                       href: "maintain_division.html" },
            { label: "Stage 1 Plans",                          href: "#" },
            { label: "ASU Data View",                          href: "fps_asuview.html" },
            { label: "VLA Project Totals",                     href: "projectprofitability_vla.html" },
            { label: "Generic Bids",                           href: "budget_bids_resource_centre.html" },
            { label: "Project Audit Trails",                   href: "projectaudit_trail.html" },
			{ label: "Update Rates",                           href: "fec_testmanagement.html" },
             { label: "Misc Project Data",                   href: "misc_projectdata.html" },
            { label: "SMG Summary Report - All Work",          href: "#", column: 2 },
            { label: "SMG Summary Report - Assured Work Only", href: "#" },
            { label: "All Programs Profitability",             href: "#" },
            { label: "Program Exceptional Bids",               href: "#" },
            { label: "Income Contribution Rpt",                href: "#" },
            { label: "Income Cont - Summary",                  href: "#" },
            { label: "Project Specific - Query",               href: "#" },
            { label: "Resource Grades by Programme",           href: "#" },
            { label: "Resource Utilisation",                   href: "#" },
            { label: "Open FPS",                               href: "#" },
            { label: "Close FPS",                              href: "#" },
            { label: "Run Snapshot Queries",                   href: "#" },
            { label: "Run Comparison Queries",                 href: "#" }
        ]
    },
    
    { 
    label: "Lab Testing Manager's", 
    id: "lab-testing-managers",   
     items: [
     {
                label: "Requirements for Tests",
                id: "requirements-for-tests",
                items: [
                    { label: "Test List", href: "test_lists_for_VLA.html" },
                    { label: "Test Supplier View",href: "test_supplierview.html" },
                    { label: "Test Price Check",href: "test_price_check.html" },
                    { label: "Log of Test Requirement Changes",                          href: "test_requirementlog.html" }
                ]
            },
            {
                label: "Set Up Supply of Tests",
                id: "set-up-supply-of-tests",
                items: [
                    { label: "Create New Portfolios",       href: "create_newportfolios.html" },
                    { label: "Set Up Portfolio Components",    href: "setup_portfoliocomponents.html" }
                    
                ]
            },
]
      },  
    
    


    { label: "Project Plan Viewer",  href: "projectplan_viewer.html" },
      { 
    label: "Project Group", 
    id: "project-group",   
     items: [
      { label: "Set Up A New Project",                              href: "setup_newproject.html" },
     { label: "Plan Project Individually",                         href: "planproject_individually.html" },

            {
                label: "Group Profitability",
                 href: "project_profitability.html"
               
            }
]
      }
];

const userConfig = {
    name: "FPS User",
    homeHref: "../index.html"
};

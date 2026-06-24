/*
 * TRANSFORMENGINE MIGRATION — ICostBookApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - Added three new sub-client properties for frmMaintainance backend endpoints:
 *       ICostBookMaintenanceApiClient CostbookMaintenance  — Tabs 1, 2, 4 (settings + account categories)
 *       ICostBookCapsStaffApiClient   CostbookCapsStaff    — Tab 5 (CAPS Staff full CRUD)
 *       ICostBookAccountGroupApiClient CostbookAccountGroup — Tab 3 (CSG7 Inflation Options full CRUD)
 *   - Redundant self-using directive removed (namespace already matches)
 *
 * PRESERVED:
 *   - All pre-existing properties unchanged: Projects, Customers, Diseases, Programs, Staff,
 *     Contracts, YearlyDetails, ProjectSummary, CostbookSettings
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.FPSApps.Application.Interfaces.CostBookApiClients;

// TRANSFORMENGINE: Aggregate API client interface — sub-client properties added for frmMaintainance (Phase 7)
public interface ICostBookApiClient
{
    ICostBookProjectApiClient Projects { get; }
    ICostBookCustomerApiClient Customers { get; }
    ICostBookDiseaseApiClient Diseases { get; }
    ICostBookProgramApiClient Programs { get; }
    ICostBookStaffApiClient Staff { get; }
    ICostBookContractApiClient Contracts { get; }
    ICostBookYearlyDetailsApiClient YearlyDetails { get; }
    ICostBookProjectSummaryApiClient ProjectSummary { get; }
    ICostBookSettingsApiClient CostbookSettings { get; }

    // TRANSFORMENGINE: New sub-clients added for frmMaintainance backend — Phase 7
    //   CostbookMaintenance  → MaintenanceController  (/api/v1/maintenance/settings + /account-categories)
    //   CostbookCapsStaff    → CapsStaffController    (/api/v1/capsstaff)
    //   CostbookAccountGroup → AccountGroupController (/api/v1/accountgroup)
    ICostBookMaintenanceApiClient CostbookMaintenance { get; }
    ICostBookCapsStaffApiClient CostbookCapsStaff { get; }
    ICostBookAccountGroupApiClient CostbookAccountGroup { get; }
}

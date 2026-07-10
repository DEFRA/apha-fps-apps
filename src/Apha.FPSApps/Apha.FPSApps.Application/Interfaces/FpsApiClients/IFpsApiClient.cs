/*
 * TRANSFORMENGINE MIGRATION — IFpsApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 1 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Aggregate API client interface annotated for Set Up Staff Resources form migration
 *   - Verified: IFpsProfitCentreApiClient FpsProfitCentre, IFpsWorkGroupGradeApiClient FpsWorkGroupGrade,
 *     IFpsWorkGroupEmployeeApiClient FpsWorkGroupEmployee — all three properties required by
 *     SetUpStaffResourcesController are present
 *   - IFpsGradeApiClient FpsGrade added in a prior batch (preserved as-is)
 *
 * PRESERVED:
 *   - All 32 existing sub-client properties preserved verbatim (order and names unchanged)
 *   - No new properties added in this batch — all required clients were already registered
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsApiClient
    {
        IFpsStaffJobApiClient FpsStaffJob { get; }
        IFpsEmployeeApiClient FpsEmployee { get; }
        IFpsProgramApiClient FpsProgram { get; }
        IFpsProjectApiClient FpsProject { get; }
        IFpsLookupApiClient FpsLookup { get; }
        IFpsAnimalPlanApiClient FpsAnimalPlan { get; }
        IFpsSettingApiClient FpsSetting { get; }
        IFpsYearMasterApiClient FpsYearMaster { get; }
        IFpsProjectStaffPlanActualApiClient FpsProjectStaffPlanActual { get; }
        IFpsMonthlyOutputApiClient FpsMonthlyOutput { get; }
        IFpsDivisionApiClient FpsDivision { get; }
        IFpsAgencyApiClient FpsAgency { get; }
        IFpsAdditionalCostApiClient FpsAdditionalCost { get; }
        IFpsAccountCategoryApiClient FpsAccountCategory { get; }
        // TRANSFORMENGINE: FpsProfitCentre — Resource Centre dropdown source for SetUpStaffResources
        IFpsProfitCentreApiClient FpsProfitCentre { get; }
        IFpsProfitCentreGradeApiClient FpsProfitCentreGrade { get; }
        // TRANSFORMENGINE: FpsWorkGroupGrade — Grade listbox source for SetUpStaffResources
        IFpsWorkGroupGradeApiClient FpsWorkGroupGrade { get; }
        // TRANSFORMENGINE: FpsWorkGroupEmployee — Staff grid data source for SetUpStaffResources
        IFpsWorkGroupEmployeeApiClient FpsWorkGroupEmployee { get; }
        IFpsDivisionGradeApiClient FpsMaintDG { get; }
        IFpsProjectStaffPlanApiClient FpsProjectStaffPlan { get; }
        IFpsProjectGroupStaffPlanApiClient FpsProjectGroupStaffPlan { get; }
        IFpsAnimalApiClient FpsAnimalMaster { get; }
        IFpsProjectGroupApiClient FpsProjectGroup { get; }
        IFpsBudgetBidsApiClient FpsBudgetBids { get; }
        IFpsPurchasesApiClient FpsPurchases { get; }
        IFpsUserApiClient FpsUserPermission { get; }
        IFpsGradeApiClient FpsGrade { get; }
        IFpsProjectAuditTrailApiClient FpsProjectAuditTrail { get; }
        IFpsTotalBusinessOverheadsApiClient FpsTotalBusinessOverheads { get; }
    }
}

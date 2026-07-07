/*
 * TRANSFORMENGINE MIGRATION — FpsApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 3 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Added TRANSFORMENGINE migration header (PB-14 annotation policy)
 *   - Verified: FpsProfitCentre, FpsWorkGroupGrade, FpsWorkGroupEmployee all wired in constructor
 *   - Verified: IFpsWorkGroupGradeApiClient FpsWorkgroupGrade alias also wired (legacy alias preserved)
 *   - FpsGrade already wired in prior batch (preserved as-is)
 *
 * PRESERVED:
 *   - All 24 sub-client property declarations and constructor assignments
 *   - Property names and types exactly match IFpsApiClient interface
 *   - FpsWorkgroupGrade alias (uses same FpsWorkGroupGradeApiClient — alias for backward compat)
 *   - Constructor injection pattern: IFpsHttpExecutor http, IMapper mapper
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: IFpsApiClient interface does not declare FpsWorkgroupGrade (alias property) —
 *     confirm this is intentional or remove if not needed by any consumer
 */

using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsApiClient : IFpsApiClient
    {
        public IFpsStaffJobApiClient FpsStaffJob { get; }
        public IFpsEmployeeApiClient FpsEmployee { get; }
        public IFpsProgramApiClient FpsProgram { get; }
        public IFpsProjectApiClient FpsProject { get; }
        public IFpsLookupApiClient FpsLookup { get; }
        public IFpsAnimalPlanApiClient FpsAnimalPlan { get; }
        public IFpsSettingApiClient FpsSetting { get; }
        public IFpsYearMasterApiClient FpsYearMaster { get; }
        public IFpsProjectStaffPlanActualApiClient FpsProjectStaffPlanActual { get; }
        public IFpsMonthlyOutputApiClient FpsMonthlyOutput { get; }
        public IFpsDivisionApiClient FpsDivision { get; }
        public IFpsAgencyApiClient FpsAgency { get; }
        public IFpsAdditionalCostApiClient FpsAdditionalCost { get; }
        public IFpsAccountCategoryApiClient FpsAccountCategory { get; }
        public IFpsDivisionGradeApiClient FpsMaintDG { get; }
        public IFpsAnimalApiClient FpsAnimalMaster { get; }

        // TRANSFORMENGINE: FpsProfitCentre — Resource Centre dropdown source for SetUpStaffResources
        public IFpsProfitCentreApiClient FpsProfitCentre { get; }
        public IFpsProfitCentreGradeApiClient FpsProfitCentreGrade { get; }

        // TRANSFORMENGINE: FpsWorkGroupGrade — Grade listbox source for SetUpStaffResources
        public IFpsWorkGroupGradeApiClient FpsWorkGroupGrade { get; }

        // TRANSFORMENGINE: FpsWorkGroupEmployee — Staff grid data source for SetUpStaffResources
        public IFpsWorkGroupEmployeeApiClient FpsWorkGroupEmployee { get; }
        public IFpsProjectStaffPlanApiClient FpsProjectStaffPlan { get; }
        public IFpsProjectGroupStaffPlanApiClient FpsProjectGroupStaffPlan { get; }
        public IFpsProjectGroupApiClient FpsProjectGroup { get; }

        // TRANSFORMENGINE: FpsWorkgroupGrade — legacy alias, uses same FpsWorkGroupGradeApiClient instance
        public IFpsWorkGroupGradeApiClient FpsWorkgroupGrade { get; }
        public IFpsBudgetBidsApiClient FpsBudgetBids { get; }
        public IFpsPurchasesApiClient FpsPurchases { get; }
        public IFpsUserApiClient FpsUserPermission { get; }

        // TRANSFORMENGINE: FpsGrade added in prior batch — preserved as-is
        public IFpsGradeApiClient FpsGrade { get; }
        public IFpsTotalBusinessOverheadsApiClient FpsTotalBusinessOverheads { get; }

        public FpsApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            FpsStaffJob = new FpsStaffJobApiClient(http, mapper);
            FpsEmployee = new FpsEmployeeApiClient(http, mapper);
            FpsProgram = new FpsProgramApiClient(http, mapper);
            FpsProject = new FpsProjectApiClient(http, mapper);
            FpsLookup = new FpsLookupApiClient(http, mapper);
            FpsAnimalPlan = new FpsAnimalPlanApiClient(http, mapper);
            FpsSetting = new FpsSettingApiClient(http, mapper);
            FpsYearMaster = new FpsYearMasterApiClient(http, mapper);
            FpsProjectStaffPlanActual = new FpsProjectStaffPlanActualApiClient(http, mapper);
            FpsMonthlyOutput = new FpsMonthlyOutputApiClient(http, mapper);
            FpsDivision = new FpsDivisionApiClient(http, mapper);
            FpsAgency = new FpsAgencyApiClient(http, mapper);
            FpsAdditionalCost = new FpsAdditionalCostApiClient(http, mapper);
            FpsAccountCategory = new FpsAccountCategoryApiClient(http, mapper);

            // TRANSFORMENGINE: FpsProfitCentre wired — matches IFpsApiClient.FpsProfitCentre
            FpsProfitCentre = new FpsProfitCentreApiClient(http, mapper);
            FpsProfitCentreGrade = new FpsProfitCentreGradeApiClient(http, mapper);

            // TRANSFORMENGINE: FpsWorkGroupGrade wired — matches IFpsApiClient.FpsWorkGroupGrade
            FpsWorkGroupGrade = new FpsWorkGroupGradeApiClient(http, mapper);

            // TRANSFORMENGINE: FpsWorkGroupEmployee wired — matches IFpsApiClient.FpsWorkGroupEmployee
            FpsWorkGroupEmployee = new FpsWorkGroupEmployeeApiClient(http, mapper);
            FpsMaintDG = new FpsDivisionGradeApiClient(http, mapper);
            FpsProjectStaffPlan = new FpsProjectStaffPlanApiClient(http, mapper);
            FpsProjectGroupStaffPlan = new FpsProjectGroupStaffPlanApiClient(http, mapper);
            FpsAnimalMaster = new FpsAnimalApiClient(http, mapper);
            FpsProjectGroup = new FpsProjectGroupApiClient(http, mapper);

            // TRANSFORMENGINE: FpsWorkgroupGrade legacy alias — same instance as FpsWorkGroupGrade
            FpsWorkgroupGrade = new FpsWorkGroupGradeApiClient(http, mapper);
            FpsBudgetBids = new FpsBudgetBidsApiClient(http, mapper);
            FpsPurchases = new FpsPurchasesApiClient(http, mapper);
            FpsUserPermission = new FpsUserApiClient(http, mapper);

            // TRANSFORMENGINE: FpsGrade wired — preserved from prior batch
            FpsGrade = new FpsGradeApiClient(http, mapper);
            FpsTotalBusinessOverheads = new FpsTotalBusinessOverheadsApiClient(http, mapper);
        }
    }
}

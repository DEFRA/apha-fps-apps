using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Mapster;
namespace Apha.FPSApps.Web.Mappings
{
    public class FpsViewModelMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig(typeof(PaginationFilter<>), typeof(QueryParameters<>));
            config.NewConfig(typeof(QueryParameters<>), typeof(PaginationFilter<>));
            config.NewConfig<StaffJobItemViewModel, StaffJobViewDto>().TwoWays();
            config.NewConfig<PaginationModel, PaginationDto>().TwoWays();
            config.NewConfig<TestPriceCheckDto, TestPriceCheckItem>()
                .Ignore(d => d.IsDefraProjectList);
            config.NewConfig<TestPriceCheckItem, TestPriceCheckDto>();
            config.NewConfig<TestReqBreakdownItem, TestReqBreakdownDto>()
                .Map(d => d.Pc, s => s.PC)
                .Map(d => d.WgPrice, s => s.WGPrice);
            config.NewConfig<TestReqBreakdownDto, TestReqBreakdownItem>()
                .Map(d => d.PC, s => s.Pc)
                .Map(d => d.WGPrice, s => s.WgPrice);
            config.NewConfig<TestActualBreakdownItem, TestActualBreakdownDto>().TwoWays();
            config.NewConfig<ProgramViewModel, ProgramDto>().TwoWays();
            config.NewConfig<AnimalMaintenanceViewModel, AnimalDto>().TwoWays();
            config.NewConfig<UserPermissionViewModel, UserDto>().TwoWays();
            config.NewConfig<EmployeeViewModel, EmployeeDto>().TwoWays();
            config.NewConfig<StaffJobViewDto, StaffJobDto>().TwoWays();
            config.NewConfig<ProjectDto, ProjectViewModel>().TwoWays();
            config.NewConfig<ProjectSpecificQueryDto, ProjectSpecificQueryItem>().TwoWays();
            // The "Cust Inc" field binds to ProgramProjectEditViewModel.BudgetExt, but the value
            // persisted by the API is ProjectDto.CustIncome (ProjectDto.BudgetExt is read-only
            // display state). Map both directions explicitly so edits are not silently dropped.
            config.NewConfig<ProjectDto, ProgramProjectEditViewModel>()
                .Map(d => d.BudgetExt, s => s.CustIncome);
            config.NewConfig<ProgramProjectEditViewModel, ProjectDto>()
                .Map(d => d.CustIncome, s => s.BudgetExt ?? 0m);
            config.NewConfig<ProjectDto, ProgramProjectItem>()
                .Map(d => d.TransferIncome, s => s.TransferIncome);
            config.NewConfig<ProgramProjectItem, ProjectDto>();
            config.NewConfig<AnimalPlanItem, AnimalCostViewDto>().TwoWays();
            config.NewConfig<AnimalPlanItem, AnimalRequestDto>().TwoWays();
            config.NewConfig<AnimalCostsItem, AnimalCostViewDto>().TwoWays();
            config.NewConfig<AnimalSnapshotItem, AnimalSnapshotViewDto>().TwoWays();
            config.NewConfig<TimeSnapshotItem, ProgramPlanCostViewDto>().TwoWays();
            config.NewConfig<ProjectSnapshotItem, ProjectSnapshotViewDto>().TwoWays();
            config.NewConfig<TestSnapshotItem, TestFeePlanViewDto>().TwoWays();
            config.NewConfig<GenericBidItem, GenericBidViewDto>().TwoWays();
            config.NewConfig<ExceptionalCostSnapshotItem, ProjectExceptionalCostViewDto>().TwoWays();
            config.NewConfig<CompareStaff2Item, TimeCostCalcsViewDto>().TwoWays();
            config.NewConfig<ActualProjectCostItem, ProjectSubContractDto>().TwoWays();
            config.NewConfig<DivisionViewModel, DivisionDto>().TwoWays();
            config.NewConfig<DivisionGradeItem, DivisionGradeDto>().TwoWays();
            config.NewConfig<GradeItem, GradeDto>().TwoWays();

            // Maps CostCentreItem (DataGrid row: CostCentreNo double, ProfitCentre string) <-> CostCentreDto
            config.NewConfig<CostCentreItem, CostCentreDto>().TwoWays();

            config.NewConfig<ResourceCentreMaintenanceItem, ProfitCentreDto>().TwoWays();
            config.NewConfig<TestPlanItem, TestRequirementDto>().TwoWays();
            config.NewConfig<AdditionalCostItemViewModel, AdditionalCostDto>().TwoWays();
            config.NewConfig<AccountCategoryViewModel, AccountCategoryDto>().TwoWays();
            config.NewConfig<TestPlanActualItem, TestRequirementDto>().TwoWays();
            config.NewConfig<ActualTestOutputItem, MonthlyOutputDto>().TwoWays();

            // ProgrammeNewProject
            config.NewConfig<ProjectDto, ProgrammeNewProjectViewModel>().TwoWays();

            // PortfolioNew
            config.NewConfig<ProjectDto, PortfolioNewViewModel>().TwoWays();

            //Work Group Staff Maintenance
            config.NewConfig<WorkGroupEmployeeStaffItem, WorkGroupEmployeeStaffDto>().TwoWays();
            // Resource Set-Up
            config.NewConfig<SetUpStaffResourcesItem, WorkGroupEmployeeStaffDto>()
                .Ignore(d => d.PersonStatus)
                .Ignore(d => d.PersonClass)
                .Ignore(d => d.TimeRecorder)
                .Ignore(d => d.StartDate)
                .Ignore(d => d.EndDate)
                .Ignore(d => d.HoursPerWeek);
            config.NewConfig<WorkGroupEmployeeStaffDto, SetUpStaffResourcesItem>();
            config.NewConfig<WorkGroupEmployeeItem, WorkGroupEmployeeDto>().TwoWays();

            // ProfitCentreGradeMaint
            config.NewConfig<ProfitCentreGradeMaintItem, ProfitCentreGradeDto>().TwoWays();

            // BudgetResourceLevel
            config.NewConfig<BudgetResourceCentreLevelItem, BidViewDto>().TwoWays();
            config.NewConfig<PurchaseItem, PurchaseDto>().TwoWays();
            config.NewConfig<WorkGroupItem, WorkGroupDto>()
                .Map(d => d.WorkGroupName, s => s.WorkGroupName);
            config.NewConfig<WorkGroupDto, WorkGroupItem>()
                .Map(d => d.WorkGroup, s => s.WorkGroupName);

            // ProjectPlanViewer details grid
            // Budget maps to budget_cvl (the sole budget column in tlkpproject; there is no budget_ext DB column).
            config.NewConfig<ProjectDto, ProjectDetailsGridItem>()
                .Map(d => d.Status, s => s.ProjectStatus)
                .Map(d => d.Budget, s => s.BudgetCvl);

            // ProjectProfitability
            config.NewConfig<ProjectProfitabilityDto, ProjectProfitabilityItem>().TwoWays();

            // ProjectProfitabilityVla
            //   are expected to match ProjectProfitabilityVlaDto exactly (JobCode, Program, Customer,
            //   Manager, Status, StaffCosts, TestCost, AnimalCosts, AdditionalCosts, TotalCosts,
            //   Budget, Profit, TargetProfit, OffTarget, Id).
            //   ProjectProfitabilityVlaItem is defined in Phase 11; see DEFERRED note in file header.
            config.NewConfig<ProjectProfitabilityVlaDto, ProjectProfitabilityVlaItem>().TwoWays();

            // Staff Plan view
            config.NewConfig<StaffPlanViewItem, ProjectStaffPlanViewDto>().TwoWays();

            // Staff Plan Details view
            config.NewConfig<StaffPlanDetailsViewItem, ProjectStaffPlanDetailsViewDto>().TwoWays();

            // Project Group Staff Plan view
            config.NewConfig<ProjectGroupStaffPlanViewItem, ProjectGroupStaffPlanViewDto>().TwoWays();

            // Workgroup Staff Plan view
            config.NewConfig<WgStaffPlanViewItem, WgStaffPlanViewDto>().TwoWays();

            // Test Supplier
            config.NewConfig<TestSupplierItem, Apha.FPSApps.Application.Dtos.PACT.TestSupplierViewDto>().TwoWays();
            config.NewConfig<TestSupplierItem, TestRequirementDto>()
                .Map(d => d.TestCode, s => s.TestCode)
                .Map(d => d.Buyer, s => s.Buyer)
                .Map(d => d.UnitPrice, s => s.UnitPrice)
                .Map(d => d.NoRequired, s => s.NoRequired)
                .Map(d => d.ProjectBuyerCode, s => s.ProjectBuyerCode)
                .Map(d => d.TestBuyerCode, s => s.TestBuyerCode)
                .Map(d => d.Active, s => s.Active)
                .Map(d => d.RecUnitPrice, s => s.RecUnitPrice);
            config.NewConfig<TestRequirementDto, TestSupplierItem>();
            config.NewConfig<MaintWGGradeItem, WorkgroupGradeDto>().TwoWays();

            // Test Capability (FPS portfolio page — reuses PACT TestCapabilityDto)
            config.NewConfig<Apha.FPSApps.Web.Areas.FPS.Models.TestCapabilityItem, Apha.FPSApps.Application.Dtos.PACT.TestCapabilityDto>().TwoWays();

            // Plan Staff ZT Code
            config.NewConfig<PlanStaffZTCodeItemViewModel, StaffJobViewDto>().TwoWays();
            config.NewConfig<PlanStaffZTCodeItemViewModel, StaffJobDto>()
                .Map(d => d.StaffId, s => s.StaffID);
            config.NewConfig<StaffJobDto, PlanStaffZTCodeItemViewModel>();

            // Resource Allocation — Staff Jobs grid
            config.NewConfig<ResourceStaffJobDetailDto, ResourceStaffJobItem>()
                .Map(d => d.Project, s => s.JobCode)
                .Map(d => d.Description, s => s.JobDescription)
                .Map(d => d.Hour, s => s.PlannedHours)
                .Map(d => d.Status, s => s.ProjectStatus)
                .Ignore(d => d.StaffId);

            // Contribution Summary — row grid item
            config.NewConfig<ContributionSummaryRowDto, ContributionSummaryRowItem>().TwoWays();
            // Total Business Overheads
            config.NewConfig<TotalBusinessOverheadsViewModel, TotalBusinessOverheadsDto>().TwoWays();

            // Misc Project Data
            config.NewConfig<ProjectDto, ProjectMiscItem>()
                .Map(d => d.ParentProject, s => s.ParentProject)
                .Map(d => d.Program, s => s.Program)
                .Map(d => d.CostCentre, s => s.CostCentre)
                .Map(d => d.OracleProjectCode, s => s.OracleProjectCode)
                .Map(d => d.SubAccountCode, s => s.SubAccountCode);
            config.NewConfig<ProjectMiscItem, ProjectDto>();

            // all 5 *LogItem ViewModel types are created in Phase 11.
            // Audit log items are read-only grid rows — TwoWays() is intentionally omitted.

            // UserEmail is NOT in ProjectLogDto (requires backend UserId→email resolution); Ignore() it.
            config.NewConfig<ProjectLogDto, ProjectLogItem>()
                .Ignore(d => d.UserEmail);

            // UserEmail is NOT in StaffJobLogDto (requires UserId→email resolution); Ignore() it.
            config.NewConfig<StaffJobLogDto, StaffJobLogItem>()
                .Ignore(d => d.UserEmail);

            // UserEmail is NOT in TestRequirementLogDto (requires UserId→email resolution); Ignore() it.
            config.NewConfig<TestRequirementLogDto, TestRequirementLogItem>()
                .Ignore(d => d.UserEmail);

            // UserEmail is NOT in AnimalRequestLogDto (requires UserId→email resolution); Ignore() it.
            config.NewConfig<AnimalRequestLogDto, AnimalRequestLogItem>()
                .Ignore(d => d.UserEmail);

            // UserEmail is NOT in AdditionalCostLogDto (requires UserId→email resolution); Ignore() it.
            config.NewConfig<AdditionalCostLogDto, AdditionalCostLogItem>()
                .Ignore(d => d.UserEmail);

            // Bulk Rates — queue grid row. JobName mapped to its friendly display name; Status
            // to its friendly label (colour applied client-side in Index.cshtml via JS).
            config.NewConfig<BulkRatesQueueEntryDto, BulkRatesQueueGridItem>()
                .Map(d => d.JobName, s => FriendlyBulkRatesJobName(s.JobName))
                .Map(d => d.Status, s => BulkRatesStatusDisplay.FriendlyLabel(s.Status));

            // Bulk Rates — staging grids (Detail page). ValidationSummary has no source member —
            // it's populated after mapping, from BulkRatesUploadResultDto, by
            // BulkRatesController.BuildStagingGridConfig.
            config.NewConfig<BulkRatesFecStagingRowDto, FecStagingGridItem>()
                .Ignore(d => d.ValidationSummary);
            config.NewConfig<BulkRatesAgrupStagingRowDto, AgrupStagingGridItem>()
                .Map(d => d.Active, s => s.Active == 1)
                .Ignore(d => d.ValidationSummary);
            config.NewConfig<BulkRatesStaffStagingRowDto, StaffStagingGridItem>()
                .Ignore(d => d.ValidationSummary);
            config.NewConfig<BulkRatesAnimalStagingRowDto, AnimalStagingGridItem>()
                .Ignore(d => d.ValidationSummary);

            // TestListVla grid row ↔ DTO (frmTestList / fsubTest_MainList):
            config.NewConfig<TestorProductDto, TestListVlaItem>().TwoWays();

            // TestRCCost grid row ↔ DTO (fsubTestRCPrice / Component Charges general tab):
            config.NewConfig<TestRCCostItem, TestRCCostDto>().TwoWays();

            // TestRequirementRCCost grid row ↔ DTO (fsubTestRequirementRCPrice / Component Charges project tab):
            config.NewConfig<TestRequirementRCCostItem, TestRequirementRCCostDto>().TwoWays();

            // TestRequirementItem grid row ↔ DTO (Test Requirements tab — stage2TestRequirementsGrid):
            // Convention TwoWays(): Buyer, NoRequired, UnitPrice, TestCode, FpsYear all match DTO names.
            config.NewConfig<TestRequirementItem, TestRequirementDto>().TwoWays();

            // ResourceAllocation - Stage 2 Check Resource Allocation
            config.NewConfig<ResourceStaffAllocationDto, ResourceStaffAllocationItem>().TwoWays();
            config.NewConfig<ResourceStaffJobDto, ResourceStaffJobItem>().TwoWays();


            // DataGrid row: WorkgroupMaintenanceItem <-> WorkGroupDto (grid display and row selection)
            // All property names match exactly between Item and Dto (convention-based) — no explicit map needed.
            // Phase 11 adds [DataGridColumn] attributes to WorkgroupMaintenanceItem.
            config.NewConfig<WorkgroupMaintenanceItem, WorkGroupDto>().TwoWays();

            // Modal form ViewModel: WorkgroupMaintenanceViewModel does not map 1:1 to Dto —
            // the ViewModel wraps DataGridConfig<WorkgroupMaintenanceItem>; modal binding uses
            // WorkgroupMaintenanceItem directly from the grid row item. No ViewModel <-> Dto map needed.

            // ResourceMgmtReplan — Resource Re-allocation Screen (frmRM_RePlan)
            config.NewConfig<ResourceMgmtReplanViewDto, ResourceMgmtReplanGridItem>().TwoWays();
            config.NewConfig<ProjectStaffReplanDto, ResourceMgmtReplanGridItem>()
                .Map(d => d.StaffRowKey, s => $"{s.ParentProject}|{s.WgGrade}");
            config.NewConfig<ResourceMgmtReplanStaffJobDto, ResourceMgmtReplanAllTimeItem>().TwoWays();
            config.NewConfig<StaffJobViewDto, ResourceMgmtReplanAllTimeItem>()
                .Map(d => d.StaffId, s => s.StaffID);
            config.NewConfig<ResourceMgmtReplanStaffJobDto, ResourceMgmtReplanStagedItem>().TwoWays();

            config.NewConfig<Apha.FPSApps.Application.Dtos.FPS.BatchJobHistoryDto, YearEndHistoryItem>();
        }

        // Mapster's Map builds an expression tree, which cannot contain a switch
        // expression — a plain method call is used instead.
        private static string FriendlyBulkRatesJobName(string jobName) => jobName switch
        {
            "BulkTestRatesUpdate" => "FEC Test Rates",
            "BulkStaffRatesUpdate" => "Staff Rates",
            "BulkAnimalRatesUpdate" => "Animal Rates",
            _ => jobName
        };
    }
}

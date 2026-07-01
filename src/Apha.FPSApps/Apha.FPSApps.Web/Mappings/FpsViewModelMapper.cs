/*
 * TRANSFORMENGINE MIGRATION — FpsViewModelMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 10 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - Phase 10 (Step 15b): Added TRANSFORMENGINE TODO placeholder comments for Phase 11 ViewModel/Item
 *     <-> Dto mappings that cannot be activated until Phase 11 creates the ViewModel types.
 *   - Phase 11 (Steps 16-17): Activated all four deferred CreateMap entries now that ViewModel types exist:
 *       CreateMap<TestListVlaItem, TestListVlaDto>().ReverseMap()
 *       CreateMap<TestRCCostItem, TestRCCostDto>().ReverseMap()
 *       CreateMap<TestRequirementRCCostItem, TestRequirementRCCostDto>().ReverseMap()
 *       CreateMap<TestRequirementItem, TestRequirementDto>().ReverseMap()
 *
 * PRESERVED:
 *   - All existing CreateMap entries unchanged
 *   - All ForMember projections, file-level usings, and namespace
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If ViewModel property names differ from Dto (e.g. display-only fields or
 *     renamed grid columns), replace ReverseMap with explicit ForMember projections.
 *   - TRANSFORMENGINE TODO: TestRequirementItem ↔ TestRequirementDto — convention ReverseMap used;
 *     verify Buyer/NoRequired/UnitPrice/TestCode/FpsYear all round-trip correctly via AutoMapper.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
namespace Apha.FPSApps.Web.Mappings
{
    public class FpsViewModelMapper : Profile
    {
        public FpsViewModelMapper()
        {
            CreateMap(typeof(PaginationFilter<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap<StaffJobItemViewModel, StaffJobViewDto>().ReverseMap();
            CreateMap<PaginationModel, PaginationDto>().ReverseMap();
            CreateMap<TestPriceCheckDto, TestPriceCheckItem>()
                .ForMember(d => d.IsDefraProjectList, o => o.Ignore());
            CreateMap<TestPriceCheckItem, TestPriceCheckDto>();
            CreateMap<ProgramViewModel, ProgramDto>().ReverseMap();
            CreateMap<AnimalMaintenanceViewModel, AnimalDto>().ReverseMap();
            CreateMap<UserPermissionViewModel, UserDto>().ReverseMap();
            CreateMap<EmployeeViewModel, EmployeeDto>().ReverseMap();
            CreateMap<StaffJobViewDto, StaffJobDto>().ReverseMap();
            CreateMap<ProjectDto, ProjectViewModel>().ReverseMap();
            CreateMap<ProjectDto, ProgramProjectEditViewModel>().ReverseMap();
            CreateMap<ProjectDto, ProgramProjectItem>()
                .ForMember(d => d.TransferIncome, o => o.MapFrom(s => s.TransferIncome))
                .ReverseMap();
            CreateMap<AnimalPlanItem, AnimalCostViewDto>().ReverseMap();
            CreateMap<AnimalPlanItem, AnimalRequestDto>().ReverseMap();
            CreateMap<CompareStaff2Item, TimeCostCalcsViewDto>().ReverseMap();
            CreateMap<ActualProjectCostItem, ProjectSubContractDto>().ReverseMap();
            CreateMap<DivisionViewModel, DivisionDto>().ReverseMap();
            CreateMap<DivisionGradeItem, DivisionGradeDto>().ReverseMap();
            CreateMap<GradeItem, GradeDto>().ReverseMap();
            CreateMap<ResourceCentreMaintenanceItem, ProfitCentreDto>().ReverseMap();
            CreateMap<TestPlanItem, TestRequirementDto>().ReverseMap();
            CreateMap<AdditionalCostItemViewModel, AdditionalCostDto>().ReverseMap();
            CreateMap<AccountCategoryViewModel, AccountCategoryDto>().ReverseMap();
            CreateMap<TestPlanActualItem, TestRequirementDto>().ReverseMap();
            CreateMap<ActualTestOutputItem, MonthlyOutputDto>().ReverseMap();

            // ProgrammeNewProject
            CreateMap<ProjectDto, ProgrammeNewProjectViewModel>().ReverseMap();

            // PortfolioNew
            CreateMap<ProjectDto, PortfolioNewViewModel>().ReverseMap();

            // Resource Set-Up
            CreateMap<WorkGroupEmployeeItem, WorkGroupEmployeeDto>().ReverseMap();

            // ProfitCentreGradeMaint
            CreateMap<ProfitCentreGradeMaintItem, ProfitCentreGradeDto>().ReverseMap();

            // BudgetResourceLevel
            CreateMap<BudgetResourceCentreLevelItem, BidViewDto>().ReverseMap();
            CreateMap<PurchaseItem, PurchaseDto>().ReverseMap();
            CreateMap<WorkGroupItem, WorkGroupDto>()
                .ForMember(d => d.WorkGroupName, o => o.MapFrom(s => s.WorkGroupName))
                .ReverseMap()
                .ForMember(d => d.WorkGroup, o => o.MapFrom(s => s.WorkGroupName));

            // ProjectProfitability
            CreateMap<ProjectProfitabilityDto, ProjectProfitabilityItem>().ReverseMap();

            // ProjectProfitabilityVla
            // TRANSFORMENGINE: convention-mapped — all property names on ProjectProfitabilityVlaItem
            //   are expected to match ProjectProfitabilityVlaDto exactly (JobCode, Program, Customer,
            //   Manager, Status, StaffCosts, TestCost, AnimalCosts, AdditionalCosts, TotalCosts,
            //   Budget, Profit, TargetProfit, OffTarget, Id).
            //   ProjectProfitabilityVlaItem is defined in Phase 11; see DEFERRED note in file header.
            CreateMap<ProjectProfitabilityVlaDto, ProjectProfitabilityVlaItem>().ReverseMap();

            // Staff Plan view
            CreateMap<StaffPlanViewItem, ProjectStaffPlanViewDto>().ReverseMap();

            // Project Group Staff Plan view
            CreateMap<ProjectGroupStaffPlanViewItem, ProjectGroupStaffPlanViewDto>().ReverseMap();

            // Test Supplier
            CreateMap<TestSupplierItem, Apha.FPSApps.Application.Dtos.PACT.TestSupplierViewDto>().ReverseMap();
            CreateMap<TestSupplierItem, TestRequirementDto>()
                .ForMember(d => d.TestCode, o => o.MapFrom(s => s.TestCode))
                .ForMember(d => d.Buyer, o => o.MapFrom(s => s.Buyer))
                .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.UnitPrice))
                .ForMember(d => d.NoRequired, o => o.MapFrom(s => s.NoRequired))
                .ForMember(d => d.ProjectBuyerCode, o => o.MapFrom(s => s.ProjectBuyerCode))
                .ForMember(d => d.TestBuyerCode, o => o.MapFrom(s => s.TestBuyerCode))
                .ForMember(d => d.Active, o => o.MapFrom(s => s.Active))
                .ForMember(d => d.RecUnitPrice, o => o.MapFrom(s => s.RecUnitPrice))
                .ReverseMap();
            CreateMap<MaintWGGradeItem, WorkgroupGradeDto>().ReverseMap();

            // Test Capability (FPS portfolio page — reuses PACT TestCapabilityDto)
            CreateMap<Apha.FPSApps.Web.Areas.FPS.Models.TestCapabilityItem, Apha.FPSApps.Application.Dtos.PACT.TestCapabilityDto>().ReverseMap();

            // Plan Staff ZT Code
            CreateMap<PlanStaffZTCodeItemViewModel, StaffJobViewDto>().ReverseMap();
            CreateMap<PlanStaffZTCodeItemViewModel, StaffJobDto>()
                .ForMember(d => d.StaffId, o => o.MapFrom(s => s.StaffID))
                .ReverseMap();

            // Misc Project Data
            CreateMap<ProjectDto, ProjectMiscItem>()
                .ForMember(d => d.ParentProject, o => o.MapFrom(s => s.ParentProject))
                .ForMember(d => d.Program, o => o.MapFrom(s => s.Program))
                .ForMember(d => d.CostCentre, o => o.MapFrom(s => s.CostCentre))
                .ForMember(d => d.OracleProjectCode, o => o.MapFrom(s => s.OracleProjectCode))
                .ForMember(d => d.SubAccountCode, o => o.MapFrom(s => s.SubAccountCode))
                .ReverseMap();

            // TRANSFORMENGINE: Phase 11 (Steps 16-17) — activated ViewModel/Item ↔ Dto CreateMap entries.
            // All four types created in Phase 11; ReverseMap is convention-based (property names match Dto).

            // TestListVla grid row ↔ DTO (frmTestList / fsubTest_MainList):
            CreateMap<TestListVlaItem, TestListVlaDto>().ReverseMap();

            // TestRCCost grid row ↔ DTO (fsubTestRCPrice / Component Charges general tab):
            CreateMap<TestRCCostItem, TestRCCostDto>().ReverseMap();

            // TestRequirementRCCost grid row ↔ DTO (fsubTestequirementRCPrice / Component Charges project tab):
            CreateMap<TestRequirementRCCostItem, TestRequirementRCCostDto>().ReverseMap();

            // TestRequirementItem grid row ↔ DTO (Test Requirements tab — stage2TestRequirementsGrid):
            // Convention ReverseMap: Buyer, NoRequired, UnitPrice, TestCode, FpsYear all match DTO names.
            CreateMap<TestRequirementItem, TestRequirementDto>().ReverseMap();
        }
    }
}

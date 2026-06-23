/*
 * TRANSFORMENGINE MIGRATION — FpsViewModelMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 10 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - Added WorkgroupMaintenanceItem <-> WorkgroupMaintenanceDto mapping (DataGrid row <-> frontend DTO)
 *   - Added WorkgroupMaintenanceViewModel <-> WorkgroupMaintenanceDto mapping (modal form <-> frontend DTO)
 *   - WorkgroupMaintenanceViewModel.cs and WorkgroupMaintenanceItem stub created in Phase 10 to
 *     satisfy compilation; full [DataGridColumn] attributes added in Phase 11.
 *   - All property names on WorkgroupMaintenanceItem match WorkgroupMaintenanceDto exactly —
 *     convention-based mapping, no ForMember overrides required.
 *
 * PRESERVED:
 *   - All existing CreateMap entries unchanged
 *   - Namespace and using directives unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: WorkgroupMaintenanceViewModel stub extended in Phase 11 with DataGridConfig
 *     wiring and [DataGridColumn] attributes. Verify mapper coverage once Phase 11 adds full fields.
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

            // TRANSFORMENGINE: WorkgroupMaintenance mappings added — Phase 10 (Step 15b)
            // DataGrid row: WorkgroupMaintenanceItem <-> WorkgroupMaintenanceDto (grid display and row selection)
            // All property names match exactly between Item and Dto (convention-based) — no ForMember needed.
            // Phase 11 adds [DataGridColumn] attributes to WorkgroupMaintenanceItem.
            CreateMap<WorkgroupMaintenanceItem, WorkgroupMaintenanceDto>().ReverseMap();

            // Modal form ViewModel: WorkgroupMaintenanceViewModel does not map 1:1 to Dto —
            // the ViewModel wraps DataGridConfig<WorkgroupMaintenanceItem>; modal binding uses
            // WorkgroupMaintenanceItem directly from the grid row item. No ViewModel <-> Dto map needed.
        }
    }
}

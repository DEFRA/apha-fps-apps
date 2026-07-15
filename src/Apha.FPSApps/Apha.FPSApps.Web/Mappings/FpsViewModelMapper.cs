/*
 * TRANSFORMENGINE MIGRATION — FpsViewModelMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 4 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Added TRANSFORMENGINE migration header (Phase 4 update pass)
 *   - Verified WorkGroupEmployeeStaffItem <-> WorkGroupEmployeeStaffDto entry present (line ~51)
 *   - Verified WorkGroupEmployeeItem <-> WorkGroupEmployeeDto entry present (line ~53)
 *   - SetUpStaffResources Phase 5 ViewModel/Item entries deferred (models not yet created)
 *   - Inline TRANSFORMENGINE comment added to SetUpStaffResources section for Phase 5 follow-up
 *   - Phase 6 (2026-07-07): CreateMap<SetUpStaffResourcesItem, WorkGroupEmployeeStaffDto>().ReverseMap() added
 *     now that SetUpStaffResourcesItem model exists (created in Phase 6 as prerequisite for views)
 *
 * PRESERVED:
 *   - All existing CreateMap entries (StaffJob, Program, AnimalMaintenance, UserPermission,
 *     Employee, Project, AnimalPlan, CompareStaff2, ActualProjectCost, Division, DivisionGrade,
 *     Grade, ResourceCentreMaintenance, TestPlan, AdditionalCost, AccountCategory,
 *     ProgrammeNewProject, PortfolioNew, WorkGroupEmployee, ProfitCentreGradeMaint,
 *     BudgetResourceLevel, Purchase, WorkGroup, ProjectProfitability,
 *     ProjectProfitabilityVla, StaffPlanView, ProjectGroupStaffPlanView,
 *     TestSupplier, MaintWGGrade, TestCapability, PlanStaffZTCode, ProjectMisc mappings)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: SetUpStaffResourcesViewModel has no direct Dto mirror (it is a composite view model);
 *     controller maps via SetUpStaffResourcesItem <-> WorkGroupEmployeeStaffDto for grid rows only.
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
            CreateMap<TestReqBreakdownItem, TestReqBreakdownDto>()
                .ForMember(d => d.Pc, o => o.MapFrom(s => s.PC))
                .ForMember(d => d.WgPrice, o => o.MapFrom(s => s.WGPrice))
                .ReverseMap()
                .ForMember(d => d.PC, o => o.MapFrom(s => s.Pc))
                .ForMember(d => d.WGPrice, o => o.MapFrom(s => s.WgPrice));
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

            // Work Group Staff Maintenance
            // TRANSFORMENGINE: Phase 4 VERIFIED — WorkGroupEmployeeStaffItem <-> WorkGroupEmployeeStaffDto
            //   present; covers SetUpStaffResources staff-grid row shape (PactId, SpNumber, WorkGroupGrade,
            //   Name, PersonStatus, PersonClass, HrsPaid, Leave, SickSpecial, HrsAvail, MakeAvailable,
            //   TimeRecorder, StartDate, EndDate, HoursPerWeek).
            CreateMap<WorkGroupEmployeeStaffItem, WorkGroupEmployeeStaffDto>().ReverseMap();
            // Resource Set-Up
            // TRANSFORMENGINE: Phase 4 VERIFIED — WorkGroupEmployeeItem <-> WorkGroupEmployeeDto present.
            // TRANSFORMENGINE: Phase 6 — SetUpStaffResourcesItem <-> WorkGroupEmployeeStaffDto added now that model exists.
            //   Subset mapping: SetUpStaffResourcesItem carries only the 7 staff-grid fields from the HTML prototype.
            //   Fields not in SetUpStaffResourcesItem (PersonStatus, PersonClass, TimeRecorder, StartDate, EndDate,
            //   HoursPerWeek) are ignored on the Item→Dto direction; preserved from Dto source on Dto→Item direction.
            CreateMap<SetUpStaffResourcesItem, WorkGroupEmployeeStaffDto>()
                .ForMember(d => d.PersonStatus,   o => o.Ignore())
                .ForMember(d => d.PersonClass,     o => o.Ignore())
                .ForMember(d => d.TimeRecorder,    o => o.Ignore())
                .ForMember(d => d.StartDate,       o => o.Ignore())
                .ForMember(d => d.EndDate,         o => o.Ignore())
                .ForMember(d => d.HoursPerWeek,    o => o.Ignore())
                .ReverseMap();
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

            // Contribution Summary — row grid item
            CreateMap<ContributionSummaryRowDto, ContributionSummaryRowItem>().ReverseMap();
            // Total Business Overheads
            CreateMap<TotalBusinessOverheadsViewModel, TotalBusinessOverheadsDto>().ReverseMap();

            // Misc Project Data
            CreateMap<ProjectDto, ProjectMiscItem>()
                .ForMember(d => d.ParentProject, o => o.MapFrom(s => s.ParentProject))
                .ForMember(d => d.Program, o => o.MapFrom(s => s.Program))
                .ForMember(d => d.CostCentre, o => o.MapFrom(s => s.CostCentre))
                .ForMember(d => d.OracleProjectCode, o => o.MapFrom(s => s.OracleProjectCode))
                .ForMember(d => d.SubAccountCode, o => o.MapFrom(s => s.SubAccountCode))
                .ReverseMap();

            // all 5 *LogItem ViewModel types are created in Phase 11.
            // Audit log items are read-only grid rows — .ReverseMap() is intentionally omitted.

            // UserEmail is NOT in ProjectLogDto (requires backend UserId→email resolution); Ignore() it.
            CreateMap<ProjectLogDto, ProjectLogItem>()
                .ForMember(d => d.UserEmail, o => o.Ignore());

            // UserEmail is NOT in StaffJobLogDto (requires UserId→email resolution); Ignore() it.
            CreateMap<StaffJobLogDto, StaffJobLogItem>()
                .ForMember(d => d.UserEmail, o => o.Ignore());

            // UserEmail is NOT in TestRequirementLogDto (requires UserId→email resolution); Ignore() it.
            CreateMap<TestRequirementLogDto, TestRequirementLogItem>()
                .ForMember(d => d.UserEmail, o => o.Ignore());

            // UserEmail is NOT in AnimalRequestLogDto (requires UserId→email resolution); Ignore() it.
            CreateMap<AnimalRequestLogDto, AnimalRequestLogItem>()
                .ForMember(d => d.UserEmail, o => o.Ignore());

            // UserEmail is NOT in AdditionalCostLogDto (requires UserId→email resolution); Ignore() it.
            CreateMap<AdditionalCostLogDto, AdditionalCostLogItem>()
                .ForMember(d => d.UserEmail, o => o.Ignore());
        }
    }
}

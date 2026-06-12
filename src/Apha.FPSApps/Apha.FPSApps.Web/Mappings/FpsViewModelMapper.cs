// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — FpsViewModelMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 10 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Updated CreateMap<WorkGroupEmployeeItem, WorkGroupEmployeeDto> to add explicit
 *     ForMember overrides for MakeAvailable (bool Item <-> int Dto) in both directions.
 *     Without this override AutoMapper throws an InvalidOperationException at runtime when
 *     mapping MakeAvailable because bool and int are not implicitly assignable.
 *   - Phase 11 UPDATE: Added ForMember for TimeRecorder (bool Item <-> int Dto) to match
 *     MakeAvailable pattern. Phase 10 deferred this until Phase 11 added the property to Item.
 *   - Phase 11 UPDATE: Added ForMember for StaffName (Item) <-> Name (Dto). WorkGroupEmployeeItem
 *     uses StaffName to match JS column key 'staffName'; DTO uses Name.
 *   - Phase 11 UPDATE: Added ForMember for WgGrade (Item) <-> WorkGroupGrade (Dto). Item uses
 *     WgGrade to match JS column key 'wgGrade'; DTO uses WorkGroupGrade.
 *   - Phase 11 UPDATE: Added ForMember for SpNumber (Item nullable string?) <-> SpNumber (Dto string).
 *     Dto.SpNumber is not null but Item.SpNumber is string? — explicit null-coalescing added.
 *
 * PRESERVED:
 *   - All existing CreateMap entries unchanged
 *   - Namespace Apha.FPSApps.Web.Mappings unchanged
 *   - All lookup, pagination, and domain-model mapper registrations unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Once MaintWGStaffController is wired end-to-end, run
 *     AssertConfigurationIsValid() to confirm all mapped members resolve without warnings.
 *   - TRANSFORMENGINE TODO: WorkGroupEmployeeItem.PersonStatus maps to WorkGroupEmployeeDto.PersonStatus
 *     (same name); WorkGroupEmployeeItem.PersonClass maps to WorkGroupEmployeeDto.PersonClass
 *     (same name) — AutoMapper handles these by convention. Verify no unmapped-member warning.
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
            CreateMap<ResourceCentreMaintenanceItem, ProfitCentreDto>().ReverseMap();
            CreateMap<TestPlanItem, TestRequirementDto>().ReverseMap();
            CreateMap<AdditionalCostItemViewModel, AdditionalCostDto>().ReverseMap();
            CreateMap<TestPlanActualItem, TestRequirementDto>().ReverseMap();
            CreateMap<ActualTestOutputItem, MonthlyOutputDto>().ReverseMap();

            // ProgrammeNewProject
            CreateMap<ProjectDto, ProgrammeNewProjectViewModel>().ReverseMap();

            // Resource Set-Up
            // TRANSFORMENGINE: Phase 10 UPDATE — WorkGroupEmployeeItem.MakeAvailable is bool (checkbox);
            // WorkGroupEmployeeDto.MakeAvailable is int (0/1). Explicit ForMember required both ways.
            // TRANSFORMENGINE: Phase 11 UPDATE — TimeRecorder (bool Item <-> int Dto) added.
            //   StaffName (Item) <-> Name (Dto) added — JS key 'staffName' vs DTO field 'Name'.
            //   WgGrade (Item) <-> WorkGroupGrade (Dto) added — JS key 'wgGrade' vs DTO 'WorkGroupGrade'.
            //   SpNumber (Item string?) <-> SpNumber (Dto string) added — explicit null coalesce.
            //   StartDate, EndDate, HoursPerWeek, HrsAvail, PersonStatus, PersonClass — auto-mapped by name.
            CreateMap<WorkGroupEmployeeItem, WorkGroupEmployeeDto>()
                .ForMember(d => d.MakeAvailable,   o => o.MapFrom(s => s.MakeAvailable ? 1 : 0))
                .ForMember(d => d.TimeRecorder,    o => o.MapFrom(s => s.TimeRecorder ? 1 : 0))
                .ForMember(d => d.Name,            o => o.MapFrom(s => s.StaffName ?? string.Empty))
                .ForMember(d => d.WorkGroupGrade,  o => o.MapFrom(s => s.WgGrade ?? string.Empty))
                .ForMember(d => d.SpNumber,        o => o.MapFrom(s => s.SpNumber ?? string.Empty))
                .ReverseMap()
                .ForMember(d => d.MakeAvailable,   o => o.MapFrom(s => s.MakeAvailable != 0))
                .ForMember(d => d.TimeRecorder,    o => o.MapFrom(s => s.TimeRecorder != 0))
                .ForMember(d => d.StaffName,       o => o.MapFrom(s => s.Name))
                .ForMember(d => d.WgGrade,         o => o.MapFrom(s => s.WorkGroupGrade))
                .ForMember(d => d.SpNumber,        o => o.MapFrom(s => s.SpNumber));

            // ProfitCentreGradeMaint
            CreateMap<ProfitCentreGradeMaintItem, ProfitCentreGradeDto>().ReverseMap();

            // ProjectProfitability
            CreateMap<ProjectProfitabilityDto, ProjectProfitabilityItem>().ReverseMap();

            // Staff Plan view
            CreateMap<StaffPlanViewItem, ProjectStaffPlanViewDto>().ReverseMap();

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
        }
    }
}

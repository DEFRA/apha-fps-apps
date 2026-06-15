// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — EntityMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-15
 *
 * CHANGED:
 *   - Added CreateMap<ProjectProfitabilityVlaView, ProjectProfitabilityVlaDto>().ReverseMap()
 *     for the frmJobcodeTotalsVLA form migration.
 *   - All property names on ProjectProfitabilityVlaView and ProjectProfitabilityVlaDto
 *     are deliberately aligned (StaffCosts, TestCost, AnimalCosts, AdditionalCosts,
 *     TotalCosts, Budget, Profit, TargetProfit, OffTarget, JobCode, Program, Customer,
 *     Manager, Status, Id) so AutoMapper convention mapping resolves without explicit
 *     ForMember calls.
 *
 * PRESERVED:
 *   - All 48 existing CreateMap registrations unchanged.
 *   - Existing pagination generic maps: PaginationParameters<>/QueryParameters<>,
 *     PagedData<>/PaginatedResult<>.
 *   - All ProfitCentre, WorkGroup, Employee, Division, Agency, and base
 *     ProjectProfitability mappings preserved.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: if the PostgreSQL view omits the Id column (no ROW_NUMBER()),
 *     remove Id from both ProjectProfitabilityVlaView and ProjectProfitabilityVlaDto
 *     and update this mapping accordingly. The ReverseMap() is safe to keep regardless.
 *   - TRANSFORMENGINE TODO: confirm no custom ForMember overrides are needed once the
 *     Phase 4 view DDL is finalised and column names are confirmed.
 */

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Mappings
{
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            CreateMap(typeof(PaginationParameters<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PagedData<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<PaginationData, PaginationDto>().ReverseMap();
            CreateMap<StaffJobView, StaffJobViewDto>().ReverseMap();
            CreateMap<StaffJobZtView, StaffJobZtViewDto>()
                .ForMember(dest => dest.ZtDescription, opt => opt.MapFrom(src => src.Name))
                .ReverseMap();
            CreateMap<StaffWorkgroupLookup, StaffWorkgroupLookupDto>().ReverseMap();
            CreateMap<StaffJob, StaffJobDto>().ReverseMap();
            CreateMap<FpsSetting, FpsSettingDto>().ReverseMap();
            CreateMap<Program, ProgramDto>().ReverseMap();
            CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<ProjectView, Project>().ReverseMap();
            CreateMap<JobCode, JobCodeDto>().ReverseMap();
            CreateMap<JobCode, Apha.Common.Contracts.FPS.JobCodeRes>().ReverseMap();
            CreateMap<Contract, ContractDto>().ReverseMap();
            CreateMap<AnimalCostView, AnimalCostViewDto>().ReverseMap();
            CreateMap<Animal, AnimalDto>().ReverseMap();
            CreateMap<AnimalRequest, AnimalRequestDto>().ReverseMap();
            CreateMap<AccountCode, AccountCodeDto>().ReverseMap();
            CreateMap<SubAccount, SubAccountDto>().ReverseMap();
            CreateMap<ProjectGroup, ProjectGroupDto>().ReverseMap();
            CreateMap<Employee, EmployeeDto>().ReverseMap();
            CreateMap<Manager, ManagerDto>().ReverseMap();
            CreateMap<ProjectView, ProjectDto>().ReverseMap();
            CreateMap<PactProjectView, ProjectDto>()
                .ForMember(d => d.FpsCalYear, o => o.MapFrom(s => s.FpsYear))
                .ReverseMap()
                .ForMember(d => d.FpsYear, o => o.MapFrom(s => s.FpsCalYear));
            CreateMap<YearMaster, YearMasterDto>().ReverseMap();
            CreateMap<Division, DivisionDto>().ReverseMap();
            CreateMap<DivisionGrade, DivisionGradeDto>().ReverseMap();
            CreateMap<Agency, AgencyDto>().ReverseMap();
            CreateMap<TimeCostCalcsView, TimeCostCalcsViewDto>().ReverseMap();
            CreateMap<ProjectStaffPlanView, ProjectStaffPlanViewDto>().ReverseMap();
            CreateMap<ProjectGroupStaffPlanView, ProjectGroupStaffPlanViewDto>().ReverseMap();
            CreateMap<AdditionalCost, AdditionalCostDto>().ReverseMap();
            CreateMap<AccountCategory, AccountCategoryDto>().ReverseMap();
            CreateMap<WorkGroupStaff, WorkGroupStaffDto>().ReverseMap();
            CreateMap<WorkGroupPerson, WorkGroupPersonDto>().ReverseMap();
           

            // ResourceSetUp
            CreateMap<ProfitCentre, ProfitCentreDto>().ReverseMap();
            CreateMap<ProfitCentreView, ProfitCentreDto>()
                .ForMember(d => d.ProfitCentreId, o => o.MapFrom(s => s.ProfitCentreId))
                .ForMember(d => d.ProfitCentreName, o => o.MapFrom(s => s.ProfitCentreName))
                .ForMember(d => d.Division, o => o.MapFrom(s => s.Division))
                .ForMember(d => d.ContTarget, o => o.MapFrom(s => s.ContTarget))
                .ForMember(d => d.ProfitCentreHead, o => o.MapFrom(s => s.ProfitCentreHead))
                .ForMember(d => d.DivisionId, o => o.MapFrom(s => s.DivisionId))
                .ForMember(d => d.EmailRecipient, o => o.MapFrom(s => s.EmailRecipient));
            CreateMap<ProfitCentreCostSummary, ProfitCentreCostDto>().ReverseMap();
            CreateMap<ProfitCentreGrade, ProfitCentreGradeDto>().ReverseMap();
            CreateMap<WorkgroupGrade, WorkgroupGradeDto>().ReverseMap();
            CreateMap<WorkGroupGradeView, WorkgroupGradeDto>().ReverseMap();
            CreateMap<WorkGroupEmployee, WorkGroupEmployeeDto>().ReverseMap();
            CreateMap<WorkGroupEmployeeView, WorkGroupEmployeeDto>().ReverseMap();
            CreateMap<PactStaff, PactStaffDto>().ReverseMap();
            CreateMap<ProjectProfitabilityView, ProjectProfitabilityDto>().ReverseMap();
            CreateMap<MonthlyOutput, MonthlyOutputDto>().ReverseMap();

            // TRANSFORMENGINE: new mapping — frmJobcodeTotalsVLA migration (Phase 3)
            //   Property names are aligned between entity and DTO; no ForMember overrides needed.
            //   Covers: Id, JobCode, Program, Customer, Manager, Status, StaffCosts, TestCost,
            //   AnimalCosts, AdditionalCosts, TotalCosts, Budget, Profit, TargetProfit, OffTarget.
            CreateMap<ProjectProfitabilityVlaView, ProjectProfitabilityVlaDto>().ReverseMap();
        }
    }
}

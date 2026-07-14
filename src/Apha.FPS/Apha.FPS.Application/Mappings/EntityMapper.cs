/*
 * TRANSFORMENGINE MIGRATION — EntityMapper.cs (updated)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - Added CreateMap registrations for all six new DepartmentIncome entity <-> DTO pairs:
 *       DepartmentIncomeTime       <-> DepartmentIncomeTimeDto
 *       DepartmentIncomeTest       <-> DepartmentIncomeTestDto
 *       DepartmentIncomeAnimal     <-> DepartmentIncomeAnimalDto
 *       DepartmentIncomeAdditional <-> DepartmentIncomeAdditionalDto
 *       DepartmentIncomeTotals     <-> DepartmentIncomeTotalsDto
 *       PeriodLookup               <-> PeriodLookupDto
 *   - All properties are name-matched between entity and DTO; no ForMember overrides required
 *
 * PRESERVED:
 *   - All existing CreateMap registrations unchanged
 *   - Existing ForMember overrides (Grade, PactProjectView, TotalBusinessOverheads, etc.) preserved exactly
 *
 * DEFERRED: none — fully automated.
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
            CreateMap<Grade, GradeDto>()
                .ForMember(d => d.Description, o => o.MapFrom(s => s.DescLong))
                .ReverseMap()
                .ForMember(d => d.DescLong, o => o.MapFrom(s => s.Description));

            CreateMap<Agency, AgencyDto>().ReverseMap();
            CreateMap<TimeCostCalcsView, TimeCostCalcsViewDto>().ReverseMap();
            CreateMap<ProjectStaffPlanView, ProjectStaffPlanViewDto>().ReverseMap();
            CreateMap<ProjectGroupStaffPlanView, ProjectGroupStaffPlanViewDto>().ReverseMap();
            CreateMap<AdditionalCost, AdditionalCostDto>().ReverseMap();
            CreateMap<AccountCategory, AccountCategoryDto>().ReverseMap();
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
            //ProjectProfitabilityVlaView
            CreateMap<ProjectProfitabilityVlaView, ProjectProfitabilityVlaDto>().ReverseMap();

            CreateMap<User, UserDto>().ReverseMap();

            // BudgetResourceLevel
            CreateMap<Bid, BidDto>().ReverseMap();
            CreateMap<BidView, BidViewDto>().ReverseMap();
            CreateMap<Purchase, PurchaseDto>().ReverseMap();
            //   All 5 log entities from fps schema partitioned tables.
            //   Property names are fully aligned between entity and DTO; no ForMember overrides needed.
            //   Covers all columns: sequenceno, parentproject/jobcode/testcode, date range, user tracking fields, fpsyear.
            CreateMap<ProjectLog, ProjectLogDto>().ReverseMap();
            CreateMap<StaffJobLog, StaffJobLogDto>().ReverseMap();
            CreateMap<TestRequirementLog, TestRequirementLogDto>().ReverseMap();
            CreateMap<AnimalRequestLog, AnimalRequestLogDto>().ReverseMap();
            CreateMap<AdditionalCostLog, AdditionalCostLogDto>().ReverseMap();
            // MaintTotalBusinessOverheads
            CreateMap<TotalBusinessOverheads, TotalBusinessOverheadsDto>()
                .ForMember(d => d.TotalBusinessOverheads, o => o.MapFrom(s => s.BusinessOverheads))
                .ReverseMap()
                .ForMember(d => d.BusinessOverheads, o => o.MapFrom(s => s.TotalBusinessOverheads));

            // TRANSFORMENGINE: DepartmentIncome entity <-> DTO mappings (Phase 3)
            // All six pairs are property-name aligned between entity and DTO; no ForMember overrides required.
            // Covers all query types from frmDeptIncome: Time, Tests, Animals, Additional, Totals, plus PeriodLookup.
            CreateMap<DepartmentIncomeTime,       DepartmentIncomeTimeDto>().ReverseMap();
            CreateMap<DepartmentIncomeTest,       DepartmentIncomeTestDto>().ReverseMap();
            CreateMap<DepartmentIncomeAnimal,     DepartmentIncomeAnimalDto>().ReverseMap();
            CreateMap<DepartmentIncomeAdditional, DepartmentIncomeAdditionalDto>().ReverseMap();
            CreateMap<DepartmentIncomeTotals,     DepartmentIncomeTotalsDto>().ReverseMap();
            CreateMap<PeriodLookup,               PeriodLookupDto>().ReverseMap();
        }
    }
}

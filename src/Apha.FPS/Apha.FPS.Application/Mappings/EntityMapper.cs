/*
 * TRANSFORMENGINE MIGRATION — EntityMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - Added three new CreateMap entries for the TestListVla resource family:
 *     TestOrProduct <-> TestListVlaDto
 *     TestRCCost <-> TestRCCostDto
 *     TestRequirementRCCost <-> TestRequirementRCCostDto
 *   - All three mappings use .ReverseMap() — all property names are aligned between entity and DTO
 *
 * PRESERVED:
 *   - All pre-existing CreateMap registrations unchanged
 *   - Generic pagination mappings (PaginationParameters<>, PagedData<>) unchanged
 *   - All ForMember overrides (Grade<->GradeDto, StaffJobZtView, PactProjectView) unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — mappings fully automated; no ForMember overrides required
 *     as entity and DTO property names are identical for all three new pairs.
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

            // TRANSFORMENGINE: Grade <-> GradeDto — ForMember required: Grade.DescLong <-> GradeDto.Description (field rename)
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

            // TRANSFORMENGINE: new mapping â€” frmJobcodeTotalsVLA migration (Phase 3)
            //   Property names are aligned between entity and DTO; no ForMember overrides needed.
            //   Covers: Id, JobCode, Program, Customer, Manager, Status, StaffCosts, TestCost,
            //   AnimalCosts, AdditionalCosts, TotalCosts, Budget, Profit, TargetProfit, OffTarget.
            CreateMap<ProjectProfitabilityVlaView, ProjectProfitabilityVlaDto>().ReverseMap();
              

            // UserPermission
            CreateMap<User, UserDto>().ReverseMap();

            // BudgetResourceLevel
            CreateMap<Bid, BidDto>().ReverseMap();
            CreateMap<BidView, BidViewDto>().ReverseMap();
            CreateMap<Purchase, PurchaseDto>().ReverseMap();

            // TRANSFORMENGINE: TestListVla migration (Phase 3) — frmTestList / fsubTest_MainList
            //   TestOrProduct <-> TestListVlaDto: all property names aligned; no ForMember overrides needed.
            //   Covers composite PK (ItemCode, FpsYear) plus all VLA pricing and descriptor fields.
            CreateMap<TestOrProduct, TestListVlaDto>().ReverseMap();

            // TRANSFORMENGINE: TestRCCost migration (Phase 3) — fsubTestRCPrice component charges tab
            //   TestRCCost <-> TestRCCostDto: all property names aligned; no ForMember overrides needed.
            //   Covers composite PK (TestCode, ProfitCentre, FpsYear) and Price.
            CreateMap<TestRCCost, TestRCCostDto>().ReverseMap();

            // TRANSFORMENGINE: TestRequirementRCCost migration (Phase 3) — fsubTestequirementRCPrice project charges tab
            //   TestRequirementRCCost <-> TestRequirementRCCostDto: all property names aligned; no ForMember overrides needed.
            //   Covers composite PK (TestCode, Buyer, ProfitCentre, FpsYear) and Price.
            CreateMap<TestRequirementRCCost, TestRequirementRCCostDto>().ReverseMap();
        }
    }
}

/*
 * TRANSFORMENGINE MIGRATION — RequestMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 6 — Backend Readiness Gate - Route + Contract + Mapper Confirmation
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - Phase 5: Added 6 new AutoMapper profiles for TestListVla, TestRCCost, TestRequirementRCCost:
 *     TestListVlaReq <-> TestListVlaDto
 *     TestListVlaRes <-> TestListVlaDto
 *     TestRCCostReq <-> TestRCCostDto
 *     TestRCCostRes <-> TestRCCostDto
 *     TestRequirementRCCostReq <-> TestRequirementRCCostDto
 *     TestRequirementRCCostRes <-> TestRequirementRCCostDto
 *   - Phase 5: PaginatedResult<TestListVlaDto> -> PaginationRes<TestListVlaRes> added for paged list endpoint
 *   - Phase 6: Readiness gate confirmed — all 7 new mappings verified present and sufficient
 *   - Phase 6: Req->Dto coverage: TestListVlaReq↔TestListVlaDto, TestRCCostReq↔TestRCCostDto, TestRequirementRCCostReq↔TestRequirementRCCostDto
 *   - Phase 6: Res->Dto coverage: TestListVlaRes↔TestListVlaDto, TestRCCostRes↔TestRCCostDto, TestRequirementRCCostRes↔TestRequirementRCCostDto
 *   - Phase 6: Pagination coverage: PaginatedResult<TestListVlaDto> -> PaginationRes<TestListVlaRes> for GetAll paged endpoint
 *   - Phase 6: No ForMember projections required — all fields are flat 1:1 across Req/Res/Dto for these 3 resource families
 *
 * PRESERVED:
 *   - All existing mappings (StaffJob, Animal, Employee, Program, Project, Contract,
 *     Division, Grade, ProfitCentre, WorkGroup, BudgetBids, Purchases, User, etc.)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If TestListVlaRes / TestListVlaReq add display-only computed fields
 *     in future, add ForMember projections here.
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using AutoMapper;

namespace Apha.FPS.Api.Mappings
{
    public class RequestMapper : Profile
    {
        public RequestMapper()
        {
            CreateMap(typeof(PaginationReq<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<Pagination, PaginationDto>().ReverseMap();

            CreateMap<StaffJobViewDto, StaffJobViewRes>().ReverseMap();
            CreateMap<StaffJobZtViewDto, StaffJobZtViewRes>().ReverseMap();
            CreateMap<StaffWorkgroupLookupDto, StaffWorkgroupLookupRes>().ReverseMap();
            CreateMap<StaffJobDto, StaffJobReq>().ReverseMap();
            CreateMap<StaffJobDto, StaffJobRes>().ReverseMap();

            CreateMap<FpsSettingRes, FpsSettingDto>().ReverseMap();

            CreateMap<AnimalCostViewDto, AnimalCostViewRes>().ReverseMap();
            CreateMap<AnimalDto, AnimalRes>().ReverseMap();
            CreateMap<AnimalReq, AnimalDto>().ReverseMap();
            CreateMap<AnimalRequestDto, AnimalRequestReq>().ReverseMap();
            CreateMap<AnimalRequestDto, AnimalRequestRes>().ReverseMap();
            CreateMap<EmployeeDto, EmployeeReq>().ReverseMap();
            CreateMap<EmployeeDto, EmployeeRes>().ReverseMap();
            CreateMap<ManagerDto, ManagerRes>().ReverseMap();
            CreateMap<ProgramReq, ProgramDto>().ReverseMap();
            CreateMap<ProgramRes, ProgramDto>().ReverseMap();

            CreateMap<ProjectDto, ProjectReq>()
                .ForMember(d => d.BudgetExt, o => o.MapFrom(s => s.CustIncome)).ReverseMap()
                .ForMember(d => d.CustIncome, o => o.MapFrom(s => s.BudgetExt));
            CreateMap<ProjectDto, ProjectRes>()
                .ForMember(d => d.BudgetExt, o => o.MapFrom(s => s.CustIncome)).ReverseMap()
                .ForMember(d => d.CustIncome, o => o.MapFrom(s => s.BudgetExt));

            // TRANSFORMENGINE: VLA profitability mappings — frmJobcodeTotalsVLA Phase 5
            //   JobCode (DTO natural key) -> Project (response display column per HTML prototype)
            //   Id is int? in DTO (nullable ROW_NUMBER) -> int in Res (non-nullable contract property)
            CreateMap<ProjectProfitabilityVlaDto, ProjectProfitabilityVlaRes>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.GetValueOrDefault(0)))
                .ForMember(d => d.Project, o => o.MapFrom(s => s.JobCode));
            CreateMap<PaginatedResult<ProjectProfitabilityVlaDto>, PaginationRes<ProjectProfitabilityVlaRes>>();

            CreateMap<ContractDto, ContractRes>()
                .ForMember(d => d.ContractNo, o => o.MapFrom(s => s.Contractno))
                .ForMember(d => d.Category, o => o.MapFrom(s => s.Category));
            CreateMap<YearMasterRes, YearMasterDto>().ReverseMap();
            CreateMap<DivisionReq, DivisionDto>().ReverseMap();
            CreateMap<DivisionRes, DivisionDto>().ReverseMap();
            CreateMap<GradeDto, GradeRes>().ReverseMap();
            CreateMap<GradeReq, GradeDto>().ReverseMap();
            CreateMap<DivisionGradeReq, DivisionGradeDto>().ReverseMap();
            CreateMap<DivisionGradeRes, DivisionGradeDto>().ReverseMap();
            CreateMap<AgencyRes, AgencyDto>().ReverseMap();

            // ProgrammeNewProject mappings
            CreateMap<AccountCodeDto, AccountCodeRes>().ReverseMap();
            CreateMap<SubAccountDto, SubAccountRes>()
                .ForMember(d => d.SubAccount, o => o.MapFrom(s => s.SubAccountName)).ReverseMap()
                .ForMember(d => d.SubAccountName, o => o.MapFrom(s => s.SubAccount));
            CreateMap<ProjectGroupDto, ProjectGroupRes>().ReverseMap();
            CreateMap<TimeCostCalcsViewDto, TimeCostCalcsViewRes>().ReverseMap();
            CreateMap<TimeCostCalcsTotalsDto, TimeCostCalcsTotalsRes>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostReq>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostRes>().ReverseMap();
            CreateMap<AccountCategoryDto, AccountCategoryReq>().ReverseMap();
            CreateMap<AccountCategoryDto, AccountCategoryRes>().ReverseMap();
            CreateMap<MonthlyOutputDto, MonthlyOutputRes>().ReverseMap();
            CreateMap<CostCentreWorkgroup, CostCentreWorkgroupRes>().ReverseMap();
            CreateMap<WorkGroupPersonDto, WorkGroupPersonRes>().ReverseMap();

            // ResourceSetUp
            CreateMap<ProfitCentreDto, ProfitCentreRes>().ReverseMap();
            CreateMap<ProfitCentreReq, ProfitCentreDto>().ReverseMap();
            CreateMap<ProfitCentreCostDto, ProfitCentreCostRes>().ReverseMap();
            CreateMap<ProfitCentreGradeDto, ProfitCentreGradeRes>().ReverseMap();
            CreateMap<ProfitCentreGradeReq, ProfitCentreGradeDto>().ReverseMap();
            CreateMap<WorkgroupGradeDto, WorkgroupGradeRes>().ReverseMap();
            CreateMap<WorkGroupEmployeeDto, WorkGroupEmployeeReq>().ReverseMap();
            CreateMap<WorkGroupEmployeeDto, WorkGroupEmployeeRes>().ReverseMap();
            CreateMap<ProjectProfitabilityDto, ProjectProfitabilityRes>().ReverseMap();

            CreateMap<ProjectStaffPlanViewDto, ProjectStaffPlanViewRes>().ReverseMap();
            CreateMap<PaginatedResult<ProjectStaffPlanViewDto>, PaginationRes<ProjectStaffPlanViewRes>>();

            CreateMap<ProjectGroupStaffPlanViewDto, ProjectGroupStaffPlanViewRes>().ReverseMap();
            CreateMap<PaginatedResult<ProjectGroupStaffPlanViewDto>, PaginationRes<ProjectGroupStaffPlanViewRes>>();

            CreateMap<PactStaffDto, PactStaffRes>().ReverseMap();
            CreateMap<WorkgroupGradeDto, WorkgroupGradeReq>().ReverseMap();
             

            // UserPermission
            CreateMap<UserDto, UserRes>().ReverseMap();
            CreateMap<UserReq, UserDto>().ReverseMap();
            CreateMap<UserPermissionDto, UserPermissionRes>().ReverseMap();
            CreateMap<UserPermissionReq, UserPermissionDto>().ReverseMap();
            CreateMap<PermissionOptionsDto, PermissionOptionsRes>().ReverseMap();

            // BudgetResourceLevel
            CreateMap<BidDto, BidReq>().ReverseMap();
            CreateMap<BidDto, BidRes>().ReverseMap();
            CreateMap<BidViewDto, BidViewRes>().ReverseMap();
            CreateMap<PurchaseDto, PurchaseReq>().ReverseMap();
            CreateMap<PurchaseDto, PurchaseRes>().ReverseMap();

            // TRANSFORMENGINE: TestListVla mappings — frmTestList / fsubTest_MainList Phase 5
            //   TestListVlaReq and TestListVlaRes both map bidirectionally to TestListVlaDto.
            //   PaginatedResult<TestListVlaDto> -> PaginationRes<TestListVlaRes> for paged list endpoint.
            CreateMap<TestListVlaReq, TestListVlaDto>().ReverseMap();
            CreateMap<TestListVlaRes, TestListVlaDto>().ReverseMap();
            CreateMap<PaginatedResult<TestListVlaDto>, PaginationRes<TestListVlaRes>>();

            // TRANSFORMENGINE: TestRCCost mappings — fsubTestRCPrice Phase 5
            //   TestRCCostReq and TestRCCostRes both map bidirectionally to TestRCCostDto.
            CreateMap<TestRCCostReq, TestRCCostDto>().ReverseMap();
            CreateMap<TestRCCostRes, TestRCCostDto>().ReverseMap();

            // TRANSFORMENGINE: TestRequirementRCCost mappings — fsubTestequirementRCPrice Phase 5
            //   TestRequirementRCCostReq and TestRequirementRCCostRes both map bidirectionally to TestRequirementRCCostDto.
            CreateMap<TestRequirementRCCostReq, TestRequirementRCCostDto>().ReverseMap();
            CreateMap<TestRequirementRCCostRes, TestRequirementRCCostDto>().ReverseMap();

        }
    }
}

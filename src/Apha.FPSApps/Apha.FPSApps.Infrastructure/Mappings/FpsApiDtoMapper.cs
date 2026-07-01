/*
 * TRANSFORMENGINE MIGRATION — FpsApiDtoMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 10 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - Phase 10 (Step 15a): Added CreateMap entries for TestListVla, TestRCCost, TestRequirementRCCost
 *     frontend DTOs <-> backend Req/Res contracts (all convention-mapped; 1:1 property names):
 *       TestListVlaDto <-> TestListVlaRes  (ReverseMap)
 *       TestListVlaDto <-> TestListVlaReq  (ReverseMap)
 *       TestRCCostDto <-> TestRCCostRes    (ReverseMap)
 *       TestRCCostDto <-> TestRCCostReq    (ReverseMap)
 *       TestRequirementRCCostDto <-> TestRequirementRCCostRes  (ReverseMap)
 *       TestRequirementRCCostDto <-> TestRequirementRCCostReq  (ReverseMap)
 *
 * PRESERVED:
 *   - All existing CreateMap entries for prior forms and shared pagination contracts
 *   - All ForMember projections (ProjectDto/ProjectReq, ProjectProfitabilityVlaDto/Res, SubAccountDto/Res)
 *   - All lookup CreateMap entries (StatusDto, DiseaseDto, CustomerDto, etc.)
 *   - Namespace, class name, and Profile base class
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If TestListVlaRes gains additional computed/display fields in a future
 *     backend phase, add ForMember projections here and update frontend TestListVlaDto accordingly.
 *   - TRANSFORMENGINE TODO: All property names on the three new Dto types are 1:1 mirrors of their
 *     Res/Req contracts — convention mapping assumed; verify at runtime via AutoMapper validation.
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;

using Apha.FPSApps.Application.Pagination;
using AutoMapper;
namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class FpsApiDtoMapper : Profile
    {
        public FpsApiDtoMapper()
        {
            CreateMap(typeof(ApiResponseDto<>), typeof(ApiResponse<>)).ReverseMap();
            CreateMap<ApiErrorDto, ApiError>().ReverseMap();
            CreateMap<ApiMetaDto, ApiMeta>().ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();
            CreateMap<PaginationDto, Pagination>().ReverseMap();

            CreateMap<StaffJobViewDto, StaffJobViewRes>().ReverseMap();
            CreateMap<StaffJobZtViewDto, StaffJobZtViewRes>().ReverseMap();
            CreateMap<StaffWorkgroupLookupDto, StaffWorkgroupLookupRes>().ReverseMap();
            CreateMap<StaffJobDto, StaffJobReq>().ReverseMap();
            CreateMap<StaffJobDto, StaffJobRes>().ReverseMap();
            CreateMap<ProgramDto, ProgramReq>().ReverseMap();
            CreateMap<ProgramDto, ProgramRes>().ReverseMap();
            CreateMap<ManagerDto, ManagerRes>().ReverseMap();
            CreateMap<EmployeeDto, EmployeeReq>().ReverseMap();
            CreateMap<EmployeeDto, EmployeeRes>().ReverseMap();

            // FPS Project
            // CustIncome in the FPS API wire format lives in ProjectReq.BudgetExt (see FPS RequestMapper)
            CreateMap<ProjectDto, ProjectReq>()
                .ForMember(d => d.BudgetExt, o => o.MapFrom(s => s.CustIncome))
                .ReverseMap()
                .ForMember(d => d.CustIncome, o => o.MapFrom(s => s.BudgetExt));
            CreateMap<ProjectDto, ProjectRes>().ReverseMap();

            // FPS Lookups
            CreateMap<StatusDto, StatusRes>().ReverseMap();
            CreateMap<DiseaseDto, DiseaseRes>().ReverseMap();
            CreateMap<CustomerDto, CustomerRes>().ReverseMap();
            CreateMap<ContractDto, ContractRes>().ReverseMap();
            CreateMap<ProjectGroupDto, ProjectGroupRes>().ReverseMap();



            // FPS Animal Plan
            CreateMap<AnimalCostViewDto, AnimalCostViewRes>().ReverseMap();
            CreateMap<AnimalDto, AnimalRes>().ReverseMap();
            CreateMap<AnimalRequestDto, AnimalRequestReq>().ReverseMap();
            CreateMap<AnimalRequestDto, AnimalRequestRes>().ReverseMap();

            // FPS Animal Master
            CreateMap<AnimalDto, AnimalReq>().ReverseMap();

            // YEar Master
            CreateMap<YearMasterDto, YearMasterRes>().ReverseMap();
            CreateMap<YearMasterDto, YearMasterReq>().ReverseMap();

            // Testor Product
            CreateMap<TestorProductDto, Apha.Common.Contracts.FPS.TestorProductRes>().ReverseMap();

            // View Project Plan vs Actual Staff
            CreateMap<TimeCostCalcsViewDto, TimeCostCalcsViewRes>().ReverseMap();
            CreateMap<TimeCostCalcsTotalsDto, TimeCostCalcsTotalsRes>().ReverseMap();

            // Division
            CreateMap<DivisionDto, DivisionRes>().ReverseMap();
            CreateMap<DivisionDto, DivisionReq>().ReverseMap();

            // Division Grade
            CreateMap<DivisionGradeDto, DivisionGradeRes>().ReverseMap();
            CreateMap<DivisionGradeDto, DivisionGradeReq>().ReverseMap();

            // TRANSFORMENGINE: Grade mappings added — Phase 10 (Step 15a)
            // Grade CRUD: maps frontend GradeDto to/from backend GradeReq (POST/PUT) and GradeRes (GET/POST/PUT responses)
            CreateMap<GradeDto, GradeReq>().ReverseMap();
            CreateMap<GradeDto, GradeRes>().ReverseMap();


            // Agency
            CreateMap<AgencyDto, AgencyRes>().ReverseMap();

            // Additional Cost
            CreateMap<AdditionalCostDto, AdditionalCostReq>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostRes>().ReverseMap();
            CreateMap<AccountCategoryDto, AccountCategoryRes>().ReverseMap();
            CreateMap<AccountCategoryDto, AccountCategoryReq>().ReverseMap();

            // View Project Plan vs Actual Tests
            CreateMap<MonthlyOutputDto, MonthlyOutputRes>().ReverseMap();

            // ProgrammeNewProject (merged into ProjectDto - mappings above)
            CreateMap<AccountCodeDto, AccountCodeRes>().ReverseMap();
            CreateMap<SubAccountDto, SubAccountRes>()
                .ForMember(d => d.SubAccount, o => o.MapFrom(s => s.SubAccount)).ReverseMap();
            CreateMap<CostCentreWorkgroupDto, CostCentreWorkgroupRes>().ReverseMap();
            CreateMap<PactStaffDto, PactStaffRes>().ReverseMap();
            CreateMap<WorkGroupPersonDto, WorkGroupPersonRes>().ReverseMap();

            // Resource Set-Up
            CreateMap<ProfitCentreDto, ProfitCentreRes>().ReverseMap();
            CreateMap<ProfitCentreDto, ProfitCentreReq>().ReverseMap();
            CreateMap<ProfitCentreCostDto, ProfitCentreCostRes>().ReverseMap();
            CreateMap<ProfitCentreGradeDto, ProfitCentreGradeRes>().ReverseMap();
            CreateMap<ProfitCentreGradeDto, ProfitCentreGradeReq>().ReverseMap();
            CreateMap<WorkgroupGradeDto, WorkgroupGradeRes>().ReverseMap();
            CreateMap<WorkGroupEmployeeDto, WorkGroupEmployeeReq>().ReverseMap();
            CreateMap<WorkGroupEmployeeDto, WorkGroupEmployeeRes>().ReverseMap();

            // ProjectProfitability
            CreateMap<ProjectProfitabilityDto, ProjectProfitabilityRes>().ReverseMap();

            // ProjectProfitabilityVla
            // TRANSFORMENGINE: Project<->JobCode ForMember required — VlaRes.Project maps to VlaDto.JobCode;
            //   ForMember(Id) handles int->int? coercion: Id=GetValueOrDefault(0) on reverse.
            //   TotalCount is on Res only; silently ignored in Res->Dto direction (see DEFERRED note above).
            CreateMap<ProjectProfitabilityVlaDto, ProjectProfitabilityVlaRes>()
                .ForMember(d => d.Project, o => o.MapFrom(s => s.JobCode))
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.GetValueOrDefault(0)))
                .ReverseMap()
                .ForMember(d => d.JobCode, o => o.MapFrom(s => s.Project))
                .ForMember(d => d.Id, o => o.MapFrom(s => (int?)s.Id));

            // Staff Plan view
            CreateMap<ProjectStaffPlanViewDto, ProjectStaffPlanViewRes>().ReverseMap();

            // Project Group Staff Plan view
            CreateMap<ProjectGroupStaffPlanViewDto, ProjectGroupStaffPlanViewRes>().ReverseMap();

            CreateMap<PactStaffDto,PactStaffRes>().ReverseMap();

            // WorkgroupGrade
            CreateMap<WorkgroupGradeDto, WorkgroupGradeReq>().ReverseMap();


            // Job Code (ZT lookup) - now served from PACT API
            CreateMap<FpsJobCodeZtDto, Apha.Common.Contracts.PACT.JobCodeZtRes>().ReverseMap();


            // BudgetResourceLevel
            CreateMap<BidDto, BidReq>().ReverseMap();
            CreateMap<BidDto, BidRes>().ReverseMap();
            CreateMap<BidViewDto, BidViewRes>().ReverseMap();
            CreateMap<PurchaseDto, PurchaseReq>().ReverseMap();
            CreateMap<PurchaseDto, PurchaseRes>().ReverseMap();

            // UserPermission
            CreateMap<UserDto, UserRes>().ReverseMap();
            CreateMap<UserDto, UserReq>().ReverseMap();
            CreateMap<UserPermissionDataDto, UserPermissionRes>().ReverseMap();
            CreateMap<UserPermissionDataDto, UserPermissionReq>().ReverseMap();
            CreateMap<PermissionOptionsDto, PermissionOptionsRes>().ReverseMap();

            // TRANSFORMENGINE: Phase 10 (Step 15a) — TestListVla CRUD: maps frontend TestListVlaDto to/from
            //   backend TestListVlaRes (GET /api/v1/testlistvla responses) and
            //   TestListVlaReq (POST/PUT /api/v1/testlistvla request bodies).
            //   All 11 property names are 1:1 between Dto and Res/Req — convention mapping applies.
            CreateMap<TestListVlaDto, TestListVlaRes>().ReverseMap();
            CreateMap<TestListVlaDto, TestListVlaReq>().ReverseMap();

            // TRANSFORMENGINE: Phase 10 (Step 15a) — TestRCCost CRUD: maps frontend TestRCCostDto to/from
            //   backend TestRCCostRes (GET /api/v1/testrccost responses) and
            //   TestRCCostReq (POST/PUT /api/v1/testrccost request bodies).
            //   All 4 property names are 1:1 between Dto and Res/Req — convention mapping applies.
            CreateMap<TestRCCostDto, TestRCCostRes>().ReverseMap();
            CreateMap<TestRCCostDto, TestRCCostReq>().ReverseMap();

            // TRANSFORMENGINE: Phase 10 (Step 15a) — TestRequirementRCCost CRUD: maps frontend
            //   TestRequirementRCCostDto to/from backend TestRequirementRCCostRes
            //   (GET /api/v1/testrequirementrccost responses) and TestRequirementRCCostReq
            //   (POST/PUT /api/v1/testrequirementrccost request bodies).
            //   All 5 property names are 1:1 between Dto and Res/Req — convention mapping applies.
            CreateMap<TestRequirementRCCostDto, TestRequirementRCCostRes>().ReverseMap();
            CreateMap<TestRequirementRCCostDto, TestRequirementRCCostReq>().ReverseMap();


        }
    }
}

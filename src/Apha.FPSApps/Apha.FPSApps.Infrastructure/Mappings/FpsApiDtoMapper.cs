/*
 * TRANSFORMENGINE MIGRATION — FpsApiDtoMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 10 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - Added CostCentre CRUD mapper entries: CostCentreDto <-> CostCentreReq and CostCentreDto <-> CostCentreRes
 *   - Entries placed adjacent to existing CostCentreWorkgroupDto mapping for locality
 *
 * PRESERVED:
 *   - All existing CreateMap entries unchanged
 *   - Existing CostCentreWorkgroupDto <-> CostCentreWorkgroupRes mapping (already present)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated for this phase
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

            // TRANSFORMENGINE: Grade mappings added � Phase 10 (Step 15a)
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

            // TRANSFORMENGINE: CostCentre CRUD mappings added — Phase 10 (Step 15a)
            // CostCentre CRUD: maps frontend CostCentreDto to/from backend CostCentreReq (POST/PUT)
            //   and CostCentreRes (GET/GET-paged/POST/PUT responses)
            CreateMap<CostCentreDto, CostCentreReq>().ReverseMap();
            CreateMap<CostCentreDto, CostCentreRes>().ReverseMap();

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


        }
    }
}

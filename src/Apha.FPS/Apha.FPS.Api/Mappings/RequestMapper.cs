/*
 * TRANSFORMENGINE MIGRATION — RequestMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - Added CostCentreReq <-> CostCentreDto mapping (frmMaintCostCentres POST/PUT request binding)
 *   - Added CostCentreDto <-> CostCentreRes mapping (frmMaintCostCentres GET paged/by-id/POST/PUT response)
 *
 * PRESERVED:
 *   - All existing mappings (StaffJob, Animal, Program, Project, Division, Grade, ProfitCentre, etc.)
 *   - CostCentreWorkgroup <-> CostCentreWorkgroupRes mapping (existing workgroup-lookup endpoint)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: CostCentreReq does not include FpsYear; controller sets it from IFpsRequestContext before passing to service. Verify this aligns with frontend contract expectations.
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
            // TRANSFORMENGINE: CostCentre CRUD mappings — Phase 5 frmMaintCostCentres migration
            //   CostCentreReq → CostCentreDto (POST create, PUT update request binding; FpsYear excluded from Req — set server-side)
            //   CostCentreDto → CostCentreRes (GET paged, GET by id, POST, PUT response)
            CreateMap<CostCentreReq, CostCentreDto>().ReverseMap();
            CreateMap<CostCentreDto, CostCentreRes>().ReverseMap();
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
             

            // BudgetResourceLevel
            CreateMap<BidDto, BidReq>().ReverseMap();
            CreateMap<BidDto, BidRes>().ReverseMap();
            CreateMap<BidViewDto, BidViewRes>().ReverseMap();
            CreateMap<PurchaseDto, PurchaseReq>().ReverseMap();
            CreateMap<PurchaseDto, PurchaseRes>().ReverseMap();

          
        }
    }
}

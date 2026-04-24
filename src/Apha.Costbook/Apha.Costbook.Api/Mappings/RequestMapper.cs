using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.DataAccess;
using AutoMapper;

namespace Apha.Costbook.Api.Mappings
{
    public class RequestMapper : Profile
    {
        public RequestMapper()
        {
            // ── Pagination ────────────────────────────────────────────────────
            CreateMap(typeof(PaginationReq<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();
            CreateMap<Pagination, PaginationDto>().ReverseMap();

            // ── Project entity ↔ Dto/Res/Req ─────────────────────────────────
            CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<Project, ProjectHeaderDto>();          // entity → header DTO (no reverse needed)
            CreateMap<ProjectDto, ProjectRes>().ReverseMap();
            CreateMap<ProjectDto, ProjectReq>().ReverseMap();

            // ── Lookup entities ───────────────────────────────────────────────
            CreateMap<Customer, CustomerDto>().ReverseMap();
            CreateMap<Disease, DiseaseDto>().ReverseMap();
            CreateMap<Program, ProgramDto>().ReverseMap();
            CreateMap<Staff, StaffDto>().ReverseMap();
            CreateMap<CustomerDto, CustomerRes>().ReverseMap();
            CreateMap<DiseaseDto, DiseaseRes>().ReverseMap();
            CreateMap<ProgramDto, ProgramRes>().ReverseMap();
            CreateMap<StaffDto, StaffRes>().ReverseMap();

            // ── Yearly details: entity ↔ Dto (used by YearlyDetailsService) ──
            CreateMap<ProjectYear, ProjectYearDto>().ReverseMap();
            CreateMap<StaffRequirement, StaffRequirementDto>().ReverseMap();
            CreateMap<TestRequirement, TestRequirementDto>().ReverseMap();
            CreateMap<AnimalRequirement, AnimalRequirementDto>().ReverseMap();
            CreateMap<AdditionalCost, AdditionalCostDto>().ReverseMap();

            // ── Yearly details: Dto ↔ Res/Req (used by API controller) ───────
            CreateMap<ProjectHeaderDto, ProjectHeaderRes>().ReverseMap();
            CreateMap<ProjectYearDto, ProjectYearRes>().ReverseMap();
            CreateMap<ProjectYearDto, ProjectYearReq>().ReverseMap();
            CreateMap<StaffRequirementDto, StaffRequirementRes>().ReverseMap();
            CreateMap<StaffRequirementDto, StaffRequirementReq>().ReverseMap();
            CreateMap<TestRequirementDto, TestRequirementRes>().ReverseMap();
            CreateMap<TestRequirementDto, TestRequirementReq>().ReverseMap();
            CreateMap<AnimalRequirementDto, AnimalRequirementRes>().ReverseMap();
            CreateMap<AnimalRequirementDto, AnimalRequirementReq>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostRes>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostReq>().ReverseMap();
            CreateMap<PayRateDto, PayRateRes>().ReverseMap();
            CreateMap<AnimalRateDto, AnimalRateRes>().ReverseMap();
            CreateMap<AccountCategoryDto, AccountCategoryRes>().ReverseMap();
        }
    }
}

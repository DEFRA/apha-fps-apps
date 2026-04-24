using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class CostbookApiDtoMapper : Profile
    {
        public CostbookApiDtoMapper()
        {
            // ── Existing project mappings ─────────────────────────────────────
            CreateMap<ProjectDto, ProjectRes>().ReverseMap();
            CreateMap<ProjectDto, ProjectReq>().ReverseMap();
            CreateMap<CustomerDto, CustomerRes>().ReverseMap();
            CreateMap<DiseaseDto, DiseaseRes>().ReverseMap();
            CreateMap<Application.Dtos.CostBook.ProgramDto, Common.Contracts.Costbook.ProgramRes>().ReverseMap();
            CreateMap<StaffDto, StaffRes>().ReverseMap();
            CreateMap<ContractDto, ContractRes>().ReverseMap();
            CreateMap<ProjectEditDataDto, ProjectEditRes>().ReverseMap();

            // ── Yearly details: Res/Req ↔ Dto (used by CostBookYearlyDetailsApiClient) ──
            CreateMap<ProjectHeaderRes, ProjectHeaderDto>().ReverseMap();
            CreateMap<ProjectYearRes, ProjectYearDto>().ReverseMap();
            CreateMap<ProjectYearDto, ProjectYearReq>().ReverseMap();
            CreateMap<StaffRequirementRes, StaffRequirementDto>().ReverseMap();
            CreateMap<StaffRequirementDto, StaffRequirementReq>().ReverseMap();
            CreateMap<TestRequirementRes, TestRequirementDto>().ReverseMap();
            CreateMap<TestRequirementDto, TestRequirementReq>().ReverseMap();
            CreateMap<AnimalRequirementRes, AnimalRequirementDto>().ReverseMap();
            CreateMap<AnimalRequirementDto, AnimalRequirementReq>().ReverseMap();
            CreateMap<AdditionalCostRes, AdditionalCostDto>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostReq>().ReverseMap();
            CreateMap<PayRateRes, PayRateDto>().ReverseMap();
            CreateMap<AnimalRateRes, AnimalRateDto>().ReverseMap();
            CreateMap<AccountCategoryRes, AccountCategoryDto>().ReverseMap();
        }
    }
}

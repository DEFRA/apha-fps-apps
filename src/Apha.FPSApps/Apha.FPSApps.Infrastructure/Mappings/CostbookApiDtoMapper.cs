using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;

using Apha.FPSApps.Application.Pagination;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class CostbookApiDtoMapper: Profile
    {
        public CostbookApiDtoMapper()
        {
            CreateMap<ProjectDto, ProjectRes>().ReverseMap();
            CreateMap<ProjectDto, ProjectReq>().ReverseMap();
            CreateMap<CustomerDto, CustomerRes>().ReverseMap();
            CreateMap<DiseaseDto, DiseaseRes>().ReverseMap();
            CreateMap<Application.Dtos.CostBook.ProgramDto, Common.Contracts.Costbook.ProgramRes>().ReverseMap();
            CreateMap<StaffDto, StaffRes>().ReverseMap();
            CreateMap<ContractDto, ContractRes>().ReverseMap();
            CreateMap<ProjectEditDataDto, ProjectEditRes>().ReverseMap();
        }
    }
}

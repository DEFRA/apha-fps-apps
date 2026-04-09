using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using AutoMapper;

namespace Apha.PACT.Api.Mappings
{
    public class RequestMapper : Profile
    {
        public RequestMapper()
        {
            CreateMap(typeof(PaginationReq<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<Pagination, PaginationDto>().ReverseMap();

            CreateMap<JobCodeReq, JobCodeDto>().ReverseMap();
            CreateMap<JobCodeRes, JobCodeDto>().ReverseMap();
            CreateMap<TimeCodeValidReq, TimeCodeValidDto>().ReverseMap();
            CreateMap<TimeCodeValidRes, TimeCodeValidDto>().ReverseMap();
            CreateMap<WorkGroupRes, WorkGroupDto>().ReverseMap();
            CreateMap<TestCapabilityReq, TestCapabilityDto>().ReverseMap();
            CreateMap<TestCapabilityRes, TestCapabilityDto>().ReverseMap();
            CreateMap<TestRequirementReq, TestRequirementtDto>().ReverseMap();
            CreateMap<TestRequirementtRes, TestRequirementtDto>().ReverseMap();
            CreateMap<TestorProductRes, TestorProductDto>().ReverseMap();
        }
    }
}

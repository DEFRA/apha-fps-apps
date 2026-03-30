using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Mappings
{
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            CreateMap(typeof(PaginationParameters<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PagedData<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<PaginationData, PaginationDto>().ReverseMap();

            CreateMap<JobCode, JobCodeDto>().ReverseMap();
            CreateMap<TimeCodeValid, TimeCodeValidDto>().ReverseMap();
            CreateMap<WorkGroup, WorkGroupDto>().ReverseMap();
        }
    }
}

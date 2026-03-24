using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Mappings
{
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            CreateMap(typeof(PaginationParameters<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PagedData<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<PaginationData, PaginationDto>().ReverseMap();
        }
    }
}

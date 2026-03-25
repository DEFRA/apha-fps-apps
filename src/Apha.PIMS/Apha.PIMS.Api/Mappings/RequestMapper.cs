using Apha.Common.Contracts;
using Apha.PIMS.Application.Pagination;
using AutoMapper;

namespace Apha.PIMS.Api.Mappings
{
    public class RequestMapper : Profile
    {
        public RequestMapper()
        {
            CreateMap(typeof(PaginationReq<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<Pagination, PaginationDto>().ReverseMap();
        }
    }
}

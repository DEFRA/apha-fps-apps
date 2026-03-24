using Apha.Common.Contracts;
using Apha.Costbook.Application.Pagination;
using AutoMapper;

namespace Apha.Costbook.Api.Mappings
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

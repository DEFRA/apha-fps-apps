using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
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

            CreateMap<WeatherForecastRes, WeatherForecastDto>().ReverseMap();           
            CreateMap<WeatherForecastCriteriaReq, WeatherForecastCriteriaDto>().ReverseMap();
            CreateMap<Pagination, PaginationDto>().ReverseMap();
        }
    }
}

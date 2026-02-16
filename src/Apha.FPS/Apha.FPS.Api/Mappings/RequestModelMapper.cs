using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using AutoMapper;

namespace Apha.FPS.Api.Mappings
{
    public class RequestModelMapper : Profile
    {
        public RequestModelMapper()
        {
            CreateMap(typeof(PaginationReq<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<WeatherForecastRes, WeatherForecastDto>().ReverseMap();           
            CreateMap<WeatherForecastCriteriaReq, WeatherForecastCriteriaDto>().ReverseMap();
            CreateMap<Pagination, PaginationDto>().ReverseMap();
        }
    }
}

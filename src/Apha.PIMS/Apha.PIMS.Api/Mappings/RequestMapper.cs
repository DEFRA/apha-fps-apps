using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.PIMS.Application.Dtos;
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

            CreateMap<WeatherForecastRes, WeatherForecastDto>().ReverseMap();           
            CreateMap<WeatherForecastCriteriaReq, WeatherForecastCriteriaDto>().ReverseMap();
            CreateMap<Pagination, PaginationDto>().ReverseMap();
        }
    }
}

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Mappings
{
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            CreateMap(typeof(PaginationParameters<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PagedData<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<WeatherForecast, WeatherForecastDto>().ReverseMap();
            CreateMap<WeatherForecastCriteria, WeatherForecastCriteriaDto>().ReverseMap();
            CreateMap<PaginationData, PaginationDto>().ReverseMap();
        }
    }
}

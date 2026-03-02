using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;
using AutoMapper;

namespace Apha.Costbook.Application.Mappings
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

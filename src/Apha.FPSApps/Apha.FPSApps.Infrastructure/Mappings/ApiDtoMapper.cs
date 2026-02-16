using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.DTOs;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class ApiDtoMapper : Profile
    {
        public ApiDtoMapper() 
        {            
            CreateMap(typeof(ApiResponseDto<>), typeof(ApiResponse<>)).ReverseMap();            
            CreateMap<ApiErrorDto, ApiError>().ReverseMap();
            CreateMap<ApiMetaDto, ApiMeta>().ReverseMap();
            CreateMap<WeatherForecastDto, WeatherForecastRes>().ReverseMap();            
        }
    }
}

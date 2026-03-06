using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.DTOs;
using Apha.FPSApps.Application.Pagination;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class ApiDtoMapper : Profile
    {
        public ApiDtoMapper() 
        {            
            CreateMap(typeof(ApiResponseDto<>), typeof(ApiResponse<>)).ReverseMap();            
            CreateMap(typeof(PaginatedApiResponseDto<>), typeof(PaginatedApiResponse<>)).ReverseMap();            
            CreateMap<ApiErrorDto, ApiError>().ReverseMap();
            CreateMap<ApiMetaDto, ApiMeta>().ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();
            CreateMap<PaginationDto, Pagination>().ReverseMap();
            CreateMap<WeatherForecastDto, WeatherForecastRes>().ReverseMap();      
            CreateMap<StaffJobViewDto, StaffJobViewRes>().ReverseMap();
        }
    }
}

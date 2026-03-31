using Apha.Common.Contracts;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Pagination;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Infrastructure.Mappings.PIMS
{
    public class ApiDtoMapper : Profile
    {
        public ApiDtoMapper()
        {
            CreateMap(typeof(ApiResponseDto<>), typeof(ApiResponse<>)).ReverseMap();
            CreateMap<ApiErrorDto, ApiError>().ReverseMap();
            CreateMap<ApiMetaDto, ApiMeta>().ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();
            CreateMap<PaginationDto, Pagination>().ReverseMap();
        }
    }   
}

using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.DTOs;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using DocumentFormat.OpenXml.Office2016.Excel;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsStaffJobApiClient : IFpsStaffJobApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsStaffJobApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<PaginatedApiResponseDto<List<StaffJobViewDto>>> GetAllStaffJobAsync(QueryParameters<string> staffJobReq)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString($"api/staffjob", staffJobReq);
                var response = await _http.GetPaginatedAsync<List<StaffJobViewRes>>(url);

                if (response.Success)
                {
                    return _mapper.Map<PaginatedApiResponseDto<List<StaffJobViewDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<PaginatedApiResponseDto<List<StaffJobViewDto>>>(response);
                    return PaginatedApiResponseDto<List<StaffJobViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception ex)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to retrieve user",
                        Code = "INTERNAL_ERROR",
                        Details = null
                    }
                };
                return PaginatedApiResponseDto<List<StaffJobViewDto>>.FailureResponse(apiErrosDto,
                   new ApiMetaDto());
            }
        }
    }
}

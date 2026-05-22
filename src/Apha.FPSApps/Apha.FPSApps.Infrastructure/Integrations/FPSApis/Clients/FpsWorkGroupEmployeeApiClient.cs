using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsWorkGroupEmployeeApiClient : IFpsWorkGroupEmployeeApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsWorkGroupEmployeeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<WorkGroupEmployeeDto>>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade)
        {
            var baseUrl = string.Format(FpsApiEndpoints.GetWgStaff, Uri.EscapeDataString(wgGrade));
            var url = QueryStringHelper.AddQueryString(baseUrl, query);
            var response = await _http.GetAsync<List<WorkGroupEmployeeRes>>(url);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeDto>>>(response);
                return ApiResponseDto<List<WorkGroupEmployeeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeDto>> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            var url = string.Format(FpsApiEndpoints.GetWgEmployeeById, Uri.EscapeDataString(pactId));
            var response = await _http.GetAsync<WorkGroupEmployeeRes>(url);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(response);
                return ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeDto>> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto)
        {
            var req = _mapper.Map<WorkGroupEmployeeReq>(dto);
            var response = await _http.PutAsync<WorkGroupEmployeeReq, WorkGroupEmployeeRes>(FpsApiEndpoints.UpdateWgEmployee, req);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(response);
                return ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            var url = string.Format(FpsApiEndpoints.DeleteWgEmployee, Uri.EscapeDataString(pactId));
            var response = await _http.DeleteAsync<bool>(url);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<bool>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }
    }
}

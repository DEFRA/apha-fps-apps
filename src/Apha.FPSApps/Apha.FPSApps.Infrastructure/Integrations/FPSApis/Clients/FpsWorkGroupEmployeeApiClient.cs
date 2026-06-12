// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — FpsWorkGroupEmployeeApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Added CreateWorkGroupEmployeeAsync method: HTTP POST to api/v1/wgstaff
 *     (matches backend WorkGroupEmployeeController [HttpPost] action added in Phase 5)
 *   - Wraps POST call in try/catch(Exception) returning FailureResponse with InternalCodeError const
 *   - Added private const string InternalCodeError = "INTERNAL_ERROR" for catch-block FailureResponse
 *
 * PRESERVED:
 *   - GetWorkGroupEmployeeAsync — GET paginated list filtered by wgGrade via FpsApiEndpoints.GetWgStaff
 *   - GetWorkGroupEmployeeByIdAsync — GET single record via FpsApiEndpoints.GetWgEmployeeById
 *   - UpdateWorkGroupEmployeeAsync — PUT via FpsApiEndpoints.UpdateWgEmployee
 *   - DeleteWorkGroupEmployeeAsync — DELETE via FpsApiEndpoints.DeleteWgEmployee
 *   - private readonly IFpsHttpExecutor _http and IMapper _mapper fields unchanged
 *   - Namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Existing methods (Get, GetById, Update, Delete) do not wrap HTTP
 *     calls in try/catch — these should be hardened in a follow-up pass for consistency with
 *     the InternalCodeError pattern applied to CreateWorkGroupEmployeeAsync.
 *   - TRANSFORMENGINE TODO: wgGrade parameter in GetWorkGroupEmployeeAsync must be sourced from
 *     parent page context, URL route, or session state — confirm MaintWGStaff page supplies it.
 */

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
        // TRANSFORMENGINE: Sonar S1192 — InternalCodeError const for catch-block FailureResponse
        private const string InternalCodeError = "INTERNAL_ERROR";

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

        // TRANSFORMENGINE: CreateWorkGroupEmployeeAsync added Phase 9 — POST /api/v1/wgstaff
        // Maps to backend WorkGroupEmployeeController [HttpPost] CreateWorkGroupEmployeeAsync action.
        // FpsApiEndpoints.UpdateWgEmployee ("api/v1/wgstaff") is the correct base URL for POST as well as PUT.
        public async Task<ApiResponseDto<WorkGroupEmployeeDto>> CreateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto)
        {
            try
            {
                var req = _mapper.Map<WorkGroupEmployeeReq>(dto);
                var response = await _http.PostAsync<WorkGroupEmployeeReq, WorkGroupEmployeeRes>(FpsApiEndpoints.UpdateWgEmployee, req);
                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(response);
                }

                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(response);
                return ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create WorkGroupEmployee", Code = InternalCodeError } },
                    new ApiMetaDto());
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

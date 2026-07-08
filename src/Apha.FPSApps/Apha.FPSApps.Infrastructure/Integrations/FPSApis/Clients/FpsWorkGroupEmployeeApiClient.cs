/*
 * TRANSFORMENGINE MIGRATION — FpsWorkGroupEmployeeApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 3 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Added TRANSFORMENGINE migration header (PB-14 annotation policy)
 *   - Added private const InternalCodeError = "INTERNAL_ERROR" (Sonar S1192)
 *   - Wrapped all 8 HTTP calls in try/catch(Exception) with FailureResponse fallback
 *   - Preserved all URL composition via FpsApiEndpoints constants and Uri.EscapeDataString
 *
 * PRESERVED:
 *   - All 8 interface methods: GetWorkGroupEmployeeAsync, GetWorkGroupEmployeeForStaffAsync,
 *     GetWorkGroupEmployeeByIdAsync, GetWorkGroupEmployeeByIdForStaffAsync,
 *     CreateWorkGroupEmployeeForStaffAsync, UpdateWorkGroupEmployeeAsync,
 *     UpdateWorkGroupEmployeeForStaffAsync, DeleteWorkGroupEmployeeAsync
 *   - private readonly _http and _mapper fields (Sonar S2933)
 *   - Mapper used for success response mapping; FailureResponse used for error path
 *   - FpsApiEndpoints constants for all URL paths
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify backend wgstaff route paths match FpsApiEndpoints values exactly
 */

using Apha.Common.Constants;
using Apha.Common.Contracts;
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

        // TRANSFORMENGINE: InternalCodeError as private const — Sonar S1192 compliance
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsWorkGroupEmployeeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET api/v1/wgstaff?wgGrade={0} → WgStaffController.GetWorkGroupEmployeeAsync
        public async Task<ApiResponseDto<List<WorkGroupEmployeeDto>>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade)
        {
            try
            {
                var baseUrl = string.Format(FpsApiEndpoints.GetWgStaff, Uri.EscapeDataString(wgGrade));
                var url = QueryStringHelper.AddQueryString(baseUrl, query);
                var response = await _http.GetAsync<List<WorkGroupEmployeeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeDto>>>(response);
                return ApiResponseDto<List<WorkGroupEmployeeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<WorkGroupEmployeeDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkGroupEmployee data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/wgstaff/staff?wgGrade={0} → WgStaffController.GetWorkGroupEmployeeForStaffAsync
        public async Task<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>> GetWorkGroupEmployeeForStaffAsync(QueryParameters<string> query, string wgGrade)
        {
            try
            {
                var baseUrl = string.Format(FpsApiEndpoints.GetWgStaffForStaff, Uri.EscapeDataString(wgGrade));
                var url = QueryStringHelper.AddQueryString(baseUrl, query);
                var response = await _http.GetAsync<List<WorkGroupEmployeeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>>(response);
                return ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkGroupEmployeeForStaff data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/wgstaff/activestaff?wgGrade={0} → WgStaffController.GetAllActiveWorkGroupEmployeesAsync
        public async Task<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>> GetAllActiveWorkGroupEmployeesAsync(QueryParameters<string> query, string wgGrade)
        {
            try
            {
                var baseUrl = string.Format(FpsApiEndpoints.GetActiveWgStaffForStaff, Uri.EscapeDataString(wgGrade));
                var url = QueryStringHelper.AddQueryString(baseUrl, query);
                var response = await _http.GetAsync<List<WorkGroupEmployeeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>>(response);
                return ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve all active WorkGroupEmployee data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/wgstaff/{pactId} → WgStaffController.GetWorkGroupEmployeeByIdAsync
        public async Task<ApiResponseDto<WorkGroupEmployeeDto>> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            try
            {
                var response = await _http.GetAsync<WorkGroupEmployeeRes>(
                    string.Format(FpsApiEndpoints.GetWgEmployeeById, Uri.EscapeDataString(pactId)));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(response);
                return ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkGroupEmployee by ID", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/wgstaff/{pactId} → WgStaffController.GetWorkGroupEmployeeByIdAsync (staff variant)
        public async Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> GetWorkGroupEmployeeByIdForStaffAsync(string pactId)
        {
            try
            {
                var response = await _http.GetAsync<WorkGroupEmployeeRes>(
                    string.Format(FpsApiEndpoints.GetWgEmployeeById, Uri.EscapeDataString(pactId)));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(response);
                return ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkGroupEmployeeForStaff by ID", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST api/v1/wgstaff/staff → WgStaffController.CreateWorkGroupEmployeeForStaffAsync
        public async Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> CreateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeStaffDto dto)
        {
            try
            {
                var req = _mapper.Map<WorkGroupEmployeeReq>(dto);
                var response = await _http.PostAsync<WorkGroupEmployeeReq, WorkGroupEmployeeRes>(FpsApiEndpoints.CreateWgEmployeeForStaff, req);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(response);
                return ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create WorkGroupEmployeeForStaff", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT api/v1/wgstaff → WgStaffController.UpdateWorkGroupEmployeeAsync
        public async Task<ApiResponseDto<WorkGroupEmployeeDto>> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto)
        {
            try
            {
                var req = _mapper.Map<WorkGroupEmployeeReq>(dto);
                var response = await _http.PutAsync<WorkGroupEmployeeReq, WorkGroupEmployeeRes>(FpsApiEndpoints.UpdateWgEmployee, req);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(response);
                return ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update WorkGroupEmployee", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT api/v1/wgstaff/staff → WgStaffController.UpdateWorkGroupEmployeeForStaffAsync
        public async Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> UpdateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeStaffDto dto)
        {
            try
            {
                var req = _mapper.Map<WorkGroupEmployeeReq>(dto);
                var response = await _http.PutAsync<WorkGroupEmployeeReq, WorkGroupEmployeeRes>(FpsApiEndpoints.UpdateWgEmployeeForStaff, req);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(response);
                return ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update WorkGroupEmployeeForStaff", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE api/v1/wgstaff/{pactId} → WgStaffController.DeleteWorkGroupEmployeeAsync
        public async Task<ApiResponseDto<bool>> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            try
            {
                var response = await _http.DeleteAsync<bool>(
                    string.Format(FpsApiEndpoints.DeleteWgEmployee, Uri.EscapeDataString(pactId)));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete WorkGroupEmployee", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}

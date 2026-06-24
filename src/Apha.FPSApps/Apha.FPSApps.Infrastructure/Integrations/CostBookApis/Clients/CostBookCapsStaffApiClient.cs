/*
 * TRANSFORMENGINE MIGRATION — CostBookCapsStaffApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend HTTP API client created for frmMaintainance Tab 5 (CAPS Staff)
 *   - Implements ICostBookCapsStaffApiClient via ICostBookHttpExecutor
 *   - GetAllCapsStaffAsync()         → GET    api/v1/capsstaff
 *   - GetPaginatedCapsStaffAsync()   → GET    api/v1/capsstaff/paginated (QueryParameters<string>)
 *   - GetCapsStaffByMNumberAsync()   → GET    api/v1/capsstaff/{mNumber}
 *   - AddCapsStaffAsync()            → POST   api/v1/capsstaff
 *   - UpdateCapsStaffAsync()         → PUT    api/v1/capsstaff/{mNumber}
 *   - DeleteCapsStaffAsync()         → DELETE api/v1/capsstaff/{mNumber}
 *   - All HTTP calls wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - AutoMapper used to map CapsStaffReq/Res backend contracts to/from CapsStaffDto
 *
 * PRESERVED:
 *   - Backend CapsStaffController route template api/v1/capsstaff preserved exactly
 *   - MNumber route parameter URL-encoded via HttpUtility.UrlEncode
 *   - Paginated endpoint mirrors backend GetPaginatedCapsStaff action
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether Dt2Number field is surfaced in the Tab 5 modal — not in HTML prototype
 */

using Apha.Common.Contracts.Costbook;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System.Web;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookCapsStaffApiClient : ICostBookCapsStaffApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        // TRANSFORMENGINE: Backend route api/v{version:apiVersion}/capsstaff — exact match required
        private const string BaseUrl = "api/v1/capsstaff";

        public CostBookCapsStaffApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET api/v1/capsstaff → full list for Tab 5 staff grid
        public async Task<ApiResponseDto<List<CapsStaffDto>>> GetAllCapsStaffAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<CapsStaffRes>>(BaseUrl);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<CapsStaffDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<CapsStaffDto>>>(response);
                return ApiResponseDto<List<CapsStaffDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<CapsStaffDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve CAPS staff", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/capsstaff/paginated → paginated list for Tab 5 staff grid
        public async Task<ApiResponseDto<List<CapsStaffDto>>> GetPaginatedCapsStaffAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString($"{BaseUrl}/paginated", query);
                var response = await _http.GetAsync<List<CapsStaffRes>>(url);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<CapsStaffDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<CapsStaffDto>>>(response);
                return ApiResponseDto<List<CapsStaffDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<CapsStaffDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paginated CAPS staff", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/capsstaff/{mNumber} → single record lookup for edit/delete modal
        public async Task<ApiResponseDto<CapsStaffDto>> GetCapsStaffByMNumberAsync(string mNumber)
        {
            try
            {
                var url = $"{BaseUrl}/{HttpUtility.UrlEncode(mNumber)}";
                var response = await _http.GetAsync<CapsStaffRes>(url);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<CapsStaffDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<CapsStaffDto>>(response);
                return ApiResponseDto<CapsStaffDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<CapsStaffDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve CAPS staff member", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST api/v1/capsstaff → create from Tab 5 add modal (formTblCapsStaff); Admin role required on backend
        public async Task<ApiResponseDto<CapsStaffDto>> AddCapsStaffAsync(CapsStaffDto dto)
        {
            try
            {
                var request = _mapper.Map<CapsStaffReq>(dto);
                var response = await _http.PostAsync<CapsStaffReq, CapsStaffRes>(BaseUrl, request);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<CapsStaffDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<CapsStaffDto>>(response);
                return ApiResponseDto<CapsStaffDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<CapsStaffDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to add CAPS staff member", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT api/v1/capsstaff/{mNumber} → update from Tab 5 edit modal; Admin role required on backend
        public async Task<ApiResponseDto<CapsStaffDto>> UpdateCapsStaffAsync(string mNumber, CapsStaffDto dto)
        {
            try
            {
                var request = _mapper.Map<CapsStaffReq>(dto);
                var url = $"{BaseUrl}/{HttpUtility.UrlEncode(mNumber)}";
                var response = await _http.PutAsync<CapsStaffReq, CapsStaffRes>(url, request);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<CapsStaffDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<CapsStaffDto>>(response);
                return ApiResponseDto<CapsStaffDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<CapsStaffDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update CAPS staff member", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE api/v1/capsstaff/{mNumber} → delete from Tab 5 confirm modal; Admin role required on backend
        public async Task<ApiResponseDto<bool>> DeleteCapsStaffAsync(string mNumber)
        {
            try
            {
                var url = $"{BaseUrl}/{HttpUtility.UrlEncode(mNumber)}";
                var response = await _http.DeleteAsync<bool?>(url);

                if (response.Success && response.Data.HasValue)
                    return ApiResponseDto<bool>.SuccessResponse(response.Data.Value);

                if (response.Success && !response.Data.HasValue)
                    return ApiResponseDto<bool>.SuccessResponse(true);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete CAPS staff member", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

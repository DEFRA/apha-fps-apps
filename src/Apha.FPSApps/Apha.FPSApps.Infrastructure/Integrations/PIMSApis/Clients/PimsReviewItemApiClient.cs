/*
 * TRANSFORMENGINE MIGRATION — PimsReviewItemApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New HTTP API client implementing IPimsReviewItemApiClient
 *   - Binds to backend ReviewItemController routes:
 *       GET    /api/v1/reviewitem               — full list
 *       GET    /api/v1/reviewitem/{itemid}      — get by integer PK
 *       POST   /api/v1/reviewitem               — create
 *       PUT    /api/v1/reviewitem/{itemid}      — update; route PK is authoritative
 *       DELETE /api/v1/reviewitem/{itemid}      — delete
 *   - Integer PK (itemid) — matches backend controller route constraint {itemid:int}
 *   - Other Tab lookup CRUD from frmMaintainance
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Req/Res contracts: ReviewItemReq, ReviewItemRes from Apha.Common.Contracts.PIMS
 *
 * PRESERVED:
 *   - All CRUD semantics matching IPimsReviewItemApiClient interface (GetAll, GetById, Create, Update, Delete)
 *   - Integer PK (itemid) semantics
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm integer PK generation strategy — verify DB identity/sequence vs application-assigned
 */

using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsReviewItemApiClient : IPimsReviewItemApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: S1192 — base URL extracted to const; matches backend ReviewItemController [Route("api/v{version:apiVersion}/reviewitem")]
        private const string BaseUrl = "api/v1/reviewitem";

        public PimsReviewItemApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /api/v1/reviewitem — full list
        public async Task<ApiResponseDto<List<ReviewItemDto>>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ReviewItemRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ReviewItemDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ReviewItemDto>>>(response);
                return ApiResponseDto<List<ReviewItemDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ReviewItemDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ReviewItem data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/reviewitem/{itemid:int}
        public async Task<ApiResponseDto<ReviewItemDto>> GetByIdAsync(int itemid)
        {
            try
            {
                var url = $"{BaseUrl}/{itemid}";
                var response = await _http.GetAsync<ReviewItemRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);
                return ApiResponseDto<ReviewItemDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReviewItemDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ReviewItem by ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST /api/v1/reviewitem
        public async Task<ApiResponseDto<ReviewItemDto>> CreateAsync(ReviewItemDto dto)
        {
            try
            {
                var request = _mapper.Map<ReviewItemReq>(dto);
                var response = await _http.PostAsync<ReviewItemReq, ReviewItemRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);
                return ApiResponseDto<ReviewItemDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReviewItemDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create ReviewItem", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT /api/v1/reviewitem/{itemid:int} — route PK is authoritative
        public async Task<ApiResponseDto<ReviewItemDto>> UpdateAsync(int itemid, ReviewItemDto dto)
        {
            try
            {
                var request = _mapper.Map<ReviewItemReq>(dto);
                var url = $"{BaseUrl}/{itemid}";
                var response = await _http.PutAsync<ReviewItemReq, ReviewItemRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);
                return ApiResponseDto<ReviewItemDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReviewItemDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update ReviewItem", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE /api/v1/reviewitem/{itemid:int}
        public async Task<ApiResponseDto<bool>> DeleteAsync(int itemid)
        {
            try
            {
                var url = $"{BaseUrl}/{itemid}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete ReviewItem", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

/*
 * TRANSFORMENGINE MIGRATION — PimsAccessLevelApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New HTTP API client implementing IPimsAccessLevelApiClient
 *   - Binds to backend AccessLevelController routes:
 *       GET    /api/v1/accesslevel                              — full list
 *       GET    /api/v1/accesslevel/{systemid}                   — scoped by system
 *       GET    /api/v1/accesslevel/{systemid}/{accesslevelid}   — composite PK get
 *       POST   /api/v1/accesslevel                              — create
 *       PUT    /api/v1/accesslevel/{systemid}/{accesslevelid}   — update; composite PK is authoritative
 *       DELETE /api/v1/accesslevel/{systemid}/{accesslevelid}   — delete
 *   - Composite PK (systemid int + accesslevelid int)
 *   - Note: AccessLevelReq does not exist in backend contracts — AccessLevelRes shape used for write; using AccessLevelRes as request body
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Response contract: AccessLevelRes from Apha.Common.Contracts.PIMS
 *
 * PRESERVED:
 *   - Composite PK semantics (systemid + accesslevelid)
 *   - GetBySystemId scoped list endpoint preserved
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: AccessLevelReq does not exist in backend — body uses AccessLevelRes shape; create dedicated request contract if write semantics differ
 */

using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsAccessLevelApiClient : IPimsAccessLevelApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: S1192 — base URL extracted to const; matches backend AccessLevelController [Route("api/v{version:apiVersion}/accesslevel")]
        private const string BaseUrl = "api/v1/accesslevel";

        public PimsAccessLevelApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /api/v1/accesslevel — full lookup list
        public async Task<ApiResponseDto<List<AccessLevelDto>>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<AccessLevelRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessLevelDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessLevelDto>>>(response);
                return ApiResponseDto<List<AccessLevelDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessLevelDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessLevel data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/accesslevel/{systemid:int} — scoped by system
        public async Task<ApiResponseDto<List<AccessLevelDto>>> GetBySystemIdAsync(int systemid)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}";
                var response = await _http.GetAsync<List<AccessLevelRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessLevelDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessLevelDto>>>(response);
                return ApiResponseDto<List<AccessLevelDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessLevelDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessLevel by system ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/accesslevel/{systemid:int}/{accesslevelid:int} — composite PK get
        public async Task<ApiResponseDto<AccessLevelDto>> GetByIdAsync(int systemid, int accesslevelid)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}/{accesslevelid}";
                var response = await _http.GetAsync<AccessLevelRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessLevelDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessLevelDto>>(response);
                return ApiResponseDto<AccessLevelDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessLevelDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessLevel by composite ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST /api/v1/accesslevel — AccessLevelRes used as request body (no dedicated AccessLevelReq contract)
        public async Task<ApiResponseDto<AccessLevelDto>> CreateAsync(AccessLevelDto dto)
        {
            try
            {
                // TRANSFORMENGINE TODO STUB: AccessLevelReq does not exist; mapping AccessLevelDto -> AccessLevelRes as request body until dedicated Req contract is created
                var request = _mapper.Map<AccessLevelRes>(dto);
                var response = await _http.PostAsync<AccessLevelRes, AccessLevelRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessLevelDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessLevelDto>>(response);
                return ApiResponseDto<AccessLevelDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessLevelDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create AccessLevel", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT /api/v1/accesslevel/{systemid:int}/{accesslevelid:int} — composite PK is authoritative; AccessLevelRes used as request body
        public async Task<ApiResponseDto<AccessLevelDto>> UpdateAsync(int systemid, int accesslevelid, AccessLevelDto dto)
        {
            try
            {
                // TRANSFORMENGINE TODO STUB: AccessLevelReq does not exist; mapping AccessLevelDto -> AccessLevelRes as request body until dedicated Req contract is created
                var request = _mapper.Map<AccessLevelRes>(dto);
                var url = $"{BaseUrl}/{systemid}/{accesslevelid}";
                var response = await _http.PutAsync<AccessLevelRes, AccessLevelRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessLevelDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessLevelDto>>(response);
                return ApiResponseDto<AccessLevelDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessLevelDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update AccessLevel", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE /api/v1/accesslevel/{systemid:int}/{accesslevelid:int}
        public async Task<ApiResponseDto<bool>> DeleteAsync(int systemid, int accesslevelid)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}/{accesslevelid}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete AccessLevel", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

/*
 * TRANSFORMENGINE MIGRATION — PimsAccessUserLevelApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New HTTP API client implementing IPimsAccessUserLevelApiClient
 *   - Binds to backend AccessUserLevelController routes:
 *       GET    /api/v1/accessuserlevel                                         — full list
 *       GET    /api/v1/accessuserlevel/{systemid}                              — scoped by system
 *       GET    /api/v1/accessuserlevel/{systemid}/{ntlogin}                    — scoped by user within system
 *       GET    /api/v1/accessuserlevel/{systemid}/{ntlogin}/{accesslevelid}    — triple composite PK get
 *       POST   /api/v1/accessuserlevel                                         — create assignment
 *       DELETE /api/v1/accessuserlevel/{systemid}/{ntlogin}/{accesslevelid}    — delete by triple composite PK
 *   - Triple composite PK (systemid int + ntlogin string + accesslevelid int) — Uri.EscapeDataString applied to ntlogin
 *   - No PUT endpoint — assignment table has no mutable fields beyond composite PK
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Req/Res contracts: AccessUserLevelReq, AccessUserLevelRes from Apha.Common.Contracts.PIMS
 *
 * PRESERVED:
 *   - Triple composite PK semantics (systemid + ntlogin + accesslevelid)
 *   - GetBySystemId and GetByUser scoped list endpoints preserved
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm triple composite delete route is acceptable for client consumers
 */

using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsAccessUserLevelApiClient : IPimsAccessUserLevelApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: S1192 — base URL extracted to const; matches backend AccessUserLevelController [Route("api/v{version:apiVersion}/accessuserlevel")]
        private const string BaseUrl = "api/v1/accessuserlevel";

        public PimsAccessUserLevelApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /api/v1/accessuserlevel — full list
        public async Task<ApiResponseDto<List<AccessUserLevelDto>>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<AccessUserLevelRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(response);
                return ApiResponseDto<List<AccessUserLevelDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessUserLevelDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUserLevel data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/accessuserlevel/{systemid:int} — scoped by system
        public async Task<ApiResponseDto<List<AccessUserLevelDto>>> GetBySystemIdAsync(int systemid)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}";
                var response = await _http.GetAsync<List<AccessUserLevelRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(response);
                return ApiResponseDto<List<AccessUserLevelDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessUserLevelDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUserLevel by system ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/accessuserlevel/{systemid:int}/{ntlogin} — scoped by user within system; Uri.EscapeDataString on ntlogin
        public async Task<ApiResponseDto<List<AccessUserLevelDto>>> GetByUserAsync(int systemid, string ntlogin)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}/{Uri.EscapeDataString(ntlogin)}";
                var response = await _http.GetAsync<List<AccessUserLevelRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(response);
                return ApiResponseDto<List<AccessUserLevelDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessUserLevelDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUserLevel by user", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/accessuserlevel/{systemid:int}/{ntlogin}/{accesslevelid:int} — triple composite PK get; Uri.EscapeDataString on ntlogin
        public async Task<ApiResponseDto<AccessUserLevelDto>> GetByIdAsync(int systemid, string ntlogin, int accesslevelid)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}/{Uri.EscapeDataString(ntlogin)}/{accesslevelid}";
                var response = await _http.GetAsync<AccessUserLevelRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessUserLevelDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessUserLevelDto>>(response);
                return ApiResponseDto<AccessUserLevelDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessUserLevelDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUserLevel by composite ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST /api/v1/accessuserlevel — create assignment
        public async Task<ApiResponseDto<AccessUserLevelDto>> CreateAsync(AccessUserLevelDto dto)
        {
            try
            {
                var request = _mapper.Map<AccessUserLevelReq>(dto);
                var response = await _http.PostAsync<AccessUserLevelReq, AccessUserLevelRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessUserLevelDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessUserLevelDto>>(response);
                return ApiResponseDto<AccessUserLevelDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessUserLevelDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create AccessUserLevel", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE /api/v1/accessuserlevel/{systemid:int}/{ntlogin}/{accesslevelid:int} — triple composite PK delete; no PUT (no mutable fields)
        public async Task<ApiResponseDto<bool>> DeleteAsync(int systemid, string ntlogin, int accesslevelid)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}/{Uri.EscapeDataString(ntlogin)}/{accesslevelid}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete AccessUserLevel", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

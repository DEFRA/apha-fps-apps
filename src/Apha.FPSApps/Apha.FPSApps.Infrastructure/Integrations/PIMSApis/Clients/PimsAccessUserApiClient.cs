/*
 * TRANSFORMENGINE MIGRATION — PimsAccessUserApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New HTTP API client implementing IPimsAccessUserApiClient
 *   - Binds to backend AccessUserController routes:
 *       GET    /api/v1/accessuser                          — full list
 *       GET    /api/v1/accessuser/{systemid}               — scoped by system (Admin tab system selector)
 *       GET    /api/v1/accessuser/{systemid}/{ntlogin}     — composite PK get
 *       POST   /api/v1/accessuser                          — create
 *       PUT    /api/v1/accessuser/{systemid}/{ntlogin}     — update; composite PK is authoritative
 *       DELETE /api/v1/accessuser/{systemid}/{ntlogin}     — delete
 *   - Composite PK (systemid int + ntlogin string) — Uri.EscapeDataString applied to ntlogin segment
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Req/Res contracts: AccessUserReq, AccessUserRes from Apha.Common.Contracts.PIMS
 *
 * PRESERVED:
 *   - Composite PK semantics (systemid + ntlogin)
 *   - GetBySystemId scoped list endpoint preserved for Admin tab system filtering
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm systemid is client-provided vs session-derived — see backend controller deferred note
 */

using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsAccessUserApiClient : IPimsAccessUserApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: S1192 — base URL extracted to const; matches backend AccessUserController [Route("api/v{version:apiVersion}/accessuser")]
        private const string BaseUrl = "api/v1/accessuser";

        public PimsAccessUserApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /api/v1/accessuser — full list
        public async Task<ApiResponseDto<List<AccessUserDto>>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<AccessUserRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessUserDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessUserDto>>>(response);
                return ApiResponseDto<List<AccessUserDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessUserDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUser data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/accessuser/{systemid:int} — scoped by system; satisfiable from Admin tab system selector
        public async Task<ApiResponseDto<List<AccessUserDto>>> GetBySystemIdAsync(int systemid)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}";
                var response = await _http.GetAsync<List<AccessUserRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessUserDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessUserDto>>>(response);
                return ApiResponseDto<List<AccessUserDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessUserDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUser by system ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/accessuser/{systemid:int}/{ntlogin} — composite PK get; Uri.EscapeDataString on ntlogin
        public async Task<ApiResponseDto<AccessUserDto>> GetByIdAsync(int systemid, string ntlogin)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}/{Uri.EscapeDataString(ntlogin)}";
                var response = await _http.GetAsync<AccessUserRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessUserDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessUserDto>>(response);
                return ApiResponseDto<AccessUserDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessUserDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUser by composite ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST /api/v1/accessuser — create
        public async Task<ApiResponseDto<AccessUserDto>> CreateAsync(AccessUserDto dto)
        {
            try
            {
                var request = _mapper.Map<AccessUserReq>(dto);
                var response = await _http.PostAsync<AccessUserReq, AccessUserRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessUserDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessUserDto>>(response);
                return ApiResponseDto<AccessUserDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessUserDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create AccessUser", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT /api/v1/accessuser/{systemid:int}/{ntlogin} — composite PK is authoritative; Uri.EscapeDataString on ntlogin
        public async Task<ApiResponseDto<AccessUserDto>> UpdateAsync(int systemid, string ntlogin, AccessUserDto dto)
        {
            try
            {
                var request = _mapper.Map<AccessUserReq>(dto);
                var url = $"{BaseUrl}/{systemid}/{Uri.EscapeDataString(ntlogin)}";
                var response = await _http.PutAsync<AccessUserReq, AccessUserRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessUserDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessUserDto>>(response);
                return ApiResponseDto<AccessUserDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessUserDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update AccessUser", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE /api/v1/accessuser/{systemid:int}/{ntlogin} — Uri.EscapeDataString on ntlogin
        public async Task<ApiResponseDto<bool>> DeleteAsync(int systemid, string ntlogin)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}/{Uri.EscapeDataString(ntlogin)}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete AccessUser", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

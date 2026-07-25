/*
 * TRANSFORMENGINE MIGRATION — PimsSettingApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New HTTP API client implementing IPimsSettingApiClient
 *   - Binds to backend SettingController routes:
 *       GET /api/v1/setting                — full settings list
 *       GET /api/v1/setting/userupdateable — filtered list for user UI
 *       GET /api/v1/setting/{id}           — get by string PK
 *       PUT /api/v1/setting/{id}           — update setting value
 *   - No create/delete endpoints — settings are pre-configured rows (update-only)
 *   - String PK (id) — Uri.EscapeDataString applied before URL embedding
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Req/Res contracts: SettingReq, SettingRes from Apha.Common.Contracts.PIMS
 *
 * PRESERVED:
 *   - Read-only list of all settings and user-updateable-only filtered list
 *   - String PK (setting id) semantics
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm whether admin-only guard is required on UpdateAsync (see backend controller deferred note)
 *   - TRANSFORMENGINE TODO: confirm TestSetting environment-conditional editing
 */

using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsSettingApiClient : IPimsSettingApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: S1192 — base URL extracted to const; matches backend SettingController [Route("api/v{version:apiVersion}/setting")]
        private const string BaseUrl = "api/v1/setting";

        public PimsSettingApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /api/v1/setting — full settings list
        public async Task<ApiResponseDto<List<SettingDto>>> GetAllSettingsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<SettingRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<SettingDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<SettingDto>>>(response);
                return ApiResponseDto<List<SettingDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<SettingDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve Setting data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/setting/userupdateable — filtered list for user UI
        public async Task<ApiResponseDto<List<SettingDto>>> GetAllUserUpdateableSettingsAsync()
        {
            try
            {
                var url = $"{BaseUrl}/userupdateable";
                var response = await _http.GetAsync<List<SettingRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<SettingDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<SettingDto>>>(response);
                return ApiResponseDto<List<SettingDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<SettingDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve user-updateable Setting data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/setting/{id} — string PK; Uri.EscapeDataString applied
        public async Task<ApiResponseDto<SettingDto>> GetSettingByIdAsync(string id)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(id)}";
                var response = await _http.GetAsync<SettingRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<SettingDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<SettingDto>>(response);
                return ApiResponseDto<SettingDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<SettingDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve Setting by ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT /api/v1/setting/{id} — route id is authoritative; Uri.EscapeDataString applied; no create/delete (pre-configured rows)
        public async Task<ApiResponseDto<SettingDto>> UpdateSettingAsync(string id, SettingDto dto)
        {
            try
            {
                var request = _mapper.Map<SettingReq>(dto);
                var url = $"{BaseUrl}/{Uri.EscapeDataString(id)}";
                var response = await _http.PutAsync<SettingReq, SettingRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<SettingDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<SettingDto>>(response);
                return ApiResponseDto<SettingDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<SettingDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update Setting", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

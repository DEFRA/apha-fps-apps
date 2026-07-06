/*
 * TRANSFORMENGINE MIGRATION — PimsFrequencyApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New HTTP API client implementing IPimsFrequencyApiClient
 *   - Binds to backend FrequencyController routes:
 *       GET    /api/v1/frequency                 — full list
 *       GET    /api/v1/frequency/{frequencyid}   — get by integer PK
 *       POST   /api/v1/frequency                 — create
 *       PUT    /api/v1/frequency/{frequencyid}   — update; route PK is authoritative
 *       DELETE /api/v1/frequency/{frequencyid}   — delete
 *   - Integer PK (frequencyid) — matches backend controller route constraint {frequencyid:int}
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Req/Res contracts: FrequencyReq, FrequencyRes from Apha.Common.Contracts.PIMS
 *
 * PRESERVED:
 *   - All CRUD semantics matching IPimsFrequencyApiClient interface (GetAll, GetById, Create, Update, Delete)
 *   - Integer PK (frequencyid) semantics
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
    public class PimsFrequencyApiClient : IPimsFrequencyApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: S1192 — base URL extracted to const; matches backend FrequencyController [Route("api/v{version:apiVersion}/frequency")]
        private const string BaseUrl = "api/v1/frequency";

        public PimsFrequencyApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /api/v1/frequency — full list
        public async Task<ApiResponseDto<List<FrequencyDto>>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<FrequencyRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<FrequencyDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<FrequencyDto>>>(response);
                return ApiResponseDto<List<FrequencyDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<FrequencyDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve Frequency data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/frequency/{frequencyid:int}
        public async Task<ApiResponseDto<FrequencyDto>> GetByIdAsync(int frequencyid)
        {
            try
            {
                var url = $"{BaseUrl}/{frequencyid}";
                var response = await _http.GetAsync<FrequencyRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<FrequencyDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<FrequencyDto>>(response);
                return ApiResponseDto<FrequencyDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<FrequencyDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve Frequency by ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST /api/v1/frequency
        public async Task<ApiResponseDto<FrequencyDto>> CreateAsync(FrequencyDto dto)
        {
            try
            {
                var request = _mapper.Map<FrequencyReq>(dto);
                var response = await _http.PostAsync<FrequencyReq, FrequencyRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<FrequencyDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<FrequencyDto>>(response);
                return ApiResponseDto<FrequencyDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<FrequencyDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create Frequency", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT /api/v1/frequency/{frequencyid:int} — route PK is authoritative
        public async Task<ApiResponseDto<FrequencyDto>> UpdateAsync(int frequencyid, FrequencyDto dto)
        {
            try
            {
                var request = _mapper.Map<FrequencyReq>(dto);
                var url = $"{BaseUrl}/{frequencyid}";
                var response = await _http.PutAsync<FrequencyReq, FrequencyRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<FrequencyDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<FrequencyDto>>(response);
                return ApiResponseDto<FrequencyDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<FrequencyDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update Frequency", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE /api/v1/frequency/{frequencyid:int}
        public async Task<ApiResponseDto<bool>> DeleteAsync(int frequencyid)
        {
            try
            {
                var url = $"{BaseUrl}/{frequencyid}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete Frequency", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

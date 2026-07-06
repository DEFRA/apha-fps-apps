/*
 * TRANSFORMENGINE MIGRATION — PimsAccessSystemApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New HTTP API client implementing IPimsAccessSystemApiClient
 *   - Binds to backend AccessSystemController routes:
 *       GET /api/v1/accesssystem              — full reference list
 *       GET /api/v1/accesssystem/{systemid}   — get by integer PK
 *   - Read-only resource: no create/update/delete endpoints (reference/lookup data)
 *   - Integer PK (systemid)
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Response contract: AccessSystemRes from Apha.Common.Contracts.PIMS
 *
 * PRESERVED:
 *   - Read-only lookup semantics matching AccessSystemController actions
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsAccessSystemApiClient : IPimsAccessSystemApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: S1192 — base URL extracted to const; matches backend AccessSystemController [Route("api/v{version:apiVersion}/accesssystem")]
        private const string BaseUrl = "api/v1/accesssystem";

        public PimsAccessSystemApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /api/v1/accesssystem — full reference lookup list
        public async Task<ApiResponseDto<List<AccessSystemDto>>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<AccessSystemRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessSystemDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessSystemDto>>>(response);
                return ApiResponseDto<List<AccessSystemDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessSystemDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessSystem data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/accesssystem/{systemid:int}
        public async Task<ApiResponseDto<AccessSystemDto>> GetByIdAsync(int systemid)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}";
                var response = await _http.GetAsync<AccessSystemRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessSystemDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessSystemDto>>(response);
                return ApiResponseDto<AccessSystemDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessSystemDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessSystem by ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

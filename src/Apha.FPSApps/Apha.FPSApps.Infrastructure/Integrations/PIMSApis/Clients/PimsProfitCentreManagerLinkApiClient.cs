/*
 * TRANSFORMENGINE MIGRATION — PimsProfitCentreManagerLinkApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New HTTP API client implementing IPimsProfitCentreManagerLinkApiClient
 *   - Binds to backend ProfitCentreManagerLinkController routes:
 *       GET    /api/v1/profitcentremanagerlink                          — full list
 *       GET    /api/v1/profitcentremanagerlink/{profitcentre}           — scoped by profit centre
 *       GET    /api/v1/profitcentremanagerlink/{profitcentre}/{manager} — composite natural PK get
 *       POST   /api/v1/profitcentremanagerlink                          — create link
 *       DELETE /api/v1/profitcentremanagerlink/{profitcentre}/{manager} — delete by composite natural PK
 *   - Composite natural PK (profitcentre string + manager string) — Uri.EscapeDataString applied to both segments
 *   - No PUT endpoint — link table has no mutable fields beyond composite PK
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Req/Res contracts: ProfitCentreManagerLinkReq, ProfitCentreManagerLinkRes from Apha.Common.Contracts.PIMS
 *
 * PRESERVED:
 *   - Composite natural PK semantics (profitcentre + manager)
 *   - GetByProfitCentre scoped list endpoint preserved
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm composite natural PK delete route with URL-encoded string segments is acceptable
 */

using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsProfitCentreManagerLinkApiClient : IPimsProfitCentreManagerLinkApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: S1192 — base URL extracted to const; matches backend ProfitCentreManagerLinkController [Route("api/v{version:apiVersion}/profitcentremanagerlink")]
        private const string BaseUrl = "api/v1/profitcentremanagerlink";

        public PimsProfitCentreManagerLinkApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /api/v1/profitcentremanagerlink — full list
        public async Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ProfitCentreManagerLinkRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);
                return ApiResponseDto<List<ProfitCentreManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProfitCentreManagerLinkDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProfitCentreManagerLink data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/profitcentremanagerlink/{profitcentre} — scoped by profit centre; Uri.EscapeDataString applied
        public async Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetByProfitCentreAsync(string profitcentre)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(profitcentre)}";
                var response = await _http.GetAsync<List<ProfitCentreManagerLinkRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);
                return ApiResponseDto<List<ProfitCentreManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProfitCentreManagerLinkDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProfitCentreManagerLink by profit centre", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/profitcentremanagerlink/{profitcentre}/{manager} — composite natural PK get; Uri.EscapeDataString on both segments
        public async Task<ApiResponseDto<ProfitCentreManagerLinkDto>> GetByIdAsync(string profitcentre, string manager)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(profitcentre)}/{Uri.EscapeDataString(manager)}";
                var response = await _http.GetAsync<ProfitCentreManagerLinkRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProfitCentreManagerLinkDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreManagerLinkDto>>(response);
                return ApiResponseDto<ProfitCentreManagerLinkDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProfitCentreManagerLinkDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProfitCentreManagerLink by composite ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST /api/v1/profitcentremanagerlink — create link
        public async Task<ApiResponseDto<ProfitCentreManagerLinkDto>> CreateAsync(ProfitCentreManagerLinkDto dto)
        {
            try
            {
                var request = _mapper.Map<ProfitCentreManagerLinkReq>(dto);
                var response = await _http.PostAsync<ProfitCentreManagerLinkReq, ProfitCentreManagerLinkRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProfitCentreManagerLinkDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreManagerLinkDto>>(response);
                return ApiResponseDto<ProfitCentreManagerLinkDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProfitCentreManagerLinkDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create ProfitCentreManagerLink", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE /api/v1/profitcentremanagerlink/{profitcentre}/{manager} — composite natural PK delete; no PUT (no mutable fields)
        public async Task<ApiResponseDto<bool>> DeleteAsync(string profitcentre, string manager)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(profitcentre)}/{Uri.EscapeDataString(manager)}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete ProfitCentreManagerLink", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

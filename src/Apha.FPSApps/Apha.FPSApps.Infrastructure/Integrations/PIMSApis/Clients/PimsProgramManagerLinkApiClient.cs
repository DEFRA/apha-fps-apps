/*
 * TRANSFORMENGINE MIGRATION — PimsProgramManagerLinkApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New HTTP API client implementing IPimsProgramManagerLinkApiClient
 *   - Binds to backend ProgramManagerLinkController routes:
 *       GET    /api/v1/programmanagerlink                      — full list
 *       GET    /api/v1/programmanagerlink/{program}            — scoped by program
 *       GET    /api/v1/programmanagerlink/{program}/{manager}  — composite natural PK get
 *       POST   /api/v1/programmanagerlink                      — create link
 *       DELETE /api/v1/programmanagerlink/{program}/{manager}  — delete by composite natural PK
 *   - Composite natural PK (program string + manager string) — Uri.EscapeDataString applied to both segments
 *   - No PUT endpoint — link table has no mutable fields beyond composite PK
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Req/Res contracts: ProgramManagerLinkReq, ProgramManagerLinkRes from Apha.Common.Contracts.PIMS
 *
 * PRESERVED:
 *   - Composite natural PK semantics (program + manager)
 *   - GetByProgram scoped list endpoint preserved
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
    public class PimsProgramManagerLinkApiClient : IPimsProgramManagerLinkApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: S1192 — base URL extracted to const; matches backend ProgramManagerLinkController [Route("api/v{version:apiVersion}/programmanagerlink")]
        private const string BaseUrl = "api/v1/programmanagerlink";

        public PimsProgramManagerLinkApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /api/v1/programmanagerlink — full list
        public async Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ProgramManagerLinkRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);
                return ApiResponseDto<List<ProgramManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProgramManagerLinkDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProgramManagerLink data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/programmanagerlink/{program} — scoped by program; Uri.EscapeDataString applied
        public async Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetByProgramAsync(string program)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(program)}";
                var response = await _http.GetAsync<List<ProgramManagerLinkRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);
                return ApiResponseDto<List<ProgramManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProgramManagerLinkDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProgramManagerLink by program", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/programmanagerlink/{program}/{manager} — composite natural PK get; Uri.EscapeDataString on both segments
        public async Task<ApiResponseDto<ProgramManagerLinkDto>> GetByIdAsync(string program, string manager)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(program)}/{Uri.EscapeDataString(manager)}";
                var response = await _http.GetAsync<ProgramManagerLinkRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProgramManagerLinkDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProgramManagerLinkDto>>(response);
                return ApiResponseDto<ProgramManagerLinkDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProgramManagerLinkDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProgramManagerLink by composite ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST /api/v1/programmanagerlink — create link
        public async Task<ApiResponseDto<ProgramManagerLinkDto>> CreateAsync(ProgramManagerLinkDto dto)
        {
            try
            {
                var request = _mapper.Map<ProgramManagerLinkReq>(dto);
                var response = await _http.PostAsync<ProgramManagerLinkReq, ProgramManagerLinkRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProgramManagerLinkDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProgramManagerLinkDto>>(response);
                return ApiResponseDto<ProgramManagerLinkDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProgramManagerLinkDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create ProgramManagerLink", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE /api/v1/programmanagerlink/{program}/{manager} — composite natural PK delete; no PUT (no mutable fields)
        public async Task<ApiResponseDto<bool>> DeleteAsync(string program, string manager)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(program)}/{Uri.EscapeDataString(manager)}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete ProgramManagerLink", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

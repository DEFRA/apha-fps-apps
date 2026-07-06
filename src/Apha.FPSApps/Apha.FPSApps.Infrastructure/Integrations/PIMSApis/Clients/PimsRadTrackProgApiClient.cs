/*
 * TRANSFORMENGINE MIGRATION — PimsRadTrackProgApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New HTTP API client implementing IPimsRadTrackProgApiClient
 *   - Binds to backend RadTrackProgController routes:
 *       GET    /api/v1/radtrackprog               — full list for Programme Tab grid
 *       GET    /api/v1/radtrackprog/{program}     — get by natural string PK (program varchar(10))
 *       POST   /api/v1/radtrackprog               — create new programme; natural PK client-supplied
 *       PUT    /api/v1/radtrackprog/{program}     — update; route PK is authoritative
 *       DELETE /api/v1/radtrackprog/{program}     — delete by natural string PK
 *   - Natural string PK (program varchar(10)) — matches backend controller route
 *   - Programme Tab CRUD from frmPIMSMainForm
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Req/Res contracts: RadTrackProgReq, RadTrackProgRes from Apha.Common.Contracts.PIMS
 *
 * PRESERVED:
 *   - All CRUD semantics matching IPimsRadTrackProgApiClient interface (GetAll, GetById, Create, Update, Delete)
 *   - Natural string PK (program) semantics
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm Programme Tab maps solely to tblradtrackprog (see backend controller deferred note)
 *   - TRANSFORMENGINE TODO: verify publicationprefix varchar(5) max length enforced via validation attribute on RadTrackProgReq
 */

using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsRadTrackProgApiClient : IPimsRadTrackProgApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: S1192 — base URL extracted to const; matches backend RadTrackProgController [Route("api/v{version:apiVersion}/radtrackprog")]
        private const string BaseUrl = "api/v1/radtrackprog";

        public PimsRadTrackProgApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /api/v1/radtrackprog — full list for Programme Tab grid; no pagination needed
        public async Task<ApiResponseDto<List<RadTrackProgDto>>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<RadTrackProgRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<RadTrackProgDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<RadTrackProgDto>>>(response);
                return ApiResponseDto<List<RadTrackProgDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<RadTrackProgDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve RadTrackProg data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/radtrackprog/{program} — natural string PK lookup
        public async Task<ApiResponseDto<RadTrackProgDto>> GetByIdAsync(string program)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(program)}";
                var response = await _http.GetAsync<RadTrackProgRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);
                return ApiResponseDto<RadTrackProgDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<RadTrackProgDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve RadTrackProg by program", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST /api/v1/radtrackprog — create new programme; natural PK is client-supplied in request body
        public async Task<ApiResponseDto<RadTrackProgDto>> CreateAsync(RadTrackProgDto dto)
        {
            try
            {
                var request = _mapper.Map<RadTrackProgReq>(dto);
                var response = await _http.PostAsync<RadTrackProgReq, RadTrackProgRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);
                return ApiResponseDto<RadTrackProgDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<RadTrackProgDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create RadTrackProg", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT /api/v1/radtrackprog/{program} — route PK is authoritative; mirrors backend dto.Program = program guard
        public async Task<ApiResponseDto<RadTrackProgDto>> UpdateAsync(string program, RadTrackProgDto dto)
        {
            try
            {
                var request = _mapper.Map<RadTrackProgReq>(dto);
                var url = $"{BaseUrl}/{Uri.EscapeDataString(program)}";
                var response = await _http.PutAsync<RadTrackProgReq, RadTrackProgRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);
                return ApiResponseDto<RadTrackProgDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<RadTrackProgDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update RadTrackProg", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE /api/v1/radtrackprog/{program} — delete by natural string PK
        public async Task<ApiResponseDto<bool>> DeleteAsync(string program)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(program)}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete RadTrackProg", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

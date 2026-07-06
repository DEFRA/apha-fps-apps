/*
 * TRANSFORMENGINE MIGRATION — PimsReportGroupApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New HTTP API client implementing IPimsReportGroupApiClient
 *   - Binds to backend ReportGroupController routes: GET/POST /api/v1/reportgroup, GET/PUT/DELETE /api/v1/reportgroup/{groupid:int}
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Mapper used for all success response mappings (ReportGroupRes -> ReportGroupDto via ApiResponseDto)
 *   - Req/Res contracts: ReportGroupReq, ReportGroupRes from Apha.Common.Contracts.PIMS
 *
 * PRESERVED:
 *   - All CRUD semantics matching IPimsReportGroupApiClient interface (GetAll, GetById, Create, Update, Delete)
 *   - Integer PK (groupid) matching backend route constraint {groupid:int}
 *   - ReportGroup also serves as lookup source for Report dropdown
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
    public class PimsReportGroupApiClient : IPimsReportGroupApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: S1192 — base URL extracted to const; matches backend ReportGroupController [Route("api/v{version:apiVersion}/reportgroup")]
        private const string BaseUrl = "api/v1/reportgroup";

        public PimsReportGroupApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /api/v1/reportgroup — full list (also used as Report dropdown source)
        public async Task<ApiResponseDto<List<ReportGroupDto>>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ReportGroupRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ReportGroupDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ReportGroupDto>>>(response);
                return ApiResponseDto<List<ReportGroupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ReportGroupDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ReportGroup data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/reportgroup/{groupid:int}
        public async Task<ApiResponseDto<ReportGroupDto>> GetByIdAsync(int groupid)
        {
            try
            {
                var url = $"{BaseUrl}/{groupid}";
                var response = await _http.GetAsync<ReportGroupRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);
                return ApiResponseDto<ReportGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReportGroupDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ReportGroup by ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST /api/v1/reportgroup
        public async Task<ApiResponseDto<ReportGroupDto>> CreateAsync(ReportGroupDto dto)
        {
            try
            {
                var request = _mapper.Map<ReportGroupReq>(dto);
                var response = await _http.PostAsync<ReportGroupReq, ReportGroupRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);
                return ApiResponseDto<ReportGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReportGroupDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create ReportGroup", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT /api/v1/reportgroup/{groupid:int} — route PK (groupid) is authoritative
        public async Task<ApiResponseDto<ReportGroupDto>> UpdateAsync(int groupid, ReportGroupDto dto)
        {
            try
            {
                var request = _mapper.Map<ReportGroupReq>(dto);
                var url = $"{BaseUrl}/{groupid}";
                var response = await _http.PutAsync<ReportGroupReq, ReportGroupRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);
                return ApiResponseDto<ReportGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReportGroupDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update ReportGroup", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE /api/v1/reportgroup/{groupid:int}
        public async Task<ApiResponseDto<bool>> DeleteAsync(int groupid)
        {
            try
            {
                var url = $"{BaseUrl}/{groupid}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete ReportGroup", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

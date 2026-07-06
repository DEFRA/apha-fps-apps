/*
 * TRANSFORMENGINE MIGRATION — PimsReportApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New HTTP API client implementing IPimsReportApiClient
 *   - Binds to backend ReportController routes: GET/POST /api/v1/report, GET/PUT/DELETE /api/v1/report/{id:int}
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Mapper used for all success response mappings (ReportRes -> ReportDto via ApiResponseDto)
 *   - Req/Res contracts: ReportReq, ReportRes from Apha.Common.Contracts.PIMS
 *
 * PRESERVED:
 *   - All CRUD semantics matching IPimsReportApiClient interface (GetAll, GetById, Create, Update, Delete)
 *   - Integer PK (id) matching backend route constraint {id:int}
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm role requirements match environment-specific access policy for report management
 */

using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsReportApiClient : IPimsReportApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: S1192 — base URL extracted to const; matches backend ReportController [Route("api/v{version:apiVersion}/report")]
        private const string BaseUrl = "api/v1/report";

        public PimsReportApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /api/v1/report — full list; no required params (Reports Tab grid loads all)
        public async Task<ApiResponseDto<List<ReportDto>>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ReportRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ReportDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ReportDto>>>(response);
                return ApiResponseDto<List<ReportDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ReportDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve Report data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/report/{id:int}
        public async Task<ApiResponseDto<ReportDto>> GetByIdAsync(int id)
        {
            try
            {
                var url = $"{BaseUrl}/{id}";
                var response = await _http.GetAsync<ReportRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReportDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReportDto>>(response);
                return ApiResponseDto<ReportDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReportDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve Report by ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST /api/v1/report
        public async Task<ApiResponseDto<ReportDto>> CreateAsync(ReportDto dto)
        {
            try
            {
                var request = _mapper.Map<ReportReq>(dto);
                var response = await _http.PostAsync<ReportReq, ReportRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReportDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReportDto>>(response);
                return ApiResponseDto<ReportDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReportDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create Report", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT /api/v1/report/{id:int} — route PK (id) is authoritative
        public async Task<ApiResponseDto<ReportDto>> UpdateAsync(int id, ReportDto dto)
        {
            try
            {
                var request = _mapper.Map<ReportReq>(dto);
                var url = $"{BaseUrl}/{id}";
                var response = await _http.PutAsync<ReportReq, ReportRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReportDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReportDto>>(response);
                return ApiResponseDto<ReportDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReportDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update Report", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE /api/v1/report/{id:int}
        public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
        {
            try
            {
                var url = $"{BaseUrl}/{id}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete Report", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

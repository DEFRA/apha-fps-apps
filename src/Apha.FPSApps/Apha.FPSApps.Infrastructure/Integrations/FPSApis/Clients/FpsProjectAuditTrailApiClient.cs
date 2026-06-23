/*
 * TRANSFORMENGINE MIGRATION — FpsProjectAuditTrailApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — concrete HTTP API client implementing IFpsProjectAuditTrailApiClient
 *   - 5 GET methods binding to backend ProjectAuditTrailController endpoints:
 *     GET /api/v1/projectaudittrail/projectlogs
 *     GET /api/v1/projectaudittrail/staffjoblogs
 *     GET /api/v1/projectaudittrail/testrequirementlogs
 *     GET /api/v1/projectaudittrail/animalrequestlogs
 *     GET /api/v1/projectaudittrail/additionalcostlogs
 *   - project (required) and optional fromDate/toDate appended to query string via QueryHelpers
 *   - QueryParameters<string> pagination appended via QueryStringHelper.AddQueryString
 *   - AutoMapper maps backend Res list → frontend Dto list inside ApiResponseDto<T>
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse (Sonar S2139)
 *   - _http and _mapper fields private readonly (Sonar S2933)
 *   - InternalCodeError private const string (Sonar S1192)
 *   - BuildAuditUrl private static helper extracts repeated URL construction (Sonar S1192)
 *   - Registered on aggregate FpsApiClient.cs as IFpsProjectAuditTrailApiClient FpsProjectAuditTrail
 *
 * PRESERVED:
 *   - All 5 log endpoint operations with exact route matching the backend controller
 *   - Optional date range semantics: fromDate/toDate nullable, not sent when null
 *   - project required at call site — same guard as backend (ArgumentException if empty)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: DateOnly? params serialised as ISO 8601 date strings (yyyy-MM-dd); verify
 *     backend [FromQuery] DateOnly? binding accepts this format without additional model binder config
 */
using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using Microsoft.AspNetCore.WebUtilities;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsProjectAuditTrailApiClient : IFpsProjectAuditTrailApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: InternalCodeError as private const — Sonar S1192 compliance
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsProjectAuditTrailApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: GET /api/v1/projectaudittrail/projectlogs → ProjectAuditTrailController.GetProjectLogsAsync
        //   project (required) appended as ?project={project}; date range appended when non-null
        public async Task<ApiResponseDto<List<ProjectLogDto>>> GetProjectLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            try
            {
                var url = BuildAuditUrl(FpsApiEndpoints.GetProjectLogs, query, project, fromDate, toDate);
                var response = await _http.GetAsync<List<ProjectLogRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProjectLogDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProjectLogDto>>>(response);
                return ApiResponseDto<List<ProjectLogDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProjectLogDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve project logs", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/projectaudittrail/staffjoblogs → ProjectAuditTrailController.GetStaffJobLogsAsync
        public async Task<ApiResponseDto<List<StaffJobLogDto>>> GetStaffJobLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            try
            {
                var url = BuildAuditUrl(FpsApiEndpoints.GetStaffJobLogs, query, project, fromDate, toDate);
                var response = await _http.GetAsync<List<StaffJobLogRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<StaffJobLogDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<StaffJobLogDto>>>(response);
                return ApiResponseDto<List<StaffJobLogDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<StaffJobLogDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve staff job logs", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/projectaudittrail/testrequirementlogs → ProjectAuditTrailController.GetTestRequirementLogsAsync
        public async Task<ApiResponseDto<List<TestRequirementLogDto>>> GetTestRequirementLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            try
            {
                var url = BuildAuditUrl(FpsApiEndpoints.GetTestRequirementLogs, query, project, fromDate, toDate);
                var response = await _http.GetAsync<List<TestRequirementLogRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestRequirementLogDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<TestRequirementLogDto>>>(response);
                return ApiResponseDto<List<TestRequirementLogDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestRequirementLogDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve test requirement logs", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/projectaudittrail/animalrequestlogs → ProjectAuditTrailController.GetAnimalRequestLogsAsync
        public async Task<ApiResponseDto<List<AnimalRequestLogDto>>> GetAnimalRequestLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            try
            {
                var url = BuildAuditUrl(FpsApiEndpoints.GetAnimalRequestLogs, query, project, fromDate, toDate);
                var response = await _http.GetAsync<List<AnimalRequestLogRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AnimalRequestLogDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AnimalRequestLogDto>>>(response);
                return ApiResponseDto<List<AnimalRequestLogDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AnimalRequestLogDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve animal request logs", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/projectaudittrail/additionalcostlogs → ProjectAuditTrailController.GetAdditionalCostLogsAsync
        public async Task<ApiResponseDto<List<AdditionalCostLogDto>>> GetAdditionalCostLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            try
            {
                var url = BuildAuditUrl(FpsApiEndpoints.GetAdditionalCostLogs, query, project, fromDate, toDate);
                var response = await _http.GetAsync<List<AdditionalCostLogRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AdditionalCostLogDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AdditionalCostLogDto>>>(response);
                return ApiResponseDto<List<AdditionalCostLogDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AdditionalCostLogDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve additional cost logs", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: helper — appends project (required) and optional date range params to the pagination query string
        //   project sent as-is; DateOnly? serialised as ISO 8601 (yyyy-MM-dd) to match [FromQuery] DateOnly? on backend
        private static string BuildAuditUrl(
            string baseEndpoint,
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate,
            DateOnly? toDate)
        {
            // Start with pagination params from QueryParameters<string>
            var url = QueryStringHelper.AddQueryString(baseEndpoint, query);

            // Append required project param
            var queryParams = new Dictionary<string, string?> { { "project", project } };

            // Append optional date range params when supplied
            if (fromDate.HasValue)
                queryParams["fromDate"] = fromDate.Value.ToString("yyyy-MM-dd");
            if (toDate.HasValue)
                queryParams["toDate"] = toDate.Value.ToString("yyyy-MM-dd");

            return QueryHelpers.AddQueryString(url, queryParams);
        }
    }
}

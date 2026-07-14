/*
 * TRANSFORMENGINE MIGRATION — FpsDepartmentIncomeApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - Stub NotImplementedException bodies replaced with full async HTTP calls via IFpsHttpExecutor
 *   - All 6 interface methods implemented: GetTimeIncomeAsync, GetTestIncomeAsync,
 *     GetAnimalIncomeAsync, GetAdditionalIncomeAsync, GetTotalsAsync, GetPeriodsAsync
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse on failure
 *   - URL base extracted to private const BaseUrl = "api/v1/department-income" (Sonar S1192)
 *   - Optional filter params (project, monthFrom, monthTo) appended via QueryHelpers only when non-null
 *   - Response mapping uses _mapper.Map<ApiResponseDto<T>>(response) on success path
 *   - _http and _mapper declared private readonly (Sonar S2933)
 *   - InternalCodeError declared private const string (Sonar S1192)
 *   - Private helper BuildIncomeUrl() extracts repeated optional-param URL construction
 *
 * PRESERVED:
 *   - All 6 interface method signatures from IFpsDepartmentIncomeApiClient
 *   - Constructor guards: ArgumentNullException on null http and mapper
 *   - Resource is read-only (report form) — no Create/Update/Delete calls
 *   - Backend route paths match DepartmentIncomeController [Route] attributes resolved in Phase 5
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify AutoMapper profile FpsDepartmentIncomeApiDtoMapper maps all
 *     Res properties to Dto counterparts before enabling in production
 *   - TRANSFORMENGINE TODO: fPeriodTotals stored proc backing GetPeriodsAsync must be
 *     re-implemented in DepartmentIncomeRepository (see PeriodLookupRes.cs TODO)
 */

using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.DepartmentIncome;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using Microsoft.AspNetCore.WebUtilities;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsDepartmentIncomeApiClient : IFpsDepartmentIncomeApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";

        // TRANSFORMENGINE: BaseUrl matches backend DepartmentIncomeController [Route("api/v{version:apiVersion}/department-income")]
        private const string BaseUrl = "api/v1/department-income";

        public FpsDepartmentIncomeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/time — optional filter params appended via QueryHelpers
        public async Task<ApiResponseDto<List<DepartmentIncomeTimeDto>>> GetTimeIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            try
            {
                var url = BuildIncomeUrl($"{BaseUrl}/time", project, monthFrom, monthTo);
                var response = await _http.GetAsync<List<DepartmentIncomeTimeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(response);
                return ApiResponseDto<List<DepartmentIncomeTimeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<DepartmentIncomeTimeDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Department Income time data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/tests — optional filter params appended via QueryHelpers
        public async Task<ApiResponseDto<List<DepartmentIncomeTestDto>>> GetTestIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            try
            {
                var url = BuildIncomeUrl($"{BaseUrl}/tests", project, monthFrom, monthTo);
                var response = await _http.GetAsync<List<DepartmentIncomeTestRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(response);
                return ApiResponseDto<List<DepartmentIncomeTestDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<DepartmentIncomeTestDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Department Income test data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/animals — optional filter params appended via QueryHelpers
        public async Task<ApiResponseDto<List<DepartmentIncomeAnimalDto>>> GetAnimalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            try
            {
                var url = BuildIncomeUrl($"{BaseUrl}/animals", project, monthFrom, monthTo);
                var response = await _http.GetAsync<List<DepartmentIncomeAnimalRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<DepartmentIncomeAnimalDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeAnimalDto>>>(response);
                return ApiResponseDto<List<DepartmentIncomeAnimalDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<DepartmentIncomeAnimalDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Department Income animal data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/additional — optional filter params appended via QueryHelpers
        public async Task<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>> GetAdditionalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            try
            {
                var url = BuildIncomeUrl($"{BaseUrl}/additional", project, monthFrom, monthTo);
                var response = await _http.GetAsync<List<DepartmentIncomeAdditionalRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>>(response);
                return ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Department Income additional data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/totals — optional filter params appended via QueryHelpers
        public async Task<ApiResponseDto<List<DepartmentIncomeTotalsDto>>> GetTotalsAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            try
            {
                var url = BuildIncomeUrl($"{BaseUrl}/totals", project, monthFrom, monthTo);
                var response = await _http.GetAsync<List<DepartmentIncomeTotalsRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<DepartmentIncomeTotalsDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeTotalsDto>>>(response);
                return ApiResponseDto<List<DepartmentIncomeTotalsDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<DepartmentIncomeTotalsDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Department Income totals data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/periods — no filter params (lookup endpoint)
        public async Task<ApiResponseDto<List<PeriodLookupDto>>> GetPeriodsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<PeriodLookupRes>>($"{BaseUrl}/periods");
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<PeriodLookupDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<PeriodLookupDto>>>(response);
                return ApiResponseDto<List<PeriodLookupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<PeriodLookupDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Department Income period lookup data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: Private helper — appends optional filter params to an income sub-endpoint URL only when non-null
        private static string BuildIncomeUrl(string endpoint, string? project, int? monthFrom, int? monthTo)
        {
            var queryParams = new Dictionary<string, string?>();

            if (project is not null)
                queryParams["project"] = project;
            if (monthFrom.HasValue)
                queryParams["monthFrom"] = monthFrom.Value.ToString();
            if (monthTo.HasValue)
                queryParams["monthTo"] = monthTo.Value.ToString();

            return queryParams.Count > 0
                ? QueryHelpers.AddQueryString(endpoint, queryParams)
                : endpoint;
        }
    }
}

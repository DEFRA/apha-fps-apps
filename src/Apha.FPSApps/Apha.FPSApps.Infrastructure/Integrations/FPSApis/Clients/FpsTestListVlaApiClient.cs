/*
 * TRANSFORMENGINE MIGRATION — FpsTestListVlaApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New HTTP API client created implementing IFpsTestListVlaApiClient
 *   - All HTTP calls routed via IFpsHttpExecutor to backend TestListVlaController
 *   - BaseUrl matches backend route: api/v1/testlistvla
 *   - Lookup endpoint: GET api/v1/testlistvla/lookup?fpsYear={year} (GetAllByYearAsync)
 *   - Paged list endpoint: GET api/v1/testlistvla?... (GetAllAsync)
 *   - Composite PK (itemCode + fpsYear) used in GetByIdAsync, UpdateAsync, DeleteAsync route segments
 *   - fpsYear appended as query parameter on paged list and lookup calls
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Mapper used for all success-path response transformations (never manual construction)
 *   - _http and _mapper are private readonly (Sonar S2933)
 *   - InternalCodeError and BaseUrl are private const string (Sonar S1192)
 *
 * PRESERVED:
 *   - All 6 interface methods: GetAllAsync, GetAllByYearAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync
 *   - Composite PK parameter ordering: itemCode before fpsYear (matches backend route template)
 *   - Required business context parameter fpsYear on list and lookup methods
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm FpsApiDtoMapper includes TestListVlaRes → TestListVlaDto and
 *     TestListVlaDto → TestListVlaReq mappings before end-to-end testing.
 *   - TRANSFORMENGINE TODO: Confirm PaginationRes<TestListVlaRes> → ApiResponseDto<List<TestListVlaDto>>
 *     is handled by the mapper profile registered in Phase 10.
 */

using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    /// <summary>
    /// HTTP API client for TestOrProduct VLA list management.
    /// Targets backend route: GET/POST/PUT/DELETE api/v1/testlistvla
    /// and lookup: GET api/v1/testlistvla/lookup
    /// Composite PK: ItemCode + FpsYear.
    /// </summary>
    public class FpsTestListVlaApiClient : IFpsTestListVlaApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: InternalCodeError as private const — Sonar S1192 compliance
        private const string InternalCodeError = "INTERNAL_ERROR";

        // TRANSFORMENGINE: BaseUrl matches backend TestListVlaController [Route("api/v{version:apiVersion}/testlistvla")]
        private const string BaseUrl = "api/v1/testlistvla";

        public FpsTestListVlaApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: GET api/v1/testlistvla — paged list; fpsYear appended as query param (required from page year-selector)
        public async Task<ApiResponseDto<List<TestListVlaDto>>> GetAllAsync(QueryParameters<string> query, int fpsYear)
        {
            try
            {
                var urlWithQuery = QueryStringHelper.AddQueryString(BaseUrl, query);
                var url = $"{urlWithQuery}&fpsYear={fpsYear}";
                var response = await _http.GetAsync<List<TestListVlaRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(response);
                return ApiResponseDto<List<TestListVlaDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestListVlaDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve TestListVla data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/testlistvla/lookup?fpsYear={year} — unpaged lookup list for select-list population
        public async Task<ApiResponseDto<List<TestListVlaDto>>> GetAllByYearAsync(int fpsYear)
        {
            try
            {
                var url = $"{BaseUrl}/lookup?fpsYear={fpsYear}";
                var response = await _http.GetAsync<List<TestListVlaRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(response);
                return ApiResponseDto<List<TestListVlaDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestListVlaDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve TestListVla lookup data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/testlistvla/{itemCode}/{fpsYear} — single record by composite PK
        public async Task<ApiResponseDto<TestListVlaDto>> GetByIdAsync(string itemCode, int fpsYear)
        {
            try
            {
                var url = $"{BaseUrl}/{itemCode}/{fpsYear}";
                var response = await _http.GetAsync<TestListVlaRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestListVlaDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestListVlaDto>>(response);
                return ApiResponseDto<TestListVlaDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestListVlaDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve TestListVla by ID", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST api/v1/testlistvla — create new VLA test record
        //   TestListVlaDto mapped to TestListVlaReq for the request body
        public async Task<ApiResponseDto<TestListVlaDto>> CreateAsync(TestListVlaDto dto)
        {
            try
            {
                var request = _mapper.Map<TestListVlaReq>(dto);
                var response = await _http.PostAsync<TestListVlaReq, TestListVlaRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestListVlaDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestListVlaDto>>(response);
                return ApiResponseDto<TestListVlaDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestListVlaDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create TestListVla", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT api/v1/testlistvla/{itemCode}/{fpsYear} — update VLA test record by composite PK
        //   itemCode and fpsYear placed in path; DTO body carries the full writable payload
        public async Task<ApiResponseDto<TestListVlaDto>> UpdateAsync(string itemCode, int fpsYear, TestListVlaDto dto)
        {
            try
            {
                var request = _mapper.Map<TestListVlaReq>(dto);
                var url = $"{BaseUrl}/{itemCode}/{fpsYear}";
                var response = await _http.PutAsync<TestListVlaReq, TestListVlaRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestListVlaDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestListVlaDto>>(response);
                return ApiResponseDto<TestListVlaDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestListVlaDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update TestListVla", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE api/v1/testlistvla/{itemCode}/{fpsYear} — delete VLA test record by composite PK
        public async Task<ApiResponseDto<bool>> DeleteAsync(string itemCode, int fpsYear)
        {
            try
            {
                var url = $"{BaseUrl}/{itemCode}/{fpsYear}";
                var response = await _http.DeleteAsync<bool?>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete TestListVla", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}

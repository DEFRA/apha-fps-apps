/*
 * TRANSFORMENGINE MIGRATION — FpsTestRCCostApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New HTTP API client created implementing IFpsTestRCCostApiClient
 *   - All HTTP calls routed via IFpsHttpExecutor to backend TestRCCostController
 *   - BaseUrl matches backend route: api/v1/testrccost
 *   - Composite PK (testCode + profitCentre + fpsYear) used in GetByKeyAsync, UpdateAsync, DeleteAsync route segments
 *   - testCode + fpsYear encoded in route segment for GetByTestCodeAsync (list by parent key)
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Mapper used for all success-path response transformations (never manual construction)
 *   - _http and _mapper are private readonly (Sonar S2933)
 *   - InternalCodeError and BaseUrl are private const string (Sonar S1192)
 *   - No paged list endpoint — backend returns flat list for given testCode+fpsYear
 *
 * PRESERVED:
 *   - All 5 interface methods: GetByTestCodeAsync, GetByKeyAsync, CreateAsync, UpdateAsync, DeleteAsync
 *   - Composite PK parameter ordering: testCode, profitCentre, fpsYear (matches backend route template)
 *   - Subform resource family kept separate from TestListVla CRUD resource
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm FpsApiDtoMapper includes TestRCCostRes → TestRCCostDto and
 *     TestRCCostDto → TestRCCostReq mappings before end-to-end testing.
 */

using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    /// <summary>
    /// HTTP API client for component charges per profit centre (TestRCCost).
    /// Targets backend route: GET/POST/PUT/DELETE api/v1/testrccost
    /// Composite PK: TestCode + ProfitCentre + FpsYear.
    /// testCode + fpsYear are required business context from the parent TestListVla row.
    /// </summary>
    public class FpsTestRCCostApiClient : IFpsTestRCCostApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: InternalCodeError as private const — Sonar S1192 compliance
        private const string InternalCodeError = "INTERNAL_ERROR";

        // TRANSFORMENGINE: BaseUrl matches backend TestRCCostController [Route("api/v{version:apiVersion}/testrccost")]
        private const string BaseUrl = "api/v1/testrccost";

        public FpsTestRCCostApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: GET api/v1/testrccost/{testCode}/{fpsYear} — list all charges for a test+year (testCode+fpsYear from parent row)
        public async Task<ApiResponseDto<List<TestRCCostDto>>> GetByTestCodeAsync(string testCode, int fpsYear)
        {
            try
            {
                var url = $"{BaseUrl}/{testCode}/{fpsYear}";
                var response = await _http.GetAsync<List<TestRCCostRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestRCCostDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<TestRCCostDto>>>(response);
                return ApiResponseDto<List<TestRCCostDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestRCCostDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve TestRCCost data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear} — single record by full composite PK
        public async Task<ApiResponseDto<TestRCCostDto>> GetByKeyAsync(string testCode, string profitCentre, int fpsYear)
        {
            try
            {
                var url = $"{BaseUrl}/{testCode}/{profitCentre}/{fpsYear}";
                var response = await _http.GetAsync<TestRCCostRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestRCCostDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestRCCostDto>>(response);
                return ApiResponseDto<TestRCCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestRCCostDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve TestRCCost by key", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST api/v1/testrccost — create new component charge row
        //   TestRCCostDto mapped to TestRCCostReq for the request body
        public async Task<ApiResponseDto<TestRCCostDto>> CreateAsync(TestRCCostDto dto)
        {
            try
            {
                var request = _mapper.Map<TestRCCostReq>(dto);
                var response = await _http.PostAsync<TestRCCostReq, TestRCCostRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestRCCostDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestRCCostDto>>(response);
                return ApiResponseDto<TestRCCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestRCCostDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create TestRCCost", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear} — update component charge by composite PK
        //   All three PK segments placed in path; DTO body carries the full writable payload
        public async Task<ApiResponseDto<TestRCCostDto>> UpdateAsync(string testCode, string profitCentre, int fpsYear, TestRCCostDto dto)
        {
            try
            {
                var request = _mapper.Map<TestRCCostReq>(dto);
                var url = $"{BaseUrl}/{testCode}/{profitCentre}/{fpsYear}";
                var response = await _http.PutAsync<TestRCCostReq, TestRCCostRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestRCCostDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestRCCostDto>>(response);
                return ApiResponseDto<TestRCCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestRCCostDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update TestRCCost", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear} — delete component charge by composite PK
        public async Task<ApiResponseDto<bool>> DeleteAsync(string testCode, string profitCentre, int fpsYear)
        {
            try
            {
                var url = $"{BaseUrl}/{testCode}/{profitCentre}/{fpsYear}";
                var response = await _http.DeleteAsync<bool?>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete TestRCCost", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}

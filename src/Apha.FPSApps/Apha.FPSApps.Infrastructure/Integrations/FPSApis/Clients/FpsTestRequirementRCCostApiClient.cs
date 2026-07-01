/*
 * TRANSFORMENGINE MIGRATION — FpsTestRequirementRCCostApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New HTTP API client created implementing IFpsTestRequirementRCCostApiClient
 *   - All HTTP calls routed via IFpsHttpExecutor to backend TestRequirementRCCostController
 *   - BaseUrl matches backend route: api/v1/testrequirementrccost
 *   - Composite PK (testCode + buyer + profitCentre + fpsYear) used in GetByKeyAsync, UpdateAsync, DeleteAsync route segments
 *   - testCode + fpsYear encoded in route segment for GetByTestCodeAsync (list by parent key)
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Mapper used for all success-path response transformations (never manual construction)
 *   - _http and _mapper are private readonly (Sonar S2933)
 *   - InternalCodeError and BaseUrl are private const string (Sonar S1192)
 *   - No paged list endpoint — backend returns flat list for given testCode+fpsYear
 *
 * PRESERVED:
 *   - All 5 interface methods: GetByTestCodeAsync, GetByKeyAsync, CreateAsync, UpdateAsync, DeleteAsync
 *   - Composite PK parameter ordering: testCode, buyer, profitCentre, fpsYear (matches backend route template)
 *   - Subform resource family kept separate from TestListVla CRUD resource and TestRCCost resource
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm FpsApiDtoMapper includes TestRequirementRCCostRes → TestRequirementRCCostDto
 *     and TestRequirementRCCostDto → TestRequirementRCCostReq mappings before end-to-end testing.
 *   - TRANSFORMENGINE TODO: buyer FK (fps.tlkptestreqmt) and profitCentre FK (fps.tbltestrccost)
 *     validation are service-layer responsibilities — not enforced at this client layer.
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
    /// HTTP API client for project-specific component charges (TestRequirementRCCost).
    /// Targets backend route: GET/POST/PUT/DELETE api/v1/testrequirementrccost
    /// Composite PK: TestCode + Buyer + ProfitCentre + FpsYear.
    /// testCode + fpsYear are required business context from the parent TestListVla row.
    /// buyer is from the test requirement tab row; profitCentre is from the RC cost subform row.
    /// </summary>
    public class FpsTestRequirementRCCostApiClient : IFpsTestRequirementRCCostApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: InternalCodeError as private const — Sonar S1192 compliance
        private const string InternalCodeError = "INTERNAL_ERROR";

        // TRANSFORMENGINE: BaseUrl matches backend TestRequirementRCCostController [Route("api/v{version:apiVersion}/testrequirementrccost")]
        private const string BaseUrl = "api/v1/testrequirementrccost";

        public FpsTestRequirementRCCostApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: GET api/v1/testrequirementrccost/{testCode}/{fpsYear} — list all project charges for test+year (testCode+fpsYear from parent row)
        public async Task<ApiResponseDto<List<TestRequirementRCCostDto>>> GetByTestCodeAsync(string testCode, int fpsYear)
        {
            try
            {
                var url = $"{BaseUrl}/{testCode}/{fpsYear}";
                var response = await _http.GetAsync<List<TestRequirementRCCostRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestRequirementRCCostDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<TestRequirementRCCostDto>>>(response);
                return ApiResponseDto<List<TestRequirementRCCostDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestRequirementRCCostDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve TestRequirementRCCost data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear} — single record by full 4-part composite PK
        public async Task<ApiResponseDto<TestRequirementRCCostDto>> GetByKeyAsync(string testCode, string buyer, string profitCentre, int fpsYear)
        {
            try
            {
                var url = $"{BaseUrl}/{testCode}/{buyer}/{profitCentre}/{fpsYear}";
                var response = await _http.GetAsync<TestRequirementRCCostRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(response);
                return ApiResponseDto<TestRequirementRCCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestRequirementRCCostDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve TestRequirementRCCost by key", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST api/v1/testrequirementrccost — create new project component charge row
        //   TestRequirementRCCostDto mapped to TestRequirementRCCostReq for the request body
        public async Task<ApiResponseDto<TestRequirementRCCostDto>> CreateAsync(TestRequirementRCCostDto dto)
        {
            try
            {
                var request = _mapper.Map<TestRequirementRCCostReq>(dto);
                var response = await _http.PostAsync<TestRequirementRCCostReq, TestRequirementRCCostRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(response);
                return ApiResponseDto<TestRequirementRCCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestRequirementRCCostDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create TestRequirementRCCost", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear} — update project charge by composite PK
        //   All four PK segments placed in path; DTO body carries the full writable payload
        public async Task<ApiResponseDto<TestRequirementRCCostDto>> UpdateAsync(string testCode, string buyer, string profitCentre, int fpsYear, TestRequirementRCCostDto dto)
        {
            try
            {
                var request = _mapper.Map<TestRequirementRCCostReq>(dto);
                var url = $"{BaseUrl}/{testCode}/{buyer}/{profitCentre}/{fpsYear}";
                var response = await _http.PutAsync<TestRequirementRCCostReq, TestRequirementRCCostRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(response);
                return ApiResponseDto<TestRequirementRCCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestRequirementRCCostDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update TestRequirementRCCost", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear} — delete project charge by composite PK
        public async Task<ApiResponseDto<bool>> DeleteAsync(string testCode, string buyer, string profitCentre, int fpsYear)
        {
            try
            {
                var url = $"{BaseUrl}/{testCode}/{buyer}/{profitCentre}/{fpsYear}";
                var response = await _http.DeleteAsync<bool?>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete TestRequirementRCCost", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}

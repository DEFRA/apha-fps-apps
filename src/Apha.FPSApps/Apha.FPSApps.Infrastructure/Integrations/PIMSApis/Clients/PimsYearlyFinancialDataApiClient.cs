/*
 * TRANSFORMENGINE MIGRATION — PimsYearlyFinancialDataApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: typed HTTP API client implementing IPimsYearlyFinancialDataApiClient
 *   - Six HTTP methods bound to backend YearlyFinancialDataController routes confirmed in Phase 5:
 *       GET  api/v1/yearlyfinancialdata/{project}              → GetAllAsync (paginated list)
 *       GET  api/v1/yearlyfinancialdata/{year}/{project}       → GetByKeyAsync (composite key)
 *       POST api/v1/yearlyfinancialdata                        → CreateAsync
 *       PUT  api/v1/yearlyfinancialdata/{year}/{project}       → UpdateAsync (composite key from route)
 *       DELETE api/v1/yearlyfinancialdata/{year}/{project}     → DeleteAsync (composite key)
 *       GET  api/v1/yearlyfinancialdata/{project}/{year}/pactcosts → GetPactCostsAsync
 *   - All HTTP calls wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - GetPactCostsAsync: backend returns IReadOnlyList<PactProjectYearCostsRes>; client takes
 *     first/only item to satisfy the single-object interface contract
 *   - BaseUrl and InternalCodeError extracted as private const string (Sonar S1192)
 *   - _http and _mapper declared private readonly (Sonar S2933)
 *   - Registered on PimsApiClient aggregate (see PimsApiClient.cs)
 *
 * PRESERVED:
 *   - Return types and method signatures exactly match IPimsYearlyFinancialDataApiClient
 *   - Composite key parameter order (year first, project second) consistent with interface
 *   - Mapper used for success response transformation per codebase convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify that GetPactCostsAsync always returns exactly one aggregated
 *     row per (project, year). If the backend can return >1 row (e.g. per-month rows), the
 *     single-object interface contract must be revisited.
 *   - TRANSFORMENGINE TODO: PimsApiDtoMapper requires YearlyFinancialDataRes<->YearlyFinancialDataDto
 *     and PactProjectYearCostsRes->PactProjectYearCostsDto entries — added in same Phase 9 batch.
 */

using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    /// <summary>
    /// HTTP API client for the YearlyFinancialData backend resource.
    /// All calls target <c>api/v1/yearlyfinancialdata</c> via <see cref="IPimsHttpExecutor"/>.
    /// Composite key: (<see cref="short"/> year, <see cref="string"/> project).
    /// </summary>
    public class PimsYearlyFinancialDataApiClient : IPimsYearlyFinancialDataApiClient
    {
        // TRANSFORMENGINE: S2933 — private readonly fields per Sonar rule
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: S1192 — extract repeated string literals to private consts
        private const string InternalCodeError = "INTERNAL_ERROR";
        private const string BaseUrl = "api/v1/yearlyfinancialdata";

        public PimsYearlyFinancialDataApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET api/v1/yearlyfinancialdata/{project} — paginated list filtered by project
        //   Maps to YearlyFinancialDataController.GetAll (backend Phase 5)
        /// <inheritdoc />
        public async Task<ApiResponseDto<List<YearlyFinancialDataDto>>> GetAllAsync(
            string project, QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString($"{BaseUrl}/{project}", query);
                var response = await _http.GetAsync<List<YearlyFinancialDataRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<YearlyFinancialDataDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<YearlyFinancialDataDto>>>(response);
                return ApiResponseDto<List<YearlyFinancialDataDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<YearlyFinancialDataDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve yearly financial data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/yearlyfinancialdata/{year}/{project} — single record by composite key
        //   Maps to YearlyFinancialDataController.GetByKey (backend Phase 5)
        /// <inheritdoc />
        public async Task<ApiResponseDto<YearlyFinancialDataDto>> GetByKeyAsync(short year, string project)
        {
            try
            {
                string url = $"{BaseUrl}/{year}/{project}";
                var response = await _http.GetAsync<YearlyFinancialDataRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(response);
                return ApiResponseDto<YearlyFinancialDataDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<YearlyFinancialDataDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve yearly financial data by key", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST api/v1/yearlyfinancialdata — create new record
        //   Maps to YearlyFinancialDataController.Create (backend Phase 5)
        //   DTO → Req via mapper; Res → Dto on success
        /// <inheritdoc />
        public async Task<ApiResponseDto<YearlyFinancialDataDto>> CreateAsync(YearlyFinancialDataDto dto)
        {
            try
            {
                YearlyFinancialDataReq request = _mapper.Map<YearlyFinancialDataReq>(dto);
                var response = await _http.PostAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(response);
                return ApiResponseDto<YearlyFinancialDataDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<YearlyFinancialDataDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create yearly financial data record", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT api/v1/yearlyfinancialdata/{year}/{project} — update existing record
        //   Composite key from route params; body carries updated field values.
        //   Maps to YearlyFinancialDataController.Update (backend Phase 5)
        /// <inheritdoc />
        public async Task<ApiResponseDto<YearlyFinancialDataDto>> UpdateAsync(
            short year, string project, YearlyFinancialDataDto dto)
        {
            try
            {
                YearlyFinancialDataReq request = _mapper.Map<YearlyFinancialDataReq>(dto);
                string url = $"{BaseUrl}/{year}/{project}";
                var response = await _http.PutAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(response);
                return ApiResponseDto<YearlyFinancialDataDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<YearlyFinancialDataDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update yearly financial data record", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE api/v1/yearlyfinancialdata/{year}/{project} — delete by composite key
        //   Maps to YearlyFinancialDataController.Delete (backend Phase 5)
        /// <inheritdoc />
        public async Task<ApiResponseDto<bool>> DeleteAsync(short year, string project)
        {
            try
            {
                string url = $"{BaseUrl}/{year}/{project}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete yearly financial data record", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/yearlyfinancialdata/{project}/{year}/pactcosts
        //   "Update Costing" button endpoint — reads vpactprojectyearcosts aggregation view.
        //   Backend returns IReadOnlyList<PactProjectYearCostsRes>; interface contract is single
        //   PactProjectYearCostsDto (year-level aggregate assumed to have one row per project+year).
        //   Maps to YearlyFinancialDataController.GetPactCosts (backend Phase 5)
        /// <inheritdoc />
        public async Task<ApiResponseDto<PactProjectYearCostsDto>> GetPactCostsAsync(string project, short year)
        {
            try
            {
                string url = $"{BaseUrl}/{project}/{year}/pactcosts";
                var response = await _http.GetAsync<List<PactProjectYearCostsRes>>(url);
                if (response.Success)
                {
                    // TRANSFORMENGINE: backend returns list; take first aggregate row per interface contract
                    //   If response.Data is empty (no PACT data), return an empty DTO rather than null
                    var firstItem = response.Data?.FirstOrDefault();
                    var costDto = firstItem is not null
                        ? _mapper.Map<PactProjectYearCostsDto>(firstItem)
                        : new PactProjectYearCostsDto();
                    return ApiResponseDto<PactProjectYearCostsDto>.SuccessResponse(costDto);
                }

                var dto = _mapper.Map<ApiResponseDto<PactProjectYearCostsDto>>(response);
                return ApiResponseDto<PactProjectYearCostsDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<PactProjectYearCostsDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve PACT project year costs", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

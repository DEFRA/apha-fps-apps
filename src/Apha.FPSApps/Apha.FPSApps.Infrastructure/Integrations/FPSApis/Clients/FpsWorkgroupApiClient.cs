/*
 * TRANSFORMENGINE MIGRATION — FpsWorkgroupApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - NEW FILE: HTTP API client implementing IFpsWorkgroupApiClient
 *   - Source form: frmMaintWorkGroup2 (RecordSource: WorkGroup_MAP → fps.workgroup)
 *   - 5 CRUD methods bound to WorkgroupController routes under api/v1/workgroup
 *       GetPagedAsync           → GET  api/v1/workgroup/paged
 *       GetByWorkGroupNameAsync → GET  api/v1/workgroup/{workGroupName}
 *       CreateAsync             → POST api/v1/workgroup
 *       UpdateAsync             → PUT  api/v1/workgroup/{workGroupName}
 *       DeleteAsync             → DELETE api/v1/workgroup/{workGroupName}
 *   - 3 lookup methods bound to dedicated endpoints (SEPARATE from CRUD resource family):
 *       GetProfitCentresAsync   → GET api/v1/workgroup/profitcentres
 *       GetOwnersAsync          → GET api/v1/workgroup/owners  (ManagerRes → ManagerDto via IMapper)
 *       GetCostCentresAsync     → GET api/v1/workgroup/costcentres?profitCentre={pc}
 *   - WorkgroupMaintenanceDto ↔ WorkgroupMaintenanceReq / WorkgroupMaintenanceRes via IMapper
 *   - ManagerRes → ManagerDto mapped via IMapper for owners lookup
 *   - profitCentre query parameter appended directly to URL for cascading cost centre lookup
 *   - Every HTTP call wrapped in try/catch(Exception) with FailureResponse fallback
 *
 * PRESERVED:
 *   - All 8 interface method signatures match IFpsWorkgroupApiClient exactly
 *   - Error-handling pattern consistent with FpsGradeApiClient and all Phase 9 clients
 *   - _http and _mapper are private readonly (Sonar S2933)
 *   - InternalCodeError and BaseUrl are private const string (Sonar S1192)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: GetCostCentresAsync returns List<double?> — if the frontend requires a
 *     labelled projection (value + display text), coordinate with backend to update the response type
 *   - TRANSFORMENGINE TODO: Verify backend WorkgroupController [Route] attribute is exactly
 *     "api/v{version:apiVersion}/workgroup" (lowercase) — correct BaseUrl if build fails
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
    public class FpsWorkgroupApiClient : IFpsWorkgroupApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: InternalCodeError as private const — Sonar S1192 compliance
        private const string InternalCodeError = "INTERNAL_ERROR";

        // TRANSFORMENGINE: BaseUrl matches backend WorkgroupController [Route("api/v{version:apiVersion}/workgroup")]
        private const string BaseUrl = "api/v1/workgroup";

        public FpsWorkgroupApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // ── CRUD methods ─────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: GET api/v1/workgroup/paged → WorkgroupController.GetAllPagedAsync
        //   QueryParameters<string> appended as query string via QueryStringHelper.AddQueryString
        public async Task<ApiResponseDto<List<WorkgroupMaintenanceDto>>> GetPagedAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString($"{BaseUrl}/paged", query);
                var response = await _http.GetAsync<List<WorkgroupMaintenanceRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<WorkgroupMaintenanceDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<WorkgroupMaintenanceDto>>>(response);
                return ApiResponseDto<List<WorkgroupMaintenanceDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<WorkgroupMaintenanceDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkGroup data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/workgroup/{workGroupName} → WorkgroupController.GetByWorkGroupNameAsync
        //   workGroupName sourced from grid row selection / route state (confirmed page-sourced)
        public async Task<ApiResponseDto<WorkgroupMaintenanceDto>> GetByWorkGroupNameAsync(string workGroupName)
        {
            try
            {
                var url = $"{BaseUrl}/{workGroupName}";
                var response = await _http.GetAsync<WorkgroupMaintenanceRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkgroupMaintenanceDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkgroupMaintenanceDto>>(response);
                return ApiResponseDto<WorkgroupMaintenanceDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkgroupMaintenanceDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkGroup by name", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST api/v1/workgroup → WorkgroupController.CreateAsync
        //   WorkgroupMaintenanceDto mapped to WorkgroupMaintenanceReq for the request body
        public async Task<ApiResponseDto<WorkgroupMaintenanceDto>> CreateAsync(WorkgroupMaintenanceDto dto)
        {
            try
            {
                var request = _mapper.Map<WorkgroupMaintenanceReq>(dto);
                var response = await _http.PostAsync<WorkgroupMaintenanceReq, WorkgroupMaintenanceRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkgroupMaintenanceDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkgroupMaintenanceDto>>(response);
                return ApiResponseDto<WorkgroupMaintenanceDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkgroupMaintenanceDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create WorkGroup", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT api/v1/workgroup/{workGroupName} → WorkgroupController.UpdateAsync
        //   workGroupName is the ORIGINAL key (before any rename); dto.WorkGroupName may differ (rename support)
        //   workGroupName sourced from grid row selection / route state (confirmed page-sourced)
        public async Task<ApiResponseDto<WorkgroupMaintenanceDto>> UpdateAsync(string workGroupName, WorkgroupMaintenanceDto dto)
        {
            try
            {
                var request = _mapper.Map<WorkgroupMaintenanceReq>(dto);
                var url = $"{BaseUrl}/{workGroupName}";
                var response = await _http.PutAsync<WorkgroupMaintenanceReq, WorkgroupMaintenanceRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkgroupMaintenanceDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkgroupMaintenanceDto>>(response);
                return ApiResponseDto<WorkgroupMaintenanceDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkgroupMaintenanceDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update WorkGroup", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE api/v1/workgroup/{workGroupName} → WorkgroupController.DeleteAsync
        //   workGroupName sourced from grid row selection / route state (confirmed page-sourced)
        public async Task<ApiResponseDto<bool>> DeleteAsync(string workGroupName)
        {
            try
            {
                var url = $"{BaseUrl}/{workGroupName}";
                var response = await _http.DeleteAsync<bool?>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete WorkGroup", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // ── Lookup endpoints (SEPARATE from CRUD resource family) ────────────────────

        // TRANSFORMENGINE: GET api/v1/workgroup/profitcentres → WorkgroupController.GetProfitCentresAsync
        //   Populates ResourceCentre dropdown in the add/edit modal
        public async Task<ApiResponseDto<List<string>>> GetProfitCentresAsync()
        {
            try
            {
                var url = $"{BaseUrl}/profitcentres";
                var response = await _http.GetAsync<List<string>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<string>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<string>>>(response);
                return ApiResponseDto<List<string>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<string>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve ProfitCentres data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/workgroup/owners → WorkgroupController.GetOwnersAsync
        //   Populates Owner dropdown in the add/edit modal (qryManager source → ManagerRes → ManagerDto)
        public async Task<ApiResponseDto<List<ManagerDto>>> GetOwnersAsync()
        {
            try
            {
                var url = $"{BaseUrl}/owners";
                var response = await _http.GetAsync<List<ManagerRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ManagerDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ManagerDto>>>(response);
                return ApiResponseDto<List<ManagerDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ManagerDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Owners data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/workgroup/costcentres?profitCentre={pc} → WorkgroupController.GetCostCentresAsync
        //   Cascading CostCentre dropdown; profitCentre sourced from modal ProfitCentre change event
        //   (VBA Form_Current: Requery CostCentre combo equivalent)
        public async Task<ApiResponseDto<List<double?>>> GetCostCentresAsync(string profitCentre)
        {
            try
            {
                var url = $"{BaseUrl}/costcentres?profitCentre={Uri.EscapeDataString(profitCentre)}";
                var response = await _http.GetAsync<List<double?>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<double?>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<double?>>>(response);
                return ApiResponseDto<List<double?>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<double?>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve CostCentres data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}

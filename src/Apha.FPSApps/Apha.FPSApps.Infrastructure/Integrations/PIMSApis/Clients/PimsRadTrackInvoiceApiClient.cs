// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — PimsRadTrackInvoiceApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: HTTP API client implementation for the frontend to call the backend
 *     RadTrackInvoice REST API at route api/v1/radtrackinvoice.
 *   - Implements IPimsRadTrackInvoiceApiClient (interface created in Phase 7).
 *   - 6 methods wired to the 6 backend controller endpoints confirmed in Phase 5:
 *       GetAllAsync     -> GET    api/v1/radtrackinvoice         (paged + filtered)
 *       GetTotalsAsync  -> GET    api/v1/radtrackinvoice/totals  (aggregate footer row)
 *       GetByIdAsync    -> GET    api/v1/radtrackinvoice/{id}
 *       CreateAsync     -> POST   api/v1/radtrackinvoice
 *       UpdateAsync     -> PUT    api/v1/radtrackinvoice/{id}
 *       DeleteAsync     -> DELETE api/v1/radtrackinvoice/{id}
 *   - Filter params (project, contract, year, program) appended as flat query-string
 *     parameters on GetAllAsync and GetTotalsAsync, following the PimsMilestoneApiClient
 *     pattern (GetLogMilestonesAsync) already established in this codebase.
 *   - All HTTP calls wrapped in try/catch(Exception) returning FailureResponse with
 *     InternalCodeError (Sonar S2139 pattern used across all PIMS API clients).
 *   - BaseUrl extracted as private const string (Sonar S1192); totals sub-path
 *     derived from BaseUrl at call site using $"{BaseUrl}/totals".
 *   - GetTotalsAsync deserializes HTTP response directly into RadTrackInvoiceTotalsDto
 *     because no RadTrackInvoiceTotalsRes contract exists yet (backend DEFERRED note).
 *   - DeleteAsync returns ApiResponseDto<object> matching the backend anonymous
 *     { success: bool } response and the IPimsMilestoneApiClient.DeleteMilestoneAsync pattern.
 *
 * PRESERVED:
 *   - All return types wrapped in ApiResponseDto<T> per frontend convention.
 *   - Nullable optional filter parameters (project, contract, year, program) match
 *     the interface signature defined in Phase 7.
 *   - _http and _mapper are private readonly (Sonar S2933).
 *   - No await inside loops (Sonar S6966).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm query-string format for filter parameters.
 *     Backend uses [FromQuery] RadTrackInvoiceFilter — verify that ASP.NET Core binds
 *     flat params ?project=...&contract=... correctly against the nested filter object.
 *     If nested binding is required (?filter.project=...) adjust the url-building logic
 *     in GetAllAsync and GetTotalsAsync accordingly.
 *   - TRANSFORMENGINE TODO: If RadTrackInvoiceTotalsRes contract is added to
 *     Apha.Common.Contracts.PIMS, replace the direct RadTrackInvoiceTotalsDto HTTP type
 *     in GetTotalsAsync with RadTrackInvoiceTotalsRes and add a PimsApiDtoMapper entry.
 *   - TRANSFORMENGINE TODO: Add RadTrackInvoice endpoint constants to PimsApiEndpoints.cs
 *     (Apha.Common.Constants) to align with the project convention used by other PIMS clients.
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
    public class PimsRadTrackInvoiceApiClient : IPimsRadTrackInvoiceApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: BaseUrl matches the backend controller [Route] attribute exactly.
        // Backend: [Route("api/v{version:apiVersion}/radtrackinvoice")] -> resolved at v1.
        private const string BaseUrl = "api/v1/radtrackinvoice";

        public PimsRadTrackInvoiceApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET list — paged + filtered invoice grid (frmpimsinvoices data grid).
        // QueryParameters carries page/size/sort; filter dimensions appended as flat query-string
        // params following the PimsMilestoneApiClient.GetLogMilestonesAsync pattern.
        public async Task<ApiResponseDto<List<RadTrackInvoiceDto>>> GetAllAsync(
            QueryParameters<string> query,
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(BaseUrl, query);
                if (!string.IsNullOrWhiteSpace(project))
                    url += $"&project={Uri.EscapeDataString(project)}";
                if (!string.IsNullOrWhiteSpace(contract))
                    url += $"&contract={Uri.EscapeDataString(contract)}";
                if (year.HasValue)
                    url += $"&year={year.Value}";
                if (!string.IsNullOrWhiteSpace(program))
                    url += $"&program={Uri.EscapeDataString(program)}";

                var response = await _http.GetAsync<List<RadTrackInvoiceRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(response);
                return ApiResponseDto<List<RadTrackInvoiceDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<RadTrackInvoiceDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve RadTrack invoice data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET totals — aggregate footer row (PlannedAmount, DueAmount, ActualAmount sums).
        // No RadTrackInvoiceTotalsRes contract exists; HTTP response deserialized directly into
        // RadTrackInvoiceTotalsDto. Filter params built as query-string list to avoid separator tracking.
        public async Task<ApiResponseDto<RadTrackInvoiceTotalsDto>> GetTotalsAsync(
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null)
        {
            try
            {
                string url = $"{BaseUrl}/totals";
                var queryParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(project))
                    queryParts.Add($"project={Uri.EscapeDataString(project)}");
                if (!string.IsNullOrWhiteSpace(contract))
                    queryParts.Add($"contract={Uri.EscapeDataString(contract)}");
                if (year.HasValue)
                    queryParts.Add($"year={year.Value}");
                if (!string.IsNullOrWhiteSpace(program))
                    queryParts.Add($"program={Uri.EscapeDataString(program)}");
                if (queryParts.Count > 0)
                    url += "?" + string.Join("&", queryParts);

                var response = await _http.GetAsync<RadTrackInvoiceTotalsDto>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<RadTrackInvoiceTotalsDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<RadTrackInvoiceTotalsDto>>(response);
                return ApiResponseDto<RadTrackInvoiceTotalsDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<RadTrackInvoiceTotalsDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve RadTrack invoice totals", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET by PK — used by Edit and Delete modal open flows in frmpimsinvoices.html.
        // id = InvoiceCounter (integer GENERATED BY DEFAULT AS IDENTITY).
        public async Task<ApiResponseDto<RadTrackInvoiceDto>> GetByIdAsync(int id)
        {
            try
            {
                var response = await _http.GetAsync<RadTrackInvoiceRes>($"{BaseUrl}/{id}");
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(response);
                return ApiResponseDto<RadTrackInvoiceDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<RadTrackInvoiceDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve RadTrack invoice by ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST create — Add Invoice modal save action in frmpimsinvoices.html.
        // Maps RadTrackInvoiceDto -> RadTrackInvoiceReq before sending to backend.
        public async Task<ApiResponseDto<RadTrackInvoiceDto>> CreateAsync(RadTrackInvoiceDto dto)
        {
            try
            {
                RadTrackInvoiceReq request = _mapper.Map<RadTrackInvoiceReq>(dto);
                var response = await _http.PostAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(response);
                return ApiResponseDto<RadTrackInvoiceDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<RadTrackInvoiceDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create RadTrack invoice", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT update — Edit Invoice modal save action. Route id = InvoiceCounter.
        // id parameter passed explicitly to match the backend route {id:int} and enforce route/body consistency.
        public async Task<ApiResponseDto<RadTrackInvoiceDto>> UpdateAsync(int id, RadTrackInvoiceDto dto)
        {
            try
            {
                RadTrackInvoiceReq request = _mapper.Map<RadTrackInvoiceReq>(dto);
                var response = await _http.PutAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>($"{BaseUrl}/{id}", request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(response);
                return ApiResponseDto<RadTrackInvoiceDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<RadTrackInvoiceDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update RadTrack invoice", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE — Delete Invoice confirmation dialog action.
        // Returns ApiResponseDto<object> wrapping the backend anonymous { success: bool } response,
        // matching the IPimsMilestoneApiClient.DeleteMilestoneAsync convention.
        public async Task<ApiResponseDto<object>> DeleteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync<object>($"{BaseUrl}/{id}");
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<object>>(response);

                var dto = _mapper.Map<ApiResponseDto<object>>(response);
                return ApiResponseDto<object>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<object>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete RadTrack invoice", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

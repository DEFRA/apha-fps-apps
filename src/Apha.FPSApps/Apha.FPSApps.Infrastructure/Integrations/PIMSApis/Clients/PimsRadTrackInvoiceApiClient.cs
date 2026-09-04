using Apha.Common.Constants;
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

        public PimsRadTrackInvoiceApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // GET list — filter.* prefix matches backend [FromQuery] QueryParameters<RadTrackInvoiceFilter>
        public async Task<ApiResponseDto<List<RadTrackInvoiceDto>>> GetAllAsync(
            QueryParameters<string> query,
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null)
        {
            string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetAllRadTrackInvoices, query);
            if (!string.IsNullOrWhiteSpace(project))
                url += $"&filter.project={Uri.EscapeDataString(project)}";
            if (!string.IsNullOrWhiteSpace(contract))
                url += $"&filter.contract={Uri.EscapeDataString(contract)}";
            if (year.HasValue)
                url += $"&filter.year={year.Value}";
            if (!string.IsNullOrWhiteSpace(program))
                url += $"&filter.program={Uri.EscapeDataString(program)}";

            var response = await _http.GetAsync<List<RadTrackInvoiceRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<RadTrackInvoiceDto>>>(response);
            return ApiResponseDto<List<RadTrackInvoiceDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        // GET totals — flat query params match backend [FromQuery] RadTrackInvoiceFilter? filter
        public async Task<ApiResponseDto<RadTrackInvoiceTotalsDto>> GetTotalsAsync(
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null,
            string? search = null)
        {
            string url = PimsApiEndpoints.GetRadTrackInvoiceTotals;
            var queryParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(project))
                queryParts.Add($"project={Uri.EscapeDataString(project)}");
            if (!string.IsNullOrWhiteSpace(contract))
                queryParts.Add($"contract={Uri.EscapeDataString(contract)}");
            if (year.HasValue)
                queryParts.Add($"year={year.Value}");
            if (!string.IsNullOrWhiteSpace(program))
                queryParts.Add($"program={Uri.EscapeDataString(program)}");
            if (!string.IsNullOrWhiteSpace(search))
                queryParts.Add($"search={Uri.EscapeDataString(search)}");
            if (queryParts.Count > 0)
                url += "?" + string.Join("&", queryParts);

            var response = await _http.GetAsync<RadTrackInvoiceTotalsDto>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<RadTrackInvoiceTotalsDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<RadTrackInvoiceTotalsDto>>(response);
            return ApiResponseDto<RadTrackInvoiceTotalsDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        // GET by PK
        public async Task<ApiResponseDto<RadTrackInvoiceDto>> GetByIdAsync(int id)
        {
            var response = await _http.GetAsync<RadTrackInvoiceRes>(
                string.Format(PimsApiEndpoints.GetRadTrackInvoiceById, id));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(response);
            return ApiResponseDto<RadTrackInvoiceDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        // POST create
        public async Task<ApiResponseDto<RadTrackInvoiceDto>> CreateAsync(RadTrackInvoiceDto dto)
        {
            RadTrackInvoiceReq request = _mapper.Map<RadTrackInvoiceReq>(dto);
            var response = await _http.PostAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(
                PimsApiEndpoints.CreateRadTrackInvoice, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(response);
            return ApiResponseDto<RadTrackInvoiceDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        // PUT update
        public async Task<ApiResponseDto<RadTrackInvoiceDto>> UpdateAsync(int id, RadTrackInvoiceDto dto)
        {
            RadTrackInvoiceReq request = _mapper.Map<RadTrackInvoiceReq>(dto);
            var response = await _http.PutAsync<RadTrackInvoiceReq, RadTrackInvoiceRes>(
                string.Format(PimsApiEndpoints.UpdateRadTrackInvoice, id), request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<RadTrackInvoiceDto>>(response);
            return ApiResponseDto<RadTrackInvoiceDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        // DELETE
        public async Task<ApiResponseDto<object>> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync<object>(
                string.Format(PimsApiEndpoints.DeleteRadTrackInvoice, id));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<object>>(response);

            var dto = _mapper.Map<ApiResponseDto<object>>(response);
            return ApiResponseDto<object>.FailureResponse(dto.Errors, dto.Meta);
        }

        // Lookups
        public async Task<ApiResponseDto<List<string>>> GetProjectsAsync()
        {
            var response = await _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoiceProjects);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<string>>>(response);
            var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
            return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<int>>> GetYearsAsync()
        {
            var response = await _http.GetAsync<List<int>>(PimsApiEndpoints.GetRadTrackInvoiceYears);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<int>>>(response);
            var dto = _mapper.Map<ApiResponseDto<List<int>>>(response);
            return ApiResponseDto<List<int>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<string>>> GetContractsAsync()
        {
            var response = await _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoiceContracts);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<string>>>(response);
            var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
            return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<string>>> GetProgramsAsync()
        {
            var response = await _http.GetAsync<List<string>>(PimsApiEndpoints.GetRadTrackInvoicePrograms);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<string>>>(response);
            var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
            return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}

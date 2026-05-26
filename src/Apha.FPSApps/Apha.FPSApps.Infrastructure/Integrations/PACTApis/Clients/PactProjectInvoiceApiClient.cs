using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactProjectInvoiceApiClient : IPactProjectInvoiceApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactProjectInvoiceApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectInvoiceDto>>> GetPagedProjectInvoicesAsync(QueryParameters<string> query, string? parentProject)
        {

            string baseUrl = string.IsNullOrWhiteSpace(parentProject)
                ? PactApiEndpoints.GetPagedProjectInvoices
                : string.Format(PactApiEndpoints.GetPagedProjectInvoices, Uri.EscapeDataString(parentProject));

            string url = QueryStringHelper.AddQueryString(baseUrl, query);

            var response = await _http.GetAsync<List<ProjectInvoiceRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(response);
            return ApiResponseDto<List<ProjectInvoiceDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ProjectInvoiceDto>>> GetPagedProjectInvoiceManualAsync(QueryParameters<string> query, string? parentProject)
        {
            string baseUrl = PactApiEndpoints.GetPagedProjectInvoiceManual;

            // Add parentProject as query parameter if provided
            if (!string.IsNullOrWhiteSpace(parentProject))
            {
                baseUrl += $"?parentProject={Uri.EscapeDataString(parentProject)}";
            }

            string url = QueryStringHelper.AddQueryString(baseUrl, query);

            var response = await _http.GetAsync<List<ProjectInvoiceRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(response);
            return ApiResponseDto<List<ProjectInvoiceDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ProjectInvoiceDto>>> GetPagedProjectInvoicesByMonthAsync(QueryParameters<string> query, int? month)
        {
            string baseUrl = PactApiEndpoints.GetPagedProjectInvoicesByMonth;

            // Add month as query parameter if provided
            if (month.HasValue)
            {
                baseUrl += $"?month={month.Value}";
            }

            string url = QueryStringHelper.AddQueryString(baseUrl, query);

            var response = await _http.GetAsync<List<ProjectInvoiceRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectInvoiceDto>>>(response);
            return ApiResponseDto<List<ProjectInvoiceDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectInvoiceDto>> GetByIdAsync(int invoiceCounter)
        {
            var response = await _http.GetAsync<ProjectInvoiceRes>(
                string.Format(PactApiEndpoints.GetProjectInvoiceById, invoiceCounter));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(response);
            return ApiResponseDto<ProjectInvoiceDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectInvoiceDto>> CreateAsync(ProjectInvoiceDto dto)
        {
            ProjectInvoiceReq request = _mapper.Map<ProjectInvoiceReq>(dto);
            
            var response = await _http.PostAsync<ProjectInvoiceReq, ProjectInvoiceRes>(
                PactApiEndpoints.CreateProjectInvoice, request);
            
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(response);
            return ApiResponseDto<ProjectInvoiceDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<ProjectInvoiceDto>> UpdateAsync(int invoiceCounter, ProjectInvoiceDto dto)
        {
            ProjectInvoiceReq request = _mapper.Map<ProjectInvoiceReq>(dto);
            
            var response = await _http.PutAsync<ProjectInvoiceReq, ProjectInvoiceRes>(
                string.Format(PactApiEndpoints.UpdateProjectInvoice, invoiceCounter), request);
            
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProjectInvoiceDto>>(response);
            return ApiResponseDto<ProjectInvoiceDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(int invoiceCounter)
        {
            var response = await _http.DeleteAsync<bool?>(
                string.Format(PactApiEndpoints.DeleteProjectInvoice, invoiceCounter));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<decimal>> GetTotalAmountAsync(string? parentProject)
        {
            if (string.IsNullOrWhiteSpace(parentProject))
                return ApiResponseDto<decimal>.SuccessResponse(0m);

            string url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetProjectInvoiceTotalAmount, new { parentProject });

            var response = await _http.GetAsync<decimal?>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<decimal>>(response);

            var dto = _mapper.Map<ApiResponseDto<decimal>>(response);
            return ApiResponseDto<decimal>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<MonthlyInvoicesPivotDto>> GetMonthlyInvoicesSummaryAsync(QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetMonthlyInvoicesSummary, query);
            var response = await _http.GetAsync<MonthlyInvoicesPivotRes>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(response);            

            var responseDto = _mapper.Map<ApiResponseDto<MonthlyInvoicesPivotDto>>(response);
            return ApiResponseDto<MonthlyInvoicesPivotDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<CopyInvoicesResultDto>> CopyInvoicesAsync(int sourceMonth, int destinationMonth, List<ProjectInvoiceDto>? invoiceRecords = null)
        {
            // Map DTOs to request objects if provided
            List<ProjectInvoiceReq>? invoiceRequests = null;
            if (invoiceRecords != null && invoiceRecords.Count > 0)
            {
                invoiceRequests = invoiceRecords.Select(dto => _mapper.Map<ProjectInvoiceReq>(dto)).ToList();
            }

            var request = new CopyInvoicesReq
            {
                InvoiceRecords = invoiceRequests
            };

            // Convert int months to strings for API endpoint
            string url = $"{PactApiEndpoints.CopyProjectInvoices}?sourceMonth={sourceMonth.ToString()}&destinationMonth={destinationMonth.ToString()}";

            var response = await _http.PostAsync<CopyInvoicesReq, CopyInvoicesRes>(url, request);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<CopyInvoicesResultDto>>(response);
            return ApiResponseDto<CopyInvoicesResultDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}

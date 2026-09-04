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

        public async Task<ApiResponseDto<List<InvoiceImportRowDto>>> GetFailedInvoiceImportAsync(QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetFailedInvoiceImport, query);
            var response = await _http.GetAsync<List<InvoiceImportRowRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<InvoiceImportRowDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<InvoiceImportRowDto>>>(response);
            return ApiResponseDto<List<InvoiceImportRowDto>>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<InvoiceImportRowDto>> GetFailedInvoiceImportByIdAsync(int id)
        {
            string url = PactApiEndpoints.GetFailedInvoiceImportById.Replace("{id}", id.ToString());
            var response = await _http.GetAsync<InvoiceImportRowRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<InvoiceImportRowDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<InvoiceImportRowDto>>(response);
            return ApiResponseDto<InvoiceImportRowDto>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<bool>> SaveFailedInvoiceImportAsync(int id, InvoiceImportRowDto dto)
        {
            var req = _mapper.Map<InvoiceImportRowReq>(dto);
            string url = PactApiEndpoints.SaveFailedInvoiceImport.Replace("{id}", id.ToString());
            var response = await _http.PutAsync<InvoiceImportRowReq, bool?>(url, req);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<bool>> DeleteFailedInvoiceImportByIdAsync(int id)
        {
            string url = PactApiEndpoints.DeleteFailedInvoiceImportById.Replace("{id}", id.ToString());
            var response = await _http.DeleteAsync<bool?>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<bool>> DeleteFailedInvoiceImportByUserAsync()
        {
            var response = await _http.DeleteAsync<bool?>(PactApiEndpoints.DeleteFailedInvoiceImportByUser);
            if (response.Success)
            {
                var mappedResponse = _mapper.Map<ApiResponseDto<bool>>(response);

                // If no records were deleted (Data is false), add a specific message
                if (!mappedResponse.Data)
                {
                    mappedResponse.Errors = new List<ApiErrorDto>
                    {
                        new ApiErrorDto { Message = "No failed imported records found to delete." }
                    };
                }

                return mappedResponse;
            }

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<InvoiceImportResultDto>> ImportInvoiceAsync(InvoiceImportReqDto request)
        {
            var req = _mapper.Map<InvoiceImportReq>(request);
            var response = await _http.PostAsync<InvoiceImportReq, InvoiceImportRes>(PactApiEndpoints.ImportInvoice, req);

            if (response.Success)

            {
                var dto = _mapper.Map<InvoiceImportResultDto>(response.Data);
                return ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(dto);
            }

            var failDto = _mapper.Map<ApiResponseDto<InvoiceImportResultDto>>(response);
            return ApiResponseDto<InvoiceImportResultDto>.FailureResponse(failDto.Errors, failDto.Meta ?? new ApiMetaDto());
        }
    }
}

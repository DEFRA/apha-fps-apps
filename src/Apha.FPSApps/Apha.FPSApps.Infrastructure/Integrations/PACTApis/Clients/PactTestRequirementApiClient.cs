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
    public class PactTestRequirementApiClient : IPactTestRequirementApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PactTestRequirementApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TestRequirementDto>>> GetPagedTestReqmtAsync(
            QueryParameters<string> query, string testCode)
        {
            var baseUrl = string.Format(PactApiEndpoints.GetPagedTestReqmt, Uri.EscapeDataString(testCode));
            var url = QueryStringHelper.AddQueryString(baseUrl, query);
            var response = await _http.GetAsync<List<TestRequirementtRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(response);
            return ApiResponseDto<List<TestRequirementDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<TestRequirementDto>>> GetPagedTestReqmtbyProjectAsync(
           QueryParameters<string> query, string parentProject)
        {

            var baseUrl = string.Format(PactApiEndpoints.GetPagedTestReqmtbyProject, Uri.EscapeDataString(parentProject));
            var url = QueryStringHelper.AddQueryString(baseUrl, query);

            var response = await _http.GetAsync<List<TestRequirementtRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(response);
            return ApiResponseDto<List<TestRequirementDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<TestRequirementDto>>> GetAllTestReqmtForExportAsync(
            string testCode, string? filter)
        {
            var url = string.Format(PactApiEndpoints.GetAllTestReqmtForExport, Uri.EscapeDataString(testCode));
            if (!string.IsNullOrWhiteSpace(filter))
                url += $"?filter={Uri.EscapeDataString(filter)}";

            var response = await _http.GetAsync<List<TestRequirementtRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(response);
            return ApiResponseDto<List<TestRequirementDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<TestRequirementDto>> GetTestReqmtByIdAsync(string testCode, string buyer)
        {
            var url = string.Format(PactApiEndpoints.GetTestReqmtById,
                Uri.EscapeDataString(testCode), Uri.EscapeDataString(buyer));
            var response = await _http.GetAsync<TestRequirementtRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestRequirementDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<TestRequirementDto>>(response);
            return ApiResponseDto<TestRequirementDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<TestRequirementDto>> CreateTestReqmtAsync(TestRequirementDto dto)
        {
            var request = _mapper.Map<TestRequirementReq>(dto);
            var response = await _http.PostAsync<TestRequirementReq, TestRequirementtRes>(
                PactApiEndpoints.CreateTestReqmt, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestRequirementDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<TestRequirementDto>>(response);
            return ApiResponseDto<TestRequirementDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<TestRequirementDto>> UpdateTestReqmtAsync(TestRequirementDto dto)
        {
            var request = _mapper.Map<TestRequirementReq>(dto);
            var response = await _http.PutAsync<TestRequirementReq, TestRequirementtRes>(
                PactApiEndpoints.UpdateTestReqmt, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestRequirementDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<TestRequirementDto>>(response);
            return ApiResponseDto<TestRequirementDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteTestReqmtAsync(string testCode, string buyer)
        {
            var url = string.Format(PactApiEndpoints.DeleteTestReqmt,
                Uri.EscapeDataString(testCode), Uri.EscapeDataString(buyer));
            var response = await _http.DeleteAsync<bool>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<TestRequirementDto>> GetTestReqmtPricingAsync(string testCode, string? projectCode = null)
        {
            var url = $"{PactApiEndpoints.GetTestReqmtPricing}?testCode={Uri.EscapeDataString(testCode)}";
            if (!string.IsNullOrWhiteSpace(projectCode))
                url += $"&projectCode={Uri.EscapeDataString(projectCode)}";

            var response = await _http.GetAsync<TestRequirementtRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestRequirementDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<TestRequirementDto>>(response);
            return ApiResponseDto<TestRequirementDto>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}

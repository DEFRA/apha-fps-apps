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
    public class PactTestCapabilityApiClient : IPactTestCapabilityApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;        

        public PactTestCapabilityApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TestCapabilityDto>>> GetPagedByWorkGroupAsync(
            QueryParameters<string> query, string? workGroup)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedTestCapabilityByWorkGroup, query);
            if (!string.IsNullOrWhiteSpace(workGroup))
                url += $"&workGroup={Uri.EscapeDataString(workGroup)}";

            var response = await _http.GetAsync<List<TestCapabilityRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(response);
            return ApiResponseDto<List<TestCapabilityDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<TestCapabilityDto>>> GetPagedByTestCodeAsync(
            QueryParameters<string> query, string? testCode)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedTestCapabilityByTestCode, query);
            if (!string.IsNullOrWhiteSpace(testCode))
                url += $"&testCode={Uri.EscapeDataString(testCode)}";

            var response = await _http.GetAsync<List<TestCapabilityRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(response);
            return ApiResponseDto<List<TestCapabilityDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<TestCapabilityDto>> GetTestCapabilityByIdAsync(string testCode, string workGroup)
        {
            var url = string.Format(PactApiEndpoints.GetTestCapabilityById,
                Uri.EscapeDataString(testCode), Uri.EscapeDataString(workGroup));
            var response = await _http.GetAsync<TestCapabilityRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestCapabilityDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<TestCapabilityDto>>(response);
            return ApiResponseDto<TestCapabilityDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<TestCapabilityDto>> CreateTestCapabilityAsync(TestCapabilityDto dto)
        {
            var request = _mapper.Map<TestCapabilityReq>(dto);
            var response = await _http.PostAsync<TestCapabilityReq, TestCapabilityRes>(
                PactApiEndpoints.CreateTestCapability, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestCapabilityDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<TestCapabilityDto>>(response);
            return ApiResponseDto<TestCapabilityDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<TestCapabilityDto>> UpdateTestCapabilityAsync(TestCapabilityDto dto)
        {
            var request = _mapper.Map<TestCapabilityReq>(dto);
            var response = await _http.PutAsync<TestCapabilityReq, TestCapabilityRes>(
                PactApiEndpoints.UpdateTestCapability, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestCapabilityDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<TestCapabilityDto>>(response);
            return ApiResponseDto<TestCapabilityDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteTestCapabilityAsync(string testCode, string workGroup)
        {
            var url = string.Format(PactApiEndpoints.DeleteTestCapability,
                Uri.EscapeDataString(testCode), Uri.EscapeDataString(workGroup));
            var response = await _http.DeleteAsync<bool?>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<TestCapabilityDto>>> GetPagedTestCapabilityByPortfolioAsync(
            QueryParameters<string> query, string? portfolio)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedTestCapabilityByPortfolio, query);
            if (!string.IsNullOrWhiteSpace(portfolio))
                url += $"&portfolio={Uri.EscapeDataString(portfolio)}";

            var response = await _http.GetAsync<List<TestCapabilityRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(response);
            return ApiResponseDto<List<TestCapabilityDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<WgTestCapabilitiesWithDescriptionDto>>> GetPagedWgTestCapabilitiesWithDescriptionAsync(QueryParameters<string> query, string workGroup)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedWgTestCapabilitiesWithDescription, query);
            if (!string.IsNullOrWhiteSpace(workGroup))
                url += $"&workGroup={Uri.EscapeDataString(workGroup)}";

            var response = await _http.GetAsync<List<WgTestCapabilitiesWithDescriptionRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<WgTestCapabilitiesWithDescriptionDto>>>(response);

            var failureResponse = _mapper.Map<ApiResponseDto<List<WgTestCapabilitiesWithDescriptionDto>>>(response);
            return ApiResponseDto<List<WgTestCapabilitiesWithDescriptionDto>>.FailureResponse(failureResponse.Errors, failureResponse.Meta);
        }
    }
}

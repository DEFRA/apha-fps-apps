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
    public class PactWorkGroupTestCapabilityApiClient : IPactWorkGroupTestCapabilityApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PactWorkGroupTestCapabilityApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TestCapabilityDto>>> GetPagedByWorkGroupAsync(
            QueryParameters<string> query, string? workGroup)
        {
            try
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
            catch (Exception)
            {
                return ApiResponseDto<List<TestCapabilityDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve test capabilities by work group", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<TestCapabilityDto>>> GetPagedByTestCodeAsync(
            QueryParameters<string> query, string? testCode)
        {
            try
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
            catch (Exception)
            {
                return ApiResponseDto<List<TestCapabilityDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve test capabilities by test code", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TestCapabilityDto>> GetTestCapabilityByIdAsync(string testCode, string workGroup)
        {
            try
            {
                var url = string.Format(PactApiEndpoints.GetTestCapabilityById,
                    Uri.EscapeDataString(testCode), Uri.EscapeDataString(workGroup));
                var response = await _http.GetAsync<TestCapabilityRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestCapabilityDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<TestCapabilityDto>>(response);
                return ApiResponseDto<TestCapabilityDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestCapabilityDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve test capability", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TestCapabilityDto>> CreateTestCapabilityAsync(TestCapabilityDto dto)
        {
            try
            {
                var request = _mapper.Map<TestCapabilityReq>(dto);
                var response = await _http.PostAsync<TestCapabilityReq, TestCapabilityRes>(
                    PactApiEndpoints.CreateTestCapability, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestCapabilityDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestCapabilityDto>>(response);
                return ApiResponseDto<TestCapabilityDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestCapabilityDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create test capability", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TestCapabilityDto>> UpdateTestCapabilityAsync(TestCapabilityDto dto)
        {
            try
            {
                var request = _mapper.Map<TestCapabilityReq>(dto);
                var response = await _http.PutAsync<TestCapabilityReq, TestCapabilityRes>(
                    PactApiEndpoints.UpdateTestCapability, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestCapabilityDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestCapabilityDto>>(response);
                return ApiResponseDto<TestCapabilityDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestCapabilityDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update test capability", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteTestCapabilityAsync(string testCode, string workGroup)
        {
            try
            {
                var url = string.Format(PactApiEndpoints.DeleteTestCapability,
                    Uri.EscapeDataString(testCode), Uri.EscapeDataString(workGroup));
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete test capability", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<TestReqmtDto>>> GetPagedTestReqmtAsync(
            QueryParameters<string> query, string testCode)
        {
            try
            {
                var baseUrl = string.Format(PactApiEndpoints.GetPagedTestReqmt, Uri.EscapeDataString(testCode));
                var url = QueryStringHelper.AddQueryString(baseUrl, query);
                var response = await _http.GetAsync<List<TestReqmtRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestReqmtDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<TestReqmtDto>>>(response);
                return ApiResponseDto<List<TestReqmtDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestReqmtDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve test requirements", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<TestReqmtDto>>> GetAllTestReqmtForExportAsync(
            string testCode, string? filter)
        {
            try
            {
                var url = string.Format(PactApiEndpoints.GetAllTestReqmtForExport, Uri.EscapeDataString(testCode));
                if (!string.IsNullOrWhiteSpace(filter))
                    url += $"?filter={Uri.EscapeDataString(filter)}";

                var response = await _http.GetAsync<List<TestReqmtRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestReqmtDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<TestReqmtDto>>>(response);
                return ApiResponseDto<List<TestReqmtDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestReqmtDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve test requirements for export", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TestReqmtDto>> GetTestReqmtByIdAsync(string testCode, string buyer)
        {
            try
            {
                var url = string.Format(PactApiEndpoints.GetTestReqmtById,
                    Uri.EscapeDataString(testCode), Uri.EscapeDataString(buyer));
                var response = await _http.GetAsync<TestReqmtRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestReqmtDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<TestReqmtDto>>(response);
                return ApiResponseDto<TestReqmtDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestReqmtDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve test requirement", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TestReqmtDto>> CreateTestReqmtAsync(TestReqmtDto dto)
        {
            try
            {
                var request = _mapper.Map<TestReqmtReq>(dto);
                var response = await _http.PostAsync<TestReqmtReq, TestReqmtRes>(
                    PactApiEndpoints.CreateTestReqmt, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestReqmtDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestReqmtDto>>(response);
                return ApiResponseDto<TestReqmtDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestReqmtDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create test requirement", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TestReqmtDto>> UpdateTestReqmtAsync(TestReqmtDto dto)
        {
            try
            {
                var request = _mapper.Map<TestReqmtReq>(dto);
                var response = await _http.PutAsync<TestReqmtReq, TestReqmtRes>(
                    PactApiEndpoints.UpdateTestReqmt, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestReqmtDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestReqmtDto>>(response);
                return ApiResponseDto<TestReqmtDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestReqmtDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update test requirement", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteTestReqmtAsync(string testCode, string buyer)
        {
            try
            {
                var url = string.Format(PactApiEndpoints.DeleteTestReqmt,
                    Uri.EscapeDataString(testCode), Uri.EscapeDataString(buyer));
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete test requirement", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<TestorProductDto>>> GetAllTestorProductsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<TestorProductRes>>(PactApiEndpoints.GetAllTestorProducts);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestorProductDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<TestorProductDto>>>(response);
                return ApiResponseDto<List<TestorProductDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestorProductDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve test or products", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TestReqmtDto>> GetTestReqmtPricingAsync(string testCode, string? projectCode = null)
        {
            try
            {
                var url = $"{PactApiEndpoints.GetTestReqmtPricing}?testCode={Uri.EscapeDataString(testCode)}";
                if (!string.IsNullOrWhiteSpace(projectCode))
                    url += $"&projectCode={Uri.EscapeDataString(projectCode)}";

                var response = await _http.GetAsync<TestReqmtRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestReqmtDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<TestReqmtDto>>(response);
                return ApiResponseDto<TestReqmtDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestReqmtDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve test requirement pricing", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

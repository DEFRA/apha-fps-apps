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
    }
}

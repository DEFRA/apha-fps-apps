using Apha.Common.Constants;
using Apha.Common.Contracts;
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
    public class PactTestListApiClient : IPactTestListApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string BaseRoute = "/api/v1/testlist";
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PactTestListApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TestOrProductDto>>> GetPagedTestOrProductsAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString($"{BaseRoute}/paged", query);
                var response = await _http.GetAsync<List<TestOrProductRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestOrProductDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<TestOrProductDto>>>(response);
                return ApiResponseDto<List<TestOrProductDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestOrProductDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve test/product list", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TestOrProductDto>> GetTestOrProductByIdAsync(string itemCode)
        {
            try
            {
                var response = await _http.GetAsync<TestOrProductRes>($"{BaseRoute}/{Uri.EscapeDataString(itemCode)}");
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestOrProductDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<TestOrProductDto>>(response);
                return ApiResponseDto<TestOrProductDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestOrProductDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve test/product", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TestOrProductDto>> CreateTestOrProductAsync(TestOrProductDto dto)
        {
            try
            {
                var request = _mapper.Map<TestOrProductReq>(dto);
                var response = await _http.PostAsync<TestOrProductReq, TestOrProductRes>(BaseRoute, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestOrProductDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestOrProductDto>>(response);
                return ApiResponseDto<TestOrProductDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestOrProductDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create test/product", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TestOrProductDto>> UpdateTestOrProductAsync(string itemCode, TestOrProductDto dto)
        {
            try
            {
                var request = _mapper.Map<TestOrProductReq>(dto);
                var response = await _http.PutAsync<TestOrProductReq, TestOrProductRes>($"{BaseRoute}/{Uri.EscapeDataString(itemCode)}", request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestOrProductDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestOrProductDto>>(response);
                return ApiResponseDto<TestOrProductDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestOrProductDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update test/product", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteTestOrProductAsync(string itemCode)
        {
            try
            {
                var response = await _http.DeleteAsync<bool>($"{BaseRoute}/{Uri.EscapeDataString(itemCode)}");
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete test/product", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<string>>> GetOwnersAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<string>>($"{BaseRoute}/owners");
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<string>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
                return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<string>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve owners", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}


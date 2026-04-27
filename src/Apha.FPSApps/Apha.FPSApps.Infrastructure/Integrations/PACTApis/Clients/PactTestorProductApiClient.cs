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
    public class PactTestorProductApiClient : IPactTestorProductApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PactTestorProductApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TestorProductDto>>> GetPagedTestOrProductsAsync(QueryParameters<string> query)
        {

            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedTestOrProducts, query);
            var response = await _http.GetAsync<List<TestorProductRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestorProductDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestorProductDto>>>(response);
            return ApiResponseDto<List<TestorProductDto>>.FailureResponse(dto.Errors, dto.Meta);

        }

        public async Task<ApiResponseDto<TestorProductDto>> GetTestOrProductByIdAsync(string itemCode)
        {

            var response = await _http.GetAsync<TestorProductRes>(string.Format(PactApiEndpoints.GetTestOrProductById, Uri.EscapeDataString(itemCode)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestorProductDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<TestorProductDto>>(response);
            return ApiResponseDto<TestorProductDto>.FailureResponse(dto.Errors, dto.Meta);

        }

        public async Task<ApiResponseDto<TestorProductDto>> CreateTestOrProductAsync(TestorProductDto dto)
        {

            var request = _mapper.Map<TestorProductReq>(dto);
            var response = await _http.PostAsync<TestorProductReq, TestorProductRes>(PactApiEndpoints.CreateTestOrProduct, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestorProductDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<TestorProductDto>>(response);
            return ApiResponseDto<TestorProductDto>.FailureResponse(responseDto.Errors, responseDto.Meta);

        }

        public async Task<ApiResponseDto<TestorProductDto>> UpdateTestOrProductAsync(string itemCode, TestorProductDto dto)
        {

            var request = _mapper.Map<TestorProductReq>(dto);
            var response = await _http.PutAsync<TestorProductReq, TestorProductRes>(string.Format(PactApiEndpoints.UpdateTestOrProduct, Uri.EscapeDataString(itemCode)), request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestorProductDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<TestorProductDto>>(response);
            return ApiResponseDto<TestorProductDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteTestOrProductAsync(string itemCode)
        {

            var response = await _http.DeleteAsync<bool>(string.Format(PactApiEndpoints.DeleteTestOrProduct, Uri.EscapeDataString(itemCode)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);

        }

        public async Task<ApiResponseDto<List<string>>> GetOwnersAsync()
        {

            var response = await _http.GetAsync<List<string>>(PactApiEndpoints.GetTestListOwners);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<string>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
            return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);

        }
    }
}


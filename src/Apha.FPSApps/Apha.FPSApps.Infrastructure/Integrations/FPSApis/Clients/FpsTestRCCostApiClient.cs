using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    /// <summary>
    /// HTTP API client for component charges per profit centre (TestRCCost).
    /// Targets backend route: GET/POST/PUT/DELETE api/v1/testrccost
    /// Composite PK: TestCode + ProfitCentre + FpsYear.
    /// testCode + fpsYear are required business context from the parent TestListVla row.
    /// </summary>
    public class FpsTestRCCostApiClient : IFpsTestRCCostApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        private const string InternalCodeError = "INTERNAL_ERROR";

        private const string BaseUrl = "api/v1/testrccost";

        public FpsTestRCCostApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<TestRCCostDto>>> GetByTestCodeAsync(string testCode, int fpsYear)
        {
            try
            {
                var url = $"{BaseUrl}/{testCode}/{fpsYear}";
                var response = await _http.GetAsync<List<TestRCCostRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestRCCostDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<TestRCCostDto>>>(response);
                return ApiResponseDto<List<TestRCCostDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestRCCostDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve TestRCCost data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TestRCCostDto>> GetByKeyAsync(string testCode, string profitCentre, int fpsYear)
        {
            try
            {
                var url = $"{BaseUrl}/{testCode}/{profitCentre}/{fpsYear}";
                var response = await _http.GetAsync<TestRCCostRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestRCCostDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestRCCostDto>>(response);
                return ApiResponseDto<TestRCCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestRCCostDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve TestRCCost by key", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        //   TestRCCostDto mapped to TestRCCostReq for the request body
        public async Task<ApiResponseDto<TestRCCostDto>> CreateAsync(TestRCCostDto dto)
        {
            try
            {
                var request = _mapper.Map<TestRCCostReq>(dto);
                var response = await _http.PostAsync<TestRCCostReq, TestRCCostRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestRCCostDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestRCCostDto>>(response);
                return ApiResponseDto<TestRCCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestRCCostDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create TestRCCost", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        //   All three PK segments placed in path; DTO body carries the full writable payload
        public async Task<ApiResponseDto<TestRCCostDto>> UpdateAsync(string testCode, string profitCentre, int fpsYear, TestRCCostDto dto)
        {
            try
            {
                var request = _mapper.Map<TestRCCostReq>(dto);
                var url = $"{BaseUrl}/{testCode}/{profitCentre}/{fpsYear}";
                var response = await _http.PutAsync<TestRCCostReq, TestRCCostRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestRCCostDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestRCCostDto>>(response);
                return ApiResponseDto<TestRCCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestRCCostDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update TestRCCost", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(string testCode, string profitCentre, int fpsYear)
        {
            try
            {
                var url = $"{BaseUrl}/{testCode}/{profitCentre}/{fpsYear}";
                var response = await _http.DeleteAsync<bool?>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete TestRCCost", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}

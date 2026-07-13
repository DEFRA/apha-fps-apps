using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    /// <summary>
    /// HTTP API client for TestOrProduct VLA list view operations.
    /// Targets backend route: GET api/v1/testlistvla and lookup: GET api/v1/testlistvla/lookup.
    /// </summary>
    public class FpsTestListVlaApiClient : IFpsTestListVlaApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        private const string InternalCodeError = "INTERNAL_ERROR";

        private const string BaseUrl = "api/v1/testlistvla";

        public FpsTestListVlaApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<TestListVlaDto>>> GetAllAsync(QueryParameters<string> query, int fpsYear)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString(BaseUrl, query);
                url = QueryStringHelper.AddQueryString(url, new { fpsYear });

                var response = await _http.GetAsync<List<TestListVlaRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(response);
                return ApiResponseDto<List<TestListVlaDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception ex)
            {
                return ApiResponseDto<List<TestListVlaDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = $"Failed to retrieve TestListVla data: {ex.Message}", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<TestListVlaDto>>> GetAllByYearAsync(int fpsYear)
        {
            try
            {
                var url = $"{BaseUrl}/lookup?fpsYear={fpsYear}";
                var response = await _http.GetAsync<List<TestListVlaRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(response);
                return ApiResponseDto<List<TestListVlaDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestListVlaDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve TestListVla lookup data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TestListVlaDto>> GetByIdAsync(string itemCode, int fpsYear)
        {
            try
            {
                var url = $"{BaseUrl}/{itemCode}/{fpsYear}";
                var response = await _http.GetAsync<TestListVlaRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TestListVlaDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TestListVlaDto>>(response);
                return ApiResponseDto<TestListVlaDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TestListVlaDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve TestListVla by ID", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

    }
}

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

        private const string BaseUrl = "api/v1/testlistvla";
        public FpsTestListVlaApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<TestListVlaDto>>> GetAllAsync(QueryParameters<string> query, int fpsYear)
        {
            var url = QueryStringHelper.AddQueryString(BaseUrl, query);
            url = QueryStringHelper.AddQueryString(url, new { fpsYear });

            var response = await _http.GetAsync<List<TestListVlaRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(response);
            return ApiResponseDto<List<TestListVlaDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<TestListVlaDto>>> GetAllByYearAsync(int fpsYear)
        {
            var url = $"{BaseUrl}/lookup?fpsYear={fpsYear}";
            var response = await _http.GetAsync<List<TestListVlaRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<TestListVlaDto>>>(response);
            return ApiResponseDto<List<TestListVlaDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<TestListVlaDto>> GetByIdAsync(string itemCode, int fpsYear)
        {
            var url = $"{BaseUrl}/{itemCode}/{fpsYear}";
            var response = await _http.GetAsync<TestListVlaRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestListVlaDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<TestListVlaDto>>(response);
            return ApiResponseDto<TestListVlaDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

    }
}

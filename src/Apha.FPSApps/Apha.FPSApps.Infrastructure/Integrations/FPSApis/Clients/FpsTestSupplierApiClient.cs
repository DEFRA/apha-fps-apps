using Apha.Common.Constants;
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
    public class FpsTestSupplierApiClient : IFpsTestSupplierApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsTestSupplierApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TestSupplierViewDto>>> GetPagedTestSupplierAsync(
            QueryParameters<string> query, string testCode, bool showRejected)
        {
            var baseUrl = string.Format(FpsApiEndpoints.GetPagedTestSupplier, testCode, showRejected);
            var url = QueryStringHelper.AddQueryString(baseUrl, query);
            var response = await _http.GetAsync<List<TestSupplierViewRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(response);
            return ApiResponseDto<List<TestSupplierViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}

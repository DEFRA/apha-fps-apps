using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsTestReqBreakdownApiClient : IFpsTestReqBreakdownApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsTestReqBreakdownApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TestReqBreakdownDto>>> GetPlannedTestsByWorkgroupAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedTestReqBreakdown, query);
            var response = await _http.GetAsync<List<TestReqBreakdownRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestReqBreakdownDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestReqBreakdownDto>>>(response);
            return ApiResponseDto<List<TestReqBreakdownDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}

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
    public class PactRecreateAndReleaseSummaryLogApiClient : IPactRecreateAndReleaseSummaryLogApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactRecreateAndReleaseSummaryLogApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>> GetAllRecreateSummariesLogsAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetAllRecreateSummariesLogs, query);
            var response = await _http.GetAsync<List<RecreateSummariesLogRes>>(url);

            if (response.Success)
            {
                var dto = _mapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(response);
                var pagination = response.Pagination;
                var result = new PaginatedResult<RecreateSummaryLogDto>(
                    dto.Data ?? new List<RecreateSummaryLogDto>(),
                    pagination?.TotalRecords ?? 0,
                    pagination?.PageNumber ?? query.Page,
                    pagination?.PageSize ?? query.PageSize);
                return ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>.SuccessResponse(result);
            }

            var failDto = _mapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(response);
            return ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>.FailureResponse(failDto.Errors, failDto.Meta);
        }
    }
}

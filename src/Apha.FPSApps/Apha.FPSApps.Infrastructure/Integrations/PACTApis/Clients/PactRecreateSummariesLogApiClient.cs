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
    public class PactRecreateSummariesLogApiClient : IPactRecreateSummariesLogApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactRecreateSummariesLogApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<PaginatedResult<RecreateSummariesLogDto>>> GetAllRecreateSummariesLogsAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetAllRecreateSummariesLogs, query);
            var response = await _http.GetAsync<List<RecreateSummariesLogRes>>(url);

            if (response.Success)
            {
                var dto = _mapper.Map<ApiResponseDto<List<RecreateSummariesLogDto>>>(response);
                var pagination = response.Pagination;
                var result = new PaginatedResult<RecreateSummariesLogDto>(
                    dto.Data ?? new List<RecreateSummariesLogDto>(),
                    pagination?.TotalRecords ?? 0,
                    pagination?.PageNumber ?? query.Page,
                    pagination?.PageSize ?? query.PageSize);
                return ApiResponseDto<PaginatedResult<RecreateSummariesLogDto>>.SuccessResponse(result);
            }

            var failDto = _mapper.Map<ApiResponseDto<List<RecreateSummariesLogDto>>>(response);
            return ApiResponseDto<PaginatedResult<RecreateSummariesLogDto>>.FailureResponse(failDto.Errors, failDto.Meta);
        }
    }
}

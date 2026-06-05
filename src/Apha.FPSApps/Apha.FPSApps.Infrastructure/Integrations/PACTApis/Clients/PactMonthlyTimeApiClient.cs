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
    public class PactMonthlyTimeApiClient : IPactMonthlyTimeApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactMonthlyTimeApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<MonthlyTimeLogDto>>> SearchAsync(
            QueryParameters<string> query,
            MonthlyTimeLogFilterDto filter)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.SearchMonthlyTimeLog, query);

            if (!string.IsNullOrWhiteSpace(filter.WorkGroup))
                url += $"&workGroup={Uri.EscapeDataString(filter.WorkGroup)}";
            if (!string.IsNullOrWhiteSpace(filter.TimeCode))
                url += $"&timeCode={Uri.EscapeDataString(filter.TimeCode)}";
            if (!string.IsNullOrWhiteSpace(filter.PactStaffId))
                url += $"&pactStaffId={Uri.EscapeDataString(filter.PactStaffId)}";
            if (!string.IsNullOrWhiteSpace(filter.ParentProject))
                url += $"&parentProject={Uri.EscapeDataString(filter.ParentProject)}";
            if (filter.DateImported.HasValue)
                url += $"&dateImported={Uri.EscapeDataString(filter.DateImported.Value.ToString("yyyy-MM-dd"))}";
            if (filter.Month.HasValue)
                url += $"&month={filter.Month.Value}";
            if (!string.IsNullOrWhiteSpace(filter.UserId))
                url += $"&userId={Uri.EscapeDataString(filter.UserId)}";
            if (!string.IsNullOrWhiteSpace(filter.InsertDelete))
                url += $"&insertDelete={Uri.EscapeDataString(filter.InsertDelete)}";

            var response = await _http.GetAsync<List<MonthlyTimeLogRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthlyTimeLogDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MonthlyTimeLogDto>>>(response);
            return ApiResponseDto<List<MonthlyTimeLogDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}

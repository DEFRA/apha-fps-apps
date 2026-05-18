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
    public class PactMonthlyOutputApiClient : IPactMonthlyOutputApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactMonthlyOutputApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<MonthlyOutputLogDto>>> SearchAsync(
            QueryParameters<string> query,
            MonthlyOutputLogFilterDto filter)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.SearchMonthlyOutputLog, query);

            if (!string.IsNullOrWhiteSpace(filter.WorkGroup))
                url += $"&workGroup={Uri.EscapeDataString(filter.WorkGroup)}";
            if (!string.IsNullOrWhiteSpace(filter.TestCode))
                url += $"&testCode={Uri.EscapeDataString(filter.TestCode)}";
            if (!string.IsNullOrWhiteSpace(filter.Buyer))
                url += $"&buyer={Uri.EscapeDataString(filter.Buyer)}";
            if (filter.DateImported.HasValue)
                url += $"&dateImported={Uri.EscapeDataString(filter.DateImported.Value.ToString("yyyy-MM-dd"))}";
            if (filter.Month.HasValue)
                url += $"&month={filter.Month.Value}";
            if (!string.IsNullOrWhiteSpace(filter.UserId))
                url += $"&userId={Uri.EscapeDataString(filter.UserId)}";
            if (!string.IsNullOrWhiteSpace(filter.InsertDelete))
                url += $"&insertDelete={Uri.EscapeDataString(filter.InsertDelete)}";

            var response = await _http.GetAsync<List<MonthlyOutputLogRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthlyOutputLogDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MonthlyOutputLogDto>>>(response);
            return ApiResponseDto<List<MonthlyOutputLogDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}

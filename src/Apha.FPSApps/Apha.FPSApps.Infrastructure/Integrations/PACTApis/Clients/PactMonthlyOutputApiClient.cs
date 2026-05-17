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
            string? workGroup,
            string? testCode,
            string? buyer,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.SearchMonthlyOutputLog, query);

            if (!string.IsNullOrWhiteSpace(workGroup))
                url += $"&workGroup={Uri.EscapeDataString(workGroup)}";
            if (!string.IsNullOrWhiteSpace(testCode))
                url += $"&testCode={Uri.EscapeDataString(testCode)}";
            if (!string.IsNullOrWhiteSpace(buyer))
                url += $"&buyer={Uri.EscapeDataString(buyer)}";
            if (dateImported.HasValue)
                url += $"&dateImported={Uri.EscapeDataString(dateImported.Value.ToString("yyyy-MM-dd"))}";
            if (month.HasValue)
                url += $"&month={month.Value}";
            if (!string.IsNullOrWhiteSpace(userId))
                url += $"&userId={Uri.EscapeDataString(userId)}";
            if (!string.IsNullOrWhiteSpace(insertDelete))
                url += $"&insertDelete={Uri.EscapeDataString(insertDelete)}";

            var response = await _http.GetAsync<List<MonthlyOutputLogRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthlyOutputLogDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MonthlyOutputLogDto>>>(response);
            return ApiResponseDto<List<MonthlyOutputLogDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}

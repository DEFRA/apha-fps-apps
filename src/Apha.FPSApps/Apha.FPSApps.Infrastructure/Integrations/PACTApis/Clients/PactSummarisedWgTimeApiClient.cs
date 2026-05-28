using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;

public class PactSummarisedWgTimeApiClient : IPactSummarisedWgTimeApiClient
{
    private readonly IPactHttpExecutor _http;
    private readonly IMapper _mapper;

    public PactSummarisedWgTimeApiClient(IPactHttpExecutor http, IMapper mapper)
    {
        _http = http;
        _mapper = mapper;
    }

    public async Task<ApiResponseDto<SummarisedWgTimeViewDto>> GetSummarisedWorkgroupTimeSummaryAsync(
        QueryParameters<string> query,
        string? workGroup)
    {
        string baseUrl = PactApiEndpoints.GetPagedSummarisedWorkgroupTime;

        if (!string.IsNullOrWhiteSpace(workGroup))
        {
            baseUrl += $"?workGroup={Uri.EscapeDataString(workGroup)}";
        }

        string url = QueryStringHelper.AddQueryString(baseUrl, query);

        var response = await _http.GetAsync<SummarisedWgTimePivotRes>(url);

        if (response.Success)
        {
            var dto = _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(response);
            return dto;
        }

        var responseDto = _mapper.Map<ApiResponseDto<SummarisedWgTimeViewDto>>(response);
        return ApiResponseDto<SummarisedWgTimeViewDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
    }
}

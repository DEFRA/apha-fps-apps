using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT;

public class SummarisedWgTimeService : ISummarisedWorkgroupTimeService
{
    private readonly IPactApiClient _pactClient;

    public SummarisedWgTimeService(IPactApiClient pactClient)
    {
        _pactClient = pactClient;
    }

    public async Task<ApiResponseDto<SummarisedWgTimeViewDto>> GetSummarisedWorkgroupTimeSummaryAsync(
        QueryParameters<string> query,
        string? workGroup)
    {
        return await _pactClient.PactSummarisedWgTime.GetSummarisedWorkgroupTimeSummaryAsync(query, workGroup);
    }
}

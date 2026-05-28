using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PACT;
public interface ISummarisedWorkgroupTimeService
{
    Task<ApiResponseDto<SummarisedWgTimeViewDto>> GetSummarisedWorkgroupTimeSummaryAsync(
        QueryParameters<string> query,
        string? workGroup);
}

using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces;
public interface ISummarisedWgTimeService
{
    Task<SummarisedWgTimePivotDto> GetSummarisedWorkgroupTimeSummaryAsync(
        QueryParameters<string> query, 
        string? workGroup);
}

using Apha.PACT.Core.Entities;

namespace Apha.PACT.Core.Interfaces;

public interface ISummarisedWgTimeRepository
{
    Task<IEnumerable<SummarisedWgTimeView>> GetSummarisedWorkgroupTimeAsync(
        string? workGroup,
        CancellationToken cancellationToken = default);
}

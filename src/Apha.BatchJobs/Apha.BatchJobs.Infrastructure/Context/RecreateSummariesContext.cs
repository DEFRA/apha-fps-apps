using Apha.BatchJobs.Domain.Interfaces;

namespace Apha.BatchJobs.Infrastructure.Context;

/// <summary>
/// Scoped holder for RecreateSummaries run parameters.
/// </summary>
public sealed class RecreateSummariesContext : IRecreateSummariesContext
{
    /// <inheritdoc />
    public int Month { get; set; }

    /// <inheritdoc />
    public string TriggeredBy { get; set; } = "system";
}

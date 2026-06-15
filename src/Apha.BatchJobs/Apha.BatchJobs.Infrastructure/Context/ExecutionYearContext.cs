using Apha.BatchJobs.Domain.Interfaces;

namespace Apha.BatchJobs.Infrastructure.Context;

/// <summary>
/// Scoped holder for resolved execution year metadata.
/// </summary>
public sealed class ExecutionYearContext : IExecutionYearContext
{
    /// <inheritdoc />
    public int? FpsYear { get; set; }

    /// <inheritdoc />
    public string YearSource { get; set; } = "Unspecified";
}

using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Service-layer entry point for Year End Cutover.
/// Concrete transactional cutover implementation will be added incrementally.
/// </summary>
public sealed class YearEndCutoverService : IYearEndCutoverService
{
    private readonly ILogger<YearEndCutoverService> _logger;

    public YearEndCutoverService(ILogger<YearEndCutoverService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        _logger.LogInformation(
            "YearEndCutover service contract invoked | CorrelationId={CorrelationId} | TargetFpsYear={TargetFpsYear} | CurrentFpsYear={CurrentFpsYear}",
            context.CorrelationId,
            context.TargetFpsYear,
            context.CurrentFpsYear);

        throw new NotImplementedException("YearEndCutover service pipeline will be implemented in the next Year End slice.");
    }
}

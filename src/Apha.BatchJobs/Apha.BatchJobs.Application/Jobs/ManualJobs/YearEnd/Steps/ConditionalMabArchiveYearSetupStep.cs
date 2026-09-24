using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// No-op — MABArchive is out of scope for Year End until separately approved.
/// </summary>
public sealed class ConditionalMabArchiveYearSetupStep : IYearEndDataSetupStep
{
    private readonly ILogger<ConditionalMabArchiveYearSetupStep> _logger;

    public ConditionalMabArchiveYearSetupStep(ILogger<ConditionalMabArchiveYearSetupStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ConditionalMabArchiveYearSetupStep";

    public Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        _logger.LogInformation(
            "YearEnd MABArchive setup skipped (no-op) | CorrelationId={CorrelationId} | Reason=MABArchive is not baseline Year End scope",
            context.CorrelationId);

        return Task.CompletedTask;
    }
}

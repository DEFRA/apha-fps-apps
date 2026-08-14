using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// MABArchive is not baseline Year End scope (governing design decision) — this step is a no-op
/// until MABArchive participation is separately approved as a future, explicit change.
/// </summary>
public sealed class ConditionalMabArchiveYearSetupStep : IYearEndDataSetupStep
{
    private readonly ILogger<ConditionalMabArchiveYearSetupStep> _logger;

    public ConditionalMabArchiveYearSetupStep(ILogger<ConditionalMabArchiveYearSetupStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ConditionalMabArchiveYearSetupStep";

    public Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        _logger.LogInformation(
            "YearEnd MABArchive setup skipped (no-op) | CorrelationId={CorrelationId} | Reason=MABArchive is not baseline Year End scope",
            context.CorrelationId);

        return Task.FromResult(context);
    }
}

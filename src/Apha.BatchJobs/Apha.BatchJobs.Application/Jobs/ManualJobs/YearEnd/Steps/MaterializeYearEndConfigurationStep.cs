using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Materializes the Approve-frozen Year End staging (fps.tblsettings_staging /
/// fps.tlkpmonthhours_staging) into the real fps.tblsettings / fps.tlkpmonthhours target-year rows.
/// Staging is a singleton, not scoped by jobqueueid — the API/UI side guarantees at most one
/// non-terminal Year End DataSetup request exists at a time.
/// </summary>
public sealed class MaterializeYearEndConfigurationStep : IYearEndDataSetupStep
{
    private const string SettingsSchema = "fps";
    private const string SettingsTable = "tblsettings";
    private const string MonthHoursTable = "tlkpmonthhours";

    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<MaterializeYearEndConfigurationStep> _logger;

    public MaterializeYearEndConfigurationStep(
        IYearEndDataSetupRepository repository,
        ILogger<MaterializeYearEndConfigurationStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "MaterializeYearEndConfigurationStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End Data Setup requires targetFpsYear before materializing configuration.");
        }

        if (!Guid.TryParse(context.CorrelationId, out var jobExecutionId))
        {
            throw new InvalidOperationException($"CorrelationId '{context.CorrelationId}' is not a valid JobExecutionId GUID.");
        }

        var jobQueueEntry = await _repository.ResolveJobQueueByExecutionIdAsync(jobExecutionId, cancellationToken);
        if (jobQueueEntry is null)
        {
            throw new InvalidOperationException($"No fps.job_queue row found for JobExecutionId {jobExecutionId}.");
        }

        var (jobQueueId, _) = jobQueueEntry.Value;
        var targetFpsYear = context.TargetFpsYear.Value;

        var existingSettings = await _repository.CountRowsByYearAsync(SettingsSchema, SettingsTable, "fpsyear", targetFpsYear, cancellationToken);
        if (existingSettings > 0)
        {
            throw new InvalidOperationException($"fps.{SettingsTable} already contains {existingSettings} rows for target year {targetFpsYear}. Cleanup is required before Year End configuration materialization.");
        }

        var existingMonthHours = await _repository.CountRowsByYearAsync(SettingsSchema, MonthHoursTable, "fpsyear", targetFpsYear, cancellationToken);
        if (existingMonthHours > 0)
        {
            throw new InvalidOperationException($"fps.{MonthHoursTable} already contains {existingMonthHours} rows for target year {targetFpsYear}. Cleanup is required before Year End configuration materialization.");
        }

        var settingsInserted = await _repository.MaterializeStagedSettingsAsync(targetFpsYear, cancellationToken);
        if (settingsInserted == 0)
        {
            throw new InvalidOperationException("No staged settings found — Approve should have required complete staging before triggering the Worker.");
        }

        var monthHoursInserted = await _repository.MaterializeStagedMonthHoursAsync(targetFpsYear, cancellationToken);
        if (monthHoursInserted == 0)
        {
            throw new InvalidOperationException("No staged month hours found — Approve should have required complete staging before triggering the Worker.");
        }

        _logger.LogInformation(
            "YearEnd configuration materialized | CorrelationId={CorrelationId} | JobQueueId={JobQueueId} | TargetFpsYear={TargetFpsYear} | SettingsInserted={SettingsInserted} | MonthHoursInserted={MonthHoursInserted}",
            context.CorrelationId,
            jobQueueId,
            targetFpsYear,
            settingsInserted,
            monthHoursInserted);
    }
}

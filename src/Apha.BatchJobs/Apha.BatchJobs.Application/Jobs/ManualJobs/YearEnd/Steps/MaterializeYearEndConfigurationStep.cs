using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Materializes the Approve-frozen Year End staging (fps.yearend_settings_staging /
/// fps.yearend_monthhours_staging) into the real fps.tblsettings / fps.tlkpmonthhours target-year rows.
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

        var (jobQueueId, persistedTargetFpsYear) = jobQueueEntry.Value;
        if (!persistedTargetFpsYear.HasValue)
        {
            throw new InvalidOperationException($"fps.job_queue row {jobQueueId} has no target_fpsyear set.");
        }

        if (persistedTargetFpsYear.Value != context.TargetFpsYear.Value)
        {
            throw new InvalidOperationException(
                $"Target year mismatch: fps.job_queue.target_fpsyear={persistedTargetFpsYear.Value} but execution context TargetFpsYear={context.TargetFpsYear.Value}.");
        }

        // job_queue.target_fpsyear is the authoritative source of truth (design decision 6);
        // context.TargetFpsYear is only the derived value that was just cross-checked against it above.
        var targetFpsYear = persistedTargetFpsYear.Value;

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

        var settingsInserted = await _repository.MaterializeStagedSettingsAsync(jobQueueId, targetFpsYear, cancellationToken);
        if (settingsInserted == 0)
        {
            throw new InvalidOperationException($"No staged settings found for jobqueueid {jobQueueId} — Approve should have required complete staging before triggering the Worker.");
        }

        var monthHoursInserted = await _repository.MaterializeStagedMonthHoursAsync(jobQueueId, targetFpsYear, cancellationToken);
        if (monthHoursInserted == 0)
        {
            throw new InvalidOperationException($"No staged month hours found for jobqueueid {jobQueueId} — Approve should have required complete staging before triggering the Worker.");
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

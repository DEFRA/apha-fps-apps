using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Exceptions;

namespace Apha.BatchJobs.Worker.Execution;

/// <summary>
/// Reads BATCH_JOB_* environment variables, validates them, and returns the immutable
/// <see cref="BatchExecutionRequest"/>(s) the runner consumes.
/// </summary>
public sealed class BatchExecutionRequestResolver
{
    private readonly IReadOnlyList<string> _monthlyScheduledNotificationJobs;

    public BatchExecutionRequestResolver()
        : this(MonthlyScheduledNotificationJobs.JobNames)
    {
    }

    /// <summary>Test seam — lets tests exercise fan-out with more than the one job currently configured in production.</summary>
    internal BatchExecutionRequestResolver(IReadOnlyList<string> monthlyScheduledNotificationJobs)
    {
        _monthlyScheduledNotificationJobs = monthlyScheduledNotificationJobs;
    }

    /// <summary>
    /// Resolves one worker invocation's environment into the job(s) to run. Normally this is a
    /// single request. When BATCH_JOB_NAME is the shared category trigger
    /// <see cref="MonthlyScheduledNotificationJobs.EventBridgeCategoryName"/>, it expands into one
    /// request per configured job, each with its own self-generated execution ID.
    /// </summary>
    public IReadOnlyList<BatchExecutionRequest> Resolve()
    {
        var rawJobName  = ResolveJobName();
        var requestedBy = ResolveRequestedBy();
        var runMode     = ResolveRunMode();
        var requestedAt = ResolveRequestedAt(runMode);
        var parameters  = ResolveParametersJson();

        if (!string.Equals(rawJobName, MonthlyScheduledNotificationJobs.EventBridgeCategoryName, StringComparison.OrdinalIgnoreCase))
        {
            return
            [
                new BatchExecutionRequest(
                    rawJobName,
                    runMode,
                    ResolveJobExecutionId(rawJobName, runMode),
                    requestedBy,
                    requestedAt?.UtcDateTime,
                    parameters)
            ];
        }

        if (runMode != RunMode.Scheduled)
            throw new JobValidationException(
                $"BATCH_JOB_NAME '{rawJobName}' is a shared category trigger and requires BATCH_RUN_MODE=Scheduled.");

        if (_monthlyScheduledNotificationJobs.Count == 0)
            throw new JobValidationException(
                $"BATCH_JOB_NAME '{rawJobName}' is a shared category trigger but has no jobs configured.");

        return _monthlyScheduledNotificationJobs
            .Select(jobName => new BatchExecutionRequest(
                jobName,
                runMode,
                ResolveJobExecutionId(jobName, runMode, publishExecutionId: false),
                requestedBy,
                requestedAt?.UtcDateTime,
                parameters))
            .ToArray();
    }

    private static string ResolveJobName()
    {
        var jobName = Environment.GetEnvironmentVariable("BATCH_JOB_NAME");
        if (string.IsNullOrWhiteSpace(jobName))
            throw new JobValidationException(
                "BATCH_JOB_NAME is not set. Verify the EventBridge input transformer maps $.detail.jobName → BATCH_JOB_NAME.");
        if (LooksLikeTemplatePlaceholder(jobName))
            throw new JobValidationException(
                $"BATCH_JOB_NAME resolved to template placeholder '{jobName}'. Provide a real registered job name.");
        return jobName;
    }

    private static string ResolveRequestedBy()
    {
        var requestedBy = Environment.GetEnvironmentVariable("BATCH_REQUESTED_BY") ?? "system";
        if (LooksLikeTemplatePlaceholder(requestedBy))
            throw new JobValidationException(
                $"BATCH_REQUESTED_BY resolved to template placeholder '{requestedBy}'. Provide a real requester identity.");
        return requestedBy;
    }

    private static RunMode ResolveRunMode()
    {
        var raw = Environment.GetEnvironmentVariable("BATCH_RUN_MODE") ?? "Manual";
        if (!Enum.TryParse<RunMode>(raw, ignoreCase: true, out var runMode))
            throw new JobValidationException(
                $"BATCH_RUN_MODE value '{raw}' is not valid. Expected: Scheduled or Manual.");
        return runMode;
    }

    private Guid ResolveJobExecutionId(string jobName, RunMode runMode, bool publishExecutionId = true)
    {
        var raw =
            Environment.GetEnvironmentVariable("BATCH_JOB_EXECUTION_ID")
            ?? Environment.GetEnvironmentVariable("BATCH_EXECUTION_ID");

        if (!string.IsNullOrWhiteSpace(raw))
        {
            if (!Guid.TryParse(raw, out var parsed))
                throw new JobValidationException($"BATCH_JOB_EXECUTION_ID '{raw}' is not a valid GUID.");
            return parsed;
        }

        if (runMode == RunMode.Scheduled && IsWorkerManagedJob(jobName))
        {
            var id = Guid.NewGuid();
            if (publishExecutionId)
            {
                // Publish back so any subsequent read within this process observes the same value.
                // Skipped for category fan-out (see MonthlyScheduledNotificationJobs) — each job
                // there needs its own independently generated ID, not a shared published one.
                Environment.SetEnvironmentVariable("BATCH_JOB_EXECUTION_ID", id.ToString("D"));
            }
            return id;
        }

        throw new JobValidationException("BATCH_JOB_EXECUTION_ID is required for non-worker-managed runs.");
    }

    private static DateTimeOffset? ResolveRequestedAt(RunMode runMode)
    {
        var raw = Environment.GetEnvironmentVariable("BATCH_REQUESTED_AT_UTC");
        if (string.IsNullOrWhiteSpace(raw))
            return runMode == RunMode.Scheduled ? DateTimeOffset.UtcNow : null;

        if (!DateTimeOffset.TryParse(raw,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal |
                System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            throw new JobValidationException(
                $"BATCH_REQUESTED_AT_UTC value '{raw}' is not a valid ISO-8601 timestamp.");
        }

        return parsed;
    }

    private static string? ResolveParametersJson()
    {
        var json = Environment.GetEnvironmentVariable("BATCH_JOB_PARAMETERS_JSON");
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            System.Text.Json.JsonDocument.Parse(json).Dispose();
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new JobValidationException($"BATCH_JOB_PARAMETERS_JSON is not valid JSON: {ex.Message}");
        }

        return json;
    }

    private bool IsWorkerManagedJob(string jobName) =>
        string.Equals(jobName, BatchJobNames.MabArchive, StringComparison.OrdinalIgnoreCase) ||
        _monthlyScheduledNotificationJobs.Any(name => string.Equals(name, jobName, StringComparison.OrdinalIgnoreCase));

    private static bool LooksLikeTemplatePlaceholder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        return trimmed.Length > 2 && trimmed[0] == '<' && trimmed[^1] == '>';
    }
}

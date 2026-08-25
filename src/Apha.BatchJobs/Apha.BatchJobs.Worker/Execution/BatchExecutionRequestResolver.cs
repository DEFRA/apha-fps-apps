using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Exceptions;

namespace Apha.BatchJobs.Worker.Execution;

/// <summary>
/// Reads BATCH_JOB_* environment variables, validates them, and returns the immutable
/// <see cref="BatchExecutionRequest"/> the runner consumes.
/// </summary>
public sealed class BatchExecutionRequestResolver
{
    public BatchExecutionRequest Resolve()
    {
        var jobName     = ResolveJobName();
        var requestedBy = ResolveRequestedBy();
        var runMode     = ResolveRunMode();
        var executionId = ResolveJobExecutionId(jobName, runMode);
        var requestedAt = ResolveRequestedAt(runMode);
        var parameters  = ResolveParametersJson();

        return new BatchExecutionRequest(
            jobName,
            runMode,
            executionId,
            requestedBy,
            requestedAt?.UtcDateTime,
            parameters);
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

    private static Guid ResolveJobExecutionId(string jobName, RunMode runMode)
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
            // Publish back so any subsequent read within this process observes the same value.
            Environment.SetEnvironmentVariable("BATCH_JOB_EXECUTION_ID", id.ToString("D"));
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

    private static bool IsWorkerManagedJob(string jobName) =>
        string.Equals(jobName, BatchJobNames.MabArchive, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(jobName, BatchJobNames.MilestoneUpdateNotifications, StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeTemplatePlaceholder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        return trimmed.Length > 2 && trimmed[0] == '<' && trimmed[^1] == '>';
    }
}

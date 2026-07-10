using System.Text.Json;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;

/// <summary>
/// Immutable execution context for BulkRates worker services.
/// Carries the correlation identity and trigger parameters resolved from environment variables.
/// The worker uses jobexecutionid to load authoritative data from fps.job_queue;
/// event-supplied parameters (year) are only used for early mismatch detection.
/// </summary>
public sealed record BulkRatesExecutionContext(
    string CorrelationId,
    Guid JobExecutionId,
    string JobName,
    int? TriggerYear)
{
    /// <summary>
    /// Builds execution context from worker environment variables.
    /// </summary>
    public static BulkRatesExecutionContext FromEnvironment(string? correlationId)
    {
        var jobExecutionIdEnv = Environment.GetEnvironmentVariable("BATCH_JOB_EXECUTION_ID")
            ?? Environment.GetEnvironmentVariable("BATCH_EXECUTION_ID");

        if (string.IsNullOrWhiteSpace(jobExecutionIdEnv) || !Guid.TryParse(jobExecutionIdEnv, out var jobExecutionId))
        {
            throw new InvalidOperationException(
                "BATCH_JOB_EXECUTION_ID must be a valid GUID for BulkRates jobs. " +
                "It is set by the API before publishing the EventBridge trigger.");
        }

        var jobName = Environment.GetEnvironmentVariable("BATCH_JOB_NAME") ?? string.Empty;
        var parametersJson = Environment.GetEnvironmentVariable("BATCH_JOB_PARAMETERS_JSON");
        var triggerYear = TryReadInt(parametersJson, "year");

        return new BulkRatesExecutionContext(
            string.IsNullOrWhiteSpace(correlationId) ? jobExecutionId.ToString("D") : correlationId,
            jobExecutionId,
            jobName,
            triggerYear);
    }

    private static int? TryReadInt(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty(propertyName, out var value))
                return null;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var n))
                return n;

            if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var s))
                return s;
        }
        catch (JsonException)
        {
            // Non-critical; year from persisted record is authoritative.
        }

        return null;
    }
}

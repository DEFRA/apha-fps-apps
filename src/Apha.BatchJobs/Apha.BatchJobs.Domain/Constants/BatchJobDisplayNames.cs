namespace Apha.BatchJobs.Domain.Constants;

/// <summary>
/// Maps technical <see cref="BatchJobNames"/> constants to the business-readable names Worker
/// emails must use instead (spec: batchjobs-worker-email-notifications-final-spec-2026-09-09.md
/// §4). Covers every currently registered <c>IBatchJob</c> — not just the jobs with their own
/// process-specific notifier — because the generic BatchAlerting notification applies to all of
/// them with no allow-list. An unrecognised job name throws rather than falling back to the raw
/// technical constant, so a future unmapped job is never leaked into a business email; add it
/// here instead.
/// </summary>
public static class BatchJobDisplayNames
{
    private static readonly IReadOnlyDictionary<string, string> Map = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [BatchJobNames.BulkTestRatesUpdate] = "Bulk Test Rates Update",
        [BatchJobNames.BulkStaffRatesUpdate] = "Bulk Staff Rates Update",
        [BatchJobNames.BulkAnimalRatesUpdate] = "Bulk Animal Rates Update",
        [BatchJobNames.MabArchive] = "MABArchive",
        [BatchJobNames.RecreateSummary] = "Recreate Summary",
        [BatchJobNames.YearEndDataSetup] = "Year End DataSetup",
        [BatchJobNames.YearEndCutover] = "Year End CutOver",
        [BatchJobNames.HealthCheck] = "Health Check",
        [BatchJobNames.MilestoneUpdateNotifications] = "Milestone Update Notifications",
    };

    /// <summary>
    /// Returns the business display name for <paramref name="jobName"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="jobName"/> is not a currently registered job. Add it to <see cref="Map"/>
    /// rather than catching this and falling back to the raw name.
    /// </exception>
    public static string GetDisplayName(string jobName)
    {
        if (Map.TryGetValue(jobName, out var displayName))
            return displayName;

        throw new ArgumentOutOfRangeException(
            nameof(jobName),
            jobName,
            $"No business display name is registered for job '{jobName}'.");
    }
}

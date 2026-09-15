using Apha.BatchJobs.Application.FailureHandling;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Tests for <see cref="BatchFailureMessageProvider"/> — the friendly-message mapping shared by
/// <c>BatchRunSummaryWriter</c> (CloudWatch run-summary line) and <c>JobOrchestrator</c>
/// (persisted <c>job_queue.errormessage</c>), so the same failure category always produces the
/// same text in both places.
/// </summary>
public sealed class BatchFailureMessageProviderTests
{
    [Theory]
    [InlineData(BatchFailureCategory.Sql, "Job failed due to a SQL error.")]
    [InlineData(BatchFailureCategory.DependencyOutage, "Job failed due to a dependency outage (database unavailable, network timeout, etc.).")]
    [InlineData(BatchFailureCategory.Configuration, "Job failed due to a configuration or validation error.")]
    [InlineData(BatchFailureCategory.Concurrency, "Job failed because the distributed lock could not be acquired.")]
    [InlineData(BatchFailureCategory.Email, "Job failed due to a business notification email error.")]
    [InlineData(BatchFailureCategory.Timeout, "Job failed because execution exceeded the configured runtime timeout.")]
    [InlineData(BatchFailureCategory.Authorization, "Job failed due to an authorization error.")]
    public void GetHumanReadableMessage_ForKnownCategory_ReturnsExactPreviousText(BatchFailureCategory category, string expected)
    {
        Assert.Equal(expected, BatchFailureMessageProvider.GetHumanReadableMessage(category));
    }

    [Fact]
    public void GetHumanReadableMessage_ForBusinessCategory_ReturnsGenericFallback()
    {
        // Business has no dedicated case, matching the pre-extraction switch's default branch.
        Assert.Equal(
            "Job failed with a business or runtime exception.",
            BatchFailureMessageProvider.GetHumanReadableMessage(BatchFailureCategory.Business));
    }

    [Fact]
    public void GetHumanReadableMessage_ForUnmappedCategoryValue_ReturnsGenericFallback()
    {
        // Simulates a category value added in the future with no case wired up here yet — must
        // degrade to the generic message, never throw or return an empty/technical string.
        var unmapped = (BatchFailureCategory)999;

        Assert.Equal(
            "Job failed with a business or runtime exception.",
            BatchFailureMessageProvider.GetHumanReadableMessage(unmapped));
    }
}

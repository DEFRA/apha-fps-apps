using Apha.BatchJobs.Domain.Constants;

namespace Apha.BatchJobs.UnitTests;

public sealed class BatchJobDisplayNamesTests
{
    [Theory]
    [InlineData(BatchJobNames.BulkTestRatesUpdate, "Bulk Test Rates Update")]
    [InlineData(BatchJobNames.BulkStaffRatesUpdate, "Bulk Staff Rates Update")]
    [InlineData(BatchJobNames.BulkAnimalRatesUpdate, "Bulk Animal Rates Update")]
    [InlineData(BatchJobNames.MabArchive, "MABArchive")]
    [InlineData(BatchJobNames.RecreateSummary, "Recreate Summary")]
    [InlineData(BatchJobNames.YearEndDataSetup, "Year End DataSetup")]
    [InlineData(BatchJobNames.YearEndCutover, "Year End CutOver")]
    [InlineData(BatchJobNames.HealthCheck, "Health Check")]
    [InlineData(BatchJobNames.MilestoneUpdateNotifications, "Milestone Update Notifications")]
    public void GetDisplayName_WhenJobIsRegistered_ShouldReturnBusinessName(string jobName, string expectedDisplayName)
    {
        Assert.Equal(expectedDisplayName, BatchJobDisplayNames.GetDisplayName(jobName));
    }

    [Fact]
    public void GetDisplayName_WhenJobIsNotRegistered_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => BatchJobDisplayNames.GetDisplayName("SomeFutureJob"));

        Assert.Equal("jobName", ex.ParamName);
    }

    [Fact]
    public void GetDisplayName_ShouldNotFallBackToTheLockName()
    {
        // BatchJobNames.YearEndLock ("YearEnd") is a lock name, never a JobName value — it must
        // not accidentally resolve to a display name.
        Assert.Throws<ArgumentOutOfRangeException>(() => BatchJobDisplayNames.GetDisplayName(BatchJobNames.YearEndLock));
    }
}

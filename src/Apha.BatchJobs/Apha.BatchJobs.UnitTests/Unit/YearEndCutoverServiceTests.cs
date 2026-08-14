using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Pure unit coverage for <see cref="YearEndCutoverService"/> — only the guard clauses that fire
/// before the shared transaction opens (target-year presence, latest Data Setup completeness).
/// "Current year" is now derived live from <c>fps.tblyearmaster</c> inside the transaction via
/// <see cref="YearEndYearContextResolver"/>, so target-vs-current ordering and the zero/multiple
/// Open-year cases require a real Postgres connection and are covered by
/// <c>YearEndCutoverServiceIntegrationTests</c> instead.
/// </summary>
public sealed class YearEndCutoverServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenTargetYearMissing_ShouldThrow()
    {
        var service = CreateService();
        var context = new YearEndExecutionContext("corr-2", null, CurrentFpsYear: null, TargetFpsYear: null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));

        Assert.Contains("plannedYear", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoDataSetupExecutionFoundForTargetYear_ShouldThrow()
    {
        var executionRepository = Substitute.For<IJobExecutionRepository>();
        executionRepository
            .GetLastExecutionByFpsYearAsync(BatchJobNames.YearEndDataSetup, 2027, Arg.Any<CancellationToken>())
            .Returns((JobExecutionRecord?)null);

        var service = CreateService(executionRepository);
        var context = new YearEndExecutionContext("corr-4", null, CurrentFpsYear: null, TargetFpsYear: 2027);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));

        Assert.Contains(BatchJobNames.YearEndDataSetup, ex.Message, StringComparison.Ordinal);
        Assert.Contains("Completed", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLatestDataSetupExecutionNotCompleted_ShouldThrow()
    {
        var executionRepository = Substitute.For<IJobExecutionRepository>();
        executionRepository
            .GetLastExecutionByFpsYearAsync(BatchJobNames.YearEndDataSetup, 2027, Arg.Any<CancellationToken>())
            .Returns(new JobExecutionRecord
            {
                ExecutionId = 1,
                JobName = BatchJobNames.YearEndDataSetup,
                JobExecutionId = Guid.NewGuid(),
                JobQueueId = Guid.NewGuid(),
                UserId = "test-user",
                JobType = JobType.Unknown,
                RunMode = RunMode.Manual,
                Status = JobStatus.Failed,
                StartedAt = DateTime.UtcNow,
                FpsYear = 2027
            });

        var service = CreateService(executionRepository);
        var context = new YearEndExecutionContext("corr-5", null, CurrentFpsYear: null, TargetFpsYear: 2027);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));

        Assert.Contains("Failed", ex.Message, StringComparison.Ordinal);
    }

    private static YearEndCutoverService CreateService(IJobExecutionRepository? executionRepository = null)
    {
        var dbContextFactory = Substitute.For<IDbContextFactory<BatchJobsDbContext>>();
        return new YearEndCutoverService(
            dbContextFactory,
            executionRepository ?? Substitute.For<IJobExecutionRepository>(),
            NullLogger<YearEndCutoverService>.Instance);
    }
}

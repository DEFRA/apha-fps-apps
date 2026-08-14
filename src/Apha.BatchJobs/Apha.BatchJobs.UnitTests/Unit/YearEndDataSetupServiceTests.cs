using System.Data.Common;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Pure unit coverage for <see cref="YearEndDataSetupService"/> — only the guard that fires before
/// any database connection is opened. Step ordering, context threading, commit-on-success, and
/// rollback-on-failure all require a real transaction against Postgres now that every step shares
/// one connection/transaction, so those are covered by
/// <c>YearEndDataSetupServiceIntegrationTests</c> instead.
/// </summary>
public sealed class YearEndDataSetupServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenNoStepsRegistered_ShouldThrowBeforeOpeningConnection()
    {
        // The factory is never invoked for this path — CreateDbContext() would need a live
        // connection string, and this guard must fire before any DB access is attempted.
        var dbContextFactory = Substitute.For<IDbContextFactory<BatchJobsDbContext>>();
        var service = new YearEndDataSetupService(dbContextFactory, [], NullLogger<YearEndDataSetupService>.Instance);
        var context = new YearEndExecutionContext("corr-1", null, CurrentFpsYear: 2026, TargetFpsYear: 2027);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));

        Assert.Contains("no registered execution steps", ex.Message, StringComparison.OrdinalIgnoreCase);
        dbContextFactory.DidNotReceive().CreateDbContext();
    }
}

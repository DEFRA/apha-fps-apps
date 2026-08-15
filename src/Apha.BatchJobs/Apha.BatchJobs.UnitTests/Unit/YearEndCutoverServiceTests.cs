using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Pure unit coverage for <see cref="YearEndCutoverService"/> — only the one guard clause that
/// fires before any database interaction at all (target-year/<c>plannedYear</c> presence). Since
/// the 2026-08-15 CutOver design ("frozen design" in
/// <c>fps-year-end-cutover-contract-trace-and-open-questions-2026-08-15.md</c>), every other
/// precondition — current-year resolution, target-Planned, latest Data Setup completeness, staging
/// locks, the post-update assertions — is revalidated live from inside the shared transaction (no
/// longer via an injectable <see cref="Apha.BatchJobs.Domain.Interfaces.IJobExecutionRepository"/>
/// for the Data Setup check), so none of those can be exercised with a mocked
/// <see cref="IDbContextFactory{TContext}"/> — they require a real Postgres connection and are
/// covered by <see cref="YearEndCutoverServiceIntegrationTests"/> instead.
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

    private static YearEndCutoverService CreateService()
    {
        var dbContextFactory = Substitute.For<IDbContextFactory<BatchJobsDbContext>>();
        return new YearEndCutoverService(
            dbContextFactory,
            NullLogger<YearEndCutoverService>.Instance);
    }
}

using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class MabArchiveExecutionContextTests
{
    private readonly IReloadFpsTotalsService _totalsService = Substitute.For<IReloadFpsTotalsService>();
    private readonly IMyFpsYearlyDataService _dataService = Substitute.For<IMyFpsYearlyDataService>();
    private readonly IExecutionYearContext _executionYearContext = Substitute.For<IExecutionYearContext>();
    private readonly IEmailNotificationService _emailNotificationService = Substitute.For<IEmailNotificationService>();

    private MabArchiveLoadOrchestrator CreateSubject()
    {
        return new MabArchiveLoadOrchestrator(
            _totalsService,
            _dataService,
            _executionYearContext,
            _emailNotificationService,
            NullLogger<MabArchiveLoadOrchestrator>.Instance);
    }

    [Fact]
    public void BuildExecutionContext_WhenOverrideMonthGreaterThan4_ShouldUseCurrentYearAsPrimary()
    {
        var originalOverride = Environment.GetEnvironmentVariable("MABARCHIVE_TEST_UTCNOW");
        Environment.SetEnvironmentVariable("MABARCHIVE_TEST_UTCNOW", "2026-05-20T00:00:00Z");

        try
        {
            var subject = CreateSubject();

            var context = subject.BuildExecutionContext();

            Assert.Equal(2026, context.CurrentYear);
            Assert.Equal(2025, context.PreviousYear);
            Assert.Equal(5, context.CurrentMonth);
            Assert.Equal(2026, context.PrimaryYear);
            Assert.False(context.IncludePartialRefreshYear);
            Assert.False(context.RequiresPartialRefresh);
            Assert.Null(context.PartialRefreshYear);
        }
        finally
        {
            Environment.SetEnvironmentVariable("MABARCHIVE_TEST_UTCNOW", originalOverride);
        }
    }

    [Fact]
    public void BuildExecutionContext_WhenOverrideMonthLessOrEqual4_ShouldUsePreviousYearAsPrimary()
    {
        var originalOverride = Environment.GetEnvironmentVariable("MABARCHIVE_TEST_UTCNOW");
        Environment.SetEnvironmentVariable("MABARCHIVE_TEST_UTCNOW", "2026-04-10T00:00:00Z");

        try
        {
            var subject = CreateSubject();

            var context = subject.BuildExecutionContext();

            Assert.Equal(2026, context.CurrentYear);
            Assert.Equal(2025, context.PreviousYear);
            Assert.Equal(4, context.CurrentMonth);
            Assert.Equal(2025, context.PrimaryYear);
            Assert.True(context.IncludePartialRefreshYear);
            Assert.True(context.RequiresPartialRefresh);
            Assert.Equal(2026, context.PartialRefreshYear);
        }
        finally
        {
            Environment.SetEnvironmentVariable("MABARCHIVE_TEST_UTCNOW", originalOverride);
        }
    }

    [Fact]
    public void BuildExecutionContext_WhenOverrideInvalid_ShouldFallbackToRuntimeClock()
    {
        var originalOverride = Environment.GetEnvironmentVariable("MABARCHIVE_TEST_UTCNOW");
        Environment.SetEnvironmentVariable("MABARCHIVE_TEST_UTCNOW", "not-a-valid-utc-datetime");

        try
        {
            var before = DateTime.UtcNow;
            var subject = CreateSubject();

            var context = subject.BuildExecutionContext();
            var after = DateTime.UtcNow;

            var possibleYears = new[] { before.Year, after.Year };
            var possibleMonths = new[] { before.Month, after.Month };

            Assert.Contains(context.CurrentYear, possibleYears);
            Assert.Contains(context.CurrentMonth, possibleMonths);
            Assert.Equal(context.CurrentYear - 1, context.PreviousYear);
            Assert.Equal(context.CurrentMonth <= 4, context.IncludePartialRefreshYear);
            Assert.Equal(context.CurrentMonth <= 4 ? context.PreviousYear : context.CurrentYear, context.PrimaryYear);
        }
        finally
        {
            Environment.SetEnvironmentVariable("MABARCHIVE_TEST_UTCNOW", originalOverride);
        }
    }
}
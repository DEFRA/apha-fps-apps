using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.YearEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed integration tests for <see cref="PeriodSetupStep"/>'s fps.tblperiod copy: the
/// target year must end up with exactly 12 rows, <c>periodlocked</c>/<c>finalsummariesrun</c> reset to
/// 0 regardless of the source year's values, and <c>periodname</c> is regenerated for the target year
/// rather than carried over verbatim.
/// </summary>
/// <remarks>
/// Same shape as <see cref="InactiveEmployeeCleanupStepIntegrationTests"/>: <see cref="YearEndDataSetupRepository"/>
/// opens its own connection per call, so seeded rows are committed up front and always removed again in
/// a finally block. <c>fps.tblperiod</c> has no FK columns, so seeding is a single INSERT per row — no
/// reference chain needed. Each test uses its own disposable far-future year pair.
/// </remarks>
[Trait("Category", "Integration")]
public sealed class PeriodSetupStepIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;

    public PeriodSetupStepIntegrationTests()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? DefaultConnectionString;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using var context = CreateDbContext();
            if (!await context.Database.CanConnectAsync())
            {
                _skipReason = "Integration DB unavailable.";
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task ExecuteAsync_CopiesTwelvePeriods_ResetsLockAndFinalSummariesRun_RegeneratesPeriodName()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int sourceYear = 90640;
        const int targetYear = 90641;
        try
        {
            // fps.tblperiod.fpsyear has a FK to fps.tblyearmaster(fpsyear) — both years need a parent
            // row before any tblperiod row (source-seeded or step-inserted) can reference them.
            await SeedYearMasterAsync(sourceYear, "Open");
            await SeedYearMasterAsync(targetYear, "Planned");

            // Source periods are locked/released and carry the drifted "same year twice" wording seen
            // live (e.g. "2025/25" instead of "2025/26"), so a passing test proves the target year is
            // reset/regenerated rather than just copied forward.
            await SeedTwelveSourcePeriodsAsync(sourceYear, lockedAndReleased: true);

            await RunStepAsync(sourceYear, targetYear);

            var rows = await GetPeriodRowsAsync(targetYear);
            Assert.Equal(12, rows.Count);
            Assert.All(rows, r => Assert.Equal((short)0, r.PeriodLocked));
            Assert.All(rows, r => Assert.Equal((short)0, r.FinalSummariesRun ?? -1));

            var period1 = Assert.Single(rows, r => r.EndPeriod == 1);
            Assert.Equal($"April {targetYear} Only", period1.PeriodName);

            var mayPeriod = Assert.Single(rows, r => r.EndPeriod == 2);
            Assert.Equal($"April - May {targetYear}/{(targetYear + 1) % 100:D2}", mayPeriod.PeriodName);

            var januaryPeriod = Assert.Single(rows, r => r.EndPeriod == 10);
            Assert.Equal($"April {targetYear} - January {targetYear + 1}", januaryPeriod.PeriodName);

            var yearTotal = Assert.Single(rows, r => r.EndPeriod == 12);
            Assert.Equal($"Year Total  {targetYear}/{(targetYear + 1) % 100:D2}", yearTotal.PeriodName);
        }
        finally
        {
            await CleanupYearAsync(sourceYear);
            await CleanupYearAsync(targetYear);
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_SourceYearDoesNotHaveExactlyTwelvePeriods_ThrowsBeforeAnyInsert()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int sourceYear = 90642;
        const int targetYear = 90643;
        try
        {
            // Only the source year needs a parent row: the step throws on the count mismatch before
            // ever attempting to insert into the target year.
            await SeedYearMasterAsync(sourceYear, "Open");

            // Only 3 of the expected 12 periods.
            for (var endPeriod = 1; endPeriod <= 3; endPeriod++)
            {
                await SeedSourcePeriodAsync(sourceYear, endPeriod, $"seed-{sourceYear}-{endPeriod}", periodLocked: 0, finalSummariesRun: 0);
            }

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => RunStepAsync(sourceYear, targetYear));

            Assert.Contains("exactly 12", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(await GetPeriodRowsAsync(targetYear));
        }
        finally
        {
            await CleanupYearAsync(sourceYear);
            await CleanupYearAsync(targetYear);
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_TargetYearAlreadyHasRows_ThrowsWithoutModifyingThem()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int sourceYear = 90644;
        const int targetYear = 90645;
        try
        {
            await SeedYearMasterAsync(sourceYear, "Open");
            await SeedYearMasterAsync(targetYear, "Planned");

            await SeedTwelveSourcePeriodsAsync(sourceYear, lockedAndReleased: false);
            await SeedSourcePeriodAsync(targetYear, endPeriod: 1, "Pre-existing", periodLocked: 0, finalSummariesRun: 0);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => RunStepAsync(sourceYear, targetYear));

            Assert.Contains("already has", ex.Message, StringComparison.OrdinalIgnoreCase);
            var targetRows = await GetPeriodRowsAsync(targetYear);
            var onlyRow = Assert.Single(targetRows);
            Assert.Equal("Pre-existing", onlyRow.PeriodName);
        }
        finally
        {
            await CleanupYearAsync(sourceYear);
            await CleanupYearAsync(targetYear);
        }
    }

    private async Task RunStepAsync(int sourceYear, int targetYear)
    {
        var step = new PeriodSetupStep(
            new YearEndDataSetupRepository(CreateDbContext()),
            NullLogger<PeriodSetupStep>.Instance);

        var context = new YearEndExecutionContext(
            CorrelationId: $"period-setup-it-{sourceYear}-{targetYear}",
            ParametersJson: null,
            CurrentFpsYear: sourceYear,
            TargetFpsYear: targetYear);

        await step.ExecuteAsync(context, CancellationToken.None);
    }

    private bool CanRun() => string.IsNullOrWhiteSpace(_skipReason);

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private async Task SeedTwelveSourcePeriodsAsync(int fpsYear, bool lockedAndReleased)
    {
        for (var endPeriod = 1; endPeriod <= 12; endPeriod++)
        {
            await SeedSourcePeriodAsync(
                fpsYear,
                endPeriod,
                DriftedLegacyStyleName(fpsYear, endPeriod),
                periodLocked: lockedAndReleased ? (short)-1 : (short)0,
                finalSummariesRun: lockedAndReleased ? (short)-1 : (short)0);
        }
    }

    // Deliberately mimics the drifted "same year twice" wording seen live (e.g. "April - May 2025/25"
    // instead of "2025/26") so the regeneration assertions prove the fix corrects it for the target
    // year rather than carrying the source year's stale text forward.
    private static string DriftedLegacyStyleName(int fpsYear, int endPeriod) => endPeriod switch
    {
        1 => $"April {fpsYear} Only",
        12 => $"Year Total  {fpsYear}/{fpsYear % 100:D2}",
        _ => $"April - Month{endPeriod} {fpsYear}/{fpsYear % 100:D2}"
    };

    private async Task SeedSourcePeriodAsync(int fpsYear, int endPeriod, string periodName, short periodLocked, short finalSummariesRun)
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlRawAsync(
            @"INSERT INTO fps.tblperiod (periodname, periodtype, startperiod, endperiod, finalsummariesrun, periodlocked, fpsyear)
              VALUES ({0}, 'Cumulative', 1, {1}, {2}, {3}, {4});",
            periodName, endPeriod, finalSummariesRun, periodLocked, fpsYear);
    }

    /// <summary>
    /// Seeds the fps.tblyearmaster parent row fk_tblperiod_fpsyear requires before any tblperiod row
    /// for that year can be inserted (seeded directly by a test, or inserted by the step itself).
    /// </summary>
    private async Task SeedYearMasterAsync(int fpsYear, string yearStatus)
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlRawAsync(
            @"INSERT INTO fps.tblyearmaster (fpsyear, fpsyearcode, yearstatus, active, createdby)
              VALUES ({0}, {1}, {2}, true, 'PeriodSetupStepIntegrationTests');",
            fpsYear, fpsYear.ToString(), yearStatus);
    }

    private async Task<List<PeriodRow>> GetPeriodRowsAsync(int fpsYear)
    {
        await using var context = CreateDbContext();
        await context.Database.OpenConnectionAsync();
        var connection = context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT periodname, endperiod, periodlocked, finalsummariesrun FROM fps.tblperiod WHERE fpsyear = @year ORDER BY endperiod;";
        var param = command.CreateParameter();
        param.ParameterName = "year";
        param.Value = fpsYear;
        command.Parameters.Add(param);

        var results = new List<PeriodRow>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new PeriodRow(
                reader.GetString(0),
                reader.GetDouble(1),
                reader.GetInt16(2),
                reader.IsDBNull(3) ? null : reader.GetInt16(3)));
        }
        return results;
    }

    private async Task CleanupYearAsync(int fpsYear)
    {
        await using var context = CreateDbContext();
        // Child rows (tblperiod) before the tblyearmaster parent, per fk_tblperiod_fpsyear. Both may be
        // absent depending on how far a given test got before failing/throwing — DELETE is a no-op then.
        await context.Database.ExecuteSqlRawAsync("DELETE FROM fps.tblperiod WHERE fpsyear = {0};", fpsYear);
        await context.Database.ExecuteSqlRawAsync("DELETE FROM fps.tblyearmaster WHERE fpsyear = {0};", fpsYear);
    }

    private sealed record PeriodRow(string PeriodName, double EndPeriod, short PeriodLocked, short? FinalSummariesRun);
}

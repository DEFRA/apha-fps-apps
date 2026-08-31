using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.YearEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed integration tests for <see cref="ValidateTargetYearEmptyTablesStep"/>. Proves the
/// contract: a nonempty target year is a hard failure that leaves the offending row untouched (never a
/// DELETE), a genuinely empty target year passes, and a missing table/year column is a hard failure
/// rather than a silent skip, since <c>ValidateYearScopedSchemaStep</c> is supposed to already
/// guarantee both exist.
/// </summary>
[Trait("Category", "Integration")]
public sealed class ValidateTargetYearEmptyTablesStepIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;

    public ValidateTargetYearEmptyTablesStepIntegrationTests()
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
    public async Task ExecuteAsync_WhenTargetYearHasNoRowsInAnyMustBeEmptyTable_Passes()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90630;
        await RunStepAsync(targetYear);
        // No exception — a fresh, never-used target year has zero rows in all 21 tables.
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenTargetYearHasAnExistingRow_ThrowsAndDoesNotDeleteIt()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90631;
        await SeedRecreateSummariesLogRowAsync(targetYear, id: 1);
        try
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => RunStepAsync(targetYear));

            Assert.Contains("recreatesummaries_log", ex.Message, StringComparison.Ordinal);

            // The strongest proof this port fix requires: the row must still be there afterward.
            // Pre-Phase-7B, this exact scenario would have DELETEd it instead of failing.
            Assert.Equal(1, await CountRecreateSummariesLogRowsAsync(targetYear));
        }
        finally
        {
            await CleanupRecreateSummariesLogAsync(targetYear);
        }
    }

    [SkippableFact]
    public async Task ValidateEntryAsync_WhenTableDoesNotExist_ThrowsRatherThanSkipping()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        var step = CreateStep();
        var entry = new YearEndTableRuleMatrixEntry(
            "fps", "zzz_yearend_phase7b_nonexistent_table", YearEndTableRole.YearScopedTargetMustBeEmpty,
            YearEndTableRuleAction.TargetYearMustBeEmpty, ["id", "fpsyear"]);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => step.ValidateEntryAsync(entry, targetFpsYear: 90632, CancellationToken.None));

        Assert.Contains("zzz_yearend_phase7b_nonexistent_table", ex.Message, StringComparison.Ordinal);
        Assert.Contains("does not exist", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [SkippableFact]
    public async Task ValidateEntryAsync_WhenTableHasNoResolvableYearColumn_ThrowsRatherThanSkipping()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        // fps.tblusers is real (GlobalReference in the matrix) but deliberately has neither fpsyear
        // nor year — exactly the shape ValidateEntryAsync must refuse to silently tolerate here.
        var step = CreateStep();
        var entry = new YearEndTableRuleMatrixEntry(
            "fps", "tblusers", YearEndTableRole.YearScopedTargetMustBeEmpty,
            YearEndTableRuleAction.TargetYearMustBeEmpty, []);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => step.ValidateEntryAsync(entry, targetFpsYear: 90632, CancellationToken.None));

        Assert.Contains("tblusers", ex.Message, StringComparison.Ordinal);
        Assert.Contains("year column", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private ValidateTargetYearEmptyTablesStep CreateStep() =>
        new(new YearEndDataSetupRepository(CreateDbContext()), NullLogger<ValidateTargetYearEmptyTablesStep>.Instance);

    private async Task RunStepAsync(int targetYear)
    {
        var step = CreateStep();
        var context = new YearEndExecutionContext(
            CorrelationId: $"validate-empty-it-{targetYear}",
            ParametersJson: null,
            CurrentFpsYear: null,
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

    private async Task SeedRecreateSummariesLogRowAsync(int fpsYear, int id)
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO fps.recreatesummaries_log (id, fpsyear) VALUES ({id}, {fpsYear});");
    }

    private async Task<long> CountRecreateSummariesLogRowsAsync(int fpsYear)
    {
        await using var context = CreateDbContext();
        return await context.Database
            .SqlQuery<long>($@"SELECT COUNT(*) AS ""Value"" FROM fps.recreatesummaries_log WHERE fpsyear = {fpsYear}")
            .SingleAsync();
    }

    private async Task CleanupRecreateSummariesLogAsync(int fpsYear)
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM fps.recreatesummaries_log WHERE fpsyear = {fpsYear};");
    }
}

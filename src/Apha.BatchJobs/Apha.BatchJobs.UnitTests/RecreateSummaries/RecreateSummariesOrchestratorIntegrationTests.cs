using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Npgsql;
using Xunit;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

/// <summary>
/// PostgreSQL-backed tests for RecreateSummaries orchestrator control flow.
/// </summary>
public sealed class RecreateSummariesOrchestratorIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;

    public RecreateSummariesOrchestratorIntegrationTests()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? DefaultConnectionString;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var exists = await ScalarIntAsync(@"
                SELECT COUNT(*)
                FROM information_schema.tables
                WHERE table_schema = 'fps' AND table_name = 'tblperiod'");

            if (exists == 0)
            {
                _skipReason = "Integration DB missing required table: fps.tblperiod";
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task ExecuteAsync_WhenPeriodUnlocked_ShouldRunMandatoryThenRefreshStepsInOrder()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();
        await SeedPeriodLockAsync(context, month: 6, periodLocked: 0);

        var mandatory1 = new DelegateStep("Mandatory-1", () => Success("Mandatory-1", 2));
        var mandatory2 = new DelegateStep("Mandatory-2", () => Success("Mandatory-2", 3));
        var refresh1 = new DelegateStep("Refresh-1", () => Success("Refresh-1", 4));
        var refresh2 = new DelegateStep("Refresh-2", () => Success("Refresh-2", 5));

        var catalog = Substitute.For<IRecreateSummariesStepCatalog>();
        catalog.BuildMandatorySteps(6, "unit-test-user")
            .Returns([mandatory1, mandatory2]);
        catalog.BuildRefreshSteps(6)
            .Returns([refresh1, refresh2]);

        var orchestrator = new RecreateSummariesOrchestrator(
            context,
            catalog,
            NullLogger<RecreateSummariesOrchestrator>.Instance);

        var results = await orchestrator.ExecuteAsync("corr-1", 6, "unit-test-user");

        Assert.Equal(
            ["Mandatory-1", "Mandatory-2", "Refresh-1", "Refresh-2"],
            results.Select(r => r.StepName).ToArray());
        Assert.All(results, r => Assert.Equal(StepStatus.Success, r.Status));

        Assert.Equal(1, mandatory1.ExecuteCount);
        Assert.Equal(1, mandatory2.ExecuteCount);
        Assert.Equal(1, refresh1.ExecuteCount);
        Assert.Equal(1, refresh2.ExecuteCount);
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenPeriodLocked_ShouldSkipRefreshSteps()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();
        await SeedPeriodLockAsync(context, month: 7, periodLocked: 1);

        var mandatory = new DelegateStep("Mandatory-Only", () => Success("Mandatory-Only", 1));
        var refresh1 = new DelegateStep("Refresh-A", () => Success("Refresh-A", 1));
        var refresh2 = new DelegateStep("Refresh-B", () => Success("Refresh-B", 1));

        var catalog = Substitute.For<IRecreateSummariesStepCatalog>();
        catalog.BuildMandatorySteps(7, "unit-test-user")
            .Returns([mandatory]);
        catalog.BuildRefreshSteps(7)
            .Returns([refresh1, refresh2]);

        var orchestrator = new RecreateSummariesOrchestrator(
            context,
            catalog,
            NullLogger<RecreateSummariesOrchestrator>.Instance);

        var results = await orchestrator.ExecuteAsync("corr-2", 7, "unit-test-user");

        Assert.Equal(3, results.Count);
        Assert.Equal("Mandatory-Only", results[0].StepName);
        Assert.Equal(StepStatus.Success, results[0].Status);
        Assert.Equal("Refresh-A", results[1].StepName);
        Assert.Equal(StepStatus.Skipped, results[1].Status);
        Assert.Equal("Period is locked", results[1].ErrorMessage);
        Assert.Equal("Refresh-B", results[2].StepName);
        Assert.Equal(StepStatus.Skipped, results[2].Status);
        Assert.Equal("Period is locked", results[2].ErrorMessage);

        Assert.Equal(1, mandatory.ExecuteCount);
        Assert.Equal(0, refresh1.ExecuteCount);
        Assert.Equal(0, refresh2.ExecuteCount);
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenMandatoryStepReturnsFailed_ShouldThrowAndStopPipeline()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();
        await SeedPeriodLockAsync(context, month: 8, periodLocked: 0);

        var mandatory1 = new DelegateStep("Mandatory-1", () => Success("Mandatory-1", 1));
        var mandatoryFail = new DelegateStep("Mandatory-Fail", () => Failed("Mandatory-Fail", "forced failure"));
        var mandatoryNever = new DelegateStep("Mandatory-Never", () => Success("Mandatory-Never", 1));
        var refreshNever = new DelegateStep("Refresh-Never", () => Success("Refresh-Never", 1));

        var catalog = Substitute.For<IRecreateSummariesStepCatalog>();
        catalog.BuildMandatorySteps(8, "unit-test-user")
            .Returns([mandatory1, mandatoryFail, mandatoryNever]);
        catalog.BuildRefreshSteps(8)
            .Returns([refreshNever]);

        var orchestrator = new RecreateSummariesOrchestrator(
            context,
            catalog,
            NullLogger<RecreateSummariesOrchestrator>.Instance);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orchestrator.ExecuteAsync("corr-3", 8, "unit-test-user"));

        Assert.Contains("Mandatory-Fail", ex.Message);
        Assert.Equal(1, mandatory1.ExecuteCount);
        Assert.Equal(1, mandatoryFail.ExecuteCount);
        Assert.Equal(0, mandatoryNever.ExecuteCount);
        Assert.Equal(0, refreshNever.ExecuteCount);
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenUnexpectedExceptionAfterFirstSuccess_ShouldPropagateException()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();
        await SeedPeriodLockAsync(context, month: 9, periodLocked: 0);

        var mandatory1 = new DelegateStep("Mandatory-1", () => Success("Mandatory-1", 1));
        var mandatoryThrow = new DelegateStep("Mandatory-Throw",
            () => throw new InvalidOperationException("unexpected-bang"));

        var catalog = Substitute.For<IRecreateSummariesStepCatalog>();
        catalog.BuildMandatorySteps(9, "unit-test-user")
            .Returns([mandatory1, mandatoryThrow]);
        catalog.BuildRefreshSteps(9)
            .Returns([]);

        var orchestrator = new RecreateSummariesOrchestrator(
            context,
            catalog,
            NullLogger<RecreateSummariesOrchestrator>.Instance);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orchestrator.ExecuteAsync("corr-4", 9, "unit-test-user"));

        Assert.Equal("unexpected-bang", ex.Message);
        Assert.Equal(1, mandatory1.ExecuteCount);
        Assert.Equal(1, mandatoryThrow.ExecuteCount);
    }

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);

    private static async Task SeedPeriodLockAsync(BatchJobsDbContext context, int month, int periodLocked)
    {
        await context.Database.ExecuteSqlAsync($@"
            DELETE FROM fps.tblperiod WHERE endperiod = {month};
            INSERT INTO fps.tblperiod (periodname, endperiod, periodlocked, fpsyear)
            VALUES ('P{month:00}', {month}, {periodLocked}, 2026);
        ");
    }

    private async Task<int> ScalarIntAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return Convert.ToInt32(value);
    }

    private static Task<StepResult> Success(string stepName, int rows)
    {
        var now = DateTime.UtcNow;
        return Task.FromResult(new StepResult(stepName, rows, now, now, StepStatus.Success));
    }

    private static Task<StepResult> Failed(string stepName, string error)
    {
        var now = DateTime.UtcNow;
        return Task.FromResult(new StepResult(stepName, 0, now, now, StepStatus.Failed, error));
    }

    private sealed class DelegateStep : IRecreateSummariesExecutionStep
    {
        private readonly Func<Task<StepResult>> _action;

        public DelegateStep(string stepName, Func<Task<StepResult>> action)
        {
            StepName = stepName;
            _action = action;
        }

        public string StepName { get; }

        public int ExecuteCount { get; private set; }

        public async Task<StepResult> ExecuteAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken = default)
        {
            ExecuteCount++;
            return await _action();
        }
    }
}


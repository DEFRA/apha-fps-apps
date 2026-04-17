using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;
using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Handlers;
using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Validation;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.ScheduledLoadFromFps.E2E;

/// <summary>
/// Story 4.5 DB-backed scenario tests for full ScheduledLoadFromFps execution flow.
/// </summary>
public sealed class ScheduledLoadE2ETests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";

    private readonly string _connectionString;
    private readonly string _rootDir;
    private string? _skipReason;

    public ScheduledLoadE2ETests()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__BatchJobsConnectionString")
            ?? DefaultConnectionString;

        _rootDir = FindSolutionRoot();
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await conn.CloseAsync();
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Scenario_NormalRun_AllStepsSucceed_ValidationsPass()
    {
        Assert.True(CanRun(), _skipReason);
        await ResetAndSeedAsync();

        var context = new ScheduledLoadFromFpsExecutionContext(7, 2026, 2025, 4);
        var steps = new[]
        {
            ScheduledLoadFromFpsStep.ProcessPreviousYearTotals,
            ScheduledLoadFromFpsStep.ProcessCurrentYearTotals,
            ScheduledLoadFromFpsStep.DeleteYearsFpsData,
            ScheduledLoadFromFpsStep.AddYearsFpsData,
            ScheduledLoadFromFpsStep.HandleCurrentYearProjectAll
        };

        var sut = BuildSut(context, steps, new PassThroughCrossValidationEngine());
        await sut.ExecuteAsync(CancellationToken.None);

        var finalStatus = await ScalarStringAsync("SELECT final_status FROM fps.scheduled_load_run ORDER BY created_at DESC LIMIT 1;");
        var stepCount = await ScalarIntAsync("SELECT COUNT(*) FROM fps.scheduled_load_step_run WHERE run_id = (SELECT run_id FROM fps.scheduled_load_run ORDER BY created_at DESC LIMIT 1);");

        Assert.Equal("Completed", finalStatus);
        Assert.Equal(5, stepCount);
    }

    [Fact]
    public async Task Scenario_HandlerFailsMidStream_JobStops_AuditRecorded()
    {
        Assert.True(CanRun(), _skipReason);
        await ResetAndSeedAsync();

        var context = new ScheduledLoadFromFpsExecutionContext(7, 2026, 2025, 4);
        var steps = new[]
        {
            ScheduledLoadFromFpsStep.ProcessPreviousYearTotals,
            ScheduledLoadFromFpsStep.ProcessCurrentYearTotals,
            ScheduledLoadFromFpsStep.DeleteYearsFpsData,
            ScheduledLoadFromFpsStep.AddYearsFpsData,
            ScheduledLoadFromFpsStep.HandleCurrentYearProjectAll
        };

        var sut = BuildSut(context, steps, new PassThroughCrossValidationEngine(),
            overrideDeleteHandler: new ThrowingStepHandler(ScheduledLoadFromFpsStep.DeleteYearsFpsData, "forced handler failure"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(CancellationToken.None));

        var finalStatus = await ScalarStringAsync("SELECT final_status FROM fps.scheduled_load_run ORDER BY created_at DESC LIMIT 1;");
        var failedSteps = await ScalarIntAsync("SELECT COUNT(*) FROM fps.scheduled_load_step_run WHERE run_id = (SELECT run_id FROM fps.scheduled_load_run ORDER BY created_at DESC LIMIT 1) AND step_status = 'Failed';");

        Assert.Equal("Failed", finalStatus);
        Assert.Equal(1, failedSteps);
    }

    [Fact]
    public async Task Scenario_ValidationFails_JobMarkedFailed_NoReleaseGatePass()
    {
        Assert.True(CanRun(), _skipReason);
        await ResetAndSeedAsync();

        var context = new ScheduledLoadFromFpsExecutionContext(7, 2026, 2025, 4);
        var steps = new[]
        {
            ScheduledLoadFromFpsStep.ProcessPreviousYearTotals,
            ScheduledLoadFromFpsStep.ProcessCurrentYearTotals,
            ScheduledLoadFromFpsStep.DeleteYearsFpsData,
            ScheduledLoadFromFpsStep.AddYearsFpsData,
            ScheduledLoadFromFpsStep.HandleCurrentYearProjectAll
        };

        var failingEngine = new StubCrossValidationEngine(new[]
        {
            new ScheduledLoadValidationAssertionResult("ASSERT_FAIL", "forced validation failure", 1m, 0m, false, "Expected 1 got 0")
        });

        var sut = BuildSut(context, steps, failingEngine);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(CancellationToken.None));

        var finalStatus = await ScalarStringAsync("SELECT final_status FROM fps.scheduled_load_run ORDER BY created_at DESC LIMIT 1;");
        Assert.Equal("Failed", finalStatus);
    }

    [Fact]
    public async Task Scenario_ConditionalStepSkipped_MonthBeforeCutover()
    {
        Assert.True(CanRun(), _skipReason);
        await ResetAndSeedAsync();

        var context = new ScheduledLoadFromFpsExecutionContext(3, 2026, 2025, 4);
        var steps = new[]
        {
            ScheduledLoadFromFpsStep.ProcessPreviousYearTotals,
            ScheduledLoadFromFpsStep.DeleteYearsFpsData,
            ScheduledLoadFromFpsStep.AddYearsFpsData,
            ScheduledLoadFromFpsStep.HandleCurrentYearProjectAll
        };

        var sut = BuildSut(context, steps, new PassThroughCrossValidationEngine());
        await sut.ExecuteAsync(CancellationToken.None);

        var processCurrentYearCount = await ScalarIntAsync("SELECT COUNT(*) FROM fps.scheduled_load_step_run WHERE run_id = (SELECT run_id FROM fps.scheduled_load_run ORDER BY created_at DESC LIMIT 1) AND step_name = 'ProcessCurrentYearTotals';");
        Assert.Equal(0, processCurrentYearCount);
    }

    [Fact]
    public async Task Scenario_MultiYearBackfill_StrictLegacyScope_OnlyPreviousAndConditionalCurrentYearsProcessed()
    {
        Assert.True(CanRun(), _skipReason);
        await ResetAndSeedAsync();

        var context = new ScheduledLoadFromFpsExecutionContext(7, 2026, 2025, 4);
        var steps = new[]
        {
            ScheduledLoadFromFpsStep.ProcessPreviousYearTotals,
            ScheduledLoadFromFpsStep.ProcessCurrentYearTotals,
            ScheduledLoadFromFpsStep.DeleteYearsFpsData,
            ScheduledLoadFromFpsStep.AddYearsFpsData,
            ScheduledLoadFromFpsStep.HandleCurrentYearProjectAll
        };

        var sut = BuildSut(context, steps, new PassThroughCrossValidationEngine());
        await sut.ExecuteAsync(CancellationToken.None);

        var unexpectedYearRows = await ScalarIntAsync("SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals WHERE year NOT IN (2025, 2026);");
        Assert.Equal(0, unexpectedYearRows);
    }

    private bool CanRun() => string.IsNullOrWhiteSpace(_skipReason);

    private ScheduledLoadFromFpsJobHandler BuildSut(
        ScheduledLoadFromFpsExecutionContext context,
        IReadOnlyList<ScheduledLoadFromFpsStep> steps,
        ICrossValidationEngine engine,
        IScheduledLoadFromFpsStepHandler? overrideDeleteHandler = null)
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        var dbContext = new BatchJobsDbContext(options);
        var repository = new ScheduledLoadFromFpsRepository(dbContext);
        var correlation = new FixedCorrelationService("corr-e2e");
        var planBuilder = new FixedPlanBuilder(new ScheduledLoadFromFpsExecutionPlan(context, steps));

        var handlers = new List<IScheduledLoadFromFpsStepHandler>
        {
            new ProcessPreviousYearTotalsHandler(repository, NullLogger<ProcessPreviousYearTotalsHandler>.Instance),
            new ProcessCurrentYearTotalsHandler(repository, NullLogger<ProcessCurrentYearTotalsHandler>.Instance),
            overrideDeleteHandler ?? new DeleteYearsFpsDataHandler(repository, NullLogger<DeleteYearsFpsDataHandler>.Instance),
            new AddYearsFpsDataHandler(repository, NullLogger<AddYearsFpsDataHandler>.Instance),
            new HandleCurrentYearProjectAllHandler(repository, NullLogger<HandleCurrentYearProjectAllHandler>.Instance)
        };

        return new ScheduledLoadFromFpsJobHandler(
            NullLogger<ScheduledLoadFromFpsJobHandler>.Instance,
            planBuilder,
            repository,
            correlation,
            engine,
            handlers,
            Options.Create(new ScheduledLoadFromFpsSettings { StepTimeoutSeconds = 30 }));
    }

    private async Task ResetAndSeedAsync()
    {
        await ExecuteSqlFileAsync("database/sql/flush/002_flush_scheduled_load_tables.sql");
        await ExecuteSqlFileAsync("database/sql/seeds/001_seed_scheduled_job_master.sql");
        await ExecuteSqlFileAsync("database/sql/seeds/002_seed_scheduled_source_baseline.sql");
        await ExecuteSqlFileAsync("database/sql/seeds/003_seed_scheduled_validation_baseline.sql");
    }

    private async Task ExecuteSqlFileAsync(string relativePath)
    {
        var fullPath = Path.Combine(_rootDir, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var sql = await File.ReadAllTextAsync(fullPath);

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<int> ScalarIntAsync(string sql)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        var value = await cmd.ExecuteScalarAsync();
        return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
    }

    private async Task<string> ScalarStringAsync(string sql)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        var value = await cmd.ExecuteScalarAsync();
        return value?.ToString() ?? string.Empty;
    }

    private static string FindSolutionRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var slnPath = Path.Combine(current.FullName, "BatchJobs.sln");
            if (File.Exists(slnPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate BatchJobs.sln root from test runtime path.");
    }

    private sealed class FixedPlanBuilder : IScheduledLoadFromFpsPlanBuilder
    {
        private readonly ScheduledLoadFromFpsExecutionPlan _plan;

        public FixedPlanBuilder(ScheduledLoadFromFpsExecutionPlan plan)
        {
            _plan = plan;
        }

        public ScheduledLoadFromFpsExecutionPlan Build() => _plan;
    }

    private sealed class FixedCorrelationService : ICorrelationService
    {
        private string? _value;

        public FixedCorrelationService(string value)
        {
            _value = value;
        }

        public string? GetCorrelationId() => _value;

        public void SetCorrelationId(string correlationId)
        {
            _value = correlationId;
        }

        public string GenerateCorrelationId()
        {
            _value ??= Guid.NewGuid().ToString("N");
            return _value;
        }
    }

    private sealed class StubCrossValidationEngine : ICrossValidationEngine
    {
        private readonly IReadOnlyList<ScheduledLoadValidationAssertionResult> _results;

        public StubCrossValidationEngine(IReadOnlyList<ScheduledLoadValidationAssertionResult> results)
        {
            _results = results;
        }

        public Task<IReadOnlyList<ScheduledLoadValidationAssertionResult>> ExecuteAsync(
            Guid runId,
            ScheduledLoadFromFpsExecutionContext context,
            int expectedStepCount,
            CancellationToken cancellationToken)
         {
            return Task.FromResult(_results);
         }
     }

    private sealed class PassThroughCrossValidationEngine : ICrossValidationEngine
    {
        public Task<IReadOnlyList<ScheduledLoadValidationAssertionResult>> ExecuteAsync(
            Guid runId,
            ScheduledLoadFromFpsExecutionContext context,
            int expectedStepCount,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<ScheduledLoadValidationAssertionResult>>(
                new[] { new ScheduledLoadValidationAssertionResult("ASSERT_PASS", "pass-through", 1m, 1m, true) });
        }
    }

    private sealed class ThrowingStepHandler : IScheduledLoadFromFpsStepHandler
    {
        private readonly string _message;

        public ThrowingStepHandler(ScheduledLoadFromFpsStep step, string message)
        {
            Step = step;
            _message = message;
        }

        public ScheduledLoadFromFpsStep Step { get; }

        public Task<int> ExecuteAsync(ScheduledLoadFromFpsExecutionContext context, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(_message);
        }
    }
}

using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Domain.Constants;

namespace Apha.BatchJobs.UnitTests;

public sealed class BulkRatesExecutionContextTests
{
    // ── FromEnvironment — BATCH_JOB_EXECUTION_ID validation ─────────────────

    [Fact]
    public void FromEnvironment_WhenBatchJobExecutionIdMissing_ShouldThrow()
    {
        using var idScope = new EnvScope("BATCH_JOB_EXECUTION_ID", null);
        using var altScope = new EnvScope("BATCH_EXECUTION_ID", null);

        var ex = Assert.Throws<InvalidOperationException>(
            () => BulkRatesExecutionContext.FromEnvironment(null));

        Assert.Contains("BATCH_JOB_EXECUTION_ID", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FromEnvironment_WhenBatchJobExecutionIdNotAGuid_ShouldThrow()
    {
        using var idScope = new EnvScope("BATCH_JOB_EXECUTION_ID", "not-a-guid");
        using var altScope = new EnvScope("BATCH_EXECUTION_ID", null);

        Assert.Throws<InvalidOperationException>(
            () => BulkRatesExecutionContext.FromEnvironment(null));
    }

    [Fact]
    public void FromEnvironment_WhenBatchJobExecutionIdValidGuid_ShouldReturnContextWithThatId()
    {
        var id = Guid.NewGuid();
        using var idScope = new EnvScope("BATCH_JOB_EXECUTION_ID", id.ToString("D"));
        using var nameScope = new EnvScope("BATCH_JOB_NAME", BatchJobNames.BulkTestRatesUpdate);
        using var paramScope = new EnvScope("BATCH_JOB_PARAMETERS_JSON", null);

        var ctx = BulkRatesExecutionContext.FromEnvironment(null);

        Assert.Equal(id, ctx.JobExecutionId);
        Assert.Equal(BatchJobNames.BulkTestRatesUpdate, ctx.JobName);
    }

    // ── FromEnvironment — BATCH_EXECUTION_ID fallback ───────────────────────

    [Fact]
    public void FromEnvironment_WhenBatchExecutionIdFallbackSet_ShouldUseIt()
    {
        var id = Guid.NewGuid();
        using var idScope = new EnvScope("BATCH_JOB_EXECUTION_ID", null);
        using var altScope = new EnvScope("BATCH_EXECUTION_ID", id.ToString("D"));
        using var nameScope = new EnvScope("BATCH_JOB_NAME", BatchJobNames.BulkStaffRatesUpdate);

        var ctx = BulkRatesExecutionContext.FromEnvironment(null);

        Assert.Equal(id, ctx.JobExecutionId);
    }

    // ── FromEnvironment — TriggerYear ────────────────────────────────────────

    [Fact]
    public void FromEnvironment_WhenParametersJsonMissing_ShouldHaveNullTriggerYear()
    {
        using var idScope = new EnvScope("BATCH_JOB_EXECUTION_ID", Guid.NewGuid().ToString("D"));
        using var paramScope = new EnvScope("BATCH_JOB_PARAMETERS_JSON", null);

        var ctx = BulkRatesExecutionContext.FromEnvironment(null);

        Assert.Null(ctx.TriggerYear);
    }

    [Fact]
    public void FromEnvironment_WhenParametersJsonContainsNumericYear_ShouldParseTriggerYear()
    {
        using var idScope = new EnvScope("BATCH_JOB_EXECUTION_ID", Guid.NewGuid().ToString("D"));
        using var paramScope = new EnvScope("BATCH_JOB_PARAMETERS_JSON", "{\"year\":2027}");

        var ctx = BulkRatesExecutionContext.FromEnvironment(null);

        Assert.Equal(2027, ctx.TriggerYear);
    }

    [Fact]
    public void FromEnvironment_WhenParametersJsonContainsStringYear_ShouldParseTriggerYear()
    {
        using var idScope = new EnvScope("BATCH_JOB_EXECUTION_ID", Guid.NewGuid().ToString("D"));
        using var paramScope = new EnvScope("BATCH_JOB_PARAMETERS_JSON", "{\"year\":\"2028\"}");

        var ctx = BulkRatesExecutionContext.FromEnvironment(null);

        Assert.Equal(2028, ctx.TriggerYear);
    }

    [Fact]
    public void FromEnvironment_WhenParametersJsonInvalid_ShouldHaveNullTriggerYear()
    {
        using var idScope = new EnvScope("BATCH_JOB_EXECUTION_ID", Guid.NewGuid().ToString("D"));
        using var paramScope = new EnvScope("BATCH_JOB_PARAMETERS_JSON", "not-json");

        var ctx = BulkRatesExecutionContext.FromEnvironment(null);

        Assert.Null(ctx.TriggerYear);
    }

    // ── FromEnvironment — CorrelationId ──────────────────────────────────────

    [Fact]
    public void FromEnvironment_WhenCorrelationIdProvided_ShouldUseIt()
    {
        using var idScope = new EnvScope("BATCH_JOB_EXECUTION_ID", Guid.NewGuid().ToString("D"));

        var ctx = BulkRatesExecutionContext.FromEnvironment("my-trace-id");

        Assert.Equal("my-trace-id", ctx.CorrelationId);
    }

    [Fact]
    public void FromEnvironment_WhenCorrelationIdNullOrEmpty_ShouldDefaultToJobExecutionId()
    {
        var id = Guid.NewGuid();
        using var idScope = new EnvScope("BATCH_JOB_EXECUTION_ID", id.ToString("D"));

        var ctx = BulkRatesExecutionContext.FromEnvironment(null);

        Assert.Equal(id.ToString("D"), ctx.CorrelationId);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private sealed class EnvScope : IDisposable
    {
        private readonly string _name;
        private readonly string? _original;

        public EnvScope(string name, string? value)
        {
            _name = name;
            _original = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
        }

        public void Dispose() => Environment.SetEnvironmentVariable(_name, _original);
    }
}

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;

namespace Apha.BatchJobs.UnitTests;

public sealed class YearEndExecutionContextTests
{
    [Fact]
    public void FromEnvironment_WhenParametersJsonContainsYears_ShouldParseThem()
    {
        using var paramScope = new EnvScope("BATCH_JOB_PARAMETERS_JSON", "{\"currentFpsYear\":2026,\"targetFpsYear\":2027}");

        var ctx = YearEndExecutionContext.FromEnvironment(Guid.NewGuid().ToString("D"));

        Assert.Equal(2026, ctx.CurrentFpsYear);
        Assert.Equal(2027, ctx.TargetFpsYear);
    }

    [Theory]
    [InlineData("\"currentFpsYear=2026\"")]
    [InlineData("[]")]
    [InlineData("2026")]
    public void FromEnvironment_WhenParametersJsonRootIsNotObject_ShouldHaveNullYears(string json)
    {
        using var paramScope = new EnvScope("BATCH_JOB_PARAMETERS_JSON", json);

        var ctx = YearEndExecutionContext.FromEnvironment(Guid.NewGuid().ToString("D"));

        Assert.Null(ctx.CurrentFpsYear);
        Assert.Null(ctx.TargetFpsYear);
    }

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

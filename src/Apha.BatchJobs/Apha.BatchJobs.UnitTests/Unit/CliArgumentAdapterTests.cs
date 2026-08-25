using Apha.BatchJobs.Worker.Bootstrap;

namespace Apha.BatchJobs.UnitTests.Unit;

public sealed class CliArgumentAdapterTests : IDisposable
{
    private readonly string? _originalValue = Environment.GetEnvironmentVariable("BATCH_JOB_NAME");

    [Fact]
    public void Apply_NoArgs_DoesNotSetEnvVar()
    {
        Environment.SetEnvironmentVariable("BATCH_JOB_NAME", null);

        CliArgumentAdapter.Apply([]);

        Assert.Null(Environment.GetEnvironmentVariable("BATCH_JOB_NAME"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Apply_BlankArg_DoesNotSetEnvVar(string arg)
    {
        Environment.SetEnvironmentVariable("BATCH_JOB_NAME", null);

        CliArgumentAdapter.Apply([arg]);

        Assert.Null(Environment.GetEnvironmentVariable("BATCH_JOB_NAME"));
    }

    [Fact]
    public void Apply_ValidArg_SetsBatchJobName()
    {
        Environment.SetEnvironmentVariable("BATCH_JOB_NAME", null);

        CliArgumentAdapter.Apply(["RecreateSummary"]);

        Assert.Equal("RecreateSummary", Environment.GetEnvironmentVariable("BATCH_JOB_NAME"));
    }

    public void Dispose() =>
        Environment.SetEnvironmentVariable("BATCH_JOB_NAME", _originalValue);
}

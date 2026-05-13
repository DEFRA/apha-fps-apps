namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Opt-in parity harness tests for SQL baseline versus DotNetLinq execution.
/// These require a reachable local PostgreSQL database and explicit enablement.
/// </summary>
public sealed class RecreateSummariesParityHarnessTests
{
    [Fact]
    public async Task SqlFiles_And_DotNetLinq_Should_Produce_Identical_Target_Table_Snapshots()
    {
        if (!ShouldRunParityHarness())
        {
            return;
        }

        var harness = new RecreateSummariesParityHarness();
        var report = await harness.ExecuteAsync(month: 1, triggeredBy: "parity-harness");

        Assert.NotEmpty(report.Runs);
        Assert.Equal(2, report.Runs.Count);
        Assert.NotEmpty(report.TableResults);
        Assert.True(report.AllTablesMatch, BuildMismatchMessage(report));
    }

    private static bool ShouldRunParityHarness()
    {
        var enabled = Environment.GetEnvironmentVariable("RUN_RECREATE_SUMMARIES_PARITY");
        if (!string.Equals(enabled, "true", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static string BuildMismatchMessage(ParityReport report)
    {
        var mismatches = report.TableResults
            .Where(x => !x.IsMatch)
            .Select(x => $"{x.TableName}: baseline rows={x.BaselineRowCount}, candidate rows={x.CandidateRowCount}, baseline hash={x.BaselineHash}, candidate hash={x.CandidateHash}")
            .ToArray();

        return $"Parity mismatches found.{Environment.NewLine}Report: {report.ReportPath}{Environment.NewLine}{string.Join(Environment.NewLine, mismatches)}";
    }
}

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Npgsql;

namespace Apha.BatchJobs.UnitTests;

internal sealed class RecreateSummariesParityHarness
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=admin123;Timeout=30;Command Timeout=300";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _connectionString;
    private readonly string _repoRoot;
    private readonly string _workerProjectPath;
    private readonly string _validationOutputDir;

    public RecreateSummariesParityHarness()
    {
        _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__BatchJobsConnectionString") ?? DefaultConnectionString;
        _repoRoot = FindRepoRoot();
        _workerProjectPath = Path.Combine(_repoRoot, "src", "Apha.BatchJobs", "Apha.BatchJobs.Worker", "Apha.BatchJobs.Worker.csproj");
        _validationOutputDir = Path.Combine(_repoRoot, "src", "Apha.BatchJobs", "docs", "database", "validation");
    }

    public async Task<ParityReport> ExecuteAsync(int month, string triggeredBy, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_validationOutputDir);

        var report = new ParityReport
        {
            ExecutedAtUtc = DateTime.UtcNow,
            Month = month,
            TriggeredBy = triggeredBy,
            ConnectionStringName = "ConnectionStrings__BatchJobsConnectionString"
        };

        await EnsureDatabaseAvailableAsync(cancellationToken);

        await ResetOutputsAsync(month, triggeredBy, cancellationToken);
        var sqlRun = await ExecuteWorkerAsync("SqlFiles", month, triggeredBy, cancellationToken);
        report.Runs.Add(sqlRun);
        sqlRun.Snapshots = await CaptureSnapshotsAsync(month, triggeredBy, cancellationToken);

        await ResetOutputsAsync(month, triggeredBy, cancellationToken);
        var linqRun = await ExecuteWorkerAsync("DotNetLinq", month, triggeredBy, cancellationToken);
        report.Runs.Add(linqRun);
        linqRun.Snapshots = await CaptureSnapshotsAsync(month, triggeredBy, cancellationToken);

        report.TableResults = CompareSnapshots(sqlRun.Snapshots, linqRun.Snapshots).ToList();
        report.AllTablesMatch = report.TableResults.All(r => r.IsMatch);

        var timestamp = report.ExecutedAtUtc.ToString("yyyyMMdd-HHmmss");
        report.ReportPath = Path.Combine(_validationOutputDir, $"recreate-summaries-parity-{timestamp}.json");
        await File.WriteAllTextAsync(report.ReportPath, JsonSerializer.Serialize(report, JsonOptions), cancellationToken);

        return report;
    }

    private async Task EnsureDatabaseAvailableAsync(CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand("SELECT 1;", conn);
        _ = await cmd.ExecuteScalarAsync(cancellationToken);
    }

    private async Task ResetOutputsAsync(int month, string triggeredBy, CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM fps.projectmonthfinal;
            DELETE FROM fps.projectmonth3;
            DELETE FROM fps.projectmonth2;
            DELETE FROM fps.projectmonthcasework;
            DELETE FROM fps.timecostcalcs;
            DELETE FROM fps.projectmonth;
            DELETE FROM fps.fpsyeartotals;
            DELETE FROM fps.period_monthlyoutput WHERE period = @period;
            DELETE FROM fps.period_proj_subcontract WHERE period = @period;
            DELETE FROM fps.period_timecostcalcs WHERE period = @period;
            DELETE FROM fps.recreatesummaries_log WHERE userid = @userId AND period = @period;
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("period", month);
        cmd.Parameters.AddWithValue("userId", triggeredBy);
        _ = await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<ParityRun> ExecuteWorkerAsync(string mode, int month, string triggeredBy, CancellationToken cancellationToken)
    {
        var run = new ParityRun
        {
            Mode = mode,
            StartedAtUtc = DateTime.UtcNow
        };

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{_workerProjectPath}\" -- RecreateSummaries",
            WorkingDirectory = _repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.StartInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        process.StartInfo.Environment["BATCH_JOB_NAME"] = "RecreateSummaries";
        process.StartInfo.Environment["BATCH_RUN_MODE"] = "Manual";
        process.StartInfo.Environment["BATCH_RECREATE_SUMMARIES_MONTH"] = month.ToString();
        process.StartInfo.Environment["BATCH_RECREATE_SUMMARIES_TRIGGERED_BY"] = triggeredBy;
        process.StartInfo.Environment["BatchJobs__RecreateSummariesImplementationMode"] = mode;
        process.StartInfo.Environment["ConnectionStrings__BatchJobsConnectionString"] = _connectionString;

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        process.OutputDataReceived += (_, e) => { if (e.Data is not null) stdout.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) stderr.AppendLine(e.Data); };

        if (!process.Start())
        {
            throw new InvalidOperationException($"Failed to start worker process for mode '{mode}'.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromMinutes(10));
        await process.WaitForExitAsync(timeoutCts.Token);

        run.EndedAtUtc = DateTime.UtcNow;
        run.ExitCode = process.ExitCode;
        run.StdOut = stdout.ToString();
        run.StdErr = stderr.ToString();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"RecreateSummaries worker failed in mode '{mode}' with exit code {process.ExitCode}.{Environment.NewLine}" +
                $"STDOUT:{Environment.NewLine}{run.StdOut}{Environment.NewLine}" +
                $"STDERR:{Environment.NewLine}{run.StdErr}");
        }

        return run;
    }

    private async Task<List<TableSnapshot>> CaptureSnapshotsAsync(int month, string triggeredBy, CancellationToken cancellationToken)
    {
        var definitions = GetSnapshotDefinitions(month, triggeredBy);
        var snapshots = new List<TableSnapshot>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        foreach (var definition in definitions)
        {
            var rows = new List<Dictionary<string, string?>>();
            await using var cmd = new NpgsqlCommand(definition.Sql, conn);
            cmd.Parameters.AddWithValue("period", month);
            cmd.Parameters.AddWithValue("userId", triggeredBy);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, string?>(StringComparer.Ordinal);
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i).ToString();
                }
                rows.Add(row);
            }

            var json = JsonSerializer.Serialize(rows);
            snapshots.Add(new TableSnapshot
            {
                TableName = definition.Name,
                RowCount = rows.Count,
                Hash = ComputeHash(json)
            });
        }

        return snapshots;
    }

    private static IEnumerable<TableParityResult> CompareSnapshots(IEnumerable<TableSnapshot> baseline, IEnumerable<TableSnapshot> candidate)
    {
        var candidateMap = candidate.ToDictionary(x => x.TableName, StringComparer.OrdinalIgnoreCase);
        foreach (var baseSnapshot in baseline)
        {
            candidateMap.TryGetValue(baseSnapshot.TableName, out var candidateSnapshot);
            yield return new TableParityResult
            {
                TableName = baseSnapshot.TableName,
                BaselineRowCount = baseSnapshot.RowCount,
                CandidateRowCount = candidateSnapshot?.RowCount ?? -1,
                BaselineHash = baseSnapshot.Hash,
                CandidateHash = candidateSnapshot?.Hash ?? string.Empty,
                IsMatch = candidateSnapshot is not null
                    && baseSnapshot.RowCount == candidateSnapshot.RowCount
                    && string.Equals(baseSnapshot.Hash, candidateSnapshot.Hash, StringComparison.Ordinal)
            };
        }
    }

    private static string ComputeHash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    private static IReadOnlyList<SnapshotDefinition> GetSnapshotDefinitions(int month, string triggeredBy) =>
    [
        new("fpsyeartotals", "SELECT parentproject, program, totaladditionalcosts::text, totalanimalcosts::text, totalstaffcosts::text, totaltestcosts::text, totalcosts::text, custincome::text, transferincome::text, totalincome::text, budget_cvl::text, requiredprofit::text, manager, customer, projectstatus, pvsincome::text, plancaseworkdebit::text, totalpaycosts::text, fpsyear::text FROM fps.fpsyeartotals ORDER BY parentproject, fpsyear::text"),
        new("projectmonth", "SELECT project, monthno::text, costprofile::text FROM fps.projectmonth ORDER BY project, monthno"),
        new("timecostcalcs", "SELECT workgroup, jobcode, project, month::text, staffid, gradecode, name, chargerate::text, class, time::text, cost::text, division, pay::text, nonpay::text, overhead::text, fpsyear::text FROM fps.timecostcalcs ORDER BY project, month, staffid, jobcode"),
        new("projectmonthcasework", "SELECT project, monthno::text, cwdebit::text, cwcredit::text FROM fps.projectmonthcasework ORDER BY project, monthno"),
        new("projectmonth2", "SELECT project, monthno::text, costprofile::text, subcontracts::text, animals::text, nonanimal::text, timecosts::text, transfercosts::text, totalcost::text, invoices::text, coiw::text, sumofcostprofile::text, portsales::text, mstonedue::text, due__done::text, ontime::text, totalhours::text, paycosts::text FROM fps.projectmonth2 ORDER BY project, monthno"),
        new("projectmonth3", "SELECT endperiod::text, periodname, project, cumcost::text, cuminvoices::text, cumcoiw::text, cumportsales::text, cumprofile::text, sumofcostprofile::text, sumofmstonedue::text, sumofdue__done::text, sumofontime::text, cumcwdebit::text, cumcwcredit::text, cumtotalhours::text, cumsubcontracts::text, cumtestcosts::text, cumpaycosts::text FROM fps.projectmonth3 ORDER BY project, endperiod"),
        new("projectmonthfinal", "SELECT project, monthno::text, costprofile::text, subcontracts::text, animals::text, nonanimals::text, timecosts::text, transfercosts::text, totalcost::text, invoices::text, coiw::text, portsales::text, cumcost::text, cumprofile::text, periodname, sumofcostprofile::text, cuminvoices::text, cumcoiw::text, cumportsales::text, mstonedue::text, due__done::text, ontime::text, sumofmstonedue::text, sumofdue__done::text, sumofontime::text, cumflag::text, cwdebit::text, cwcredit::text, cumcwdebit::text, cumcwcredit::text, totalhours::text, cumtotalhours::text, cumsubcontracts::text, cumtestcosts::text, paycosts::text, cumpaycosts::text FROM fps.projectmonthfinal ORDER BY project, monthno"),
        new("period_monthlyoutput", "SELECT period::text, project, oracleprojectcode, subaccountcode, isdefraproject, opc, occ::text, month::text, spc, workgroup, scc::text, testcode, volume::text, testprice::text, totalcost::text FROM fps.period_monthlyoutput WHERE period = @period ORDER BY project, month, workgroup, testcode"),
        new("period_proj_subcontract", "SELECT period::text, subcontcounter::text, project, oracleprojectcode, subaccountcode, isdefraproject, opc, occ::text, month::text, amount::text, acctcode FROM fps.period_proj_subcontract WHERE period = @period ORDER BY subcontcounter, project, month"),
        new("period_timecostcalcs", "SELECT period::text, project, oracleprojectcode, subaccountcode, month::text, defraproject, occ::text, opc, spc, scc::text, name, gradecode, spnumber, chargerate::text, pay::text, nonpay::text, overhead::text, time::text, totalcost::text FROM fps.period_timecostcalcs WHERE period = @period ORDER BY project, month, name, gradecode, spnumber"),
        new("recreatesummaries_log", "SELECT userid, period::text FROM fps.recreatesummaries_log WHERE userid = @userId AND period = @period ORDER BY userid, period")
    ];

    private static string FindRepoRoot()
    {
        // In git worktrees .git can be a file, so detect the BatchJobs project folder first,
        // then resolve the repository root from there.
        var startPaths = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        const string marker = "BatchJobs.csproj";

        foreach (var start in startPaths)
        {
            var current = new DirectoryInfo(start);
            while (current is not null)
            {
                if (File.Exists(Path.Combine(current.FullName, marker)))
                {
                    var parent = current.Parent;

                    // Expected layout: <repo>/src/Apha.BatchJobs/BatchJobs.csproj
                    if (parent is not null &&
                        string.Equals(parent.Name, "src", StringComparison.OrdinalIgnoreCase))
                    {
                        return parent.Parent?.FullName ?? parent.FullName;
                    }

                    // Fallback for alternate layouts.
                    return parent?.FullName ?? current.FullName;
                }

                current = current.Parent;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not locate repository root for parity harness (looked for '{marker}' walking up from CWD '{Directory.GetCurrentDirectory()}' and '{AppContext.BaseDirectory}').");
    }

    private sealed record SnapshotDefinition(string Name, string Sql);
}

internal sealed class ParityReport
{
    public DateTime ExecutedAtUtc { get; set; }
    public int Month { get; set; }
    public string TriggeredBy { get; set; } = string.Empty;
    public string ConnectionStringName { get; set; } = string.Empty;
    public List<ParityRun> Runs { get; set; } = [];
    public List<TableParityResult> TableResults { get; set; } = [];
    public bool AllTablesMatch { get; set; }
    public string ReportPath { get; set; } = string.Empty;
}

internal sealed class ParityRun
{
    public string Mode { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime EndedAtUtc { get; set; }
    public int ExitCode { get; set; }
    public string StdOut { get; set; } = string.Empty;
    public string StdErr { get; set; } = string.Empty;
    public List<TableSnapshot> Snapshots { get; set; } = [];
}

internal sealed class TableSnapshot
{
    public string TableName { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public string Hash { get; set; } = string.Empty;
}

internal sealed class TableParityResult
{
    public string TableName { get; set; } = string.Empty;
    public int BaselineRowCount { get; set; }
    public int CandidateRowCount { get; set; }
    public string BaselineHash { get; set; } = string.Empty;
    public string CandidateHash { get; set; } = string.Empty;
    public bool IsMatch { get; set; }
}

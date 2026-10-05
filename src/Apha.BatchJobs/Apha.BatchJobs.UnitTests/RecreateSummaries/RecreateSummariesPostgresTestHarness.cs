using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries.Execution;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;
using Xunit;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

internal sealed class RecreateSummariesPostgresTestHarness : IAsyncDisposable
{
    private readonly string _connectionString;
    private readonly NpgsqlConnection _connection;
    private readonly NpgsqlTransaction _transaction;

    private RecreateSummariesPostgresTestHarness(
        string connectionString,
        BatchJobsDbContext dbContext,
        NpgsqlConnection connection,
        NpgsqlTransaction transaction)
    {
        _connectionString = connectionString;
        DbContext = dbContext;
        _connection = connection;
        _transaction = transaction;
        Prefix = $"UT{Random.Shared.Next(1000, 9999)}";
    }

    public BatchJobsDbContext DbContext { get; }

    public string Prefix { get; }

    public int FpsYear => 2026;

    public static async Task<RecreateSummariesPostgresTestHarness> CreateAsync()
    {
        var rawConnectionString = ResolveConnectionString();

        var builder = new NpgsqlConnectionStringBuilder(rawConnectionString)
        {
            IncludeErrorDetail = true
        };

        var connectionString = builder.ConnectionString;

        var connection = new NpgsqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();
        }
        catch (Exception ex)
        {
            await connection.DisposeAsync();
            Skip.If(true, $"Integration DB unavailable: {ex.Message}");
            return null!; // unreachable — Skip.If always throws
        }
        var transaction = await connection.BeginTransactionAsync();

        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(connection)
            .Options;

        var dbContext = new BatchJobsDbContext(options);
        await dbContext.Database.UseTransactionAsync(transaction);

        var harness = new RecreateSummariesPostgresTestHarness(connectionString, dbContext, connection, transaction);
        await harness.SeedReferenceDataAsync();
        return harness;
    }

    // Lookup values the step fixtures hard-code; seeded only where missing and rolled back with the test.
    private async Task SeedReferenceDataAsync()
    {
        var years = $"(VALUES ({FpsYear}), ({FpsYear - 1})) AS y(fpsyear)";

        await ExecuteSqlAsync($@"
            INSERT INTO fps.tblstatus (status)
            SELECT 'Active' WHERE NOT EXISTS (SELECT 1 FROM fps.tblstatus WHERE status = 'Active');

            INSERT INTO fps.tbldisease (disease)
            SELECT 'General' WHERE NOT EXISTS (SELECT 1 FROM fps.tbldisease WHERE disease = 'General');

            INSERT INTO fps.tlkpcustomer (customer)
            SELECT 'Cust' WHERE NOT EXISTS (SELECT 1 FROM fps.tlkpcustomer WHERE customer = 'Cust');

            INSERT INTO fps.tlkpaccountcode (code, description)
            SELECT v.code, 'Test' FROM (VALUES ('IA1'), ('IA2'), ('IA-CUR'), ('IA-OLD'), ('IA-SRC')) AS v(code)
            WHERE NOT EXISTS (SELECT 1 FROM fps.tlkpaccountcode a WHERE a.code = v.code);

            INSERT INTO fps.tlkpsubaccount (subaccountcode)
            SELECT v.code FROM (VALUES ('SAC1'), ('SAC2'), ('SAC-CUR'), ('SAC-OLD'), ('SAC-SRC')) AS v(code)
            WHERE NOT EXISTS (SELECT 1 FROM fps.tlkpsubaccount s WHERE s.subaccountcode = v.code);

            INSERT INTO fps.tblcontract (contractno, category, customer, fpsyear)
            SELECT 'Contract', (SELECT category FROM fps.tblcategory ORDER BY category LIMIT 1), 'Cust', y.fpsyear
            FROM {years}
            WHERE EXISTS (SELECT 1 FROM fps.tblyearmaster m WHERE m.fpsyear = y.fpsyear)
              AND NOT EXISTS (SELECT 1 FROM fps.tblcontract c WHERE c.contractno = 'Contract' AND c.fpsyear = y.fpsyear);

            INSERT INTO fps.tblkpaccountcategory (accshortname, accounttype, fpsyear)
            SELECT 'A1', 'NPRC', y.fpsyear
            FROM {years}
            WHERE EXISTS (SELECT 1 FROM fps.tblyearmaster m WHERE m.fpsyear = y.fpsyear)
              AND NOT EXISTS (SELECT 1 FROM fps.tblkpaccountcategory a WHERE a.accshortname = 'A1' AND a.fpsyear = y.fpsyear);

            INSERT INTO fps.grade (gradecode, fpsyear)
            SELECT 'GC1', {FpsYear}
            WHERE NOT EXISTS (SELECT 1 FROM fps.grade WHERE gradecode = 'GC1' AND fpsyear = {FpsYear});

            INSERT INTO fps.divisiongrade (divisiongrade, gradecode, division, fpsyear)
            SELECT 'D1', 'GC1', (SELECT divname FROM fps.tlkpdivision ORDER BY divname LIMIT 1), {FpsYear}
            WHERE NOT EXISTS (SELECT 1 FROM fps.divisiongrade WHERE divisiongrade = 'D1' AND fpsyear = {FpsYear});
        ");
    }

    private static string ResolveConnectionString()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString");
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment;
        }

        var currentDirectory = Directory.GetCurrentDirectory();

        var candidatePaths = new[]
        {
            Path.GetFullPath(Path.Combine(currentDirectory, "Apha.BatchJobs.UnitTests", "appsettings.local.json")),
            Path.GetFullPath(Path.Combine(currentDirectory, "appsettings.local.json")),
            Path.GetFullPath(Path.Combine(currentDirectory, "Apha.BatchJobs.Worker", "appsettings.Local.json")),
            Path.GetFullPath(Path.Combine(currentDirectory, "src", "Apha.BatchJobs", "Apha.BatchJobs.Worker", "appsettings.Local.json")),
            Path.GetFullPath(Path.Combine(currentDirectory, "src", "Apha.BatchJobs", "Apha.BatchJobs.UnitTests", "appsettings.local.json")),
        };

        var settingsPath = candidatePaths.FirstOrDefault(File.Exists);

        if (!File.Exists(settingsPath))
        {
            return string.Empty;
        }

        var json = File.ReadAllText(settingsPath);
        var config = JsonSerializer.Deserialize<LocalSettings>(json, JsonOptions)?.PostgresConfig;

        if (config is null
            || string.IsNullOrWhiteSpace(config.Host)
            || string.IsNullOrWhiteSpace(config.Database)
            || string.IsNullOrWhiteSpace(config.User)
            || string.IsNullOrWhiteSpace(config.Password)
            || config.Port <= 0)
        {
            return string.Empty;
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = config.Host,
            Port = config.Port,
            Database = config.Database,
            Username = config.User,
            Password = config.Password,
            Timeout = 30,
            SslMode = config.Ssl ? SslMode.Require : SslMode.Disable,
        };

        return builder.ConnectionString;
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private sealed class LocalSettings
    {
        public LocalPostgresConfig? PostgresConfig { get; set; }
    }

    private sealed class LocalPostgresConfig
    {
        public string? Label { get; set; }
        public string? Host { get; set; }
        public string? User { get; set; }
        public int Port { get; set; }
        public bool Ssl { get; set; }
        public string? Database { get; set; }
        public string? Password { get; set; }
    }

    public string Id(string suffix) => $"{Prefix}_{suffix}";

    public async Task<int> ExecuteSqlAsync(string sql)
        => await DbContext.Database.ExecuteSqlRawAsync(sql);

    public async Task<StepResult> ExecuteStepAsync(string typeName, params object[] args)
    {
        var type = typeof(IRecreateSummariesExecutionStep).Assembly
            .GetType($"Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps.{typeName}");

        if (type is null)
        {
            throw new InvalidOperationException($"Unable to locate RecreateSummaries step type '{typeName}'.");
        }

        var step = Activator.CreateInstance(type, args: args) as IRecreateSummariesExecutionStep;

        if (step is null)
        {
            throw new InvalidOperationException($"Unable to create RecreateSummaries step '{typeName}'.");
        }

        var connection = (NpgsqlConnection)DbContext.Database.GetDbConnection();
        var context = new RecreateSummariesExecutionContext(DbContext, connection, FpsYear);
        return await step.ExecuteAsync(context, CancellationToken.None);
    }

    public async Task<int> ScalarIntAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return Convert.ToInt32(value);
    }

    public async Task<decimal?> ScalarNullableDecimalAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return value is null || value is DBNull ? null : Convert.ToDecimal(value);
    }

    public async Task<double?> ScalarNullableDoubleAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return value is null || value is DBNull ? null : Convert.ToDouble(value);
    }

    public async Task<string?> ScalarStringAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return value is null || value is DBNull ? null : value.ToString();
    }

    public async ValueTask DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        await DbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
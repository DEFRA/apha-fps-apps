using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

internal sealed class RecreateSummariesPostgresTestHarness : IAsyncDisposable
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=LOCAL_DB_PASSWORD;Timeout=30";
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
        var rawConnectionString = TestConnectionStringResolver.ResolveForTests(DefaultConnectionString);

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
        catch (NpgsqlException ex) when (ex.SqlState == "28P01" || ex.Message.Contains("password"))
        {
            throw new SkipException("Integration DB unavailable: Postgres authentication failed");
        }
        catch (Exception ex)
        {
            throw new SkipException($"Integration DB unavailable: {ex.Message}");
        }

        var transaction = await connection.BeginTransactionAsync();

        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(connection)
            .Options;

        var dbContext = new BatchJobsDbContext(options);
        await dbContext.Database.UseTransactionAsync(transaction);

        return new RecreateSummariesPostgresTestHarness(connectionString, dbContext, connection, transaction);
    }

    public string Id(string suffix) => $"{Prefix}_{suffix}";

    public async Task<int> ExecuteSqlAsync(string sql)
        => await DbContext.Database.ExecuteSqlRawAsync(sql);

    public async Task<StepResult> ExecuteStepAsync(string typeName, params object[] args)
    {
        var type = typeof(IRecreateSummariesExecutionStep).Assembly
            .GetType($"Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries.{typeName}");

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
        var context = new RecreateSummariesExecutionContext(DbContext, connection);
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
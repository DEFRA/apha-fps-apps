using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed integration tests proving <see cref="ValidateYearScopedSchemaStep"/>'s
/// target-year partition validation (Phase 2 step 2) — every table in the Year End Table Rule
/// Matrix that needs a target-year partition must have one attached before Data Setup proceeds.
/// </summary>
/// <remarks>
/// Unlike the other Phase 1/2 integration tests, this one is safe to run against
/// <c>batchjob_testing</c>: the step under test only ever issues <c>SELECT</c>s against
/// <c>pg_catalog</c>/<c>information_schema</c>/<c>fps.tblyearmaster</c>, and the transaction is put
/// into <c>READ ONLY</c> mode as the very first statement after <c>BEGIN</c> — enforced by
/// Postgres itself, not just by this test never issuing a write. The transaction is always rolled
/// back and never commits. No seed data, no DML, no DDL, no locks.
/// </remarks>
[Trait("Category", "Integration")]
public sealed class ValidateYearScopedSchemaStepPartitionIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;
    private int _liveOpenYear;

    public ValidateYearScopedSchemaStepPartitionIntegrationTests()
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
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                _skipReason = "Integration DB unavailable.";
                return;
            }

            var openYears = await context.Database
                .SqlQuery<int>($@"SELECT fpsyear AS ""Value"" FROM fps.tblyearmaster WHERE yearstatus = 'Open'")
                .ToListAsync();

            if (openYears.Count == 1)
            {
                _liveOpenYear = openYears[0];
            }
            else
            {
                _skipReason = "fps.tblyearmaster does not have exactly one Open year on this database.";
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task ExecuteAsync_WhenTargetYearHasAllPartitionsAttached_Passes()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await RunUnderReadOnlyTransactionAsync(async (connection, transaction, cancellationToken) =>
        {
            var step = new ValidateYearScopedSchemaStep(NullLogger<ValidateYearScopedSchemaStep>.Instance);
            var context = new YearEndExecutionContext(
                "partition-check-positive", null, CurrentFpsYear: _liveOpenYear, TargetFpsYear: _liveOpenYear + 1);

            // Should not throw — the currently Planned year already has every partition attached.
            var resultContext = await step.ExecuteAsync(context, connection, transaction, cancellationToken);

            Assert.Equal(context.TargetFpsYear, resultContext.TargetFpsYear);
        });
    }

    /// <summary>
    /// Supersedes the old "2099 must fail" assertion. <c>fpsyear</c> is the authoritative
    /// business-year discriminator; a partitioned parent with a <c>DEFAULT</c> partition attached
    /// (confirmed 2026-08-14 read-only scan: all 62 matrix entries have one in both
    /// <c>batch_jobs_foundation_db</c> and the RDS <c>batchjobs</c> database) can legally accept
    /// any year's rows without DDL, including a year nobody has ever heard of. No dedicated
    /// partition for 2099 exists anywhere, so this specifically proves the <c>DEFAULT</c>-routing
    /// path, not the explicit-partition path already covered by the positive test above.
    /// </summary>
    [SkippableFact]
    public async Task ExecuteAsync_WhenTargetYearHasNoDedicatedPartition_PassesViaDefaultRouting()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await RunUnderReadOnlyTransactionAsync(async (connection, transaction, cancellationToken) =>
        {
            var step = new ValidateYearScopedSchemaStep(NullLogger<ValidateYearScopedSchemaStep>.Instance);
            const int farFutureYear = 2099; // no dedicated partition exists for this year anywhere
            var context = new YearEndExecutionContext(
                "partition-check-default-routing", null, CurrentFpsYear: _liveOpenYear, TargetFpsYear: farFutureYear);

            // Should not throw — every matrix table routes 2099 through its DEFAULT partition.
            var resultContext = await step.ExecuteAsync(context, connection, transaction, cancellationToken);

            Assert.Equal(context.TargetFpsYear, resultContext.TargetFpsYear);
        });
    }

    // No integration test covers "a partitioned matrix table with neither an explicit target-year
    // partition nor a DEFAULT partition attached" (the remaining genuine failure case for
    // ValidateTargetYearPartitionsAsync's routability check). Every real fps.* matrix table in both
    // batch_jobs_foundation_db and batchjobs already has a DEFAULT partition by design (see the
    // scan referenced above), so that state can't be reproduced against live matrix data without
    // DDL — which would violate this test class's "no DDL, no seed data" invariant (see class
    // remarks). Exercising that branch would need either test-only DDL or restructuring
    // ValidateYearScopedSchemaStep to accept an injectable table list; out of scope here.

    private async Task RunUnderReadOnlyTransactionAsync(Func<System.Data.Common.DbConnection, System.Data.Common.DbTransaction, CancellationToken, Task> action)
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.OpenConnectionAsync();
        var connection = dbContext.Database.GetDbConnection();

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var dbTransaction = transaction.GetDbTransaction();

        // Must be the very first statement after BEGIN — enforced read-only at the Postgres
        // engine level, not just application discipline.
        await using (var setReadOnly = connection.CreateCommand())
        {
            setReadOnly.Transaction = dbTransaction;
            setReadOnly.CommandText = "SET TRANSACTION READ ONLY;";
            await setReadOnly.ExecuteNonQueryAsync();
        }

        try
        {
            await action(connection, dbTransaction, CancellationToken.None);
        }
        finally
        {
            // Always rolled back — this transaction never commits, regardless of outcome.
            await transaction.RollbackAsync();
        }
    }

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);
}

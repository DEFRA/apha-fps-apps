using System.Reflection;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Resilience;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.Resilience;

/// <summary>
/// Tests for <see cref="BatchJobsRetryStrategy"/>.
///
/// This strategy handles EF Core / Npgsql DB-operation retry only. It is distinct from
/// <see cref="Apha.BatchJobs.Application.FailureHandling.BatchFailureClassifier"/>, which
/// classifies the final job-level outcome after all retries are exhausted. The two mechanisms
/// have different owners, different retry scopes, and must not be conflated.
///
/// ShouldRetryOn is protected, so it is exercised via reflection; ExecuteAsync tests verify
/// end-to-end retry and non-retry paths without requiring a live Postgres connection.
/// </summary>
public sealed class BatchJobsRetryStrategyTests
{
    // Strategy for ShouldRetryOn tests — context can be disposed immediately because
    // ShouldRetryOn only inspects the exception, never touches the DbContext.
    private static BatchJobsRetryStrategy CreateForClassificationTests()
    {
        var opts = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new BatchJobsDbContext(opts);
        var deps = db.GetService<ExecutionStrategyDependencies>();
        return new BatchJobsRetryStrategy(deps, maxRetryCount: 3, maxRetryDelay: TimeSpan.Zero);
    }

    // Strategy for ExecuteAsync tests — context must stay alive because EF Core accesses
    // CurrentContext.Context during OnRetry (change-tracker clear).
    private static (BatchJobsRetryStrategy Strategy, BatchJobsDbContext Db) CreateForExecuteAsyncTests()
    {
        var opts = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new BatchJobsDbContext(opts);
        var deps = db.GetService<ExecutionStrategyDependencies>();
        return (new BatchJobsRetryStrategy(deps, maxRetryCount: 3, maxRetryDelay: TimeSpan.Zero), db);
    }

    private static bool ShouldRetryOn(BatchJobsRetryStrategy strategy, Exception ex)
    {
        var method = typeof(ExecutionStrategy)
            .GetMethod("ShouldRetryOn", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (bool)method.Invoke(strategy, [ex])!;
    }

    // ─────────────────────────────────────────────────────────────
    // ShouldRetryOn classification
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void ShouldRetryOn_TransientNpgsqlException_ReturnsTrue()
    {
        Assert.True(ShouldRetryOn(CreateForClassificationTests(), new FakeTransientNpgsqlException()));
    }

    [Fact]
    public void ShouldRetryOn_NonTransientNpgsqlException_ReturnsFalse()
    {
        Assert.False(ShouldRetryOn(CreateForClassificationTests(), new FakeNonTransientNpgsqlException()));
    }

    [Fact]
    public void ShouldRetryOn_DiskFullPostgresException_ReturnsFalse()
    {
        // 53100 is explicitly excluded regardless of IsTransient — retrying a disk-full
        // re-executes the heavy query and makes spill worse, not better.
        var diskFull = new PostgresException("No space left on device", "ERROR", "ERROR", "53100");
        Assert.False(ShouldRetryOn(CreateForClassificationTests(), diskFull));
    }

    [Fact]
    public void ShouldRetryOn_OperationCanceledException_ReturnsFalse()
    {
        Assert.False(ShouldRetryOn(CreateForClassificationTests(), new OperationCanceledException()));
    }

    [Fact]
    public void ShouldRetryOn_NonDbException_ReturnsFalse()
    {
        Assert.False(ShouldRetryOn(CreateForClassificationTests(), new InvalidOperationException("business failure")));
    }

    // ─────────────────────────────────────────────────────────────
    // ExecuteAsync behaviour
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_TransientFailureThenSuccess_RetriesAndReturnsResult()
    {
        var (strategy, db) = CreateForExecuteAsyncTests();
        await using (db)
        {
            var callCount = 0;

            IExecutionStrategy iStrategy = strategy;
            var result = await iStrategy.ExecuteAsync<object?, int>(
                null,
                (_, _, _) =>
                {
                    callCount++;
                    if (callCount == 1)
                        throw new FakeTransientNpgsqlException();
                    return Task.FromResult(callCount);
                },
                verifySucceeded: null);

            Assert.Equal(2, callCount);
            Assert.Equal(2, result);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DiskFullException_DoesNotRetry_PropagatesImmediately()
    {
        var (strategy, db) = CreateForExecuteAsyncTests();
        await using (db)
        {
            var callCount = 0;

            IExecutionStrategy iStrategy = strategy;
            await Assert.ThrowsAsync<PostgresException>(() =>
                iStrategy.ExecuteAsync<object?, int>(
                    null,
                    (_, _, _) =>
                    {
                        callCount++;
                        throw new PostgresException("No space left on device", "ERROR", "ERROR", "53100");
                    },
                    verifySucceeded: null));

            Assert.Equal(1, callCount);
        }
    }

    [Fact]
    public async Task ExecuteAsync_OperationCanceledException_PropagatesWithoutRetry()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var (strategy, db) = CreateForExecuteAsyncTests();
        await using (db)
        {
            var callCount = 0;

            IExecutionStrategy iStrategy = strategy;
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                iStrategy.ExecuteAsync<object?, int>(
                    null,
                    (_, _, ct) =>
                    {
                        callCount++;
                        ct.ThrowIfCancellationRequested();
                        return Task.FromResult(0);
                    },
                    verifySucceeded: null,
                    cts.Token));

            Assert.True(callCount <= 1); // at most one attempt — no retry on cancellation
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Fakes — NpgsqlException is abstract; IsTransient is computed
    // from SqlState, so a subclass is needed to control its value.
    // ─────────────────────────────────────────────────────────────

    private sealed class FakeTransientNpgsqlException : NpgsqlException
    {
        public FakeTransientNpgsqlException() : base("transient connection error") { }
        public override bool IsTransient => true;
    }

    private sealed class FakeNonTransientNpgsqlException : NpgsqlException
    {
        public FakeNonTransientNpgsqlException() : base("non-transient error") { }
        public override bool IsTransient => false;
    }
}

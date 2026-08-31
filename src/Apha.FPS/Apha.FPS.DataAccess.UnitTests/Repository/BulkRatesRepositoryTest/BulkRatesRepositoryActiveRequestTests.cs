using Apha.Common.Constants;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Npgsql;

namespace Apha.FPS.DataAccess.UnitTests.Repository.BulkRatesRepositoryTest
{
    /// <summary>
    /// Proves <see cref="BulkRatesRepository.CanInitiateRequestAsync"/> uses the same
    /// blocking-status definition the retired row-returning GetActiveRequestAsync it replaced did
    /// (Initiated/ReleasedForApproval/Approved/Running block; Completed/Rejected/Failed/
    /// Cancelled don't) — see docs/bulk-rates-no-active-request-handling-spec.md Section 3.
    ///
    /// Uses <see cref="BulkRatesJobNames.Animal"/>, distinct from the job name the sibling
    /// <see cref="BulkRatesRepositoryYearFilterTests"/>/<see cref="BulkRatesRepositoryDownloadConcurrencyTests"/>
    /// use, to avoid cross-test-class interference on the shared local Postgres instance.
    ///
    /// The "false" assertions (a blocking row exists) are robust regardless of any other data
    /// already present for this job — at least one blocking row is sufficient by definition. The
    /// "true" assertions (only non-blocking rows exist) implicitly trust the shared dev DB has no
    /// stray blocking row for this job left over from unrelated activity, the same assumption the
    /// sibling tests in this folder already make; this suite doesn't attempt to force a literal
    /// empty-table precondition (that would mean deleting shared dev data, which is out of scope
    /// for a test).
    ///
    /// Soft-skips (no assertions run, test still passes) when Postgres is unreachable — same
    /// pattern as the sibling tests in this folder.
    /// </summary>
    public sealed class BulkRatesRepositoryActiveRequestTests : IAsyncLifetime
    {
        // No working credential is checked in - set ConnectionStrings__FPSConnectionString locally to run
        // this suite against a real Postgres instance. Without it, the connection attempt fails and
        // InitializeAsync soft-skips, matching the "Postgres unreachable" path this suite already handles.
        private const string DefaultConnectionString =
            "Host=localhost;Port=5432;Database=batch_jobs_foundation_db_cloud;Username=postgres;Password=<LOCAL_DB_PASSWORD>;SSL Mode=Disable";
        private readonly string _connectionString;
        private bool _dbAvailable;
        private readonly List<Guid> _createdJobQueueIds = new();

        public BulkRatesRepositoryActiveRequestTests()
        {
            _connectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
                ?? DefaultConnectionString;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                _dbAvailable = true;
            }
            catch
            {
                _dbAvailable = false;
            }
        }

        public async Task DisposeAsync()
        {
            if (!_dbAvailable || _createdJobQueueIds.Count == 0) return;

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            foreach (var jobQueueId in _createdJobQueueIds)
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM fps.job_queue WHERE jobqueueid = @id;";
                cmd.Parameters.AddWithValue("id", jobQueueId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private BulkRatesRepository CreateRepository()
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(DateTime.UtcNow.Year);
            var options = new DbContextOptionsBuilder<FpsDbContext>().UseNpgsql(_connectionString).Options;
            var context = new FpsDbContext(options, requestContext);
            return new BulkRatesRepository(context, NullLogger<BulkRatesRepository>.Instance);
        }

        private async Task<(BulkRatesRepository Repo, int JobId)> CreateRowWithStatusAsync(string statusName)
        {
            var repo = CreateRepository();
            var jobId = await repo.GetJobIdByNameAsync(BulkRatesJobNames.Animal)
                ?? throw new InvalidOperationException($"fps.job_master has no '{BulkRatesJobNames.Animal}' row.");
            var statusId = await repo.GetStatusIdByNameAsync(jobId, statusName)
                ?? throw new InvalidOperationException($"fps.job_status has no '{statusName}' row for '{BulkRatesJobNames.Animal}'.");

            var jobQueueId = Guid.NewGuid();
            var jobExecutionId = Guid.NewGuid();
            await repo.CreateRequestAsync(
                jobQueueId, jobExecutionId, jobId, statusId,
                "active-request-test", DateTime.UtcNow, DateTime.UtcNow.Year);
            _createdJobQueueIds.Add(jobQueueId);

            return (repo, jobId);
        }

        [Theory]
        [InlineData("Initiated")]
        [InlineData("ReleasedForApproval")]
        [InlineData("Approved")]
        [InlineData("Running")]
        public async Task CanInitiateRequestAsync_WhenBlockingStatusRowExists_ReturnsFalse(string blockingStatus)
        {
            if (!_dbAvailable) return;

            var (repo, _) = await CreateRowWithStatusAsync(blockingStatus);

            var result = await repo.CanInitiateRequestAsync(BulkRatesJobNames.Animal);

            Assert.False(result);
        }

        [Theory]
        [InlineData("Completed")]
        [InlineData("Rejected")]
        [InlineData("Failed")]
        [InlineData("Cancelled")]
        public async Task CanInitiateRequestAsync_WhenOnlyTerminalStatusRowExists_ReturnsTrue(string terminalStatus)
        {
            if (!_dbAvailable) return;

            var (repo, _) = await CreateRowWithStatusAsync(terminalStatus);

            var result = await repo.CanInitiateRequestAsync(BulkRatesJobNames.Animal);

            Assert.True(result);
        }
    }
}

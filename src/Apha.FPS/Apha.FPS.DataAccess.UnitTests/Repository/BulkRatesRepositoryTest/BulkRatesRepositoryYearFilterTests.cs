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
    /// Proves that BulkRatesRepository's <c>.IgnoreQueryFilters()</c> calls actually bypass
    /// <c>BatchJobQueue</c>'s ambient FpsYear query filter (<c>HasQueryFilter(e => e.FpsYear ==
    /// FilterFpsYear)</c>, owned by YearEnd) — a Bulk Rates request created for one year must
    /// still be readable back when the ambient <see cref="IFpsRequestContext"/> reports a
    /// completely different year, since Bulk Rates filters explicitly by caller-supplied year,
    /// never by "whatever year the UI currently has selected". If any of the read methods this
    /// repository builds on <c>QueueRowsQuery()</c> lost its <c>.IgnoreQueryFilters()</c> call,
    /// this test would fail with a null/empty result rather than the mismatch going unnoticed.
    ///
    /// Soft-skips (no assertions run, test still passes) when Postgres is unreachable.
    /// </summary>
    public sealed class BulkRatesRepositoryYearFilterTests : IAsyncLifetime
    {
        // No working credential is checked in - set ConnectionStrings__FPSConnectionString locally to run
        // this suite against a real Postgres instance. Without it, the connection attempt fails and
        // InitializeAsync soft-skips, matching the "Postgres unreachable" path this suite already handles.
        private const string DefaultConnectionString =
            "Host=localhost;Port=5432;Database=batch_jobs_foundation_db_cloud;Username=postgres;Password=<LOCAL_DB_PASSWORD>;SSL Mode=Disable";
        private readonly string _connectionString;
        private bool _dbAvailable;
        private readonly List<Guid> _createdJobQueueIds = new();

        public BulkRatesRepositoryYearFilterTests()
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

        private BulkRatesRepository CreateRepository(int ambientFpsYear)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(ambientFpsYear);
            var options = new DbContextOptionsBuilder<FpsDbContext>().UseNpgsql(_connectionString).Options;
            var context = new FpsDbContext(options, requestContext);
            return new BulkRatesRepository(context, NullLogger<BulkRatesRepository>.Instance);
        }

        [Fact]
        public async Task GetRequestAsync_WhenAmbientFpsYearDiffersFromRequestYear_StillReturnsRow()
        {
            if (!_dbAvailable) return;

            // Deliberately far apart so a real filter leak can't pass by coincidence.
            const int ambientYear = 1999;
            const int requestYear = 2031;

            var repo = CreateRepository(ambientYear);
            var jobId = await repo.GetJobIdByNameAsync(BulkRatesJobNames.Staff)
                ?? throw new InvalidOperationException($"fps.job_master has no '{BulkRatesJobNames.Staff}' row.");
            var statusId = await repo.GetStatusIdByNameAsync(jobId, "Initiated")
                ?? throw new InvalidOperationException($"fps.job_status has no 'Initiated' row for '{BulkRatesJobNames.Staff}'.");

            var jobQueueId = Guid.NewGuid();
            var jobExecutionId = Guid.NewGuid();
            var created = await repo.CreateRequestAsync(
                jobQueueId, jobExecutionId, jobId, statusId,
                "year-filter-test", DateTime.UtcNow, requestYear);
            _createdJobQueueIds.Add(jobQueueId);

            Assert.Equal(requestYear, created.FpsYear);

            // The read path — GetJobIdByNameAsync/GetStatusIdByNameAsync above don't touch
            // BatchJobQueue at all, so a filter leak wouldn't show up there; GetRequestAsync is
            // the one that would silently return null if .IgnoreQueryFilters() were ever removed
            // from QueueRowsQuery(), since ambientYear (1999) != requestYear (2031).
            var fetched = await repo.GetRequestAsync(jobExecutionId);

            Assert.NotNull(fetched);
            Assert.Equal(jobQueueId, fetched!.JobQueueId);
            Assert.Equal(requestYear, fetched.FpsYear);
        }
    }
}

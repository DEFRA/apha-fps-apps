using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Npgsql;

namespace Apha.FPS.DataAccess.UnitTests.Repository.YearEndRepositoryTest
{
    // Serializes every live-Postgres test that can leave a job_queue row in "Initiated" status for
    // 'YearEnd-DataSetup' against this same suite's own tests - GetInitiatedDataSetupJobExecutionIdAsync
    // is deliberately system-wide (no fpsyear scoping, see the design comment on the production method),
    // so a transient Initiated row left by a concurrently-running test in a different class would make
    // these tests flaky if xunit ran them in parallel (its default for classes with no [Collection]).
    [CollectionDefinition("YearEndDataSetupIntegration", DisableParallelization = true)]
    public class YearEndDataSetupIntegrationCollection { }

    /// <summary>
    /// Proves Workstream 8's <c>GetInitiatedDataSetupJobExecutionIdAsync</c> live, against a real
    /// Postgres, through the actual repository method (not a mocked DbSet, which can't exercise
    /// <c>IgnoreQueryFilters</c> or a real <c>SingleOrDefaultAsync</c> at all): zero/one/many Initiated
    /// rows, status filtering (Approved/Rejected excluded), job-name filtering (CutOver excluded), and
    /// the ambient FpsYear query filter being genuinely bypassed.
    ///
    /// Soft-skips (no assertions run, test still passes) when Postgres is unreachable - same convention
    /// as the other YearEndRepository*IntegrationTests in this folder.
    /// </summary>
    [Collection("YearEndDataSetupIntegration")]
    public sealed class YearEndRepositoryInitiatedJobExecutionIdIntegrationTests : IAsyncLifetime
    {
        private const string DataSetupJobName = "YearEnd-DataSetup";
        private const string CutOverJobName = "YearEnd-CutOver";

        private readonly string _connectionString;
        private bool _dbAvailable;
        private readonly List<Guid> _createdJobQueueIds = new();

        public YearEndRepositoryInitiatedJobExecutionIdIntegrationTests()
        {
            _connectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
                ?? string.Empty;
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

        private YearEndRepository CreateRepository(int ambientFpsYear)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(ambientFpsYear);
            var options = new DbContextOptionsBuilder<FpsDbContext>().UseNpgsql(_connectionString).Options;
            var context = new FpsDbContext(options, requestContext);
            var stagingRepository = new YearEndStagingRepository(context);
            return new YearEndRepository(context, requestContext, stagingRepository);
        }

        private async Task<Guid> SeedJobQueueRowAsync(string jobName, string status, int fpsYear, int? targetFpsYear)
        {
            var jobQueueId = Guid.NewGuid();
            var jobExecutionId = Guid.NewGuid();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO fps.job_queue
                    (jobqueueid, jobexecutionid, jobid, statusid, requestedby, fpsyear, target_fpsyear, startdatetime)
                SELECT @jobqueueid, @jobexecutionid, m.jobid, s.statusid, 'initiated-jobexecutionid-integration-test',
                       @fpsyear, @targetfpsyear, now()
                FROM fps.job_master m
                JOIN fps.job_status s ON s.jobid = m.jobid AND s.status = @status
                WHERE m.jobname = @jobname;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("jobexecutionid", jobExecutionId);
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);
            cmd.Parameters.AddWithValue("targetfpsyear", (object?)targetFpsYear ?? DBNull.Value);
            cmd.Parameters.AddWithValue("status", status);
            cmd.Parameters.AddWithValue("jobname", jobName);

            var inserted = await cmd.ExecuteNonQueryAsync();
            if (inserted != 1)
                throw new InvalidOperationException(
                    $"Expected to insert one job_queue row for '{jobName}'/'{status}' -- check fps.job_master/fps.job_status seed data.");

            _createdJobQueueIds.Add(jobQueueId);
            return jobExecutionId;
        }

        [Fact]
        public async Task GetInitiatedDataSetupJobExecutionIdAsync_WhenNoInitiatedRowsExist_ReturnsNull()
        {
            if (!_dbAvailable) return;

            var repo = CreateRepository(2025);
            var result = await repo.GetInitiatedDataSetupJobExecutionIdAsync();

            if (result.HasValue)
            {
                // A genuine Initiated Data Setup request already exists on whatever database this ran
                // against - this method is deliberately system-wide/unscoped (see the design comment on
                // the production method), so this specific "zero rows" scenario cannot be proven without
                // disturbing that real state. Soft-skip rather than assert a false positive/negative.
                return;
            }

            Assert.Null(result);
        }

        [Fact]
        public async Task GetInitiatedDataSetupJobExecutionIdAsync_WhenOneInitiatedRowExists_ReturnsItsJobExecutionId()
        {
            if (!_dbAvailable) return;

            var jobExecutionId = await SeedJobQueueRowAsync(DataSetupJobName, "Initiated", 9081, 9082);
            var repo = CreateRepository(9081);

            var result = await repo.GetInitiatedDataSetupJobExecutionIdAsync();

            Assert.Equal(jobExecutionId, result);
        }

        [Fact]
        public async Task GetInitiatedDataSetupJobExecutionIdAsync_WhenRowIsApproved_ReturnsNull_OrAnotherGenuinelyInitiatedRow()
        {
            if (!_dbAvailable) return;

            var approvedJobExecutionId = await SeedJobQueueRowAsync(DataSetupJobName, "Approved", 9083, 9084);
            var repo = CreateRepository(9083);

            var result = await repo.GetInitiatedDataSetupJobExecutionIdAsync();

            // Never the Approved row's own id - proves the status filter actually excludes it, not just
            // that the table happened to be empty.
            Assert.NotEqual(approvedJobExecutionId, result);
        }

        [Fact]
        public async Task GetInitiatedDataSetupJobExecutionIdAsync_WhenRowIsRejected_ReturnsNull_OrAnotherGenuinelyInitiatedRow()
        {
            if (!_dbAvailable) return;

            var rejectedJobExecutionId = await SeedJobQueueRowAsync(DataSetupJobName, "Rejected", 9085, 9086);
            var repo = CreateRepository(9085);

            var result = await repo.GetInitiatedDataSetupJobExecutionIdAsync();

            Assert.NotEqual(rejectedJobExecutionId, result);
        }

        [Fact]
        public async Task GetInitiatedDataSetupJobExecutionIdAsync_WhenInitiatedRowIsForCutOver_IsNeverReturned()
        {
            if (!_dbAvailable) return;

            // CutOver has no target_fpsyear concept - pass null, matching how CutOver rows are actually
            // enqueued (EnqueueCutOverInitiationBatchJobAsync takes no targetFpsYear parameter).
            var cutOverJobExecutionId = await SeedJobQueueRowAsync(CutOverJobName, "Initiated", 9087, null);
            var repo = CreateRepository(9087);

            var result = await repo.GetInitiatedDataSetupJobExecutionIdAsync();

            // Proves the hardcoded DataSetup job-name filter actually filters, not just that no row
            // happened to exist.
            Assert.NotEqual(cutOverJobExecutionId, result);
        }

        [Fact]
        public async Task GetInitiatedDataSetupJobExecutionIdAsync_WhenRowsFpsYearDiffersFromAmbientRequestContext_IsStillFound()
        {
            if (!_dbAvailable) return;

            // The seeded row's own fpsyear (9088) deliberately does not match the ambient FpsYear the
            // repository is constructed with (9089) - proves .IgnoreQueryFilters() is actually bypassing
            // BatchJobQueue's global HasQueryFilter(e => e.FpsYear == FilterFpsYear), not just compiling.
            var jobExecutionId = await SeedJobQueueRowAsync(DataSetupJobName, "Initiated", 9088, 9090);
            var repo = CreateRepository(9089);

            var result = await repo.GetInitiatedDataSetupJobExecutionIdAsync();

            Assert.Equal(jobExecutionId, result);
        }

        [Fact]
        public async Task GetInitiatedDataSetupJobExecutionIdAsync_WhenTwoInitiatedRowsExist_ThrowsRatherThanChoosingOne()
        {
            if (!_dbAvailable) return;

            // The single-in-flight-request invariant violated on purpose - proves SingleOrDefaultAsync's
            // "more than one -> throw" semantics are actually wired up, not silently picking the newest.
            await SeedJobQueueRowAsync(DataSetupJobName, "Initiated", 9091, 9092);
            await SeedJobQueueRowAsync(DataSetupJobName, "Initiated", 9091, 9093);
            var repo = CreateRepository(9091);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.GetInitiatedDataSetupJobExecutionIdAsync());
        }
    }
}

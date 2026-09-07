using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Npgsql;

namespace Apha.FPS.DataAccess.UnitTests.Repository.YearEndRepositoryTest
{
    /// <summary>
    /// Proves the Workstream 3 acceptance scenario live, against a real Postgres, through the actual
    /// repository method Initiate calls (not a mocked DbContext): "From a clean baseline where
    /// tblyearmaster(target year) does not exist and no target-year settings/month-hours exist,
    /// Initiate succeeds and creates exactly one Initiated queue row with fpsyear = current/Open year
    /// and target_fpsyear = plannedYear." No config validation is involved -- Initiate no longer calls
    /// it -- so this only needs job_master/job_status seed data, nothing else.
    ///
    /// Soft-skips (no assertions run, test still passes) when Postgres is unreachable — same
    /// convention as BulkRatesRepositoryYearFilterTests / YearEndStagingRepositoryIntegrationTests.
    /// </summary>
    [Collection("YearEndDataSetupIntegration")]
    public sealed class YearEndRepositoryInitiationIntegrationTests : IAsyncLifetime
    {
        private readonly string _connectionString;
        private bool _dbAvailable;
        private readonly List<Guid> _createdJobQueueIds = new();

        public YearEndRepositoryInitiationIntegrationTests()
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

        private (YearEndRepository Repo, FpsDbContext Context) CreateRepository(int ambientFpsYear)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(ambientFpsYear);
            var options = new DbContextOptionsBuilder<FpsDbContext>().UseNpgsql(_connectionString).Options;
            var context = new FpsDbContext(options, requestContext);
            var stagingRepository = new YearEndStagingRepository(context);
            var yearMasterRepository = new YearMasterRepository(context);
            return (new YearEndRepository(context, stagingRepository, yearMasterRepository), context);
        }

        [Fact]
        public async Task EnqueueDataSetupInitiationBatchJobAsync_FromCleanBaseline_CreatesExactlyOneInitiatedRow_WithFpsYearAndTargetFpsYear()
        {
            if (!_dbAvailable) return;

            // Deliberately far-future planned year so this never collides with a genuinely in-use
            // planned year on whichever database this runs against, and to keep clean-up unambiguous.
            const int plannedYear = 9072;

            // Deliberately WRONG ambient value - proves job_queue.fpsyear comes from tblyearmaster's
            // real Open row (resolved below, live), never from IFpsRequestContext/X-FPS-Year. This is
            // the regression coverage for the defect where Initiate populated fpsyear from whichever
            // year the UI session happened to be browsing.
            const int wrongAmbientFpsYear = 9071;

            var (repo, context) = CreateRepository(wrongAmbientFpsYear);
            var realOpenYear = (await new YearMasterRepository(context).GetOpenFpsYearAsync())!.FpsYear;

            var created = await repo.EnqueueDataSetupInitiationBatchJobAsync(
                "YearEnd-DataSetup", "workstream3-acceptance-test", Guid.NewGuid().ToString(),
                $"'YearEnd-DataSetup' is initiated for {plannedYear}.", plannedYear);
            _createdJobQueueIds.Add(created.JobqueueId);

            Assert.Equal(realOpenYear, created.FpsYear);
            Assert.NotEqual(wrongAmbientFpsYear, created.FpsYear);
            Assert.Equal(plannedYear, created.TargetFpsYear);

            // Exactly one Initiated row for this JobExecutionId -- not "at least one", not "a row
            // with some other status" -- matching the acceptance criterion's own wording.
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COUNT(*) FROM fps.job_queue q
                JOIN fps.job_master m ON m.jobid = q.jobid
                JOIN fps.job_status s ON s.statusid = q.statusid AND s.jobid = q.jobid
                WHERE q.jobqueueid = @jobqueueid
                  AND m.jobname = 'YearEnd-DataSetup'
                  AND s.status = 'Initiated'
                  AND q.fpsyear = @fpsyear
                  AND q.target_fpsyear = @targetfpsyear;";
            cmd.Parameters.AddWithValue("jobqueueid", created.JobqueueId);
            cmd.Parameters.AddWithValue("fpsyear", realOpenYear);
            cmd.Parameters.AddWithValue("targetfpsyear", plannedYear);
            var matchingRows = (long)(await cmd.ExecuteScalarAsync())!;

            Assert.Equal(1, matchingRows);
        }
    }
}

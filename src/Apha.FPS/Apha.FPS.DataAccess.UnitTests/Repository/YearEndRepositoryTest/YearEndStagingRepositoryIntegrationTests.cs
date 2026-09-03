using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Npgsql;

namespace Apha.FPS.DataAccess.UnitTests.Repository.YearEndRepositoryTest
{
    /// <summary>
    /// Proves the Year End planned-year staging persistence primitives (CR067) round-trip correctly
    /// through the real EF mappings against a real Postgres — this is Workstream 2's own exit
    /// criteria (schema &lt;-&gt; EF mappings match, TargetFpsYear round-trips, JobExecutionId
    /// resolution works, staging INSERT/UPDATE/READ/DELETE works, re-Confirm upsert doesn't
    /// duplicate), not just "it compiles".
    ///
    /// Soft-skips (no assertions run, test still passes) when Postgres is unreachable — same
    /// convention as BulkRatesRepositoryYearFilterTests.
    /// </summary>
    [Collection("YearEndDataSetupIntegration")]
    public sealed class YearEndStagingRepositoryIntegrationTests : IAsyncLifetime
    {
        // No connection string is checked in - set ConnectionStrings__FPSConnectionString locally to
        // run this suite against a real Postgres instance. Without it, the connection attempt fails
        // and InitializeAsync soft-skips.
        private readonly string _connectionString;
        private bool _dbAvailable;
        private readonly List<Guid> _createdJobQueueIds = new();

        public YearEndStagingRepositoryIntegrationTests()
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
                // Staging rows FK to job_queue -- delete children first.
                await using (var del1 = conn.CreateCommand())
                {
                    del1.CommandText = "DELETE FROM fps.yearend_settings_staging WHERE jobqueueid = @id;";
                    del1.Parameters.AddWithValue("id", jobQueueId);
                    await del1.ExecuteNonQueryAsync();
                }
                await using (var del2 = conn.CreateCommand())
                {
                    del2.CommandText = "DELETE FROM fps.yearend_monthhours_staging WHERE jobqueueid = @id;";
                    del2.Parameters.AddWithValue("id", jobQueueId);
                    await del2.ExecuteNonQueryAsync();
                }
                await using (var del3 = conn.CreateCommand())
                {
                    del3.CommandText = "DELETE FROM fps.job_queue WHERE jobqueueid = @id;";
                    del3.Parameters.AddWithValue("id", jobQueueId);
                    await del3.ExecuteNonQueryAsync();
                }
            }
        }

        private YearEndStagingRepository CreateRepository(int ambientFpsYear)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(ambientFpsYear);
            var options = new DbContextOptionsBuilder<FpsDbContext>().UseNpgsql(_connectionString).Options;
            var context = new FpsDbContext(options, requestContext);
            return new YearEndStagingRepository(context);
        }

        /// <summary>
        /// Inserts a disposable job_queue row directly via raw SQL (Workstream 2 deliberately has no
        /// "create request" primitive yet -- that's Workstream 3's Initiate change), for the real
        /// YearEnd-DataSetup job and 'Initiated' status. Registers it for cleanup.
        /// </summary>
        private async Task<Guid> CreateTestJobQueueRowAsync(int fpsYear, int? targetFpsYear)
        {
            var jobQueueId = Guid.NewGuid();
            var jobExecutionId = Guid.NewGuid();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO fps.job_queue
                    (jobqueueid, jobexecutionid, jobid, statusid, requestedby, fpsyear, target_fpsyear)
                SELECT @jobqueueid, @jobexecutionid, m.jobid, s.statusid, 'staging-repo-integration-test',
                       @fpsyear, @targetfpsyear
                FROM fps.job_master m
                JOIN fps.job_status s ON s.jobid = m.jobid AND s.status = 'Initiated'
                WHERE m.jobname = 'YearEnd-DataSetup';";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("jobexecutionid", jobExecutionId);
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);
            cmd.Parameters.AddWithValue("targetfpsyear", (object?)targetFpsYear ?? DBNull.Value);

            var inserted = await cmd.ExecuteNonQueryAsync();
            if (inserted != 1)
                throw new InvalidOperationException(
                    "Expected to insert one job_queue row for 'YearEnd-DataSetup'/'Initiated' -- check fps.job_master/fps.job_status seed data.");

            _createdJobQueueIds.Add(jobQueueId);
            return jobQueueId;
        }

        private async Task<Guid> GetJobExecutionIdAsync(Guid jobQueueId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT jobexecutionid FROM fps.job_queue WHERE jobqueueid = @id;";
            cmd.Parameters.AddWithValue("id", jobQueueId);
            return (Guid)(await cmd.ExecuteScalarAsync())!;
        }

        [Fact]
        public async Task ResolveRequestAsync_ReturnsCorrectSummary_EvenWhenAmbientYearDiffers()
        {
            if (!_dbAvailable) return;

            const int requestFpsYear = 2025;
            const int requestTargetFpsYear = 2026;
            // Deliberately far from requestFpsYear so a real IgnoreQueryFilters() regression
            // couldn't pass by coincidence -- same reasoning as BulkRatesRepositoryYearFilterTests.
            const int ambientFpsYear = 1999;

            var jobQueueId = await CreateTestJobQueueRowAsync(requestFpsYear, requestTargetFpsYear);
            var jobExecutionId = await GetJobExecutionIdAsync(jobQueueId);

            var repo = CreateRepository(ambientFpsYear);
            var summary = await repo.ResolveRequestAsync(jobExecutionId);

            Assert.NotNull(summary);
            Assert.Equal(jobQueueId, summary!.JobQueueId);
            Assert.Equal(requestFpsYear, summary.FpsYear);
            Assert.Equal(requestTargetFpsYear, summary.TargetFpsYear);
            Assert.Equal("Initiated", summary.Status);
        }

        [Fact]
        public async Task ResolveRequestAsync_WhenNoMatchingRow_ReturnsNull()
        {
            if (!_dbAvailable) return;

            var repo = CreateRepository(2025);
            var summary = await repo.ResolveRequestAsync(Guid.NewGuid());

            Assert.Null(summary);
        }

        [Fact]
        public async Task UpsertStagedSetting_ThenRead_RoundTrips_AndReConfirmDoesNotDuplicate()
        {
            if (!_dbAvailable) return;

            var jobQueueId = await CreateTestJobQueueRowAsync(2025, 2026);
            var repo = CreateRepository(2025);

            await repo.UpsertStagedSettingAsync(new YearEndSettingStaging
            {
                JobQueueId = jobQueueId,
                Id = "HoursInDay",
                Setting = "8",
                Notes = "first confirm"
            });

            var afterFirst = await repo.GetStagedSettingsAsync(jobQueueId);
            Assert.Single(afterFirst);
            Assert.Equal("8", afterFirst[0].Setting);

            // Re-Confirm the same setting with a different value -- must update in place, not duplicate.
            await repo.UpsertStagedSettingAsync(new YearEndSettingStaging
            {
                JobQueueId = jobQueueId,
                Id = "HoursInDay",
                Setting = "7.5",
                Notes = "re-confirmed"
            });

            var afterSecond = await repo.GetStagedSettingsAsync(jobQueueId);
            Assert.Single(afterSecond);
            Assert.Equal("7.5", afterSecond[0].Setting);
            Assert.Equal("re-confirmed", afterSecond[0].Notes);
        }

        [Fact]
        public async Task UpsertStagedMonthHour_ThenRead_RoundTrips_IncludingMonthYear_AndReConfirmDoesNotDuplicate()
        {
            if (!_dbAvailable) return;

            var jobQueueId = await CreateTestJobQueueRowAsync(2025, 2026);
            var repo = CreateRepository(2025);

            await repo.UpsertStagedMonthHourAsync(new YearEndMonthHourStaging
            {
                JobQueueId = jobQueueId,
                MonthYear = 2026,
                Month = 1,
                Fmonth = 0,
                Days = 20.0m,
                CvlHours = 5.5m,
                VidHours = 3.0m
            });

            var afterFirst = await repo.GetStagedMonthHoursAsync(jobQueueId);
            Assert.Single(afterFirst);
            Assert.Equal((short)2026, afterFirst[0].MonthYear);
            Assert.Equal(20.0m, afterFirst[0].Days);

            // Re-Confirm the same (Month, Fmonth) with different values -- must update in place.
            await repo.UpsertStagedMonthHourAsync(new YearEndMonthHourStaging
            {
                JobQueueId = jobQueueId,
                MonthYear = 2026,
                Month = 1,
                Fmonth = 0,
                Days = 21.5m,
                CvlHours = 6.0m,
                VidHours = 3.5m
            });

            var afterSecond = await repo.GetStagedMonthHoursAsync(jobQueueId);
            Assert.Single(afterSecond);
            Assert.Equal(21.5m, afterSecond[0].Days);
            Assert.Equal(6.0m, afterSecond[0].CvlHours);
        }

        [Fact]
        public async Task DeleteStagingAsync_RemovesBothSettingsAndMonthHours()
        {
            if (!_dbAvailable) return;

            var jobQueueId = await CreateTestJobQueueRowAsync(2025, 2026);
            var repo = CreateRepository(2025);

            await repo.UpsertStagedSettingAsync(new YearEndSettingStaging
            {
                JobQueueId = jobQueueId,
                Id = "HoursInDay",
                Setting = "8"
            });
            await repo.UpsertStagedMonthHourAsync(new YearEndMonthHourStaging
            {
                JobQueueId = jobQueueId,
                MonthYear = 2026,
                Month = 1,
                Fmonth = 0,
                Days = 20.0m
            });

            Assert.Single(await repo.GetStagedSettingsAsync(jobQueueId));
            Assert.Single(await repo.GetStagedMonthHoursAsync(jobQueueId));

            await repo.DeleteStagingAsync(jobQueueId);

            Assert.Empty(await repo.GetStagedSettingsAsync(jobQueueId));
            Assert.Empty(await repo.GetStagedMonthHoursAsync(jobQueueId));
        }
    }
}

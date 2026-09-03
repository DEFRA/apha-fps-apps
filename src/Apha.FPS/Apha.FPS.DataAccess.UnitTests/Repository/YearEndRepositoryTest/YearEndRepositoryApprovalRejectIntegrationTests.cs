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
    /// Proves Workstream 6's Approve/Reject repository behavior live, against a real Postgres, through
    /// the actual methods (not mocked DbSets, which can't exercise the new by-jobQueueId
    /// <c>ExecuteUpdateAsync</c>-free conditional query at all): Reject transitions status AND deletes
    /// both staging sets atomically in the same transaction; Approve transitions status and leaves
    /// staging untouched (retained/frozen, for Workstream 7's Worker to consume); a second request's
    /// staging is never touched by the first's Reject; the new resolve-by-jobQueueId query re-checks
    /// Initiated status, not just presence.
    ///
    /// Soft-skips (no assertions run, test still passes) when Postgres is unreachable — same convention
    /// as YearEndStagingRepositoryIntegrationTests / YearEndRepositoryInitiationIntegrationTests.
    /// </summary>
    public sealed class YearEndRepositoryApprovalRejectIntegrationTests : IAsyncLifetime
    {
        // No connection string is checked in - set ConnectionStrings__FPSConnectionString locally to
        // run this suite against a real Postgres instance. Without it, the connection attempt fails
        // and InitializeAsync soft-skips.
        private readonly string _connectionString;
        private bool _dbAvailable;
        private readonly List<Guid> _createdJobQueueIds = new();

        public YearEndRepositoryApprovalRejectIntegrationTests()
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

        // Same scoped-FpsDbContext-instance wiring as production DI (ServiceCollectionExtension.cs) —
        // this is exactly the atomicity assumption Reject's staging deletion relies on, so the
        // integration test must construct it the same way, not just via separate contexts.
        private (YearEndRepository Repo, YearEndStagingRepository StagingRepo) CreateRepositories(int ambientFpsYear)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(ambientFpsYear);
            var options = new DbContextOptionsBuilder<FpsDbContext>().UseNpgsql(_connectionString).Options;
            var context = new FpsDbContext(options, requestContext);
            var stagingRepo = new YearEndStagingRepository(context);
            var repo = new YearEndRepository(context, requestContext, stagingRepo);
            return (repo, stagingRepo);
        }

        private async Task<Guid> CreateTestJobQueueRowAsync(int fpsYear, int? targetFpsYear, string status = "Initiated")
        {
            var jobQueueId = Guid.NewGuid();
            var jobExecutionId = Guid.NewGuid();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO fps.job_queue
                    (jobqueueid, jobexecutionid, jobid, statusid, requestedby, fpsyear, target_fpsyear, startdatetime)
                SELECT @jobqueueid, @jobexecutionid, m.jobid, s.statusid, 'approval-reject-integration-test',
                       @fpsyear, @targetfpsyear, now()
                FROM fps.job_master m
                JOIN fps.job_status s ON s.jobid = m.jobid AND s.status = @status
                WHERE m.jobname = 'YearEnd-DataSetup';";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("jobexecutionid", jobExecutionId);
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);
            cmd.Parameters.AddWithValue("targetfpsyear", (object?)targetFpsYear ?? DBNull.Value);
            cmd.Parameters.AddWithValue("status", status);

            var inserted = await cmd.ExecuteNonQueryAsync();
            if (inserted != 1)
                throw new InvalidOperationException(
                    $"Expected to insert one job_queue row for 'YearEnd-DataSetup'/'{status}' -- check fps.job_master/fps.job_status seed data.");

            _createdJobQueueIds.Add(jobQueueId);
            return jobQueueId;
        }

        /// <summary>Reconnects with a fresh connection — proves committed state, not one EF change
        /// tracker's in-memory view.</summary>
        private async Task<(string Status, int SettingsCount, int MonthHoursCount)> ReadStateAsync(Guid jobQueueId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var statusCmd = conn.CreateCommand();
            statusCmd.CommandText = @"
                SELECT s.status FROM fps.job_queue q
                JOIN fps.job_status s ON s.statusid = q.statusid AND s.jobid = q.jobid
                WHERE q.jobqueueid = @id;";
            statusCmd.Parameters.AddWithValue("id", jobQueueId);
            var status = (string)(await statusCmd.ExecuteScalarAsync())!;

            await using var settingsCmd = conn.CreateCommand();
            settingsCmd.CommandText = "SELECT COUNT(*) FROM fps.yearend_settings_staging WHERE jobqueueid = @id;";
            settingsCmd.Parameters.AddWithValue("id", jobQueueId);
            var settingsCount = Convert.ToInt32(await settingsCmd.ExecuteScalarAsync());

            await using var monthHoursCmd = conn.CreateCommand();
            monthHoursCmd.CommandText = "SELECT COUNT(*) FROM fps.yearend_monthhours_staging WHERE jobqueueid = @id;";
            monthHoursCmd.Parameters.AddWithValue("id", jobQueueId);
            var monthHoursCount = Convert.ToInt32(await monthHoursCmd.ExecuteScalarAsync());

            return (status, settingsCount, monthHoursCount);
        }

        [Fact]
        public async Task EnqueueDataSetupRejectBatchJobAsync_TransitionsStatus_AndDeletesBothStagingSets_InOneTransaction()
        {
            if (!_dbAvailable) return;

            var jobQueueId = await CreateTestJobQueueRowAsync(2025, 2026);
            var (repo, stagingRepo) = CreateRepositories(2025);

            await stagingRepo.UpsertStagedSettingAsync(new YearEndSettingStaging { JobQueueId = jobQueueId, Id = "HoursInDay", Setting = "8" });
            await stagingRepo.UpsertStagedMonthHourAsync(new YearEndMonthHourStaging { JobQueueId = jobQueueId, MonthYear = 2026, Month = 1, Fmonth = 0, Days = 20m });

            await repo.EnqueueDataSetupRejectBatchJobAsync(jobQueueId, "rejector@example.com", "rejected in integration test");

            var (status, settingsCount, monthHoursCount) = await ReadStateAsync(jobQueueId);
            Assert.Equal("Rejected", status);
            Assert.Equal(0, settingsCount);
            Assert.Equal(0, monthHoursCount);
        }

        [Fact]
        public async Task EnqueueDataSetupApprovalBatchJobAsync_TransitionsStatus_ButRetainsBothStagingSets()
        {
            if (!_dbAvailable) return;

            // Approve's counterpart to the Reject test above — proves the opposite: staging is
            // retained, not deleted, because Workstream 7's Worker consumes exactly these frozen rows.
            var jobQueueId = await CreateTestJobQueueRowAsync(2025, 2026);
            var (repo, stagingRepo) = CreateRepositories(2025);

            await stagingRepo.UpsertStagedSettingAsync(new YearEndSettingStaging { JobQueueId = jobQueueId, Id = "HoursInDay", Setting = "8" });
            await stagingRepo.UpsertStagedMonthHourAsync(new YearEndMonthHourStaging { JobQueueId = jobQueueId, MonthYear = 2026, Month = 1, Fmonth = 0, Days = 20m });

            await repo.EnqueueDataSetupApprovalBatchJobAsync(jobQueueId, "approver@example.com", "approved in integration test");

            var (status, settingsCount, monthHoursCount) = await ReadStateAsync(jobQueueId);
            Assert.Equal("Approved", status);
            Assert.Equal(1, settingsCount);
            Assert.Equal(1, monthHoursCount);
        }

        [Fact]
        public async Task EnqueueDataSetupRejectBatchJobAsync_NeverTouchesAnotherRequestsStaging()
        {
            if (!_dbAvailable) return;

            // Two independently-seeded Initiated-shaped requests. Rejecting the first must leave the
            // second completely untouched — proven by JobQueueId-scoped resolution itself, not just the
            // app-level single-in-flight-request invariant enforced elsewhere.
            var firstJobQueueId = await CreateTestJobQueueRowAsync(2025, 2026);
            var secondJobQueueId = await CreateTestJobQueueRowAsync(2025, 2026);
            var (repo, stagingRepo) = CreateRepositories(2025);

            await stagingRepo.UpsertStagedSettingAsync(new YearEndSettingStaging { JobQueueId = firstJobQueueId, Id = "HoursInDay", Setting = "8" });
            await stagingRepo.UpsertStagedSettingAsync(new YearEndSettingStaging { JobQueueId = secondJobQueueId, Id = "HoursInDay", Setting = "7.5" });

            await repo.EnqueueDataSetupRejectBatchJobAsync(firstJobQueueId, "rejector@example.com", "reject first only");

            var (firstStatus, firstSettingsCount, _) = await ReadStateAsync(firstJobQueueId);
            var (secondStatus, secondSettingsCount, _) = await ReadStateAsync(secondJobQueueId);

            Assert.Equal("Rejected", firstStatus);
            Assert.Equal(0, firstSettingsCount);
            Assert.Equal("Initiated", secondStatus);
            Assert.Equal(1, secondSettingsCount);
        }

        [Fact]
        public async Task EnqueueDataSetupApprovalBatchJobAsync_WhenRowNotInitiated_ThrowsKeyNotFoundException()
        {
            if (!_dbAvailable) return;

            // Proves the new by-jobQueueId query re-checks Initiated status live against Postgres, not
            // just against a mocked/fake harness.
            var jobQueueId = await CreateTestJobQueueRowAsync(2025, 2026, status: "Rejected");
            var (repo, _) = CreateRepositories(2025);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => repo.EnqueueDataSetupApprovalBatchJobAsync(jobQueueId, "approver@example.com", "note"));
        }
    }
}

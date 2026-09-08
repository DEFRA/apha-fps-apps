using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Npgsql;

namespace Apha.FPS.DataAccess.UnitTests.Repository.FpsSettingRepositoryTest
{
    /// <summary>
    /// Proves the Workstream 4 grid-read-path overlay (planned-year staging design) against a real
    /// Postgres — the exact acceptance scenario: a request with fpsyear/target_fpsyear and no
    /// tblyearmaster row for the target year still loads a settings grid, current-year values show
    /// by default, staging overrides them when present, and a different request's staging never
    /// leaks across.
    ///
    /// Uses the real, live current/Open year for CurrentYear — fps.tblsettings has a genuine FK to
    /// fps.tblyearmaster, so (unlike job_queue) a fake far-future year can't be used there. Reads the
    /// real current-year value rather than inserting one, so this never mutates shared live config
    /// data. TargetYear stays a fake far-future year — job_queue.target_fpsyear and the staging
    /// tables have no FK to tblyearmaster, so that's safe, and it doubles as living proof that no
    /// real target-year tblsettings row is required.
    ///
    /// Soft-skips (no assertions run, test still passes) when Postgres is unreachable — same
    /// convention as BulkRatesRepositoryYearFilterTests / YearEndStagingRepositoryIntegrationTests.
    ///
    /// [Collection("YearEndDataSetupIntegration")]: fps.tblsettings_staging/fps.tlkpmonthhours_staging
    /// are singletons as of the 2026-09-07 staging simplification (no jobqueueid scoping). Joins the
    /// same collection already shared by YearEndStagingRepositoryIntegrationTests/
    /// YearEndRepositoryApprovalRejectIntegrationTests/etc. to force sequential execution against
    /// those tables too — xUnit parallelizes different classes by default, and concurrent writers to a
    /// singleton table would corrupt each other's rows.
    /// </summary>
    [Collection("YearEndDataSetupIntegration")]
    public sealed class FpsSettingRepositoryYearEndGridIntegrationTests : IAsyncLifetime
    {
        private const int TargetYear = 9082;
        private const string SettingId = "HoursInDay";

        private readonly string _connectionString;
        private bool _dbAvailable;
        private int _currentYear;
        private string? _realCurrentYearValue;
        private readonly List<Guid> _createdJobQueueIds = new();

        public FpsSettingRepositoryYearEndGridIntegrationTests()
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

                await using var yearCmd = conn.CreateCommand();
                yearCmd.CommandText = @"
                    SELECT fpsyear FROM fps.tblyearmaster
                    WHERE active AND yearstatus = 'Open'
                    ORDER BY fpsyear DESC LIMIT 1;";
                _currentYear = (int)(await yearCmd.ExecuteScalarAsync())!;

                await using var valueCmd = conn.CreateCommand();
                valueCmd.CommandText = "SELECT setting FROM fps.tblsettings WHERE id = @id AND fpsyear = @fpsyear;";
                valueCmd.Parameters.AddWithValue("id", SettingId);
                valueCmd.Parameters.AddWithValue("fpsyear", _currentYear);
                _realCurrentYearValue = (string?)await valueCmd.ExecuteScalarAsync();
            }
            catch
            {
                _dbAvailable = false;
            }
        }

        public async Task DisposeAsync()
        {
            if (!_dbAvailable) return;

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Staging is a singleton (no jobqueueid scoping) — cleared unconditionally, not per
            // created job_queue row, so a failed assertion never leaks rows into a later test.
            await using (var delStaging = conn.CreateCommand())
            {
                delStaging.CommandText = "DELETE FROM fps.tblsettings_staging;";
                await delStaging.ExecuteNonQueryAsync();
            }

            foreach (var jobQueueId in _createdJobQueueIds)
            {
                await using var del2 = conn.CreateCommand();
                del2.CommandText = "DELETE FROM fps.job_queue WHERE jobqueueid = @id;";
                del2.Parameters.AddWithValue("id", jobQueueId);
                await del2.ExecuteNonQueryAsync();
            }
        }

        private FpsDbContext CreateDbContext(int ambientFpsYear)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(ambientFpsYear);
            var options = new DbContextOptionsBuilder<FpsDbContext>().UseNpgsql(_connectionString).Options;
            return new FpsDbContext(options, requestContext);
        }

        private FpsSettingRepository CreateRepository(int ambientFpsYear)
        {
            var context = CreateDbContext(ambientFpsYear);
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(ambientFpsYear);
            return new FpsSettingRepository(context, requestContext);
        }

        private YearEndStagingRepository CreateStagingRepository(int ambientFpsYear)
            => new(CreateDbContext(ambientFpsYear));

        private async Task<Guid> CreateTestJobQueueRowAsync()
        {
            var jobQueueId = Guid.NewGuid();
            var jobExecutionId = Guid.NewGuid();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO fps.job_queue
                    (jobqueueid, jobexecutionid, jobid, statusid, requestedby, fpsyear, target_fpsyear)
                SELECT @jobqueueid, @jobexecutionid, m.jobid, s.statusid, 'grid-read-integration-test',
                       @currentyear, @targetyear
                FROM fps.job_master m
                JOIN fps.job_status s ON s.jobid = m.jobid AND s.status = 'Initiated'
                WHERE m.jobname = 'YearEnd-DataSetup';";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("jobexecutionid", jobExecutionId);
            cmd.Parameters.AddWithValue("currentyear", _currentYear);
            cmd.Parameters.AddWithValue("targetyear", TargetYear);
            await cmd.ExecuteNonQueryAsync();

            _createdJobQueueIds.Add(jobQueueId);
            return jobQueueId;
        }

        [Fact]
        public async Task GetYearEndSettingsAsync_FromCleanTargetYear_LoadsCurrentYearDefaults_ExistsForPlannedYearIsNo()
        {
            if (!_dbAvailable) return;

            var jobQueueId = await CreateTestJobQueueRowAsync();
            var repo = CreateRepository(_currentYear);

            var request = new YearEndRequestSummary(jobQueueId, _currentYear, TargetYear, "Initiated");
            var result = await repo.GetYearEndSettingsAsync(request);

            var hoursInDay = result.Single(s => s.Id == SettingId);
            Assert.Equal(_realCurrentYearValue, hoursInDay.Setting); // whatever the real open-year row holds
            Assert.Equal("No", hoursInDay.ExistsForPlannedYear);
            Assert.Equal(TargetYear, hoursInDay.FpsYear); // displayed year is the target, not current
        }

        [Fact]
        public async Task GetYearEndSettingsAsync_WithStagedRow_OverridesCurrentYearValue_ExistsForPlannedYearIsYes()
        {
            if (!_dbAvailable) return;

            var jobQueueId = await CreateTestJobQueueRowAsync();

            var stagingRepo = CreateStagingRepository(_currentYear);
            await stagingRepo.UpsertStagedSettingAsync(new FpsSettingStaging
            {
                Id = SettingId,
                Setting = "7.5",
                FpsYear = TargetYear
            });

            var repo = CreateRepository(_currentYear);
            var request = new YearEndRequestSummary(jobQueueId, _currentYear, TargetYear, "Initiated");
            var result = await repo.GetYearEndSettingsAsync(request);

            var hoursInDay = result.Single(s => s.Id == SettingId);
            Assert.Equal("7.5", hoursInDay.Setting); // staged value, not the real current-year value
            Assert.Equal("Yes", hoursInDay.ExistsForPlannedYear);
        }

        [Fact]
        public async Task GetYearEndSettingsAsync_StagingForADifferentTargetYear_NeverVisible()
        {
            if (!_dbAvailable) return;

            // Staging is a singleton (no jobqueueid scoping) as of the 2026-09-07 simplification — the
            // only isolation left is by fpsyear. A row staged for a different target year must never
            // leak into a grid read scoped to this one, even though there's no longer a second
            // request's jobqueueid to isolate from.
            const int otherTargetYear = TargetYear + 1;
            var jobQueueId = await CreateTestJobQueueRowAsync();

            var stagingRepo = CreateStagingRepository(_currentYear);
            await stagingRepo.UpsertStagedSettingAsync(new FpsSettingStaging
            {
                Id = SettingId,
                Setting = "99",
                FpsYear = otherTargetYear
            });

            var repo = CreateRepository(_currentYear);
            var request = new YearEndRequestSummary(jobQueueId, _currentYear, TargetYear, "Initiated");
            var result = await repo.GetYearEndSettingsAsync(request);

            var hoursInDay = result.Single(s => s.Id == SettingId);
            Assert.NotEqual("99", hoursInDay.Setting); // never the other target year's staged value
            Assert.Equal(_realCurrentYearValue, hoursInDay.Setting); // this year's own default instead
            Assert.Equal("No", hoursInDay.ExistsForPlannedYear);
        }
    }
}

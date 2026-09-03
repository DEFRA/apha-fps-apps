using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Npgsql;

namespace Apha.FPS.DataAccess.UnitTests.Repository.MonthHourRepositoryTest
{
    /// <summary>
    /// Proves the Workstream 4 grid-read-path overlay (planned-year staging design) for Month Hours
    /// against a real Postgres — same scenario/rationale as
    /// FpsSettingRepositoryYearEndGridIntegrationTests: real current/Open year (fps.tlkpmonthhours
    /// also FKs to fps.tblyearmaster, so no fake-year insert), read rather than mutate real data, fake
    /// far-future TargetYear (no FK there, doubles as proof no real target-year row is required).
    ///
    /// Month=4/Fmonth=1 is used as the test slot deliberately, not Month=1/Fmonth=0: the legacy
    /// current-year lookup source (openMonthHours) is filtered to Fmonth &gt; 0, so Fmonth=0 slots can
    /// never resolve a current-year default — that's existing behaviour, unchanged, not something this
    /// test is proving.
    ///
    /// Soft-skips (no assertions run, test still passes) when Postgres is unreachable — same
    /// convention as BulkRatesRepositoryYearFilterTests / YearEndStagingRepositoryIntegrationTests.
    /// </summary>
    public sealed class MonthHourRepositoryYearEndGridIntegrationTests : IAsyncLifetime
    {
        private const int TargetYear = 9082;
        private const short TestMonth = 4;
        private const short TestFmonth = 1;

        private readonly string _connectionString;
        private bool _dbAvailable;
        private int _currentYear;
        private decimal? _realCurrentYearDays;

        private readonly List<Guid> _createdJobQueueIds = new();

        public MonthHourRepositoryYearEndGridIntegrationTests()
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

                // Mirrors the repository's own lookup exactly: openMonthHours is FpsYear=_currentYear
                // and Fmonth>0, matched by (Year = key.Year-1, Month, Fmonth) — key.Year here is
                // _currentYear+1, so Year = _currentYear.
                await using var valueCmd = conn.CreateCommand();
                valueCmd.CommandText = @"
                    SELECT days FROM fps.tlkpmonthhours
                    WHERE fpsyear = @fpsyear AND year = @year AND month = @month AND fmonth = @fmonth;";
                valueCmd.Parameters.AddWithValue("fpsyear", _currentYear);
                valueCmd.Parameters.AddWithValue("year", _currentYear);
                valueCmd.Parameters.AddWithValue("month", TestMonth);
                valueCmd.Parameters.AddWithValue("fmonth", TestFmonth);
                var scalar = await valueCmd.ExecuteScalarAsync();
                _realCurrentYearDays = scalar is null or DBNull ? null : (decimal)scalar;
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
                await using (var del1 = conn.CreateCommand())
                {
                    del1.CommandText = "DELETE FROM fps.yearend_monthhours_staging WHERE jobqueueid = @id;";
                    del1.Parameters.AddWithValue("id", jobQueueId);
                    await del1.ExecuteNonQueryAsync();
                }
                await using (var del2 = conn.CreateCommand())
                {
                    del2.CommandText = "DELETE FROM fps.job_queue WHERE jobqueueid = @id;";
                    del2.Parameters.AddWithValue("id", jobQueueId);
                    await del2.ExecuteNonQueryAsync();
                }
            }
        }

        private FpsDbContext CreateDbContext(int ambientFpsYear)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(ambientFpsYear);
            var options = new DbContextOptionsBuilder<FpsDbContext>().UseNpgsql(_connectionString).Options;
            return new FpsDbContext(options, requestContext);
        }

        private MonthHourRepository CreateRepository(int ambientFpsYear)
            => new(CreateDbContext(ambientFpsYear));

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
        public async Task GetYearEndMonthHoursAsync_FromCleanTargetYear_LoadsCurrentYearDefaults_ExistsForPlannedYearIsNo()
        {
            if (!_dbAvailable) return;

            var jobQueueId = await CreateTestJobQueueRowAsync();
            var repo = CreateRepository(_currentYear);

            var request = new YearEndRequestSummary(jobQueueId, _currentYear, TargetYear, "Initiated");
            var result = await repo.GetYearEndMonthHoursAsync(request);

            var slot = result.Single(m => m.Month == TestMonth && m.Fmonth == TestFmonth);
            Assert.Equal(_realCurrentYearDays, slot.Days); // whatever the real open-year row holds
            Assert.Equal("No", slot.ExistsForPlannedYear);
            Assert.Equal(TargetYear, slot.FpsYear); // displayed year is the target, not current
        }

        [Fact]
        public async Task GetYearEndMonthHoursAsync_WithStagedRow_OverridesCurrentYearValue_ExistsForPlannedYearIsYes()
        {
            if (!_dbAvailable) return;

            var jobQueueId = await CreateTestJobQueueRowAsync();

            var stagingRepo = CreateStagingRepository(_currentYear);
            await stagingRepo.UpsertStagedMonthHourAsync(new YearEndMonthHourStaging
            {
                JobQueueId = jobQueueId,
                MonthYear = (short)TargetYear,
                Month = TestMonth,
                Fmonth = TestFmonth,
                Days = 12.5m,
                CvlHours = 3m,
                VidHours = 2m
            });

            var repo = CreateRepository(_currentYear);
            var request = new YearEndRequestSummary(jobQueueId, _currentYear, TargetYear, "Initiated");
            var result = await repo.GetYearEndMonthHoursAsync(request);

            var slot = result.Single(m => m.Month == TestMonth && m.Fmonth == TestFmonth);
            Assert.Equal(12.5m, slot.Days); // staged value, not the real current-year value
            Assert.Equal("Yes", slot.ExistsForPlannedYear);
        }

        [Fact]
        public async Task GetYearEndMonthHoursAsync_AnotherRequestsStaging_NeverVisible()
        {
            if (!_dbAvailable) return;

            var thisRequestJobQueueId = await CreateTestJobQueueRowAsync();
            var otherRequestJobQueueId = await CreateTestJobQueueRowAsync();

            var stagingRepo = CreateStagingRepository(_currentYear);
            // Staged only against the OTHER request, never this one.
            await stagingRepo.UpsertStagedMonthHourAsync(new YearEndMonthHourStaging
            {
                JobQueueId = otherRequestJobQueueId,
                MonthYear = (short)TargetYear,
                Month = TestMonth,
                Fmonth = TestFmonth,
                Days = 99m
            });

            var repo = CreateRepository(_currentYear);
            var request = new YearEndRequestSummary(thisRequestJobQueueId, _currentYear, TargetYear, "Initiated");
            var result = await repo.GetYearEndMonthHoursAsync(request);

            var slot = result.Single(m => m.Month == TestMonth && m.Fmonth == TestFmonth);
            Assert.NotEqual(99m, slot.Days); // never the other request's staged value
            Assert.Equal(_realCurrentYearDays, slot.Days); // this request's own default instead
            Assert.Equal("No", slot.ExistsForPlannedYear);
        }
    }
}

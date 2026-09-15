using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.YearEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed integration tests for <see cref="InactiveEmployeeCleanupStep"/>'s rule: inactive
/// candidate is <c>personstatus='I'</c> (case-insensitive) AND <c>enddate IS NULL</c>, evaluated
/// against the target year only; General Staff exemption is <c>spnumber LIKE 'G%'</c> AND
/// <c>UPPER(firstname)='GENERAL'</c> (both required); an unexpected <c>personstatus</c> value is a
/// hard validation failure before any deletion; no <c>mabarchive</c> table is referenced.
/// </summary>
/// <remarks>
/// <see cref="YearEndDataSetupRepository"/> opens its own connection per call (main's established
/// repository pattern — see <see cref="Apha.BatchJobs.UnitTests.YearEndCutoverServiceIntegrationTests"/>),
/// so seeded rows are inserted and committed up front and always removed again in a finally block —
/// unlike a single-shared-transaction/rollback style, which this repository shape doesn't support.
/// Each test uses its own disposable far-future target year.
/// </remarks>
[Trait("Category", "Integration")]
public sealed class InactiveEmployeeCleanupStepIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;

    public InactiveEmployeeCleanupStepIntegrationTests()
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
            if (!await context.Database.CanConnectAsync())
            {
                _skipReason = "Integration DB unavailable.";
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task ExecuteAsync_InactiveNormalEmployee_IsRemoved()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90610;
        var seed = new SeedHelper(this, targetYear);
        try
        {
            await seed.SeedReferenceChainAsync(targetYear);
            await seed.EmployeeAsync("PACT001", "SP001", "Alan", personStatus: "I", enddate: null);

            await RunStepAsync(targetYear);

            Assert.False(await seed.WgEmployeeExistsAsync("PACT001"));
        }
        finally
        {
            await seed.CleanupAsync();
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_ActiveEmployee_IsRetained()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90611;
        var seed = new SeedHelper(this, targetYear);
        try
        {
            await seed.SeedReferenceChainAsync(targetYear);
            await seed.EmployeeAsync("PACT002", "SP002", "Alan", personStatus: "A", enddate: null);

            await RunStepAsync(targetYear);

            Assert.True(await seed.WgEmployeeExistsAsync("PACT002"));
        }
        finally
        {
            await seed.CleanupAsync();
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_GPrefixAndGeneralInactive_IsRetainedAsGeneralStaff()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90612;
        var seed = new SeedHelper(this, targetYear);
        try
        {
            await seed.SeedReferenceChainAsync(targetYear);
            await seed.EmployeeAsync("PACT003", "G123", "General", personStatus: "I", enddate: null);

            await RunStepAsync(targetYear);

            Assert.True(await seed.WgEmployeeExistsAsync("PACT003"));
        }
        finally
        {
            await seed.CleanupAsync();
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_GeneralFirstNameWithoutGPrefixInactive_IsRemovedUnderTheLegacyAndRule()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90613;
        var seed = new SeedHelper(this, targetYear);
        try
        {
            await seed.SeedReferenceChainAsync(targetYear);
            // Mirrors the live discordant case (spnumber "T195", firstname "General") — under the
            // confirmed AND rule this is NOT the General Staff exemption, since spnumber doesn't
            // start with 'G'.
            await seed.EmployeeAsync("PACT004", "T195", "General", personStatus: "I", enddate: null);

            await RunStepAsync(targetYear);

            Assert.False(await seed.WgEmployeeExistsAsync("PACT004"));
        }
        finally
        {
            await seed.CleanupAsync();
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_GPrefixButNotGeneralInactive_IsRemoved()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90614;
        var seed = new SeedHelper(this, targetYear);
        try
        {
            await seed.SeedReferenceChainAsync(targetYear);
            await seed.EmployeeAsync("PACT005", "G456", "Graham", personStatus: "I", enddate: null);

            await RunStepAsync(targetYear);

            Assert.False(await seed.WgEmployeeExistsAsync("PACT005"));
        }
        finally
        {
            await seed.CleanupAsync();
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_LowercasePersonStatus_IsTreatedTheSameAsUppercase()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90615;
        var seed = new SeedHelper(this, targetYear);
        try
        {
            await seed.SeedReferenceChainAsync(targetYear);
            await seed.EmployeeAsync("PACT006", "SP006", "Alan", personStatus: "i", enddate: null);

            await RunStepAsync(targetYear);

            Assert.False(await seed.WgEmployeeExistsAsync("PACT006"));
        }
        finally
        {
            await seed.CleanupAsync();
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_UnexpectedPersonStatusValue_ThrowsBeforeAnyDeletion()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90616;
        var seed = new SeedHelper(this, targetYear);
        try
        {
            await seed.SeedReferenceChainAsync(targetYear);
            // A genuinely inactive, non-General-Staff row that WOULD be removed if the step ran to
            // completion — proves the validation failure blocks the whole step, not just the bad row.
            await seed.EmployeeAsync("PACT007", "SP007", "Alan", personStatus: "I", enddate: null);
            // The known live-data anomaly: neither clearly active nor inactive.
            await seed.EmployeeAsync("PACT008", "SP008", "Brenda", personStatus: "AI", enddate: null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => RunStepAsync(targetYear));

            Assert.Contains("personstatus", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("PACT008", ex.Message, StringComparison.Ordinal);

            // No partial cleanup: the otherwise-eligible row must still be present.
            Assert.True(await seed.WgEmployeeExistsAsync("PACT007"));
            Assert.True(await seed.WgEmployeeExistsAsync("PACT008"));
        }
        finally
        {
            await seed.CleanupAsync();
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_InactiveStatusInADifferentYear_DoesNotInfluenceTheTargetYearDecision()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90617;
        const int otherYear = 90618;
        var seed = new SeedHelper(this, targetYear);
        try
        {
            await seed.SeedReferenceChainAsync(targetYear);
            await seed.SeedReferenceChainAsync(otherYear);

            // Active in the target year...
            await seed.EmployeeAsync("PACT009", "SP009", "Alan", personStatus: "A", enddate: null, fpsYear: targetYear);
            // ...but inactive in a different year for the same person/spnumber. If the join were not
            // year-scoped, or if any lookup ignored fpsyear, this could wrongly influence the result.
            await seed.EmployeeAsync("PACT009", "SP009", "Alan", personStatus: "I", enddate: null, fpsYear: otherYear);

            await RunStepAsync(targetYear);

            Assert.True(await seed.WgEmployeeExistsAsync("PACT009", targetYear));
            Assert.True(await seed.WgEmployeeExistsAsync("PACT009", otherYear));
        }
        finally
        {
            await seed.CleanupYearAsync(otherYear);
            await seed.CleanupAsync();
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_DependentTblStaffJobRows_AreDeletedBeforeTblWgEmployee()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90619;
        var seed = new SeedHelper(this, targetYear);
        try
        {
            await seed.SeedReferenceChainAsync(targetYear);
            await seed.EmployeeAsync("PACT010", "SP010", "Alan", personStatus: "I", enddate: null);
            await seed.StaffJobAsync("PACT010");

            await RunStepAsync(targetYear);

            Assert.False(await seed.WgEmployeeExistsAsync("PACT010"));
            Assert.False(await seed.StaffJobExistsAsync("PACT010"));
        }
        finally
        {
            await seed.CleanupAsync();
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_NeverReferencesMabArchive_SucceedsWithOnlyFpsSchemaSeeded()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        // No mabarchive.* table is seeded or expected to exist for this test — if the step still
        // referenced any mabarchive table, this would fail rather than clean up normally.
        const int targetYear = 90620;
        var seed = new SeedHelper(this, targetYear);
        try
        {
            await seed.SeedReferenceChainAsync(targetYear);
            await seed.EmployeeAsync("PACT011", "SP011", "Alan", personStatus: "I", enddate: null);

            await RunStepAsync(targetYear);

            Assert.False(await seed.WgEmployeeExistsAsync("PACT011"));
        }
        finally
        {
            await seed.CleanupAsync();
        }
    }

    private async Task RunStepAsync(int targetYear)
    {
        var step = new InactiveEmployeeCleanupStep(
            new YearEndDataSetupRepository(CreateDbContext()),
            NullLogger<InactiveEmployeeCleanupStep>.Instance);

        var context = new YearEndExecutionContext(
            CorrelationId: $"inactive-cleanup-it-{targetYear}",
            ParametersJson: null,
            CurrentFpsYear: null,
            TargetFpsYear: targetYear);

        await step.ExecuteAsync(context, CancellationToken.None);
    }

    private bool CanRun() => string.IsNullOrWhiteSpace(_skipReason);

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    /// <summary>
    /// Seeds the minimum FK chain <c>tblwgemployee</c>/<c>tblstaffjob</c> rows require (workgroupgrade
    /// -> grade/profitcentregrade/workgroup -> costcentre; tlkpproject -> tblcontract/tlkpprogram),
    /// plus employee/staffjob helper inserts, for one or more FPS years. Everything inserted is
    /// committed immediately (main's repository opens its own connection per call, so an
    /// uncommitted-transaction/rollback pattern isn't available here) and removed again by
    /// <see cref="CleanupAsync"/>/<see cref="CleanupYearAsync"/>.
    /// </summary>
    private sealed class SeedHelper
    {
        private readonly InactiveEmployeeCleanupStepIntegrationTests _owner;
        private readonly int _defaultYear;
        private readonly List<int> _seededYears = [];

        public SeedHelper(InactiveEmployeeCleanupStepIntegrationTests owner, int defaultYear)
        {
            _owner = owner;
            _defaultYear = defaultYear;
        }

        public async Task SeedReferenceChainAsync(int fpsYear)
        {
            _seededYears.Add(fpsYear);

            await ExecAsync($@"INSERT INTO fps.costcentre (costcentre, profitcentre, fpsyear) VALUES (900001, 'ITPC', {fpsYear});");
            await ExecAsync($@"INSERT INTO fps.workgroup (workgroup, profitcentre, costcentre, fpsyear) VALUES ('ITWG', 'ITPC', 900001, {fpsYear});");
            await ExecAsync($@"INSERT INTO fps.grade (gradecode, fpsyear) VALUES ('ITGR', {fpsYear});");
            await ExecAsync($@"INSERT INTO fps.divisiongrade (divisiongrade, gradecode, division, fpsyear) VALUES ('ITDG', 'ITGR', 'ITDIV', {fpsYear});");
            await ExecAsync($@"INSERT INTO fps.profitcentregrade (pcgrade, divisiongrade, gradecode, profitcentre, fpsyear) VALUES ('ITPCG', 'ITDG', 'ITGR', 'ITPC', {fpsYear});");
            await ExecAsync($@"INSERT INTO fps.workgroupgrade (wggrade, profitcentregrade, gradecode, workgroup, fpsyear) VALUES ('ITWGG', 'ITPCG', 'ITGR', 'ITWG', {fpsYear});");

            await ExecAsync($@"INSERT INTO fps.tblcontract (contractno, category, fpsyear) VALUES ('ITCN', 'ITCAT', {fpsYear});");
            await ExecAsync($@"INSERT INTO fps.tlkpprogram (programno, fpsyear) VALUES ('ITPR', {fpsYear});");

            // tlkpproject's NOT NULL columns include several global-reference (non-year-scoped) FKs —
            // disease/projectstatus/customer/incomeaccountcode. Look up one real existing value from
            // each rather than fabricating a placeholder that would violate the FK.
            var disease = await ScalarStringAsync("SELECT disease FROM fps.tbldisease LIMIT 1;");
            var projectStatus = await ScalarStringAsync("SELECT status FROM fps.tblstatus LIMIT 1;");
            var customer = await ScalarStringAsync("SELECT customer FROM fps.tlkpcustomer LIMIT 1;");
            var incomeAccountCode = await ScalarStringAsync("SELECT code FROM fps.tlkpaccountcode LIMIT 1;");

            await ExecAsync($@"
                INSERT INTO fps.tlkpproject (
                    parentproject, projecttitle, program, customer, transferincome, custincome,
                    projectstatus, disease, contract, isdefraproject, incomeaccountcode, fpsyear)
                VALUES ('ITPROJ', 'IT Test Project', 'ITPR', '{customer}', 0, 0, '{projectStatus}', '{disease}', 'ITCN', 0, '{incomeAccountCode}', {fpsYear});");
        }

        public async Task EmployeeAsync(string pactid, string spnumber, string firstName, string personStatus, DateTime? enddate, int? fpsYear = null)
        {
            var year = fpsYear ?? _defaultYear;

            await ExecAsync($@"
                INSERT INTO fps.tblemployee (spnumber, firstname, lastname, fpsyear)
                VALUES ('{spnumber}', '{firstName}', 'TestSurname', {year})
                ON CONFLICT DO NOTHING;");

            var enddateSql = enddate.HasValue ? $"'{enddate:yyyy-MM-dd}'" : "NULL";
            await ExecAsync($@"
                INSERT INTO fps.tblwgemployee (
                    pactid, spnumber, workgroupgrade, personstatus, hrspaid, leave, sickspecial,
                    hrsavail, enddate, fpsyear)
                VALUES ('{pactid}', '{spnumber}', 'ITWGG', '{personStatus}', 0, 0, 0, 0, {enddateSql}, {year});");
        }

        public Task StaffJobAsync(string pactid) =>
            ExecAsync($@"
                INSERT INTO fps.tblstaffjob (staffid, jobcode, plannedhours, fpsyear)
                VALUES ('{pactid}', 'ITPROJ', 0, {_defaultYear});");

        public Task<bool> WgEmployeeExistsAsync(string pactid, int? fpsYear = null) =>
            ScalarBoolAsync($@"SELECT EXISTS (SELECT 1 FROM fps.tblwgemployee WHERE pactid = '{pactid}' AND fpsyear = {fpsYear ?? _defaultYear});");

        public Task<bool> StaffJobExistsAsync(string pactid) =>
            ScalarBoolAsync($@"SELECT EXISTS (SELECT 1 FROM fps.tblstaffjob WHERE staffid = '{pactid}' AND fpsyear = {_defaultYear});");

        /// <summary>Deletes every seeded row for every year seeded via <see cref="SeedReferenceChainAsync"/>, FK-safe order.</summary>
        public async Task CleanupAsync()
        {
            foreach (var year in _seededYears)
            {
                await CleanupYearAsync(year);
            }
        }

        public async Task CleanupYearAsync(int fpsYear)
        {
            await ExecAsync($"DELETE FROM fps.tblstaffjob WHERE fpsyear = {fpsYear};");
            await ExecAsync($"DELETE FROM fps.tblwgemployee WHERE fpsyear = {fpsYear};");
            await ExecAsync($"DELETE FROM fps.tblemployee WHERE fpsyear = {fpsYear};");
            await ExecAsync($"DELETE FROM fps.tlkpproject WHERE fpsyear = {fpsYear};");
            await ExecAsync($"DELETE FROM fps.tlkpprogram WHERE fpsyear = {fpsYear};");
            await ExecAsync($"DELETE FROM fps.tblcontract WHERE fpsyear = {fpsYear};");
            await ExecAsync($"DELETE FROM fps.workgroupgrade WHERE fpsyear = {fpsYear};");
            await ExecAsync($"DELETE FROM fps.profitcentregrade WHERE fpsyear = {fpsYear};");
            await ExecAsync($"DELETE FROM fps.divisiongrade WHERE fpsyear = {fpsYear};");
            await ExecAsync($"DELETE FROM fps.grade WHERE fpsyear = {fpsYear};");
            await ExecAsync($"DELETE FROM fps.workgroup WHERE fpsyear = {fpsYear};");
            await ExecAsync($"DELETE FROM fps.costcentre WHERE fpsyear = {fpsYear};");
        }

        private async Task ExecAsync(string sql)
        {
            await using var context = _owner.CreateDbContext();
            await context.Database.ExecuteSqlRawAsync(sql);
        }

        private async Task<bool> ScalarBoolAsync(string sql)
        {
            await using var context = _owner.CreateDbContext();
            await context.Database.OpenConnectionAsync();
            var connection = context.Database.GetDbConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            var result = await command.ExecuteScalarAsync();
            return result is bool value && value;
        }

        private async Task<string> ScalarStringAsync(string sql)
        {
            await using var context = _owner.CreateDbContext();
            await context.Database.OpenConnectionAsync();
            var connection = context.Database.GetDbConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            var result = await command.ExecuteScalarAsync();
            return result as string
                ?? throw new InvalidOperationException($"Expected an existing row for seed lookup: {sql}");
        }
    }
}

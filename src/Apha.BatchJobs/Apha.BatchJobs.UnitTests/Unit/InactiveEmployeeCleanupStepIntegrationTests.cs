using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed integration tests for <see cref="InactiveEmployeeCleanupStep"/>'s 2026-08-14
/// redesign around the legacy <c>Annual_WGEmployeeList.sql</c> rule: inactive candidate is
/// <c>personstatus='I'</c> (case-insensitive) AND <c>enddate IS NULL</c>, evaluated against the
/// target year only; General Staff exemption is <c>spnumber LIKE 'G%'</c> AND
/// <c>UPPER(firstname)='GENERAL'</c> (both required — confirmed non-equivalent to OR against live
/// data); an unexpected <c>personstatus</c> value is a hard validation failure before any deletion;
/// no <c>mabarchive</c> table is referenced.
/// </summary>
/// <remarks>
/// Must only ever run against an isolated/local database — never <c>batchjob_testing</c>. Each test
/// seeds a disposable far-future target year (never a competing current/Open year), runs the step
/// inside its own transaction, and always rolls back — nothing is ever committed, so these are safe
/// to write even though this session cannot execute them against a real database (self-skips via
/// <c>ConnectionStrings__FPSConnectionString</c> defaulting to an unreachable local connection, same
/// as every other Year End DB-mutating integration test in this project).
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

        const int targetYear = 90510;
        await RunSeededAsync(targetYear, async (connection, transaction, seed) =>
        {
            await seed.EmployeeAsync("PACT001", "SP001", "Alan", personStatus: "I", enddate: null);

            var step = new InactiveEmployeeCleanupStep(NullLogger<InactiveEmployeeCleanupStep>.Instance);
            await step.ExecuteAsync(Context(targetYear), connection, transaction, CancellationToken.None);

            Assert.False(await seed.WgEmployeeExistsAsync("PACT001"));
        });
    }

    [SkippableFact]
    public async Task ExecuteAsync_ActiveEmployee_IsRetained()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90511;
        await RunSeededAsync(targetYear, async (connection, transaction, seed) =>
        {
            await seed.EmployeeAsync("PACT002", "SP002", "Alan", personStatus: "A", enddate: null);

            var step = new InactiveEmployeeCleanupStep(NullLogger<InactiveEmployeeCleanupStep>.Instance);
            await step.ExecuteAsync(Context(targetYear), connection, transaction, CancellationToken.None);

            Assert.True(await seed.WgEmployeeExistsAsync("PACT002"));
        });
    }

    [SkippableFact]
    public async Task ExecuteAsync_GPrefixAndGeneralInactive_IsRetainedAsGeneralStaff()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90512;
        await RunSeededAsync(targetYear, async (connection, transaction, seed) =>
        {
            await seed.EmployeeAsync("PACT003", "G123", "General", personStatus: "I", enddate: null);

            var step = new InactiveEmployeeCleanupStep(NullLogger<InactiveEmployeeCleanupStep>.Instance);
            await step.ExecuteAsync(Context(targetYear), connection, transaction, CancellationToken.None);

            Assert.True(await seed.WgEmployeeExistsAsync("PACT003"));
        });
    }

    [SkippableFact]
    public async Task ExecuteAsync_GeneralFirstNameWithoutGPrefixInactive_IsRemovedUnderTheLegacyAndRule()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90513;
        await RunSeededAsync(targetYear, async (connection, transaction, seed) =>
        {
            // Mirrors the live discordant case found during investigation (e.g. spnumber "T195",
            // firstname "General") — under the confirmed AND rule this is NOT the General Staff
            // exemption, because spnumber doesn't start with 'G'.
            await seed.EmployeeAsync("PACT004", "T195", "General", personStatus: "I", enddate: null);

            var step = new InactiveEmployeeCleanupStep(NullLogger<InactiveEmployeeCleanupStep>.Instance);
            await step.ExecuteAsync(Context(targetYear), connection, transaction, CancellationToken.None);

            Assert.False(await seed.WgEmployeeExistsAsync("PACT004"));
        });
    }

    [SkippableFact]
    public async Task ExecuteAsync_GPrefixButNotGeneralInactive_IsRemoved()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90514;
        await RunSeededAsync(targetYear, async (connection, transaction, seed) =>
        {
            await seed.EmployeeAsync("PACT005", "G456", "Graham", personStatus: "I", enddate: null);

            var step = new InactiveEmployeeCleanupStep(NullLogger<InactiveEmployeeCleanupStep>.Instance);
            await step.ExecuteAsync(Context(targetYear), connection, transaction, CancellationToken.None);

            Assert.False(await seed.WgEmployeeExistsAsync("PACT005"));
        });
    }

    [SkippableFact]
    public async Task ExecuteAsync_LowercasePersonStatus_IsTreatedTheSameAsUppercase()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90515;
        await RunSeededAsync(targetYear, async (connection, transaction, seed) =>
        {
            await seed.EmployeeAsync("PACT006", "SP006", "Alan", personStatus: "i", enddate: null);

            var step = new InactiveEmployeeCleanupStep(NullLogger<InactiveEmployeeCleanupStep>.Instance);
            await step.ExecuteAsync(Context(targetYear), connection, transaction, CancellationToken.None);

            Assert.False(await seed.WgEmployeeExistsAsync("PACT006"));
        });
    }

    [SkippableFact]
    public async Task ExecuteAsync_UnexpectedPersonStatusValue_ThrowsBeforeAnyDeletion()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90516;
        await RunSeededAsync(targetYear, async (connection, transaction, seed) =>
        {
            // A genuinely inactive, non-General-Staff row that WOULD be removed if the step ran to
            // completion — proves the validation failure blocks the whole step, not just the bad row.
            await seed.EmployeeAsync("PACT007", "SP007", "Alan", personStatus: "I", enddate: null);
            // The known live-data anomaly: neither clearly active nor inactive.
            await seed.EmployeeAsync("PACT008", "SP008", "Brenda", personStatus: "AI", enddate: null);

            var step = new InactiveEmployeeCleanupStep(NullLogger<InactiveEmployeeCleanupStep>.Instance);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => step.ExecuteAsync(Context(targetYear), connection, transaction, CancellationToken.None));

            Assert.Contains("personstatus", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("PACT008", ex.Message, StringComparison.Ordinal);

            // No partial cleanup: the otherwise-eligible row must still be present.
            Assert.True(await seed.WgEmployeeExistsAsync("PACT007"));
            Assert.True(await seed.WgEmployeeExistsAsync("PACT008"));
        });
    }

    [SkippableFact]
    public async Task ExecuteAsync_InactiveStatusInADifferentYear_DoesNotInfluenceTheTargetYearDecision()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90517;
        const int otherYear = 90518;
        await RunSeededAsync(targetYear, async (connection, transaction, seed) =>
        {
            // Active in the target year...
            await seed.EmployeeAsync("PACT009", "SP009", "Alan", personStatus: "A", enddate: null);
            // ...but inactive in a different year for the same person/spnumber. If the join were not
            // year-scoped, or if any lookup ignored fpsyear, this could wrongly influence the result.
            await seed.SeedReferenceChainAsync(otherYear);
            await seed.EmployeeAsync("PACT009", "SP009", "Alan", personStatus: "I", enddate: null, fpsYear: otherYear);

            var step = new InactiveEmployeeCleanupStep(NullLogger<InactiveEmployeeCleanupStep>.Instance);
            await step.ExecuteAsync(Context(targetYear), connection, transaction, CancellationToken.None);

            Assert.True(await seed.WgEmployeeExistsAsync("PACT009", targetYear));
            Assert.True(await seed.WgEmployeeExistsAsync("PACT009", otherYear));
        });
    }

    [SkippableFact]
    public async Task ExecuteAsync_DependentTblStaffJobRows_AreDeletedBeforeTblWgEmployee()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        const int targetYear = 90519;
        await RunSeededAsync(targetYear, async (connection, transaction, seed) =>
        {
            await seed.EmployeeAsync("PACT010", "SP010", "Alan", personStatus: "I", enddate: null);
            await seed.StaffJobAsync("PACT010");

            var step = new InactiveEmployeeCleanupStep(NullLogger<InactiveEmployeeCleanupStep>.Instance);
            await step.ExecuteAsync(Context(targetYear), connection, transaction, CancellationToken.None);

            Assert.False(await seed.WgEmployeeExistsAsync("PACT010"));
            Assert.False(await seed.StaffJobExistsAsync("PACT010"));
        });
    }

    [SkippableFact]
    public async Task ExecuteAsync_NeverReferencesMabArchive_SucceedsWithOnlyFpsSchemaSeeded()
    {
        Skip.IfNot(CanRun(), _skipReason ?? "Integration DB unavailable.");

        // No mabarchive.* table is seeded or expected to exist for this test — if the step still
        // referenced mabarchive.my_tblwgemployee (which doesn't exist in batchjob_testing at all,
        // per the 2026-08-14 investigation), this would fail rather than clean up normally.
        const int targetYear = 90520;
        await RunSeededAsync(targetYear, async (connection, transaction, seed) =>
        {
            await seed.EmployeeAsync("PACT011", "SP011", "Alan", personStatus: "I", enddate: null);

            var step = new InactiveEmployeeCleanupStep(NullLogger<InactiveEmployeeCleanupStep>.Instance);
            await step.ExecuteAsync(Context(targetYear), connection, transaction, CancellationToken.None);

            Assert.False(await seed.WgEmployeeExistsAsync("PACT011"));
        });
    }

    private static YearEndExecutionContext Context(int targetYear) =>
        new(CorrelationId: $"inactive-cleanup-it-{targetYear}", ParametersJson: null, CurrentFpsYear: null, TargetFpsYear: targetYear);

    /// <summary>
    /// Opens a real transaction (not read-only — this step mutates), seeds the FK-required
    /// reference chain for <paramref name="targetYear"/>, runs <paramref name="body"/>, and always
    /// rolls back at the end regardless of outcome. Nothing seeded or deleted by this test is ever
    /// committed.
    /// </summary>
    private async Task RunSeededAsync(int targetYear, Func<System.Data.Common.DbConnection, System.Data.Common.DbTransaction, SeedHelper, Task> body)
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.OpenConnectionAsync();
        var connection = dbContext.Database.GetDbConnection();

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var dbTransaction = transaction.GetDbTransaction();

        var seed = new SeedHelper(connection, dbTransaction, targetYear);

        try
        {
            await seed.SeedReferenceChainAsync(targetYear);
            await body(connection, dbTransaction, seed);
        }
        finally
        {
            await transaction.RollbackAsync();
        }
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
    /// Seeds the minimum FK chain <c>tblwgemployee</c>/<c>tblstaffjob</c> rows require
    /// (workgroupgrade -> grade/profitcentregrade/workgroup -> costcentre; tlkpproject ->
    /// tblcontract/tlkpprogram), plus employee/staffjob helper inserts, for a given FPS year.
    /// Everything is rolled back by the owning transaction — no explicit cleanup needed.
    /// </summary>
    private sealed class SeedHelper
    {
        private readonly System.Data.Common.DbConnection _connection;
        private readonly System.Data.Common.DbTransaction _transaction;
        private readonly int _defaultYear;

        public SeedHelper(System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction, int defaultYear)
        {
            _connection = connection;
            _transaction = transaction;
            _defaultYear = defaultYear;
        }

        public async Task SeedReferenceChainAsync(int fpsYear)
        {
            await ExecAsync($@"INSERT INTO fps.costcentre (costcentre, profitcentre, fpsyear) VALUES (900001, 'ITPC', {fpsYear});");
            await ExecAsync($@"INSERT INTO fps.workgroup (workgroup, profitcentre, costcentre, fpsyear) VALUES ('ITWG', 'ITPC', 900001, {fpsYear});");
            await ExecAsync($@"INSERT INTO fps.grade (gradecode, fpsyear) VALUES ('ITGR', {fpsYear});");
            await ExecAsync($@"INSERT INTO fps.divisiongrade (divisiongrade, gradecode, division, fpsyear) VALUES ('ITDG', 'ITGR', 'ITDIV', {fpsYear});");
            await ExecAsync($@"INSERT INTO fps.profitcentregrade (pcgrade, divisiongrade, gradecode, profitcentre, fpsyear) VALUES ('ITPCG', 'ITDG', 'ITGR', 'ITPC', {fpsYear});");
            await ExecAsync($@"INSERT INTO fps.workgroupgrade (wggrade, profitcentregrade, gradecode, workgroup, fpsyear) VALUES ('ITWGG', 'ITPCG', 'ITGR', 'ITWG', {fpsYear});");

            await ExecAsync($@"INSERT INTO fps.tblcontract (contractno, category, fpsyear) VALUES ('ITCN', 'ITCAT', {fpsYear});");
            await ExecAsync($@"INSERT INTO fps.tlkpprogram (programno, fpsyear) VALUES ('ITPR', {fpsYear});");

            // tlkpproject's NOT NULL columns include several global-reference (non-year-scoped) FKs
            // — disease/projectstatus/customer/incomeaccountcode. Those tables are outside this
            // test's control, so this looks up one real existing value from each rather than
            // fabricating a placeholder that would violate the FK.
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

        private async Task ExecAsync(string sql)
        {
            await using var command = _connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }

        private async Task<bool> ScalarBoolAsync(string sql)
        {
            await using var command = _connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = sql;
            var result = await command.ExecuteScalarAsync();
            return result is bool value && value;
        }

        private async Task<string> ScalarStringAsync(string sql)
        {
            await using var command = _connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = sql;
            var result = await command.ExecuteScalarAsync();
            return result as string
                ?? throw new InvalidOperationException($"Expected an existing row for seed lookup: {sql}");
        }
    }
}

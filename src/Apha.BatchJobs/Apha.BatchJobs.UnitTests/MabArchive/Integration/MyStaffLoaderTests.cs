using Apha.BatchJobs.Infrastructure.MabArchive.Configuration;
using Apha.BatchJobs.Infrastructure.MabArchive.Loaders;
using Apha.BatchJobs.UnitTests.RecreateSummaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Apha.BatchJobs.UnitTests.MabArchive.Integration;

[Trait("Category", "Integration")]
public sealed class MyStaffLoaderTests
{
    // Regression: tblemployee is partitioned by fpsyear; Name/Title must come from the target year's row,
    // as legacy sp_AddMY_Staff read tblEmployee from the same per-year database.
    [SkippableFact]
    public async Task LoadAsync_EmployeeInMultipleYears_UsesTargetYearNameAndTitle()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;
        var year = harness.FpsYear;
        var spNumber = harness.Id("SP");
        var pactId = harness.Id("PA");

        await harness.ExecuteSqlAsync($@"
            DELETE FROM mabarchive.my_staff WHERE year = {year};

            {SeedWorkGroupChainSql(harness, "A", year)}

            -- Other-year rows in every prior year, so a year-blind join is unlikely to pick the target row.
            INSERT INTO fps.tblemployee (spnumber, firstname, lastname, title, fpsyear)
            SELECT '{spNumber}', 'Staff', 'Old', 'TOld', m.fpsyear
            FROM fps.tblyearmaster m
            WHERE m.fpsyear < {year};

            INSERT INTO fps.tblemployee (spnumber, firstname, lastname, title, fpsyear)
            VALUES ('{spNumber}', 'Staff', 'Current', 'TCur', {year});

            INSERT INTO fps.tblwgemployee
                (pactid, spnumber, workgroupgrade, personstatus, personclass, hrspaid, leave, sickspecial, hrsavail,
                 makeavailable, timerecorder, fpsyear)
            VALUES
                ('{pactId}', '{spNumber}', '{harness.Id("WGGA")}', 'A', 'P', 37, 0, 0, 37, 1, 0, {year});
        ");

        // Empty identity disables the profit centre filter, isolating the employee join.
        var loader = CreateLoader(db, authorizationUserName: string.Empty);
        await loader.LoadAsync(year, CancellationToken.None);

        var archived = await db.MaDstMyStaff
            .AsNoTracking()
            .SingleAsync(s => s.Year == year && s.StaffId == pactId);

        Assert.Equal("Current, Staff", archived.Name);
        Assert.Equal("TCur", archived.Title);
    }

    // Regression: tbluser_profitcentre is partitioned by fpsyear; a grant that exists only in another
    // year must not authorize staff for the target year.
    [SkippableFact]
    public async Task LoadAsync_ProfitCentreGrantedOnlyInOtherYear_StaffExcluded()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;
        var year = harness.FpsYear;
        var priorYear = year - 1;
        var userName = harness.Id("USR");
        var pactIdA = harness.Id("PAA");
        var pactIdB = harness.Id("PAB");

        await harness.ExecuteSqlAsync($@"
            DELETE FROM mabarchive.my_staff WHERE year = {year};

            {SeedWorkGroupChainSql(harness, "A", year)}
            {SeedWorkGroupChainSql(harness, "B", year)}

            INSERT INTO fps.tblkpprofitcentre (profitcentre, profitcentrename, division, fpsyear)
            VALUES ('{harness.Id("PCB")}', 'Profit Centre', (SELECT divname FROM fps.tlkpdivision LIMIT 1), {priorYear});

            INSERT INTO fps.tblusers (username) VALUES ('{userName}');

            INSERT INTO fps.tbluser_profitcentre (profitcentre, user_id, fpsyear)
            SELECT v.profitcentre, u.user_id, v.fpsyear
            FROM fps.tblusers u
            CROSS JOIN (VALUES ('{harness.Id("PCA")}', {year}), ('{harness.Id("PCB")}', {priorYear})) AS v(profitcentre, fpsyear)
            WHERE u.username = '{userName}';

            INSERT INTO fps.tblemployee (spnumber, firstname, lastname, fpsyear)
            VALUES
                ('{harness.Id("SPA")}', 'Staff', 'A', {year}),
                ('{harness.Id("SPB")}', 'Staff', 'B', {year});

            INSERT INTO fps.tblwgemployee
                (pactid, spnumber, workgroupgrade, personstatus, personclass, hrspaid, leave, sickspecial, hrsavail,
                 makeavailable, timerecorder, fpsyear)
            VALUES
                ('{pactIdA}', '{harness.Id("SPA")}', '{harness.Id("WGGA")}', 'A', 'P', 37, 0, 0, 37, 1, 0, {year}),
                ('{pactIdB}', '{harness.Id("SPB")}', '{harness.Id("WGGB")}', 'A', 'P', 37, 0, 0, 37, 1, 0, {year});
        ");

        var loader = CreateLoader(db, authorizationUserName: userName);
        await loader.LoadAsync(year, CancellationToken.None);

        var archivedIds = await db.MaDstMyStaff
            .AsNoTracking()
            .Where(s => s.Year == year && (s.StaffId == pactIdA || s.StaffId == pactIdB))
            .Select(s => s.StaffId)
            .ToListAsync();

        Assert.Equal(new[] { pactIdA }, archivedIds);
    }

    private static MyStaffLoader CreateLoader(
        Apha.BatchJobs.Infrastructure.Data.BatchJobsDbContext db,
        string authorizationUserName) =>
        new(
            db,
            Options.Create(new MabArchiveSettings { StaffProfitCentreAuthorizationUserName = authorizationUserName }),
            NullLogger<MyStaffLoader>.Instance);

    // Profit centre -> profit centre grade -> cost centre -> workgroup -> workgroup grade for one year.
    private static string SeedWorkGroupChainSql(RecreateSummariesPostgresTestHarness harness, string key, int year)
    {
        var profitCentre = harness.Id($"PC{key}");
        var pcGrade = harness.Id($"PCG{key}");
        var workGroup = harness.Id($"WG{key}");
        var wgGrade = harness.Id($"WGG{key}");
        var costCentre = Random.Shared.Next(900000, 999999);

        return $@"
            INSERT INTO fps.tblkpprofitcentre (profitcentre, profitcentrename, division, fpsyear)
            VALUES ('{profitCentre}', 'Profit Centre', (SELECT divname FROM fps.tlkpdivision LIMIT 1), {year});

            INSERT INTO fps.profitcentregrade
                (pcgrade, divisiongrade, gradecode, profitcentre, chargerate, payrate, npr, ohr, defrachargerate, fpsyear)
            VALUES
                ('{pcGrade}', 'D1', 'GC1', '{profitCentre}', 10::money, 5::money, 2::money, 1::money, 12::money, {year});

            INSERT INTO fps.costcentre (costcentre, profitcentre, fpsyear)
            VALUES ({costCentre}, '{profitCentre}', {year});

            INSERT INTO fps.workgroup (workgroup, profitcentre, costcentre, fpsyear)
            VALUES ('{workGroup}', '{profitCentre}', {costCentre}, {year});

            INSERT INTO fps.workgroupgrade (wggrade, profitcentregrade, gradecode, workgroup, fpsyear)
            VALUES ('{wgGrade}', '{pcGrade}', 'GC1', '{workGroup}', {year});";
    }
}

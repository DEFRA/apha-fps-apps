using Apha.BatchJobs.Infrastructure.MabArchive.Loaders;
using Apha.BatchJobs.UnitTests.RecreateSummaries;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Apha.BatchJobs.UnitTests.MabArchive.Integration;

[Trait("Category", "Integration")]
public sealed class MyTlkpTestReqmtLoaderTests
{
    // Regression: same Year+TestCode+ProjectBuyerCode but different Buyer must both be archived.
    // Previously the wrong EF key (Year, ProjectBuyerCode, TestCode) caused a key collision here.
    [SkippableFact]
    public async Task LoadAsync_SameYearTestCodeProjectBuyerCode_DifferentBuyer_BothRowsArchived()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;
        var year = harness.FpsYear;
        var testCode = harness.Id("TC");
        var projectBuyerCode = harness.Id("PB");
        var buyer1 = harness.Id("B1");
        var buyer2 = harness.Id("B2");

        await harness.ExecuteSqlAsync($@"
            DELETE FROM mabarchive.my_tlkptestreqmt
            WHERE year = {year} AND testcode = '{testCode}';

            INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, norequired, projectbuyercode, active, fpsyear)
            VALUES
                ('{testCode}', '{buyer1}', 10::money, 1, '{projectBuyerCode}', 1, {year}),
                ('{testCode}', '{buyer2}', 15::money, 2, '{projectBuyerCode}', 1, {year});
        ");

        var loader = new MyTlkpTestReqmtLoader(db);
        var inserted = await loader.LoadAsync(year, CancellationToken.None);

        Assert.Equal(2, inserted);

        var archived = await db.MaDstMyTlkpTestReqmt
            .AsNoTracking()
            .Where(r => r.Year == year && r.TestCode == testCode)
            .ToListAsync();

        Assert.Equal(2, archived.Count);
        Assert.Contains(archived, r => r.Buyer == buyer1);
        Assert.Contains(archived, r => r.Buyer == buyer2);
    }

    // Regression: same Year+TestCode+Buyer is a genuine PK collision and must not be inserted twice.
    [SkippableFact]
    public async Task LoadAsync_SameYearTestCodeBuyer_DuplicateSourceRow_ThrowsOnSave()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;
        var year = harness.FpsYear;
        var testCode = harness.Id("TC");
        var buyer = harness.Id("BU");

        // fps.tlkptestreqmt PK is (testcode, buyer, fpsyear) so this insert itself would violate
        // the source PK — we test EF behaviour directly by adding duplicate entities instead.
        var duplicate = new[]
        {
            new Apha.BatchJobs.Infrastructure.Data.MaDstMyTlkpTestReqmt { Year = year, TestCode = testCode, Buyer = buyer, UnitPrice = 10m },
            new Apha.BatchJobs.Infrastructure.Data.MaDstMyTlkpTestReqmt { Year = year, TestCode = testCode, Buyer = buyer, UnitPrice = 20m },
        };

        await db.MaDstMyTlkpTestReqmt.AddRangeAsync(duplicate);

        await Assert.ThrowsAnyAsync<Exception>(() => db.SaveChangesAsync());
    }
}

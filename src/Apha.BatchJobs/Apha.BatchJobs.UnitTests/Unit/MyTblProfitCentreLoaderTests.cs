using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.MabArchive.Loaders;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.UnitTests;

public sealed class MyTblProfitCentreLoaderTests
{
    [Fact]
    public async Task LoadAsync_WhenProfitCentreExistsInSeveralYears_ShouldArchiveOnlyTheRequestedYearsVersion()
    {
        await using var context = CreateContext();
        context.MaSrcTblkpProfitCentre.AddRange(
            CreateSource(2025, "PC01", "Profit Centre 2025", "DivA", 100m),
            CreateSource(2026, "PC01", "Profit Centre 2026", "DivB", 200m),
            CreateSource(2025, "PC02", "Only In 2025", "DivA", 10m),
            CreateSource(2026, "PC03", "Only In 2026", "DivB", 30m));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var loader = new MyTblProfitCentreLoader(context);

        var rowsAffected = await loader.LoadAsync(2026, CancellationToken.None);

        Assert.Equal(2, rowsAffected);

        var archived = await context.MaDstMyTblProfitCentre
            .AsNoTracking()
            .OrderBy(x => x.ProfitCentre)
            .ToListAsync();

        Assert.Equal(["PC01", "PC03"], archived.Select(x => x.ProfitCentre));
        Assert.All(archived, x => Assert.Equal(2026, x.Year));

        var pc01 = archived[0];
        Assert.Equal("Profit Centre 2026", pc01.ProfitCentreName);
        Assert.Equal("DivB", pc01.Division);
        Assert.Equal(200m, pc01.ContTarget);
    }

    [Fact]
    public async Task LoadAsync_WhenRequestedYearHasNoSourceRows_ShouldArchiveNothing()
    {
        await using var context = CreateContext();
        context.MaSrcTblkpProfitCentre.Add(CreateSource(2025, "PC01", "Profit Centre 2025", "DivA", 100m));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var loader = new MyTblProfitCentreLoader(context);

        var rowsAffected = await loader.LoadAsync(2026, CancellationToken.None);

        Assert.Equal(0, rowsAffected);
        Assert.Empty(await context.MaDstMyTblProfitCentre.AsNoTracking().ToListAsync());
    }

    private static MaSrcTblkpProfitCentre CreateSource(int fpsYear, string profitCentre, string name, string division, decimal contTarget) =>
        new()
        {
            FpsYear = fpsYear,
            ProfitCentre = profitCentre,
            ProfitCentreName = name,
            Division = division,
            ContTarget = contTarget,
            ProfitCentreHead = "Head",
            DivisionId = 1
        };

    private static BatchJobsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new BatchJobsDbContext(options);
    }
}

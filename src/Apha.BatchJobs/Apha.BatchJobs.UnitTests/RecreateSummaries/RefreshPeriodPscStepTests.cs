using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class RefreshPeriodPscStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        // Arrange: In-memory EF Core context
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new BatchJobsDbContext(options);

        // Seed RsProjSubContract, RsTlkpProject, RsCostCentre
        db.RsProjSubContract.Add(new RsProjSubContractTable {
            SubContCounter = 1,
            Project = "P1",
            Month = 1,
            Amount = 100m,
            AcctCode = "A1"
        });
        db.RsTlkpProject.Add(new RsTlkpProjectTable {
            ParentProject = "P1",
            OracleProjectCode = "OPC1",
            SubAccountCode = "SAC1",
            IsDefraProject = 0,
            CostCentre = 1.0
        });
        db.RsCostCentre.Add(new RsCostCentreTable {
            CostCentre = 1.0,
            ProfitCentre = "PC1"
        });
        await db.SaveChangesAsync();

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection());
        var step = new RefreshPeriodPscStep(1);
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("RefreshPeriodPsc", result.StepName);

        // Validate output in RsPeriodProjSubContract
        var rows = await db.RsPeriodProjSubContract.ToListAsync();
        Assert.Single(rows);
        var row = rows[0];
        Assert.Equal(1, row.Period);
        Assert.Equal(1, row.SubContCounter);
        Assert.Equal("P1", row.Project);
        Assert.Equal("OPC1", row.OracleProjectCode);
        Assert.Equal("SAC1", row.SubAccountCode);
        Assert.Equal("No", row.IsDefraProject);
        Assert.Equal("PC1", row.Opc);
        Assert.Equal("CC1", row.Occ);
        Assert.Equal(1, row.Month);
        Assert.Equal(100m, row.Amount);
        Assert.Equal("A1", row.AcctCode);
    }
}

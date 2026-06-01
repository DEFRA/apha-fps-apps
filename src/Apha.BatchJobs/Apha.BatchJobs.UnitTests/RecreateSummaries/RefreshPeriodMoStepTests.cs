using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class RefreshPeriodMoStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        // Arrange: In-memory EF Core context
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new BatchJobsDbContext(options);

        // Seed RsMonthlyOutput, RsWorkGroup, RsTlkpTestReqmt, RsTlkpProject, RsCostCentre
        db.RsMonthlyOutput.Add(new RsMonthlyOutputTable {
            Buyer = "P1",
            Month = 1,
            WorkGroup = "WG1",
            TestCode = "T1",
            Volume = 5
        });
        db.RsWorkGroup.Add(new RsWorkGroupTable {
            WorkGroup = "WG1",
            ProfitCentre = "PC1",
            CostCentre = 1.0
        });
        db.RsTlkpTestReqmt.Add(new RsTlkpTestReqmtTable {
            ProjectBuyerCode = "P1",
            TestCode = "T1",
            UnitPrice = 10m
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
        var step = new RefreshPeriodMoStep(1);
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("RefreshPeriodMo", result.StepName);

        // Validate output in RsPeriodMonthlyOutput
        var rows = await db.RsPeriodMonthlyOutput.ToListAsync();
        Assert.Single(rows);
        var row = rows[0];
        Assert.Equal(1, row.Period);
        Assert.Equal("P1", row.Project);
        Assert.Equal("OPC1", row.OracleProjectCode);
        Assert.Equal("SAC1", row.SubAccountCode);
        Assert.Equal("No", row.IsDefraProject);
        Assert.Equal("PC1", row.Opc);
        Assert.Equal(1d, row.Occ ?? 0d);
        Assert.Equal(1, row.Month);
        Assert.Equal("PC1", row.Spc);
        Assert.Equal("WG1", row.WorkGroup);
        Assert.Equal(1d, row.Scc ?? 0d);
        Assert.Equal("T1", row.TestCode);
        Assert.Equal(5, row.Volume);
        Assert.Equal(10m, row.TestPrice);
        Assert.Equal(50m, row.TotalCost);
    }
}

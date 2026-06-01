using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class RefreshPeriodTccStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        // Arrange: In-memory EF Core context
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new BatchJobsDbContext(options);

        // Seed RsTimeCostCalcs, RsWorkGroup, RsTlkpProject, RsCostCentre, RsTblWgEmployee
        db.RsTimeCostCalcs.Add(new RsTimeCostCalcsTable {
            WorkGroup = "WG1",
            JobCode = "JC1",
            Project = "P1",
            Month = 1,
            StaffId = "S1",
            GradeCode = "GC1",
            Name = "Staff1",
            ChargeRate = 10m,
            Class = "Charge",
            Time = 8d,
            Cost = 80d,
            Division = "DivA",
            Pay = 40m,
            NonPay = 16m,
            Overhead = 8m,
            FpsYear = 2026
        });
        db.RsWorkGroup.Add(new RsWorkGroupTable {
            WorkGroup = "WG1",
            ProfitCentre = "PC1",
            CostCentre = 1.0
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
        db.RsTblWgEmployee.Add(new RsTblWgEmployeeTable {
            PactId = "S1",
            SpNumber = "SP123"
        });
        await db.SaveChangesAsync();

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection());
        var step = new RefreshPeriodTccStep(1);
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("RefreshPeriodTcc", result.StepName);

        // Validate output in RsPeriodTimeCostCalcs
        var rows = await db.RsPeriodTimeCostCalcs.ToListAsync();
        Assert.Single(rows);
        var row = rows[0];
        Assert.Equal(1, row.Period);
        Assert.Equal("P1", row.Project);
        Assert.Equal("OPC1", row.OracleProjectCode);
        Assert.Equal("SAC1", row.SubAccountCode);
        Assert.Equal(1, row.Month);
        Assert.Equal("No", row.DefraProject);
        Assert.Equal(1d, row.Occ ?? 0d);
        Assert.Equal("PC1", row.Opc);
        Assert.Equal("PC1", row.Spc);
        Assert.Equal(1d, row.Scc ?? 0d);
        Assert.Equal("Staff1", row.Name);
        Assert.Equal("GC1", row.GradeCode);
        Assert.Equal("SP123", row.SpNumber);
        Assert.Equal(10m, row.ChargeRate);
        Assert.Equal(40m, row.Pay);
        Assert.Equal(16m, row.NonPay);
        Assert.Equal(8m, row.Overhead);
        Assert.Equal(8d, row.Time);
        Assert.Equal(80m, row.TotalCost);
    }
}

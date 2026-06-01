using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class CreateTimeCostCalcsStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        // Arrange: In-memory EF Core context
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new BatchJobsDbContext(options);

        // Seed RsTblkpProfitCentre
        db.RsTblkpProfitCentre.Add(new RsTblkpProfitCentreTable {
            ProfitCentre = "PC1",
            Division = "DivA"
        });
        // Seed RsProfitCentreGrade
        db.RsProfitCentreGrade.Add(new RsProfitCentreGradeTable {
            PcGrade = "G1",
            ProfitCentre = "PC1",
            ChargeRate = 10m,
            DefraChargeRate = 12m,
            PayRate = 5m,
            Npr = 2m,
            Ohr = 1m
        });
        // Seed RsWorkGroupGrade
        db.RsWorkGroupGrade.Add(new RsWorkGroupGradeTable {
            WgGrade = "WG1",
            ProfitCentreGrade = "G1",
            WorkGroup = "WG",
            GradeCode = "GC1"
        });
        // Seed RsVpactTblStaff
        db.RsVpactTblStaff.Add(new RsVpactTblStaffView {
            PactId = "S1",
            Name = "Staff1",
            WorkGroupGrade = "WG1"
        });
        // Seed RsMonthlyTime
        db.RsMonthlyTime.Add(new RsMonthlyTimeTable {
            PactStaffId = "S1",
            WorkGroup = "WG",
            TimeCode = "JC1",
            ParentProject = "P1",
            Month = 1,
            Hours = 8d
        });
        // Seed RsTimeCodeValid
        db.RsTimeCodeValid.Add(new RsTimeCodeValidTable {
            WorkGroup = "WG",
            TimeCode = "JC1",
            ParentProject = "P1"
        });
        // Seed RsTlkpProject
        db.RsTlkpProject.Add(new RsTlkpProjectTable {
            ParentProject = "P1",
            Program = "PRG1",
            FpsYear = 2026,
            IsDefraProject = 0
        });
        // Seed RsTlkpProgram
        db.RsTlkpProgram.Add(new RsTlkpProgramTable {
            ProgramNo = "PRG1",
            SectorName = "Charge"
        });
        await db.SaveChangesAsync();

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection());
        var step = new CreateTimeCostCalcsStep();
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("CreateTimeCostCalcs", result.StepName);

        // Validate output in RsTimeCostCalcs
        var rows = await db.RsTimeCostCalcs.ToListAsync();
        Assert.Single(rows);
        var row = rows[0];
        Assert.Equal("WG", row.WorkGroup);
        Assert.Equal("JC1", row.JobCode);
        Assert.Equal("P1", row.Project);
        Assert.Equal(1, row.Month);
        Assert.Equal("S1", row.StaffId);
        Assert.Equal("GC1", row.GradeCode);
        Assert.Equal("Staff1", row.Name);
        Assert.Equal(10m, row.ChargeRate); // IsDefraProject = 0
        Assert.Equal("Charge", row.Class);
        Assert.Equal(8d, row.Time);
        Assert.Equal(8d * 10d, row.Cost ?? 0d);
        Assert.Equal("DivA", row.Division);
        Assert.Equal(8m * 5m, row.Pay);
        Assert.Equal(8m * 2m, row.NonPay);
        Assert.Equal(8m * 1m, row.Overhead);
        Assert.Equal(2026, row.FpsYear);
    }
}

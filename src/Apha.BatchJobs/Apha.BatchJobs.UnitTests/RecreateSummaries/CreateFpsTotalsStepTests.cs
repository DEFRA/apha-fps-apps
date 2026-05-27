using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class CreateFpsTotalsStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        // Arrange: In-memory EF Core context
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new BatchJobsDbContext(options);

        // Seed RsTlkpProject
        db.RsTlkpProject.Add(new RsTlkpProjectTable
        {
            ParentProject = "PRJ1",
            Program = "PRG1",
            PlanCaseworkDebit = 10,
            CustIncome = 100,
            TransferIncome = 200,
            BudgetCvl = 50,
            Profit = 20,
            Manager = "Mgr",
            Customer = "Cust",
            ProjectStatus = "Active",
            PvsIncome = 5,
            FpsYear = 2026
        });
        // Seed joined tables
        db.RsQryTotalAdditionalCosts.Add(new RsQryTotalAdditionalCostsView { JobCode = "PRJ1", TotalAdditionalCosts = 1, FpsYear = 2026 });
        db.RsQryTotalAnimalCosts.Add(new RsQryTotalAnimalCostsView { JobCode = "PRJ1", TotalAnimalCosts = 2, FpsYear = 2026 });
        db.RsQryTotalStaffCosts.Add(new RsQryTotalStaffCostsView { JobCode = "PRJ1", TotalStaffCosts = 3, TotalPayCosts = 7, FpsYear = 2026 });
        db.RsQryTotalTestCosts.Add(new RsQryTotalTestCostsView { JobCode = "PRJ1", TotalTestCosts = 4, FpsYear = 2026 });
        await db.SaveChangesAsync();

        // Fake NpgsqlConnection (not used in LINQ path)
        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection());
        var step = new CreateFpsTotalsStep();

        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.Equal("CreateFpsTotals", result.StepName);
        Assert.Equal(1, result.RowsAffected);
        var totals = await db.RsFpsYearTotals.FirstOrDefaultAsync();
        Assert.NotNull(totals);
        Assert.Equal("PRJ1", totals.ParentProject);
        Assert.Equal("PRG1", totals.Program);
        Assert.Equal(1, totals.TotalAdditionalCosts);
        Assert.Equal(2, totals.TotalAnimalCosts);
        Assert.Equal(3, totals.TotalStaffCosts);
        Assert.Equal(4, totals.TotalTestCosts);
        Assert.Equal(10, totals.PlanCaseworkDebit);
        Assert.Equal(100, totals.CustIncome);
        Assert.Equal(200, totals.TransferIncome);
        Assert.Equal(300, totals.TotalIncome);
        Assert.Equal(50, totals.BudgetCvl);
        Assert.Equal(20, totals.RequiredProfit);
        Assert.Equal("Mgr", totals.Manager);
        Assert.Equal("Cust", totals.Customer);
        Assert.Equal("Active", totals.ProjectStatus);
        Assert.Equal(5, totals.PvsIncome);
        Assert.Equal(10, totals.PlanCaseworkDebit);
        Assert.Equal(7, totals.TotalPayCosts);
        Assert.Equal(2026, totals.FpsYear);
        // TotalCosts = 1+2+3+4+10 = 20
        Assert.Equal(20, totals.TotalCosts);
    }
}

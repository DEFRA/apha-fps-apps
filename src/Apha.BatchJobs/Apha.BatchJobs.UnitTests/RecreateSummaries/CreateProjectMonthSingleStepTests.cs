using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class CreateProjectMonthSingleStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        // Arrange: In-memory EF Core context
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new BatchJobsDbContext(options);

        // Seed RsProjectMonth
        db.RsProjectMonth.Add(new RsProjectMonthTable {
            Project = "P1",
            MonthNo = 1,
            CostProfile = 100m
        });
        // Seed RsQryJobMonthSubContracts
        db.RsQryJobMonthSubContracts.Add(new RsQryJobMonthSubContractsView {
            Project = "P1",
            Month = 1,
            Total = 10m,
            Animals = 5m,
            Other = 2m
        });
        // Seed RsQryJobMonthTime
        db.RsQryJobMonthTime.Add(new RsQryJobMonthTimeView {
            Project = "P1",
            Month = 1,
            SumOfCost = 20d,
            SumOfHours = 8d,
            SumOfPayRate = 4m
        });
        // Seed RsQryJobMonthMilestone
        db.RsQryJobMonthMilestone.Add(new RsQryJobMonthMilestoneView {
            Project = "P1",
            DueMonth = 1,
            MstoneDue = 1d,
            DueDone = 1d,
            OnTime = 1d
        });
        // Seed RsQryJobMonthTransfersTotal
        db.RsQryJobMonthTransfersTotal.Add(new RsQryJobMonthTransfersTotalView {
            Project = "P1",
            Month = 1,
            SumOfTransferCost = 3m
        });
        // Seed RsQryJobMonthInvoices
        db.RsQryJobMonthInvoices.Add(new RsQryJobMonthInvoicesView {
            ProjectParent = "P1",
            Month = 1,
            SumOfAmount1 = 15m,
            WorkCost = 7m
        });
        // Seed RsQryJobMonthPortfolioSales
        db.RsQryJobMonthPortfolioSales.Add(new RsQryJobMonthPortfolioSalesView {
            PlanPortfolio = "P1",
            Month = 1,
            Fee = 2m
        });
        // Seed RsQryJobMonthTotProfile
        db.RsQryJobMonthTotProfile.Add(new RsQryJobMonthTotProfileView {
            Project = "P1",
            SumOfCostProfile = 100m
        });
        await db.SaveChangesAsync();

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection());
        var step = new CreateProjectMonthSingleStep();
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("CreateProjectMonthSingle", result.StepName);

        // Validate output in RsProjectMonth2
        var rows = await db.RsProjectMonth2.ToListAsync();
        Assert.Single(rows);
        var row = rows[0];
        Assert.Equal("P1", row.Project);
        Assert.Equal(1, row.MonthNo);
        Assert.Equal(100m, row.CostProfile);
        Assert.Equal(10m, row.SubContracts);
        Assert.Equal(5m, row.Animals);
        Assert.Equal(2m, row.NonAnimal);
        Assert.Equal(20d, row.TimeCosts);
        Assert.Equal(3d, row.TransferCosts);
        Assert.Equal(10m + 20m + 3m, row.TotalCost); // SubContracts + TimeCosts + TransferCosts
        Assert.Equal(15m, row.Invoices);
        Assert.Equal(7m, row.Coiw);
        Assert.Equal(100m, row.SumOfCostProfile);
        Assert.Equal(2d, row.PortSales);
        Assert.Equal(1d, row.MstoneDue);
        Assert.Equal(1d, row.DueDone);
        Assert.Equal(1d, row.OnTime);
        Assert.Equal(8d, row.TotalHours);
        Assert.Equal(4m, row.PayCosts);
    }
}

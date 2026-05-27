using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class CreateProjectMonthCaseworkStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        // Arrange: In-memory EF Core context
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new BatchJobsDbContext(options);

        // Seed RsQryProjectMonthCw
        db.RsQryProjectMonthCw.Add(new RsQryProjectMonthCwView {
            Project = "P1",
            MonthNo = 1,
            CwDebit = 1m,
            CwCredit = 2m
        });
        await db.SaveChangesAsync();

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection());
        var step = new CreateProjectMonthCaseworkStep();
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("CreateProjectMonthCasework", result.StepName);

        // Validate output in RsProjectMonthCasework
        var rows = await db.RsProjectMonthCasework.ToListAsync();
        Assert.Single(rows);
        var row = rows[0];
        Assert.Equal("P1", row.Project);
        Assert.Equal(1, row.MonthNo);
        Assert.Equal(1d, row.CwDebit);
        Assert.Equal(2d, row.CwCredit);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class DeleteProjectMonthCaseworkStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        // Arrange: In-memory EF Core context
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new BatchJobsDbContext(options);

        // Seed RsProjectMonthCasework
        db.RsProjectMonthCasework.Add(new RsProjectMonthCaseworkTable {
            Project = "P1",
            MonthNo = 1,
            CwDebit = 1d,
            CwCredit = 2d
        });
        await db.SaveChangesAsync();

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection());
        var step = new DeleteProjectMonthCaseworkStep();
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("DeleteProjectMonthCasework", result.StepName);
        Assert.Empty(db.RsProjectMonthCasework);
    }
}

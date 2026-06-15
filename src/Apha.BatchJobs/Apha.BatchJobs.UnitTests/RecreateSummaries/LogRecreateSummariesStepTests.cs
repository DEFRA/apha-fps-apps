using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class LogRecreateSummariesStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        // Arrange: In-memory EF Core context
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new BatchJobsDbContext(options);

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection());
        var step = new LogRecreateSummariesStep(5, 2026, "DOMAIN\\user");
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("LogRecreateSummaries", result.StepName);

        // Validate output in RsRecreateSummariesLog
        var rows = await db.RsRecreateSummariesLog.ToListAsync();
        Assert.Single(rows);
        var row = rows[0];
        Assert.Equal(5, row.Period);
        Assert.Equal("user", row.UserId); // Normalized
        Assert.True((DateTime.UtcNow - row.DateDone).TotalSeconds < 10); // DateDone is recent
    }
}

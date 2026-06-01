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
        // Arrange: PostgreSQL EF Core context
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__BatchJobsConnectionString")
            ?? "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=admin123;Timeout=30";
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        using var db = new BatchJobsDbContext(options);

        await db.Database.EnsureCreatedAsync();
        // Clean up tables
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"RsProjectMonthCasework\" RESTART IDENTITY CASCADE;");
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"RsQryProjectMonthCwView\" RESTART IDENTITY CASCADE;");
        // Insert required data for views using raw SQL
        await db.Database.ExecuteSqlRawAsync("INSERT INTO \"RsQryProjectMonthCwView\" (\"Project\", \"MonthNo\", \"CwDebit\", \"CwCredit\") VALUES ('P1', 1, 1, 2);");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(connectionString));
        var step = new CreateProjectMonthCaseworkStep();
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("CreateProjectMonthCasework", result.StepName);
        // Additional asserts as needed, or adapt to your real DB state
    }
}
